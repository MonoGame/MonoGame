#region License
/*
 Microsoft Public License (Ms-PL)
 MonoGame - Copyright © 2012 The MonoGame Team
 
 All rights reserved.
 
 This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
 accept the license, do not use the software.
 
 1. Definitions
 The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
 U.S. copyright law.
 
 A "contribution" is the original software, or any additions or changes to the software.
 A "contributor" is any person that distributes its contribution under this license.
 "Licensed patents" are a contributor's patent claims that read directly on its contribution.
 
 2. Grant of Rights
 (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
 each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
 (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
 each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
 
 3. Conditions and Limitations
 (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
 (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
 your patent license from such contributor to the software ends automatically.
 (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
 notices that are present in the software.
 (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
 a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
 code form, you may only do so under a license that complies with this license.
 (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
 or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
 permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
 purpose and non-infringement.
 */
#endregion License

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
            set
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
            set
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
            Copy(sourceBitmap, new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height), destinationBitmap, new Rectangle(0, 0, destinationBitmap.Width, destinationBitmap.Height));
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
            throw new NotImplementedException();
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
            // See what Microsoft's implementation returns
            throw new NotImplementedException();
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
