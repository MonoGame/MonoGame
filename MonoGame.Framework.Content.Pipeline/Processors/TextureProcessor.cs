// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.ComponentModel;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;

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

        [DefaultValueAttribute(typeof(Color), "255,0,255,255")]
        public virtual Color ColorKeyColor { get; set; }

        [DefaultValueAttribute(true)]
        public virtual bool ColorKeyEnabled { get; set; }

        public virtual bool GenerateMipmaps { get; set; }

        [DefaultValueAttribute(true)]
        public virtual bool PremultiplyAlpha { get; set; }

        public virtual bool ResizeToPowerOfTwo { get; set; }

        public virtual bool MakeSquare { get; set; }

        public virtual TextureProcessorOutputFormat TextureFormat { get; set; }

        public override TextureContent Process(TextureContent input, ContentProcessorContext context)
        {
            SurfaceFormat format;
            if (input.Faces[0][0].TryGetFormat(out format))
            {
                // If it is already a compressed format, we cannot do anything else so just return it
                if (format.IsCompressedFormat())
                    return input;
            }

            if (ColorKeyEnabled || ResizeToPowerOfTwo || MakeSquare || PremultiplyAlpha)
            {
                // Convert to floating point format for modifications. Keep the original format for conversion back later on if required.
                var originalType = input.Faces[0][0].GetType();
                try
                {
                    input.ConvertBitmapType(typeof(PixelBitmapContent<Vector4>));
                }
                catch (Exception ex)
                {
                    context.Logger.LogImportantMessage("Could not convert input texture for processing. " + ex.ToString());
                    throw ex; 
                }

                for (int f = 0; f < input.Faces.Count; ++f)
                {
                    var face = input.Faces[f];
                    for (int m = 0; m < face.Count; ++m)
                    {
                        var bmp = (PixelBitmapContent<Vector4>)face[m];

                        if (ColorKeyEnabled)
                        {
                            bmp.ReplaceColor(ColorKeyColor.ToVector4(), Vector4.Zero);
                        }

                        if (ResizeToPowerOfTwo)
                        {
                            if (!GraphicsUtil.IsPowerOfTwo(bmp.Width) || !GraphicsUtil.IsPowerOfTwo(bmp.Height) || (MakeSquare && bmp.Height != bmp.Width))
                            {
                                var newWidth = GraphicsUtil.GetNextPowerOfTwo(bmp.Width);
                                var newHeight = GraphicsUtil.GetNextPowerOfTwo(bmp.Height);
                                if (MakeSquare)
                                    newWidth = newHeight = Math.Max(newWidth, newHeight);
                                var resized = new PixelBitmapContent<Vector4>(newWidth, newHeight);
                                BitmapContent.Copy(bmp, resized);
                                bmp = resized;
                            }
                        }
                        else if (MakeSquare && bmp.Height != bmp.Width)
                        {
                            var newSize = Math.Max(bmp.Width, bmp.Height);
                            var resized = new PixelBitmapContent<Vector4>(newSize, newSize);
                            BitmapContent.Copy(bmp, resized);
                        }

                        if (PremultiplyAlpha)
                        {
                            for (int y = 0; y < bmp.Height; ++y)
                            {
                                var row = bmp.GetRow(y);
                                for (int x = 0; x < bmp.Width; ++x)
                                    row[x] = Color.FromNonPremultiplied(row[x]).ToVector4();
                            }
                        }

                        face[m] = bmp;
                    }
                }

                // If no change to the surface format was desired, change it back now before it early outs
                if (TextureFormat == TextureProcessorOutputFormat.NoChange)
                    input.ConvertBitmapType(originalType);
            }

            if (TextureFormat == TextureProcessorOutputFormat.NoChange)
                return input;
			
			try 
			{
			    if (TextureFormat != TextureProcessorOutputFormat.Color)
				{
					input.ConvertBitmapType(typeof(PixelBitmapContent<Vector4>));
                	GraphicsUtil.CompressTexture(context.TargetProfile, input, TextureFormat, context, GenerateMipmaps, false);
				}
                else
                {
                    input.ConvertBitmapType(typeof(PixelBitmapContent<Color>));
                    if (GenerateMipmaps)
                        input.GenerateMipmaps(false);
                }
			}
			catch (EntryPointNotFoundException ex)
            {
				context.Logger.LogImportantMessage ("Could not find the entry point to compress the texture. " + ex.ToString());
				throw ex;
			}
			catch (DllNotFoundException ex)
            {
				context.Logger.LogImportantMessage ("Could not compress texture. Required shared lib is missing. " + ex.ToString());
				throw ex;
			}
			catch (Exception ex)
			{
				context.Logger.LogImportantMessage ("Could not convert texture. " + ex.ToString());
				throw ex;
			}

            return input;
        }
    }
}
