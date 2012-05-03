using System;

using PssVector4 = Sce.Pss.Core.Vector4;

namespace Microsoft.Xna.Framework
{
    public static class PSSExtensions
    {
        public static PssVector4 ToPssVector4(this Vector4 v)
        {
            return new PssVector4(v.X, v.Y, v.Z, v.W);
        }
    }
}

