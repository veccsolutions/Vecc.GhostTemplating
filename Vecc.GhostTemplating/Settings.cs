using System.Text.Json.Serialization;

namespace Vecc.GhostTemplating
{
    public class Settings
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("logo")]
        public string Logo { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        [JsonPropertyName("accent_color")]
        public string AccentColor { get; set; }

        [JsonPropertyName("cover_image")]
        public string CoverImage { get; set; }

        [JsonPropertyName("facebook")]
        public string Facebook { get; set; }

        [JsonPropertyName("twitter")]
        public string Twitter { get; set; }

        [JsonPropertyName("lang")]
        public string Language { get; set; }

        [JsonPropertyName("timezone")]
        public string Timezone { get; set; }

        [JsonPropertyName("codeinjection_head")]
        public string CodeInjectionHead { get; set; }

        [JsonPropertyName("codeinjection_foot")]
        public string CodeInjectionFooter { get; set; }

        [JsonPropertyName("navigation")]
        public Navigation[] Navigation { get; set; }

        [JsonPropertyName("secondary_navigation")]
        public Navigation[] SecondaryNavigation { get; set; }

        [JsonPropertyName("meta_title")]
        public string MetaTitle { get; set; }

        [JsonPropertyName("meta_description")]
        public string MetaDescription { get; set; }

        [JsonPropertyName("og_image")]
        public string OGImage { get; set; }

        [JsonPropertyName("og_title")]
        public string OGTitle { get; set; }

        [JsonPropertyName("og_description")]
        public string OGDescription { get; set; }

        [JsonPropertyName("twitter_image")]
        public string TwitterImage { get; set; }

        [JsonPropertyName("twitter_description")]
        public string TwitterDescription { get; set; }

        [JsonPropertyName("members_support_address")]
        public string MembersSupportAddress { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
