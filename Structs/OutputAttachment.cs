namespace Titan.Fang
{
    public readonly struct OutputAttachment(Image image, LoadOperation loadOperation, StoreOperation storeOperation)
    {
        public readonly Image Image = image;
        public readonly LoadOperation LoadOperation = loadOperation;
        public readonly StoreOperation StoreOperation = storeOperation;
    }
}
