using Slimsy.Configuration;
using Slimsy.Services;

using Umbraco.Cms.Core.Models;

using static Slimsy.Tests.TagHelperTestHarness;

namespace Slimsy.Tests
{
    public class SlimsyServiceTests
    {
        // Fake media is 2000x1500, options default to WidthStep 160 / MaxWidth 960 (see TagHelperTestHarness.DefaultOptions)
        private static readonly int[] DefaultSteps = { 160, 320, 480, 640, 800, 960 };

        private static string[] Candidates(string srcset) =>
            System.Net.WebUtility.HtmlDecode(srcset).Split(',', StringSplitOptions.RemoveEmptyEntries);

        private static int[] Widths(string srcset) =>
            Candidates(srcset).Select(c => int.Parse(c[(c.LastIndexOf(' ') + 1)..^1])).ToArray();

        private static string Param(string url, string name)
        {
            // tolerate a trailing srcset descriptor, e.g. "...&format=webp 160w"
            url = url.Split(' ')[0];
            var query = System.Net.WebUtility.HtmlDecode(url[(url.IndexOf('?') + 1)..]);
            return query.Split('&').Select(p => p.Split('=')).Where(p => p[0] == name).Select(p => p[1]).FirstOrDefault() ?? string.Empty;
        }

        #region MimeType

        [Theory]
        [InlineData("jpg", "image/jpeg")]
        [InlineData("jpeg", "image/jpeg")]
        [InlineData("png", "image/png")]
        [InlineData("gif", "image/gif")]
        [InlineData("webp", "image/webp")]
        [InlineData("avif", "image/avif")]
        [InlineData("jxl", "image/jxl")]
        public void MimeType_KnownExtensions(string extension, string expected)
        {
            Assert.Equal(expected, SlimsyService.MimeType(extension));
        }

        [Theory]
        [InlineData("svg")]
        [InlineData("pdf")]
        [InlineData("")]
        public void MimeType_UnsupportedExtensions_ReturnNull(string extension)
        {
            Assert.Null(SlimsyService.MimeType(extension));
        }

        #endregion

        #region GetSrcSetUrls (width & height)

        [Fact]
        public void SrcSet_StepsByWidthStep_UpToMaxWidth()
        {
            var service = CreateService(DefaultOptions());

            var srcset = service.GetSrcSetUrls(CreateMedia(), 400, 300).ToString()!;

            Assert.Equal(DefaultSteps, Widths(srcset));
        }

        [Fact]
        public void SrcSet_StopsAtSourceWidth_WhenSmallerThanMaxWidth()
        {
            var options = DefaultOptions();
            options.MaxWidth = 5000;
            var service = CreateService(options);

            var widths = Widths(service.GetSrcSetUrls(CreateMedia(), 400, 300).ToString()!);

            // source image is 2000px wide
            Assert.Equal(1920, widths.Max());
        }

        [Fact]
        public void SrcSet_HeightFollowsRequestedRatio()
        {
            var service = CreateService(DefaultOptions());

            var candidates = Candidates(service.GetSrcSetUrls(CreateMedia(), 400, 300).ToString()!);

            Assert.All(candidates, c =>
            {
                var url = c.Split(' ')[0];
                var w = int.Parse(Param(url, "width"));
                Assert.Equal((int)Math.Round(w * 0.75m), int.Parse(Param(url, "height")));
            });
        }

        [Fact]
        public void SrcSet_UsesDefaultQuality_WhenNoneGiven()
        {
            var service = CreateService(DefaultOptions());

            var candidates = Candidates(service.GetSrcSetUrls(CreateMedia(), 400, 300).ToString()!);

            Assert.All(candidates, c => Assert.Equal("90", Param(c, "quality")));
        }

        [Fact]
        public void SrcSet_UsesSuppliedQuality()
        {
            var service = CreateService(DefaultOptions());

            var candidates = Candidates(service.GetSrcSetUrls(CreateMedia(), 400, 300, quality: 55).ToString()!);

            Assert.All(candidates, c => Assert.Equal("55", Param(c, "quality")));
        }

