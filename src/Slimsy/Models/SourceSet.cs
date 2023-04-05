using Microsoft.AspNetCore.Html;

namespace Slimsy.Models
{
    internal class SourceSet
    {
        public IHtmlContent? Source { get; set; }
        public IHtmlContent? Lqip { get; set; }
        public string? Format { get; set; }
    }
}
