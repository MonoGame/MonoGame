// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides properties and methods for creating and maintaining a bitmap resource.
    /// </summary>
    public abstract class BitmapContent : ContentItem
    {
        int height;
        int width;

        /// <summary>
        /// Gets or sets the height of the bitmap, in pixels.
        /// </summary>
        public int Height
        {
            get
            {
                return height;
            }
            protected set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("height");
                height = value;
            }
        }

        /// <summary>
        /// Gets or sets the width of the bitmap, in pixels.
        /// </summary>
        public int Width
        {
            get
            {
                return width;
            }
            protected set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("width");
                width = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of BitmapContent.
        /// </summary>
        protected BitmapContent()
        {
        }

        /// <summary>
        /// Initializes a new instance of BitmapContent with the specified width or height.
        /// </summary>
        /// <param name="width">Width, in pixels, of the bitmap resource.</param>
        /// <param name="height">Height, in pixels, of the bitmap resource.</param>
        protected BitmapContent(int width, int height)
        {
            // Write to properties so validation is run.
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Copies one bitmap into another.
        /// The destination bitmap can be in any format and size. If the destination is larger or smaller, the source bitmap is scaled accordingly.
        /// </summary>
        /// <param name="sourceBitmap">BitmapContent being copied.</param>
        /// <param name="destinationBitmap">BitmapContent being overwritten.</param>
        public static void Copy(BitmapContent sourceBitmap, BitmapContent destinationBitmap)
        {
            if (sourceBitmap == null)
                throw new ArgumentNullException("sourceBitmap");
            if (destinationBitmap == null)
                throw new ArgumentNullException("destinationBitmap");

            var sourceRegion = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
            var destinationRegion = new Rectangle(0, 0, destinationBitmap.Width, destinationBitmap.Height);

            Copy(sourceBitmap, sourceRegion, destinationBitmap, destinationRegion);
        }

        /// <summary>
        /// Copies one bitmap into another.
        /// The destination bitmap can be in any format and size. If the destination is larger or smaller, the source bitmap is scaled accordingly.
        /// </summary>
        /// <param name="sourceBitmap">BitmapContent being copied.</param>
        /// <param name="sourceRegion">Region of sourceBitmap.</param>
        /// <param name="destinationBitmap">BitmapContent being overwritten.</param>
        /// <param name="destinationRegion">Region of bitmap to be overwritten.</param>
        public static void Copy(BitmapContent sourceBitmap, Rectangle sourceRegion, BitmapContent destinationBitmap, Rectangle destinationRegion)
        {
            ValidateCopyArguments(sourceBitmap, sourceRegion, destinationBitmap, destinationRegion);

            SurfaceFormat sourceFormat;
            if (!sourceBitmap.TryGetFormat(out sourceFormat))
                throw new InvalidOperationException("Could not retrieve surface format of source bitmap");
            SurfaceFormat destinationFormat;
            if (!destinationBitmap.TryGetFormat(out destinationFormat))
                throw new InvalidOperationException("Could not retrieve surface format of destination bitmap");

            // If the formats are the same and the regions are the full bounds of the bitmaps and they are the same size, do a simpler copy
            if (sourceFormat == destinationFormat && sourceRegion == destinationRegion
                && sourceRegion == new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height)
                && destinationRegion == new Rectangle(0, 0, destinationBitmap.Width, destinationBitmap.Height))
            {
                destinationBitmap.SetPixelData(sourceBitmap.GetPixelData());
                return;
            }

            // The basic process is
            // 1. Copy from source bitmap region to a new PixelBitmapContent<Vector4> using sourceBitmap.TryCopyTo()
            // 2. If source and destination regions are a different size, resize Vector4 version
            // 3. Copy from Vector4 to destination region using destinationBitmap.TryCopyFrom()

            // Copy from the source to the intermediate Vector4 format
            var intermediate = new PixelBitmapContent<Vector4>(sourceRegion.Width, sourceRegion.Height);
            var intermediateRegion = new Rectangle(0, 0, intermediate.Width, intermediate.Height);
            if (sourceBitmap.TryCopyTo(intermediate, sourceRegion, intermediateRegion))
            {
                // Resize the intermediate if required
                if (intermediate.Width != destinationRegion.Width || intermediate.Height != destinationRegion.Height)
                    intermediate = intermediate.Resize(destinationRegion.Width, destinationRegion.Height) as PixelBitmapContent<Vector4>;
                // Copy from the intermediate to the destination
                if (destinationBitmap.TryCopyFrom(intermediate, new Rectangle(0, 0, intermediate.Width, intermediate.Height), destinationRegion))
                    return;
            }

            // If we got here, one of the above steps didn't work
            throw new InvalidOperationException("Could not copy between " + sourceFormat + " and " + destinationFormat);
        }

        /// <summary>
        /// Reads encoded bitmap content.
        /// </summary>
        /// <returns>Array containing encoded bitmap data.</returns>
        public abstract byte[] GetPixelData();

        /// <summary>
        /// Writes encoded bitmap content.
        /// </summary>
        /// <param name="sourceData">Array containing encoded bitmap data to be set.</param>
        public abstract void SetPixelData(byte[] sourceData);

        /// <summary>
        /// Returns a string description of the bitmap resource.
        /// </summary>
        /// <returns>Description of the bitmap.</returns>
        public override string ToString()
        {
            return string.Format("{0}, {1}x{2}", GetType().Name, Width, Height);
        }

        /// <summary>
        /// Attempts to copy a region from a specified bitmap.
        /// </summary>
        /// <param name="sourceBitmap">BitmapContent being copied.</param>
        /// <param name="sourceRegion">Location of sourceBitmap.</param>
        /// <param name="destinationRegion">Region of destination bitmap to be overwritten.</param>
        /// <returns>true if region copy is supported; false otherwise.</returns>
        protected abstract bool TryCopyFrom(BitmapContent sourceBitmap, Rectangle sourceRegion, Rectangle destinationRegion);

        /// <summary>
        /// Attempts to copy a region of the specified bitmap onto another.
        /// </summary>
        /// <param name="destinationBitmap">BitmapContent being overwritten.</param>
        /// <param name="sourceRegion">Location of the source bitmap.</param>
        /// <param name="destinationRegion">Region of destination bitmap to be overwritten.</param>
        /// <returns>true if region copy is supported; false otherwise.</returns>
        protected abstract bool TryCopyTo(BitmapContent destinationBitmap, Rectangle sourceRegion, Rectangle destinationRegion);

        /// <summary>
        /// Gets the corresponding GPU texture format for the specified bitmap type.
        /// </summary>
        /// <param name="format">Format being retrieved.</param>
        /// <returns>The GPU texture format of the bitmap type.</returns>
        public abstract bool TryGetFormat(out SurfaceFormat format);

        /// <summary>
        /// Validates the arguments to the Copy function.
        /// </summary>
        /// <param name="sourceBitmap">BitmapContent being copied.</param>
        /// <param name="sourceRegion">Location of sourceBitmap.</param>
        /// <param name="destinationBitmap">BitmapContent being overwritten.</param>
        /// <param name="destinationRegion">Region of bitmap to be overwritten.</param>
        protected static void ValidateCopyArguments(BitmapContent sourceBitmap, Rectangle sourceRegion, BitmapContent destinationBitmap, Rectangle destinationRegion)
        {
            if (sourceBitmap == null)
                throw new ArgumentNullException("sourceBitmap");
            if (destinationBitmap == null)
                throw new ArgumentNullException("destinationBitmap");
            // Make sure regions are within the bounds of the bitmaps
            if (sourceRegion.Left < 0
                || sourceRegion.Top < 0
                || sourceRegion.Width <= 0
                || sourceRegion.Height <= 0
                || sourceRegion.Right > sourceBitmap.Width
                || sourceRegion.Bottom > sourceBitmap.Height)
                throw new ArgumentOutOfRangeException("sourceRegion");
            if (destinationRegion.Left < 0
                || destinationRegion.Top < 0
                || destinationRegion.Width <= 0
                || destinationRegion.Height <= 0
                || destinationRegion.Right > destinationBitmap.Width
                || destinationRegion.Bottom > destinationBitmap.Height)
                throw new ArgumentOutOfRangeException("destinationRegion");
        }
    }
}
