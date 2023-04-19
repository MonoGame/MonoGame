// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class IndexBuffer : BufferResource
    {
        public int IndexCount { get { return ElementCount; } }

        public IndexElementSize IndexElementSize { get { return ElementStride == 4 ? IndexElementSize.ThirtyTwoBits : IndexElementSize.SixteenBits; } }

        protected IndexBuffer(GraphicsDevice graphicsDevice, IndexElementSize indexElementSize, int indexCount, BufferUsage usage, bool dynamic, ShaderAccess shaderAccess) :
            base(graphicsDevice, indexCount, indexElementSize == IndexElementSize.ThirtyTwoBits ? 4 : 2, usage, dynamic, BufferType.IndexBuffer, shaderAccess)
        {
        }

        protected IndexBuffer(GraphicsDevice graphicsDevice, Type indexType, int indexCount, BufferUsage usage, bool dynamic) :
            this(graphicsDevice, SizeForType(graphicsDevice, indexType), indexCount, usage, dynamic)
        {
        }

        protected IndexBuffer(GraphicsDevice graphicsDevice, IndexElementSize indexElementSize, int indexCount, BufferUsage usage, bool dynamic) :
            this(graphicsDevice, indexElementSize, indexCount, usage, dynamic, ShaderAccess.None)
        {
        }

        public IndexBuffer(GraphicsDevice graphicsDevice, IndexElementSize indexElementSize, int indexCount, BufferUsage usage) :
            this(graphicsDevice, indexElementSize, indexCount, usage, false)
        {
        }

        public IndexBuffer(GraphicsDevice graphicsDevice, Type indexType, int indexCount, BufferUsage usage) :
            this(graphicsDevice, indexType, indexCount, usage, false)
        {
        }

        public IndexBuffer(GraphicsDevice graphicsDevice, IndexElementSize indexElementSize, int indexCount, BufferUsage usage, ShaderAccess shaderAccess) :
            this(graphicsDevice, indexElementSize, indexCount, usage, false, shaderAccess)
        {
        }

        public IndexBuffer(GraphicsDevice graphicsDevice, Type indexType, int indexCount, BufferUsage usage, ShaderAccess shaderAccess) :
            this(graphicsDevice, SizeForType(graphicsDevice, indexType), indexCount, usage, false, shaderAccess)
        {
        }

        /// <summary>
        /// Gets the relevant IndexElementSize enum value for the given type.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device.</param>
        /// <param name="type">The type to use for the index buffer</param>
        /// <returns>The IndexElementSize enum value that matches the type</returns>
        static IndexElementSize SizeForType(GraphicsDevice graphicsDevice, Type type)
        {
            switch (ReflectionHelpers.ManagedSizeOf(type))
            {
                case 2:
                    return IndexElementSize.SixteenBits;
                case 4:
                    if (graphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
                        throw new NotSupportedException("The profile does not support an elementSize of IndexElementSize.ThirtyTwoBits; use IndexElementSize.SixteenBits or a type that has a size of two bytes.");
                    return IndexElementSize.ThirtyTwoBits;
                default:
                    throw new ArgumentOutOfRangeException("type","Index buffers can only be created for types that are sixteen or thirty two bits in length");
            }
        }

        public void GetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (data.Length < (startIndex + elementCount))
                throw new InvalidOperationException("The array specified in the data parameter is not the correct size for the amount of data requested.");
            if (BufferUsage == BufferUsage.WriteOnly)
                throw new NotSupportedException("This IndexBuffer was created with a usage type of BufferUsage.WriteOnly. Calling GetData on a resource that was created with BufferUsage.WriteOnly is not supported.");

            PlatformGetData<T>(offsetInBytes, data, startIndex, elementCount, ElementStride);
        }

        public void GetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
            this.GetData<T>(0, data, startIndex, elementCount);
        }

        public void GetData<T>(T[] data) where T : struct
        {
            this.GetData<T>(0, data, 0, data.Length);
        }

        public void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
        {
            SetDataInternal<T>(offsetInBytes, data, startIndex, elementCount, SetDataOptions.None);
        }
        		
		public void SetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
            SetDataInternal<T>(0, data, startIndex, elementCount, SetDataOptions.None);
		}
		
        public void SetData<T>(T[] data) where T : struct
        {
            SetDataInternal<T>(0, data, 0, data.Length, SetDataOptions.None);
        }

        protected void SetDataInternal<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
        {
            var elementSizeInBytes = ReflectionHelpers.SizeOf<T>.Get();

            if (data == null)
                throw new ArgumentNullException("data");
            if (data.Length < (startIndex + elementCount))
                throw new InvalidOperationException("The array specified in the data parameter is not the correct size for the amount of data requested.");
            if (ElementStride < elementSizeInBytes)
                throw new ArgumentOutOfRangeException("The index stride must be greater than or equal to the size of the specified data (" + elementSizeInBytes + ").");    

            PlatformSetData<T>(offsetInBytes, data, startIndex, elementCount, elementSizeInBytes, options, ElementCount * ElementStride, elementSizeInBytes);
        }
	}
}
