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
            //    options.DomainPrefix = "https://mytestsiteprefix.com";
            //    return options;
            //});

            composition.Components().Append<TestSiteComponent>();
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
            var slimsyOptions = _factory.GetInstance<SlimsyOptions>();

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
}
