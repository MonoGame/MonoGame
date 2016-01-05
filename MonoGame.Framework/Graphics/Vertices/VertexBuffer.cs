// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;

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

        public void GetData<T> (int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride) where T : struct
        {
            var elementSizeInBytes = Marshal.SizeOf(typeof(T));

            if (vertexStride == 0)
                vertexStride = elementSizeInBytes;

            if (data == null)
                throw new ArgumentNullException("data");
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
        /// <example>
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
        /// </example>
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
            var elementSizeInBytes = Marshal.SizeOf(typeof(T));
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
            var elementSizeInBytes = Marshal.SizeOf(typeof(T));
            SetDataInternal<T>(0, data, 0, data.Length, elementSizeInBytes, SetDataOptions.None);
        }

        protected void SetDataInternal<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options) where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data");

            var elementSizeInBytes = Marshal.SizeOf(typeof(T));
            var bufferSize = VertexCount * VertexDeclaration.VertexStride;

            if (vertexStride == 0)
                vertexStride = elementSizeInBytes;

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
