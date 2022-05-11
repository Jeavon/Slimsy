namespace Website
{
    using System.Linq;
    using Slimsy.Application;
    using Slimsy.Extensions;
    using Slimsy.Interfaces;
    using Umbraco.Cms.Core.Composing;
    using Umbraco.Cms.Core.DependencyInjection;
    using Umbraco.Cms.Core.Web;
    using Umbraco.Extensions;

    [ComposeAfter(typeof(SlimsyComposer))]
    public class TestSiteComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            // option 1: if you want to set the settings in code then do this
            //builder.SetSlimsyOptions(serviceProvider =>
            //{
            //    var options = SlimsyComposer.GetDefaultOptions(serviceProvider);
            //    //options.DomainPrefix = "https://setviacomposer.com";
            //    options.WidthStep = 200;
            //    return options;
            //});

            // option 2: if you want to replace SlimsyOptions with your own class/logic then do that
            //builder.Services.AddUnique<ISlimsyOptions, SlimsyCustomConfigOptions>();
        }
    }

    public class SlimsyCustomConfigOptions : ISlimsyOptions
    {
        public SlimsyCustomConfigOptions(IUmbracoContextFactory context)
        {
            // do some crazy stuff to get the config settings out of the flux capacitor 
            Format = "png";
            BackgroundColor = "";
            MaxWidth = 4000;
            WidthStep = 50;
            DefaultQuality = 95;
            DomainPrefix = "https://setviacustomconfigoptions.com";
        }
        public string Format { get; set; }
        public string BackgroundColor { get; set; }
        public int DefaultQuality { get; set; }
        public int MaxWidth { get; set; }
        public int WidthStep { get; set; }
        public string DomainPrefix { get; set; }
    }

    public class SlimsyConfigFromUmbracoOptions : ISlimsyOptions
    {
        public SlimsyConfigFromUmbracoOptions(IUmbracoContextFactory context)
        {
            // do some crazy stuff to get the config settings out of the flux capacitor

            using (var cref = context.EnsureUmbracoContext())
            {
                var cache = cref.UmbracoContext.Content;
                var node = cache.GetAtRoot().FirstOrDefault();
                var domainPrefix = node.Value<string>("domainPrefix");
                DomainPrefix = domainPrefix;
                DefaultQuality = 85;
            }

            Format = "png";
            BackgroundColor = "";
            MaxWidth = 4000;
            WidthStep = 50;
        }
        public string Format { get; set; }
        public string BackgroundColor { get; set; }
        public int DefaultQuality { get; set; }
        public int MaxWidth { get; set; }
        public int WidthStep { get; set; }
        public string DomainPrefix { get; set; }
    }
}
