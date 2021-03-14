using System.Text.Json.Serialization;

namespace Vecc.GhostTemplating.Client
{
    public class AuthorsResponse
    {
        [JsonPropertyName("authors")]
        public Author[] Authors { get; set; }
    }
}
