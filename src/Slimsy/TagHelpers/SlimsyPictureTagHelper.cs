using System.Web;

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Extensions;

using Slimsy.Configuration;
using Slimsy.Enums;
using Slimsy.Models;
using Slimsy.Services;

using TagHelper = Microsoft.AspNetCore.Razor.TagHelpers.TagHelper;

namespace Slimsy
{
    public class SlimsyPictureTagHelper : TagHelper
    {
        public MediaWithCrops? MediaItem { get; set; }
        /// <summary>
        /// Crop Alias to use, when this attribute is passed, Width, Height, ImageCropMode & ImageCropAnchor parameters are ignored
        /// </summary>
        public string? CropAlias { get; set; }
        /// <summary>
        /// Mobile Crop Alias to use for smaller width variants. When provided, mobile and desktop sources will be generated separately with media queries
        /// </summary>
        public string? MobileCropAlias { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string? AltText { get; set; }
        public string? CssClass { get; set; }
        public bool RenderLQIP { get; set; } = true;
        public bool Decorative { get; set; } = false;
        public FetchPriority FetchPriority { get; set; } = FetchPriority.Auto;
        public ImageCropMode ImageCropMode { get; set; } = ImageCropMode.Crop;
        public ImageCropAnchor ImageCropAnchor { get; set; } = ImageCropAnchor.Center;
        public string PropertyAlias { get; set; } = Umbraco.Cms.Core.Constants.Conventions.Media.File;
        public Loading Loading { get; set; } = Loading.Lazy;
        public string? ManualSizes { get; set; }
        private readonly SlimsyService _slimsyService;
        private readonly SlimsyOptions _slimsyOptions;

        public SlimsyPictureTagHelper(SlimsyService slimsyService, IOptionsMonitor<SlimsyOptions> slimsyOptions)
        {
            _slimsyService = slimsyService;
            _slimsyOptions = slimsyOptions.CurrentValue;
        }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Loading == Loading.Eager && string.IsNullOrEmpty(ManualSizes))
            {
                ManualSizes = "100vw";
            }

            List<PictureSource> pictureSources = _slimsyOptions.TagHelper.DefaultPictureSources.ToList();

            // supporting upgrades
            if (!pictureSources.Select(s => s.Extension).Contains("webp"))
            {
                pictureSources.Add(new PictureSource(){Extension="webp", Quality=70});
            }

            CssClass = Loading switch
            {
                Loading.Lazy when !string.IsNullOrEmpty(CssClass) => $"lazyload {CssClass}",
                Loading.Lazy => "lazyload",
                _ => CssClass
            };

