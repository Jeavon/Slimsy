namespace Slimsy.TestSite.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ViewEngines;
    using Microsoft.Extensions.Logging;
    using Slimsy.Services;
    using Slimsy.TestSite.ViewModels;
    using Umbraco.Cms.Core.Models.PublishedContent;
    using Umbraco.Cms.Core.Services;
    using Umbraco.Cms.Core.Web;
    using Umbraco.Cms.Web.Common.Controllers;
    using Umbraco.Cms.Web.Common.PublishedModels;

    public class PersonController : RenderController
    {
        private readonly SlimsyService _slimsyService;
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly ServiceContext _serviceContext;

        public PersonController(ILogger<RenderController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor, SlimsyService slimsyService, IVariationContextAccessor variationContextAccessor, ServiceContext context) : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            this._slimsyService = slimsyService;
            this._variationContextAccessor = variationContextAccessor;
            this._serviceContext = context;
        }

        public override IActionResult Index()
        {
            var page = CurrentPage as Person;
            var vm = new PersonViewModel(CurrentPage,
                new PublishedValueFallback(_serviceContext, _variationContextAccessor))
            {
                PictureSrc = _slimsyService.GetCropUrl(page.Photo, 500, 0),
                PictureSrcSet = _slimsyService.GetSrcSetUrls(page.Photo, 500, 0)
            };

            return CurrentTemplate(vm);
        }
    }
}
