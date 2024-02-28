using Umbraco.Cms.Core.Models.Blocks;

namespace Slimsy.UmbracoClone
{
    public interface IRichTextEditorIntermediateValue
    {
        public string Markup { get; }

        public RichTextBlockModel? RichTextBlockModel { get; }
    }
}
