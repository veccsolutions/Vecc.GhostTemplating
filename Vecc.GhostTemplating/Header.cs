namespace Vecc.GhostTemplating
{
    public class Header
    {
        public Author Author { get; set; }
        public string HeaderClass { get; set; } = "site-header";
        public string BodyClasses { get; set; }
        public string InnerDivClasses { get; set; } = "inner";
        public string OGDescription { get; set; }
        public string OGType { get; set; } = "website";
        public string OGTitle { get; set; }
        public string OGUrl { get; set; }
        public string OGImage { get; set; }
        public Settings Settings { get; set; }
        public string TwitterCard { get; set; }
        public string TwitterTitle { get; set; }
        public string TwitterDescription { get; set; }
        public string TwitterUrl { get; set; }
        public string TwitterImage { get; set; }
        public int ImageWidth { get; set; } = 2000;
        public int ImageHeight { get; set; } = 2667;
        public string Title { get; internal set; }
        public string Url { get; set; }
        public Navigation[] Navigation { get => Settings.Navigation; }
        public bool RenderNavigation { get; set; } = true;
    }
}
