// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Defines when render target data is preserved.
    /// </summary>
    public enum RenderTargetUsage
    {
        /// <summary>
        /// The render target content will not be preserved.
        /// </summary>
        DiscardContents,
        /// <summary>
        /// The render target content will be preserved.
        /// </summary>
        PreserveContents,
        /// <summary>
        /// The render target content might be preserved if the platform can do so without a penalty in performance or memory usage.
        /// </summary>
        PlatformContents
    }
}

