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
        /// Generate SrcSet markup based on a width and height for the image cropped around the focal point
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
        /// Generate SrcSet markup based on a width and height for the image cropped around the focal point and at a specific quality
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
            var w = WidthStep();

            var outputStringBuilder = new StringBuilder();
            var heightRatio = (decimal)height / width;

            while (w <= MaxWidth(publishedContent))
            {
                var h = (int)Math.Round(w * heightRatio);
                var cropString = urlHelper.GetCropUrl(publishedContent, w, h, propertyAlias, quality: 90, preferFocalPoint: true,
                    furtherOptions: Format(outputFormat), htmlEncode:false).ToString();

                var strPos = cropString.IndexOf("&quality=90", StringComparison.Ordinal);
                var fixedCropUrl = strPos != -1 ? cropString.Remove(strPos, 11) : cropString;

                outputStringBuilder.Append($"{fixedCropUrl}&quality={quality} {w}w,");
                w += WidthStep();
            }

            // remove the last comma
            var outputString = outputStringBuilder.ToString().Substring(0, outputStringBuilder.Length - 1);

            return new HtmlString(HttpUtility.HtmlEncode(outputString));
        }

        public static IHtmlString GetSrcSetUrls(this UrlHelper urlHelper, IPublishedContent publishedContent, int width, int height, ImageCropMode? imageCropMode, string outputFormat = "")
        {
            var w = WidthStep();

            var outputStringBuilder = new StringBuilder();
            var heightRatio = (decimal)height / width;

            while (w <= MaxWidth(publishedContent))
            {
                var h = (int)Math.Round(w * heightRatio);
                outputStringBuilder.Append(
                    $"{urlHelper.GetCropUrl(publishedContent, w, h, imageCropMode: imageCropMode, quality: 90, preferFocalPoint: true, furtherOptions: Format(outputFormat), htmlEncode: false)} {w}w,");
                w += WidthStep();
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
            var w = WidthStep();

            var outputStringBuilder = new StringBuilder();

            while (w <= MaxWidth(publishedContent))
            {
                decimal heightRatio;
                if (w < width)
                {
                    heightRatio = (decimal)aspectRatio.Height / aspectRatio.Width;
                }
                else
                {
                    heightRatio = (decimal)height / width;
                }

                var h = (int)Math.Round(w * heightRatio);

                outputStringBuilder.Append(
                    $"{urlHelper.GetCropUrl(publishedContent, w, h, quality: 90, preferFocalPoint: true, furtherOptions: Format(), htmlEncode: false)} {w}w,");

                w += WidthStep();
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
            var w = WidthStep();

            var outputStringBuilder = new StringBuilder();

            var cropperJson = publishedContent.GetPropertyValue<string>(propertyAlias);
            var imageCrops = JsonConvert.DeserializeObject<ImageCropDataSet>(cropperJson);

            var crop = imageCrops.Crops.FirstOrDefault(
                x => string.Equals(x.Alias, cropAlias, StringComparison.InvariantCultureIgnoreCase));

            if (crop != null)
            {
                var heightRatio = (decimal)crop.Height / crop.Width;

                while (w <= MaxWidth(publishedContent))
                {
                    var h = (int)Math.Round(w * heightRatio);
                    outputStringBuilder.Append(
                        $"{urlHelper.GetCropUrl(publishedContent, w, h, propertyAlias, cropAlias, quality: 90, furtherOptions: Format(outputFormat), htmlEncode: false)} {w}w,");
                    w += WidthStep();
                }

            }

            // remove the last comma
            var outputString = outputStringBuilder.ToString().Substring(0, outputStringBuilder.Length - 1);

            return new HtmlString(HttpUtility.HtmlEncode(outputString));
        }
        #endregion

        #region Internal Functions

        private static int WidthStep()
        {
            // this should be overridable from an appsetting
            return 160;
        }

        private static int MaxWidth(IPublishedContent publishedContent)
        {
            // this should be overridable from an appsetting
            var maxWidth = 2048;

            // if publishedContent is a media item we can see if we can get the source image width & height
            if (publishedContent.ItemType == PublishedItemType.Media)
            {
                var sourceWidth = publishedContent.GetPropertyValue<int>(Constants.Conventions.Media.Width);

                // if source width is less than max width then we should stop at source width
                if (sourceWidth < maxWidth)
                {
                    maxWidth = sourceWidth;
                }
            }

            return maxWidth;
        }

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