        [Fact]
        public void SrcSet_AppendsOutputFormat()
        {
            var service = CreateService(DefaultOptions());

            var candidates = Candidates(service.GetSrcSetUrls(CreateMedia(), 400, 300, outputFormat: "webp").ToString()!);

            Assert.All(candidates, c => Assert.Equal("webp", Param(c, "format")));
        }

        [Fact]
        public void SrcSet_FallsBackToConfiguredFormat_WhenNoOutputFormat()
        {
            var service = CreateService(DefaultOptions());

            var candidates = Candidates(service.GetSrcSetUrls(CreateMedia(), 400, 300).ToString()!);

            Assert.All(candidates, c => Assert.Equal("jpg", Param(c, "format")));
        }

        [Fact]
        public void SrcSet_OutputIsHtmlEncoded()
        {
            var service = CreateService(DefaultOptions());

            var srcset = service.GetSrcSetUrls(CreateMedia(), 400, 300).ToString()!;

            Assert.DoesNotContain("&width", srcset.Replace("&amp;width", string.Empty));
            Assert.Contains("&amp;", srcset);
        }

        [Fact]
        public void SrcSet_UseCropAsSrc_InsertsExactWidthCandidate()
        {
            var options = DefaultOptions();
            options.UseCropAsSrc = true;
            var service = CreateService(options);

            // 400 is not a multiple of the 160 step so it should be added as an extra candidate
            var widths = Widths(service.GetSrcSetUrls(CreateMedia(), 400, 300).ToString()!);

            Assert.Contains(400, widths);
            Assert.Equal(DefaultSteps.Length + 1, widths.Length);
        }

        [Fact]
        public void SrcSet_UseCropAsSrc_Off_DoesNotInsertExactWidth()
        {
            var service = CreateService(DefaultOptions());

            Assert.DoesNotContain(400, Widths(service.GetSrcSetUrls(CreateMedia(), 400, 300).ToString()!));
        }

        [Fact]
        public void SrcSet_WidthStepOption_ChangesCandidates()
        {
            var options = DefaultOptions();
            options.WidthStep = 320;
            var service = CreateService(options);

            Assert.Equal(new[] { 320, 640, 960 }, Widths(service.GetSrcSetUrls(CreateMedia(), 400, 300).ToString()!));
        }

        #endregion

        [Fact]
        public void SrcSet_WidthOnly_OmitsHeightParameter()
        {
            var service = CreateService(DefaultOptions());

            var srcset = service.GetSrcSetUrls(CreateMedia(), 400, 0).ToString()!;

            Assert.Equal(DefaultSteps, Widths(srcset));
            Assert.DoesNotContain("height=", srcset);
        }

        [Fact]
        public void SrcSet_WidthOnly_UseCropAsSrc_OmitsHeightParameter()
        {
            var options = DefaultOptions();
            options.UseCropAsSrc = true;
            var service = CreateService(options);

            var srcset = service.GetSrcSetUrls(CreateMedia(), 400, 0).ToString()!;

            Assert.Contains(400, Widths(srcset));
            Assert.DoesNotContain("height=", srcset);
        }

        [Fact]
        public void CropUrl_ZeroDimensions_AreOmitted()
        {
            var service = CreateService(DefaultOptions());

            var url = service.GetCropUrl(CreateMedia(), 400, 0, htmlEncode: false).ToString()!;

            Assert.Equal("400", Param(url, "width"));
            Assert.DoesNotContain("height=", url);
        }

        #region GetSrcSetUrls (crop alias)

        [Fact]
        public void CropSrcSet_UsesCropRatioForHeight()
        {
            var service = CreateService(DefaultOptions());
            var media = CreateMedia(Crop("wide", 2100, 900));

            var candidates = Candidates(service.GetSrcSetUrls(media, "wide").ToString()!);

            Assert.Equal(DefaultSteps, Widths(service.GetSrcSetUrls(media, "wide").ToString()!));
            Assert.All(candidates, c =>
            {
                var url = c.Split(' ')[0];
                var w = int.Parse(Param(url, "width"));
                Assert.Equal((int)Math.Round(w * (900m / 2100m)), int.Parse(Param(url, "height")));
            });
        }

