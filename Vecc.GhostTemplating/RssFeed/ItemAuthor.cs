using System.Xml.Serialization;

namespace Vecc.GhostTemplating.RssFeed
{
    public class ItemAuthor
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("uri")]
        public string Uri { get; set; }
    }
}
