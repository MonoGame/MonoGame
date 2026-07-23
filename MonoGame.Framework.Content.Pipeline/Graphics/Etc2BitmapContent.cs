// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline.Utilities;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Supports the processing of a texture compressed using ETC2.
    /// </summary>
    public class Etc2BitmapContent : BitmapContent
    {
        private const SurfaceFormat Format = SurfaceFormat.Rgba8Etc2;
        private byte[] _data = [];

        /// <summary>
        /// Initializes a new instance of Etc2BitmapContent.
        /// </summary>
        protected Etc2BitmapContent()
        {
        }

        /// <summary>
        /// Initializes a new instance of Etc2BitmapContent with the specified width or height.
        /// </summary>
        /// <param name="width">Width in pixels of the bitmap resource.</param>
        /// <param name="height">Height in pixels of the bitmap resource.</param>
        public Etc2BitmapContent(int width, int height) : base(width, height)
        {
        }

        public override byte[] GetPixelData() => _data;

        public override void SetPixelData(byte[]? sourceData)
        {
            var bytesRequired = ((Width + 3) >> 2) * ((Height + 3) >> 2) * Format.GetSize();
            if (bytesRequired != (sourceData?.Length ?? 0))
                throw new ArgumentException($"ETC2 bitmap with width {Width} and height {Height} needs {bytesRequired} bytes. Received {sourceData?.Length ?? 0} bytes");

            if (sourceData == null || sourceData.Length == 0)
            {
                _data = [];
                return;
            }

            if (_data == null || _data.Length != bytesRequired)
            {
                _data = new byte[bytesRequired];
            }

            Buffer.BlockCopy(sourceData, 0, _data, 0, bytesRequired);
        }

        protected override bool TryCopyFrom(BitmapContent sourceBitmap, Rectangle sourceRegion, Rectangle destinationRegion)
        {
            if (!sourceBitmap.TryGetFormat(out var sourceFormat))
                return false;

            TryGetFormat(out _);

            // A shortcut for copying the entire bitmap to another bitmap of the same type and format
            if (Format == sourceFormat && (sourceRegion == new Rectangle(0, 0, Width, Height)) && sourceRegion == destinationRegion)
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

            CrunchHelpers.EncodeBytes(
                sourceBitmap: sourceBitmap,
                crunchFormat: CrunchFormat.Etc2A,
                out var compressedBytes);
            SetPixelData(compressedBytes);

            return true;
        }

        protected override bool TryCopyTo(BitmapContent destinationBitmap, Rectangle sourceRegion, Rectangle destinationRegion)
        {
            if (!destinationBitmap.TryGetFormat(out var destinationFormat))
                return false;

            // A shortcut for copying the entire bitmap to another bitmap of the same type and format
            if (Format == destinationFormat && (sourceRegion == new Rectangle(0, 0, Width, Height)) && sourceRegion == destinationRegion)
            {
                destinationBitmap.SetPixelData(GetPixelData());
                return true;
            }

            // No other support for copying from a ETC2 texture yet
            return false;
        }

        /// <summary>
        /// Gets the corresponding GPU texture format for the specified bitmap type.
        /// </summary>
        /// <param name="format">Format being retrieved.</param>
        /// <returns>The GPU texture format of the bitmap type.</returns>
        public override bool TryGetFormat(out SurfaceFormat format)
        {
            format = Format;
            return true;
        }

        /// <summary>
        /// Returns a string description of the bitmap.
        /// </summary>
        /// <returns>Description of the bitmap.</returns>
        public override string ToString() => $"ETC2 {Width}x{Height}";
    }
}
