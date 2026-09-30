namespace Titan.Fang
{
    public readonly struct ImageTransition(
        Image image,
        ImageLayout sourceLayout,
        ImageLayout destinationLayout,
        ImageLayout lastMemoryLayout)
    {
        public readonly Image Image = image;
        public readonly ImageLayout SourceLayout = sourceLayout;
        public readonly ImageLayout DestinationLayout = destinationLayout;
        public readonly ImageLayout LastMemoryLayout = lastMemoryLayout;
    }
}
