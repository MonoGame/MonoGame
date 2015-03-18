// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Defines special usage for the graphics buffers.
    /// </summary>
    public enum BufferUsage
    {
        /// <summary>
        /// No special usage.
        /// </summary>
        None,
        /// <summary>
        /// Buffer will not be readable and will fails if reading occurs, but it will have better performance than <see cref="BufferUsage.None"/>.
        /// </summary>
        WriteOnly
    }
}