// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
#pragma warning disable 1574 // disabling ambigious overload of <see cref="GraphicsDevice.Clear"/> warning
    /// Defines the buffers for clearing when calling <see cref="GraphicsDevice.Clear"/> operation.
#pragma warning restore 1574 // restoring warning behaviour
    /// </summary>
    [Flags]
    public enum ClearOptions
    {
        /// <summary>
        /// Render target buffer.
        /// </summary>
		Target = 1,
        /// <summary>
        /// Depth buffer.
        /// </summary>
        DepthBuffer = 2,
        /// <summary>
        /// Stencil buffer.
        /// </summary>
        Stencil = 4        
    }
}

