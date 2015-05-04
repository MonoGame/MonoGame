// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides methods for accessing a mipmap chain.
    /// </summary>
    public sealed class MipmapChain : Collection<BitmapContent>
    {
        /// <summary>
        /// Initializes a new instance of MipmapChain.
        /// </summary>
        public MipmapChain()
        {
        }

        /// <summary>
        /// Initializes a new instance of MipmapChain with the specified mipmap.
        /// </summary>
        /// <param name="bitmap"></param>
        public MipmapChain(BitmapContent bitmap)
        {
            Add(bitmap);
        }

        /// <summary>
        /// Constructs a new mipmap chain containing the specified bitmap.
        /// </summary>
        /// <param name="bitmap">Bitmap used for the mipmap chain.</param>
        /// <returns>Resultant mipmap chain.</returns>
        public static implicit operator MipmapChain(BitmapContent bitmap)
        {
            return new MipmapChain(bitmap);
        }
    }
}
