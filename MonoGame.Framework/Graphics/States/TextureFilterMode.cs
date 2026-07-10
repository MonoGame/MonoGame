// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Represents the texture filtering mode used for texture sampling.
    /// </summary>
    public enum TextureFilterMode
    {
        /// <summary>
        /// The default texture filtering mode, which performs standard texture sampling without comparison.
        /// </summary>
        Default,

        /// <summary>
        /// The comparison texture filtering mode, which performs a comparison operation between the sampled texture
        /// value and a reference value during texture sampling.
        /// </summary>
        Comparison
    }
}
