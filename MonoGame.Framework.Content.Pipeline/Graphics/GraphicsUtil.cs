// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Nvidia.TextureTools;
using PVRTexLibNET;
using WrapMode = System.Drawing.Drawing2D.WrapMode;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics.PackedVector;

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

            var systemBitmap = new Bitmap(srcBmp.Width, srcBmp.Height, stride, System.Drawing.Imaging.PixelFormat.Format32bppArgb | System.Drawing.Imaging.PixelFormat.Alpha, srcDataPtr);
            srcDataHandle.Free();

            return systemBitmap;
        }

        internal static void Resize(this TextureContent content, int newWidth, int newHeight)
        {
            content.Faces[0][0] = content.Faces[0][0].Resize(newWidth, newHeight);
        }

        internal static BitmapContent Resize(this BitmapContent bitmap, int newWidth, int newHeight)
        {
            // TODO: This should be refactored to use FreeImage 
            // with a higher quality filter.

            var destination = new Bitmap(newWidth, newHeight);

            using (var source = bitmap.ToSystemBitmap())
            using (var graphics = System.Drawing.Graphics.FromImage(destination))
            {
                var imageAttr = new ImageAttributes();
                imageAttr.SetWrapMode(WrapMode.TileFlipXY);

                var destRect = new System.Drawing.Rectangle(0, 0, newWidth, newHeight);

                graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
                graphics.DrawImage(source, destRect, 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, imageAttr);
            }

            return destination.ToXnaBitmap(false); //we dont want to flip colors twice            
        }

        public static BitmapContent ToXnaBitmap(this Bitmap systemBitmap, bool flipColors)
        {
            // Any bitmap using this function should use 32bpp ARGB pixel format, since we have to
            // swizzle the channels later
            System.Diagnostics.Debug.Assert(systemBitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb);

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

            if(flipColors)
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
        /// Returns true if the format is a compressed format.
        /// </summary>
        /// <param name="format">The texture processor output format.</param>
        /// <returns>True if the format is a compressed format.</returns>
        public static bool IsCompressedTextureFormat(TextureProcessorOutputFormat format)
        {
            switch (format)
            {
                case TextureProcessorOutputFormat.AtcCompressed:
                case TextureProcessorOutputFormat.DxtCompressed:
                case TextureProcessorOutputFormat.Etc1Compressed:
                case TextureProcessorOutputFormat.PvrCompressed:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Determines if the texture format requires power-of-two dimensions on the target platform.
        /// </summary>
        /// <param name="format">The texture format.</param>
        /// <param name="platform">The target platform.</param>
        /// <param name="profile">The targeted graphics profile.</param>
        /// <returns>True if the texture format requires power-of-two dimensions on the target platform.</returns>
        public static bool RequiresPowerOfTwo(TextureProcessorOutputFormat format, TargetPlatform platform, GraphicsProfile profile)
        {
            if (format == TextureProcessorOutputFormat.Compressed)
                format = GetTextureFormatForPlatform(format, platform);

            switch (format)
            {
                case TextureProcessorOutputFormat.DxtCompressed:
                    return profile == GraphicsProfile.Reach;

                case TextureProcessorOutputFormat.PvrCompressed:
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if the given texture format requires equal width and height on the target platform.
        /// </summary>
        /// <param name="format">The texture format.</param>
        /// <param name="platform">The target platform.</param>
        /// <returns>True if the texture format requires equal width and height on the target platform.</returns>
        public static bool RequiresSquare(TextureProcessorOutputFormat format, TargetPlatform platform)
        {
            if (format == TextureProcessorOutputFormat.Compressed)
                format = GetTextureFormatForPlatform(format, platform);

            switch (format)
            {
                case TextureProcessorOutputFormat.PvrCompressed:
                    return platform == TargetPlatform.iOS;
            }

            return false;
        }

        enum AlphaRange
        {
            /// <summary>
            /// Pixel data has no alpha values below 1.0.
            /// </summary>
            Opaque,

            /// <summary>
            /// Pixel data contains alpha values that are either 0.0 or 1.0.
            /// </summary>
            Cutout,

            /// <summary>
            /// Pixel data contains alpha values that cover the full range of 0.0 to 1.0.
            /// </summary>
            Full,
        }

        /// <summary>
        /// Gets the alpha range in a set of pixels.
        /// </summary>
        /// <param name="pixelData">An array of full-colour 32-bit pixel data in RGBA or BGRA order.</param>
        /// <returns>A member of the AlphaRange enum to describe the range of alpha in the pixel data.</returns>
        static AlphaRange CalculateAlphaRange(byte[] pixelData)
        {
            AlphaRange result = AlphaRange.Opaque;
            for (int i = 3; i < pixelData.Length; i += 4)
            {
                var value = pixelData[i];
                if (value == 0)
                    result = AlphaRange.Cutout;
                else if (value < 255)
                    return AlphaRange.Full;
            }
            return result;
        }

        /// <summary>
        /// If format is TextureProcessorOutputFormat.Compressed, the appropriate compressed texture format for the target
        /// platform is returned. Otherwise the format is returned unchanged.
        /// </summary>
        /// <param name="format">The supplied texture format.</param>
        /// <param name="platform">The target platform.</param>
        /// <returns>The texture format.</returns>
        public static TextureProcessorOutputFormat GetTextureFormatForPlatform(TextureProcessorOutputFormat format, TargetPlatform platform)
        {
            // Select the default texture compression format for the target platform
            if (format == TextureProcessorOutputFormat.Compressed)
            {
                switch (platform)
                {
                    case TargetPlatform.iOS:
                        format = TextureProcessorOutputFormat.PvrCompressed;
                        break;

                    case TargetPlatform.Android:
                        format = TextureProcessorOutputFormat.Etc1Compressed;
                        break;

                    default:
                        format = TextureProcessorOutputFormat.DxtCompressed;
                        break;
                }
            }

            if (IsCompressedTextureFormat(format))
            {
                // Make sure the target platform supports the selected texture compression format
                switch (platform)
                {
                    case TargetPlatform.iOS:
                        if (format != TextureProcessorOutputFormat.PvrCompressed)
                            throw new PlatformNotSupportedException("iOS platform only supports PVR texture compression");
                        break;

                    case TargetPlatform.Windows:
                    case TargetPlatform.WindowsGL:
                    case TargetPlatform.WindowsPhone8:
                    case TargetPlatform.WindowsStoreApp:
                    case TargetPlatform.Linux:
                    case TargetPlatform.MacOSX:
                    case TargetPlatform.NativeClient:
                        if (format != TextureProcessorOutputFormat.DxtCompressed)
                            throw new PlatformNotSupportedException(format.ToString() + " platform only supports DXT texture compression");
                        break;
                }
            }

            return format;
        }

        /// <summary>
        /// Compresses TextureContent in a format appropriate to the platform
        /// </summary>
        public static void CompressTexture(GraphicsProfile profile, TextureContent content, TextureProcessorOutputFormat format, ContentProcessorContext context, bool generateMipmaps, bool premultipliedAlpha, bool sharpAlpha)
        {
            format = GetTextureFormatForPlatform(format, context.TargetPlatform);

            switch (format)
            {
                case TextureProcessorOutputFormat.AtcCompressed:
                    CompressAti(content, generateMipmaps, premultipliedAlpha);
                    break;

                case TextureProcessorOutputFormat.Color16Bit:
                    CompressColor16Bit(content, generateMipmaps, premultipliedAlpha);
                    break;

                case TextureProcessorOutputFormat.DxtCompressed:
                    CompressDxt(profile, content, generateMipmaps, premultipliedAlpha, sharpAlpha);
                    break;

                case TextureProcessorOutputFormat.Etc1Compressed:
                    CompressEtc1(content, generateMipmaps, premultipliedAlpha);
                    break;

                case TextureProcessorOutputFormat.PvrCompressed:
                    CompressPvrtc(content, generateMipmaps, premultipliedAlpha);
                    break;
            }
        }
        
        private static void CompressPvrtc(TextureContent content, bool generateMipMaps, bool premultipliedAlpha)
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

            var face = content.Faces[0][0];

            var pixelData = face.GetPixelData();
            var alphaRange = CalculateAlphaRange(pixelData);

            if (alphaRange == AlphaRange.Opaque)
                Compress(typeof(PvrtcRgb4BitmapContent), content, generateMipMaps);
            else
                Compress(typeof(PvrtcRgba4BitmapContent), content, generateMipMaps);
        }

        private static void CompressDxt(GraphicsProfile profile, TextureContent content, bool generateMipMaps, bool premultipliedAlpha, bool sharpAlpha)
        {
            var texData = content.Faces[0][0];

            if (profile == GraphicsProfile.Reach)
            {
                if (!IsPowerOfTwo(texData.Width) || !IsPowerOfTwo(texData.Height))
                    throw new PipelineException("DXT Compressed textures width and height must be powers of two in GraphicsProfile.Reach.");                
            }

            var pixelData = texData.GetPixelData();

            // Test the alpha channel to figure out if we have alpha.
            var alphaRange = CalculateAlphaRange(pixelData);

            if (alphaRange == AlphaRange.Opaque)
                Compress(typeof(Dxt1BitmapContent), content, generateMipMaps);
            else if (alphaRange == AlphaRange.Cutout || sharpAlpha)
                Compress(typeof(Dxt3BitmapContent), content, generateMipMaps);
            else
                Compress(typeof(Dxt5BitmapContent), content, generateMipMaps);
            /*
            var _dxtCompressor = new Compressor();
            var inputOptions = new InputOptions();
            if (alphaRange != AlphaRange.Opaque)           
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
            if (alphaRange == AlphaRange.Cutout || sharpAlpha)
                outputFormat = Format.DXT3;
            else if (alphaRange == AlphaRange.Full)
                outputFormat = Format.DXT5;

            var handler = new DxtDataHandler(content, outputFormat);
            outputOptions.SetOutputHandler(handler.BeginImage, handler.WriteData);

            var compressionOptions = new CompressionOptions();
            compressionOptions.SetFormat(outputFormat);
            compressionOptions.SetQuality(Quality.Normal);

            _dxtCompressor.Compress(inputOptions, compressionOptions, outputOptions);

            dataHandle.Free();
            */
        }
  
        static void CompressAti(TextureContent content, bool generateMipMaps, bool premultipliedAlpha)
        {
			var face = content.Faces[0][0];
			var pixelData = face.GetPixelData();
			var alphaRange = CalculateAlphaRange(pixelData);

            if (alphaRange == AlphaRange.Full)
                Compress(typeof(AtcExplicitBitmapContent), content, generateMipMaps);
            else
                Compress(typeof(AtcInterpolatedBitmapContent), content, generateMipMaps);
        }

        static void CompressEtc1(TextureContent content, bool generateMipMaps, bool premultipliedAlpha)
        {
            var face = content.Faces[0][0];

            var pixelData = face.GetPixelData();
            var alphaRange = CalculateAlphaRange(pixelData);

            // Use BGRA4444 for textures with non-opaque alpha values
            if (alphaRange != AlphaRange.Opaque)
                Compress(typeof(PixelBitmapContent<Bgra4444>), content, generateMipMaps);
            else
                Compress(typeof(Etc1BitmapContent), content, generateMipMaps);
        }

        static void CompressColor16Bit(TextureContent content, bool generateMipMaps, bool premultipliedAlpha)
        {
            var face = content.Faces[0][0];

            var pixelData = face.GetPixelData();
            var alphaRange = CalculateAlphaRange(pixelData);

            if (alphaRange == AlphaRange.Opaque)
                Compress(typeof(PixelBitmapContent<Bgr565>), content, generateMipMaps);
            else if (alphaRange == AlphaRange.Cutout)
                Compress(typeof(PixelBitmapContent<Bgra5551>), content, generateMipMaps);
            else
                Compress(typeof(PixelBitmapContent<Bgra4444>), content, generateMipMaps);
        }

        static void Compress(Type targetType, TextureContent content, bool generateMipMaps)
        {
            var wh = new object[2];
            if (generateMipMaps)
            {
                for (int i = 0; i < content.Faces.Count; ++i)
                {
                    var src = content.Faces[i][0];
                    var w = src.Width;
                    var h = src.Height;

                    content.Faces[i].Clear();
                    wh[0] = w;
                    wh[1] = h;
                    var dest = (BitmapContent)Activator.CreateInstance(targetType, wh);
                    BitmapContent.Copy(src, dest);
                    content.Faces[i].Add(dest);
                    while (w > 1 && h > 1)
                    {
                        if (w > 1)
                            w = w >> 1;
                        if (h > 1)
                            h = h >> 1;
                        wh[0] = w;
                        wh[1] = h;
                        dest = (BitmapContent)Activator.CreateInstance(targetType, wh);
                        BitmapContent.Copy(src, dest);
                        content.Faces[i].Add(dest);
                    }
                }
            }
            else
            {
                content.ConvertBitmapType(targetType);
            }
        }
    }
}
