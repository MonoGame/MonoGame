// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using Nvidia.TextureTools;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    class DXTDataHandler
    {
        private TextureContent _content;
        private int _currentMipLevel;
        private int _levelWidth;
        private int _levelHeight;
        private Format _format;

        public OutputOptions.WriteDataDelegate WriteData { get; private set; }
        public OutputOptions.ImageDelegate BeginImage { get; private set; }

        public DXTDataHandler(TextureContent content, Format format)
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

            var texContent = new DXTBitmapContent(_format == Format.DXT1 ? 8 : 16, _levelWidth, _levelHeight);
            _content.Faces[0][_currentMipLevel] = texContent;
            _content.Faces[0][_currentMipLevel].SetPixelData(dataBuffer);

            return true;
        }
    }

    public static class GraphicsUtil
    {
        public static byte[] GetData(this Bitmap bmp)
        {
            // Any bitmap using this function should use 32bpp ARGB pixel format, since we have to
            // swizzle the channels later
            System.Diagnostics.Debug.Assert(bmp.PixelFormat == PixelFormat.Format32bppArgb);

            var bitmapData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                                    ImageLockMode.ReadOnly,
                                    bmp.PixelFormat);

            var length = bitmapData.Stride * bitmapData.Height;
            var output = new byte[length];

            // Copy bitmap to byte[]
            Marshal.Copy(bitmapData.Scan0, output, 0, length);
            bmp.UnlockBits(bitmapData);

            // NOTE: According to http://msdn.microsoft.com/en-us/library/dd183449%28VS.85%29.aspx
            // and http://stackoverflow.com/questions/8104461/pixelformat-format32bppargb-seems-to-have-wrong-byte-order
            // Image data from any GDI based function are stored in memory as BGRA/BGR, even if the format says RBGA.
            // Because of this we flip the R and B channels.

            BGRAtoRGBA(output);
  
            return output;
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
        public static void CompressTexture(TextureContent content, TargetPlatform platform, bool premultipliedAlpha)
        {
            // TODO: At the moment, only DXT compression from windows machine is supported
            // Add more here as they become available.
            switch (platform)
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
                    CompressDXT(content);
                    break;

                case TargetPlatform.iOS:
                    CompressPVRTC(content, premultipliedAlpha);
                    break;

                default:
                    throw new NotImplementedException(string.Format("Texture Compression it not implemented for {0}", platform));
            }
        }

        [DllImport("PVRTexLibC", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr compressPVRTC(byte[] data, int height, int width, int mipLevels, bool preMultiplied, bool pvrtc4bppCompression, ref IntPtr dataSizes);

        private static void CompressPVRTC(TextureContent content, bool premultipliedAlpha)
        {
            // Note: MipGeneration will be done by NVTT, rather than PVRTC's tool.
            // This way we have the same implementation across platforms.

            IntPtr dataSizesPtr = IntPtr.Zero;
            var texDataPtr = compressPVRTC(content.Faces[0][0].GetPixelData(),
                                            content.Faces[0][0].Height,
                                            content.Faces[0][0].Width,
                                            1,
                                            premultipliedAlpha,
                                            true,
                                            ref dataSizesPtr);

            // Store the size of each mipLevel
            var dataSizesArray = new int[1];
            Marshal.Copy(dataSizesPtr, dataSizesArray, 0, dataSizesArray.Length);

            var levelSize = 0;
            byte[] levelData;
            var sourceWidth = content.Faces[0][0].Width;
            var sourceHeight = content.Faces[0][0].Height;

            content.Faces[0].Clear();

            levelSize = dataSizesArray[0];
            levelData = new byte[levelSize];

            Marshal.Copy(texDataPtr, levelData, 0, levelSize);

            var bmpContent = new PVRTCBitmapContent(4, sourceWidth, sourceHeight);
            bmpContent.SetPixelData(levelData);
            content.Faces[0].Add(bmpContent);
        }

        private static void CompressDXT(TextureContent content)
        {
            var texData = content.Faces[0][0];

            if (!IsPowerOfTwo(texData.Width) || !IsPowerOfTwo(texData.Height))
                throw new PipelineException("DXT Compressed textures width and height must be powers of two.");

            var _dxtCompressor = new Compressor();
            var inputOptions = new InputOptions();
            inputOptions.SetAlphaMode(AlphaMode.Transparency);
            inputOptions.SetTextureLayout(TextureType.Texture2D, texData.Width, texData.Height, 1);

            var pixelData = texData.GetPixelData();
            
            // Small hack here. NVTT wants 8bit data in BGRA. Flip the B and R channels
            // again here.
            GraphicsUtil.BGRAtoRGBA(pixelData);
            var dataHandle = GCHandle.Alloc(pixelData, GCHandleType.Pinned);
            var dataPtr = dataHandle.AddrOfPinnedObject();

            inputOptions.SetMipmapData(dataPtr, texData.Width, texData.Height, 1, 0, 0);
            inputOptions.SetMipmapGeneration(false);

            var containsFracAlpha = ContainsFractionalAlpha(pixelData);
            var outputOptions = new OutputOptions();
            outputOptions.SetOutputHeader(false);

            var outputFormat = containsFracAlpha ? Format.DXT5 : Format.DXT1;

            var handler = new DXTDataHandler(content, outputFormat);
            outputOptions.SetOutputHandler(handler.BeginImage, handler.WriteData);

            var compressionOptions = new CompressionOptions();
            compressionOptions.SetFormat(outputFormat);

            _dxtCompressor.Compress(inputOptions, compressionOptions, outputOptions);

            dataHandle.Free();
        }

        internal static bool ContainsFractionalAlpha(byte[] data)
        {
            for (var x = 3; x < data.Length; x += 4)
            {
                if (data[x] != 0x0 && data[x] != 0xFF)
                    return true;
            }

            return false;
        }

        internal static void Resize(this TextureContent content, int newWidth, int newHeight)
        {
            var resizedBmp = new Bitmap(newWidth, newHeight);

            using (var graphics = System.Drawing.Graphics.FromImage(resizedBmp))
            {
                graphics.DrawImage(content._bitmap, 0, 0, newWidth, newHeight);

                content._bitmap.Dispose();
                content._bitmap = resizedBmp;
            }

            var imageData = content._bitmap.GetData();

            var bitmapContent = new PixelBitmapContent<Color>(content._bitmap.Width, content._bitmap.Height);
            bitmapContent.SetPixelData(imageData);

            content.Faces.Clear();
            content.Faces.Add(new MipmapChain(bitmapContent));
        }
    }
}
