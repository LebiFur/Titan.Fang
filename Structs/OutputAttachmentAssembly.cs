using System;

namespace Titan.Fang
{
    public readonly struct OutputAttachmentAssembly : IEquatable<OutputAttachmentAssembly>
    {
        public readonly ImageFormat Format;
        public readonly LoadOperation LoadOperation;
        public readonly StoreOperation StoreOperation;

        public OutputAttachmentAssembly(OutputAttachment attachment)
        {
            Format = attachment.Image.Format;
            LoadOperation = attachment.LoadOperation;
            StoreOperation = attachment.StoreOperation;
        }

        public bool Equals(OutputAttachmentAssembly other) =>
            Format == other.Format && LoadOperation == other.LoadOperation && StoreOperation == other.StoreOperation;

        public override bool Equals(object? obj) => obj is OutputAttachmentAssembly assembly && Equals(assembly);
        public override int GetHashCode() => HashCode.Combine(Format, LoadOperation, StoreOperation);

        public static bool operator ==(OutputAttachmentAssembly a, OutputAttachmentAssembly b) => a.Equals(b);
        public static bool operator !=(OutputAttachmentAssembly a, OutputAttachmentAssembly b) => !a.Equals(b);
    }
}
