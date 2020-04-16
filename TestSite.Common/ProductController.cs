using System.Web.Mvc;
using Slimsy;
using Umbraco.Web;
using Umbraco.Web.Models;
using Umbraco.Web.PublishedModels;

namespace TestSite.Common
{
    public class ProductController : Umbraco.Web.Mvc.RenderMvcController
    {
        private readonly SlimsyService _slimsyComponent;
        private readonly UrlHelper _urlHelper;

        public ProductController(SlimsyService slimsyService)
        {
            _slimsyComponent = slimsyService;
            _urlHelper = new UrlHelper();
        }
        public override ActionResult Index(ContentModel model)
        {
            var product = model.Content as Product;
            var photo = product.Photos;
            var vm = new ProductViewModel(product)
            {
                PhotoSrc = _urlHelper.GetCropUrl(photo, "feature"),
                PhotoSrcSetUrls = _slimsyComponent.GetSrcSetUrls(photo, "feature")
            };

            return CurrentTemplate(vm);
        }
    }
}
