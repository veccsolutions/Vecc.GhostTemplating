using System.Xml.Serialization;

namespace Vecc.GhostTemplating.RssFeed
{
    public class Item
    {
        [XmlElement("author", Namespace = "http://www.w3.org/2005/Atom")]
        public ItemAuthor[] Authors { get; set; }

        [XmlElement("title")]
        public string Title { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlElement("link")]
        public string Link { get; set; }

        [XmlElement("guid")]
        public ItemGuid Guid { get; set; }

        [XmlElement("category")]
        public string[] Categories { get; set; }

        [XmlElement("creator", Namespace = "http://purl.org/dc/elements/1.1/")]
        public string Creator { get; set; }

        [XmlElement("pubDate")]
        public string PublishedDate { get; set; }

        [XmlElement("encoded", Namespace = "http://purl.org/rss/1.0/modules/content/")]
        public string Content { get; set; }
    }
}
