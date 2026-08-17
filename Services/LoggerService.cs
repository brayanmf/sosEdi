using System;
using System.Collections.Generic;

namespace SOS.Services
{
    /// <summary>
    /// Servicio centralizado de logging para la aplicación SOS.
    /// Proporciona métodos para registrar información, advertencias y errores.
    /// </summary>
    public class LoggerService
    {
        private readonly ILogger<LoggerService> _logger;

        public LoggerService(ILogger<LoggerService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Registra un evento informativo.
        /// </summary>
        public void LogInfo(string mensaje, Dictionary<string, object>? datos = null)
        {
            var logMessage = FormatearMensaje(mensaje, datos);
            _logger.LogInformation($"[INFO] {logMessage}");
        }

        /// <summary>
        /// Registra una advertencia.
        /// </summary>
        public void LogWarning(string mensaje, Dictionary<string, object>? datos = null)
        {
            var logMessage = FormatearMensaje(mensaje, datos);
            _logger.LogWarning($"[WARNING] {logMessage}");
        }

        /// <summary>
        /// Registra un error.
        /// </summary>
        public void LogError(string mensaje, Exception? ex = null, Dictionary<string, object>? datos = null)
        {
            var logMessage = FormatearMensaje(mensaje, datos);
            if (ex != null)
            {
                _logger.LogError(ex, $"[ERROR] {logMessage}");
            }
            else
            {
                _logger.LogError($"[ERROR] {logMessage}");
            }
        }

        /// <summary>
        /// Registra una excepción crítica.
        /// </summary>
        public void LogCritical(string mensaje, Exception? ex = null, Dictionary<string, object>? datos = null)
        {
            var logMessage = FormatearMensaje(mensaje, datos);
            if (ex != null)
            {
                _logger.LogCritical(ex, $"[CRITICAL] {logMessage}");
            }
            else
            {
                _logger.LogCritical($"[CRITICAL] {logMessage}");
            }
        }

        /// <summary>
        /// Registra información de debug.
        /// </summary>
        public void LogDebug(string mensaje, Dictionary<string, object>? datos = null)
        {
            var logMessage = FormatearMensaje(mensaje, datos);
            _logger.LogDebug($"[DEBUG] {logMessage}");
        }

        /// <summary>
        /// Formatea el mensaje de log con datos adicionales.
        /// </summary>
        private static string FormatearMensaje(string mensaje, Dictionary<string, object>? datos)
        {
            if (datos == null || datos.Count == 0)
                return mensaje;

            var datosFormato = string.Join(" | ", datos.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            return $"{mensaje} | {datosFormato}";
        }
    }
}
