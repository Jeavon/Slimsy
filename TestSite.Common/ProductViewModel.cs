namespace TestSite.Common
{
    using System.Web;
    using Umbraco.Web.PublishedModels;

    public class ProductViewModel : Product
    {
        public ProductViewModel(Product content) : base(content) { }
        public IHtmlString PhotoSrc { get; set; }
        public IHtmlString PhotoSrcSetUrls { get; set; }
    }
}
