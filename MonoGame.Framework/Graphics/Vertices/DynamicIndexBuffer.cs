// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
	public class DynamicIndexBuffer : IndexBuffer
	{
        /// <summary>
        /// Special offset used internally by GraphicsDevice.DrawUserXXX() methods.
        /// </summary>
        internal int UserOffset;

        public bool IsContentLost { get { return false; } }
		
		public DynamicIndexBuffer(GraphicsDevice graphicsDevice, IndexElementSize indexElementSize, int indexCount, BufferUsage usage) :
			base(graphicsDevice, indexElementSize, indexCount, usage, true)
		{
		}

   		public DynamicIndexBuffer(GraphicsDevice graphicsDevice, Type indexType, int indexCount, BufferUsage usage) :
            base(graphicsDevice, indexType, indexCount, usage, true)
        {
        }

        public void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
        {
            base.SetDataInternal<T>(offsetInBytes, data, startIndex, elementCount, options);
        }

        public void SetData<T>(T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
        {
            base.SetDataInternal<T>(0, data, startIndex, elementCount, options);
        }
    }
}

