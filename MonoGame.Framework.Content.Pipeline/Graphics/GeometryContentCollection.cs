// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides methods for maintaining a collection of geometry batches that make up a mesh.
    /// </summary>
    public sealed class GeometryContentCollection : ChildCollection<MeshContent, GeometryContent>
    {
        internal GeometryContentCollection(MeshContent parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Gets the parent of a child object.
        /// </summary>
        /// <param name="child">The child of the parent being retrieved.</param>
        /// <returns>The parent of the child object.</returns>
        protected override MeshContent GetParent(GeometryContent child)
        {
            return child.Parent;
        }

        /// <summary>
        /// Sets the parent of the specified child object.
        /// </summary>
        /// <param name="child">The child of the parent being set.</param>
        /// <param name="parent">The parent of the child object.</param>
        protected override void SetParent(GeometryContent child, MeshContent parent)
        {
            child.Parent = parent;
        }
    }
}
