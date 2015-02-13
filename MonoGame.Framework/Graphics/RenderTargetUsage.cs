// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Defines how render target data is used once a new render target is set.
    /// </summary>
    public enum RenderTargetUsage
    {
        /// <summary>
        /// Always clears the render target data.
        /// </summary>
        DiscardContents,
        /// <summary>
        /// Clears or keeps render target data depending of platform. 
        /// </summary>
        PreserveContents,
        /// <summary>
        /// Always keeps the render target data.
        /// </summary>
        PlatformContents
    }
}

