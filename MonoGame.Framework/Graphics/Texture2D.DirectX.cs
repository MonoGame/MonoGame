// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Runtime.InteropServices;
using MonoGame.Utilities.Png;

#if WINDOWS_PHONE
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
#endif

#if WINDOWS_STOREAPP || WINDOWS_UAP
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using System.Threading.Tasks;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D : Texture
    {
        private bool _shared;

        private bool _renderTarget;
        private bool _mipmap;
        private void PlatformConstruct(int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type, bool shared)
        {
            _shared = shared;

            _renderTarget = (type == SurfaceType.RenderTarget);
            _mipmap = mipmap;

            // Create texture
            GetTexture();
        }

        private void PlatformSetData<T>(int level, int arraySlice, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
        {
            var elementSizeInByte = Marshal.SizeOf(typeof(T));
            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            // Use try..finally to make sure dataHandle is freed in case of an error
            try
            {
                var startBytes = startIndex * elementSizeInByte;
                var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);
                int x, y, w, h;
                if (rect.HasValue)
                {
                    x = rect.Value.X;
                    y = rect.Value.Y;
                    w = rect.Value.Width;
                    h = rect.Value.Height;
                }
                else
                {
                    x = 0;
                    y = 0;
                    w = Math.Max(width >> level, 1);
                    h = Math.Max(height >> level, 1);

                    // For DXT textures the width and height of each level is a multiple of 4.
                    // OpenGL only: The last two mip levels require the width and height to be 
                    // passed as 2x2 and 1x1, but there needs to be enough data passed to occupy 
                    // a 4x4 block. 
                    // Ref: http://www.mentby.com/Group/mac-opengl/issue-with-dxt-mipmapped-textures.html 
                    if (_format == SurfaceFormat.Dxt1 ||
                        _format == SurfaceFormat.Dxt1SRgb ||
                        _format == SurfaceFormat.Dxt1a ||
                        _format == SurfaceFormat.Dxt3 ||
                        _format == SurfaceFormat.Dxt3SRgb ||
                        _format == SurfaceFormat.Dxt5 ||
                        _format == SurfaceFormat.Dxt5SRgb)
                    {
                        w = (w + 3) & ~3;
                        h = (h + 3) & ~3;
                    }
                }

                var box = new SharpDX.DataBox(dataPtr, GetPitch(w), 0);

                var region = new SharpDX.Direct3D11.ResourceRegion();
                region.Top = y;
                region.Front = 0;
                region.Back = 1;
                region.Bottom = y + h;
                region.Left = x;
                region.Right = x + w;

                // TODO: We need to deal with threaded contexts here!
                int subresourceIndex = CalculateSubresourceIndex(arraySlice, level);
                var d3dContext = GraphicsDevice._d3dContext;
                lock (d3dContext)
                    d3dContext.UpdateSubresource(box, GetTexture(), subresourceIndex, region);
            }
            finally
            {
                dataHandle.Free();
            }
        }

        private void PlatformGetData<T>(int level, int arraySlice, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
        {
            // Create a temp staging resource for copying the data.
            // 
            // TODO: We should probably be pooling these staging resources
            // and not creating a new one each time.
            //
            var levelWidth = Math.Max(width >> level, 1);
            var levelHeight = Math.Max(height >> level, 1);

            var desc = new SharpDX.Direct3D11.Texture2DDescription();
            desc.Width = levelWidth;
            desc.Height = levelHeight;
            desc.MipLevels = 1;
            desc.ArraySize = 1;
            desc.Format = SharpDXHelper.ToFormat(_format);
            desc.BindFlags = SharpDX.Direct3D11.BindFlags.None;
            desc.CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.Read;
            desc.SampleDescription.Count = 1;
            desc.SampleDescription.Quality = 0;
            desc.Usage = SharpDX.Direct3D11.ResourceUsage.Staging;
            desc.OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None;

            var d3dContext = GraphicsDevice._d3dContext;
            using (var stagingTex = new SharpDX.Direct3D11.Texture2D(GraphicsDevice._d3dDevice, desc))
            {
                lock (d3dContext)
                {
                    int subresourceIndex = CalculateSubresourceIndex(arraySlice, level);

                    // Copy the data from the GPU to the staging texture.
                    int elementsInRow;
                    int rows;
                    if (rect.HasValue)
                    {
                        elementsInRow = rect.Value.Width;
                        rows = rect.Value.Height;
                        d3dContext.CopySubresourceRegion(GetTexture(), subresourceIndex, new SharpDX.Direct3D11.ResourceRegion(rect.Value.Left, rect.Value.Top, 0, rect.Value.Right, rect.Value.Bottom, 1), stagingTex, 0, 0, 0, 0);
                    }
                    else
                    {
                        elementsInRow = levelWidth;
                        rows = height;
                        d3dContext.CopySubresourceRegion(GetTexture(), subresourceIndex, null, stagingTex, 0, 0, 0, 0);
                    }

                    // Copy the data to the array.
                    SharpDX.DataStream stream = null;
                    try
                    {
                        var databox = d3dContext.MapSubresource(stagingTex, 0, SharpDX.Direct3D11.MapMode.Read, SharpDX.Direct3D11.MapFlags.None, out stream);

                        var elementSize = _format.GetSize();
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
                        SharpDX.Utilities.Dispose( ref stream);
                    }
                }
            }
        }

        private int CalculateSubresourceIndex(int arraySlice, int level)
        {
            return arraySlice * _levelCount + level;
        }

        private static Texture2D PlatformFromStream(GraphicsDevice graphicsDevice, Stream stream)
        {
#if WINDOWS_PHONE
            WriteableBitmap bitmap = null;
            Threading.BlockOnUIThread(() =>
            {
                try
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(stream);
                    bitmap = new WriteableBitmap(bitmapImage);
                }
                catch { }
            });

            // Convert from ARGB to ABGR 
            ConvertToABGR(bitmap.PixelHeight, bitmap.PixelWidth, bitmap.Pixels);

            Texture2D texture = new Texture2D(graphicsDevice, bitmap.PixelWidth, bitmap.PixelHeight);
            texture.SetData<int>(bitmap.Pixels);
            return texture;
#endif
#if !WINDOWS_PHONE

            if (!stream.CanSeek)
                throw new NotSupportedException("stream must support seek operations");

            // For reference this implementation was ultimately found through this post:
            // http://stackoverflow.com/questions/9602102/loading-textures-with-sharpdx-in-metro 
            Texture2D toReturn = null;
            SharpDX.WIC.BitmapDecoder decoder;

            using (var bitmap = LoadBitmap(stream, out decoder))
            using (decoder)
            {
                SharpDX.Direct3D11.Texture2D sharpDxTexture = CreateTex2DFromBitmap(bitmap, graphicsDevice);

                toReturn = new Texture2D(graphicsDevice, bitmap.Size.Width, bitmap.Size.Height);

                toReturn._texture = sharpDxTexture;
            }
            return toReturn;
#endif
        }

        private void PlatformSaveAsJpeg(Stream stream, int width, int height)
        {
#if WINDOWS_STOREAPP || WINDOWS_UAP
            SaveAsImage(BitmapEncoder.JpegEncoderId, stream, width, height);
#endif
#if WINDOWS_PHONE

            var pixelData = new byte[Width * Height * GraphicsExtensions.GetSize(Format)];
            GetData(pixelData);

            //We Must convert from BGRA to RGBA
            ConvertToRGBA(Height, Width, pixelData);

            var waitEvent = new ManualResetEventSlim(false);
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                var bitmap = new WriteableBitmap(Width, Height);
                System.Buffer.BlockCopy(pixelData, 0, bitmap.Pixels, 0, pixelData.Length);
                bitmap.SaveJpeg(stream, width, height, 0, 100);
                waitEvent.Set();
            });

            waitEvent.Wait();
