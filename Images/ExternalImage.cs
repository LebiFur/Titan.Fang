using Silk.NET.Maths;

namespace Titan.Fang
{
    public sealed class ExternalImage : Image
    {
        public ExternalImageHandle Handle { get; }
        public string? HandleId { get; }

        private ImageLayout lastMemoryLayout = ImageLayout.Undefined;

        internal ExternalImage(
            string name,
            ExternalImageHandle handle,
            string? handleId,
            ImageFormat format,
            ImageSizing sizing,
            Vector2D<uint>? constantSize) : base(name, format, sizing, constantSize)
        {
            Handle = handle;
            HandleId = handleId;
        }

        public override ImageLayout GetLastMemoryLayout() => lastMemoryLayout;
        public override void SetLastMemoryLayout(ImageLayout layout) => lastMemoryLayout = layout;
    }
}
