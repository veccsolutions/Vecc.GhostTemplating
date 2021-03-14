using System.Text.Json.Serialization;

namespace Vecc.GhostTemplating
{
    public class Tag : IGetHeader
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("slug")]
        public string Slug { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("feature_image")]
        public string FeatureImage { get; set; }

        [JsonPropertyName("visibility")]
        public string Visibility { get; set; }

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

        [JsonPropertyName("twitter_title")]
        public string TwitterTitle { get; set; }

        [JsonPropertyName("twitter_description")]
        public string TwitterDescription { get; set; }

        [JsonPropertyName("codeinjection_head")]
        public string CodeInjectionHead { get; set; }

        [JsonPropertyName("codeinjection_foot")]
        public string CodeInjectionFooter { get; set; }

        [JsonPropertyName("canonical_url")]
        public string CanonicalUrl { get; set; }

        [JsonPropertyName("accent_color")]
        public string AccentColor { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        public string Previous { get; set; }
        public string Next { get; set; }
        public Post[] PostsToDisplay { get; set; }
        public Settings Settings { get; set; }
        public int PostCount { get; set; }

        public Header GetHeader()
        {
            return new Header
            {
                Author = null,
                BodyClasses = "tag-template",
                InnerDivClasses = "inner posts",
                HeaderClass = "site-archive-header",
                OGDescription = MetaDescription,
                Settings = Settings
            };
        }

    }
}
