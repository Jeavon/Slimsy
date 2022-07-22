using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Slimsy.Services;
using System;
using System.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Extensions;

namespace Slimsy
{
    public class SlimsyPictureTagHelper : TagHelper
    {
        public MediaWithCrops MediaItem { get; set; }
        /// <summary>
        /// Crop Alias to use, when this attribute is passed, width and height attributes are ignored
        /// </summary>
        public string CropAlias { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string? AltText { get; set; }
        public string CssClass { get; set; }
        public bool RenderWebpAlternative { get; set; } = true;
        public bool RenderLQIP { get; set; } = true;
        public string PropertyAlias { get; set; } = Umbraco.Cms.Core.Constants.Conventions.Media.File;

        private readonly SlimsyService _slimsyService;

        public SlimsyPictureTagHelper(SlimsyService slimsyService)
        {
            _slimsyService = slimsyService;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {

            CssClass = !string.IsNullOrEmpty(CssClass) ? $"lazyload {CssClass}" : "lazyload";

            if (MediaItem != null)
            {
                string defaultMimeType;

                var umbracoExtension = MediaItem.Value<string>(Umbraco.Cms.Core.Constants.Conventions.Media.Extension);

                var defaultFormat = umbracoExtension;

                switch (umbracoExtension)
                {
                    case "jpg":
                        defaultMimeType = "image/jpeg";
                        break;
                    case "png":
                        defaultMimeType = "image/png";
                        break;
                    case "gif":
                        defaultMimeType = "image/gif";
                        RenderWebpAlternative = false;
                        break;
                    default:
                        defaultMimeType = "image/jpeg";
                        defaultFormat = "jpg";
                        break;
                }

                int lqipWidth;
                int lqipHeight;

                IHtmlContent imgSrcSet, imgSrcSetWebP;
                IHtmlContent imgSrc, imgLqip, imgLqipWebP;

                if (!string.IsNullOrEmpty(CropAlias))
                {  
                    var globalImageCrops = MediaItem.Value<ImageCropperValue>(PropertyAlias);                
                    var mergedImageCrops = globalImageCrops.Merge(MediaItem.LocalCrops);

                    var crop = mergedImageCrops?.Crops?.FirstOrDefault(x => x.Alias.InvariantEquals(CropAlias));

                    lqipWidth = (int)Math.Round((decimal)crop.Width / 2);
                    lqipHeight = (int)Math.Round((decimal)crop.Height / 2);

                    imgSrcSet = _slimsyService.GetSrcSetUrls(MediaItem, CropAlias, PropertyAlias, outputFormat: defaultFormat);
                    imgSrcSetWebP = _slimsyService.GetSrcSetUrls(MediaItem, CropAlias, PropertyAlias, 70, "webp");

                    imgSrc = _slimsyService.GetCropUrl(MediaItem, cropAlias:CropAlias, useCropDimensions:true, furtherOptions: "&format=" + defaultFormat);

                    // ** Using half width/height for LQIP to reduce filesize to a minimum, CSS must oversize the images **
                    imgLqip = _slimsyService.GetCropUrl(MediaItem,  lqipWidth, lqipHeight, quality: 20, cropAlias: CropAlias, furtherOptions: "&format=" + defaultFormat);
                    imgLqipWebP = _slimsyService.GetCropUrl(MediaItem, lqipWidth, lqipHeight, cropAlias:CropAlias, quality: 20, furtherOptions: "&format=webp");
                } else
                {
                    lqipWidth = (int)Math.Round((decimal)Width / 2);
                    lqipHeight = (int)Math.Round((decimal)Height / 2);

                    imgSrcSet = _slimsyService.GetSrcSetUrls(MediaItem, Width, Height, PropertyAlias, outputFormat: defaultFormat);
                    imgSrcSetWebP = _slimsyService.GetSrcSetUrls(MediaItem, Width, Height, PropertyAlias, 70, "webp");

                    imgSrc = _slimsyService.GetCropUrl(MediaItem, Width, Height, furtherOptions: "&format=" + defaultFormat);

                    // ** Using half width/height for LQIP to reduce filesize to a minimum, CSS must oversize the images **
                    imgLqip = _slimsyService.GetCropUrl(MediaItem, lqipWidth, lqipHeight, quality: 20, furtherOptions: "&format=" + defaultFormat);
                    imgLqipWebP = _slimsyService.GetCropUrl(MediaItem, lqipWidth, lqipHeight, quality: 20, furtherOptions: "&format=webp");
                }
               
                if (AltText == null)
                {
                    AltText = MediaItem.Name;
                }

                var htmlContent = "";

                if (RenderWebpAlternative)
                {
                    if (RenderLQIP)
                    {
                        htmlContent += Environment.NewLine + $@"<source data-srcset=""{imgSrcSetWebP}"" srcset=""{imgLqipWebP}"" type=""image/webp"" data-sizes=""auto"" />" + Environment.NewLine;
                    } else
                    {
                        htmlContent += Environment.NewLine + $@"<source data-srcset=""{imgSrcSetWebP}"" type=""image/webp"" data-sizes=""auto"" />" + Environment.NewLine;
                    }
                }
      
                if (RenderLQIP)
                {
                    htmlContent += $@"<source data-srcset=""{imgSrcSet}"" srcset=""{imgLqip}"" type=""{defaultMimeType}"" data-sizes=""auto"" />" + Environment.NewLine;
                    htmlContent += $@"<img src=""{imgLqip}"" data-src=""{imgSrc}"" class=""{CssClass}"" data-sizes=""auto"" alt=""{AltText}"" />" + Environment.NewLine;
                }
                else
                {
                    htmlContent += $@"<source data-srcset=""{imgSrcSet}"" type=""{defaultMimeType}"" data-sizes=""auto"" />" + Environment.NewLine;
                    htmlContent += $@"<img data-src=""{imgSrc}"" class=""{CssClass}"" data-sizes=""auto"" alt=""{AltText}"" />" + Environment.NewLine;
                }
                
                output.TagName = "picture";
                output.TagMode = TagMode.StartTagAndEndTag;
                output.Content.SetHtmlContent(htmlContent);
            }
        }
    }
}
