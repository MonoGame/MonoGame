// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class TextureCube
	{
        private void PlatformConstruct(GraphicsDevice graphicsDevice, int size, bool mipMap, SurfaceFormat format, bool renderTarget)
        {
            throw new NotImplementedException();
        }

        private void PlatformGetData<T>(CubeMapFace cubeMapFace, int level, Rectangle rect, T[] data, int startIndex, int elementCount) where T : struct
        {
            throw new NotImplementedException();
        }

        private void PlatformSetData<T>(CubeMapFace face, int level, Rectangle rect, T[] data, int startIndex, int elementCount)
        {
            throw new NotImplementedException();
        }
	}
}

