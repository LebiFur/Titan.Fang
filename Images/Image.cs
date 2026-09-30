using Silk.NET.Maths;

namespace Titan.Fang
{
    public abstract class Image
    {
        public string Name { get; }
        public ImageFormat Format { get; }
        public ImageSizing Sizing { get; }
        public Vector2D<uint>? ConstantSize { get; }

        protected Image(
            string name,
            ImageFormat format,
            ImageSizing sizing,
            Vector2D<uint>? constantSize)
        {
            Name = name;
            Format = format;
            Sizing = sizing;
            ConstantSize = constantSize;
        }

        public abstract ImageLayout GetLastMemoryLayout();
        public abstract void SetLastMemoryLayout(ImageLayout layout);
    }
}
