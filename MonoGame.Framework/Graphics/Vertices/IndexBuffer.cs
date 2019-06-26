// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using MonoGame.Utilities;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class IndexBuffer : GraphicsResource
    {
        private readonly bool _isDynamic;

        public BufferUsage BufferUsage { get; private set; }
        public int IndexCount { get; private set; }
        public IndexElementSize IndexElementSize { get; private set; }

   		protected IndexBuffer(GraphicsDevice graphicsDevice, Type indexType, int indexCount, BufferUsage usage, bool dynamic)
            : this(graphicsDevice, SizeForType(graphicsDevice, indexType), indexCount, usage, dynamic)
        {
        }

		protected IndexBuffer(GraphicsDevice graphicsDevice, IndexElementSize indexElementSize, int indexCount, BufferUsage usage, bool dynamic)
        {
			if (graphicsDevice == null)
            {
                throw new ArgumentNullException("graphicsDevice", FrameworkResources.ResourceCreationWhenDeviceIsNull);
            }
			this.GraphicsDevice = graphicsDevice;
			this.IndexElementSize = indexElementSize;	
            this.IndexCount = indexCount;
            this.BufferUsage = usage;
			
            _isDynamic = dynamic;

            PlatformConstruct(indexElementSize, indexCount);
		}

		public IndexBuffer(GraphicsDevice graphicsDevice, IndexElementSize indexElementSize, int indexCount, BufferUsage bufferUsage) :
			this(graphicsDevice, indexElementSize, indexCount, bufferUsage, false)
		{
		}

		public IndexBuffer(GraphicsDevice graphicsDevice, Type indexType, int indexCount, BufferUsage usage) :
			this(graphicsDevice, SizeForType(graphicsDevice, indexType), indexCount, usage, false)
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

        /// <summary>
        /// The GraphicsDevice is resetting, so GPU resources must be recreated.
        /// </summary>
        internal protected override void GraphicsDeviceResetting()
        {
            PlatformGraphicsDeviceResetting();
        }

        public void GetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (data.Length < (startIndex + elementCount))
                throw new InvalidOperationException("The array specified in the data parameter is not the correct size for the amount of data requested.");
            if (BufferUsage == BufferUsage.WriteOnly)
                throw new NotSupportedException("This IndexBuffer was created with a usage type of BufferUsage.WriteOnly. Calling GetData on a resource that was created with BufferUsage.WriteOnly is not supported.");

            PlatformGetData<T>(offsetInBytes, data, startIndex, elementCount);
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
            if (data == null)
                throw new ArgumentNullException("data");
            if (data.Length < (startIndex + elementCount))
                throw new InvalidOperationException("The array specified in the data parameter is not the correct size for the amount of data requested.");

            PlatformSetDataInternal<T>(offsetInBytes, data, startIndex, elementCount, options);
        }
	}
}
