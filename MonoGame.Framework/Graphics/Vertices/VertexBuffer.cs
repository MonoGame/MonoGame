// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class VertexBuffer : GraphicsResource
    {
        internal bool _isDynamic;

		public int VertexCount { get; private set; }
		public VertexDeclaration VertexDeclaration { get; private set; }
		public BufferUsage BufferUsage { get; private set; }
		
		protected VertexBuffer(GraphicsDevice graphicsDevice, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage bufferUsage, bool dynamic)
		{
			if (graphicsDevice == null)
                throw new ArgumentNullException("Graphics Device Cannot Be null");

            this.GraphicsDevice = graphicsDevice;
            this.VertexDeclaration = vertexDeclaration;
            this.VertexCount = vertexCount;
            this.BufferUsage = bufferUsage;

            // Make sure the graphics device is assigned in the vertex declaration.
            if (vertexDeclaration.GraphicsDevice != graphicsDevice)
                vertexDeclaration.GraphicsDevice = graphicsDevice;

            _isDynamic = dynamic;

            PlatformConstruct();
		}

        public VertexBuffer(GraphicsDevice graphicsDevice, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage bufferUsage) :
			this(graphicsDevice, vertexDeclaration, vertexCount, bufferUsage, false)
        {
        }
		
		public VertexBuffer(GraphicsDevice graphicsDevice, Type type, int vertexCount, BufferUsage bufferUsage) :
			this(graphicsDevice, VertexDeclaration.FromType(type), vertexCount, bufferUsage, false)
		{
        }

        /// <summary>
        /// The GraphicsDevice is resetting, so GPU resources must be recreated.
        /// </summary>
        internal protected override void GraphicsDeviceResetting()
        {
            PlatformGraphicsDeviceResetting();
        }

        public void GetData<T> (int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride) where T : struct
        {
            var elementSizeInBytes = Marshal.SizeOf(typeof(T));

            if (vertexStride == 0)
                vertexStride = elementSizeInBytes;

            if (data == null)
                throw new ArgumentNullException("data", "This method does not accept null for this parameter.");
            if (data.Length < (startIndex + elementCount))
                throw new ArgumentOutOfRangeException("elementCount", "This parameter must be a valid index within the array.");
            if (BufferUsage == BufferUsage.WriteOnly)
                throw new NotSupportedException("Calling GetData on a resource that was created with BufferUsage.WriteOnly is not supported.");
			if (elementCount > 1 && (elementCount * vertexStride) > (VertexCount * VertexDeclaration.VertexStride))
                throw new InvalidOperationException("The array is not the correct size for the amount of data requested.");

            PlatformGetData<T>(offsetInBytes, data, startIndex, elementCount, vertexStride);
        }

        public void GetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
            var elementSizeInByte = Marshal.SizeOf(typeof(T));
            this.GetData<T>(0, data, startIndex, elementCount, elementSizeInByte);
        }

        public void GetData<T>(T[] data) where T : struct
        {
            var elementSizeInByte = Marshal.SizeOf(typeof(T));
            this.GetData<T>(0, data, 0, data.Length, elementSizeInByte);
        }

        public void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride) where T : struct
        {
            SetDataInternal<T>(offsetInBytes, data, startIndex, elementCount, vertexStride, SetDataOptions.None);
        }
        		
		public void SetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
            var elementSizeInBytes = Marshal.SizeOf(typeof(T));
            SetDataInternal<T>(0, data, startIndex, elementCount, elementSizeInBytes, SetDataOptions.None);
		}
		
        public void SetData<T>(T[] data) where T : struct
        {
            var elementSizeInBytes = Marshal.SizeOf(typeof(T));
            SetDataInternal<T>(0, data, 0, data.Length, elementSizeInBytes, SetDataOptions.None);
        }

        protected void SetDataInternal<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options) where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data", "This method does not accept null for this parameter.");

            var elementSizeInBytes = Marshal.SizeOf(typeof(T));
            var bufferSize = VertexCount * VertexDeclaration.VertexStride;

            if (vertexStride == 0)
                vertexStride = elementSizeInBytes;

            if (startIndex + elementCount > data.Length || elementCount <= 0)
                throw new ArgumentOutOfRangeException("The array specified in the data parameter is not the correct size for the amount of data requested.");
            if (elementCount > 1 && (elementCount * vertexStride > bufferSize))
                throw new InvalidOperationException("The vertex stride is larger than the vertex buffer.");
            if (vertexStride < elementSizeInBytes)
                throw new ArgumentOutOfRangeException("The vertex stride must be greater than or equal to the size of the specified data (" + elementSizeInBytes + ").");            

            PlatformSetDataInternal<T>(offsetInBytes, data, startIndex, elementCount, vertexStride, options, bufferSize, elementSizeInBytes);
        }
    }
}
