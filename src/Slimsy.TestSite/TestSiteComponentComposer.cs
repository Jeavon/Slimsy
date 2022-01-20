namespace Website
{
    using System;
    using System.Linq;
    using Slimsy.Application;
    using Umbraco.Cms.Core.Composing;
    using Umbraco.Cms.Core.Web;
    using Umbraco.Extensions;

    // option 3: 
    //public class TestSiteComponentComposer : ComponentComposer<TestSiteComponentComposer2>, IComposer { }

    public class TestSiteComponentComposer2 : IComponent
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IUmbracoContextFactory _context;

        public TestSiteComponentComposer2(IServiceProvider serviceProvider, IUmbracoContextFactory context)
        {
            this._serviceProvider = serviceProvider;
            this._context = context;
        }

        public void Initialize()
        {
            var slimsyOptions = SlimsyComposer.GetDefaultOptions(this._serviceProvider);
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
