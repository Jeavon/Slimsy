using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Slimsy.UmbracoClone;

internal static class DeliveryApiBlockReferenceExtensions
{
    internal static ApiBlockItem CreateApiBlockItem(
        this IBlockReference<IPublishedElement, IPublishedElement> blockItem,
        IApiElementBuilder apiElementBuilder)
        => new ApiBlockItem(
            apiElementBuilder.Build(blockItem.Content),
            blockItem.Settings != null ? apiElementBuilder.Build(blockItem.Settings) : null);
}