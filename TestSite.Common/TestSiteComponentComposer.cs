namespace TestSite.Common
{
    using System.Linq;
    using Slimsy.Interfaces;
    using Umbraco.Core;
    using Umbraco.Core.Composing;
    using Umbraco.Web;

    // option 3: 
    //public class TestSiteComponentComposer : ComponentComposer<TestSiteComponent2>, IUserComposer { }

    public class TestSiteComponent2 : IComponent
    {
        private readonly IFactory _factory;
        private readonly IUmbracoContextFactory _context;

        public TestSiteComponent2(IFactory factory, IUmbracoContextFactory context)
        {
            _factory = factory;
            _context = context;
        }

        public void Initialize()
        {
            var slimsyOptions = _factory.GetInstance<ISlimsyOptions>();

            using (var cref = _context.EnsureUmbracoContext())
            {
                var cache = cref.UmbracoContext.Content;
                var node = cache.GetAtRoot().FirstOrDefault();
                var domainPrefix = node.Value<string>("domainPrefix");
                slimsyOptions.DomainPrefix = domainPrefix;
                slimsyOptions.DefaultQuality = 45;
            }
        }
        public void Terminate()
        {
        }
    }
}
