using System.Web;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Models;

namespace TestSite.Common
{
    public class ProductViewModel : ContentModel
    {
        public ProductViewModel(IPublishedContent content) : base(content) { }
        public IHtmlString PhotoSrc { get; set; }
        public IHtmlString PhotoSrcSetUrls { get; set; }
    }
}
