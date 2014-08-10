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
		}

		protected virtual char GetCharacterForIndex (int index)
		{
			return (char)(((int)FirstCharacter) + index);
		}

		private List<Glyph> ExtractGlyphs (System.Drawing.Bitmap bitmap, out int linespacing)
		{
			linespacing = 0;
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
							linespacing = Math.Max (linespacing, bottom - top);
						} else {
							x += re.Width;
						}
					}
				}
			}

			for(int i=0; i < regions.Count; i++) {
				var rect = regions[i];
				rect.Inflate (-1, -1);
				var newbitmap = new System.Drawing.Bitmap(rect.Width, rect.Height);
				BitmapUtils.CopyRect (bitmap, rect, newbitmap, new System.Drawing.Rectangle (0,0, rect.Width, rect.Height));
				glyphs.Add (new Glyph (GetCharacterForIndex (i), newbitmap));
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

			int linespacing = 0;
            var glyphs = ExtractGlyphs(systemBitmap, out linespacing);
			// Optimize.
			foreach (Glyph glyph in glyphs) {
				GlyphCropper.Crop (glyph);
			}

            systemBitmap.Dispose();
            systemBitmap = GlyphPacker.ArrangeGlyphs(glyphs.ToArray(), true, true);
			
			foreach (Glyph glyph in glyphs) {
				glyph.XAdvance += linespacing;
				if (!output.CharacterMap.Contains (glyph.Character))
					output.CharacterMap.Add (glyph.Character);
				output.Glyphs.Add (new Rectangle (glyph.Subrect.X, glyph.Subrect.Y, glyph.Subrect.Width, glyph.Subrect.Height));
				output.Cropping.Add (new Rectangle (0, 0, glyph.Subrect.Width, glyph.Subrect.Height));
				ABCFloat abc = glyph.CharacterWidths;
				output.Kerning.Add (new Vector3 (abc.A, abc.B, abc.C));
			}
			
			output.Texture.Faces.Add (new MipmapChain (systemBitmap.ToXnaBitmap()));
            systemBitmap.Dispose();

            GraphicsUtil.CompressTexture(context.TargetProfile, output.Texture, context, false, false, false);

			return output;
		}
	}
}
