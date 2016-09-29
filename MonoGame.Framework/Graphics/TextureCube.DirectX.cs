// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using MapFlags = SharpDX.Direct3D11.MapFlags;

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
            // Create a temp staging resource for copying the data.
            // 
            // TODO: Like in Texture2D, we should probably be pooling these staging resources
            // and not creating a new one each time.
            //
            var desc = new Texture2DDescription
            {
                Width = size,
                Height = size,
                MipLevels = 1,
                ArraySize = 1,
                Format = SharpDXHelper.ToFormat(_format),
                SampleDescription = new SampleDescription(1, 0),
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                Usage = ResourceUsage.Staging,
                OptionFlags = ResourceOptionFlags.None,
            };

            var d3dContext = GraphicsDevice._d3dContext;
            using (var stagingTex = new SharpDX.Direct3D11.Texture2D(GraphicsDevice._d3dDevice, desc))
            {
                lock (d3dContext)
                {
                    // Copy the data from the GPU to the staging texture.
                    int subresourceIndex = CalculateSubresourceIndex(cubeMapFace, 0);
                    d3dContext.CopySubresourceRegion(GetTexture(), subresourceIndex, null, stagingTex, 0);

                    // Copy the data to the array.
                    DataStream stream = null;
                    try
                    {
                        var databox = d3dContext.MapSubresource(stagingTex, 0, MapMode.Read, MapFlags.None, out stream);

                        const int startIndex = 0;
                        var elementCount = data.Length;
                        var elementSize = _format.GetSize();
                        var elementsInRow = size;
                        var rows = size;
                        var rowSize = elementSize * elementsInRow;
                        if (rowSize == databox.RowPitch)
                            stream.ReadRange(data, startIndex, elementCount);
                        else
                        {
                            // Some drivers may add pitch to rows.
                            // We need to copy each row separatly and skip trailing zeros.
                            stream.Seek(startIndex, SeekOrigin.Begin);

                            int elementSizeInByte = Marshal.SizeOf(typeof(T));
                            for (var row = 0; row < rows; row++)
                            {
                                int i;
                                for (i = row * rowSize / elementSizeInByte; i < (row + 1) * rowSize / elementSizeInByte; i++)
                                    data[i] = stream.Read<T>();

                                if (i >= elementCount)
                                    break;

                                stream.Seek(databox.RowPitch - rowSize, SeekOrigin.Current);
                            }
                        }
                    }
                    finally
                    {
                        SharpDX.Utilities.Dispose(ref stream);
                    }
                }
            }
        }

        private void PlatformSetData<T>(CubeMapFace face, int level, IntPtr dataPtr, int xOffset, int yOffset, int width, int height)
        {
                var box = new DataBox(dataPtr, GetPitch(width), 0);

                int subresourceIndex = CalculateSubresourceIndex(face, level);

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

	    private int CalculateSubresourceIndex(CubeMapFace face, int level)
	    {
	        return (int) face * _levelCount + level;
	    }
	}
}

