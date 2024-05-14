// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class BufferResource
    {
        private void PlatformConstruct()
        {
        }

        internal void PlatformGetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int elementStride)
        {
        }

        internal void PlatformSetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, int elementStride, SetDataOptions options, int bufferSize, int elementSizeInBytes)
        {
        }

        private void PlatformGraphicsDeviceResetting()
        {
        }
    }
}
