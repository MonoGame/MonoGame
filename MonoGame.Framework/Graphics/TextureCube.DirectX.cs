// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

using SharpDX;
using SharpDX.Direct3D11;

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class TextureCube
	{
        private bool _renderTarget;
        private bool _mipMap;

        private void PlatformConstruct(GraphicsDevice graphicsDevice, int size, bool mipMap, SurfaceFormat format, bool renderTarget)
        {
            _renderTarget = renderTarget;
            _mipMap = mipMap;

            // Create texture
            GetTexture();
        }

        internal override SharpDX.Direct3D11.Resource CreateTexture()
        {
            var description = new Texture2DDescription
            {
                Width = size,
                Height = size,
                MipLevels = _levelCount,
                ArraySize = 6, // A texture cube is a 2D texture array with 6 textures.
                Format = SharpDXHelper.ToFormat(_format),
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Default,
                OptionFlags = ResourceOptionFlags.TextureCube
            };

            if (_renderTarget)
            {
                description.BindFlags |= BindFlags.RenderTarget;
                if (_mipMap)
                    description.OptionFlags |= ResourceOptionFlags.GenerateMipMaps;
            }

            return new SharpDX.Direct3D11.Texture2D(GraphicsDevice._d3dDevice, description);
        }

        private void PlatformGetData<T>(CubeMapFace cubeMapFace, T[] data) where T : struct
        {
            throw new NotImplementedException();
        }

        private void PlatformSetData(CubeMapFace face, int level, IntPtr dataPtr, int xOffset, int yOffset, int width, int height)
        {
                var box = new DataBox(dataPtr, GetPitch(width), 0);

            int subresourceIndex = (int)face * _levelCount + level;

                var region = new ResourceRegion
                {
                    Top = yOffset,
                    Front = 0,
                    Back = 1,
                    Bottom = yOffset + height,
                    Left = xOffset,
                    Right = xOffset + width
                };

            var d3dContext = GraphicsDevice._d3dContext;
            lock (d3dContext)
                d3dContext.UpdateSubresource(box, GetTexture(), subresourceIndex, region);
        }
	}
}

