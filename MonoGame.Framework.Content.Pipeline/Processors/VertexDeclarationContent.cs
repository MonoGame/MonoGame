// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    /// <summary>
    /// Provides methods and properties for maintaining the vertex declaration data of a VertexContent.
    /// </summary>
    public class VertexDeclarationContent : ContentItem
    {
        Collection<VertexElement> vertexElements;
        int? vertexStride;

        /// <summary>
        /// Gets the VertexElement object of the vertex declaration.
        /// </summary>
        /// <value>The VertexElement object of the vertex declaration.</value>
        public Collection<VertexElement> VertexElements { get { return vertexElements; } }

        /// <summary>
        /// The number of bytes from one vertex to the next.
        /// </summary>
        /// <value>The stride (in bytes).</value>
        public int? VertexStride
        {
            get { return vertexStride; }
            set { vertexStride = value; }
        }

        /// <summary>
        /// Initializes a new instance of VertexDeclarationContent.
        /// </summary>
        public VertexDeclarationContent()
        {
            vertexElements = new Collection<VertexElement>();
        }
    }
}
