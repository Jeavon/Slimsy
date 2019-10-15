namespace Slimsy
{
    using System.Configuration;
    public class SlimsyWebConfigOptions : ISlimsyOptions
    {
        public SlimsyWebConfigOptions()
        {
            var slimsyDefaultQuality = ConfigurationManager.AppSettings["Slimsy:DefaultQuality"];
            if (!int.TryParse(slimsyDefaultQuality, out int defaultQuality))
            {
                defaultQuality = 90;
            }

            var slimsyWidthStep = ConfigurationManager.AppSettings["Slimsy:WidthStep"];
            if (!int.TryParse(slimsyWidthStep, out int widthStep))
            {
                widthStep = 160;
            }

            var slimsyMaxWidth = ConfigurationManager.AppSettings["Slimsy:MaxWidth"];
            if (!int.TryParse(slimsyMaxWidth, out int maxWidth))
            {
                maxWidth = 2048;
            }

            var slimsyFormat = ConfigurationManager.AppSettings["Slimsy:Format"];
            var outputFormat = slimsyFormat ?? "auto";

            var slimsyBgColor = ConfigurationManager.AppSettings["Slimsy:BGColor"];
            var bgColor = slimsyBgColor != null && slimsyBgColor != "false" ? slimsyBgColor : string.Empty;

            Format = outputFormat;
            BackgroundColor = bgColor;
            MaxWidth = maxWidth;
            WidthStep = widthStep;
            DefaultQuality = defaultQuality;
            DomainPrefix = "https://www.umbraco.com";
        }

        public string Format { get; set; }
        public string BackgroundColor { get; set; }
        public int DefaultQuality { get; set; }
        public int MaxWidth { get; set; }
        public int WidthStep { get; set; }
        public string DomainPrefix { get; set; }
    }
}
