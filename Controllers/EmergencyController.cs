using Microsoft.AspNetCore.Mvc;
using SOS.Models;
using System.Threading.Tasks;

namespace SOS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmergencyController : ControllerBase
    {
        private readonly AlertasRepository _alertasRepository;
        private readonly ConfirmacionesRepository _confirmacionesRepository;

        public EmergencyController(AlertasRepository alertasRepository, ConfirmacionesRepository confirmacionesRepository)
        {
            _alertasRepository = alertasRepository;
            _confirmacionesRepository = confirmacionesRepository;
        }

        /// <summary>
        /// Activa una nueva alerta de evacuación.
        /// Este endpoint debe ser llamado por un Oficial de Seguridad.
        /// </summary>
        /// <param name="alerta">Datos de la alerta a registrar.</param>
        [HttpPost("activate")]
        public async Task<IActionResult> ActivateAlert([FromBody] AlertaEvacuacion alerta)
        {
            alerta.FechaHoraActivacion = DateTime.UtcNow;
            alerta.EstadoAlerta = "Activa";
            alerta.FechaRegistro = DateTime.UtcNow;
            alerta.Activo = true;

            var id = await _alertasRepository.InsertarAlertaAsync(alerta);

            // TODO: Lógica para enviar la notificación push masiva aquí
            // Esto implicaría llamar a Azure Notification Hubs
            // o a un servicio de notificaciones similar.

            return Ok(new { message = "Alerta de evacuación activada exitosamente.", alertaId = id });
        }

        /// <summary>
        /// Registra la confirmación de seguridad de un colaborador.
        /// </summary>
        /// <param name="confirmacion">Datos de la confirmación a registrar.</param>
        [HttpPost("confirm-safety")]
        public async Task<IActionResult> ConfirmSafety([FromBody] ConfirmacionSeguridad confirmacion)
        {
            confirmacion.FechaHoraConfirmacion = DateTime.UtcNow;
            confirmacion.FechaRegistro = DateTime.UtcNow;
            confirmacion.Activo = true;

            var id = await _confirmacionesRepository.InsertarConfirmacionAsync(confirmacion);

            return Ok(new { message = "Confirmación de seguridad registrada exitosamente.", confirmacionId = id });
        }
        
        /// <summary>
        /// Obtiene la última alerta de evacuación activa.
        /// </summary>
        [HttpGet("latest-alert")]
        public async Task<ActionResult<AlertaEvacuacion>> GetLatestActiveAlert()
        {
            var latestAlert = await _alertasRepository.ObtenerUltimaAlertaActivaAsync();

            if (latestAlert == null)
            {
                return NotFound("No hay alertas activas en este momento.");
            }

            return Ok(latestAlert);
        }
    }
}