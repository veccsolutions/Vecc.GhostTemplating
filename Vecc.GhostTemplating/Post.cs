using System;
using System.Text.Json.Serialization;

namespace Vecc.GhostTemplating
{
    public class Post : IGetHeader
    {
        [JsonPropertyName("slug")]
        public string Slug { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("uuid")]
        public string UUID { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("html")]
        public string HTML { get; set; }

        [JsonPropertyName("plaintext")]
        public string PlainText { get; set; }

        [JsonPropertyName("comment_id")]
        public string CommentId { get; set; }

        [JsonPropertyName("feature_image")]
        public string FeatureImage { get; set; }

        [JsonPropertyName("featured")]
        public bool Featured { get; set; }

        [JsonPropertyName("visibility")]
        public string Visibility { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("published_at")]
        public DateTime? PublishedAt { get; set; }

        [JsonPropertyName("custom_excerpt")]
        public string CustomExcerpt { get; set; }

        [JsonPropertyName("codeinjection_head")]
        public string CodeInjectionHead { get; set; }

        [JsonPropertyName("codeinjection_foot")]
        public string CodeInjectionFooter { get; set; }

        [JsonPropertyName("custom_template")]
        public string CustomTemplate { get; set; }

        [JsonPropertyName("canonical_url")]
        public string CanonicalUrl { get; set; }

        [JsonPropertyName("send_email_when_published")]
        public bool SendEmailWhenPublished { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("excerpt")]
        public string Excerpt { get; set; }

        [JsonPropertyName("reading_time")]
        public int ReadingTime { get; set; }

        [JsonPropertyName("access")]
        public bool Access { get; set; }

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

        [JsonPropertyName("meta_title")]
        public string MetaTitle { get; set; }

        [JsonPropertyName("meta_description")]
        public string MetaDescription { get; set; }

        [JsonPropertyName("email_subject")]
        public string EmailSubject { get; set; }

        [JsonPropertyName("authors")]
        public Author[] Authors { get; set; }

        [JsonPropertyName("primary_author")]
        public Author PrimaryAuthor { get; set; }

        [JsonPropertyName("tags")]
        public Tag[] Tags { get; set; }

        [JsonPropertyName("primary_tag")]
        public Tag PrimaryTag { get; set; }

        public Settings Settings { get; set; }

        public Header GetHeader()
        {
            var result = new Header();

            result.Author = PrimaryAuthor;
            result.BodyClasses = "post-template";
            result.Description = MetaDescription ?? CustomExcerpt ?? Excerpt;
            result.ImageHeight = 0;
            result.ImageWidth = 0;
            result.OGDescription = OGDescription;
            result.OGImage = OGImage;
            result.OGTitle = OGTitle;
            result.OGUrl = Url;
            result.Settings = Settings;
            result.Title = Title;
            result.TwitterCard = "summary";
            result.TwitterDescription = TwitterDescription;
            result.TwitterImage = TwitterImage;
            result.TwitterTitle = TwitterTitle;
            result.TwitterUrl = Url;
            result.Url = Url;

            return result;
        }
    }
}
