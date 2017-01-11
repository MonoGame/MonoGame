// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D11;

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class Texture3D : Texture
	{
        private bool renderTarget;
        private bool mipMap;

        private void PlatformConstruct(GraphicsDevice graphicsDevice, int width, int height, int depth, bool mipMap, SurfaceFormat format, bool renderTarget)
        {
            this.renderTarget = renderTarget;
            this.mipMap = mipMap;

            if (mipMap)
                this._levelCount = CalculateMipLevels(width, height, depth);

            // Create texture
            GetTexture();
        }


        internal override SharpDX.Direct3D11.Resource CreateTexture()
        {
            var description = new Texture3DDescription
            {
                Width = _width,
                Height = _height,
                Depth = _depth,
                MipLevels = _levelCount,
                Format = SharpDXHelper.ToFormat(_format),
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                Usage = ResourceUsage.Default,
                OptionFlags = ResourceOptionFlags.None,
            };

            if (renderTarget)
            {
                description.BindFlags |= BindFlags.RenderTarget;
                if (mipMap)
                {
                    // Note: XNA 4 does not have a method Texture.GenerateMipMaps() 
                    // because generation of mipmaps is not supported on the Xbox 360.
                    // TODO: New method Texture.GenerateMipMaps() required.
                    description.OptionFlags |= ResourceOptionFlags.GenerateMipMaps;
                }
            }

            return new SharpDX.Direct3D11.Texture3D(GraphicsDevice._d3dDevice, description);
        }

        private void PlatformSetData<T>(int level,
                                     int left, int top, int right, int bottom, int front, int back,
                                     T[] data, int startIndex, int elementCount, int width, int height, int depth)
        {
            var elementSizeInByte = Marshal.SizeOf(typeof(T));
            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInByte);

                int rowPitch = GetPitch(width);
                int slicePitch = rowPitch * height; // For 3D texture: Size of 2D image.
                var box = new DataBox(dataPtr, rowPitch, slicePitch);

                int subresourceIndex = level;

                var region = new ResourceRegion(left, top, front, right, bottom, back);

                var d3dContext = GraphicsDevice._d3dContext;
                lock (d3dContext)
                    d3dContext.UpdateSubresource(box, GetTexture(), subresourceIndex, region);
            }
            finally
            {
                dataHandle.Free();
            }
        }

        private void PlatformGetData<T>(int level, int left, int top, int right, int bottom, int front, int back, T[] data, int startIndex, int elementCount)
             where T : struct
        {

            // Create a temp staging resource for copying the data.
            // 
            // TODO: Like in Texture2D, we should probably be pooling these staging resources
            // and not creating a new one each time.
            //
            var desc = new Texture3DDescription
            {
                Width = _width,
                Height = _height,
                Depth = _depth,
                MipLevels = 1,
                Format = SharpDXHelper.ToFormat(_format),
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                Usage = ResourceUsage.Staging,
                OptionFlags = ResourceOptionFlags.None,
            };

            var d3dContext = GraphicsDevice._d3dContext;
            using (var stagingTex = new SharpDX.Direct3D11.Texture3D(GraphicsDevice._d3dDevice, desc))
            {
                lock (d3dContext)
                {
                    // Copy the data from the GPU to the staging texture.
                    d3dContext.CopySubresourceRegion(GetTexture(), level, new ResourceRegion(left, top, front, right, bottom, back), stagingTex, 0);

                    // Copy the data to the array.
                    DataStream stream = null;
                    try
                    {
                        var databox = d3dContext.MapSubresource(stagingTex, 0, MapMode.Read, MapFlags.None, out stream);

                        // Some drivers may add pitch to rows or slices.
                        // We need to copy each row separatly and skip trailing zeros.
                        var currentIndex = startIndex;
                        var elementSize = _format.GetSize();
                        var elementsInRow = right - left;
                        var rowsInSlice = bottom - top;
                        for (var slice = front; slice < back; slice++)
                        {
                            for (var row = top; row < bottom; row++)
                            {
                                stream.ReadRange(data, currentIndex, elementsInRow);
                                stream.Seek(databox.RowPitch - (elementSize * elementsInRow), SeekOrigin.Current);
                                currentIndex += elementsInRow;
                            }
                            stream.Seek(databox.SlicePitch - (databox.RowPitch * rowsInSlice), SeekOrigin.Current);
                        }
                    }
                    finally
                    {
                        SharpDX.Utilities.Dispose(ref stream);
                    }
                }
            }
        }
	}
}

