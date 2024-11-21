// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using BCnEncoder.Shared;
using Microsoft.Xna.Framework.Content.Pipeline.Utilities;
using Microsoft.Xna.Framework.Graphics;


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

        private static void HasAnyAlpha(byte[] data, out bool hasTransparency)
        {
            hasTransparency = false;
            for (var x = 0; x < data.Length; x += 4)
            {
                // Look for non-opaque pixels.
                var alpha = data[x + 3];
                if (alpha < 255)
                {
                    hasTransparency = true;
                    break; // no need to process entire image if we identify alpha early.
                }
            }
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

            // TODO: Add a XNA unit test to see what it does
            // my guess is that this is invalid for DXT.
            //
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

            var colorBitmap = new PixelBitmapContent<Color>(sourceBitmap.Width, sourceBitmap.Height);
            BitmapContent.Copy(sourceBitmap, colorBitmap);
            var sourceData = colorBitmap.GetPixelData();

            HasAnyAlpha(sourceData, out var hasTransparency);
            byte[] compressedBytes = null;
            switch (format)
            {
                case SurfaceFormat.Dxt1 when hasTransparency:
                case SurfaceFormat.Dxt1SRgb when hasTransparency:
                case SurfaceFormat.Dxt1a:
                    BcnUtil.Encode(
                        sourceBitmap: sourceBitmap,
                        destinationFormat: CompressionFormat.Bc1WithAlpha,
                        out compressedBytes);
                    break;
                case SurfaceFormat.Dxt1:
                case SurfaceFormat.Dxt1SRgb:

                    BcnUtil.Encode(
                        sourceBitmap: sourceBitmap,
                        destinationFormat: CompressionFormat.Bc1,
                        out compressedBytes);
                    break;
                case SurfaceFormat.Dxt3SRgb:
                case SurfaceFormat.Dxt3:
                    // the bcnEncoder does not perform well enough encoding BC2/DXT3
                    //  specifically, the alpha channel seems to be off. Maybe it has something to do with the premult?
                    Crunch.EncodeBytes(
                        sourceBitmap: sourceBitmap,
                        crunchFormat: CrunchFormat.Dxt3,
                        out compressedBytes);
                    break;
                case SurfaceFormat.Dxt5:
                case SurfaceFormat.Dxt5SRgb:
                    BcnUtil.Encode(
                        sourceBitmap: sourceBitmap,
                        destinationFormat: CompressionFormat.Bc3,
                        out compressedBytes);
                    break;
                default:
                    throw new PipelineException($"{nameof(DxtBitmapContent)} cannot compress format=[{format}]");
            }

            SetPixelData(compressedBytes);

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
