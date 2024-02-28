using System.Text.RegularExpressions;

namespace Slimsy.UmbracoClone;

internal static partial class RichTextParsingRegexes
{
    [GeneratedRegex("<umb-rte-block(?:-inline)?(?: class=\"(.[^\"]*)\")? data-content-udi=\"(?<udi>.[^\"]*)\"><!--Umbraco-Block--><\\/umb-rte-block(?:-inline)?>")]
    public static partial Regex BlockRegex();
}