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
    using System;
    using System.Configuration;
    using System.Linq;
    using System.Text;

    using Newtonsoft.Json;

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
            return publishedContent.GetResponsiveImageUrl(width, height, propertyAlias, null);
        }

        public static string GetResponsiveImageUrl(this IPublishedContent publishedContent, int width, int height, string propertyAlias, string outputFormat)
        {
            string returnUrl;

            if (height == 0)
            {
                returnUrl = publishedContent.GetCropUrl(
                    width,
                    null,
                    propertyAlias,
                    quality: 90,
                    furtherOptions: string.Format("{0}&slimmage=true", Format(outputFormat)));
            }
            else
            {
                returnUrl = publishedContent.GetCropUrl(
                    width,
                    height,
                    propertyAlias,
                    quality: 90,
                    ratioMode: ImageCropRatioMode.Height,
                    furtherOptions: string.Format("{0}&slimmage=true", Format(outputFormat)));
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
                furtherOptions: string.Format("{0}&slimmage=true", Format()));

            return returnUrl != null ? returnUrl.ToLowerInvariant() : null;
        }

        public static string GetResponsiveCropUrl(this IPublishedContent publishedContent, string cropAlias, string propertyAlias, string outputFormat)
        {
            var returnUrl = publishedContent.GetCropUrl(
                propertyAlias: propertyAlias,
                cropAlias: cropAlias,
                useCropDimensions: true,
                quality: 90,
                ratioMode: ImageCropRatioMode.Height,
                furtherOptions: string.Format("{0}&slimmage=true", Format(outputFormat)));

            return returnUrl != null ? returnUrl.ToLowerInvariant() : null;
        }

        private static string Format(string outputFormat = null)
        {
            var bgColor = string.Empty;
            if (outputFormat == null)
            {
                var slimsyFormat = ConfigurationManager.AppSettings["Slimsy:Format"];
                outputFormat = slimsyFormat != "false" ? slimsyFormat ?? "jpg" : string.Empty;
                var slimsyBGColor = ConfigurationManager.AppSettings["Slimsy:BGColor"];
                bgColor = slimsyBGColor != null && slimsyBGColor != "false" ? slimsyBGColor : string.Empty;
            }

            if (!string.IsNullOrEmpty(outputFormat))
            {
                var returnString = new StringBuilder();
                returnString.Append(string.Format("&format={0}", outputFormat));

                if (!string.IsNullOrEmpty(bgColor))
                {
                    returnString.Append(string.Format("&bgcolor={0}", bgColor));
                }

                return returnString.ToString();
            }

            return null;
        }
    }
}
