// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Sce.PlayStation.Core.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class VertexBuffer
    {
        internal Array _vertexArray;

        private void PlatformConstruct()
        {
        }

        private void PlatformGraphicsDeviceResetting()
        {
        }

        private void PlatformGetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride) where T : struct
        {
            throw new NotImplementedException();
        }

        private void PlatformSetDataInternal<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int vertexStride, SetDataOptions options, int bufferSize, int elementSizeInBytes) where T : struct
        {
            if (_vertexArray == null)
                _vertexArray = new T[VertexCount];
            Array.Copy(data, offsetInBytes / VertexDeclaration.VertexStride, _vertexArray, startIndex, elementCount);
        }

        protected override void Dispose(bool disposing)
        {
            //Do nothing
            _vertexArray = null;
            base.Dispose(disposing);
        }
    }
}
