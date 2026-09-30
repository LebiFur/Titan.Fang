using System;

namespace Titan.Fang
{
    [Flags]
    public enum QueueAbilities
    {
        None = 0,
        Graphics = 1,
        Compute = 2,
        CrossQueueTransition = 4
    }
}
