namespace Slimsy.Services
{
    using Extensions;
    using HtmlAgilityPack;
    using Interfaces;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;
    using Newtonsoft.Json;
    using Umbraco.Core;
    using Umbraco.Core.Models.PublishedContent;
    using Umbraco.Core.PropertyEditors;
    using Umbraco.Core.PropertyEditors.ValueConverters;
    using Umbraco.Web;
    using Umbraco.Web.Models;
    using Umbraco.Web.PropertyEditors.ValueConverters;

    public class SlimsyService : ISlimsyService
    {
        private readonly ISlimsyOptions _slimsyOptions;
        private readonly RteMacroRenderingValueConverter _rteMacroRenderingValueConverter;
        private readonly UrlHelper _urlHelper;

        public SlimsyService(ISlimsyOptions slimsyOptions, RteMacroRenderingValueConverter rteMacroRenderingValueConverter)
        {
            this._slimsyOptions = slimsyOptions;
            this._rteMacroRenderingValueConverter = rteMacroRenderingValueConverter;
            this._urlHelper = new UrlHelper();
        }

        #region CropUrl

        public IHtmlString GetCropUrl(IPublishedContent mediaItem, string cropAlias, bool htmlEncode = true)
        {
            if (mediaItem == null) return new HtmlString(string.Empty);
            var url = mediaItem.GetCropUrl(cropAlias: cropAlias, useCropDimensions: true);
            return htmlEncode ? new HtmlString(HttpUtility.HtmlEncode(url)) : new HtmlString(url);
        }

        #endregion

        #region SrcSet

        /// <summary>
        /// Generate SrcSet markup based on a width and height for the image cropped around the focal point
        /// </summary>
        /// <param name="publishedContent"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns>HTML Markup</returns>
        public IHtmlString GetSrcSetUrls(IPublishedContent publishedContent, int width, int height)
        {
            return this.GetSrcSetUrls(publishedContent, width, height, Constants.Conventions.Media.File);
        }

        /// <summary>
        /// Generate SrcSet markup based on a width and height for the image cropped around the focal point and at a specific quality
        /// </summary>
        /// <param name="publishedContent"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="quality"></param>
        /// <returns>HTML Markup</returns>
        public IHtmlString GetSrcSetUrls(IPublishedContent publishedContent, int width, int height, int quality)
        {
            return this.GetSrcSetUrls(publishedContent, width, height, Constants.Conventions.Media.File, null, quality);
        }

        public IHtmlString GetSrcSetUrls(IPublishedContent publishedContent, int width, int height, string propertyAlias)
        {
            return this.GetSrcSetUrls(publishedContent, width, height, propertyAlias, null);
        }

        public IHtmlString GetSrcSetUrls(IPublishedContent publishedContent, int width, int height, string propertyAlias, string outputFormat, int quality = 90)
        {
            var w = this._slimsyOptions.WidthStep;
            var q = quality == 90 ? this._slimsyOptions.DefaultQuality : quality;

            var outputStringBuilder = new StringBuilder();
            var heightRatio = (decimal)height / width;

            while (w <= this.MaxWidth(publishedContent))
            {
                var h = (int)Math.Round(w * heightRatio);
                var cropString = this._urlHelper.GetCropUrl(publishedContent, w, h, propertyAlias, quality: q, preferFocalPoint: true,
                    furtherOptions: this.Format(outputFormat), htmlEncode: false).ToString();

                outputStringBuilder.Append($"{this._slimsyOptions.DomainPrefix}{cropString} {w}w,");
                w += this._slimsyOptions.WidthStep;
            }

            // remove the last comma
            var outputString = outputStringBuilder.ToString().Substring(0, outputStringBuilder.Length - 1);

            return new HtmlString(HttpUtility.HtmlEncode(outputString));
        }

