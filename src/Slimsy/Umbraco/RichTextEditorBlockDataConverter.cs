using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;

namespace Slimsy.UmbracoClone;

/// <summary>
///     Data converter for blocks in the richtext property editor
/// </summary>
internal sealed class RichTextEditorBlockDataConverter : BlockEditorDataConverter
{
    public RichTextEditorBlockDataConverter()
        : base(Constants.PropertyEditors.Aliases.TinyMce)
    {
    }

    protected override IEnumerable<ContentAndSettingsReference>? GetBlockReferences(JToken jsonLayout)
    {
        IEnumerable<RichTextBlockLayoutItem>? blockListLayout = jsonLayout.ToObject<IEnumerable<RichTextBlockLayoutItem>>();
        return blockListLayout?.Select(x => new ContentAndSettingsReference(x.ContentUdi, x.SettingsUdi)).ToList();
    }
}