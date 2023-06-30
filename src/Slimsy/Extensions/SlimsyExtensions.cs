namespace Slimsy.Extensions
{
    using Microsoft.AspNetCore.Html;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.Extensions.DependencyInjection;
    using Slimsy.Models;
    using Slimsy.Services;
    using Umbraco.Cms.Core;
    using Umbraco.Cms.Core.Models;
    using Umbraco.Cms.Core.Models.PublishedContent;
    using Umbraco.Cms.Core.Strings;
    using Umbraco.Cms.Web.Common.DependencyInjection;

    public static class SlimsyExtensions
    {
        private static SlimsyService SlimsyService { get; } = StaticServiceProvider.Instance.GetRequiredService<SlimsyService>();

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
        public static IHtmlContent GetSrcSetUrls(this IUrlHelper urlHelper, IPublishedContent publishedContent, int width, int height, string propertyAlias = Constants.Conventions.Media.File, int quality = 90, string outputFormat = "", string furtherOptions = "")
        {
            return SlimsyService.GetSrcSetUrls(publishedContent, width, height, propertyAlias, quality, outputFormat, furtherOptions);
        }

        /// <summary>
        /// Generate SrcSet markup based on a width and height for the image cropped around the focal point
        /// </summary>
        /// <param name="urlHelper"></param>
        /// <param name="mediaWithCrops"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="propertyAlias"></param>
        /// <param name="quality">Default is 90</param>
        /// <param name="outputFormat"></param>
        /// <param name="furtherOptions"></param>
        /// <returns>Url</returns>
        public static IHtmlContent GetSrcSetUrls(this IUrlHelper urlHelper, MediaWithCrops mediaWithCrops, int width, int height, string propertyAlias = Constants.Conventions.Media.File, int quality = 90, string? outputFormat = "", string? furtherOptions = "")
        {
            return SlimsyService.GetSrcSetUrls(mediaWithCrops, width, height, propertyAlias, quality, outputFormat, furtherOptions);
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
        public static IHtmlContent GetSrcSetUrls(this IUrlHelper urlHelper, IPublishedContent publishedContent, int width, int height, ImageCropMode imageCropMode, string propertyAlias = Constants.Conventions.Media.File, ImageCropAnchor imageCropAnchor = ImageCropAnchor.Center, int quality = 90, string outputFormat = "", string furtherOptions = "")
        {
            return SlimsyService.GetSrcSetUrls(publishedContent, width, height, imageCropMode, imageCropAnchor,
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
        public static IHtmlContent GetSrcSetUrls(this IUrlHelper urlHelper, IPublishedContent publishedContent, AspectRatio aspectRatio, string propertyAlias = Constants.Conventions.Media.File, int quality = 90, string outputFormat = "", string furtherOptions = "")
        {
            return SlimsyService.GetSrcSetUrls(publishedContent, aspectRatio, propertyAlias, quality, outputFormat, furtherOptions);
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
        public static IHtmlContent GetSrcSetUrls(this IUrlHelper urlHelper, IPublishedContent publishedContent, string cropAlias, string propertyAlias = Constants.Conventions.Media.File, int quality = 90, string outputFormat = "", string furtherOptions = "")
        {
            return SlimsyService.GetSrcSetUrls(publishedContent, cropAlias, propertyAlias, quality, outputFormat, furtherOptions);
        }

        public static IHtmlContent GetSrcSetUrls(this IUrlHelper urlHelper, MediaWithCrops mediaWithCrops, string cropAlias, string propertyAlias = Constants.Conventions.Media.File, int quality = 90, string outputFormat = "", string furtherOptions = "")
        {
            return SlimsyService.GetSrcSetUrls(mediaWithCrops, cropAlias, propertyAlias, quality, outputFormat, furtherOptions);
        }

        #endregion

        #region Html Helpers

        /// <summary>
        /// Convert img to img srcset, extracts width and height from querystrings
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="sourceValueHtml">This html value should be the source value from an Umbraco property or a raw grid RTE value</param>
        /// <param name="generateLqip"></param>
        /// <param name="removeStyleAttribute">If you don't want the inline style attribute added by TinyMce to render</param>
        /// <param name="renderPicture"></param>
        /// <param name="pictureSources"></param>
        /// <returns>HTML Markup</returns>
        public static IHtmlEncodedString ConvertImgToResponsive(this IHtmlHelper htmlHelper, string sourceValueHtml, bool generateLqip = true, bool removeStyleAttribute = true, bool renderPicture = false, string[]? pictureSources = null)
        {
            return SlimsyService.ConvertImgToResponsive(sourceValueHtml, generateLqip, removeStyleAttribute, renderPicture, pictureSources);
        }

        /// <summary>
        /// Convert img to img srcset, extracts width and height from querystrings
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="publishedContent"></param>
        /// <param name="propertyAlias">Alias of the TinyMce property</param>
        /// <param name="generateLqip">Set to true if you want LQIP markup to be generated</param>
        /// <param name="removeStyleAttribute">If you don't want the inline style attribute added by TinyMce to render</param>
        /// <param name="renderPicture"></param>
        /// <param name="pictureSources"></param>
        /// <returns>HTML Markup</returns>
        public static IHtmlEncodedString ConvertImgToResponsive(this IHtmlHelper htmlHelper, IPublishedContent publishedContent, string propertyAlias, bool generateLqip = true, bool removeStyleAttribute = true, bool renderPicture = false, string[]? pictureSources = null)
        {
            return SlimsyService.ConvertImgToResponsive(publishedContent, propertyAlias, generateLqip, removeStyleAttribute, renderPicture, pictureSources);
        }


        /// <summary>
        /// Convert img to img srcset, extracts width and height from querystrings
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="publishedContent"></param>
        /// <param name="propertyAlias">Alias of the TinyMce property</param>
        /// <param name="generateLqip">Set to true if you want LQIP markup to be generated</param>
        /// <param name="removeStyleAttribute">If you don't want the inline style attribute added by TinyMce to render</param>
        /// <param name="renderPicture"></param>
        /// <param name="pictureSources"></param>
        /// <returns>HTML Markup</returns>
        public static IHtmlEncodedString? ConvertImgToResponsive(this IHtmlHelper htmlHelper, PublishedContentWrapped publishedContent, string propertyAlias, bool generateLqip = true, bool removeStyleAttribute = true, bool renderPicture = false, string[]? pictureSources = null)
        {
            return htmlHelper.ConvertImgToResponsive(publishedContent.Unwrap(), propertyAlias, generateLqip, removeStyleAttribute, renderPicture, pictureSources);
        }

        /// <summary>
        /// Convert img to img srcset, extracts width and height from querystrings
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="publishedElement"></param>
        /// <param name="propertyAlias">Alias of the TinyMce property</param>
        /// <param name="generateLqip">Set to true if you want LQIP markup to be generated</param>
        /// <param name="removeStyleAttribute">If you don't want the inline style attribute added by TinyMce to render</param>
        /// <param name="renderPicture"></param>
        /// <param name="pictureSources"></param>
        /// <returns>HTML Markup</returns>
        public static IHtmlEncodedString? ConvertImgToResponsive(this IHtmlHelper htmlHelper, IPublishedElement publishedElement, string propertyAlias, bool generateLqip = true, bool removeStyleAttribute = true, bool renderPicture = false, string[]? pictureSources = null)
        {
            return SlimsyService.ConvertImgToResponsive(publishedElement, propertyAlias, generateLqip, removeStyleAttribute, renderPicture, pictureSources);
        }

        /// <summary>
        /// Convert img to img srcset, extracts width and height from querystrings
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="publishedElement"></param>
        /// <param name="propertyAlias">Alias of the TinyMce property</param>
        /// <param name="generateLqip">Set to true if you want LQIP markup to be generated</param>
        /// <param name="removeStyleAttribute">If you don't want the inline style attribute added by TinyMce to render</param>
        /// <param name="renderPicture"></param>
        /// <param name="pictureSources"></param>
        /// <returns>HTML Markup</returns>
        public static IHtmlEncodedString? ConvertImgToResponsive(this IHtmlHelper htmlHelper, PublishedElementWrapped publishedElement, string propertyAlias, bool generateLqip = true, bool removeStyleAttribute = true, bool renderPicture = false, string[]? pictureSources = null)
        {
            return htmlHelper.ConvertImgToResponsive(publishedElement.Unwrap(), propertyAlias, generateLqip, removeStyleAttribute, renderPicture, pictureSources);
        }
        #endregion

    }
}
