// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SlimsyExtensions.cs" company="Our.Umbraco">
//   2017
// </copyright>
// <summary>
//   Defines the SlimsyExtension methods for Url and Html Helpers.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Slimsy.Extensions
{
    using Interfaces;
    using Models;
    using System.Web;
    using System.Web.Mvc;
    using Umbraco.Core;
    using Umbraco.Core.Models.PublishedContent;
    using Umbraco.Web.Models;
    using Constants = Umbraco.Core.Constants;
    using Current = Umbraco.Web.Composing.Current;

    public static class SlimsyExtensions
    {
        private static ISlimsyService _slimsyService;

        static SlimsyExtensions()
        {
            _slimsyService = Current.Factory.GetInstance<ISlimsyService>();
        }

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
            return _slimsyService.GetSrcSetUrls(publishedContent, width, height, Constants.Conventions.Media.File);
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
            return _slimsyService.GetSrcSetUrls(publishedContent, width, height, Constants.Conventions.Media.File, null, quality);
        }

        public static IHtmlString GetSrcSetUrls(this UrlHelper urlHelper, IPublishedContent publishedContent, int width, int height, string propertyAlias)
        {
            return _slimsyService.GetSrcSetUrls(publishedContent, width, height, propertyAlias, null);
        }
        
        public static IHtmlString GetSrcSetUrls(this UrlHelper urlHelper, IPublishedContent publishedContent, int width, int height, string propertyAlias, string outputFormat, int quality = 90)
        {
            return _slimsyService.GetSrcSetUrls(publishedContent, width, height, propertyAlias, outputFormat, quality);
        }

        public static IHtmlString GetSrcSetUrls(this UrlHelper urlHelper, IPublishedContent publishedContent, int width, int height, ImageCropMode? imageCropMode, string outputFormat = "")
        {
            return _slimsyService.GetSrcSetUrls(publishedContent, width, height, imageCropMode, outputFormat);
        }

        /// <summary>
        /// Generate SrcSet markup based on a width and height for the image passing in a ratio for the image
        /// </summary>
        /// <param name="urlHelper"></param>
        /// <param name="publishedContent"></param>
        /// <param name="aspectRatio"></param>
        /// <returns>HTML Markup</returns>
        public static IHtmlString GetSrcSetUrls(this UrlHelper urlHelper, IPublishedContent publishedContent, AspectRatio aspectRatio)
        {
            return _slimsyService.GetSrcSetUrls(publishedContent, aspectRatio);
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
            return _slimsyService.GetSrcSetUrls(publishedContent, cropAlias, Constants.Conventions.Media.File);
        }

        public static IHtmlString GetSrcSetUrls(this UrlHelper urlHelper, IPublishedContent publishedContent, string cropAlias, string propertyAlias)
        {
            return _slimsyService.GetSrcSetUrls(publishedContent, cropAlias, propertyAlias, null);
        }

        public static IHtmlString GetSrcSetUrls(this UrlHelper urlHelper, IPublishedContent publishedContent, string cropAlias, string propertyAlias, string outputFormat, int quality = 90)
        {
            return _slimsyService.GetSrcSetUrls(publishedContent, cropAlias, propertyAlias, outputFormat, quality);
        }
        #endregion

        #region Html Helpers

        /// <summary>
        /// Convert img to img srcset, extracts width and height from querystrings
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="sourceValueHtml">This html value should be the source value from and Umbraco property or a raw grid RTE value</param>
        /// <param name="generateLqip"></param>
        /// <param name="removeStyleAttribute">If you don't want the inline sytle attribute added by TinyMce to render</param>
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
        /// <param name="removeStyleAttribute">If you don't want the inline sytle attribute added by TinyMce to render</param>
        /// <returns>HTML Markup</returns>
        public static IHtmlString ConvertImgToSrcSet(this HtmlHelper htmlHelper, IPublishedContent publishedContent, string propertyAlias, bool generateLqip = true, bool removeStyleAttribute = true)
        {
            return _slimsyService.ConvertImgToSrcSet(publishedContent, propertyAlias, generateLqip, removeStyleAttribute);
        }

        #endregion
    }
}