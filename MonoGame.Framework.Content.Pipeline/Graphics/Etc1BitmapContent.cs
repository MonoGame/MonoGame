// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Graphics;
using PVRTexLibNET;
using System;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public class Etc1BitmapContent : BitmapContent
    {
        byte[] _data;

        /// <summary>
        /// Initializes a new instance of Etc1BitmapContent.
        /// </summary>
        protected Etc1BitmapContent()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of Etc1BitmapContent with the specified width or height.
        /// </summary>
        /// <param name="width">Width in pixels of the bitmap resource.</param>
        /// <param name="height">Height in pixels of the bitmap resource.</param>
        public Etc1BitmapContent(int width, int height)
            : base(width, height)
        {
        }

        public override byte[] GetPixelData()
        {
            return _data;
        }

        public override void SetPixelData(byte[] sourceData)
        {
            int bytesRequired = ((Width + 3) >> 2) * ((Height + 3) >> 2) * SurfaceFormat.RgbEtc1.GetSize();
            if (bytesRequired != sourceData.Length)
                throw new ArgumentException("ETC1 bitmap with width " + Width + " and height " + Height + " needs "
                    + bytesRequired + " bytes. Received " + sourceData.Length + " bytes");

            if (_data == null || _data.Length != bytesRequired)
                _data = new byte[bytesRequired];
            Buffer.BlockCopy(sourceData, 0, _data, 0, bytesRequired);
        }

        protected override bool TryCopyFrom(BitmapContent sourceBitmap, Rectangle sourceRegion, Rectangle destinationRegion)
        {
            // Region copy is not supported.
            if (destinationRegion.Top != 0 ||
                destinationRegion.Left != 0 ||
                destinationRegion.Width != Width ||
                destinationRegion.Height != Height)
                return false;
            if (sourceRegion.Top != 0 ||
                sourceRegion.Left != 0 ||
                sourceRegion.Width != sourceBitmap.Width ||
                sourceRegion.Height != sourceBitmap.Height)
                return false;

            // If needed, convert to floating point format
            if (!(sourceBitmap is PixelBitmapContent<Color>))
            {
                var colorBitmap = new PixelBitmapContent<Color>(sourceBitmap.Width, sourceBitmap.Height);
                BitmapContent.Copy(sourceBitmap, colorBitmap);
                sourceBitmap = colorBitmap;
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
                PVRTexture.Transcode(PixelFormat.ETC1, VariableType.UnsignedByte, ColourSpace.lRGB /*, CompressorQuality.ETCMediumPerceptual, true*/);
                var etc1DataSize = PVRTexture.GetTextureDataSize(0);
                var etc1Data = new byte[etc1DataSize];
                PVRTexture.GetTextureData(etc1Data, etc1DataSize);
                SetPixelData(etc1Data);
            }
            finally
            {
                PVRTexture.DestroyTexture();
            }
            return true;
        }

        protected override bool TryCopyTo(BitmapContent destinationBitmap, Rectangle sourceRegion, Rectangle destinationRegion)
        {
            // No support for copying from a ETC1 texture yet
            return false;
        }

        /// <summary>
        /// Gets the corresponding GPU texture format for the specified bitmap type.
        /// </summary>
        /// <param name="format">Format being retrieved.</param>
        /// <returns>The GPU texture format of the bitmap type.</returns>
        public override bool TryGetFormat(out SurfaceFormat format)
        {
            format = SurfaceFormat.RgbEtc1;
            return true;
        }
    }
}
