namespace Slimsy.Application
{
    using System.IO;
    using Microsoft.Extensions.Configuration;
    using Slimsy.Interfaces;

    //https://stackoverflow.com/questions/41772001/access-to-configuration-without-dependency-injection
    public class SlimsySettings : ISlimsyOptions
    {
        private static SlimsySettings _slimsySettings;

        public string Format { get; set; }
        public string BackgroundColor { get; set; }
        public int DefaultQuality { get; set; }
        public int MaxWidth { get; set; }
        public int WidthStep { get; set; }
        public string DomainPrefix { get; set; }

        public SlimsySettings(IConfiguration config)
        {
            var slimsyDefaultQuality = config.GetValue<int>("DefaultQuality");
            if (slimsyDefaultQuality == 0)
            {
                slimsyDefaultQuality = 90;
            }

            var slimsyWidthStep = config.GetValue<int>("WidthStep");
            if (slimsyWidthStep == 0)
            {
                slimsyWidthStep = 160;
            }

            var slimsyMaxWidth = config.GetValue<int>("MaxWidth");
            if (slimsyMaxWidth == 0)
            {
                slimsyMaxWidth = 2048;
            }

            var slimsyFormat = config.GetValue<string>("Format");
            var outputFormat = slimsyFormat ?? "auto";

            var slimsyBgColor = config.GetValue<string>("BGColor");
            var bgColor = slimsyBgColor != "false" ? slimsyBgColor : string.Empty;

            var domainPrefix = config.GetValue<string>("DomainPrefix");
            if (string.IsNullOrEmpty(domainPrefix))
                domainPrefix = string.Empty;

            Format = outputFormat;
            BackgroundColor = bgColor;
            DefaultQuality = slimsyDefaultQuality;
            MaxWidth = slimsyMaxWidth;
            WidthStep = slimsyWidthStep;
            DomainPrefix = domainPrefix;

            _slimsySettings = this;
        }

        public static SlimsySettings? Instance
        {
            get
            {
                if (_slimsySettings == null)
                {
                    _slimsySettings = GetCurrentSettings();
                }

                return _slimsySettings;
            }
        }

        public static SlimsySettings? GetCurrentSettings()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfiguration configuration = builder.Build();

            var settings = new SlimsySettings(configuration.GetSection("SlimsyConfiguration"));
            return settings;
        }
    }
}
