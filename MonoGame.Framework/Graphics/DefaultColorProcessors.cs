using System;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Represents the default processors used in processing color values.
    /// This class cannot be inherited.
    /// </summary>
    public unsafe static class DefaultColorProcessors
    {
        /// <summary>
        /// Sets the RGB component values of each color to zero if the alpha component is zero.
        /// </summary>
        /// <remarks>
        /// This is standard XNA behavior.
        /// </remarks>
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
        /// Premultiplies the RGB component value of each color by the its alpha component.
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
