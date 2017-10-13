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
        #region SrcSet
        /// <summary>
        /// Generate SrcSet markup based on a width and height for the image
        /// </summary>
        /// <param name="publishedContent"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns>HTML Markup</returns>
        public static string GetImgSrcSet(this IPublishedContent publishedContent, int width, int height)
        {
            return publishedContent.GetImgSrcSet(width, height, Constants.Conventions.Media.File);
        }

        /// <summary>
        /// Generate SrcSet markup based on a width and height for the image with a quality setting
        /// </summary>
        /// <param name="publishedContent"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="quality"></param>
        /// <returns>HTML Markup</returns>
        public static string GetImgSrcSet(this IPublishedContent publishedContent, int width, int height, int quality)
        {
            return publishedContent.GetImgSrcSet(width, height, Constants.Conventions.Media.File, null, quality);
        }

        public static string GetImgSrcSet(this IPublishedContent publishedContent, int width, int height, string propertyAlias)
        {
            return publishedContent.GetImgSrcSet(width, height, propertyAlias, null);
        }
        
        public static string GetImgSrcSet(this IPublishedContent publishedContent, int width, int height, string propertyAlias, string outputFormat, int quality = 90)
        {
            var w = 160;
            const int MaxWidth = 2048;
            const int WidthStep = 160;

            var outputStringBuilder = new StringBuilder();
            var heightRatio = (decimal)height / (decimal)width;



            while (w <= MaxWidth)
            {
                var h = (int)Math.Round(w * heightRatio);
                var cropString = publishedContent.GetCropUrl(w, h, propertyAlias, quality: 90, preferFocalPoint: true,
                    furtherOptions: Format(outputFormat));

                var strPos = cropString.IndexOf("&quality=90", StringComparison.Ordinal);
                var fixedCropUrl = strPos != -1 ? cropString.Remove(strPos, 11) : cropString;

                outputStringBuilder.Append($"{fixedCropUrl}&quality={quality} {w}w,");
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
                outputStringBuilder.Append(
                    $"{publishedContent.GetCropUrl(w, h, imageCropMode: imageCropMode, quality: 90, preferFocalPoint: true, furtherOptions: Format(outputFormat))} {w}w,");
                w += WidthStep;
            }

            // remove the last comma
            var outputString = outputStringBuilder.ToString().Substring(0, outputStringBuilder.Length - 1);

            return outputString;
        }

        /// <summary>
        /// Generate SrcSet markup based on a width and height for the image passing in a ratio for the image
        /// </summary>
        /// <param name="publishedContent"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="aspectRatio"></param>
        /// <returns>HTML Markup</returns>
        public static string GetImgSrcSet(this IPublishedContent publishedContent, int width, int height, AspectRatio aspectRatio)
        {

            var w = 160;
            const int MaxWidth = 2048;
            const int WidthStep = 160;

            var outputStringBuilder = new StringBuilder();
            var heightRatio = (decimal)height / (decimal)width;

            while (w <= MaxWidth)
            {
                if (w < width)
                {
                    heightRatio = (decimal)aspectRatio.Height /
                                  (decimal)aspectRatio.Width;
                }
                else
                {
                    heightRatio = (decimal)height / (decimal)width;
                }

                var h = (int)Math.Round(w * heightRatio);

                outputStringBuilder.Append(
                    $"{publishedContent.GetCropUrl(w, h, quality: 90, preferFocalPoint: true, furtherOptions: Format())} {w}w,");

                w += WidthStep;
            }

            // remove the last comma
            var outputString = outputStringBuilder.ToString().Substring(0, outputStringBuilder.Length - 1);

            return outputString;
        }

        /// <summary>
        /// Generate SrcSet markup based on a width and height for the image with a crop mode e.g Padded etc
        /// </summary>
        /// <param name="publishedContent"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="imageCropMode"></param>
        /// <returns>HTML Markup</returns>
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
                outputStringBuilder.Append(
                    $"{publishedContent.GetCropUrl(w, h, imageCropMode: imageCropMode, quality: 90, preferFocalPoint: true, furtherOptions: Format())} {w}w,");
                w += WidthStep;
            }

            // remove the last comma
            var outputString = outputStringBuilder.ToString().Substring(0, outputStringBuilder.Length - 1);

            return outputString;
        }

        /// <summary>
        /// Get SrcSet based on a predefined crop
        /// </summary>
        /// <param name="publishedContent"></param>
        /// <param name="cropAlias"></param>
        /// <returns>HTML Markup</returns>
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
                x => string.Equals(x.Alias, cropAlias, StringComparison.InvariantCultureIgnoreCase));

            if (crop != null)
            {
                var heightRatio = (decimal)crop.Height / (decimal)crop.Width;

                while (w <= MaxWidth)
                {
                    var h = (int)Math.Round(w * heightRatio);
                    outputStringBuilder.Append(
                        $"{publishedContent.GetCropUrl(w, h, propertyAlias, cropAlias, quality: 90, furtherOptions: Format(outputFormat))} {w}w,");

                    w += WidthStep;
                }

            }

            // remove the last comma
            var outputString = outputStringBuilder.ToString().Substring(0, outputStringBuilder.Length - 1);

            return outputString;
        }
        #endregion

        #region Internal Functions
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
                returnString.Append($"&format={outputFormat}");

                if (!string.IsNullOrEmpty(bgColor))
                {
                    returnString.Append($"&bgcolor={bgColor}");
                }

                return returnString.ToString();
            }

            if (!string.IsNullOrEmpty(bgColor))
            {
                return $"&bgcolor={bgColor}";
            }

            return null;
        }
        #endregion
    }
}
