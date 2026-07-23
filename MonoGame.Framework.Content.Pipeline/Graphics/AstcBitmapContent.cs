// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Utilities;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public class AstcBitmapContent : BitmapContent
    {
        private const SurfaceFormat Format = SurfaceFormat.Astc4X4Rgba;
        private byte[] _bitmapData = [];

        /// <summary>
        /// Initializes a new instance of AstcBitmapContent.
        /// </summary>
        protected AstcBitmapContent()
        {
        }

        /// <summary>
        /// Initializes a new instance of AstcBitmapContent with the specified width or height.
        /// </summary>
        /// <param name="width">Width in pixels of the bitmap resource.</param>
        /// <param name="height">Height in pixels of the bitmap resource.</param>
        public AstcBitmapContent(int width, int height) : base(width, height)
        {
        }

        public override byte[] GetPixelData() => _bitmapData;

        public override void SetPixelData(byte[] sourceData) => _bitmapData = sourceData;

        protected override bool TryCopyFrom(BitmapContent sourceBitmap, Rectangle sourceRegion, Rectangle destinationRegion)
        {
            if (!sourceBitmap.TryGetFormat(out var sourceFormat))
                return false;

            TryGetFormat(out var format);

            // A shortcut for copying the entire bitmap to another bitmap of the same type and format
            if (format == sourceFormat && (sourceRegion == new Rectangle(0, 0, Width, Height)) &&
                sourceRegion == destinationRegion)
            {
                SetPixelData(sourceBitmap.GetPixelData());
                return true;
            }

            // Destination region copy is not yet supported
            if (destinationRegion != new Rectangle(0, 0, Width, Height))
                return false;

            // If the source is not Vector4 or requires resizing, send it through BitmapContent.Copy
            if (sourceBitmap is not PixelBitmapContent<Vector4> ||
                sourceRegion.Width != destinationRegion.Width ||
                sourceRegion.Height != destinationRegion.Height)
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

            BasisU.EncodeBytes(
                sourceBitmap: sourceBitmap,
                destinationFormat: format,
                out var compressedBytes);
            SetPixelData(compressedBytes);

            return true;
        }

        protected override bool TryCopyTo(BitmapContent destinationBitmap, Rectangle sourceRegion, Rectangle destinationRegion)
        {
            if (!destinationBitmap.TryGetFormat(out SurfaceFormat destinationFormat))
                return false;

            TryGetFormat(out SurfaceFormat format);

            // A shortcut for copying the entire bitmap to another bitmap of the same type and format
            if (format == destinationFormat &&
                sourceRegion == new Rectangle(0, 0, Width, Height) &&
                sourceRegion == destinationRegion)
            {
                destinationBitmap.SetPixelData(GetPixelData());
                return true;
            }

            // No other support for copying from a ASTC texture yet
            return false;
        }

        public override bool TryGetFormat(out SurfaceFormat format)
        {
            format = Format;
            return true;
        }
    }

    public class Astc5x5BitmapContent : AstcBitmapContent
    {
        /// <summary>
        /// Initializes a new instance of Astc5x5BitmapContent.
        /// </summary>
        protected Astc5x5BitmapContent()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of Astc5x5BitmapContent with the specified width or height.
        /// </summary>
        /// <param name="width">Width in pixels of the bitmap resource.</param>
        /// <param name="height">Height in pixels of the bitmap resource.</param>
        public Astc5x5BitmapContent(int width, int height)
            : base(width, height)
        {
        }

        public override bool TryGetFormat(out SurfaceFormat format)
        {
            format = SurfaceFormat.Astc5X5Rgba;
            return true;
        }
    }

    public class Astc6x6BitmapContent : AstcBitmapContent
    {
        /// <summary>
        /// Initializes a new instance of Astc6x6BitmapContent.
        /// </summary>
        protected Astc6x6BitmapContent()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of Astc6x6BitmapContent with the specified width or height.
        /// </summary>
        /// <param name="width">Width in pixels of the bitmap resource.</param>
        /// <param name="height">Height in pixels of the bitmap resource.</param>
        public Astc6x6BitmapContent(int width, int height)
            : base(width, height)
        {
        }

        public override bool TryGetFormat(out SurfaceFormat format)
        {
            format = SurfaceFormat.Astc6X6Rgba;
            return true;
        }
    }

    public class Astc8x8BitmapContent : AstcBitmapContent
    {
        /// <summary>
        /// Initializes a new instance of Astc8x8BitmapContent.
        /// </summary>
        protected Astc8x8BitmapContent()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of Astc8x8BitmapContent with the specified width or height.
        /// </summary>
        /// <param name="width">Width in pixels of the bitmap resource.</param>
        /// <param name="height">Height in pixels of the bitmap resource.</param>
        public Astc8x8BitmapContent(int width, int height)
            : base(width, height)
        {
        }

        public override bool TryGetFormat(out SurfaceFormat format)
        {
            format = SurfaceFormat.Astc8X8Rgba;
            return true;
        }
    }

    public class Astc10x10BitmapContent : AstcBitmapContent
    {
        /// <summary>
        /// Initializes a new instance of Astc10x10BitmapContent.
        /// </summary>
        protected Astc10x10BitmapContent()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of Astc10x10BitmapContent with the specified width or height.
        /// </summary>
        /// <param name="width">Width in pixels of the bitmap resource.</param>
        /// <param name="height">Height in pixels of the bitmap resource.</param>
        public Astc10x10BitmapContent(int width, int height)
            : base(width, height)
        {
        }

        public override bool TryGetFormat(out SurfaceFormat format)
        {
            format = SurfaceFormat.Astc10X10Rgba;
            return true;
        }
    }

    public class Astc12x12BitmapContent : AstcBitmapContent
    {
        /// <summary>
        /// Initializes a new instance of Astc12x12BitmapContent.
        /// </summary>
        protected Astc12x12BitmapContent()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of Astc12x12BitmapContent with the specified width or height.
        /// </summary>
        /// <param name="width">Width in pixels of the bitmap resource.</param>
        /// <param name="height">Height in pixels of the bitmap resource.</param>
        public Astc12x12BitmapContent(int width, int height)
            : base(width, height)
        {
        }

        public override bool TryGetFormat(out SurfaceFormat format)
        {
            format = SurfaceFormat.Astc12X12Rgba;
            return true;
        }
    }
}
