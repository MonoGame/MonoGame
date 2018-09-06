using System;

namespace Microsoft.Xna.Framework.Net
{
    [Flags]
    public enum SendDataOptions
    {
        None            = 0,
        Reliable        = 1 << 0,
        InOrder         = 1 << 1,
        ReliableInOrder = 1 << 2,
        Chat            = 1 << 3, // Force no encryption to comply with international regulations, can be combined with InOrder and should have its own channel
    }
}
