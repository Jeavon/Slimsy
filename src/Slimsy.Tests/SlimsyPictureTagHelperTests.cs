using Slimsy.Enums;

using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

using static Slimsy.Tests.TagHelperTestHarness;

namespace Slimsy.Tests
{
    public class SlimsyPictureTagHelperTests
    {
        private static SlimsyPictureTagHelper Create(bool imageDimensions = false, params ImageCropperValue.ImageCropperCrop[] crops)
        {
            var helper = CreateTagHelper(DefaultOptions(imageDimensions));
            helper.MediaItem = CreateMedia(crops);
            return helper;
        }

        private static string[] Sources(string html) =>
            html.Split('\n').Select(l => l.Trim()).Where(l => l.StartsWith("<source")).ToArray();

        private static string Img(string html) =>
            html.Split('\n').Select(l => l.Trim()).Single(l => l.StartsWith("<img"));

        #region Issue 92 - manual sizes on lazy sources

        [Fact]
        public void Lazy_ManualSizes_RendersRealSizesOnSources()
        {
            var helper = Create();
            helper.Width = 800;
            helper.Height = 600;
            helper.ManualSizes = "(min-width: 1000px) 400px, 100vw";

            var html = Render(helper);

            Assert.All(Sources(html), s =>
            {
                Assert.Contains(" sizes=\"(min-width: 1000px) 400px, 100vw\"", s);
                Assert.DoesNotContain("data-sizes", s);
            });
        }

        [Fact]
        public void Lazy_ManualSizes_KeepsDataSizesOnImg()
        {
            var helper = Create();
            helper.Width = 800;
            helper.Height = 600;
            helper.ManualSizes = "50vw";

            Assert.Contains("data-sizes=\"50vw\"", Img(Render(helper)));
        }

        [Fact]
        public void Lazy_ManualSizes_WithoutLqip_RendersRealSizesOnSources()
        {
            var helper = Create();
            helper.Width = 800;
            helper.Height = 600;
            helper.RenderLQIP = false;
            helper.ManualSizes = "50vw";

            Assert.All(Sources(Render(helper)), s =>
            {
                Assert.Contains(" sizes=\"50vw\"", s);
                Assert.DoesNotContain("data-sizes", s);
            });
        }

        [Fact]
        public void Lazy_Auto_UsesDataSizesOnSources()
        {
            var helper = Create();
            helper.Width = 800;
            helper.Height = 600;

            var html = Render(helper);

            Assert.All(Sources(html), s =>
            {
                Assert.Contains("data-sizes=\"auto\"", s);
                Assert.DoesNotContain(" sizes=", s);
            });
            Assert.Contains("data-sizes=\"auto\"", Img(html));
        }

        [Fact]
        public void Eager_DefaultsSizesTo100vw()
        {
            var helper = Create();
            helper.Width = 800;
            helper.Height = 600;
            helper.Loading = Loading.Eager;

            var html = Render(helper);

            Assert.All(Sources(html), s => Assert.Contains(" sizes=\"100vw\"", s));
            Assert.Contains(" sizes=\"100vw\"", Img(html));
            Assert.DoesNotContain("data-sizes", html);
        }

        [Fact]
        public void Eager_ManualSizes_AreRespected()
        {
            var helper = Create();
            helper.Width = 800;
            helper.Height = 600;
            helper.Loading = Loading.Eager;
            helper.ManualSizes = "50vw";

            Assert.All(Sources(Render(helper)), s => Assert.Contains(" sizes=\"50vw\"", s));
        }

        #endregion

        #region Issue 93 - crop dimensions

        [Fact]
        public void CropAlias_NoExplicitDimensions_UsesCropDimensionsOnImg()
        {
            var helper = Create(true, Crop("wide", 2100, 900));
            helper.CropAlias = "wide";

            var img = Img(Render(helper));

            Assert.Contains("width=\"2100\" height=\"900\"", img);
            Assert.DoesNotContain("width=\"0\"", img);
        }

        [Fact]
        public void CropAlias_ExplicitDimensions_Win()
        {
            var helper = Create(true, Crop("wide", 2100, 900));
            helper.CropAlias = "wide";
            helper.Width = 700;
            helper.Height = 300;

            Assert.Contains("width=\"700\" height=\"300\"", Img(Render(helper)));
        }

        [Fact]
        public void CropAlias_DimensionsDisabled_RendersNoDimensions()
        {
            var helper = Create(false, Crop("wide", 2100, 900));
            helper.CropAlias = "wide";

            var html = Render(helper);

            Assert.DoesNotContain("width=\"", Img(html));
            Assert.All(Sources(html), s => Assert.DoesNotContain("width=\"", s));
        }

        [Fact]
        public void CropAlias_Sources_CarryCropDimensions()
        {
            var helper = Create(true, Crop("wide", 2100, 900));
            helper.CropAlias = "wide";

            Assert.All(Sources(Render(helper)), s => Assert.Contains("width=\"2100\" height=\"900\"", s));
        }

        [Fact]
        public void MobileCropAlias_MobileSourcesCarryMobileCropDimensions()
        {
            var helper = Create(true, Crop("wide", 2100, 900), Crop("tall", 600, 800));
            helper.CropAlias = "wide";
            helper.MobileCropAlias = "tall";

            var sources = Sources(Render(helper));

            var mobile = sources.Where(s => s.Contains("media=\"(max-width: 320px)\"")).ToArray();
            var desktop = sources.Except(mobile).ToArray();

            Assert.NotEmpty(mobile);
            Assert.NotEmpty(desktop);
            Assert.All(mobile, s => Assert.Contains("width=\"600\" height=\"800\"", s));
            Assert.All(desktop, s => Assert.Contains("width=\"2100\" height=\"900\"", s));
        }

