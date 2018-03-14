// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using Nvidia.TextureTools;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public abstract class DxtBitmapContent : BitmapContent
    {
        private byte[] _bitmapData;
        private int _blockSize;
        private SurfaceFormat _format;

        private int _nvttWriteOffset;

        protected DxtBitmapContent(int blockSize)
        {
            if (!((blockSize == 8) || (blockSize == 16)))
                throw new ArgumentException("Invalid block size");
            _blockSize = blockSize;
            TryGetFormat(out _format);
        }

        protected DxtBitmapContent(int blockSize, int width, int height)
            : this(blockSize)
        {
            Width = width;
            Height = height;
        }

        public override byte[] GetPixelData()
        {
            return _bitmapData;
        }

        public override void SetPixelData(byte[] sourceData)
        {
            _bitmapData = sourceData;
        }

        private void NvttBeginImage(int size, int width, int height, int depth, int face, int miplevel)
        {
            _bitmapData = new byte[size];
            _nvttWriteOffset = 0;
        }

        private bool NvttWriteImage(IntPtr data, int length)
        {
            Marshal.Copy(data, _bitmapData, _nvttWriteOffset, length);
            _nvttWriteOffset += length;
            return true;
        }

        private void NvttEndImage()
        {
        }

        protected override bool TryCopyFrom(BitmapContent sourceBitmap, Rectangle sourceRegion, Rectangle destinationRegion)
        {
            SurfaceFormat sourceFormat;
            if (!sourceBitmap.TryGetFormat(out sourceFormat))
                return false;

            SurfaceFormat format;
            TryGetFormat(out format);

            // A shortcut for copying the entire bitmap to another bitmap of the same type and format
            if (format == sourceFormat && (sourceRegion == new Rectangle(0, 0, Width, Height)) && sourceRegion == destinationRegion)
            {
                SetPixelData(sourceBitmap.GetPixelData());
                return true;
            }

            // Destination region copy is not yet supported
            if (destinationRegion != new Rectangle(0, 0, Width, Height))
                return false;

            // If the source is not Vector4 or requires resizing, send it through BitmapContent.Copy
            if (!(sourceBitmap is PixelBitmapContent<Vector4>) || sourceRegion.Width != destinationRegion.Width || sourceRegion.Height != destinationRegion.Height)
            {
                try
                {
                    BitmapContent.Copy(sourceBitmap, sourceRegion, this, destinationRegion);
                    return true;
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }

            Format outputFormat;
            switch (format)
            {
                case SurfaceFormat.Dxt1:
                    outputFormat = Format.DXT1;
                    break;
                case SurfaceFormat.Dxt1SRgb:
                    outputFormat = Format.DXT1;
                    break;
                case SurfaceFormat.Dxt3:
                    outputFormat = Format.DXT3;
                    break;
                case SurfaceFormat.Dxt3SRgb:
                    outputFormat = Format.DXT3;
                    break;
                case SurfaceFormat.Dxt5:
                    outputFormat = Format.DXT5;
                    break;
                case SurfaceFormat.Dxt5SRgb:
                    outputFormat = Format.DXT5;
                    break;
                default:
                    return false;
            }
            
            var colorBitmap = new PixelBitmapContent<Color>(sourceBitmap.Width, sourceBitmap.Height);
            BitmapContent.Copy(sourceBitmap, colorBitmap);
            var sourceData = colorBitmap.GetPixelData();
            var dataHandle = GCHandle.Alloc(sourceData, GCHandleType.Pinned);

            // Small hack here. NVTT wants 8bit data in BGRA.
            // Flip the B and R channels again here.
            GraphicsUtil.BGRAtoRGBA(sourceData);

            // Do all the calls to the NVTT wrapper within this handler
            // so we properly clean up if things blow up.
            try
            {
                var dataPtr = dataHandle.AddrOfPinnedObject();

                var inputOptions = new InputOptions();
                inputOptions.SetTextureLayout(TextureType.Texture2D, colorBitmap.Width, colorBitmap.Height, 1);
                inputOptions.SetMipmapData(dataPtr, colorBitmap.Width, colorBitmap.Height, 1, 0, 0);
                inputOptions.SetMipmapGeneration(false);
                inputOptions.SetGamma(1.0f, 1.0f);
                if (outputFormat != Format.DXT1)
                    inputOptions.SetAlphaMode(AlphaMode.Premultiplied);
                else
                    inputOptions.SetAlphaMode(AlphaMode.None);

                var compressionOptions = new CompressionOptions();
                compressionOptions.SetFormat(outputFormat);
                compressionOptions.SetQuality(Quality.Normal);

                var outputOptions = new OutputOptions();
                outputOptions.SetOutputHeader(false);
                outputOptions.SetOutputOptionsOutputHandler(NvttBeginImage, NvttWriteImage, NvttEndImage);

                var dxtCompressor = new Compressor();
                dxtCompressor.Compress(inputOptions, compressionOptions, outputOptions);
            }
            finally
            {
                dataHandle.Free();
            }

            return true;
        }

        protected override bool TryCopyTo(BitmapContent destinationBitmap, Rectangle sourceRegion, Rectangle destinationRegion)
        {
            SurfaceFormat destinationFormat;
            if (!destinationBitmap.TryGetFormat(out destinationFormat))
                return false;

            SurfaceFormat format;
            TryGetFormat(out format);

            // A shortcut for copying the entire bitmap to another bitmap of the same type and format
            var fullRegion = new Rectangle(0, 0, Width, Height);
            if ((format == destinationFormat) && (sourceRegion == fullRegion) && (sourceRegion == destinationRegion))
            {
                destinationBitmap.SetPixelData(GetPixelData());
                return true;
            }

            // No other support for copying from a DXT texture yet
            return false;
        }
    }
}
