namespace TestSite.Common
{
    using System.Web.Mvc;
    using Slimsy.Services;
    using Umbraco.Web;
    using Umbraco.Web.Models;
    using Umbraco.Web.PublishedModels;

    public class ProductController : Umbraco.Web.Mvc.RenderMvcController
    {
        private readonly SlimsyService _slimsyService;
        private readonly UrlHelper _urlHelper;

        public ProductController(SlimsyService slimsyService)
        {
            _slimsyService = slimsyService;
            _urlHelper = new UrlHelper();
        }
        public override ActionResult Index(ContentModel model)
        {
            var product = model.Content as Product;
            var photo = product.Photos;
            var vm = new ProductViewModel(product)
            {
                PhotoSrc = _urlHelper.GetCropUrl(photo, "feature"),
                PhotoSrcSetUrls = _slimsyService.GetSrcSetUrls(photo, "feature")
            };

            return CurrentTemplate(vm);
        }
    }
}
