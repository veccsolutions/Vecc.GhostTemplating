using System;

namespace Vecc.GhostTemplating
{
    public class TemplatingOptions
    {
        public string AdminKey { get; set; }
        public string ContentKey { get; set; }
        public string Copyright { get; set; }
        public string DisqusUrl { get; set; }
        public string FavoriteIcon { get; set; }
        public string FeedlyLink { get; set; }
        public string FormattedCopyright { get => Copyright?.Replace("{year}", DateTime.Now.Year.ToString()).Replace("{sitename}", SiteName); }
        public string GeneratorName { get; set; }
        public Uri GhostUrl { get; set; }
        public string GoogleAnalyticsCode { get; set; }
        public string GoogleAnalyticsDomain { get; set; }
        public string RssFeed { get; set; }
        public TimeSpan RssTimeToLive { get; set; }
        public string SiteName { get; set; }
        public string OutputDirectory { get; set; }
    }
}
