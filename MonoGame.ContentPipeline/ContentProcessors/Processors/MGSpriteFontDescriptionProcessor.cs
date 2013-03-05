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
using System.Diagnostics;

namespace MonoGameContentProcessors.Processors
{
    [ContentProcessor(DisplayName = "MonoGame SpriteFont")]
    public class MGSpriteFontDescriptionProcessor : FontDescriptionProcessor
    {
        int NearestSuperiorPowerOf2(int n)
        {
            int power = 1;
            while (power < n)
                power *= 2;

            return power;
        }

        public override SpriteFontContent Process(FontDescription input, ContentProcessorContext context)
        {
            // Fallback if we aren't buiding for iOS.
            var platform = ContentHelper.GetMonoGamePlatform();
            if (platform != MonoGamePlatform.iOS)
                return base.Process(input, context);

            SpriteFontContent content = base.Process(input, context);
            FieldInfo TextureField = typeof(SpriteFontContent).GetField("texture", BindingFlags.Instance | BindingFlags.NonPublic);
            Texture2DContent texture = (Texture2DContent)TextureField.GetValue(content);
            
            // TODO: This is a very lame way of doing this as we're getting compression artifacts twice, but is the quickest way to get
            // Compressed fonts up and running. The SpriteFontContent/Processor contains a ton
            // of sealed/internal classes riddled with private fields, so overriding CompressFontTexture
            // or even Process is tricky. This works for now, but should be replaced when the content pipeline
            // moves a bit further

            var texWidth = NearestSuperiorPowerOf2(texture.Faces[0][0].Width);
            var texHeight = NearestSuperiorPowerOf2(texture.Faces[0][0].Height);

            // Resize to square, power of two if necessary.
            if (texWidth != texHeight || texture.Faces[0][0].Width != texture.Faces[0][0].Height)
            {
                texHeight = texWidth = Math.Max(texHeight, texWidth);
                var resizedBitmap = (BitmapContent)Activator.CreateInstance(typeof(PixelBitmapContent<Color>), new object[] { texWidth, texHeight });
                var textureRegion = new Rectangle(0, 0, texture.Faces[0][0].Width, texture.Faces[0][0].Height);
                BitmapContent.Copy(texture.Faces[0][0], textureRegion, resizedBitmap, textureRegion);

                texture.Faces[0].Clear();
                texture.Faces[0].Add(resizedBitmap);

                context.Logger.LogImportantMessage("Resized Font Texture (" + resizedBitmap.Width + "x" + resizedBitmap.Height + ")");
            }
            else
            {
                texture.ConvertBitmapType(typeof(PixelBitmapContent<Color>));
            }

            texWidth = texture.Faces[0][0].Width;
            texHeight = texture.Faces[0][0].Height;

            context.Logger.LogImportantMessage("Processed Font Texture (" + texWidth + "x" + texHeight + ")");

            MGTextureProcessor.ConvertToPVRTC(texture, 1, true, MGCompressionMode.PVRTCFourBitsPerPixel);

            return content; 

        }
    }
}
