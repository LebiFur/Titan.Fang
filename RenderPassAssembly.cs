using System;
using System.Collections.Generic;
using System.Linq;

namespace Titan.Fang
{
    public sealed class RenderPassAssembly : IEquatable<RenderPassAssembly>
    {
        public int Index { get; }

        public IReadOnlyList<OutputAttachmentAssembly> OutputAttachmentAssemblies => outputAttachmentAssemblies;

        private OutputAttachmentAssembly[] outputAttachmentAssemblies;

        internal RenderPassAssembly(int index, GraphicsPass pass)
        {
            Index = index;
            outputAttachmentAssemblies = pass.OutputAttachments.Select(x => new OutputAttachmentAssembly(x)).ToArray();
        }

        public bool Equals(RenderPassAssembly? other) =>
            other is not null && outputAttachmentAssemblies.SequenceEqual(other.outputAttachmentAssemblies);

        public override bool Equals(object? obj) => obj is RenderPassAssembly assembly && Equals(assembly);
        public override int GetHashCode() => base.GetHashCode();

        public static bool operator ==(RenderPassAssembly a, RenderPassAssembly? b) => a.Equals(b);
        public static bool operator !=(RenderPassAssembly a, RenderPassAssembly? b) => !a.Equals(b);
    }
}
