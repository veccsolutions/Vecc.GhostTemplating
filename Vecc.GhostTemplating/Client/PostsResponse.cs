using System.Text.Json.Serialization;

namespace Vecc.GhostTemplating.Client
{
    public class PostsResponse
    {
        [JsonPropertyName("posts")]
        public Post[] Posts { get; set; }
    }
}
