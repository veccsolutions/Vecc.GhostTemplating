namespace Vecc.GhostTemplating
{
    public class IndexPage : IGetHeader
    {
        public TemplatingOptions Options { get; set; }
        public Settings Settings { get; set; }
        public Post[] PostsToDisplay { get; set; }
        public string Next { get; set; }
        public string Previous { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }

        public Header GetHeader()
        {
            var result = new Header();

            result.HeaderClass = "site-home-header";
            result.OGDescription = Settings.OGDescription;
            result.OGTitle = Options.SiteName;
            result.RenderNavigation = false;
            result.Settings = Settings;

            return result;
        }
    }
}
