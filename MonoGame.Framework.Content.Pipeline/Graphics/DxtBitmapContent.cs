// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;

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

            // set squish format
            TextureSquish.CompressionMode outputFormat = TextureSquish.CompressionMode.Dxt1;
            switch (format)
            {
                case SurfaceFormat.Dxt1:
                    outputFormat = TextureSquish.CompressionMode.Dxt1;
                    break;
                case SurfaceFormat.Dxt1SRgb:
                    outputFormat = TextureSquish.CompressionMode.Dxt1;
                    break;
                case SurfaceFormat.Dxt3:
                    outputFormat = TextureSquish.CompressionMode.Dxt3;
                    break;
                case SurfaceFormat.Dxt3SRgb:
                    outputFormat = TextureSquish.CompressionMode.Dxt3;
                    break;
                case SurfaceFormat.Dxt5:
                    outputFormat = TextureSquish.CompressionMode.Dxt5;
                    break;
                case SurfaceFormat.Dxt5SRgb:
                    outputFormat = TextureSquish.CompressionMode.Dxt5;
                    break;
                default:
                    return false;
            }

            // libsquish requires RGBA8888
            var colorBitmap = new PixelBitmapContent<Color>(sourceBitmap.Width, sourceBitmap.Height);
            BitmapContent.Copy(sourceBitmap, colorBitmap);            

            var sourceTexture = new TextureSquish.Bitmap(colorBitmap.GetPixelData(), sourceBitmap.Width, sourceBitmap.Height);            

            // set quality
            outputFormat |= TextureSquish.CompressionMode.ColourIterativeClusterFit;

            // use multithreading for faster compression
            outputFormat |= TextureSquish.CompressionMode.UseParallelProcessing;            

            var data = sourceTexture.Compress(outputFormat);
            this.SetPixelData(data);            

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
