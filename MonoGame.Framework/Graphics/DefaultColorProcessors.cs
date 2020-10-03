using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public static class DefaultColorProcessors
    {
        private static byte ApplyAlpha(byte color, byte alpha)
        {
            var fc = color / 255.0f;
            var fa = alpha / 255.0f;
            var fr = (int)(255.0f * fc * fa);

            return (byte)MathHelper.Clamp(fr, byte.MinValue, byte.MaxValue);
        }

        /// <summary>
        /// Zeroes RGB of pixels having zero alpha(standard XNA behavior)
        /// </summary>
        public static readonly Func<Color, Color> ZeroTransparentPixels = c =>
        {
            if (c.A == 0)
            {
                c.R = c.G = c.B = 0;
            }

            return c;
        };

        /// <summary>
        /// Premultiplies RGB of pixels by its alpha
        /// </summary>
        public static readonly Func<Color, Color> PremultiplyAlpha = c =>
        {
            c.R = ApplyAlpha(c.R, c.A);
            c.G = ApplyAlpha(c.G, c.A);
            c.B = ApplyAlpha(c.B, c.A);

            return c;
        };
    }
}
