// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using Sce.PlayStation.Core.Graphics;
using PssVertexBuffer = Sce.PlayStation.Core.Graphics.VertexBuffer;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class IndexBuffer
    {
        internal ushort[] _buffer;

        private void PlatformConstruct(IndexElementSize indexElementSize, int indexCount)
        {
            if (indexElementSize != IndexElementSize.SixteenBits)
                throw new NotImplementedException("PSS Currently only supports ushort (SixteenBits) index elements");
            _buffer = new ushort[indexCount];
        }

        private void PlatformGraphicsDeviceResetting()
        {
        }

        private void PlatformGetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
        {
            throw new NotImplementedException();
        }

        private void PlatformSetDataInternal<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
        {
            if (typeof(T) == typeof(ushort))
            {
                Array.Copy(data, offsetInBytes / sizeof(ushort), _buffer, startIndex, elementCount);
            }
            else
            {
                throw new NotImplementedException("PSS Currently only supports ushort (SixteenBits) index elements");
                //Something like as follows probably works if you really need this, but really just make a ushort array!
                /*
                int indexOffset = offsetInBytes / sizeof(T);
                for (int i = 0; i < elementCount; i++)
                    _buffer[i + startIndex] = (ushort)(object)data[i + indexOffset];
                */
            }
        }

        protected override void Dispose(bool disposing)
        {
            //Do nothing
            _buffer = null;
            base.Dispose(disposing);
        }
	}
}
