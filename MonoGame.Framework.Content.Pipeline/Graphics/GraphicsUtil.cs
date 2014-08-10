﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;
using Nvidia.TextureTools;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    
    class DxtDataHandler
    {
        private TextureContent _content;
        private int _currentMipLevel;
        private int _levelWidth;
        private int _levelHeight;
        private Format _format;

        public OutputOptions.WriteDataDelegate WriteData { get; private set; }
        public OutputOptions.ImageDelegate BeginImage { get; private set; }

        public DxtDataHandler(TextureContent content, Format format)
        {
            _content = content;

            _currentMipLevel = 0;
            _levelWidth = content.Faces[0][0].Width;
            _levelHeight = content.Faces[0][0].Height;
            _format = format;

            WriteData = new OutputOptions.WriteDataDelegate(writeData);
            BeginImage = new OutputOptions.ImageDelegate(beginImage);
        }

        public void beginImage(int size, int width, int height, int depth, int face, int miplevel)
        {
            _levelHeight = height;
            _levelWidth = width;
            _currentMipLevel = miplevel;
        }

        protected bool writeData(IntPtr data, int length)
        {
            var dataBuffer = new byte[length];

            Marshal.Copy(data, dataBuffer, 0, length);

            DxtBitmapContent texContent = null;
            switch (_format)
            {
                case Format.DXT1:
                    texContent = new Dxt1BitmapContent(_levelWidth, _levelHeight);
                    break;
                case Format.DXT3:
                    texContent = new Dxt3BitmapContent(_levelWidth, _levelHeight);
                    break;
                case Format.DXT5:
                    texContent = new Dxt5BitmapContent(_levelWidth, _levelHeight);
                    break;
            }

            if (_content.Faces[0].Count == _currentMipLevel)
                _content.Faces[0].Add(texContent);
            else
                _content.Faces[0][_currentMipLevel] = texContent;

            _content.Faces[0][_currentMipLevel].SetPixelData(dataBuffer);

            return true;
        }
    }
    
    public static class GraphicsUtil
    {
        internal static Bitmap ToSystemBitmap(this BitmapContent bitmapContent)
        {
            var srcBmp = bitmapContent;
            var srcData = srcBmp.GetPixelData();

            var srcDataHandle = GCHandle.Alloc(srcData, GCHandleType.Pinned);
            var srcDataPtr = (IntPtr)(srcDataHandle.AddrOfPinnedObject().ToInt64());

            // stride must be aligned on a 32 bit boundary or 4 bytes
            int stride = ((srcBmp.Width * 32 + 31) & ~31) >> 3;

            var systemBitmap = new Bitmap(srcBmp.Width, srcBmp.Height, stride, PixelFormat.Format32bppArgb | PixelFormat.Alpha, srcDataPtr);
            srcDataHandle.Free();

            return systemBitmap;
        }

        internal static void Resize(this TextureContent content, int newWidth, int newHeight)
        {
            var source = content.Faces[0][0].ToSystemBitmap();

            var destination = new Bitmap(newWidth, newHeight);
            using (var graphics = System.Drawing.Graphics.FromImage(destination))
            {
                graphics.DrawImage(source, 0, 0, newWidth, newHeight);
                source.Dispose();
            }

            content.Faces.Clear();
            content.Faces.Add(new MipmapChain(destination.ToXnaBitmap()));
        }

        public static BitmapContent ToXnaBitmap(this Bitmap systemBitmap)
        {
            // Any bitmap using this function should use 32bpp ARGB pixel format, since we have to
            // swizzle the channels later
            System.Diagnostics.Debug.Assert(systemBitmap.PixelFormat == PixelFormat.Format32bppArgb);

            var bitmapData = systemBitmap.LockBits(new System.Drawing.Rectangle(0, 0, systemBitmap.Width, systemBitmap.Height),
                                    ImageLockMode.ReadOnly,
                                    systemBitmap.PixelFormat);

            var length = bitmapData.Stride * bitmapData.Height;
            var pixelData = new byte[length];

            // Copy bitmap to byte[]
            Marshal.Copy(bitmapData.Scan0, pixelData, 0, length);
            systemBitmap.UnlockBits(bitmapData);

            // NOTE: According to http://msdn.microsoft.com/en-us/library/dd183449%28VS.85%29.aspx
            // and http://stackoverflow.com/questions/8104461/pixelformat-format32bppargb-seems-to-have-wrong-byte-order
            // Image data from any GDI based function are stored in memory as BGRA/BGR, even if the format says RGBA.
            // Because of this we flip the R and B channels.

            BGRAtoRGBA(pixelData);

            var xnaBitmap = new PixelBitmapContent<Color>(systemBitmap.Width, systemBitmap.Height);
            xnaBitmap.SetPixelData(pixelData);

            return xnaBitmap;
        }

        public static void BGRAtoRGBA(byte[] data)
        {
            for (var x = 0; x < data.Length; x += 4)
            {
                data[x] ^= data[x + 2];
                data[x + 2] ^= data[x];
                data[x] ^= data[x + 2];
            }
        }

        public static bool IsPowerOfTwo(int x)
        {
            return (x & (x - 1)) == 0;
        }

        /// <summary>
        /// Returns the next power of two. Returns same value if already is PoT.
        /// </summary>
        public static int GetNextPowerOfTwo(int value)
        {
            if (IsPowerOfTwo(value))
                return value;

            var nearestPower = 1;
            while (nearestPower < value)
                nearestPower = nearestPower << 1;

            return nearestPower;
        }

        /// <summary>
        /// Compares a System.Drawing.Color to a Microsoft.Xna.Framework.Color
        /// </summary>
        internal static bool ColorsEqual(this System.Drawing.Color a, Color b)
        {
            if (a.A != b.A)
                return false;

            if (a.R != b.R)
                return false;

            if (a.G != b.G)
                return false;

            if (a.B != b.B)
                return false;

            return true;
        }

        /// <summary>
        /// Compresses TextureContent in a format appropriate to the platform
        /// </summary>
        public static void CompressTexture(GraphicsProfile profile, TextureContent content, ContentProcessorContext context, bool generateMipmaps, bool premultipliedAlpha, bool sharpAlpha)
        {
            // TODO: At the moment, only DXT compression from windows machine is supported
            //       Add more here as they become available.
            switch (context.TargetPlatform)
            {
                case TargetPlatform.Windows:
                case TargetPlatform.WindowsPhone:
                case TargetPlatform.WindowsPhone8:
                case TargetPlatform.WindowsStoreApp:
                case TargetPlatform.Ouya:
                case TargetPlatform.Android:
                case TargetPlatform.Linux: 
                case TargetPlatform.MacOSX:
                case TargetPlatform.NativeClient:
                case TargetPlatform.Xbox360:
					context.Logger.LogMessage("Using DXT Compression");
                    CompressDxt(profile, content, generateMipmaps, premultipliedAlpha, sharpAlpha);
				    break;
                case TargetPlatform.iOS:
					context.Logger.LogMessage("Using PVRTC Compression");
                    CompressPvrtc(content, generateMipmaps, premultipliedAlpha);
                    break;

                default:
                    throw new NotImplementedException(string.Format("Texture Compression it not implemented for {0}", context.TargetPlatform));
            }

        }
        
        private static void CompressPvrtc(TextureContent content, bool generateMipmaps, bool premultipliedAlpha)
        {
            // TODO: Once uncompressed mipmap generation is supported, first use NVTT to generate mipmaps,
            // then compress them withthe PVRTC tool, so we have the same implementation of mipmap generation
            // across platforms.

            // Calculate number of mip levels
            var width = content.Faces[0][0].Height;
            var height = content.Faces[0][0].Width;

			if (!IsPowerOfTwo(width) || !IsPowerOfTwo(height))
				throw new PipelineException("PVRTC Compressed textures width and height must be powers of two.");

			if (width != height)
				throw new PipelineException("PVRTC Compressed textures must be square. i.e width == height.");

            var numberOfMipLevels = 1;
            if (generateMipmaps)
            {
                while (height != 1 || width != 1)
                {
                    height = Math.Max(height / 2, 1);
                    width = Math.Max(width / 2, 1);
                    numberOfMipLevels++;
                }
            }

            IntPtr dataSizesPtr = IntPtr.Zero;
            var texDataPtr = ManagedPVRTC.ManagedPVRTC.CompressTexture(content.Faces[0][0].GetPixelData(),
                                            content.Faces[0][0].Height,
                                            content.Faces[0][0].Width,
                                            numberOfMipLevels,
                                            premultipliedAlpha,
                                            true,
                                            ref dataSizesPtr);

            // Store the size of each mip level
            var dataSizesArray = new int[numberOfMipLevels];
            Marshal.Copy(dataSizesPtr, dataSizesArray, 0, dataSizesArray.Length);

            var levelSize = 0;
            byte[] levelData;
            var sourceWidth = content.Faces[0][0].Width;
            var sourceHeight = content.Faces[0][0].Height;

            content.Faces[0].Clear();

            for (int x = 0; x < numberOfMipLevels; x++)
            {
                levelSize = dataSizesArray[x];
                levelData = new byte[levelSize];

                Marshal.Copy(texDataPtr, levelData, 0, levelSize);

                var levelWidth = Math.Max(sourceWidth >> x, 1);
                var levelHeight = Math.Max(sourceHeight >> x, 1);

                var bmpContent = new PvrtcBitmapContent(4, sourceWidth, sourceHeight);
                bmpContent.SetPixelData(levelData);
                content.Faces[0].Add(bmpContent);

                texDataPtr = IntPtr.Add(texDataPtr, levelSize);
            }
        }

        private static void CompressDxt(GraphicsProfile profile, TextureContent content, bool generateMipmaps, bool premultipliedAlpha, bool sharpAlpha)
        {
            var texData = content.Faces[0][0];

            if (profile == GraphicsProfile.Reach)
            {
                if (!IsPowerOfTwo(texData.Width) || !IsPowerOfTwo(texData.Height))
                    throw new PipelineException("DXT Compressed textures width and height must be powers of two in GraphicsProfile.Reach.");                
            }

            var pixelData = texData.GetPixelData();

            // Test the alpha channel to figure out if we have alpha.
            var containsAlpha = false;
            var containsFracAlpha = false;
            for (var x = 3; x < pixelData.Length; x += 4)
            {
                if (pixelData[x] != 0xFF)
                {
                    containsAlpha = true;

                    if (pixelData[x] != 0x0)
                        containsFracAlpha = true;
                }
            }

            var _dxtCompressor = new Compressor();
            var inputOptions = new InputOptions();
            if (containsAlpha)           
                inputOptions.SetAlphaMode(premultipliedAlpha ? AlphaMode.Premultiplied : AlphaMode.Transparency);
            else
                inputOptions.SetAlphaMode(AlphaMode.None);
            inputOptions.SetTextureLayout(TextureType.Texture2D, texData.Width, texData.Height, 1);

           
            // Small hack here. NVTT wants 8bit data in BGRA. Flip the B and R channels
            // again here.
            GraphicsUtil.BGRAtoRGBA(pixelData);
            var dataHandle = GCHandle.Alloc(pixelData, GCHandleType.Pinned);
            var dataPtr = dataHandle.AddrOfPinnedObject();

            inputOptions.SetMipmapData(dataPtr, texData.Width, texData.Height, 1, 0, 0);
            inputOptions.SetMipmapGeneration(generateMipmaps);
            inputOptions.SetGamma(1.0f, 1.0f);

            var outputOptions = new OutputOptions();
            outputOptions.SetOutputHeader(false);

            var outputFormat = Format.DXT1;
            if (containsFracAlpha)
            {
                if (sharpAlpha)
                    outputFormat = Format.DXT3;
                else
                    outputFormat = Format.DXT5;
            }

            var handler = new DxtDataHandler(content, outputFormat);
            outputOptions.SetOutputHandler(handler.BeginImage, handler.WriteData);

            var compressionOptions = new CompressionOptions();
            compressionOptions.SetFormat(outputFormat);
            compressionOptions.SetQuality(Quality.Normal);

            _dxtCompressor.Compress(inputOptions, compressionOptions, outputOptions);

            dataHandle.Free();
        }       
    }
}
