// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Slimsy.cs" company="Our.Umbraco">
//   2014
// </copyright>
// <summary>
//   Defines the Slimsy type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

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

        // this could be a overload of GetResponsiveImageUrl but then dynamics can't use it, hence a new name
        public static string GetResponsiveCropUrl(this IPublishedContent publishedContent, string cropAlias)
        {
            return publishedContent.GetCropUrl(
                cropAlias: cropAlias,
                useCropDimensions: true,
                ratioMode: ImageCropRatioMode.Height,
                furtherOptions: "&slimmage=true");
        }

        public static string GetResponsiveCropUrl(this IPublishedContent publishedContent, string propertyAlias, string cropAlias)
        {
            return publishedContent.GetCropUrl(
                propertyAlias: propertyAlias,
                cropAlias: cropAlias,
                useCropDimensions: true,
                ratioMode: ImageCropRatioMode.Height,
                furtherOptions: "&slimmage=true");
        }


    }
}