        public IHtmlString GetSrcSetUrls(IPublishedContent publishedContent, int width, int height, ImageCropMode? imageCropMode, string outputFormat = "")
        {
            var w = this._slimsyOptions.WidthStep;
            var q = this._slimsyOptions.DefaultQuality;

            var outputStringBuilder = new StringBuilder();
            var heightRatio = (decimal)height / width;

            while (w <= this.MaxWidth(publishedContent))
            {
                var h = (int)Math.Round(w * heightRatio);
                outputStringBuilder.Append(
                    $"{this._slimsyOptions.DomainPrefix}{this._urlHelper.GetCropUrl(publishedContent, w, h, imageCropMode: imageCropMode, quality: q, preferFocalPoint: true, furtherOptions: this.Format(outputFormat), htmlEncode: false)} {w}w,");
                w += this._slimsyOptions.WidthStep;
            }

            // remove the last comma
            var outputString = outputStringBuilder.ToString().Substring(0, outputStringBuilder.Length - 1);

            return new HtmlString(HttpUtility.HtmlEncode(outputString));
        }

        /// <summary>
        /// Generate SrcSet markup based on a width and height for the image passing in a ratio for the image
        /// </summary>
        /// <param name="publishedContent"></param>
        /// <param name="aspectRatio"></param>
        /// <returns>HTML Markup</returns>
        public IHtmlString GetSrcSetUrls(IPublishedContent publishedContent, AspectRatio aspectRatio)
        {
            var w = this._slimsyOptions.WidthStep;
            var q = this._slimsyOptions.DefaultQuality;

            var outputStringBuilder = new StringBuilder();

            while (w <= this.MaxWidth(publishedContent))
            {
                var heightRatio = (decimal)aspectRatio.Height / aspectRatio.Width;

                var h = (int)Math.Round(w * heightRatio);

                outputStringBuilder.Append(
                    $"{this._slimsyOptions.DomainPrefix}{this._urlHelper.GetCropUrl(publishedContent, w, h, quality: q, preferFocalPoint: true, furtherOptions: this.Format(), htmlEncode: false)} {w}w,");

                w += this._slimsyOptions.WidthStep;
            }

            // remove the last comma
            var outputString = outputStringBuilder.ToString().Substring(0, outputStringBuilder.Length - 1);

            return new HtmlString(HttpUtility.HtmlEncode(outputString));
        }

        #endregion

        #region Pre defined crops

        public IHtmlString GetSrcSetUrls(IPublishedContent publishedContent, string cropAlias)
        {
            return this.GetSrcSetUrls(publishedContent, cropAlias, Constants.Conventions.Media.File);
        }

        public IHtmlString GetSrcSetUrls(IPublishedContent publishedContent, string cropAlias, string propertyAlias)
        {
            return this.GetSrcSetUrls(publishedContent, cropAlias, propertyAlias, null);
        }

