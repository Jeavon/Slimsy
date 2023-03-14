using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using SixLabors.ImageSharp.Web.Processors;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using static Umbraco.Cms.Core.Models.ImageUrlGenerationOptions;
using Umbraco.Cms.Web.Common.ImageProcessors;

namespace Slimsy.ImageUrlGenerators
{
    public sealed class CloudflareImageUrlGenerator : IImageUrlGenerator
    {
        public IEnumerable<string> SupportedImageFileTypes { get; } = (new string[]{"jpg", "png"}).ToList();

        public string? GetImageUrl(ImageUrlGenerationOptions? options)
        {
            if (options?.ImageUrl == null)
            {
                return null;
            }

            var queryString = new Dictionary<string, string?>();
            var commands = new Dictionary<string, string?>();
            Dictionary<string, StringValues> furtherOptions = QueryHelpers.ParseQuery(options.FurtherOptions);

            if (options.Crop is not null)
            {
                CropCoordinates? crop = options.Crop;
                queryString.Add(
                    CropWebProcessor.Coordinates,
                    FormattableString.Invariant($"{crop.Left},{crop.Top},{crop.Right},{crop.Bottom}"));
            }

            if (options.FocalPoint is not null)
            {
                queryString.Add(ResizeWebProcessor.Xy, FormattableString.Invariant($"{options.FocalPoint.Left},{options.FocalPoint.Top}"));
            }

            if (options.ImageCropMode is not null)
            {
                queryString.Add(ResizeWebProcessor.Mode, options.ImageCropMode.ToString()?.ToLowerInvariant());
            }

            if (options.ImageCropAnchor is not null)
            {
                queryString.Add(ResizeWebProcessor.Anchor, options.ImageCropAnchor.ToString()?.ToLowerInvariant());
            }

            if (options.Width is not null)
            {
                queryString.Add(ResizeWebProcessor.Width, options.Width?.ToString(CultureInfo.InvariantCulture));
                commands.Add("width", options.Width?.ToString(CultureInfo.InvariantCulture));
            }

            if (options.Height is not null)
            {
                queryString.Add(ResizeWebProcessor.Height, options.Height?.ToString(CultureInfo.InvariantCulture));
                commands.Add("height", options.Height?.ToString(CultureInfo.InvariantCulture));
            }

            if (furtherOptions.Remove(FormatWebProcessor.Format, out StringValues format))
            {
                queryString.Add(FormatWebProcessor.Format, format[0]);
            }

            if (options.Quality is not null)
            {
                queryString.Add(QualityWebProcessor.Quality, options.Quality?.ToString(CultureInfo.InvariantCulture));
            }

            foreach (KeyValuePair<string, StringValues> kvp in furtherOptions)
            {
                queryString.Add(kvp.Key, kvp.Value);
            }

            if (options.CacheBusterValue is not null && !string.IsNullOrWhiteSpace(options.CacheBusterValue))
            {
                queryString.Add("rnd", options.CacheBusterValue);
            }

            string cfcommand = string.Empty;
            foreach (KeyValuePair<string, string?> command in commands)
            {
                cfcommand += command.Key + "=" + command.Value;
                if (!command.Equals(commands.Last()))
                {
                    cfcommand += ",";
                }
            }

            return QueryHelpers.AddQueryString( "/cdn-cgi/image/"+ cfcommand + options.ImageUrl, queryString);
        }
    }
}
