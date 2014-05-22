namespace Slimsy
{
    using Umbraco.Core.Models;
    using Umbraco.Web;
    using Umbraco.Web.Models;

    public static class Slimsy
    {
        public static string GetResponsiveImageUrl(this IPublishedContent publishedContent, int width, int height)
        {
            return publishedContent.GetCropUrl(
                width,
                height,
                ratioMode: ImageCropRatioMode.Height,
                furtherOptions: "&slimmage=true");
        }

        public static string GetResponsiveImageUrl(this IPublishedContent publishedContent, int width, int height, string propertyAlias)
        {
            return publishedContent.GetCropUrl(
                width,
                height,
                propertyAlias,
                ratioMode: ImageCropRatioMode.Height,
                furtherOptions: "&slimmage=true");
        }

    }
}
