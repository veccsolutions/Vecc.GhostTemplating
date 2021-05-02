using System.Xml.Serialization;

namespace Vecc.GhostTemplating.RssFeed
{
    public class AtomLink
    {
        [XmlAttribute("rel")]
        public string Relationship { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("href")]
        public string HRef { get; set; }
    }
}
