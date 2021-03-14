using System.Text.Json.Serialization;

namespace Vecc.GhostTemplating.Client
{
    public class TagsResponse
    {
        [JsonPropertyName("tags")]
        public Tag[] Tags { get; set; }
    }
}
