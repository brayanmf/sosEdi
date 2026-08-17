using RestSharp;
using System.Text.Json.Serialization;

namespace SOS.Services
{
    /// <summary>
    /// Servicio para enviar notificaciones push a través de OneSignal.
    /// Permite enviar alertas de emergencia de alta prioridad a dispositivos móviles.
    /// </summary>
    public class OneSignalNotificationService
    {
        private readonly string _oneSignalAppId;
        private readonly string _oneSignalApiKey;
        private readonly string _oneSignalApiUrl = "https://onesignal.com/api/v1";
        private readonly ILogger<OneSignalNotificationService> _logger;

        public OneSignalNotificationService(IConfiguration configuration, ILogger<OneSignalNotificationService> logger)
        {
            _logger = logger;
            _oneSignalAppId = configuration["OneSignal:AppId"]
                ?? throw new InvalidOperationException("OneSignal:AppId no configurado");
            _oneSignalApiKey = configuration["OneSignal:ApiKey"]
                ?? throw new InvalidOperationException("OneSignal:ApiKey no configurado");
        }

        /// <summary>
        /// Envía una notificación de alerta de evacuación a todos los usuarios.
        /// </summary>
        /// <param name="titulo">Título de la notificación</param>
        /// <param name="mensaje">Cuerpo del mensaje</param>
        /// <param name="alerta">Datos de la alerta para incluir en la notificación</param>
        /// <returns>ID de la notificación en OneSignal</returns>
        public async Task<string?> EnviarAlertaEvacuacionAsync(
            string titulo,
            string mensaje,
            SOS.Models.AlertaEvacuacion alerta)
        {
            try
            {
                var client = new RestClient(_oneSignalApiUrl);
                var request = new RestRequest("/notifications", Method.Post);

                // Configurar headers
                request.AddHeader("Authorization", $"Basic {_oneSignalApiKey}");
                request.AddHeader("Content-Type", "application/json; charset=utf-8");

                // Crear el payload
                var payload = new OneSignalNotificationPayload
                {
                    app_id = _oneSignalAppId,
                    included_segments = new[] { "All" }, // Enviar a todos los usuarios
                    headings = new Dictionary<string, string> { { "en", titulo } },
                    contents = new Dictionary<string, string> { { "en", mensaje } },

                    // Datos personalizados para la aplicación móvil
                    data = new Dictionary<string, object>
                    {
                        { "alerta_id", alerta.Id },
                        { "tipo_alerta", alerta.TipoAlerta ?? "" },
                        { "latitud", alerta.LatitudActivacion ?? 0 },
                        { "longitud", alerta.LongitudActivacion ?? 0 },
                        { "descripcion", alerta.DescripcionUbicacionActivacion ?? "" },
                        { "estado", alerta.EstadoAlerta ?? "Activa" },
                        { "timestamp", DateTime.UtcNow.ToString("O") }
                    },

                    // Configuración de prioridad
                    priority = 10, // Máxima prioridad (escala 1-10)
                    isAndroid = true,
                    isIos = true,

                    // Configuración específica de Android
                    android_channel_id = "emergency_alerts",
                    big_picture = "", // URL de imagen grande opcional
                    large_icon = "", // URL de icono grande opcional
                    priority_android = "high", // High priority para Android
                    ttl = 3600, // TTL de 1 hora

                    // Sonido para Android
                    android_sound = "default",
                    ios_sound = "default",

                    // iOS específico
                    ios_badgeType = "Increase",
                    ios_badgeCount = 1,
                    apns_alert = new Dictionary<string, object>
                    {
                        { "title", titulo },
                        { "body", mensaje }
                    },
                };

                // Serializar payload
                var jsonContent = System.Text.Json.JsonSerializer.Serialize(payload);
                request.AddStringBody(jsonContent, ContentType.Json);

                // Ejecutar la solicitud
                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    _logger.LogInformation($"Notificación enviada exitosamente. Response: {response.Content}");

                    // Extraer ID de la respuesta (simplificado)
                    return ExtractNotificationId(response.Content);
                }
                else
                {
                    _logger.LogError($"Error al enviar notificación: {response.StatusCode} - {response.Content}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Excepción al enviar notificación con OneSignal: {ex.Message}", ex);
                return null;
            }
        }

        /// <summary>
        /// Envía notificación a un usuario específico por su player ID.
        /// </summary>
        /// <param name="playerId">ID del dispositivo en OneSignal</param>
        /// <param name="titulo">Título de la notificación</param>
        /// <param name="mensaje">Cuerpo del mensaje</param>
        public async Task<bool> EnviarNotificacionPorPlayerIdAsync(
            string playerId,
            string titulo,
            string mensaje)
        {
            try
            {
                var client = new RestClient(_oneSignalApiUrl);
                var request = new RestRequest("/notifications", Method.Post);

                request.AddHeader("Authorization", $"Basic {_oneSignalApiKey}");
                request.AddHeader("Content-Type", "application/json; charset=utf-8");

                var payload = new OneSignalNotificationPayload
                {
                    app_id = _oneSignalAppId,
                    include_player_ids = new[] { playerId },
                    headings = new Dictionary<string, string> { { "en", titulo } },
                    contents = new Dictionary<string, string> { { "en", mensaje } },
                    priority = 10,
                    priority_android = "high",
                };

                var jsonContent = System.Text.Json.JsonSerializer.Serialize(payload);
                request.AddStringBody(jsonContent, ContentType.Json);

                var response = await client.ExecuteAsync(request);
                return response.IsSuccessful;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al enviar notificación a usuario específico: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Extrae el ID de notificación de la respuesta de OneSignal.
        /// </summary>
        private static string? ExtractNotificationId(string? responseContent)
        {
            if (string.IsNullOrEmpty(responseContent))
                return null;

            try
            {
                var jsonDoc = System.Text.Json.JsonDocument.Parse(responseContent);
                if (jsonDoc.RootElement.TryGetProperty("body", out var body) &&
                    body.TryGetProperty("notification_id", out var notifId))
                {
                    return notifId.GetString();
                }
            }
            catch { /* Ignorar errores de parseo */ }

            return null;
        }
    }
}