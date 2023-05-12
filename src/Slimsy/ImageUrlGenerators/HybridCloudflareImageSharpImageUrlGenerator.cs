using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using SixLabors.ImageSharp.Web.Processors;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using static Umbraco.Cms.Core.Models.ImageUrlGenerationOptions;
using Umbraco.Cms.Web.Common.ImageProcessors;
using System.Drawing.Drawing2D;
using Umbraco.Cms.Web.Common.Media;

namespace Slimsy.ImageUrlGenerators
{
    public sealed class HybridCloudflareImageSharpImageUrlGenerator : IImageUrlGenerator 
    {
        public IEnumerable<string> CloudFlareSupportedImageFileTypes { get; } = (new string[]{"webp", "avif"}).ToList();
 
        public IEnumerable<string> SupportedImageFileTypes { get; }
        private SixLabors.ImageSharp.Configuration _configuration { get; }

        public HybridCloudflareImageSharpImageUrlGenerator(SixLabors.ImageSharp.Configuration configuration) {

            SupportedImageFileTypes = configuration.ImageFormats.SelectMany(f => f.FileExtensions).ToArray();
            _configuration = configuration;
        }
     
        public string? GetImageUrl(ImageUrlGenerationOptions? options)
        {
            if (options?.ImageUrl == null)
            {
                return null;
            }

            var cfCommands = new Dictionary<string, string?>();
            Uri fakeBaseUri = new Uri("https://localhost/");
            var imageSharpString = new ImageSharpImageUrlGenerator(_configuration).GetImageUrl(options);

            Dictionary<string, StringValues> imageSharpCommands = QueryHelpers.ParseQuery(new Uri(fakeBaseUri, imageSharpString).Query);

            // remove format from ImageSharp and add it to Cloudflare, additionally set ImageSharp quality to 100 (as source) and add quality parameter to Cloudflare 
            if (imageSharpCommands.Remove(FormatWebProcessor.Format, out StringValues format))
            {
                if (CloudFlareSupportedImageFileTypes.Contains(format[0]))
                {
                    var quality = imageSharpCommands[QualityWebProcessor.Quality];
                    imageSharpCommands[QualityWebProcessor.Quality] = "100";
                    cfCommands.Add(FormatWebProcessor.Format, format[0]);
                    cfCommands.Add(QualityWebProcessor.Quality, quality);
                } else
                {
                    return imageSharpString;
                }
            }

            string cfCommandString = string.Empty;
            foreach (KeyValuePair<string, string?> command in cfCommands)
            {
                cfCommandString += command.Key + "=" + command.Value;
                if (!command.Equals(cfCommands.Last()))
                {
                    cfCommandString += ",";
                }
            }

            if (cfCommandString == string.Empty)
            {
                return imageSharpString;
            }

            return  QueryHelpers.AddQueryString("/cdn-cgi/image/" + cfCommandString + options.ImageUrl, imageSharpCommands);
        }
    }
}
