using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using Slimsy.Configuration;
using Slimsy.Models;
using Slimsy.Services;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Crop Alias to use, when this attribute is passed, width and height attributes are ignored
        /// </summary>
        public string? CropAlias { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string? AltText { get; set; }
        public string? CssClass { get; set; }
        [Obsolete("This property is obsolete, Use PictureSources Options instead.", false)]
        public bool RenderWebpAlternative { get; set; } = true;
        public bool RenderLQIP { get; set; } = true;
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
            List<string>? pictureSources = _slimsyOptions.TagHelper.PictureSources.ToList();

            // supporting upgrades
            if (RenderWebpAlternative && !pictureSources.InvariantContains("webp"))
            {
                pictureSources.Add("webp");
            }

            CssClass = !string.IsNullOrEmpty(CssClass) ? $"lazyload {CssClass}" : "lazyload";

            if (MediaItem != null)
            {
                var umbracoExtension = MediaItem.Value<string>(Umbraco.Cms.Core.Constants.Conventions.Media.Extension);

                if (_slimsyOptions.TagHelper.SingleSources != null && _slimsyOptions.TagHelper.SingleSources.Contains(umbracoExtension))
                {
                    // empty the sources as this should render a single source
                    pictureSources = new List<string>();
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
                            imgSrcSet = _slimsyService.GetSrcSetUrls(MediaItem, CropAlias, PropertyAlias, 70, source);
                            imgLqip = _slimsyService.GetCropUrl(MediaItem, lqipWidth, lqipHeight, cropAlias: CropAlias, quality: 20, furtherOptions: "&format=" + source);
                            var newSource = new SourceSet() { Source = imgSrcSet, Lqip = imgLqip, Format = source };
                            sources.Add(newSource);
                        }

                        // native format not included in sources so we add it as the last option
                        if (!pictureSources.InvariantContains(defaultFormat))
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
                        imgSrcSet = _slimsyService.GetSrcSetUrls(MediaItem, Width, Height, PropertyAlias, 70, source);
                        imgLqip = _slimsyService.GetCropUrl(MediaItem, lqipWidth, lqipHeight, quality: 20, furtherOptions: "&format=" + source);
                        var newSource = new SourceSet() { Source = imgSrcSet, Lqip = imgLqip, Format = source };
                        sources.Add(newSource);
                    }

                    imgSrcSet = _slimsyService.GetSrcSetUrls(MediaItem, Width, Height, PropertyAlias, outputFormat: defaultFormat);
                    // ** Using half width/height for LQIP to reduce filesize to a minimum, CSS must oversize the images **
                    imgLqip = _slimsyService.GetCropUrl(MediaItem, lqipWidth, lqipHeight, quality: 20, furtherOptions: "&format=" + defaultFormat);

                    // native format not included in sources so we add it as the last option
                    if (!pictureSources.InvariantContains(defaultFormat))
                    {
                        var nativeSource = new SourceSet() { Source = imgSrcSet, Lqip = imgLqip, Format = defaultFormat };
                        sources.Add(nativeSource);
                    }                                       
                }
               
                if (AltText == null)
                {
                    AltText = MediaItem.Name;
                }

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
                    htmlContent += $@"<img src=""{imgLqip}"" data-src=""{imgSrc}"" class=""{CssClass}"" data-sizes=""auto"" alt=""{AltText}"" />" + Environment.NewLine;
                }
                else
                {
                    htmlContent += $@"<img data-src=""{imgSrc}"" class=""{CssClass}"" data-sizes=""auto"" alt=""{AltText}"" />" + Environment.NewLine;
                }
                
                output.TagName = "picture";
                output.TagMode = TagMode.StartTagAndEndTag;
                output.Content.SetHtmlContent(htmlContent);
            }
        }
    }
}
