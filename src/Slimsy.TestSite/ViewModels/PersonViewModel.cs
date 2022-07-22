namespace Slimsy.TestSite.ViewModels
{
    using Microsoft.AspNetCore.Html;
    using Umbraco.Cms.Core.Models.PublishedContent;
    using Umbraco.Cms.Web.Common.PublishedModels;

    public class PersonViewModel : Person
    {
        public PersonViewModel(IPublishedContent content, IPublishedValueFallback publishedValueFallback) : base(content, publishedValueFallback)
        {
        }

        public IHtmlContent PictureSrc { get; set; }
        public IHtmlContent PictureSrcSet { get; set; }
    }
}
