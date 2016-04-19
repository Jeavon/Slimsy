// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Slimsy.cs" company="Our.Umbraco">
//   2014
// </copyright>
// <summary>
//   Defines the Slimsy type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Slimsy
{
    using System;
    using System.Linq;

    using System.Configuration;
    using System.Text;

    using Newtonsoft.Json;

    using Umbraco.Core;
    using Umbraco.Core.Models;
    using Umbraco.Web;
    using Umbraco.Web.Models;

    [System.Runtime.InteropServices.Guid("38B09B03-3029-45E8-BC21-21C8CC8D4278")]
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
                    preferFocalPoint: true, 
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
                    preferFocalPoint: true, 
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

        public static string GetImgSrcSet(this IPublishedContent publishedContent, int width, int height)
        {
            return publishedContent.GetImgSrcSet(width, height, Constants.Conventions.Media.File);
        }

        public static string GetImgSrcSet(this IPublishedContent publishedContent, int width, int height, string propertyAlias)
        {
            return publishedContent.GetImgSrcSet(width, height, propertyAlias, null);
        }

        public static string GetImgSrcSet(this IPublishedContent publishedContent, int width, int height, string propertyAlias, string outputFormat)
        {
            var w = 160;
            const int MaxWidth = 2048;
            const int WidthStep = 160;

            var outputStringBuilder = new StringBuilder();
            var heightRatio = (decimal)height / (decimal)width;

            while (w <= MaxWidth)
            {
                var h = (int)Math.Round(w * heightRatio);
                outputStringBuilder.Append(string.Format("{0} {1}w,", publishedContent.GetCropUrl(w, h, propertyAlias, quality: 90, preferFocalPoint :true, furtherOptions: Format(outputFormat)), w));
                w += WidthStep;
            }

            // remove the last comma
            var outputString = outputStringBuilder.ToString().Substring(0, outputStringBuilder.Length - 1);

            return outputString;
        }

        public static string GetImgSrcSet(this IPublishedContent publishedContent, int width, int height, ImageCropMode? imageCropMode, string outputFormat)
        {
            var w = 160;
            const int MaxWidth = 2048;
            const int WidthStep = 160;

            var outputStringBuilder = new StringBuilder();
            var heightRatio = (decimal)height / (decimal)width;

            while (w <= MaxWidth)
            {
                var h = (int)Math.Round(w * heightRatio);
                outputStringBuilder.Append(string.Format("{0} {1}w,", publishedContent.GetCropUrl(w, h, imageCropMode: imageCropMode, quality: 90, preferFocalPoint: true, furtherOptions: Format(outputFormat)), w ));
                w += WidthStep;
            }

            // remove the last comma
            var outputString = outputStringBuilder.ToString().Substring(0, outputStringBuilder.Length - 1);

            return outputString;
        }

        public static string GetImgSrcSet(this IPublishedContent publishedContent, int width, int height, Dictionary<int, Tuple<int, int>> aspectRatio)
        {

            var w = 160;
            const int MaxWidth = 2048;
            const int WidthStep = 160;

            var outputStringBuilder = new StringBuilder();
            var heightRatio = (decimal)height / (decimal)width;

            while (w <= MaxWidth)
            {
                if (w < aspectRatio.Keys.FirstOrDefault())
                {
                    heightRatio = (decimal)aspectRatio.Values.FirstOrDefault().Item1/
                                  (decimal)aspectRatio.Values.FirstOrDefault().Item2;
                }
                else
                {
                    heightRatio = (decimal)height / (decimal)width;
                }
              
                var h = (int)Math.Round(w * heightRatio);
                outputStringBuilder.Append(string.Format("{0} {1}w,", publishedContent.GetCropUrl(w, h, quality: 90, preferFocalPoint: true, furtherOptions: Format()), w));
                    

                w += WidthStep; 
                
            }

            // remove the last comma
            var outputString = outputStringBuilder.ToString().Substring(0, outputStringBuilder.Length - 1);

            return outputString;
        }

        public static string GetImgSrcSet(this IPublishedContent publishedContent, int width, int height, ImageCropMode? imageCropMode)
        {
            var w = 160;
            const int MaxWidth = 2048;
            const int WidthStep = 160;

            var outputStringBuilder = new StringBuilder();
            var heightRatio = (decimal)height / (decimal)width;

            while (w <= MaxWidth)
            {
                var h = (int)Math.Round(w * heightRatio);
                outputStringBuilder.Append(string.Format("{0} {1}w,", publishedContent.GetCropUrl(w, h, imageCropMode: imageCropMode, quality: 90, preferFocalPoint: true, furtherOptions: Format()), w));
                w += WidthStep;
            }

            // remove the last comma
            var outputString = outputStringBuilder.ToString().Substring(0, outputStringBuilder.Length - 1);

            return outputString;
        }

        public static string GetCropSrcSet(this IPublishedContent publishedContent, string cropAlias)
        {
            return publishedContent.GetCropSrcSet(cropAlias, Constants.Conventions.Media.File);
        }

        public static string GetCropSrcSet(this IPublishedContent publishedContent, string cropAlias, string propertyAlias)
        {
            return publishedContent.GetCropSrcSet(cropAlias, propertyAlias, null);
        }

        public static string GetCropSrcSet(this IPublishedContent publishedContent, string cropAlias, string propertyAlias, string outputFormat)
        {
            var w = 160;
            const int MaxWidth = 2048;
            const int WidthStep = 160;

            var outputStringBuilder = new StringBuilder();

            var cropperJson = publishedContent.GetPropertyValue<string>(propertyAlias);
            var imageCrops = JsonConvert.DeserializeObject<ImageCropDataSet>(cropperJson);

            var crop = imageCrops.Crops.FirstOrDefault(
                x => x.Alias.ToLowerInvariant() == cropAlias.ToLowerInvariant());

            if (crop != null)
            {
                var heightRatio = (decimal)crop.Height / (decimal)crop.Width;

                while (w <= MaxWidth)
                {
                    var h = (int)Math.Round(w * heightRatio);
                    outputStringBuilder.Append(
                        string.Format(
                            "{0} {1}w,",
                            publishedContent.GetCropUrl(w, h, propertyAlias, cropAlias, quality: 90, furtherOptions: Format(outputFormat)),
                            w));

                    outputStringBuilder.Append(
                       string.Format(
                           "{0} {1}w 2x,",
                           publishedContent.GetCropUrl(w * 2, h * 2, propertyAlias, cropAlias, quality: 90, furtherOptions: Format(outputFormat)),
                           w));
                    w += WidthStep;
                }

            }
            else
            {
                // return focal point?
            }

            // remove the last comma
            var outputString = outputStringBuilder.ToString().Substring(0, outputStringBuilder.Length - 1);

            return outputString;
        }


        private static string Format(string outputFormat = null)
        {
            var bgColor = string.Empty;
            if (outputFormat == null)
            {
                var slimsyFormat = ConfigurationManager.AppSettings["Slimsy:Format"];
                outputFormat = slimsyFormat != "false" ? (slimsyFormat ?? "jpg") : string.Empty;
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

            if (!string.IsNullOrEmpty(bgColor))
            {
                return string.Format("&bgcolor={0}", bgColor);
            }

            return null;
        }


    }
}
