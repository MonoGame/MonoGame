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

            PixelFormat targetFormat;
            ColourSpace colourSpace;
            switch (format)
            {
                case SurfaceFormat.Dxt1:
                    targetFormat = PixelFormat.DXT1;
                    colourSpace = ColourSpace.lRGB;
                    break;
                case SurfaceFormat.Dxt1SRgb:
                    targetFormat = PixelFormat.DXT1;
                    colourSpace = ColourSpace.sRGB;
                    break;
                case SurfaceFormat.Dxt3:
                    targetFormat = PixelFormat.DXT3;
                    colourSpace = ColourSpace.lRGB;
                    break;
                case SurfaceFormat.Dxt3SRgb:
                    targetFormat = PixelFormat.DXT3;
                    colourSpace = ColourSpace.sRGB;
                    break;
                case SurfaceFormat.Dxt5:
                    targetFormat = PixelFormat.DXT5;
                    colourSpace = ColourSpace.lRGB;
                    break;
                case SurfaceFormat.Dxt5SRgb:
                    targetFormat = PixelFormat.DXT5;
                    colourSpace = ColourSpace.sRGB;
                    break;
                default:
                    return false;
            }

            try
            {
                // Create the texture object in the PVR library
                var sourceData = sourceBitmap.GetPixelData();
                var rgba32F = (PixelFormat)0x2020202061626772; // static const PixelType PVRStandard32PixelType = PixelType('r', 'g', 'b', 'a', 32, 32, 32, 32);
                PVRTexture.CreateTexture(sourceData, (uint)sourceBitmap.Width, (uint)sourceBitmap.Height, 1,
                    rgba32F, true, VariableType.Float, colourSpace);
                // Resize the bitmap if needed
                if ((sourceBitmap.Width != Width) || (sourceBitmap.Height != Height))
                    PVRTexture.Resize((uint)Width, (uint)Height, 1, ResizeMode.Cubic);
                if (!PVRTexture.Transcode(targetFormat, VariableType.UnsignedByteNorm, colourSpace))
                    return false;
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
