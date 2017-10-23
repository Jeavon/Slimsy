// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Slimsy.cs" company="Our.Umbraco">
//   2017
// </copyright>
// <summary>
//   Defines the Slimsy type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Slimsy
{
    using System;
    using System.Linq;
    using System.Configuration;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;

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
        /// <param name="urlHelper"></param>
        /// <param name="publishedContent"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns>HTML Markup</returns>
        public static IHtmlString GetSrcSetUrls(this UrlHelper urlHelper, IPublishedContent publishedContent, int width, int height)
        {
            return urlHelper.GetSrcSetUrls(publishedContent, width, height, Constants.Conventions.Media.File);
        }

        /// <summary>
        /// Generate SrcSet markup based on a width and height for the image with a quality setting
        /// </summary>
        /// <param name="urlHelper"></param>
        /// <param name="publishedContent"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="quality"></param>
        /// <returns>HTML Markup</returns>
        public static IHtmlString GetSrcSetUrls(this UrlHelper urlHelper, IPublishedContent publishedContent, int width, int height, int quality)
        {
            return urlHelper.GetSrcSetUrls(publishedContent, width, height, Constants.Conventions.Media.File, null, quality);
        }

        public static IHtmlString GetSrcSetUrls(this UrlHelper urlHelper, IPublishedContent publishedContent, int width, int height, string propertyAlias)
        {
            return urlHelper.GetSrcSetUrls(publishedContent, width, height, propertyAlias, null);
        }
        
        public static IHtmlString GetSrcSetUrls(this UrlHelper urlHelper, IPublishedContent publishedContent, int width, int height, string propertyAlias, string outputFormat, int quality = 90)
        {
            var w = 160;
            const int maxWidth = 2048;
            const int widthStep = 160;

            var outputStringBuilder = new StringBuilder();
            var heightRatio = (decimal)height / (decimal)width;



            while (w <= maxWidth)
            {
                var h = (int)Math.Round(w * heightRatio);
                var cropString = publishedContent.GetCropUrl(w, h, propertyAlias, quality: 90, preferFocalPoint: true,
                    furtherOptions: Format(outputFormat));

                var strPos = cropString.IndexOf("&quality=90", StringComparison.Ordinal);
                var fixedCropUrl = strPos != -1 ? cropString.Remove(strPos, 11) : cropString;

                outputStringBuilder.Append($"{fixedCropUrl}&quality={quality} {w}w,");
                w += widthStep;
            }

            // remove the last comma
            var outputString = outputStringBuilder.ToString().Substring(0, outputStringBuilder.Length - 1);

            return new HtmlString(HttpUtility.HtmlEncode(outputString));
        }

        public static IHtmlString GetSrcSetUrls(this UrlHelper urlHelper, IPublishedContent publishedContent, int width, int height, ImageCropMode? imageCropMode, string outputFormat)
        {
            var w = 160;
            const int maxWidth = 2048;
            const int widthStep = 160;

            var outputStringBuilder = new StringBuilder();
            var heightRatio = (decimal)height / (decimal)width;

            while (w <= maxWidth)
            {
                var h = (int)Math.Round(w * heightRatio);
                outputStringBuilder.Append(
                    $"{publishedContent.GetCropUrl(w, h, imageCropMode: imageCropMode, quality: 90, preferFocalPoint: true, furtherOptions: Format(outputFormat))} {w}w,");
                w += widthStep;
            }

            // remove the last comma
            var outputString = outputStringBuilder.ToString().Substring(0, outputStringBuilder.Length - 1);

            return new HtmlString(HttpUtility.HtmlEncode(outputString));
        }

        /// <summary>
        /// Generate SrcSet markup based on a width and height for the image passing in a ratio for the image
        /// </summary>
        /// <param name="urlHelper"></param>
        /// <param name="publishedContent"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="aspectRatio"></param>
        /// <returns>HTML Markup</returns>
        public static IHtmlString GetSrcSetUrls(this UrlHelper urlHelper, IPublishedContent publishedContent, int width, int height, AspectRatio aspectRatio)
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

            return new HtmlString(HttpUtility.HtmlEncode(outputString));
        }

        /// <summary>
        /// Generate SrcSet markup based on a width and height for the image with a crop mode e.g Padded etc
        /// </summary>
        /// <param name="urlHelper"></param>
        /// <param name="publishedContent"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="imageCropMode"></param>
        /// <returns>HTML Markup</returns>
        public static IHtmlString GetSrcSetUrls(this UrlHelper urlHelper, IPublishedContent publishedContent, int width, int height, ImageCropMode? imageCropMode)
        {
            var w = 160;
            const int maxWidth = 2048;
            const int widthStep = 160;

            var outputStringBuilder = new StringBuilder();
            var heightRatio = (decimal)height / (decimal)width;

            while (w <= maxWidth)
            {
                var h = (int)Math.Round(w * heightRatio);
                outputStringBuilder.Append(
                    $"{publishedContent.GetCropUrl(w, h, imageCropMode: imageCropMode, quality: 90, preferFocalPoint: true, furtherOptions: Format())} {w}w,");
                w += widthStep;
            }

            // remove the last comma
            var outputString = outputStringBuilder.ToString().Substring(0, outputStringBuilder.Length - 1);

            return new HtmlString(HttpUtility.HtmlEncode(outputString));
        }
        #endregion

        #region Pre defined crops
        /// <summary>
        /// Get SrcSet based on a predefined crop
        /// </summary>
        /// <param name="urlHelper"></param>
        /// <param name="publishedContent"></param>
        /// <param name="cropAlias"></param>
        /// <returns>HTML Markup</returns>
        public static IHtmlString GetSrcSetUrls(this UrlHelper urlHelper, IPublishedContent publishedContent, string cropAlias)
        {
            return urlHelper.GetSrcSetUrls(publishedContent, cropAlias, Constants.Conventions.Media.File);
        }

        public static IHtmlString GetSrcSetUrls (this UrlHelper urlHelper, IPublishedContent publishedContent, string cropAlias, string propertyAlias)
        {
            return urlHelper.GetSrcSetUrls(publishedContent, cropAlias, propertyAlias, null);
        }

        public static IHtmlString GetSrcSetUrls(this UrlHelper urlHelper, IPublishedContent publishedContent, string cropAlias, string propertyAlias, string outputFormat)
        {
            var w = 160;
            const int maxWidth = 2048;
            const int widthStep = 160;

            var outputStringBuilder = new StringBuilder();

            var cropperJson = publishedContent.GetPropertyValue<string>(propertyAlias);
            var imageCrops = JsonConvert.DeserializeObject<ImageCropDataSet>(cropperJson);

            var crop = imageCrops.Crops.FirstOrDefault(
                x => string.Equals(x.Alias, cropAlias, StringComparison.InvariantCultureIgnoreCase));

            if (crop != null)
            {
                var heightRatio = (decimal)crop.Height / (decimal)crop.Width;

                while (w <= maxWidth)
                {
                    var h = (int)Math.Round(w * heightRatio);
                    outputStringBuilder.Append(
                        $"{publishedContent.GetCropUrl(w, h, propertyAlias, cropAlias, quality: 90, furtherOptions: Format(outputFormat))} {w}w,");

                    w += widthStep;
                }

            }

            // remove the last comma
            var outputString = outputStringBuilder.ToString().Substring(0, outputStringBuilder.Length - 1);

            return new HtmlString(HttpUtility.HtmlEncode(outputString));
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
                var slimsyBgColor = ConfigurationManager.AppSettings["Slimsy:BGColor"];
                bgColor = slimsyBgColor != null && slimsyBgColor != "false" ? slimsyBgColor : string.Empty;
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
