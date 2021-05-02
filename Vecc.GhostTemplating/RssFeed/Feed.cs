using System.Xml.Serialization;

namespace Vecc.GhostTemplating.RssFeed
{
    [XmlRoot(ElementName = "rss")]
    public class Feed
    {
        [XmlElement("channel")]
        public Channel[] Channels { get; set; }

        [XmlAttribute("version")]
        public string Version { get; set; } = "2.0";
    }
}
