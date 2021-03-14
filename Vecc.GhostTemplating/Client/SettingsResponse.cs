using System.Text.Json.Serialization;

namespace Vecc.GhostTemplating.Client
{
    public class SettingsResponse
    {
        [JsonPropertyName("settings")]
        public Settings Settings { get; set; }
    }
}
