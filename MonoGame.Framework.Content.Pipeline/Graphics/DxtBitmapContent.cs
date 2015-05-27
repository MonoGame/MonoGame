// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using PVRTexLibNET;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public abstract class DxtBitmapContent : BitmapContent
    {
        internal byte[] _bitmapData;
        internal int _blockSize;

        internal SurfaceFormat _format;

        public DxtBitmapContent(int blockSize)
        {
            if (!((blockSize == 8) || (blockSize == 16)))
                throw new ArgumentException("Invalid block size");
            _blockSize = blockSize;
            TryGetFormat(out _format);
        }

        public DxtBitmapContent(int blockSize, int width, int height)
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

            // Convert to full colour 32-bit format. Floating point would be preferred for processing, but it appears the ATICompressor does not support this
            var colorBitmap = new PixelBitmapContent<Color>(sourceRegion.Width, sourceRegion.Height);
            BitmapContent.Copy(sourceBitmap, sourceRegion, colorBitmap, new Rectangle(0, 0, colorBitmap.Width, colorBitmap.Height));
            sourceBitmap = colorBitmap;

            PixelFormat targetFormat;
            switch (format)
            {
                case SurfaceFormat.Dxt1:
                case SurfaceFormat.Dxt1SRgb:
                    targetFormat = PixelFormat.DXT1;
                    break;
                case SurfaceFormat.Dxt3:
                case SurfaceFormat.Dxt3SRgb:
                    targetFormat = PixelFormat.DXT3;
                    break;
                case SurfaceFormat.Dxt5:
                case SurfaceFormat.Dxt5SRgb:
                    targetFormat = PixelFormat.DXT5;
                    break;
                default:
                    return false;
            }

            try
            {
                // Create the texture object in the PVR library
                var sourceData = sourceBitmap.GetPixelData();
                PVRTexture.CreateTexture(sourceData, (uint)sourceBitmap.Width, (uint)sourceBitmap.Height, 1,
                    PixelFormat.RGBA8888, true, VariableType.UnsignedByte, ColourSpace.lRGB);
                // Resize the bitmap if needed
                if ((sourceBitmap.Width != Width) || (sourceBitmap.Height != Height))
                    PVRTexture.Resize((uint)Width, (uint)Height, 1, ResizeMode.Cubic);
                PVRTexture.Transcode(targetFormat, VariableType.UnsignedByte, ColourSpace.lRGB /*, CompressorQuality.ETCMediumPerceptual, true*/);
                var texDataSize = PVRTexture.GetTextureDataSize(0);
                var texData = new byte[texDataSize];
                PVRTexture.GetTextureData(texData, texDataSize);
                SetPixelData(texData);
            }
            finally
            {
                PVRTexture.DestroyTexture();
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
            if (format == destinationFormat && (sourceRegion == new Rectangle(0, 0, Width, Height)) && sourceRegion == destinationRegion)
            {
                destinationBitmap.SetPixelData(GetPixelData());
                return true;
            }

            // No other support for copying from a DXT texture yet
            return false;
        }
    }
}
