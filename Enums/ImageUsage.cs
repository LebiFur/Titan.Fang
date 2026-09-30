using System;

namespace Titan.Fang
{
    [Flags]
    public enum ImageUsage
    {
        None = 0,
        Read = 1,
        Write = 2,
        TransferSource = 4,
        TransferDestination = 8
    }
}
