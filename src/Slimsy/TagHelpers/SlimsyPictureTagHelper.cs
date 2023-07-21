using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using Slimsy.Configuration;
using Slimsy.Enums;
using Slimsy.Models;
using Slimsy.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Extensions;
using TagHelper = Microsoft.AspNetCore.Razor.TagHelpers.TagHelper;

namespace Slimsy
{
    public class SlimsyPictureTagHelper : TagHelper
    {
        public MediaWithCrops? MediaItem { get; set; }
        /// <summary>
        /// Crop Alias to use, when this attribute is passed, width and height parameters are ignored
        /// </summary>
        public string? CropAlias { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string? AltText { get; set; }
        public string? CssClass { get; set; }
        [Obsolete("This property is obsolete, Use DefaultPictureSources Options in appsettings.json instead.", false)]
        public bool RenderWebpAlternative { get; set; } = true;
        public bool RenderLQIP { get; set; } = true;
        public bool Decorative { get; set; } = false;
        public FetchPriority FetchPriority { get; set; } = FetchPriority.Auto;
        public string PropertyAlias { get; set; } = Umbraco.Cms.Core.Constants.Conventions.Media.File;
        private readonly SlimsyService _slimsyService;
        private readonly SlimsyOptions _slimsyOptions;

        public SlimsyPictureTagHelper(SlimsyService slimsyService, IOptionsMonitor<SlimsyOptions> slimsyOptions)
        {
            _slimsyService = slimsyService;
            _slimsyOptions = slimsyOptions.CurrentValue;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {

            List<PictureSource> pictureSources = _slimsyOptions.TagHelper.DefaultPictureSources.ToList();

            // supporting upgrades
            if (RenderWebpAlternative && !pictureSources.Select(s => s.Extension).Contains("webp"))
            {
                pictureSources.Add(new PictureSource(){Extension="webp", Quality=70});
            }

            CssClass = !string.IsNullOrEmpty(CssClass) ? $"lazyload {CssClass}" : "lazyload";

            if (MediaItem != null)
            {
                var umbracoExtension = MediaItem.Value<string>(Umbraco.Cms.Core.Constants.Conventions.Media.Extension);

                if (_slimsyOptions.TagHelper.SingleSources != null && _slimsyOptions.TagHelper.SingleSources.Contains(umbracoExtension))
                {
                    // empty the sources as this should render a single source
                    pictureSources = new List<PictureSource>();
                }

                var defaultFormat = umbracoExtension;
                string? defaultMimeType = SlimsyService.MimeType(umbracoExtension);

                if (defaultMimeType == null)
                {
                    // not supported format
                }

                int? lqipWidth;
                int? lqipHeight;

                IHtmlContent? imgSrcSet = null;
                IHtmlContent? imgSrc = null, imgLqip = null;

                List<SourceSet> sources = new();

                if (!string.IsNullOrEmpty(CropAlias))
                {
                    var globalImageCrops = MediaItem.Value<ImageCropperValue>(PropertyAlias);
                    var mergedImageCrops = globalImageCrops != null ? globalImageCrops.Merge(MediaItem.LocalCrops) : MediaItem.LocalCrops;

                    var crop = mergedImageCrops?.Crops?.FirstOrDefault(x => x.Alias.InvariantEquals(CropAlias));

                    if (crop != null) { 
                        lqipWidth = (int)Math.Round((decimal)crop.Width / 2);
                        lqipHeight = (int)Math.Round((decimal)crop.Height / 2);

                        imgSrc = _slimsyService.GetCropUrl(MediaItem, cropAlias: CropAlias, useCropDimensions: true, furtherOptions: "&format=" + defaultFormat);
                        
                        foreach (var source in pictureSources)
                        {
                            imgSrcSet = _slimsyService.GetSrcSetUrls(MediaItem, CropAlias, PropertyAlias, source.Quality, source.Extension);
                            imgLqip = _slimsyService.GetCropUrl(MediaItem, lqipWidth, lqipHeight, cropAlias: CropAlias, quality: 20, furtherOptions: "&format=" + source.Extension);
                            var newSource = new SourceSet() { Source = imgSrcSet, Lqip = imgLqip, Format = source.Extension };
                            sources.Add(newSource);
                        }

                        // native format not included in sources so we add it as the last option, it will use the Slimsy default quality
                        if (!pictureSources.Select(s => s.Extension).InvariantContains(defaultFormat))
                        {
                            imgSrcSet = _slimsyService.GetSrcSetUrls(MediaItem, CropAlias, PropertyAlias, outputFormat: defaultFormat);
                            // ** Using half width/height for LQIP to reduce filesize to a minimum, CSS must oversize the images **
                            imgLqip = _slimsyService.GetCropUrl(MediaItem, lqipWidth, lqipHeight, quality: 20, cropAlias: CropAlias, furtherOptions: "&format=" + defaultFormat);

                            var nativeSource = new SourceSet() { Source = imgSrcSet, Lqip = imgLqip, Format = defaultFormat };
                            sources.Add(nativeSource);
                        }

                    }
                } else
                {
                    lqipWidth = (int)Math.Round((decimal)Width / 2);
                    lqipHeight = (int)Math.Round((decimal)Height / 2);
                                                      
                    imgSrc = _slimsyService.GetCropUrl(MediaItem, Width, Height, furtherOptions: "&format=" + defaultFormat);

                    foreach (var source in pictureSources)
                    {
                        imgSrcSet = _slimsyService.GetSrcSetUrls(MediaItem, Width, Height, PropertyAlias, source.Quality, source.Extension);
                        imgLqip = _slimsyService.GetCropUrl(MediaItem, lqipWidth, lqipHeight, quality: 20, furtherOptions: "&format=" + source.Extension);
                        var newSource = new SourceSet() { Source = imgSrcSet, Lqip = imgLqip, Format = source.Extension };
                        sources.Add(newSource);
                    }

                    imgSrcSet = _slimsyService.GetSrcSetUrls(MediaItem, Width, Height, PropertyAlias, outputFormat: defaultFormat);
                    // ** Using half width/height for LQIP to reduce filesize to a minimum, CSS must oversize the images **
                    imgLqip = _slimsyService.GetCropUrl(MediaItem, lqipWidth, lqipHeight, quality: 20, furtherOptions: "&format=" + defaultFormat);

                    // native format not included in sources so we add it as the last option
                    if (!pictureSources.Select(s => s.Extension).InvariantContains(defaultFormat))
                    {
                        var nativeSource = new SourceSet() { Source = imgSrcSet, Lqip = imgLqip, Format = defaultFormat };
                        sources.Add(nativeSource);
                    }                                       
                }

                if (!this.Decorative)
                {
                    AltText ??= MediaItem.Name;
                }
                else
                {
                    AltText = "";
                }

                var renderHeight = Height;
                if (_slimsyOptions.TagHelper.ImageDimensions)
                {
                    // if only a width parameter we can calculate the height
                    if (renderHeight == 0)
                    {
                        var sourceWidth = MediaItem.Value<int>(Constants.Conventions.Media.Width);
                        var sourceHeight = MediaItem.Value<int>(Constants.Conventions.Media.Height);
                        if (sourceHeight != 0 && sourceWidth != 0) {
                            decimal ratio = (decimal)Width / (decimal)sourceWidth;
                            int calculatedHeight = (int)Math.Round(sourceHeight * ratio, 0);
                            renderHeight = calculatedHeight;
                        }                        
                    }
                }
                var imgDimensions = _slimsyOptions.TagHelper.ImageDimensions ? $" width=\"{this.Width}\" height=\"{renderHeight}\"" : string.Empty;
                var fetchPriorityAttribute = !(FetchPriority == FetchPriority.Auto) ? $" fetchpriority=\"{FetchPriority.ToString().ToLower()}\"" : null;

                var htmlContent = "";

                foreach (var source in sources)
                {
                    if (RenderLQIP)
                    {
                        htmlContent += Environment.NewLine + $@"<source data-srcset=""{source.Source}"" srcset=""{source.Lqip}"" type=""{SlimsyService.MimeType(source.Format)}"" data-sizes=""auto"" />" + Environment.NewLine;
                    } else
                    {
                        htmlContent += Environment.NewLine + $@"<source data-srcset=""{source.Source}"" type=""{SlimsyService.MimeType(source.Format)}"" data-sizes=""auto"" />" + Environment.NewLine;
                    }
                }
      
                if (RenderLQIP)
                {
                    htmlContent += $@"<img src=""{imgLqip}"" data-src=""{imgSrc}"" class=""{CssClass}"" data-sizes=""auto"" alt=""{AltText}""{imgDimensions}{fetchPriorityAttribute}{(this.Decorative ? " role=\"presentation\"" : string.Empty)} />" + Environment.NewLine;
                }
                else
                {
                    htmlContent += $@"<img data-src=""{imgSrc}"" class=""{CssClass}"" data-sizes=""auto"" alt=""{AltText}""{imgDimensions}{fetchPriorityAttribute}{(this.Decorative ? " role=\"presentation\"" : string.Empty)} />" + Environment.NewLine;
                }
                
                output.TagName = "picture";
                output.TagMode = TagMode.StartTagAndEndTag;
                output.Content.SetHtmlContent(htmlContent);
            }
        }
    }
}
