using System.Xml.Serialization;

namespace Vecc.GhostTemplating.RssFeed
{
    public class Image
    {
        [XmlElement("url")]
        public string Url { get; set; }

        [XmlElement("title")]
        public string Title { get; set; }

        [XmlElement("link")]
        public string Link { get; set; }
    }
}
