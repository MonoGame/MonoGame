// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Defines a culling mode for faces in rasterization process.
    /// </summary>
    public enum CullMode
    {
        /// <summary>
        /// Do not cull faces.
        /// </summary>
        None,
        /// <summary>
        /// Cull faces with clockwise order.
        /// </summary>
        CullClockwiseFace,
        /// <summary>
        /// Cull faces with counter clockwise order.
        /// </summary>
        CullCounterClockwiseFace
    }
}