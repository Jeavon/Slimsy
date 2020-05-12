// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SlimsyExtensions.cs" company="Our.Umbraco">
//   2017
// </copyright>
// <summary>
//   Defines the SlimsyExtension methods for Url and Html Helpers.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Slimsy
{
    using System.Web;
    using System.Web.Mvc;
    using Umbraco.Core;
    using Umbraco.Core.Models.PublishedContent;
    using Umbraco.Web.Models;
    using Current = Umbraco.Web.Composing.Current;

    public static class SlimsyExtensions
    {
        private static readonly SlimsyService _slimsyService;

        static SlimsyExtensions()
        {
            _slimsyService = Current.Factory.GetInstance<SlimsyService>();
        }

        #region Generate Crop

        /// <summary>
        /// Generate SrcSet markup based on a width and height for the image cropped around the focal point
        /// </summary>
        /// <param name="urlHelper"></param>
        /// <param name="publishedContent"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="propertyAlias"></param>
        /// <param name="quality">Default is 90</param>
        /// <param name="outputFormat"></param>
        /// <param name="furtherOptions"></param>
        /// <returns>Url</returns>
        public static IHtmlString GetSrcSetUrls(this UrlHelper urlHelper, IPublishedContent publishedContent, int width, int height, string propertyAlias = Constants.Conventions.Media.File, int quality = 90, string outputFormat = "", string furtherOptions = "")
        {
            return _slimsyService.GetSrcSetUrls(publishedContent, width, height, propertyAlias, quality, outputFormat, furtherOptions);
        }

        /// <summary>
        /// Generate SrcSet attribute value based on a width and height for the image cropped using a specific mode and using a specific image cropper property alias, output format and optional quality
        /// </summary>
        /// <param name="urlHelper"></param>
        /// <param name="publishedContent"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="propertyAlias"></param>
        /// <param name="imageCropMode"></param>
        /// <param name="imageCropAnchor"></param>
        /// <param name="quality">Default is 90</param>
        /// <param name="outputFormat"></param>
        /// <param name="furtherOptions"></param>
        /// <returns>Url of image</returns>
        public static IHtmlString GetSrcSetUrls(this UrlHelper urlHelper, IPublishedContent publishedContent, int width, int height, ImageCropMode imageCropMode, string propertyAlias = Constants.Conventions.Media.File, ImageCropAnchor imageCropAnchor = ImageCropAnchor.Center, int quality = 90, string outputFormat = "", string furtherOptions = "")
        {
            return _slimsyService.GetSrcSetUrls(publishedContent, width, height, imageCropMode, imageCropAnchor,
                propertyAlias, quality, outputFormat, furtherOptions);
        }

        /// <summary>
        /// Generate SrcSet markup based on a width and height for the image passing in a ratio for the image
        /// </summary>
        /// <param name="urlHelper"></param>
        /// <param name="publishedContent"></param>
        /// <param name="aspectRatio"></param>
        /// <param name="propertyAlias"></param>
        /// <param name="quality">Default is 90</param>
        /// <param name="outputFormat"></param>
        /// <param name="furtherOptions"></param>
        /// <returns>HTML Markup</returns>
        public static IHtmlString GetSrcSetUrls(this UrlHelper urlHelper, IPublishedContent publishedContent, AspectRatio aspectRatio, string propertyAlias = Constants.Conventions.Media.File, int quality = 90, string outputFormat = "", string furtherOptions = "")
        {
            return _slimsyService.GetSrcSetUrls(publishedContent, aspectRatio, propertyAlias, quality, outputFormat, furtherOptions);
        }

        #endregion

        #region Pre defined crops
        /// <summary>
        /// Get SrcSet based on a predefined crop
        /// </summary>
        /// <param name="urlHelper"></param>
        /// <param name="publishedContent"></param>
        /// <param name="cropAlias"></param>
        /// <param name="propertyAlias"></param>
        /// <param name="quality"></param>
        /// <param name="outputFormat"></param>
        /// <param name="furtherOptions"></param>
        /// <returns>HTML Markup</returns>
        public static IHtmlString GetSrcSetUrls(this UrlHelper urlHelper, IPublishedContent publishedContent, string cropAlias, string propertyAlias = Constants.Conventions.Media.File, int quality = 90, string outputFormat="", string furtherOptions = "")
        {
            return _slimsyService.GetSrcSetUrls(publishedContent, cropAlias, propertyAlias, quality, outputFormat, furtherOptions);
        }
        #endregion

        #region Html Helpers

        /// <summary>
        /// Convert img to img srcset, extracts width and height from querystrings
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="sourceValueHtml">This html value should be the source value from and Umbraco property or a raw grid RTE value</param>
        /// <param name="generateLqip"></param>
        /// <param name="removeStyleAttribute">If you don't want the inline style attribute added by TinyMce to render</param>
        /// <returns>HTML Markup</returns>
        public static IHtmlString ConvertImgToSrcSet(this HtmlHelper htmlHelper, string sourceValueHtml, bool generateLqip = true, bool removeStyleAttribute = true)
        {
            return _slimsyService.ConvertImgToSrcSet(sourceValueHtml, generateLqip, removeStyleAttribute);
        }

        /// <summary>
        /// Convert img to img srcset, extracts width and height from querystrings
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="publishedContent"></param>
        /// <param name="propertyAlias">Alias of the TinyMce property</param>
        /// <param name="generateLqip">Set to true if you want LQIP markup to be generated</param>
        /// <param name="removeStyleAttribute">If you don't want the inline style attribute added by TinyMce to render</param>
        /// <returns>HTML Markup</returns>
        public static IHtmlString ConvertImgToSrcSet(this HtmlHelper htmlHelper, IPublishedContent publishedContent, string propertyAlias, bool generateLqip = true, bool removeStyleAttribute = true)
        {
            return _slimsyService.ConvertImgToSrcSet(publishedContent, propertyAlias, generateLqip, removeStyleAttribute);
        }

        #endregion

        #region GetCropUrl

        public static IHtmlString GetCropUrl(IPublishedContent mediaItem, string cropAlias,
            bool htmlEncode = true)
        {
            return _slimsyService.GetCropUrl(mediaItem, cropAlias, htmlEncode);
        }

        public static IHtmlString GetCropUrl(IPublishedContent mediaItem, string propertyAlias,
            string cropAlias, bool htmlEncode = true)
        {
            return _slimsyService.GetCropUrl(mediaItem, propertyAlias, cropAlias, htmlEncode);
        }

        public static IHtmlString GetCropUrl(IPublishedContent mediaItem,
            int? width = null,
            int? height = null,
            string propertyAlias = Umbraco.Core.Constants.Conventions.Media.File,
            string cropAlias = null,
            int? quality = null,
            ImageCropMode? imageCropMode = null,
            ImageCropAnchor? imageCropAnchor = null,
            bool preferFocalPoint = false,
            bool useCropDimensions = false,
            bool cacheBuster = true,
            string furtherOptions = null,
            ImageCropRatioMode? ratioMode = null,
            bool upScale = true,
            bool htmlEncode = true)
        {
            return _slimsyService.GetCropUrl(mediaItem, width, height, propertyAlias, cropAlias, quality, imageCropMode,
                imageCropAnchor, preferFocalPoint, useCropDimensions, cacheBuster, furtherOptions, ratioMode, upScale,
                htmlEncode);
        }

        #endregion
    }
}