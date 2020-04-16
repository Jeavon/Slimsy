
namespace TestSite.Common.Controllers
{
    using Models.ViewModels;
    using Slimsy.Interfaces;
    using System.Web.Mvc;
    using Umbraco.Web.Models;
    using Umbraco.Web.Mvc;
    using Umbraco.Web.PublishedModels;

    public class ProductController : RenderMvcController
    {
        private readonly ISlimsyService _slimsyService;

        public ProductController(ISlimsyService slimsyService)
        {
            this._slimsyService = slimsyService;
        }

        public override ActionResult Index(ContentModel model)
        {
            var product = model.Content as Product;
            var photo = product.Photos;
            var vm = new ProductViewModel(product)
            {
                PhotoSrc = this._slimsyService.GetCropUrl(photo, "feature"),
                PhotoSrcSetUrls = this._slimsyService.GetSrcSetUrls(photo, "feature")
            };

            return this.CurrentTemplate(vm);
        }
    }
}
