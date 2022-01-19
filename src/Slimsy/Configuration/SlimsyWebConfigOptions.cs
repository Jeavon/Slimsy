namespace Slimsy.Configuration
{
    using Slimsy.Application;
    using Slimsy.Interfaces;

    public class SlimsyWebConfigOptions : ISlimsyOptions
    {
        public SlimsyWebConfigOptions()
        {
            Format = SlimsySettings.Instance.Format;
            BackgroundColor = SlimsySettings.Instance.BackgroundColor;
            DefaultQuality = SlimsySettings.Instance.DefaultQuality;
            MaxWidth = SlimsySettings.Instance.MaxWidth;
            WidthStep = SlimsySettings.Instance.WidthStep;
            DomainPrefix = SlimsySettings.Instance.DomainPrefix;
        }

        public string Format { get; set; }
        public string BackgroundColor { get; set; }
        public int DefaultQuality { get; set; }
        public int MaxWidth { get; set; }
        public int WidthStep { get; set; }
        public string DomainPrefix { get; set; }
    }
}
