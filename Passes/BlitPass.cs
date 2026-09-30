using Silk.NET.Maths;
using System.Collections.Generic;

namespace Titan.Fang
{
    public sealed class BlitPass : Pass
    {
        public override QueueAbilities RequiredAbilities => QueueAbilities.Graphics;
        public override IEnumerable<Image> ReadImages => [Source];
        public override IEnumerable<Image> WrittenImages => [Destination];
        public override ImageLayout ReadLayout => ImageLayout.TransferSource;
        public override ImageLayout WriteLayout => ImageLayout.TransferDestination;
        public override ImageUsage ReadUsage => ImageUsage.TransferSource;
        public override ImageUsage WriteUsage => ImageUsage.TransferDestination;

        public Image Source { get; }
        public Image Destination { get; }
        public ImageFilter Filter { get; }
        public Rectangle<int>? SourceRegion { get; }
        public Rectangle<int>? DestinationRegion { get; }

        public BlitPass(
            string name,
            Image source,
            Image destination,
            ImageFilter filter,
            Rectangle<int>? sourceRegion,
            Rectangle<int>? destinationRegion) : base(name)
        {
            Source = source;
            Destination = destination;
            Filter = filter;
            SourceRegion = sourceRegion;
            DestinationRegion = destinationRegion;
        }
    }
}
