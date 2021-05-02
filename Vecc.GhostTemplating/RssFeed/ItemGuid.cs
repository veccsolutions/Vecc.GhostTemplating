using System.Xml.Serialization;

namespace Vecc.GhostTemplating.RssFeed
{
    public class ItemGuid
    {
        [XmlAttribute("isPermaLink")]
        public bool IsPermaLink { get; set; }

        [XmlText]
        public string Value { get; set; }
    }
}
