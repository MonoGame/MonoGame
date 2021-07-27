// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    internal enum BufferType
    {
        StructuredBuffer,
        VertexBuffer,
        IndexBuffer,
        IndirectArgumentsBuffer,
    }

    public partial class BufferResource : ShaderResource
    {
        internal readonly bool _isDynamic;

        internal int ElementCount { get; private set; }

        internal int ElementStride { get; private set; }

        internal BufferType BufferType { get; private set; }

        public BufferUsage BufferUsage { get; private set; }

        internal BufferResource(GraphicsDevice graphicsDevice, int elementCount, int elementStride, BufferUsage bufferUsage, bool dynamic, BufferType bufferType, ShaderAccess shaderAccess) :
            base(shaderAccess)
		{
		    if (graphicsDevice == null)
		    {
		        throw new ArgumentNullException("graphicsDevice", FrameworkResources.ResourceCreationWhenDeviceIsNull);
		    }
		    this.GraphicsDevice = graphicsDevice;
            this.ElementCount = elementCount;
            this.ElementStride = elementStride;
            this.BufferType = bufferType;
            this.BufferUsage = bufferUsage;

            _isDynamic = dynamic;

            PlatformConstruct();
		}

        /// <summary>
        /// The GraphicsDevice is resetting, so GPU resources must be recreated.
        /// </summary>
        internal protected override void GraphicsDeviceResetting()
        {
            PlatformGraphicsDeviceResetting();
        }
    }
}
