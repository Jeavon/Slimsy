﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Slimsy.Services;
using System;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Slimsy
{
    public class SlimsyPictureTagHelper : TagHelper
    {
        public IPublishedContent Image { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string? AltText { get; set; }


        private readonly IUrlHelper _urlHelper;
        private readonly SlimsyService _slimsyService;

        public SlimsyPictureTagHelper(IUrlHelperFactory urlHelperFactory, IActionContextAccessor contextAccessor, SlimsyService slimsyService)
        {
            if (contextAccessor.ActionContext == null)
            {
                throw new ArgumentNullException(nameof(contextAccessor));
            }
            _urlHelper = urlHelperFactory.GetUrlHelper(contextAccessor.ActionContext);
            _slimsyService = slimsyService;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var mediaFileAlias = global::Umbraco.Cms.Core.Constants.Conventions.Media.File;

            if (Image != null)
            {
                var renderWebPAlternative = true;

                string defaultMimeType;

                var umbracoExtension = Image.Value<string>(Umbraco.Cms.Core.Constants.Conventions.Media.Extension);

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
                        renderWebPAlternative = false;
                        break;
                    default:
                        defaultMimeType = "image/jpeg";
                        defaultFormat = "jpg";
                        break;
                }

                var lqipWidth = (int)Math.Round((decimal)Width / 2);
                var lqipHeight = (int)Math.Round((decimal)Height / 2);

                var imgSrcSet = _slimsyService.GetSrcSetUrls(Image, Width, Height, mediaFileAlias, outputFormat: defaultFormat);
                var imgSrcSetWebP = _slimsyService.GetSrcSetUrls(Image, Width, Height, mediaFileAlias, 70, "webp");

                var imgSrc = _urlHelper.GetCropUrl(Image, Width, Height, furtherOptions: "&format=" + defaultFormat);

                // ** Using half width/height for LQIP to reduce filesize to a minimum, CSS must oversize the images **
                var imgLqip = _urlHelper.GetCropUrl(Image, lqipWidth, lqipHeight, quality: 20, furtherOptions: "&format=" + defaultFormat);
                var imgLqipWebP = _urlHelper.GetCropUrl(Image, lqipWidth, lqipHeight, quality: 20, furtherOptions: "&format=webp");

                if (AltText == null)
                {
                    AltText = Image.Name;
                }

                var htmlContent = "<!--[if IE 9]><video style=\"display: none\"><![endif]-->";

                if (renderWebPAlternative)
                {
                    htmlContent += $@"<source data-srcset=""{imgSrcSetWebP}"" srcset=""{imgLqipWebP}"" type=""image/webp"" data-sizes=""auto"" />";
                }

                htmlContent += $@"<source data-srcset=""{imgSrcSet}"" srcset=""{imgLqip}"" type=""{defaultMimeType}"" data-sizes=""auto"" />";

                htmlContent += "<!--[if IE 9]></video><![endif]-->";
                htmlContent += $@"<img src=""{imgLqip}""
                         data-src=""{imgSrc}""
                         class=""lazyload""
                         data-sizes=""auto""
                         alt=""{AltText}"" />";

                output.TagName = "picture";
                output.Content.SetHtmlContent(htmlContent);

            }
        }
    }
}