// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using BCnEncoder.Shared;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Utilities;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides properties and methods for creating and maintaining an ATC compressed bitmap resource.
    /// </summary>
    public abstract class AtcBitmapContent : BitmapContent
    {
        private byte[] _bitmapData = [];

        /// <summary>
        /// Initializes a new instance of AtcBitmapContent.
        /// </summary>
        public AtcBitmapContent()
        {
        }

        /// <summary>
        /// Initializes a new instance of AtcBitmapContent with the specified width or height.
        /// </summary>
        /// <param name="width">Width, in pixels, of the bitmap resource.</param>
        /// <param name="height">Height, in pixels, of the bitmap resource.</param>
        public AtcBitmapContent(int width, int height) : base(width, height)
        {
        }

        /// <inheritdoc/>
        public override byte[] GetPixelData() => _bitmapData;

        /// <inheritdoc/>
        public override void SetPixelData(byte[] sourceData) => _bitmapData = sourceData;

        /// <inheritdoc/>
        protected override bool TryCopyFrom(BitmapContent sourceBitmap, Rectangle sourceRegion, Rectangle destinationRegion)
        {
            if (!sourceBitmap.TryGetFormat(out var sourceFormat))
                return false;

            TryGetFormat(out var format);

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
            if (sourceBitmap is not PixelBitmapContent<Vector4> || sourceRegion.Width != destinationRegion.Width || sourceRegion.Height != destinationRegion.Height)
            {
                try
                {
                    Copy(sourceBitmap, sourceRegion, this, destinationRegion);
                    return true;
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }

            var compressionFormat = format switch
            {
                SurfaceFormat.RgbaAtcExplicitAlpha => CompressionFormat.AtcExplicitAlpha,
                SurfaceFormat.RgbaAtcInterpolatedAlpha => CompressionFormat.AtcInterpolatedAlpha,
                _ => throw new PipelineException(),
            };
            BcnUtil.Encode(
                sourceBitmap: sourceBitmap,
                destinationFormat: compressionFormat,
                out var compressedBytes);

            SetPixelData(compressedBytes);

            return true;
        }

        /// <inheritdoc/>
        protected override bool TryCopyTo(BitmapContent destinationBitmap, Rectangle sourceRegion, Rectangle destinationRegion)
        {
            if (!destinationBitmap.TryGetFormat(out SurfaceFormat destinationFormat))
                return false;

            TryGetFormat(out var format);

            // A shortcut for copying the entire bitmap to another bitmap of the same type and format
            if (format == destinationFormat && (sourceRegion == new Rectangle(0, 0, Width, Height)) && sourceRegion == destinationRegion)
            {
                destinationBitmap.SetPixelData(GetPixelData());
                return true;
            }

            // No other support for copying from an ATC texture yet
            return false;
        }
    }
}
