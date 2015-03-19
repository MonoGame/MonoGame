// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Interface which must be implemented for vertex types.
    /// </summary>
    public interface IVertexType
    {
        /// <summary>
        /// Returns a vertex declaration for per-vertex data.
        /// </summary>
        VertexDeclaration VertexDeclaration
        {
            get;
        }
    }
}