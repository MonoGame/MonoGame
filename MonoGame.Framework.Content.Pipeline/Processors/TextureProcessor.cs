﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.ComponentModel;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    [ContentProcessor(DisplayName="Texture - MonoGame")]
    public class TextureProcessor : ContentProcessor<TextureContent, TextureContent>
    {
        public TextureProcessor()
        {
            ColorKeyColor = new Color(255, 0, 255, 255);
            ColorKeyEnabled = true;
            PremultiplyAlpha = true;
        }

        public virtual Color ColorKeyColor { get; set; }

        public virtual bool ColorKeyEnabled { get; set; }

        public virtual bool GenerateMipmaps { get; set; }

        [DefaultValueAttribute(true)]
        public virtual bool PremultiplyAlpha { get; set; }

        public virtual bool ResizeToPowerOfTwo { get; set; }

        public virtual bool MakeSquare { get; set; }

        public virtual TextureProcessorOutputFormat TextureFormat { get; set; }

        public override TextureContent Process(TextureContent input, ContentProcessorContext context)
        {
            var bmp = input.Faces[0][0];

            if (ColorKeyEnabled)
            {
                var data = bmp.GetPixelData();
                var idx = 0;
                for (; idx < data.Length; )
                {
                    var r = data[idx + 0];
                    var g = data[idx + 1];
                    var b = data[idx + 2];
                    var a = data[idx + 3];
                    var col = new Color(r, g, b, a);
                    if (col.Equals(ColorKeyColor))
                    {
                        data[idx + 0] = 0;
                        data[idx + 1] = 0;
                        data[idx + 2] = 0;
                        data[idx + 3] = 0;    
                    }                    

                    idx += 4;
                }

                bmp.SetPixelData(data);
            }

            
            if (ResizeToPowerOfTwo)
            {
                if (!GraphicsUtil.IsPowerOfTwo(bmp.Width) || !GraphicsUtil.IsPowerOfTwo(bmp.Height) || (MakeSquare && bmp.Height != bmp.Width))
                {
                    var newWidth = GraphicsUtil.GetNextPowerOfTwo(bmp.Width);
                    var newHeight = GraphicsUtil.GetNextPowerOfTwo(bmp.Height);
                    if (MakeSquare)
                        newWidth = newHeight = Math.Max(newWidth, newHeight);
                    input.Resize(newWidth, newHeight);
                    bmp = input.Faces[0][0];
                }
            }
            else if (MakeSquare && bmp.Height != bmp.Width)
            {
                var newSize = Math.Max(bmp.Width, bmp.Height);
                input.Resize(newSize, newSize);
                bmp = input.Faces[0][0];
            }

            if (PremultiplyAlpha)
            {                
                var data = bmp.GetPixelData();
                var idx = 0;
                for (; idx < data.Length;)
                {
                    var r = data[idx + 0];
                    var g = data[idx + 1];
                    var b = data[idx + 2];
                    var a = data[idx + 3];
                    var col = Color.FromNonPremultiplied(r, g, b, a);

                    data[idx + 0] = col.R;
                    data[idx + 1] = col.G;
                    data[idx + 2] = col.B;
                    data[idx + 3] = col.A;

                    idx += 4;
                }
         
                bmp.SetPixelData(data);
            }

            if (TextureFormat == TextureProcessorOutputFormat.NoChange)
                return input;
			
			try 
			{
			    if (TextureFormat != TextureProcessorOutputFormat.Color)
                	GraphicsUtil.CompressTexture(context.TargetProfile, input, TextureFormat, context, GenerateMipmaps, PremultiplyAlpha, false);
			}
			catch(EntryPointNotFoundException ex) {
				context.Logger.LogImportantMessage ("Could not find the entry point to compress the texture", ex.ToString());
				TextureFormat = TextureProcessorOutputFormat.Color;
			}
			catch(DllNotFoundException ex) {
				context.Logger.LogImportantMessage ("Could not compress texture. Required shared lib is missing. {0}", ex.ToString());
				TextureFormat = TextureProcessorOutputFormat.Color;
			}
			catch(Exception ex)
			{
				context.Logger.LogImportantMessage ("Could not compress texture {0}", ex.ToString());
				TextureFormat = TextureProcessorOutputFormat.Color;
			}

            // Generate mipmaps for the non-compressed textures.
            if (GenerateMipmaps && TextureFormat == TextureProcessorOutputFormat.Color)
            {
                context.Logger.LogMessage("Generating mipmaps.");
                input.GenerateMipmaps(false);
            }

            return input;
        }


    }
}
