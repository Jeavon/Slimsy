using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

using Moq;

using Slimsy.Configuration;
using Slimsy.Services;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;

using TagHelper = Slimsy.Configuration.TagHelper;

namespace Slimsy.Tests
{
    /// <summary>
    /// Builds a SlimsyPictureTagHelper against faked Umbraco services so the rendered markup can be asserted
    /// </summary>
    internal static class TagHelperTestHarness
    {
        public const string MediaUrl = "/media/test/image.jpg";

        public static SlimsyOptions DefaultOptions(bool imageDimensions = false) => new()
        {
            Format = "jpg",
            BackgroundColor = "false",
            WidthStep = 160,
            MaxWidth = 960,
            MobileWidth = 320,
            TagHelper = new TagHelper
            {
                ImageDimensions = imageDimensions,
                DefaultPictureSources = new[]
                {
                    new PictureSource { Extension = "webp", Quality = 90 },
                    new PictureSource { Extension = "jpg", Quality = 70 },
                },
            },
        };

        /// <summary>
        /// Builds a media item with a 2000x1500 jpg and the supplied crops
        /// </summary>
        public static MediaWithCrops CreateMedia(params ImageCropperValue.ImageCropperCrop[] crops)
        {
            var cropperValue = new ImageCropperValue
            {
                Src = MediaUrl,
                Crops = crops,
            };

            var contentType = new Mock<IPublishedContentType>();
            contentType.SetupGet(c => c.ItemType).Returns(PublishedItemType.Media);

            var content = new Mock<IPublishedContent>();
            content.SetupGet(c => c.ContentType).Returns(contentType.Object);
            content.SetupGet(c => c.Name).Returns("Test image");
            content.SetupGet(c => c.UpdateDate).Returns(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            var properties = new Dictionary<string, object?>
            {
                [Constants.Conventions.Media.File] = cropperValue,
                [Constants.Conventions.Media.Extension] = "jpg",
                [Constants.Conventions.Media.Width] = 2000,
                [Constants.Conventions.Media.Height] = 1500,
            };

            var publishedProperties = new List<IPublishedProperty>();
            foreach (var (alias, value) in properties)
            {
                var property = new Mock<IPublishedProperty>();
                property.SetupGet(p => p.Alias).Returns(alias);
                property.Setup(p => p.HasValue(It.IsAny<string?>(), It.IsAny<string?>())).Returns(true);
                property.Setup(p => p.GetValue(It.IsAny<string?>(), It.IsAny<string?>())).Returns(value);
                property.Setup(p => p.GetSourceValue(It.IsAny<string?>(), It.IsAny<string?>())).Returns(value);
                content.Setup(c => c.GetProperty(alias)).Returns(property.Object);
                contentType.Setup(c => c.GetPropertyType(alias)).Returns(Mock.Of<IPublishedPropertyType>());
                publishedProperties.Add(property.Object);
            }

            content.SetupGet(c => c.Properties).Returns(publishedProperties);

            var fallback = new Mock<IPublishedValueFallback>();

            return new MediaWithCrops(content.Object, fallback.Object, new ImageCropperValue());
        }

        public static ImageCropperValue.ImageCropperCrop Crop(string alias, int width, int height) => new()
        {
            Alias = alias,
            Width = width,
            Height = height,
        };

        /// <summary>
        /// Renders the tag helper and returns the inner markup
        /// </summary>
        public static string Render(SlimsyPictureTagHelper tagHelper)
        {
            var output = new TagHelperOutput(
                "slimsy-picture",
                new TagHelperAttributeList(),
                (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

            var context = new TagHelperContext(
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                Guid.NewGuid().ToString());

            tagHelper.Process(context, output);

            using var writer = new StringWriter();
            output.Content.WriteTo(writer, HtmlEncoder.Default);
            return writer.ToString();
        }

        public static SlimsyService CreateService(SlimsyOptions options)
        {
            InstallUmbracoServices();

            return new SlimsyService(
                Mock.Of<IOptionsMonitor<SlimsyOptions>>(o => o.CurrentValue == options),
                Mock.Of<IUmbracoContextAccessor>(),
                null!);
        }

        public static SlimsyPictureTagHelper CreateTagHelper(SlimsyOptions options)
        {
            var optionsMonitor = Mock.Of<IOptionsMonitor<SlimsyOptions>>(o => o.CurrentValue == options);

            return new SlimsyPictureTagHelper(CreateService(options), optionsMonitor);
        }

        private static void InstallUmbracoServices()
        {
            var urlProvider = new Mock<IPublishedUrlProvider>();
            urlProvider
                .Setup(p => p.GetMediaUrl(It.IsAny<IPublishedContent>(), It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<Uri?>()))
                .Returns(MediaUrl);

            StaticServiceProvider.Instance = new MockingServiceProvider(new Dictionary<Type, object>
            {
                [typeof(IImageUrlGenerator)] = new FakeImageUrlGenerator(),
                [typeof(IPublishedUrlProvider)] = urlProvider.Object,
            });
        }
    }

    /// <summary>
    /// Umbraco's static extension classes resolve a long list of services in their static initializers,
    /// anything not explicitly registered here is satisfied with a loose mock
    /// </summary>
    internal sealed class MockingServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, object> _registered;

        public MockingServiceProvider(Dictionary<Type, object> registered) => _registered = registered;

        public object? GetService(Type serviceType)
        {
            if (_registered.TryGetValue(serviceType, out var instance))
            {
                return instance;
            }

            if (!serviceType.IsInterface)
            {
                return null;
            }

            var mock = (Mock)Activator.CreateInstance(typeof(Mock<>).MakeGenericType(serviceType))!;
            return mock.Object;
        }
    }

    /// <summary>
    /// Emits predictable urls, e.g. /media/test/image.jpg?width=180&amp;height=135&amp;format=webp
    /// </summary>
    internal sealed class FakeImageUrlGenerator : IImageUrlGenerator
    {
        public IEnumerable<string> SupportedImageFileTypes => new[] { "jpg", "png", "webp" };

        public string? GetImageUrl(ImageUrlGenerationOptions? options)
        {
            if (options == null)
            {
                return null;
            }

            var parts = new List<string>();
            if (options.Width != null) parts.Add($"width={options.Width}");
            if (options.Height != null) parts.Add($"height={options.Height}");
            if (options.Quality != null) parts.Add($"quality={options.Quality}");
            if (!string.IsNullOrEmpty(options.FurtherOptions)) parts.Add(options.FurtherOptions.TrimStart('&'));

            return options.ImageUrl + (parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty);
        }
    }
}
