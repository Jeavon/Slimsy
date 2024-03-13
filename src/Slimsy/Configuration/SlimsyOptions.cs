using System;

namespace Slimsy.Configuration
{
    public sealed class SlimsyOptions
    {
        public string Format { get; set; } = null!;
        public string BackgroundColor { get; set; } = null!;
        public int DefaultQuality { get; set; } = 90!;
        public int MaxWidth { get; set; } = 2048!;
        public int WidthStep { get; set; } = 160!;
        public bool UseCropAsSrc { get; set; }
        public TagHelper TagHelper { get; set; } = new TagHelper();
        public string? ForceRefresh { get; set; } = null;
        public bool EncodeCommas { get; set; } = false;
        public bool AddSourceDimensions { get; set; } = false;
    }

    public sealed class TagHelper
    {
        public string[]? SingleSources { get; set; } = Array.Empty<string>();
        public string[]? PictureSources { get; set; } = Array.Empty<string>();
        public PictureSource[] DefaultPictureSources { get; set; } = Array.Empty<PictureSource>();
        public bool ImageDimensions { get; set; } = false;
    }
    public sealed class PictureSource
    {
        public string Extension { get; set; } = null!;
        public int Quality { get; set; } = 90;
    }
}
