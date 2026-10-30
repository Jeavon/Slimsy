using Microsoft.AspNetCore.Html;

namespace Slimsy.Models
{
    internal class SourceSet
    {
        public IHtmlContent? Source { get; set; }
        public IHtmlContent? Lqip { get; set; }
        public string? Format { get; set; }
        public bool IsMobileSource { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
    }
}
