using System;
using System.Collections.Generic;
using System.Linq;

namespace Titan.Fang
{
    public sealed class PlacedImageBucket
    {
        public int Index { get; }

        public IReadOnlyList<PlacedImage> Images => bucket;

        internal ImageLayout LastMemoryLayout { get; set; } = ImageLayout.Undefined;

        private PlacedImage[] bucket;

        internal PlacedImageBucket(int index, IEnumerable<PlacedImage> images)
        {
            if (!images.TryGetNonEnumeratedCount(out int count)) count = images.Count();

            if (count == 0) throw new Exception("No images");

            Index = index;
            bucket = new PlacedImage[count];

            int i = 0;
            foreach (PlacedImage image in images)
            {
                bucket[i] = image;
                i++;
            }
        }
    }
}
