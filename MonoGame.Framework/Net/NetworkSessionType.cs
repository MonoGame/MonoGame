using System;

namespace Microsoft.Xna.Framework.Net
{
    public enum NetworkSessionType : byte
    {
        Local,
        SystemLink,
        PlayerMatch,
        Ranked
    }
}