            if (MediaItem != null)
            {
                var umbracoExtension = MediaItem.Value<string>(Umbraco.Cms.Core.Constants.Conventions.Media.Extension);

                var isSingleSource = _slimsyOptions.TagHelper.SingleSources != null && umbracoExtension != null && _slimsyOptions.TagHelper.SingleSources.Contains(umbracoExtension);

                if (isSingleSource)
                {
                    // empty the sources as this should render a single source
                    pictureSources = new List<PictureSource>();
                }

                var defaultFormat = umbracoExtension ?? "jpg";
                string? defaultMimeType = SlimsyService.MimeType(defaultFormat);

                var htmlContent = "";

                if (!this.Decorative)
                {
                    AltText ??= MediaItem.Name;
                }
                else
                {
                    AltText = "";
                }

                AltText = HttpUtility.HtmlAttributeEncode(AltText);

                var fetchPriorityAttribute = !(FetchPriority == FetchPriority.Auto) ? $" fetchpriority=\"{FetchPriority.ToString().ToLower()}\"" : null;

                if (defaultMimeType != null) //supported type
                {
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

                        if (crop != null)
                        {
                            lqipWidth = (int)Math.Round((decimal)crop.Width / 2);
                            lqipHeight = (int)Math.Round((decimal)crop.Height / 2);

                            imgSrc = _slimsyService.GetCropUrl(MediaItem, cropAlias: CropAlias, useCropDimensions: true, furtherOptions: "&format=" + defaultFormat);

                            // Check if mobile crop alias is provided
                            var mobileCrop = !string.IsNullOrEmpty(MobileCropAlias)
                                ? mergedImageCrops?.Crops?.FirstOrDefault(x => x.Alias.InvariantEquals(MobileCropAlias))
                                : null;

                            // setting starting width for desktop sources. This will be adjusted if mobile crop is provided
                            var startingWidth = 0;

                            if (mobileCrop != null && !isSingleSource)
                            {
                                // Generate mobile sources first and set IsMobileSource flag to true
                                foreach (var source in pictureSources)
                                {
                                    var mobileSrcSet = _slimsyService.GetSrcSetUrls(MediaItem, MobileCropAlias!, PropertyAlias, source.Quality, source.Extension, maxWidth: _slimsyOptions.MobileWidth);
                                    imgLqip = _slimsyService.GetCropUrl(MediaItem, lqipWidth, lqipHeight, cropAlias: MobileCropAlias, quality: 20, furtherOptions: "&format=" + source.Extension);
                                    var mobileSource = new SourceSet() { Source = mobileSrcSet, Lqip = imgLqip, Format = source.Extension, IsMobileSource = true };
                                    sources.Add(mobileSource);
                                }

                                // Handle mobile native format
                                if (!pictureSources.Select(s => s.Extension).InvariantContains(defaultFormat))
                                {
                                    var mobileSrcSetNative = _slimsyService.GetSrcSetUrls(MediaItem, MobileCropAlias!, PropertyAlias, outputFormat: defaultFormat, maxWidth: _slimsyOptions.MobileWidth);
                                    imgLqip = _slimsyService.GetCropUrl(MediaItem, lqipWidth, lqipHeight, quality: 20, cropAlias: MobileCropAlias, furtherOptions: "&format=" + defaultFormat);
                                    var mobileNativeSource = new SourceSet() { Source = mobileSrcSetNative, Lqip = imgLqip, Format = defaultFormat, IsMobileSource = true };
                                    sources.Add(mobileNativeSource);
                                }

                                // Set starting width for desktop sources so it doesn't include the mobile widths
                                startingWidth = _slimsyOptions.MobileWidth + _slimsyOptions.WidthStep;
                            }

                            foreach (var source in pictureSources)
                            {
                                imgSrcSet = _slimsyService.GetSrcSetUrls(MediaItem, CropAlias, PropertyAlias, source.Quality, source.Extension, startingWidth: startingWidth);
                                imgLqip = _slimsyService.GetCropUrl(MediaItem, lqipWidth, lqipHeight, cropAlias: CropAlias, quality: 20, furtherOptions: "&format=" + source.Extension);
                                var newSource = new SourceSet() { Source = imgSrcSet, Lqip = imgLqip, Format = source.Extension };
                                sources.Add(newSource);
                            }

                            // native format not included in sources so we add it as the last option, it will use the Slimsy default quality
                            if (defaultFormat != null && !pictureSources.Select(s => s.Extension).InvariantContains(defaultFormat))
                            {
                                imgSrcSet = _slimsyService.GetSrcSetUrls(MediaItem, CropAlias, PropertyAlias, outputFormat: defaultFormat, startingWidth: startingWidth);
                                // ** Using half width/height for LQIP to reduce filesize to a minimum, CSS must oversize the images **
                                imgLqip = _slimsyService.GetCropUrl(MediaItem, lqipWidth, lqipHeight, quality: 20, cropAlias: CropAlias, furtherOptions: "&format=" + defaultFormat);

                                var nativeSource = new SourceSet() { Source = imgSrcSet, Lqip = imgLqip, Format = defaultFormat };
                                sources.Add(nativeSource);
                            }

                        }
                    }
                    else
                    {
                        lqipWidth = (int)Math.Round((decimal)Width / 2);
                        lqipHeight = (int)Math.Round((decimal)Height / 2);

                        if (ImageCropMode != ImageCropMode.Crop)
                        {
                            imgSrc = _slimsyService.GetCropUrl(MediaItem, Width, Height, imageCropMode: ImageCropMode, imageCropAnchor: ImageCropAnchor, furtherOptions: "&format=" + defaultFormat);

                            foreach (var source in pictureSources)
                            {
                                imgSrcSet = _slimsyService.GetSrcSetUrls(MediaItem, Width, Height, ImageCropMode, ImageCropAnchor, PropertyAlias, source.Quality, source.Extension);
                                imgLqip = _slimsyService.GetCropUrl(MediaItem, lqipWidth, lqipHeight, PropertyAlias, quality: 20, imageCropMode: ImageCropMode, imageCropAnchor: ImageCropAnchor, furtherOptions: "&format=" + source.Extension);

                                var newSource = new SourceSet() { Source = imgSrcSet, Lqip = imgLqip, Format = source.Extension };
                                sources.Add(newSource);
                            }

                            imgSrcSet = _slimsyService.GetSrcSetUrls(MediaItem, Width, Height, ImageCropMode, ImageCropAnchor, PropertyAlias, outputFormat: defaultFormat ?? "");
                            // ** Using half width/height for LQIP to reduce filesize to a minimum, CSS must oversize the images **                        
                            imgLqip = _slimsyService.GetCropUrl(MediaItem, lqipWidth, lqipHeight, PropertyAlias, quality: 20, imageCropMode: ImageCropMode, imageCropAnchor: ImageCropAnchor, furtherOptions: "&format=" + defaultFormat);

                        }
                        else // crop use focal point
                        {
                            imgSrc = _slimsyService.GetCropUrl(MediaItem, Width, Height, furtherOptions: "&format=" + defaultFormat);

                            foreach (var source in pictureSources)
                            {
                                imgSrcSet = _slimsyService.GetSrcSetUrls(MediaItem, Width, Height, PropertyAlias, source.Quality, source.Extension);
                                imgLqip = _slimsyService.GetCropUrl(MediaItem, lqipWidth, lqipHeight, PropertyAlias, quality: 20, furtherOptions: "&format=" + source.Extension);
                                var newSource = new SourceSet() { Source = imgSrcSet, Lqip = imgLqip, Format = source.Extension };
                                sources.Add(newSource);
                            }

                            imgSrcSet = _slimsyService.GetSrcSetUrls(MediaItem, Width, Height, PropertyAlias, outputFormat: defaultFormat);
                            // ** Using half width/height for LQIP to reduce filesize to a minimum, CSS must oversize the images **
                            imgLqip = _slimsyService.GetCropUrl(MediaItem, lqipWidth, lqipHeight, PropertyAlias, quality: 20, furtherOptions: "&format=" + defaultFormat);
                        }


                        // native format not included in sources so we add it as the last option
                        if (defaultFormat != null && !pictureSources.Select(s => s.Extension).InvariantContains(defaultFormat))
                        {
                            var nativeSource = new SourceSet() { Source = imgSrcSet, Lqip = imgLqip, Format = defaultFormat };
                            sources.Add(nativeSource);
                        }
                    }

                    var renderHeight = Height;
                    if (_slimsyOptions.TagHelper.ImageDimensions)
                    {
                        // if only a width parameter we can calculate the height
                        if (renderHeight == 0)
                        {
                            var sourceWidth = MediaItem.Value<int>(Constants.Conventions.Media.Width);
                            var sourceHeight = MediaItem.Value<int>(Constants.Conventions.Media.Height);
                            if (sourceHeight != 0 && sourceWidth != 0)
                            {
                                decimal ratio = (decimal)Width / (decimal)sourceWidth;
                                int calculatedHeight = (int)Math.Round(sourceHeight * ratio, 0);
                                renderHeight = calculatedHeight;
                            }
                        }
                    }
                    var imgDimensions = _slimsyOptions.TagHelper.ImageDimensions ? $" width=\"{this.Width}\" height=\"{renderHeight}\"" : string.Empty;
                    var sizes = string.IsNullOrEmpty(ManualSizes) ? "auto" : ManualSizes;

                    foreach (var source in sources)
                    {
                        // Only add media query for mobile sources when mobile crop is provided
                        var mediaAttribute = source.IsMobileSource ? $" media=\"(max-width: {_slimsyOptions.MobileWidth}px)\"" : null;                        
                        var sourceMimeType = SlimsyService.MimeType(source.Format ?? "jpg");
                        if (Loading == Loading.Lazy)
                        {
                            if (RenderLQIP)
                            {
                                htmlContent += Environment.NewLine +
                                               $@"<source data-srcset=""{source.Source}"" srcset=""{source.Lqip}"" type=""{sourceMimeType}""{mediaAttribute} data-sizes=""{sizes}"" />" +
                                               Environment.NewLine;
                            }
                            else
                            {
                                htmlContent += Environment.NewLine +
                                               $@"<source data-srcset=""{source.Source}"" type=""{sourceMimeType}""{mediaAttribute} data-sizes=""{sizes}"" />" +
                                               Environment.NewLine;
                            }
                        }
                        else
                        {
                            htmlContent += Environment.NewLine +
                                           $@"<source srcset=""{source.Source}"" type=""{sourceMimeType}""{mediaAttribute} sizes=""{sizes}"" />" +
                                           Environment.NewLine;
                        }
                    }

                    if (Loading == Loading.Lazy)
                    {
                        if (RenderLQIP)
                        {
                            htmlContent +=
                                $@"<img src=""{imgLqip}"" data-src=""{imgSrc}"" class=""{CssClass}"" data-sizes=""{sizes}"" alt=""{AltText}""{imgDimensions}{fetchPriorityAttribute}{(this.Decorative ? " role=\"presentation\"" : string.Empty)} />" +
                                Environment.NewLine;
                        }
                        else
                        {
                            htmlContent +=
                                $@"<img data-src=""{imgSrc}"" class=""{CssClass}"" data-sizes=""{sizes}"" alt=""{AltText}""{imgDimensions}{fetchPriorityAttribute}{(this.Decorative ? " role=\"presentation\"" : string.Empty)} />" +
                                Environment.NewLine;
                        }
                    }
                    else
                    {
                        htmlContent +=
                            $@"<img src=""{imgSrc}"" class=""{CssClass}"" sizes=""{sizes}"" alt=""{AltText}""{imgDimensions}{fetchPriorityAttribute}{(this.Decorative ? " role=\"presentation\"" : string.Empty)} />" +
                            Environment.NewLine;
                    }

                    output.TagName = "picture";
                    output.TagMode = TagMode.StartTagAndEndTag;
                }
                else // not supoprted type
                {
                    var imgDimensions = _slimsyOptions.TagHelper.ImageDimensions ? $" width=\"{this.Width}\" height=\"{this.Height}\"" : string.Empty;
                    htmlContent += $@"<img src=""{MediaItem.Url()}"" class=""{CssClass}"" alt=""{AltText}""{imgDimensions}{fetchPriorityAttribute}{(this.Decorative ? " role=\"presentation\"" : string.Empty)} />" + Environment.NewLine;
                    output.TagName = null;
                }

                output.TagMode = TagMode.StartTagAndEndTag;
                output.Content.SetHtmlContent(htmlContent);
            }
        }
    }
}