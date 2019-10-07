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
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;

    using HtmlAgilityPack;
    using Newtonsoft.Json;
    
    using Umbraco.Core;
    using Umbraco.Core.Cache;
    using Umbraco.Web;
    using Umbraco.Web.Models;
    using Umbraco.Web.PropertyEditors.ValueConverters;
    using Umbraco.Core.PropertyEditors;
    using Umbraco.Core.Models.PublishedContent;
    using Umbraco.Core.PropertyEditors.ValueConverters;
    using Umbraco.Web.Macros;
    using Constants = Umbraco.Core.Constants;
    using Current = Umbraco.Web.Composing.Current;

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
            var q = quality == 90 ? DefaultQuality() : quality;

            var outputStringBuilder = new StringBuilder();
            var heightRatio = (decimal)height / width;

            while (w <= MaxWidth(publishedContent))
            {
                var h = (int)Math.Round(w * heightRatio);
                var cropString = urlHelper.GetCropUrl(publishedContent, w, h, propertyAlias, quality: q, preferFocalPoint: true,
                    furtherOptions: Format(outputFormat), htmlEncode:false).ToString();

                outputStringBuilder.Append($"{cropString} {w}w,");
                w += WidthStep();
            }

            // remove the last comma
            var outputString = outputStringBuilder.ToString().Substring(0, outputStringBuilder.Length - 1);

            return new HtmlString(HttpUtility.HtmlEncode(outputString));
        }

        public static IHtmlString GetSrcSetUrls(this UrlHelper urlHelper, IPublishedContent publishedContent, int width, int height, ImageCropMode? imageCropMode, string outputFormat = "")
        {
            var w = WidthStep();
            var q = DefaultQuality();

            var outputStringBuilder = new StringBuilder();
            var heightRatio = (decimal)height / width;

            while (w <= MaxWidth(publishedContent))
            {
                var h = (int)Math.Round(w * heightRatio);
                outputStringBuilder.Append(
                    $"{urlHelper.GetCropUrl(publishedContent, w, h, imageCropMode: imageCropMode, quality: q, preferFocalPoint: true, furtherOptions: Format(outputFormat), htmlEncode: false)} {w}w,");
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
        /// <param name="aspectRatio"></param>
        /// <returns>HTML Markup</returns>
        public static IHtmlString GetSrcSetUrls(this UrlHelper urlHelper, IPublishedContent publishedContent, AspectRatio aspectRatio)
        {
            var w = WidthStep();
            var q = DefaultQuality();

            var outputStringBuilder = new StringBuilder();

            while (w <= MaxWidth(publishedContent))
            {
                var heightRatio = (decimal)aspectRatio.Height / aspectRatio.Width;
              
                var h = (int)Math.Round(w * heightRatio);

                outputStringBuilder.Append(
                    $"{urlHelper.GetCropUrl(publishedContent, w, h, quality: q, preferFocalPoint: true, furtherOptions: Format(), htmlEncode: false)} {w}w,");

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

        public static IHtmlString GetSrcSetUrls(this UrlHelper urlHelper, IPublishedContent publishedContent, string cropAlias, string propertyAlias)
        {
            return urlHelper.GetSrcSetUrls(publishedContent, cropAlias, propertyAlias, null);
        }

        public static IHtmlString GetSrcSetUrls(this UrlHelper urlHelper, IPublishedContent publishedContent, string cropAlias, string propertyAlias, string outputFormat, int quality = 90)
        {
            var w = WidthStep();
            var q = quality == 90 ? DefaultQuality() : quality;

            var outputStringBuilder = new StringBuilder();
            var outputString = string.Empty;

            var cropperJson = publishedContent.Value<string>(propertyAlias);
            var imageCrops = JsonConvert.DeserializeObject<ImageCropperValue>(cropperJson);

            var crop = imageCrops?.Crops?.FirstOrDefault(
                x => string.Equals(x.Alias, cropAlias, StringComparison.InvariantCultureIgnoreCase));

            if (crop != null)
            {
                var heightRatio = (decimal)crop.Height / crop.Width;

                while (w <= MaxWidth(publishedContent))
                {
                    var h = (int)Math.Round(w * heightRatio);
                    outputStringBuilder.Append(
                        $"{urlHelper.GetCropUrl(publishedContent, w, h, propertyAlias, cropAlias, q, furtherOptions: Format(outputFormat), htmlEncode: false)} {w}w,");
                    w += WidthStep();
                }

                // remove the last comma
                outputString = outputStringBuilder.ToString().Substring(0, outputStringBuilder.Length - 1);
            }
            else
            {
                // this code would execute if a predefined crop has been added to the data type but this media item hasn't been re-saved
                var cropperConfiguration = (ImageCropperConfiguration)publishedContent.Properties.FirstOrDefault(x => x.Alias == propertyAlias)?.PropertyType.DataType.Configuration;
                var cropConfiguration = cropperConfiguration?.Crops.FirstOrDefault(c => c.Alias == cropAlias);
                if (cropConfiguration != null)
                {
                    // auto generate using focal point
                    return urlHelper.GetSrcSetUrls(publishedContent, cropConfiguration.Width,
                        cropConfiguration.Height, propertyAlias, outputFormat);
                }
            }

            return new HtmlString(HttpUtility.HtmlEncode(outputString));
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
            var source = ConvertImgToSrcSetInternal(sourceValueHtml, generateLqip, removeStyleAttribute);

            // We have the raw value so we need to run it through the value converter to ensure that links and macros are rendered
            var rteConverter = new RteMacroRenderingValueConverter(Current.UmbracoContextAccessor, Current.Factory.GetAllInstances<IMacroRenderer>().FirstOrDefault());
            var intermediateValue = rteConverter.ConvertSourceToIntermediate(null, null, source, false);
            var objectValue = rteConverter.ConvertIntermediateToObject(null, null, 0, intermediateValue, false);

            return objectValue as IHtmlString;
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
            var sourceValue = publishedContent.GetProperty(propertyAlias).GetSourceValue();
            if (sourceValue != null)
            {
                return ConvertImgToSrcSet(htmlHelper, sourceValue.ToString(), generateLqip);
            }

            return new HtmlString("");
        }

        /// <summary>
        /// Convert img to img srcset, extracts width and height from querystrings
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="html"></param>
        /// <param name="generateLqip"></param>
        /// <param name="removeStyleAttribute">If you don't want the inline sytle attribute added by TinyMce to render</param>
        /// <param name="removeUdiAttribute">If you don't want the inline data-udi attribute to render</param>
        /// <param name="roundWidthHeight">Round width & height values as sometimes TinyMce adds decimal points</param>
        /// <returns>HTML Markup</returns>
        private static IHtmlString ConvertImgToSrcSetInternal(string html, bool generateLqip = true, bool removeStyleAttribute = false, bool removeUdiAttribute = true, bool roundWidthHeight = true)
        {
            var urlHelper = new UrlHelper();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            if (!doc.ParseErrors.Any() && doc.DocumentNode != null)
            {
                // Find all images
                var imgNodes = doc.DocumentNode.SelectNodes("//img");

                if (imgNodes != null)
                {
                    var modified = false;

                    foreach (var img in imgNodes)
                    {
                        var srcAttr = img.Attributes.FirstOrDefault(x => x.Name == "src");
                        var udiAttr = img.Attributes.FirstOrDefault(x => x.Name == "data-udi");
                        var classAttr = img.Attributes.FirstOrDefault(x => x.Name == "class");

                        if (srcAttr != null)
                        {
                            // html decode the url as variables encoded in tinymce
                            var src = HttpUtility.HtmlDecode(srcAttr.Value);

                            var hasQueryString = src.InvariantContains("?");
                            NameValueCollection queryStringCollection;

                            if (hasQueryString)
                            {
                                queryStringCollection = HttpUtility.ParseQueryString(src.Substring(src.IndexOf('?')));

                                // ensure case of variables doesn't cause trouble
                                IDictionary<string, string> queryString = queryStringCollection.AllKeys.ToDictionary(k => k.ToLowerInvariant(), k => queryStringCollection[k]);

                                if (udiAttr != null)
                                {
                                    // Umbraco media
                                    GuidUdi guidUdi;
                                    if (GuidUdi.TryParse(udiAttr.Value, out guidUdi))
                                    {
                                        var node = GetAnyTypePublishedContent(guidUdi);

                                        var qsWidth = queryString["width"];
                                        var qsHeight = "0";
                                        if (queryString.ContainsKey("height"))
                                        {
                                            qsHeight = queryString["height"];
                                        }

                                        // TinyMce sometimes adds decimals to image resize commands, we need to fix those
                                        if (decimal.TryParse(qsWidth, out decimal decWidth) && decimal.TryParse(qsHeight, out decimal decHeight))
                                        {
                                            var width = (int)Math.Round(decWidth);
                                            var height = (int)Math.Round(decHeight);

                                            // if width is 0 (I don't know why it would be but it has been seen) then we can't do anything
                                            if (width > 0)
                                            {
                                                // change the src attribute to data-src
                                                srcAttr.Name = "data-src";
                                                if (roundWidthHeight)
                                                {
                                                    var roundedUrl = urlHelper.GetCropUrl(node, width, height,
                                                        imageCropMode: ImageCropMode.Pad, preferFocalPoint: true);
                                                    srcAttr.Value = roundedUrl.ToString();
                                                }

                                                var srcSet = GetSrcSetUrls(urlHelper, node, width, height);

                                                img.Attributes.Add("data-srcset", srcSet.ToString());
                                                img.Attributes.Add("data-sizes", "auto");

                                                if (generateLqip)
                                                {
                                                    var imgLqip =
                                                        urlHelper.GetCropUrl(node, width, height, quality: 30,
                                                            furtherOptions: "&format=auto", preferFocalPoint: true);
                                                    img.Attributes.Add("src", imgLqip.ToString());
                                                }

                                                if (classAttr != null)
                                                {
                                                    classAttr.Value = $"{classAttr.Value} lazyload";
                                                }
                                                else
                                                {
                                                    img.Attributes.Add("class", "lazyload");
                                                }

                                                if (removeStyleAttribute)
                                                {
                                                    img.Attributes.Remove("style");
                                                }

                                                if (removeUdiAttribute)
                                                {
                                                    img.Attributes.Remove("data-udi");
                                                }

                                                modified = true;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Image in TinyMce doesn't have a data-udi attribute
                            }
                        }
                    }

                    if (modified)
                    {
                        return new HtmlString(doc.DocumentNode.OuterHtml);
                    }
                }
            }
            return new HtmlString(html);
        }

        #endregion

        #region Internal Functions

        private static IPublishedContent GetAnyTypePublishedContent(GuidUdi guidUdi)
        {
            switch (guidUdi.EntityType)
            {
                case Constants.UdiEntityType.Media:
                    return Current.UmbracoContext.Media.GetById(guidUdi.Guid);
                    break;  
                case Constants.UdiEntityType.Document:
                    return Current.UmbracoContext.Content.GetById(guidUdi.Guid);
                    break;
                default:
                    return null;
            }
        }

        private static int DefaultQuality()
        {
            var slimsyDefaultQuality = ConfigurationManager.AppSettings["Slimsy:DefaultQuality"];
            if (!int.TryParse(slimsyDefaultQuality, out int defaultQuality))
            {
                defaultQuality = 90;
            }

            return defaultQuality;
        }

        private static int WidthStep()
        {
            var slimsyWidthStep = ConfigurationManager.AppSettings["Slimsy:WidthStep"];
            if (!int.TryParse(slimsyWidthStep, out int widthStep))
            {
                widthStep = 160;
            }

            return widthStep;
        }

        private static int MaxWidth(IPublishedContent publishedContent)
        {
            var slimsyMaxWidth = ConfigurationManager.AppSettings["Slimsy:MaxWidth"];
            if (!int.TryParse(slimsyMaxWidth, out int maxWidth))
            {
                maxWidth = 2048;
            }

            // if publishedContent is a media item we can see if we can get the source image width & height
            if (publishedContent.ItemType == PublishedItemType.Media)
            {
                var sourceWidth = publishedContent.Value<int>(Constants.Conventions.Media.Width);

                // if source width is less than max width then we should stop at source width
                if (sourceWidth < maxWidth)
                {
                    maxWidth = sourceWidth;
                }

                // if the source image is less than the step then max width should be the first step
                if (maxWidth < WidthStep())
                {
                    maxWidth = WidthStep();
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
                outputFormat = slimsyFormat ?? "auto";

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

        private static T GetLocalCacheItem<T>(string cacheKey)
        {
            var runtimeCache = Current.AppCaches.RuntimeCache;
            var cachedItem = runtimeCache.GetCacheItem<T>(cacheKey);
            return cachedItem;
        }

        private static void InsertLocalCacheItem<T>(string cacheKey, Func<T> getCacheItem)
        {
            var runtimeCache = Current.AppCaches.RuntimeCache;
            runtimeCache.InsertCacheItem<T>(cacheKey, getCacheItem);
        }

        #endregion
    }
}