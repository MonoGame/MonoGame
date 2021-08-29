using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public unsafe static class DefaultColorProcessors
    {
        /// <summary>
        /// Zeroes RGB of pixels having zero alpha(standard XNA behavior)
        /// </summary>
        public static readonly Action<byte[]> ZeroTransparentPixels = data =>
        {
            fixed (byte* b = &data[0])
            {
                byte* ptr = b;
                for (var i = 0; i < data.Length; i += 4, ptr += 4)
                {
                    if (ptr[3] == 0)
                    {
                        ptr[0] = ptr[1] = ptr[2] = 0;
                    }
                }
            }
        };

        /// <summary>
        /// Premultiplies RGB of pixels by its alpha
        /// </summary>
        public static readonly Action<byte[]> PremultiplyAlpha = data =>
        {
            fixed (byte* b = &data[0])
            {
                byte* ptr = b;
                for (var i = 0; i < data.Length; i += 4, ptr += 4)
                {
                    var falpha = ptr[3] / 255.0f;
                    ptr[0] = (byte)(ptr[0] * falpha);
                    ptr[1] = (byte)(ptr[1] * falpha);
                    ptr[2] = (byte)(ptr[2] * falpha);
                }
            }
        };
    }
}