#endif
#if !WINDOWS_STOREAPP && !WINDOWS_PHONE && !WINDOWS_UAP
            throw new NotImplementedException();
#endif
        }

        //Converts Pixel Data from BGRA to RGBA
        private static void ConvertToRGBA(int pixelHeight, int pixelWidth, byte[] pixels)
        {
            int offset = 0;

            for (int row = 0; row < (uint)pixelHeight; row++)
            {
                for (int col = 0; col < (uint)pixelWidth; col++)
                {
                    offset = (row * (int)pixelWidth * 4) + (col * 4);

                    byte B = pixels[offset];
                    byte R = pixels[offset + 2];

                    pixels[offset] = R;
                    pixels[offset + 2] = B;
                }
            }
        }

        private void PlatformSaveAsPng(Stream stream, int width, int height)
        {
            var pngWriter = new PngWriter();
            pngWriter.Write(this, stream);
        }

#if WINDOWS_STOREAPP || WINDOWS_UAP
        private void SaveAsImage(Guid encoderId, Stream stream, int width, int height)
        {
            var pixelData = new byte[Width * Height * GraphicsExtensions.GetSize(Format)];
            GetData(pixelData);

            // TODO: We need to convert from Format to R8G8B8A8!

            // TODO: We should implement async SaveAsPng() for WinRT.
            Task.Run(async () =>
            {
                // Create a temporary memory stream for writing the png.
                var memstream = new InMemoryRandomAccessStream();

                // Write the png.
                var encoder = await BitmapEncoder.CreateAsync(encoderId, memstream);
                encoder.SetPixelData(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Ignore, (uint)width, (uint)height, 96, 96, pixelData);
                await encoder.FlushAsync();

                // Copy the memory stream into the real output stream.
                memstream.Seek(0);
                memstream.AsStreamForRead().CopyTo(stream);

            }).Wait();
        }
