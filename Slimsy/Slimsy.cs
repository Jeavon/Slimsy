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
    using System.Configuration;

    using Umbraco.Core;
    using Umbraco.Core.Models;
    using Umbraco.Web;
    using Umbraco.Web.Models;

    public static class Slimsy
    {
        public static string GetResponsiveImageUrl(this IPublishedContent publishedContent, int width, int height)
        {
            return publishedContent.GetResponsiveImageUrl(width, height, Constants.Conventions.Media.File);
        }

        public static string GetResponsiveImageUrl(this IPublishedContent publishedContent, int width, int height, string propertyAlias)
        {
            string returnUrl;

            if (height == 0)
            {
                returnUrl = publishedContent.GetCropUrl(
                    width,
                    null,
                    propertyAlias,
                    quality: 90,
                    upScale: false,
                    furtherOptions: string.Format("{0}&slimmage=true", Format()));
            }
            else
            {
                returnUrl = publishedContent.GetCropUrl(
                    width,
                    height,
                    propertyAlias,
                    quality: 90,
                    upScale: false,
                    ratioMode: ImageCropRatioMode.Height,
                    furtherOptions: string.Format("{0}&slimmage=true", Format()));
            }

            return returnUrl != null ? returnUrl.ToLowerInvariant() : null;
        }

        // this could be a overload of GetResponsiveImageUrl but then dynamics can't use it, hence a new name
        public static string GetResponsiveCropUrl(this IPublishedContent publishedContent, string cropAlias)
        {
            var returnUrl = publishedContent.GetCropUrl(
                cropAlias: cropAlias,
                useCropDimensions: true,
                quality: 90,
                ratioMode: ImageCropRatioMode.Height,
                upScale: false,
                furtherOptions: string.Format("{0}&slimmage=true", Format()));

            return returnUrl != null ? returnUrl.ToLowerInvariant() : null;
        }

        public static string GetResponsiveCropUrl(this IPublishedContent publishedContent, string cropAlias, string propertyAlias)
        {
            var returnUrl = publishedContent.GetCropUrl(
                propertyAlias: propertyAlias,
                cropAlias: cropAlias,
                useCropDimensions: true,
                quality: 90,
                ratioMode: ImageCropRatioMode.Height,
                upScale: false,
                furtherOptions: string.Format("{0}&slimmage=true", Format()));

            return returnUrl != null ? returnUrl.ToLowerInvariant() : null;
        }

        private static string Format()
        {
            var slimsyFormat = ConfigurationManager.AppSettings["Slimsy:Format"];
            return slimsyFormat != "false" ? string.Format("&format={0}", slimsyFormat ?? "jpg") : string.Empty;
        }
    }
}
