// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
using System;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.ComponentModel;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
	[ContentProcessorAttribute(DisplayName="Font Texture - MonoGame")]
	public class FontTextureProcessor : ContentProcessor<Texture2DContent, SpriteFontContent>
	{
		private System.Drawing.Color transparentPixel = System.Drawing.Color.FromArgb(255,255,0,255);

		[DefaultValue(' ')]
		public virtual char FirstCharacter { get; set; }

		[DefaultValue (true)]
		public virtual bool PremultiplyAlpha { get; set; }

		public virtual TextureProcessorOutputFormat TextureFormat { get; set; }

		public FontTextureProcessor ()
		{
		    FirstCharacter = ' ';
		    PremultiplyAlpha = true;
		}

		protected virtual char GetCharacterForIndex (int index)
		{
			return (char)(((int)FirstCharacter) + index);
		}

		private List<Glyph> ExtractGlyphs (System.Drawing.Bitmap bitmap)
		{
			var glyphs = new List<Glyph> (); 
			var regions = new List<System.Drawing.Rectangle> ();
			for (int y = 0; y < bitmap.Height; y++) {
				for (int x = 0; x < bitmap.Width; x++) {
					if (bitmap.GetPixel (x, y) != transparentPixel) {
						// if we don't have a region that has this pixel already
						var re = regions.Find (r => {
							return r.Contains (x, y); 
						});
						if (re == System.Drawing.Rectangle.Empty) {
							// we have found the top, left of a image. 
							// we now need to scan for the 'bounds'
							int top = y;
							int bottom = y;
							int left = x;
							int right = x;
							while (bitmap.GetPixel (right, bottom) != transparentPixel) {
								right++;
							}
							while (bitmap.GetPixel (left, bottom) != transparentPixel) {
								bottom++;
							}
							// we got a glyph :)
							regions.Add (new System.Drawing.Rectangle (left, top, right - left, bottom - top));
							x = right;
						} else {
							x += re.Width;
						}
					}
				}
			}

			for(int i=0; i < regions.Count; i++) {
				var rect = regions[i];
				var newbitmap = new System.Drawing.Bitmap(rect.Width, rect.Height);
				BitmapUtils.CopyRect (bitmap, rect, newbitmap, new System.Drawing.Rectangle (0,0, rect.Width, rect.Height));
				var glyph = new Glyph (GetCharacterForIndex (i), newbitmap);
			    glyph.CharacterWidths.B = glyph.Bitmap.Width;
			    glyphs.Add(glyph);
                //newbitmap.Save (GetCharacterForIndex(i)+".png", System.Drawing.Imaging.ImageFormat.Png);
			}
			return glyphs ;
		}

		public override SpriteFontContent Process (Texture2DContent input, ContentProcessorContext context)
		{
			var output = new SpriteFontContent ();

			// extract the glyphs from the texture and map them to a list of characters.
			// we need to call GtCharacterForIndex for each glyph in the Texture to 
			// get the char for that glyph, by default we start at ' ' then '!' and then ASCII
			// after that. 
		    var systemBitmap = input.Faces[0][0].ToSystemBitmap();

            var glyphs = ExtractGlyphs(systemBitmap);
			// Optimize.
			foreach (Glyph glyph in glyphs) {
				GlyphCropper.Crop(glyph);
                output.VerticalLineSpacing = Math.Max(output.VerticalLineSpacing, glyph.Subrect.Height);
			}

            systemBitmap.Dispose();
		    var compressed = TextureFormat == TextureProcessorOutputFormat.DxtCompressed || TextureFormat == TextureProcessorOutputFormat.Compressed;
            systemBitmap = GlyphPacker.ArrangeGlyphs(glyphs.ToArray(), compressed, compressed);
			
			foreach (Glyph glyph in glyphs) {
				output.CharacterMap.Add (glyph.Character);
				output.Glyphs.Add (new Rectangle (glyph.Subrect.X, glyph.Subrect.Y, glyph.Subrect.Width, glyph.Subrect.Height));
                output.Cropping.Add(new Rectangle((int)glyph.XOffset, (int)glyph.YOffset, glyph.Subrect.Width, glyph.Subrect.Height));
				ABCFloat abc = glyph.CharacterWidths;
				output.Kerning.Add (new Vector3 (abc.A, abc.B, abc.C));
			}
			
			output.Texture.Faces[0].Add(systemBitmap.ToXnaBitmap(true));
            systemBitmap.Dispose();

            var bmp = output.Texture.Faces[0][0];
            if (PremultiplyAlpha)
            {
                var data = bmp.GetPixelData();
                var idx = 0;
                for (; idx < data.Length; )
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

            if (compressed)
                GraphicsUtil.CompressTexture(context.TargetProfile, output.Texture, context, false, PremultiplyAlpha, true);

			return output;
		}
	}
}
