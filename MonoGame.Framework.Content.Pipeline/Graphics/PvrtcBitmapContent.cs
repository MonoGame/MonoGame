// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using PVRTexLibNET;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public abstract class PvrtcBitmapContent : BitmapContent
    {
        internal byte[] _bitmapData;

        public PvrtcBitmapContent(int width, int height)
            : base(width, height)
        {
        }

        int GetDataSize()
        {
            SurfaceFormat format;
            TryGetFormat(out format);
            switch (format)
            {
                case SurfaceFormat.RgbPvrtc2Bpp:
                case SurfaceFormat.RgbaPvrtc2Bpp:
                    return (Math.Max(Width, 16) * Math.Max(Height, 8) * 2 + 7) / 8;

                case SurfaceFormat.RgbPvrtc4Bpp:
                case SurfaceFormat.RgbaPvrtc4Bpp:
                    return (Math.Max(Width, 8) * Math.Max(Height, 8) * 4 + 7) / 8;
            }

            return 0;
        }

        public override byte[] GetPixelData()
        {
            if (_bitmapData == null)
                throw new InvalidOperationException("No data set on bitmap");
            var result = new byte[_bitmapData.Length];
            Buffer.BlockCopy(_bitmapData, 0, result, 0, _bitmapData.Length);
            return result;
        }

        public override void SetPixelData(byte[] sourceData)
        {
            var size = GetDataSize();
            if (sourceData.Length != size)
                throw new ArgumentException("Incorrect data size. Expected " + size + " bytes");
            if (_bitmapData == null || _bitmapData.Length != size)
                _bitmapData = new byte[size];
            Buffer.BlockCopy(sourceData, 0, _bitmapData, 0, size);
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
            switch (format)
            {
                case SurfaceFormat.RgbPvrtc2Bpp:
                    targetFormat = PixelFormat.PVRTCI_2bpp_RGB;
                    break;
                case SurfaceFormat.RgbaPvrtc2Bpp:
                    targetFormat = PixelFormat.PVRTCI_2bpp_RGBA;
                    break;
                case SurfaceFormat.RgbPvrtc4Bpp:
                    targetFormat = PixelFormat.PVRTCI_4bpp_RGB;
                    break;
                case SurfaceFormat.RgbaPvrtc4Bpp:
                    targetFormat = PixelFormat.PVRTCI_4bpp_RGBA;
                    break;
                default:
                    return false;
            }

            // Create the texture object in the PVR library
            var sourceData = sourceBitmap.GetPixelData();
            var rgba32F = (PixelFormat)0x2020202061626772; // static const PixelType PVRStandard32PixelType = PixelType('r', 'g', 'b', 'a', 32, 32, 32, 32);
            using (var pvrTexture = PVRTexture.CreateTexture(sourceData, (uint)sourceBitmap.Width, (uint)sourceBitmap.Height, 1,
                rgba32F, true, VariableType.Float, ColourSpace.lRGB))
            {
                // Resize the bitmap if needed
                if ((sourceBitmap.Width != Width) || (sourceBitmap.Height != Height))
                    pvrTexture.Resize((uint)Width, (uint)Height, 1, ResizeMode.Cubic);
                // On Linux, anything less than CompressorQuality.PVRTCHigh crashes in libpthread.so at the end of compression
                pvrTexture.Transcode(targetFormat, VariableType.UnsignedByte, ColourSpace.lRGB, CompressorQuality.PVRTCHigh);
                var texDataSize = pvrTexture.GetTextureDataSize(0);
                var texData = new byte[texDataSize];
                pvrTexture.GetTextureData(texData, texDataSize);
                SetPixelData(texData);
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

            // No other support for copying from a PVR texture yet
            return false;
        }
    }
}
