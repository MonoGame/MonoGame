using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public delegate void ColorProcessorDelegate(ref byte r, ref byte g, ref byte b, ref byte a);

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
        public static readonly ColorProcessorDelegate ZeroTransparentPixels = (ref byte r, ref byte g, ref byte b, ref byte a) =>
        {
            if (a == 0)
            {
                r = g = b = 0;
            }
        };

        /// <summary>
        /// Premultiplies RGB of pixels by its alpha
        /// </summary>
        public static readonly ColorProcessorDelegate PremultiplyAlpha = (ref byte r, ref byte g, ref byte b, ref byte a) =>
        {
            r = ApplyAlpha(r, a);
            g = ApplyAlpha(g, a);
            b = ApplyAlpha(b, a);
        };
    }
}
