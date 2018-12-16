// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class IndexBuffer
    {
        private void PlatformConstruct(IndexElementSize indexElementSize, int indexCount)
        {
            throw new NotImplementedException();
        }

        private void PlatformGraphicsDeviceResetting()
        {
            throw new NotImplementedException();
        }

        private void PlatformGetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount) where T : struct
        {
            throw new NotImplementedException();
        }

        private void PlatformSetDataInternal<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options) where T : struct
        {
            throw new NotImplementedException();
        }
	}
}
