// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using MonoGame.Utilities;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class VertexBuffer : GraphicsResource
    {
        private readonly bool _isDynamic;

		public int VertexCount { get; private set; }
		public VertexDeclaration VertexDeclaration { get; private set; }
		public BufferUsage BufferUsage { get; private set; }
		
		protected VertexBuffer(GraphicsDevice graphicsDevice, VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage bufferUsage, bool dynamic)
		{
		    if (graphicsDevice == null)
		    {
		        throw new ArgumentNullException("graphicsDevice", FrameworkResources.ResourceCreationWhenDeviceIsNull);
		    }
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

        /// <summary>
        /// Get the vertex data froom this VertexBuffer.
        /// </summary>
        /// <typeparam name="T">The struct you want to fill.</typeparam>
        /// <param name="offsetInBytes">The offset to the first element in the vertex buffer in bytes.</param>
        /// <param name="data">An array of T's to be filled.</param>
        /// <param name="startIndex">The index to start filling the data array.</param>
        /// <param name="elementCount">The number of T's to get.</param>
        /// <param name="vertexStride">The size of how a vertex buffer element should be interpreted.</param>
        ///
        /// <remarks>
        /// Note that this pulls data from VRAM into main memory and because of that is a very expensive operation.
        /// It is often a better idea to keep a copy of the data in main memory.
        /// </remarks>
        ///
        /// <remarks>
        /// <p>Using this operation it is easy to get certain vertex elements from a VertexBuffer.</p>
        /// <p>
        /// For example to get the texture coordinates from a VertexBuffer of <see cref="VertexPositionTexture"/> you can call 
        /// GetData(4 * 3, data, elementCount, 20). 'data'should be an array of <see cref="Vector2"/> in this example.
        /// The offsetInBytes is the number of bytes taken up by the <see cref="VertexPositionTexture.Position"/> of the vertex.
        /// For vertexStride we pass the size of a <see cref="VertexPositionTexture"/>.
        /// </p>
        /// </remarks>
        public void GetData<T> (int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride = 0) where T : struct
        {
            var elementSizeInBytes = ReflectionHelpers.SizeOf<T>.Get();
            if (vertexStride == 0)
                vertexStride = elementSizeInBytes;

            var vertexByteSize = VertexCount * VertexDeclaration.VertexStride;
            if (vertexStride > vertexByteSize)
                throw new ArgumentOutOfRangeException("vertexStride", "Vertex stride can not be larger than the vertex buffer size.");

            if (data == null)
                throw new ArgumentNullException("data");
            if (data.Length < (startIndex + elementCount))
                throw new ArgumentOutOfRangeException("elementCount", "This parameter must be a valid index within the array.");
            if (BufferUsage == BufferUsage.WriteOnly)
                throw new NotSupportedException("Calling GetData on a resource that was created with BufferUsage.WriteOnly is not supported.");
			if (elementCount > 1 && elementCount * vertexStride > vertexByteSize)
                throw new InvalidOperationException("The array is not the correct size for the amount of data requested.");

            PlatformGetData<T>(offsetInBytes, data, startIndex, elementCount, vertexStride);
        }

        public void GetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
            this.GetData<T>(0, data, startIndex, elementCount, 0);
        }

        public void GetData<T>(T[] data) where T : struct
        {
            var elementSizeInByte = ReflectionHelpers.SizeOf<T>.Get();
            this.GetData<T>(0, data, 0, data.Length, elementSizeInByte);
        }

        /// <summary>
        /// Sets the vertex buffer data, specifying the index at which to start copying from the source data array,
        /// the number of elements to copy from the source data array, 
        /// and how far apart elements from the source data array should be when they are copied into the vertex buffer.
        /// </summary>
        /// <typeparam name="T">Type of elements in the data array.</typeparam>
        /// <param name="offsetInBytes">Offset in bytes from the beginning of the vertex buffer to the start of the copied data.</param>
        /// <param name="data">Data array.</param>
        /// <param name="startIndex">Index at which to start copying from <paramref name="data"/>.
        /// Must be within the <paramref name="data"/> array bounds.</param>
        /// <param name="elementCount">Number of elements to copy from <paramref name="data"/>.
        /// The combination of <paramref name="startIndex"/> and <paramref name="elementCount"/> 
        /// must be within the <paramref name="data"/> array bounds.</param>
        /// <param name="vertexStride">Specifies how far apart, in bytes, elements from <paramref name="data"/> should be when 
        /// they are copied into the vertex buffer.
        /// In almost all cases this should be <c>sizeof(T)</c>, to create a tightly-packed vertex buffer.
        /// If you specify <c>sizeof(T)</c>, elements from <paramref name="data"/> will be copied into the 
        /// vertex buffer with no padding between each element.
        /// If you specify a value greater than <c>sizeof(T)</c>, elements from <paramref name="data"/> will be copied 
        /// into the vertex buffer with padding between each element.
        /// If you specify <c>0</c> for this parameter, it will be treated as if you had specified <c>sizeof(T)</c>.
        /// With the exception of <c>0</c>, you must specify a value greater than or equal to <c>sizeof(T)</c>.</param>
        /// <remarks>
        /// If <c>T</c> is <c>VertexPositionTexture</c>, but you want to set only the position component of the vertex data,
        /// you would call this method as follows:
        /// <code>
        /// Vector3[] positions = new Vector3[numVertices];
        /// vertexBuffer.SetData(0, positions, 0, numVertices, vertexBuffer.VertexDeclaration.VertexStride);
        /// </code>
        /// 
        /// Continuing from the previous example, if you want to set only the texture coordinate component of the vertex data,
        /// you would call this method as follows (note the use of <paramref name="offsetInBytes"/>:
        /// <code>
        /// Vector2[] texCoords = new Vector2[numVertices];
        /// vertexBuffer.SetData(12, texCoords, 0, numVertices, vertexBuffer.VertexDeclaration.VertexStride);
        /// </code>
        /// </remarks>
        /// <remarks>
        /// If you provide a <c>byte[]</c> in the <paramref name="data"/> parameter, then you should almost certainly
        /// set <paramref name="vertexStride"/> to <c>1</c>, to avoid leaving any padding between the <c>byte</c> values
        /// when they are copied into the vertex buffer.
        /// </remarks>
        public void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride) where T : struct
        {
            SetDataInternal<T>(offsetInBytes, data, startIndex, elementCount, vertexStride, SetDataOptions.None);
        }

        /// <summary>
        /// Sets the vertex buffer data, specifying the index at which to start copying from the source data array,
        /// and the number of elements to copy from the source data array. This is the same as calling 
        /// <see cref="SetData{T}(int, T[], int, int, int)"/>  with <c>offsetInBytes</c> equal to <c>0</c>,
        /// and <c>vertexStride</c> equal to <c>sizeof(T)</c>.
        /// </summary>
        /// <typeparam name="T">Type of elements in the data array.</typeparam>
        /// <param name="data">Data array.</param>
        /// <param name="startIndex">Index at which to start copying from <paramref name="data"/>.
        /// Must be within the <paramref name="data"/> array bounds.</param>
        /// <param name="elementCount">Number of elements to copy from <paramref name="data"/>.
        /// The combination of <paramref name="startIndex"/> and <paramref name="elementCount"/> 
        /// must be within the <paramref name="data"/> array bounds.</param>
		public void SetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
            var elementSizeInBytes = ReflectionHelpers.SizeOf<T>.Get();
            SetDataInternal<T>(0, data, startIndex, elementCount, elementSizeInBytes, SetDataOptions.None);
		}
		
        /// <summary>
        /// Sets the vertex buffer data. This is the same as calling <see cref="SetData{T}(int, T[], int, int, int)"/> 
        /// with <c>offsetInBytes</c> and <c>startIndex</c> equal to <c>0</c>, <c>elementCount</c> equal to <c>data.Length</c>, 
        /// and <c>vertexStride</c> equal to <c>sizeof(T)</c>.
        /// </summary>
        /// <typeparam name="T">Type of elements in the data array.</typeparam>
        /// <param name="data">Data array.</param>
        public void SetData<T>(T[] data) where T : struct
        {
            var elementSizeInBytes = ReflectionHelpers.SizeOf<T>.Get();
            SetDataInternal<T>(0, data, 0, data.Length, elementSizeInBytes, SetDataOptions.None);
        }

        protected void SetDataInternal<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options) where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data");

            var elementSizeInBytes = ReflectionHelpers.SizeOf<T>.Get();
            var bufferSize = VertexCount * VertexDeclaration.VertexStride;

            if (vertexStride == 0)
                vertexStride = elementSizeInBytes;

            var vertexByteSize = VertexCount * VertexDeclaration.VertexStride;
            if (vertexStride > vertexByteSize)
                throw new ArgumentOutOfRangeException("vertexStride", "Vertex stride can not be larger than the vertex buffer size.");

            if (startIndex + elementCount > data.Length || elementCount <= 0)
                throw new ArgumentOutOfRangeException("data","The array specified in the data parameter is not the correct size for the amount of data requested.");
            if (elementCount > 1 && (elementCount * vertexStride > bufferSize))
                throw new InvalidOperationException("The vertex stride is larger than the vertex buffer.");
            if (vertexStride < elementSizeInBytes)
                throw new ArgumentOutOfRangeException("The vertex stride must be greater than or equal to the size of the specified data (" + elementSizeInBytes + ").");            

            PlatformSetDataInternal<T>(offsetInBytes, data, startIndex, elementCount, vertexStride, options, bufferSize, elementSizeInBytes);
        }
    }
}