#endif
#if !WINDOWS_PHONE

        [CLSCompliant(false)]
        public static SharpDX.Direct3D11.Texture2D CreateTex2DFromBitmap(SharpDX.WIC.BitmapSource bsource, GraphicsDevice device)
        {

            SharpDX.Direct3D11.Texture2DDescription desc;
            desc.Width = bsource.Size.Width;
            desc.Height = bsource.Size.Height;
            desc.ArraySize = 1;
            desc.BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource;
            desc.Usage = SharpDX.Direct3D11.ResourceUsage.Default;
            desc.CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None;
            desc.Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm;
            desc.MipLevels = 1;
            desc.OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None;
            desc.SampleDescription.Count = 1;
            desc.SampleDescription.Quality = 0;

            using(SharpDX.DataStream s = new SharpDX.DataStream(bsource.Size.Height * bsource.Size.Width * 4, true, true))
            {
                bsource.CopyPixels(bsource.Size.Width * 4, s);

                SharpDX.DataRectangle rect = new SharpDX.DataRectangle(s.DataPointer, bsource.Size.Width * 4);

                return new SharpDX.Direct3D11.Texture2D(device._d3dDevice, desc, rect);
            }
        }

        static SharpDX.WIC.ImagingFactory imgfactory = null;
        private static SharpDX.WIC.BitmapSource LoadBitmap(Stream stream, out SharpDX.WIC.BitmapDecoder decoder)
        {
            if (imgfactory == null)
            {
                imgfactory = new SharpDX.WIC.ImagingFactory();
            }

            decoder = new SharpDX.WIC.BitmapDecoder(
                imgfactory,
                stream,
                SharpDX.WIC.DecodeOptions.CacheOnDemand
                );

            var fconv = new SharpDX.WIC.FormatConverter(imgfactory);

            fconv.Initialize(
                decoder.GetFrame(0),
                SharpDX.WIC.PixelFormat.Format32bppPRGBA,
                SharpDX.WIC.BitmapDitherType.None, null,
                0.0, SharpDX.WIC.BitmapPaletteType.Custom);

            return fconv;
        }

#endif

        internal override SharpDX.Direct3D11.Resource CreateTexture()
		{
            // TODO: Move this to SetData() if we want to make Immutable textures!
            var desc = new SharpDX.Direct3D11.Texture2DDescription();
            desc.Width = width;
            desc.Height = height;
            desc.MipLevels = _levelCount;
            desc.ArraySize = ArraySize;
            desc.Format = SharpDXHelper.ToFormat(_format);
            desc.BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource;
            desc.CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None;
            desc.SampleDescription.Count = 1;
            desc.SampleDescription.Quality = 0;
            desc.Usage = SharpDX.Direct3D11.ResourceUsage.Default;
            desc.OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None;

            if (_renderTarget)
            {
                desc.BindFlags |= SharpDX.Direct3D11.BindFlags.RenderTarget;
                if (_mipmap)
                {
                    // Note: XNA 4 does not have a method Texture.GenerateMipMaps() 
                    // because generation of mipmaps is not supported on the Xbox 360.
                    // TODO: New method Texture.GenerateMipMaps() required.
                    desc.OptionFlags |= SharpDX.Direct3D11.ResourceOptionFlags.GenerateMipMaps;
                }
            }
            if (_shared)
                desc.OptionFlags |= SharpDX.Direct3D11.ResourceOptionFlags.Shared;

            return new SharpDX.Direct3D11.Texture2D(GraphicsDevice._d3dDevice, desc);
        }

        private void PlatformReload(Stream textureStream)
        {
#if WINDOWS_PHONE
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(textureStream);
                    WriteableBitmap bitmap = new WriteableBitmap(bitmapImage);

                    // Convert from ARGB to ABGR 
                    ConvertToABGR(bitmap.PixelHeight, bitmap.PixelWidth, bitmap.Pixels);

                    this.SetData<int>(bitmap.Pixels);

                    textureStream.Dispose();
                }
                catch { }
            });
#endif
        }
	}
}

