
namespace Slimsy.Interfaces
{
    using Models;
    using System.Web;
    using Umbraco.Core.Models.PublishedContent;
    using Umbraco.Web.Models;

    public interface ISlimsyService
    {
        IHtmlString GetCropUrl(IPublishedContent mediaItem, string cropAlias, bool htmlEncode = true);

        IHtmlString GetSrcSetUrls(IPublishedContent publishedContent, int width, int height);

        IHtmlString GetSrcSetUrls(IPublishedContent publishedContent, int width, int height, int quality);

        IHtmlString GetSrcSetUrls(IPublishedContent publishedContent, int width, int height, string propertyAlias);

        IHtmlString GetSrcSetUrls(IPublishedContent publishedContent, int width, int height, string propertyAlias, string outputFormat, int quality = 90);

        IHtmlString GetSrcSetUrls(IPublishedContent publishedContent, int width, int height, ImageCropMode? imageCropMode, string outputFormat = "");

        IHtmlString GetSrcSetUrls(IPublishedContent publishedContent, AspectRatio aspectRatio);

        IHtmlString GetSrcSetUrls(IPublishedContent publishedContent, string cropAlias);

        IHtmlString GetSrcSetUrls(IPublishedContent publishedContent, string cropAlias, string propertyAlias);

        IHtmlString GetSrcSetUrls(IPublishedContent publishedContent, string cropAlias, string propertyAlias, string outputFormat, int quality = 90);

        IHtmlString ConvertImgToSrcSet(string sourceValueHtml, bool generateLqip = true, bool removeStyleAttribute = true);

        IHtmlString ConvertImgToSrcSet(IPublishedContent publishedContent, string propertyAlias, bool generateLqip = true, bool removeStyleAttribute = true);
    }
}
