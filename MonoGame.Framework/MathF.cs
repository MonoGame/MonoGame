// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace System
{
#if (!NETCOREAPP && !NETSTANDARD2_1) || NETCOREAPP1_0 || NETCOREAPP1_1

    using System.Diagnostics.Contracts;

    internal static class MathF
    {
        public const float E = (float)Math.E;
        public const float PI = (float)Math.PI;

        [Pure]
        public static float Sqrt(float f)
        {
            return (float)Math.Sqrt(f);
        }

        [Pure]
        public static float Pow(float x, float y)
        {
            return (float)Math.Pow(x, y);
        }

        [Pure]
        public static float Sin(float f)
        {
            return (float)Math.Sin(f);
        }

        [Pure]
        public static float Cos(float f)
        {
            return (float)Math.Cos(f);
        }

        [Pure]
        public static float Tan(float f)
        {
            return (float)Math.Tan(f);
        }

        [Pure]
        public static float Asin(float f)
        {
            return (float)Math.Asin(f);
        }

        [Pure]
        public static float Acos(float f)
        {
            return (float)Math.Acos(f);
        }

        [Pure]
        public static float Atan(float f)
        {
            return (float)Math.Atan(f);
        }

        [Pure]
        public static float Round(float f)
        {
            return (float)Math.Round(f);
        }

        [Pure]
        public static float Ceiling(float f)
        {
            return (float)Math.Ceiling(f);
        }

        [Pure]
        public static float Floor(float f)
        {
            return (float)Math.Floor(f);
        }
    }
#endif
}
