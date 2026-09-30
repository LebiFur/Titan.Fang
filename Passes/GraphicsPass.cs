using System;
using System.Collections.Generic;
using System.Linq;

namespace Titan.Fang
{
    public sealed class GraphicsPass : Pass
    {
        public override QueueAbilities RequiredAbilities => QueueAbilities.Graphics;
        public override IEnumerable<Image> ReadImages => inputImages;
        public override IEnumerable<Image> WrittenImages => outputAttachments.Select(x => x.Image);
        public override ImageLayout ReadLayout => ImageLayout.Read;
        public override ImageLayout WriteLayout => ImageLayout.Write;
        public override ImageUsage ReadUsage => ImageUsage.Read;
        public override ImageUsage WriteUsage => ImageUsage.Write;

        public RenderPassAssembly? Assembly { get; internal set; }

        public GraphicsPassHandle Handle { get; }
        public string? HandleId { get; }

        public IReadOnlyList<OutputAttachment> OutputAttachments => outputAttachments;
        public IReadOnlyList<Image> InputImages => inputImages;

        private readonly OutputAttachment[] outputAttachments;
        private readonly Image[] inputImages;

        public GraphicsPass(
            string name,
            GraphicsPassHandle handle,
            string? handleId,
            IEnumerable<OutputAttachment> outputAttachments,
            IEnumerable<Image> inputImages) : base(name)
        {
            if (!outputAttachments.TryGetNonEnumeratedCount(out int outputCount)) outputCount = outputAttachments.Count();

            if (outputCount == 0) throw new Exception("No output attachments");

            if (!inputImages.TryGetNonEnumeratedCount(out int inputCount)) inputCount = inputImages.Count();

            Handle = handle;
            HandleId = handleId;

            this.outputAttachments = new OutputAttachment[outputCount];
            this.inputImages = new Image[inputCount];

            int i = 0;
            foreach (OutputAttachment outputAttachment in outputAttachments)
            {
                this.outputAttachments[i] = outputAttachment;
                i++;
            }

            i = 0;
            foreach (Image inputImage in inputImages)
            {
                this.inputImages[i] = inputImage;
                i++;
            }
        }
    }
}
