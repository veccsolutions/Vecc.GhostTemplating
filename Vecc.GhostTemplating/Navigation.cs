using System.Text.Json.Serialization;

namespace Vecc.GhostTemplating
{
    public class Navigation
    {
        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
