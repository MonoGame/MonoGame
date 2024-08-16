// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Runtime.InteropServices;
using MonoGame.Framework.Utilities;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Resource = SharpDX.Direct3D11.Resource;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D : Texture
    {
        /// <summary />
        protected bool Shared { get { return _shared; } }
        /// <summary />
        protected bool Mipmap { get { return _mipmap; } }
        /// <summary />
        protected SampleDescription SampleDescription { get { return _sampleDescription; } }

        private bool _shared;
        private bool _mipmap;
        private SampleDescription _sampleDescription;

        private SharpDX.Direct3D11.Texture2D _cachedStagingTexture;

        private void PlatformConstruct(int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type, bool shared)
        {
            _shared = shared;
            _mipmap = mipmap;
            _sampleDescription = new SampleDescription(1, 0);
        }

        private void PlatformSetData<T>(int level, T[] data, int startIndex, int elementCount) where T : struct
        {
            int w, h;
            GetSizeForLevel(Width, Height, level, out w, out h);

            // For DXT compressed formats the width and height must be
            // a multiple of 4 for the complete mip level to be set.
            if (_format.IsCompressedFormat())
            {
                w = (w + 3) & ~3;
                h = (h + 3) & ~3;
            }

            var elementSizeInByte = ReflectionHelpers.SizeOf<T>.Get();
            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            // Use try..finally to make sure dataHandle is freed in case of an error
            try
            {
                var startBytes = startIndex * elementSizeInByte;
                var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);
                var region = new ResourceRegion();
                region.Top = 0;
                region.Front = 0;
                region.Back = 1;
                region.Bottom = h;
                region.Left = 0;
                region.Right = w;

                // TODO: We need to deal with threaded contexts here!
                var subresourceIndex = CalculateSubresourceIndex(0, level);
                var d3dContext = GraphicsDevice._d3dContext;
                lock (d3dContext)
                    d3dContext.UpdateSubresource(GetTexture(), subresourceIndex, region, dataPtr, GetPitch(w), 0);
            }
            finally
            {
                dataHandle.Free();
            }
        }

        private void PlatformSetData<T>(int level, int arraySlice, Rectangle rect, T[] data, int startIndex, int elementCount) where T : struct
        {
            var elementSizeInByte = ReflectionHelpers.SizeOf<T>.Get();
            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            // Use try..finally to make sure dataHandle is freed in case of an error
            try
            {
                var startBytes = startIndex * elementSizeInByte;
                var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);
                var region = new ResourceRegion();
                region.Top = rect.Top;
                region.Front = 0;
                region.Back = 1;
                region.Bottom = rect.Bottom;
                region.Left = rect.Left;
                region.Right = rect.Right;


                // TODO: We need to deal with threaded contexts here!
                var subresourceIndex = CalculateSubresourceIndex(arraySlice, level);
                var d3dContext = GraphicsDevice._d3dContext;
                lock (d3dContext)
                    d3dContext.UpdateSubresource(GetTexture(), subresourceIndex, region, dataPtr, GetPitch(rect.Width), 0);
            }
            finally
            {
                dataHandle.Free();
            }
        }

        private void PlatformGetData<T>(int level, int arraySlice, Rectangle rect, T[] data, int startIndex, int elementCount) where T : struct
        {
            // Create a temp staging resource for copying the data.
            // 
            // TODO: We should probably be pooling these staging resources
            // and not creating a new one each time.
            //
            var min = _format.IsCompressedFormat() ? 4 : 1;
            var levelWidth = Math.Max(width >> level, min);
            var levelHeight = Math.Max(height >> level, min);

            if (_cachedStagingTexture == null)
            {
                var desc = new Texture2DDescription();
                desc.Width = levelWidth;
                desc.Height = levelHeight;
                desc.MipLevels = 1;
                desc.ArraySize = 1;
                desc.Format = SharpDXHelper.ToFormat(_format);
                desc.BindFlags = BindFlags.None;
                desc.CpuAccessFlags = CpuAccessFlags.Read;
                desc.SampleDescription = SampleDescription;
                desc.Usage = ResourceUsage.Staging;
                desc.OptionFlags = ResourceOptionFlags.None;


                _cachedStagingTexture = new SharpDX.Direct3D11.Texture2D(GraphicsDevice._d3dDevice, desc);
            }

            var d3dContext = GraphicsDevice._d3dContext;

            lock (d3dContext)
            {
                var subresourceIndex = CalculateSubresourceIndex(arraySlice, level);

                // Copy the data from the GPU to the staging texture.
                var elementsInRow = rect.Width;
                var rows = rect.Height;
                var region = new ResourceRegion(rect.Left, rect.Top, 0, rect.Right, rect.Bottom, 1);
                d3dContext.CopySubresourceRegion(GetTexture(), subresourceIndex, region, _cachedStagingTexture, 0);

                // Copy the data to the array.
                DataStream stream = null;
                try
                {
                    var databox = d3dContext.MapSubresource(_cachedStagingTexture, 0, MapMode.Read, MapFlags.None, out stream);

                    var elementSize = _format.GetSize();
                    if (_format.IsCompressedFormat())
                    {
                        // for 4x4 block compression formats an element is one block, so elementsInRow
                        // and number of rows are 1/4 of number of pixels in width and height of the rectangle
                        elementsInRow /= 4;
                        rows /= 4;
                    }
                    var rowSize = elementSize * elementsInRow;
                    if (rowSize == databox.RowPitch)
                        stream.ReadRange(data, startIndex, elementCount);
                    else
                    {
                        // Some drivers may add pitch to rows.
                        // We need to copy each row separatly and skip trailing zeros.
                        stream.Seek(0, SeekOrigin.Begin);

                        var elementSizeInByte = ReflectionHelpers.SizeOf<T>.Get();
                        for (var row = 0; row < rows; row++)
                        {
                            int i;
                            int maxElements =  (row + 1) * rowSize / elementSizeInByte;
                            for (i = row * rowSize / elementSizeInByte; i < maxElements; i++)
                                data[i + startIndex] = stream.Read<T>();

                            if (i >= elementCount)
                                break;

                            stream.Seek(databox.RowPitch - rowSize, SeekOrigin.Current);
                        }
                    }
                }
                finally
                {
                    SharpDX.Utilities.Dispose( ref stream);

                    d3dContext.UnmapSubresource(_cachedStagingTexture, 0);
                }
            }
        }

        /// <summary />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                SharpDX.Utilities.Dispose(ref _cachedStagingTexture);
            }

            base.Dispose(disposing);
        }

        private int CalculateSubresourceIndex(int arraySlice, int level)
        {
            return arraySlice * _levelCount + level;
        }

        /// <summary />
        protected internal virtual Texture2DDescription GetTexture2DDescription()
        {
            var desc = new Texture2DDescription();
            desc.Width = width;
            desc.Height = height;
            desc.MipLevels = _levelCount;
            desc.ArraySize = ArraySize;
            desc.Format = SharpDXHelper.ToFormat(_format);
            desc.BindFlags = BindFlags.ShaderResource;
            desc.CpuAccessFlags = CpuAccessFlags.None;
            desc.SampleDescription = SampleDescription;
            desc.Usage = ResourceUsage.Default;
            desc.OptionFlags = ResourceOptionFlags.None;

            if (_shared)
                desc.OptionFlags |= ResourceOptionFlags.Shared;

            return desc;
        }
        internal override Resource CreateTexture()
        {
            // TODO: Move this to SetData() if we want to make Immutable textures!
            var desc = GetTexture2DDescription();
            return new SharpDX.Direct3D11.Texture2D(GraphicsDevice._d3dDevice, desc);
        }

        private void PlatformReload(Stream textureStream)
        {
        }
    }
}

