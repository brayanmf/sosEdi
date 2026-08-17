using Microsoft.AspNetCore.Mvc;
using SOS.Models;
using SOS.Services;

namespace SOS.Controllers
{
    /// <summary>
    /// Controlador para gestionar alertas de emergencia y confirmación de seguridad de colaboradores.
    /// Proporciona endpoints para activar alertas de evacuación, registrar confirmaciones de seguridad
    /// y obtener el estado de alertas activas.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EmergencyController : ControllerBase
    {
        private readonly AlertasRepository _alertasRepository;
        private readonly ConfirmacionesRepository _confirmacionesRepository;
        private readonly OneSignalNotificationService _notificationService;
        private readonly ILogger<EmergencyController> _logger;
        private readonly LoggerService _loggerService;

        public EmergencyController(
            AlertasRepository alertasRepository,
            ConfirmacionesRepository confirmacionesRepository,
            OneSignalNotificationService notificationService,
            ILogger<EmergencyController> logger,
            LoggerService loggerService)
        {
            _alertasRepository = alertasRepository;
            _confirmacionesRepository = confirmacionesRepository;
            _notificationService = notificationService;
            _logger = logger;
            _loggerService = loggerService;
        }

        /// <summary>
        /// Activa una nueva alerta de evacuación y envía notificaciones push a todos los usuarios registrados.
        /// Este endpoint debe ser llamado únicamente por un Oficial de Seguridad autenticado.
        /// </summary>
        /// <param name="alerta">Objeto AlertaEvacuacion con los detalles de la emergencia.</param>
        /// <returns>Respuesta con ID de la alerta y ID de notificación en OneSignal.</returns>
        /// <response code="200">Alerta activada exitosamente.</response>
        /// <response code="400">Datos de entrada inválidos.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpPost("activate")]
        [Produces("application/json")]
        public async Task<IActionResult> ActivateAlert([FromBody] AlertaEvacuacion alerta)
        {
            var idSolicitud = Guid.NewGuid().ToString();
            var datosInicio = new Dictionary<string, object>
            {
                { "idSolicitud", idSolicitud },
                { "tipoAlerta", alerta?.TipoAlerta ?? "Desconocido" },
                { "idUsuario", alerta?.IdUsuario ?? "N/A" }
            };

            try
            {
                _loggerService.LogInfo("Iniciando proceso de activación de alerta de evacuación", datosInicio);

                // Validar entrada
                if (alerta == null)
                {
                    _loggerService.LogWarning("Solicitud de alerta con datos nulos", datosInicio);
                    return BadRequest(new
                    {
                        error = "Datos de alerta inválidos",
                        idSolicitud = idSolicitud
                    });
                }

                if (string.IsNullOrWhiteSpace(alerta.TipoAlerta))
                {
                    _loggerService.LogWarning("Solicitud de alerta sin tipo especificado", datosInicio);
                    return BadRequest(new
                    {
                        error = "El tipo de alerta es requerido",
                        idSolicitud = idSolicitud
                    });
                }

                // Configurar datos de la alerta
                alerta.FechaHoraActivacion = DateTime.UtcNow;
                alerta.EstadoAlerta = "Activa";
                alerta.FechaRegistro = DateTime.UtcNow;
                alerta.Activo = true;

                _loggerService.LogDebug("Datos de alerta configurados", new Dictionary<string, object>
                {
                    { "idSolicitud", idSolicitud },
                    { "mensaje", alerta.MensajeAlerta ?? "N/A" },
                    { "ubicacion", alerta.DescripcionUbicacionActivacion ?? "N/A" }
                });

                // Insertar alerta en base de datos
                _loggerService.LogInfo("Insertando alerta en base de datos...", new Dictionary<string, object>
                {
                    { "idSolicitud", idSolicitud }
                });

                var id = await _alertasRepository.InsertarAlertaAsync(alerta);
                alerta.Id = id;

                _loggerService.LogInfo("Alerta insertada exitosamente en base de datos", new Dictionary<string, object>
                {
                    { "idSolicitud", idSolicitud },
                    { "alertaId", id }
                });

                // Enviar notificación push
                _loggerService.LogInfo("Preparando envío de notificaciones push a través de OneSignal...", new Dictionary<string, object>
                {
                    { "idSolicitud", idSolicitud },
                    { "alertaId", id }
                });

                var titulo = "⚠️ ALERTA DE EVACUACIÓN";
                var mensaje = alerta.MensajeAlerta ?? "Se ha activado una alerta de emergencia. ¡Evacúen de inmediato!";

                var notificationId = await _notificationService.EnviarAlertaEvacuacionAsync(
                    titulo: titulo,
                    mensaje: mensaje,
                    alerta: alerta
                );

                if (!string.IsNullOrEmpty(notificationId))
                {
                    _loggerService.LogInfo("Notificaciones enviadas exitosamente a través de OneSignal", new Dictionary<string, object>
                    {
                        { "idSolicitud", idSolicitud },
                        { "alertaId", id },
                        { "notificationId", notificationId }
                    });

                    return Ok(new
                    {
                        message = "Alerta de evacuación activada y notificaciones enviadas exitosamente.",
                        alertaId = id,
                        notificationId = notificationId,
                        fechaActivacion = alerta.FechaHoraActivacion,
                        tipoAlerta = alerta.TipoAlerta,
                        idSolicitud = idSolicitud
                    });
                }
                else
                {
                    _loggerService.LogWarning("La alerta fue creada pero las notificaciones no se enviaron correctamente", new Dictionary<string, object>
                    {
                        { "idSolicitud", idSolicitud },
                        { "alertaId", id }
                    });

                    return Ok(new
                    {
                        message = "Alerta de evacuación activada, pero las notificaciones tuvieron un problema.",
                        alertaId = id,
                        warning = "Revisar logs del servidor para más detalles",
                        idSolicitud = idSolicitud
                    });
                }
            }
            catch (Exception ex)
            {
                _loggerService.LogError("Excepción al activar alerta de evacuación", ex, new Dictionary<string, object>
                {
                    { "idSolicitud", idSolicitud },
                    { "excepcion", ex.GetType().Name }
                });

                return StatusCode(500, new
                {
                    error = "Error al procesar la solicitud de alerta",
                    message = ex.Message,
                    idSolicitud = idSolicitud
                });
            }
        }

        /// <summary>
        /// Registra la confirmación de seguridad de un colaborador ante una alerta de evacuación.
        /// Captura su ubicación GPS actual, estado reportado y comentarios adicionales.
        /// </summary>
        /// <param name="confirmacion">Objeto ConfirmacionSeguridad con los datos del colaborador.</param>
        /// <returns>Respuesta con ID de la confirmación registrada.</returns>
        /// <response code="200">Confirmación registrada exitosamente.</response>
        /// <response code="400">Datos de entrada inválidos.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpPost("confirm-safety")]
        [Produces("application/json")]
        public async Task<IActionResult> ConfirmSafety([FromBody] ConfirmacionSeguridad confirmacion)
        {
            var idSolicitud = Guid.NewGuid().ToString();
            var datosInicio = new Dictionary<string, object>
            {
                { "idSolicitud", idSolicitud },
                { "idUsuario", confirmacion?.IdUsuario.ToString() ?? "N/A" },
                { "alertaId", confirmacion?.AlertaEvacuacionId.ToString() ?? "N/A" }
            };

            try
            {
                _loggerService.LogInfo("Iniciando registro de confirmación de seguridad", datosInicio);

                // Validar entrada
                if (confirmacion == null)
                {
                    _loggerService.LogWarning("Solicitud de confirmación con datos nulos", datosInicio);
                    return BadRequest(new
                    {
                        error = "Datos de confirmación inválidos",
                        idSolicitud = idSolicitud
                    });
                }

                if (confirmacion.IdUsuario <= 0)
                {
                    _loggerService.LogWarning("Solicitud de confirmación con ID de usuario inválido", datosInicio);
                    return BadRequest(new
                    {
                        error = "ID de usuario inválido",
                        idSolicitud = idSolicitud
                    });
                }

                if (confirmacion.AlertaEvacuacionId <= 0)
                {
                    _loggerService.LogWarning("Solicitud de confirmación sin alerta asociada", datosInicio);
                    return BadRequest(new
                    {
                        error = "ID de alerta inválido",
                        idSolicitud = idSolicitud
                    });
                }

                // Configurar datos de confirmación
                confirmacion.FechaHoraConfirmacion = DateTime.UtcNow;
                confirmacion.FechaRegistro = DateTime.UtcNow;
                confirmacion.Activo = true;

                _loggerService.LogDebug("Datos de confirmación configurados", new Dictionary<string, object>
                {
                    { "idSolicitud", idSolicitud },
                    { "estado", confirmacion.EstadoReportado ?? "N/A" },
                    { "ubicacion", $"({confirmacion.Latitud}, {confirmacion.Longitud})" }
                });

                // Insertar confirmación en base de datos
                _loggerService.LogInfo("Insertando confirmación de seguridad en base de datos...", new Dictionary<string, object>
                {
                    { "idSolicitud", idSolicitud }
                });

                var id = await _confirmacionesRepository.InsertarConfirmacionAsync(confirmacion);

                _loggerService.LogInfo("Confirmación de seguridad registrada exitosamente", new Dictionary<string, object>
                {
                    { "idSolicitud", idSolicitud },
                    { "confirmacionId", id },
                    { "idUsuario", confirmacion.IdUsuario },
                    { "estado", confirmacion.EstadoReportado ?? "N/A" },
                    { "latitud", confirmacion.Latitud },
                    { "longitud", confirmacion.Longitud }
                });

                return Ok(new
                {
                    message = "Confirmación de seguridad registrada exitosamente.",
                    confirmacionId = id,
                    idUsuario = confirmacion.IdUsuario,
                    estadoReportado = confirmacion.EstadoReportado,
                    ubicacion = new
                    {
                        latitud = confirmacion.Latitud,
                        longitud = confirmacion.Longitud
                    },
                    fechaConfirmacion = confirmacion.FechaHoraConfirmacion,
                    idSolicitud = idSolicitud
                });
            }
            catch (Exception ex)
            {
                _loggerService.LogError("Excepción al registrar confirmación de seguridad", ex, new Dictionary<string, object>
                {
                    { "idSolicitud", idSolicitud },
                    { "excepcion", ex.GetType().Name }
                });

                return StatusCode(500, new
                {
                    error = "Error al registrar confirmación de seguridad",
                    message = ex.Message,
                    idSolicitud = idSolicitud
                });
            }
        }
        
        /// <summary>
        /// Obtiene los detalles de la última alerta de evacuación activa en el sistema.
        /// Esta información es utilizada por la aplicación móvil para determinar qué acción tomar.
        /// </summary>
        /// <returns>Objeto AlertaEvacuacion con los detalles de la alerta activa más reciente.</returns>
        /// <response code="200">Alerta activa encontrada.</response>
 
        [HttpGet("latest-alert")]
        [Produces("application/json")]
   
   
        public async Task<ActionResult<AlertaEvacuacion>> GetLatestActiveAlert()
        {
            var idSolicitud = Guid.NewGuid().ToString();

            try
            {
                _loggerService.LogInfo("Consultando última alerta activa", new Dictionary<string, object>
                {
                    { "idSolicitud", idSolicitud }
                });

                var latestAlert = await _alertasRepository.ObtenerUltimaAlertaActivaAsync();

                if (latestAlert == null)
                {
                    _loggerService.LogInfo("No hay alertas activas en el sistema", new Dictionary<string, object>
                    {
                        { "idSolicitud", idSolicitud }
                    });

                    return NotFound(new
                    {
                        message = "No hay alertas activas en este momento.",
                        idSolicitud = idSolicitud
                    });
                }

                _loggerService.LogInfo("Última alerta activa encontrada", new Dictionary<string, object>
                {
                    { "idSolicitud", idSolicitud },
                    { "alertaId", latestAlert.Id },
                    { "tipoAlerta", latestAlert.TipoAlerta ?? "N/A" },
                    { "fechaActivacion", latestAlert.FechaHoraActivacion }
                });

                return Ok(latestAlert);
            }
            catch (Exception ex)
            {
                _loggerService.LogError("Excepción al obtener última alerta activa", ex, new Dictionary<string, object>
                {
                    { "idSolicitud", idSolicitud },
                    { "excepcion", ex.GetType().Name }
                });

                return StatusCode(500, new
                {
                    error = "Error al obtener información de alertas",
                    message = ex.Message,
                    idSolicitud = idSolicitud
                });
            }
        }
    }
}