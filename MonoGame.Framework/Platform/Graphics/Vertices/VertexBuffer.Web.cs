// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class VertexBuffer
    {
        private void PlatformConstruct()
        {
            throw new NotImplementedException();
        }

        private void PlatformGetData<T>(
            int offsetInBytes,
            T[] data,
            int startIndex,
            int elementCount,
            int vertexStride)
        {
            throw new NotImplementedException();
        }

        private void PlatformSetDataInternal<T>(
            int offsetInBytes,
            T[] data,
            int startIndex,
            int elementCount,
            int vertexStride,
            SetDataOptions options,
            int bufferSize,
            int elementSizeInBytes)
        {
            throw new NotImplementedException();
        }

        private void PlatformGraphicsDeviceResetting()
        {
            throw new NotImplementedException();
        }
    }
}
