// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides a base class for graphics types that define local coordinate systems.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Node '{Name}'")]
    public class NodeContent : ContentItem
    {
        /// <summary>
        /// Gets the value of the local Transform property, multiplied by the AbsoluteTransform of the parent.
        /// </summary>
        public Matrix AbsoluteTransform => Parent != null ? Transform * Parent.AbsoluteTransform : Transform;

        /// <summary>
        /// Gets the set of animations belonging to this node.
        /// </summary>
        public AnimationContentDictionary Animations { get; }

        /// <summary>
        /// Gets the children of the NodeContent object.
        /// </summary>
        public NodeContentCollection Children { get; }

        /// <summary>
        /// Gets the parent of this NodeContent object.
        /// </summary>
        public NodeContent? Parent { get; set; }

        /// <summary>
        /// Gets the transform matrix of the scene.
        /// The transform matrix defines a local coordinate system for the content in addition to any children of this object.
        /// </summary>
        public Matrix Transform { get; set; }

        /// <summary>
        /// Creates an instance of NodeContent.
        /// </summary>
        public NodeContent()
        {
            Animations = [];
            Children = new NodeContentCollection(this);
            Transform = Matrix.Identity;
        }
    }
}
