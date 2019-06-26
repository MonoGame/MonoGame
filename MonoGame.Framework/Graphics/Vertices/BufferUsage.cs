// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// A usage hint for optimizing memory placement of graphics buffers.
    /// </summary>
    public enum BufferUsage
    {
        /// <summary>
        /// No special usage.
        /// </summary>
        None,
        /// <summary>
        /// The buffer will not be readable and will be optimized for rendering and writing.
        /// </summary>
        WriteOnly
    }
}