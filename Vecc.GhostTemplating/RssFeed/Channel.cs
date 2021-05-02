using System.Xml.Serialization;

namespace Vecc.GhostTemplating.RssFeed
{
    public class Channel
    {
        [XmlElement("title")]
        public string Title { get; set; }

        [XmlElement("copyright")]
        public string Copyright { get; set; }

        [XmlElement("lastBuildDate")]
        public string LastBuildDate { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlElement("link")]
        public string Link { get; set; }

        [XmlElement("image")]
        public Image Image { get; set; }

        [XmlElement("generator")]
        public string Generator { get; set; }

        [XmlElement("category")]
        public string[] Categories { get; set; }

        [XmlElement("contributor", Namespace ="http://www.w3.org/2005/Atom")]
        public Contributor[] Contributors { get; set; }

        [XmlElement("link", Namespace = "http://www.w3.org/2005/Atom")]
        public AtomLink AtomLink{get;set;}

        [XmlElement("ttl")]
        public int TTL { get; set; }

        [XmlElement("item")]
        public Item[] Items { get; set; }
    }
}
