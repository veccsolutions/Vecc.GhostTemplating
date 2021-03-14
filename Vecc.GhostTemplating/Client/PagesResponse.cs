using System.Text.Json.Serialization;

namespace Vecc.GhostTemplating.Client
{
    public class PagesResponse
    {
        [JsonPropertyName("pages")]
        public Page[] Pages { get; set; }
    }
}