        [Fact]
        public void WidthAndHeight_NoCrop_RendersExplicitDimensionsOnImgOnly()
        {
            var helper = Create(true);
            helper.Width = 400;
            helper.Height = 300;

            var html = Render(helper);

            Assert.Contains("width=\"400\" height=\"300\"", Img(html));
            Assert.All(Sources(html), s => Assert.DoesNotContain("width=\"", s));
        }

        [Fact]
        public void WidthOnly_NoCrop_CalculatesHeightFromSourceRatio()
        {
            var helper = Create(true);
            helper.Width = 400;

            // source is 2000x1500
            Assert.Contains("width=\"400\" height=\"300\"", Img(Render(helper)));
        }

        [Fact]
        public void NoWidthHeightOrCrop_FallsBackToSourceDimensions()
        {
            var helper = Create(true);

            var img = Img(Render(helper));

            // source is 2000x1500
            Assert.Contains("width=\"2000\" height=\"1500\"", img);
        }

        [Fact]
        public void WidthOnly_NoCrop_UrlsDoNotContainHeightParameter()
        {
            var helper = Create();
            helper.Width = 400;

            var html = Render(helper);

            Assert.DoesNotContain("height=0", html);
            Assert.DoesNotContain("&amp;height=", string.Join("\n", Sources(html)));
            Assert.Contains("data-src=\"/media/test/image.jpg?width=400&amp;", Img(html));
        }

        [Fact]
        public void HeightOnly_NoCrop_CalculatesWidthFromSourceRatio()
        {
            var helper = Create(true);
            helper.Height = 300;

            Assert.Contains("width=\"400\" height=\"300\"", Img(Render(helper)));
        }

        [Fact]
        public void UnknownCropAlias_DoesNotRenderZeroDimensions()
        {
            var helper = Create(true, Crop("wide", 2100, 900));
            helper.CropAlias = "does-not-exist";

            var img = Img(Render(helper));

            Assert.DoesNotContain("width=\"0\"", img);
            Assert.DoesNotContain("height=\"0\"", img);
        }

        #endregion

        #region General structure

        [Fact]
        public void Lazy_AddsLazyloadClass_AndRendersLqip()
        {
            var helper = Create();
            helper.Width = 400;
            helper.Height = 300;
            helper.CssClass = "mine";

            var img = Img(Render(helper));

            Assert.Contains("class=\"lazyload mine\"", img);
            Assert.Contains(" src=\"", img);
            Assert.Contains("data-src=\"", img);
        }

        [Fact]
        public void Lazy_NoLqip_OmitsImgSrc()
        {
            var helper = Create();
            helper.Width = 400;
            helper.Height = 300;
            helper.RenderLQIP = false;

            var img = Img(Render(helper));

            Assert.DoesNotContain(" src=\"", img);
            Assert.Contains("data-src=\"", img);
        }

        [Fact]
        public void Eager_DoesNotUseLazyAttributes()
        {
            var helper = Create();
            helper.Width = 400;
            helper.Height = 300;
            helper.Loading = Loading.Eager;

            var html = Render(helper);

            Assert.DoesNotContain("data-src", html);
            Assert.DoesNotContain("lazyload", html);
        }

        [Fact]
        public void Sources_RenderWebpThenJpeg()
        {
            var helper = Create();
            helper.Width = 400;
            helper.Height = 300;

            var sources = Sources(Render(helper));

            Assert.Contains("type=\"image/webp\"", sources[0]);
            Assert.Contains("type=\"image/jpeg\"", sources[1]);
        }

        [Fact]
        public void Urls_ArePopulated()
        {
            var helper = Create();
            helper.Width = 400;
            helper.Height = 300;

            var html = Render(helper);

            Assert.All(Sources(html), s => Assert.Contains("data-srcset=\"/media/test/image.jpg?width=160", s));
            Assert.Contains("data-src=\"/media/test/image.jpg?width=400&amp;height=300", Img(html));
        }

        [Fact]
        public void Decorative_RendersEmptyAltAndPresentationRole()
        {
            var helper = Create();
            helper.Width = 400;
            helper.Height = 300;
            helper.Decorative = true;

            var img = Img(Render(helper));

            Assert.Contains("alt=\"\"", img);
            Assert.Contains("role=\"presentation\"", img);
        }

        [Fact]
        public void AltText_DefaultsToMediaName()
        {
            var helper = Create();
            helper.Width = 400;
            helper.Height = 300;

            Assert.Contains("alt=\"Test image\"", Img(Render(helper)));
        }

        [Fact]
        public void AltText_IsHtmlEncoded()
        {
            var helper = Create();
            helper.Width = 400;
            helper.Height = 300;
            helper.AltText = "a <b>";

            Assert.DoesNotContain("<b>", Img(Render(helper)));
        }

        [Theory]
        [InlineData(FetchPriority.High, "fetchpriority=\"high\"")]
        [InlineData(FetchPriority.Low, "fetchpriority=\"low\"")]
        public void FetchPriority_IsRendered(FetchPriority priority, string expected)
        {
            var helper = Create();
            helper.Width = 400;
            helper.Height = 300;
            helper.FetchPriority = priority;

            Assert.Contains(expected, Img(Render(helper)));
        }

        [Fact]
        public void FetchPriority_Auto_IsNotRendered()
        {
            var helper = Create();
            helper.Width = 400;
            helper.Height = 300;

            Assert.DoesNotContain("fetchpriority", Img(Render(helper)));
        }

        #endregion
    }
}
