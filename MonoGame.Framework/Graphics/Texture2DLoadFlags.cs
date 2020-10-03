using System;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Loading flags for Texture2D.FromStream/Texture2D.FromFile
    /// </summary>
    [Flags]
    public enum Texture2DLoadFlags
    {
        /// <summary>
        /// Do nothing
        /// </summary>
        None = 0,

        /// <summary>
        /// Zeroes RGB of pixels having zero alpha(standard XNA behavior)
        /// </summary>
        ZeroTransparentPixels = 1,

        /// <summary>
        /// Premultiplies RGB of pixels by its alpha
        /// </summary>
        PremultiplyAlpha = 2
    }
}
