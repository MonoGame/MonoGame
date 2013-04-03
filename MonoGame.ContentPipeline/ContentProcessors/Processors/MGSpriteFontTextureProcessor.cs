using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using MonoGameContentProcessors.Content;

namespace MonoGameContentProcessors.Processors
{
    [ContentProcessor(DisplayName = "MonoGame SpriteFont from Texture")]
    public class MGSpriteFontTextureProcessor : FontTextureProcessor
    {
        public override SpriteFontContent Process(Texture2DContent input, ContentProcessorContext context)
        {
            // Fallback if we aren't buiding for iOS.
            var platform = ContentHelper.GetMonoGamePlatform();
            if (platform != MonoGamePlatform.iOS)
                return base.Process(input, context);

            SpriteFontContent content = base.Process(input, context);
            
            // TODO: This is a very lame way of doing this as we're getting compression artifacts twice, but is the quickest way to get
            // Compressed fonts up and running. The SpriteFontContent/Processor contains a ton
            // of sealed/internal classes riddled with private fields, so overriding CompressFontTexture
            // or even Process is tricky. This works for now, but should be replaced when the content pipeline
            // moves a bit further

            var texWidth = input.Faces[0][0].Width;
            var texHeight = input.Faces[0][0].Height;

            // Resize to square, power of two if necessary.
            if (texWidth != texHeight)
            {
                texHeight = texWidth = Math.Max(texHeight, texWidth);
                var resizedBitmap = (BitmapContent)Activator.CreateInstance(typeof(PixelBitmapContent<Color>), new object[] { texWidth, texHeight });
                var textureRegion = new Rectangle(0, 0, input.Faces[0][0].Width, input.Faces[0][0].Height);
                BitmapContent.Copy(input.Faces[0][0], textureRegion, resizedBitmap, textureRegion);

                input.Faces[0].Clear();
                input.Faces[0].Add(resizedBitmap);
            }
            else
                input.ConvertBitmapType(typeof(PixelBitmapContent<Color>));

            MGTextureProcessor.ConvertToPVRTC(input, 1, true, MGCompressionMode.PVRTCFourBitsPerPixel);

            return content; 

        }
    }
}
