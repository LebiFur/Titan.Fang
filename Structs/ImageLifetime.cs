namespace Titan.Fang
{
    public readonly struct ImageLifetime(int startPassIndex, int endPassIndex)
    {
        public readonly int StartPassIndex = startPassIndex;
        public readonly int EndPassIndex = endPassIndex;

        public static bool Intersects(ImageLifetime a, ImageLifetime b)
        {
            bool aIntersects =
                (a.StartPassIndex >= b.StartPassIndex && a.StartPassIndex <= b.EndPassIndex) ||
                (a.EndPassIndex >= b.StartPassIndex && a.EndPassIndex <= b.EndPassIndex);

            bool bIntersects =
                (b.StartPassIndex >= a.StartPassIndex && b.StartPassIndex <= a.EndPassIndex) ||
                (b.EndPassIndex >= a.StartPassIndex && b.EndPassIndex <= a.EndPassIndex);

            return aIntersects || bIntersects;
        }
    }
}
