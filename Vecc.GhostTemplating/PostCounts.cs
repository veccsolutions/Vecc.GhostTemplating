using System.Text.Json.Serialization;

namespace Vecc.GhostTemplating
{
    public class PostCounts
    {
        [JsonPropertyName("posts")]
        public int Posts { get; set; }
    }
}
