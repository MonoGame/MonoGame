// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Utilities;
using MonoGame.Utilities.Png;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.WIC;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Resource = SharpDX.Direct3D11.Resource;

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

        private SampleDescription _sampleDescription;

        private void PlatformConstruct(int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type, bool shared)
        {
            _shared = shared;

            _renderTarget = (type == SurfaceType.RenderTarget);
            _mipmap = mipmap;
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

            var desc = new Texture2DDescription();
            desc.Width = levelWidth;
            desc.Height = levelHeight;
            desc.MipLevels = 1;
            desc.ArraySize = 1;
            desc.Format = SharpDXHelper.ToFormat(_format);
            desc.BindFlags = BindFlags.None;
            desc.CpuAccessFlags = CpuAccessFlags.Read;
            desc.SampleDescription.Count = 1;
            desc.SampleDescription.Quality = 0;
            desc.Usage = ResourceUsage.Staging;
            desc.OptionFlags = ResourceOptionFlags.None;

            // Save sampling description.
            _sampleDescription = desc.SampleDescription;

            var d3dContext = GraphicsDevice._d3dContext;
            using (var stagingTex = new SharpDX.Direct3D11.Texture2D(GraphicsDevice._d3dDevice, desc))
            {
                lock (d3dContext)
                {
                    var subresourceIndex = CalculateSubresourceIndex(arraySlice, level);

                    // Copy the data from the GPU to the staging texture.
                    var elementsInRow = rect.Width;
                    var rows = rect.Height;
                    var region = new ResourceRegion(rect.Left, rect.Top, 0, rect.Right, rect.Bottom, 1);
                    d3dContext.CopySubresourceRegion(GetTexture(), subresourceIndex, region, stagingTex, 0);

                    // Copy the data to the array.
                    DataStream stream = null;
                    try
                    {
                        var databox = d3dContext.MapSubresource(stagingTex, 0, MapMode.Read, MapFlags.None, out stream);

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
                                for (i = row * rowSize / elementSizeInByte; i < (row + 1) * rowSize / elementSizeInByte; i++)
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
            SaveAsImage(Windows.Graphics.Imaging.BitmapEncoder.JpegEncoderId, stream, width, height);
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
                    offset = (row * pixelWidth * 4) + (col * 4);

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
                var encoder = await Windows.Graphics.Imaging.BitmapEncoder.CreateAsync(encoderId, memstream);
                encoder.SetPixelData(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Ignore, (uint)width, (uint)height, 96, 96, pixelData);
                await encoder.FlushAsync();

                // Copy the memory stream into the real output stream.
                memstream.Seek(0);
                memstream.AsStreamForRead().CopyTo(stream);

            }).Wait();
        }
#endif
#if !WINDOWS_PHONE

        static SharpDX.Direct3D11.Texture2D CreateTex2DFromBitmap(BitmapSource bsource, GraphicsDevice device)
        {

            Texture2DDescription desc;
            desc.Width = bsource.Size.Width;
            desc.Height = bsource.Size.Height;
            desc.ArraySize = 1;
            desc.BindFlags = BindFlags.ShaderResource;
            desc.Usage = ResourceUsage.Default;
            desc.CpuAccessFlags = CpuAccessFlags.None;
            desc.Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm;
            desc.MipLevels = 1;
            desc.OptionFlags = ResourceOptionFlags.None;
            desc.SampleDescription.Count = 1;
            desc.SampleDescription.Quality = 0;

            using(DataStream s = new DataStream(bsource.Size.Height * bsource.Size.Width * 4, true, true))
            {
                bsource.CopyPixels(bsource.Size.Width * 4, s);

                DataRectangle rect = new DataRectangle(s.DataPointer, bsource.Size.Width * 4);

                return new SharpDX.Direct3D11.Texture2D(device._d3dDevice, desc, rect);
            }
        }

        static ImagingFactory imgfactory;
        private static BitmapSource LoadBitmap(Stream stream, out SharpDX.WIC.BitmapDecoder decoder)
        {
            if (imgfactory == null)
            {
                imgfactory = new ImagingFactory();
            }

            decoder = new SharpDX.WIC.BitmapDecoder(
                imgfactory,
                stream,
                DecodeOptions.CacheOnDemand
                );

            var fconv = new FormatConverter(imgfactory);

            fconv.Initialize(
                decoder.GetFrame(0),
                PixelFormat.Format32bppPRGBA,
                BitmapDitherType.None, null,
                0.0, BitmapPaletteType.Custom);

            return fconv;
        }

#endif

        internal override Resource CreateTexture()
		{
            // TODO: Move this to SetData() if we want to make Immutable textures!
            var desc = new Texture2DDescription();
            desc.Width = width;
            desc.Height = height;
            desc.MipLevels = _levelCount;
            desc.ArraySize = ArraySize;
            desc.Format = SharpDXHelper.ToFormat(_format);
            desc.BindFlags = BindFlags.ShaderResource;
            desc.CpuAccessFlags = CpuAccessFlags.None;
            desc.SampleDescription.Count = 1;
            desc.SampleDescription.Quality = 0;
            desc.Usage = ResourceUsage.Default;
            desc.OptionFlags = ResourceOptionFlags.None;

            if (_renderTarget)
            {
                desc.BindFlags |= BindFlags.RenderTarget;
                if (_mipmap)
                {
                    // Note: XNA 4 does not have a method Texture.GenerateMipMaps() 
                    // because generation of mipmaps is not supported on the Xbox 360.
                    // TODO: New method Texture.GenerateMipMaps() required.
                    desc.OptionFlags |= ResourceOptionFlags.GenerateMipMaps;
                }
            }
            if (_shared)
                desc.OptionFlags |= ResourceOptionFlags.Shared;

            // Save sampling description.
            _sampleDescription = desc.SampleDescription;

            return new SharpDX.Direct3D11.Texture2D(GraphicsDevice._d3dDevice, desc);
        }

        internal SampleDescription GetTextureSampleDescription()
        {
            return _sampleDescription;
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

