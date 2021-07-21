// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class VertexBuffer : BufferResource
    {
        public int VertexCount { get { return ElementCount; } }

        public VertexDeclaration VertexDeclaration { get; private set; }

        protected VertexBuffer(GraphicsDevice graphicsDevice, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage bufferUsage, Options bufferOptions) :
            base(graphicsDevice, vertexCount, vertexDeclaration.VertexStride, bufferUsage, bufferOptions)
        {
            this.VertexDeclaration = vertexDeclaration;

            // Make sure the graphics device is assigned in the vertex declaration.
            if (vertexDeclaration.GraphicsDevice != graphicsDevice)
                vertexDeclaration.GraphicsDevice = graphicsDevice;
        }

        public VertexBuffer(GraphicsDevice graphicsDevice, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage bufferUsage) :
           this(graphicsDevice, vertexDeclaration, vertexCount, bufferUsage, Options.BufferVertex)
        {
        }

        public VertexBuffer(GraphicsDevice graphicsDevice, Type type, int vertexCount, BufferUsage bufferUsage) :
            this(graphicsDevice, VertexDeclaration.FromType(type), vertexCount, bufferUsage, Options.BufferVertex)
        {
        }
    }
}
