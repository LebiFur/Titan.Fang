using Silk.NET.Maths;

namespace Titan.Fang
{
    public sealed class PlacedImage : Image
    {
        public ImageLifetime? Lifetime { get; internal set; }
        public PlacedImageBucket? Bucket { get; internal set; }
        public ImageUsage Usage { get; internal set; }

        public PlacedImageHandle Handle { get; }
        public string? HandleId { get; }
        public bool NeverCull { get; }
        public bool NeverAlias { get; }

        internal PlacedImage(
            string name,
            PlacedImageHandle handle,
            string? handleId,
            ImageFormat format,
            ImageSizing sizing,
            Vector2D<uint>? constantSize,
            bool neverCull,
            bool neverAlias) : base(name, format, sizing, constantSize)
        {
            Handle = handle;
            HandleId = handleId;
            NeverCull = neverCull;
            NeverAlias = neverAlias;
        }

        public override ImageLayout GetLastMemoryLayout() => Bucket!.LastMemoryLayout;
        public override void SetLastMemoryLayout(ImageLayout layout) => Bucket!.LastMemoryLayout = layout;
    }
}
