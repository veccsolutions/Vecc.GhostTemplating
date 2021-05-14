using System.Linq;
using System.Text.Json.Serialization;

namespace Vecc.GhostTemplating
{
    public class Page : Post
    {
        [JsonPropertyName("mobile_doc")]
        public string MobileDoc { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        public static implicit operator Header(Page page)
        {
            var result = new Header();

            if (page.Tags!= null && page.Tags.Any())
            {
                result.BodyClasses = "page-template page-tags";
            }
            else
            {
                result.BodyClasses = "page-template";
            }

            result.Author = page.PrimaryAuthor;
            result.Description = page.MetaDescription ?? page.CustomExcerpt ?? page.Excerpt;
            result.ImageHeight = 0;
            result.ImageWidth = 0;
            result.OGDescription = page.OGDescription;
            result.OGImage = page.OGImage;
            result.OGTitle = page.OGTitle;
            result.OGUrl = page.Url;
            result.Settings = page.Settings;
            result.Title = page.Title;
            result.TwitterCard = "summary";
            result.TwitterDescription = page.TwitterDescription;
            result.TwitterImage = page.TwitterImage;
            result.TwitterTitle = page.TwitterTitle;
            result.TwitterUrl = page.Url;
            result.Url = page.Url;

            return result;
        }
    }
}
