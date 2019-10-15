using System.Linq;
using Slimsy;
using Umbraco.Core.Composing;
using Umbraco.Core;
using Umbraco.Web;

namespace TestSite.Common
{
    [ComposeAfter(typeof(SlimsyComposer))]
    public class TestSiteComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            // if you want to set the settings in code then do this
            //composition.SetSlimsyOptions(factory =>
            //{
            //    var options = SlimsyComposer.GetDefaultOptions(factory);
            //    options.DomainPrefix = "https://setviacomposer.com";
            //    options.WidthStep = 200;
            //    return options;
            //});

            // if you want to pull settings from Umbraco then use a component
            //composition.Components().Append<TestSiteComponent>();

            // if you want to replace SlimsyOptions with your own class/logic then do that
            //composition.RegisterUnique<ISlimsyOptions, SlimsyCustomConfigOptions>();
        }
    }

    public class TestSiteComponent : IComponent
    {
        private readonly IFactory _factory;
        private readonly IUmbracoContextFactory _context;

        public TestSiteComponent(IFactory factory, IUmbracoContextFactory context)
        {
            _factory = factory;
            _context = context;
        }
        public void Initialize()
        {
            // if you want to set the settings based on things stored in Umbraco do this
            var slimsyOptions = _factory.GetInstance<ISlimsyOptions>();

            using (var cref = _context.EnsureUmbracoContext())
            {
                var cache = cref.UmbracoContext.Content;
                var node = cache.GetAtRoot().FirstOrDefault();
                var domainPrefix = node.Value<string>("domainPrefix");
                slimsyOptions.DomainPrefix = domainPrefix;
                slimsyOptions.DefaultQuality = 95;
            }
        }

        public void Terminate()
        {
        }
    }

    public class SlimsyCustomConfigOptions : ISlimsyOptions
    {
        public SlimsyCustomConfigOptions()
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
}