        public IHtmlString GetSrcSetUrls(IPublishedContent publishedContent, string cropAlias, string propertyAlias, string outputFormat, int quality = 90)
        {
            var w = this._slimsyOptions.WidthStep;
            var q = quality == 90 ? this._slimsyOptions.DefaultQuality : quality;

            var outputStringBuilder = new StringBuilder();
            var outputString = string.Empty;

            var cropperJson = publishedContent.Value<string>(propertyAlias);
            var imageCrops = JsonConvert.DeserializeObject<ImageCropperValue>(cropperJson);
            var crop = imageCrops?.Crops?.FirstOrDefault(x => x.Alias.InvariantEquals(cropAlias));

            if (crop != null)
            {
                var heightRatio = (decimal)crop.Height / crop.Width;

                while (w <= this.MaxWidth(publishedContent))
                {
                    var h = (int)Math.Round(w * heightRatio);
                    outputStringBuilder.Append(
                        $"{this._slimsyOptions.DomainPrefix}{this._urlHelper.GetCropUrl(publishedContent, w, h, propertyAlias, cropAlias, q, furtherOptions: this.Format(outputFormat), htmlEncode: false)} {w}w,");
                    w += this._slimsyOptions.WidthStep;
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
                    return this._urlHelper.GetSrcSetUrls(publishedContent, cropConfiguration.Width,
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
        /// <param name="sourceValueHtml">This html value should be the source value from and Umbraco property or a raw grid RTE value</param>
        /// <param name="generateLqip"></param>
        /// <param name="removeStyleAttribute">If you don't want the inline sytle attribute added by TinyMce to render</param>
        /// <returns>HTML Markup</returns>
        public IHtmlString ConvertImgToSrcSet(string sourceValueHtml, bool generateLqip = true, bool removeStyleAttribute = true)
        {
            var source = this.ConvertImgToSrcSetInternal(sourceValueHtml, generateLqip, removeStyleAttribute);

            // We have the raw value so we need to run it through the value converter to ensure that links and macros are rendered
            var intermediateValue = this._rteMacroRenderingValueConverter.ConvertSourceToIntermediate(null, null, source, false);
            var objectValue = this._rteMacroRenderingValueConverter.ConvertIntermediateToObject(null, null, 0, intermediateValue, false);
            return objectValue as IHtmlString;
        }

        /// <summary>
        /// Convert img to img srcset, extracts width and height from querystrings
        /// </summary>
        /// <param name="publishedContent"></param>
        /// <param name="propertyAlias">Alias of the TinyMce property</param>
        /// <param name="generateLqip">Set to true if you want LQIP markup to be generated</param>
        /// <param name="removeStyleAttribute">If you don't want the inline sytle attribute added by TinyMce to render</param>
        /// <returns>HTML Markup</returns>
        public IHtmlString ConvertImgToSrcSet(IPublishedContent publishedContent, string propertyAlias, bool generateLqip = true, bool removeStyleAttribute = true)
        {
            var sourceValue = publishedContent.GetProperty(propertyAlias).GetSourceValue();

            return sourceValue != null ?
                this.ConvertImgToSrcSet(sourceValue.ToString(), generateLqip, removeStyleAttribute) :
                new HtmlString(string.Empty);
        }

        #endregion

        #region Internal Functions

        private IPublishedContent GetAnyTypePublishedContent(GuidUdi guidUdi)
        {
            switch (guidUdi.EntityType)
            {
                case Constants.UdiEntityType.Media:
                    return Umbraco.Web.Composing.Current.UmbracoContext.Media.GetById(guidUdi.Guid);

                case Constants.UdiEntityType.Document:
                    return Umbraco.Web.Composing.Current.UmbracoContext.Content.GetById(guidUdi.Guid);

                default:
                    return null;
            }
        }

        private int MaxWidth(IPublishedContent publishedContent)
        {
            var maxWidth = this._slimsyOptions.MaxWidth;

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
                if (maxWidth < this._slimsyOptions.WidthStep)
                {
                    maxWidth = this._slimsyOptions.WidthStep;
                }
            }

            return maxWidth;
        }

        private string Format(string outputFormat = null)
        {
            var bgColor = string.Empty;
            if (outputFormat == null)
            {
                var slimsyFormat = this._slimsyOptions.Format;
                outputFormat = slimsyFormat ?? "auto";

                var slimsyBgColor = this._slimsyOptions.BackgroundColor;
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

        /// <summary>
        /// Convert img to img srcset, extracts width and height from querystrings
        /// </summary>
        /// <param name="html"></param>
        /// <param name="generateLqip"></param>
        /// <param name="removeStyleAttribute">If you don't want the inline sytle attribute added by TinyMce to render</param>
        /// <param name="removeUdiAttribute">If you don't want the inline data-udi attribute to render</param>
        /// <param name="roundWidthHeight">Round width & height values as sometimes TinyMce adds decimal points</param>
        /// <returns>HTML Markup</returns>
        private IHtmlString ConvertImgToSrcSetInternal(string html, bool generateLqip = true, bool removeStyleAttribute = false, bool removeUdiAttribute = true, bool roundWidthHeight = true)
        {
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
                                    if (GuidUdi.TryParse(udiAttr.Value, out var guidUdi))
                                    {
                                        var node = this.GetAnyTypePublishedContent(guidUdi);

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
                                                    var roundedUrl = this._urlHelper.GetCropUrl(node, width, height,
                                                        imageCropMode: ImageCropMode.Pad, preferFocalPoint: true);
                                                    srcAttr.Value = roundedUrl.ToString();
                                                }

                                                var srcSet = this.GetSrcSetUrls(node, width, height);

                                                img.Attributes.Add("data-srcset", srcSet.ToString());
                                                img.Attributes.Add("data-sizes", "auto");

                                                if (generateLqip)
                                                {
                                                    var imgLqip = this._urlHelper.GetCropUrl(node, width, height, quality: 30,
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
    }
}