        [Fact]
        public void CropSrcSet_UnknownCrop_ReturnsEmpty()
        {
            var service = CreateService(DefaultOptions());

            Assert.Equal(string.Empty, service.GetSrcSetUrls(CreateMedia(Crop("wide", 2100, 900)), "nope").ToString());
        }

        [Fact]
        public void CropSrcSet_StartingWidth_SkipsSmallerSteps()
        {
            var service = CreateService(DefaultOptions());
            var media = CreateMedia(Crop("wide", 2100, 900));

            var widths = Widths(service.GetSrcSetUrls(media, "wide", startingWidth: 480).ToString()!);

            Assert.Equal(new[] { 480, 640, 800, 960 }, widths);
        }

        [Fact]
        public void CropSrcSet_MaxWidth_CapsCandidates()
        {
            var service = CreateService(DefaultOptions());
            var media = CreateMedia(Crop("wide", 2100, 900));

            var widths = Widths(service.GetSrcSetUrls(media, "wide", maxWidth: 480).ToString()!);

            Assert.Equal(new[] { 160, 320, 480 }, widths);
        }

        [Fact]
        public void CropSrcSet_StartingWidthAboveMaxWidth_StillRendersOneCandidate()
        {
            var service = CreateService(DefaultOptions());
            var media = CreateMedia(Crop("wide", 2100, 900));

            var widths = Widths(service.GetSrcSetUrls(media, "wide", startingWidth: 1000, maxWidth: 320).ToString()!);

            Assert.Equal(new[] { 320 }, widths);
        }

        #endregion

        #region GetCropUrl options

        [Fact]
        public void CropUrl_AutoOrient_AppendsParameter()
        {
            var options = DefaultOptions();
            options.AutoOrient = true;
            var service = CreateService(options);

            var url = service.GetCropUrl(CreateMedia(), 400, 300).ToString()!;

            Assert.Contains("autoorient=true", url);
        }

        [Fact]
        public void CropUrl_AutoOrient_Off_OmitsParameter()
        {
            var service = CreateService(DefaultOptions());

            Assert.DoesNotContain("autoorient", service.GetCropUrl(CreateMedia(), 400, 300).ToString()!);
        }

        [Fact]
        public void CropUrl_AppendSourceDimensions_AddsSourceWidthAndHeight()
        {
            var options = DefaultOptions();
            options.AppendSourceDimensions = true;
            var service = CreateService(options);

            var url = service.GetCropUrl(CreateMedia(), 400, 300).ToString()!;

            Assert.Contains("sourceWidth=2000", url);
            Assert.Contains("sourceHeight=1500", url);
        }

        [Fact]
        public void CropUrl_HtmlEncodesByDefault_AndCanBeDisabled()
        {
            var options = DefaultOptions();
            options.AutoOrient = true;
            var service = CreateService(options);
            var media = CreateMedia();

            Assert.Contains("&amp;", service.GetCropUrl(media, 400, 300).ToString()!);
            Assert.DoesNotContain("&amp;", service.GetCropUrl(media, 400, 300, htmlEncode: false).ToString()!);
        }

        [Fact]
        public void CropUrl_UnknownCropAlias_ReturnsEmpty()
        {
            var service = CreateService(DefaultOptions());

            Assert.Equal(string.Empty, service.GetCropUrl(CreateMedia(Crop("wide", 2100, 900)), cropAlias: "nope").ToString());
        }

        [Fact]
        public void CropUrl_UseCropDimensions_UsesCropSize()
        {
            var service = CreateService(DefaultOptions());

            var url = service.GetCropUrl(CreateMedia(Crop("wide", 2100, 900)), cropAlias: "wide", useCropDimensions: true, htmlEncode: false).ToString()!;

            Assert.Equal("2100", Param(url, "width"));
            Assert.Equal("900", Param(url, "height"));
        }

        #endregion
    }
}
