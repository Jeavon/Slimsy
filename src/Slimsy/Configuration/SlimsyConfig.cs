namespace Slimsy.Configuration
{
    public class SlimsyConfig
    {
        public string Format { get; set; }
        public string BackgroundColor { get; set; }
        public int DefaultQuality { get; set; }
        public int MaxWidth { get; set; }
        public int WidthStep { get; set; }
        public string DomainPrefix { get; set; }

        public SlimsyConfig(){}
    }
}
