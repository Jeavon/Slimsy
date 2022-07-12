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

    }
}
