using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;  
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace MonoGameContentProcessors.Content
{
    public enum MGCompressionMode
    {
        PVRTCTwoBitsPerPixel,
        PVRTCFourBitsPerPixel,
        NoCompression
    }

    class MGBitmapContent : BitmapContent
    {
        private MGCompressionMode _bpp;
        private byte[] _pixelData;

        public MGBitmapContent() { }

        public MGBitmapContent(byte[] pData, int width, int height, MGCompressionMode bpp) :
            base(width, height)
        {
            _pixelData = pData;
            _bpp = bpp;
        }

        public override byte[] GetPixelData()
        {
            return (byte[])this._pixelData.Clone();
        }

        public override void SetPixelData(byte[] sourceData)
        {
            this._pixelData = (byte[])sourceData.Clone();
        }

        protected override bool TryCopyFrom(BitmapContent sourceBitmap, Rectangle sourceRegion, Rectangle destinationRegion)
        {
            throw new NotImplementedException();
            //return BitmapContent.InteropCopy(sourceBitmap, sourceRegion, this, destinationRegion);
        }

        protected override bool TryCopyTo(BitmapContent destinationBitmap, Rectangle sourceRegion, Rectangle destinationRegion)
        {
            throw new NotImplementedException();

            // return BitmapContent.InteropCopy(this, sourceRegion, destinationBitmap, destinationRegion);

        }

        // Because of a Validate in Texture2DContent, we cannot simply cast the correct format.
        // For IOS, since we'll never be using DXT compression, we'll pass DXT5 for PVRTC4BPP, and DXT3
        // for PVRTC2BPP. We'll unpack and properly process this at runtime in the Texture2D Reader.
        public override bool TryGetFormat(out SurfaceFormat format)
        {
            //TODO: Return the correct SurfaceFormat for proper MonoGame processing.
            //format = SurfaceFormat.RgbaPvrtc4Bpp;

            switch(_bpp)
            {
                case MGCompressionMode.PVRTCFourBitsPerPixel:
                    format = SurfaceFormat.Dxt5;
                    break;
                case MGCompressionMode.PVRTCTwoBitsPerPixel:
                    format = SurfaceFormat.Dxt3;
                    break;
                case MGCompressionMode.NoCompression:
                    format = SurfaceFormat.Color;
                    break;

                default:
                    format = SurfaceFormat.Dxt5;
                    break;
            }

            return true;
        }

 



    }
}
