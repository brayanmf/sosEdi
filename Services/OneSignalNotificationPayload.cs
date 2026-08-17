using System.Text.Json.Serialization;

namespace SOS.Services
{
    /// <summary>
    /// Estructura de datos que representa el payload para enviar a OneSignal API.
    /// </summary>
    public class OneSignalNotificationPayload
    {
        [JsonPropertyName("app_id")]
        public string? app_id { get; set; }

        [JsonPropertyName("included_segments")]
        public string[]? included_segments { get; set; }

        [JsonPropertyName("include_player_ids")]
        public string[]? include_player_ids { get; set; }

        [JsonPropertyName("headings")]
        public Dictionary<string, string>? headings { get; set; }

        [JsonPropertyName("contents")]
        public Dictionary<string, string>? contents { get; set; }

        [JsonPropertyName("data")]
        public Dictionary<string, object>? data { get; set; }

        [JsonPropertyName("priority")]
        public int priority { get; set; }

        [JsonPropertyName("isAndroid")]
        public bool isAndroid { get; set; }

        [JsonPropertyName("isIos")]
        public bool isIos { get; set; }

        [JsonPropertyName("android_channel_id")]
        public string? android_channel_id { get; set; }

        [JsonPropertyName("big_picture")]
        public string? big_picture { get; set; }

        [JsonPropertyName("large_icon")]
        public string? large_icon { get; set; }

        [JsonPropertyName("priority_android")]
        public string? priority_android { get; set; }

        [JsonPropertyName("ttl")]
        public int ttl { get; set; }

        [JsonPropertyName("android_sound")]
        public string? android_sound { get; set; }

        [JsonPropertyName("ios_sound")]
        public string? ios_sound { get; set; }

        [JsonPropertyName("ios_badgeType")]
        public string? ios_badgeType { get; set; }

        [JsonPropertyName("ios_badgeCount")]
        public int ios_badgeCount { get; set; }

        [JsonPropertyName("apns_alert")]
        public Dictionary<string, object>? apns_alert { get; set; }
    }
}
