// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.Linq;
using SharpFont;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using MonoGame.Framework.Content.Pipeline.Builder;
using Glyph = Microsoft.Xna.Framework.Content.Pipeline.Graphics.Glyph;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    [ContentProcessor(DisplayName = "Sprite Font Description - MonoGame")]
    public class FontDescriptionProcessor : ContentProcessor<FontDescription, SpriteFontContent>
    {
        public override SpriteFontContent Process(FontDescription input,
            ContentProcessorContext context)
        {
            var output = new SpriteFontContent(input);

            var estimatedSurfaceArea = 0;
            var largestHeight = 0;
            var widthsAndHeights = new List<Point>();
			var fontName = input.FontName;

			var directory = Path.GetDirectoryName (input.Identity.SourceFilename);
			var directories = new string[] { directory, 
				"/Library/Fonts",
				"C:\\Windows\\Fonts"
			};

			foreach( var dir in directories) {
				if (File.Exists(Path.Combine(dir,fontName+".ttf"))) {
					fontName += ".ttf";
					directory = dir;
					break;
				}
				if (File.Exists (Path.Combine(dir,fontName+".ttc"))) {
					fontName += ".ttc";
					directory = dir;
					break;
				}
				if (File.Exists(Path.Combine(dir,fontName+".otf"))) {
					fontName += ".otf";
					directory = dir;
					break;
				}
			}

			fontName = Path.Combine (directory, fontName);

			context.Logger.LogMessage ("Building Font {0}", fontName);
			try {
				if (!File.Exists(fontName)) {
					throw new Exception(string.Format("Could not load {0}", fontName));
				}
				var lineSpacing = 0f;
				var glyphs = ImportFont(input, out lineSpacing, context, fontName);

				// Optimize.
				foreach (Glyph glyph in glyphs)
				{
					GlyphCropper.Crop(glyph);
				}

				Bitmap outputBitmap = GlyphPacker.ArrangeGlyphs(glyphs, true, true);

				//outputBitmap.Save ("/Users/Jimmy/Desktop/Cocos2D-XNAImages/fontglyphs.png");

				// Adjust line and character spacing.
				lineSpacing += input.Spacing;

				foreach (Glyph glyph in glyphs)
				{
					glyph.XAdvance += input.Spacing;
					output.CharacterMap.Add(glyph.Character);
					output.Glyphs.Add(new Rectangle(glyph.Subrect.X, glyph.Subrect.Y, glyph.Subrect.Width, glyph.Subrect.Height));
					output.Cropping.Add(new Rectangle(0,0,glyph.Subrect.Width, glyph.Subrect.Height));
					ABCFloat abc = glyph.CharacterWidths;
					output.Kerning.Add(new Vector3(abc.A, abc.B, abc.C));
				}

//				Library lib = new Library ();
//				Face face = lib.NewFace (fontName, 0);
//				face.SetCharSize(0, (int)input.Size * 64, 0, 96);
//
//				if (face.FamilyName == "Microsoft Sans Serif" && input.FontName != "Microsoft Sans Serif")
//					    throw new PipelineException(string.Format("Font {0} is not installed on this computer.", input.FontName));
//
//				foreach (var ch in input.Characters) {
//					uint glyphIndex = face.GetCharIndex(ch);
//					face.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);
//					var width = (int)face.Glyph.Advance.X >> 6;
//
//				    var height = (int)face.Glyph.Metrics.Height >> 6;
//
//					estimatedSurfaceArea += width;
//					largestHeight = Math.Max (largestHeight, height);
//
//					widthsAndHeights.Add (new Point (width, height));
//				}
//
//				estimatedSurfaceArea *= largestHeight;
//
//				output.VerticalLineSpacing = largestHeight;
//
//	            // calculate the best height and width for our output texture.
//	            // TODO: GetMonoGamePlatform()
//	            var texBounds = calculateOutputTextureBounds(estimatedSurfaceArea, true);
//				// Create our texture
//	            var outputBitmap = new Bitmap(texBounds.X, texBounds.Y);
//	            using (var g = System.Drawing.Graphics.FromImage(outputBitmap))
//	            {
//	                g.FillRectangle(Brushes.Transparent, new System.Drawing.Rectangle(0, 0, outputBitmap.Width, outputBitmap.Height));
//				}
//                int x = 0;
//                int y = 0;
//                // Draw each glyph into the image.
//                for (int i = 0; i < input.Characters.Count; i++)
//                {
//					char c = input.Characters[i];
//					uint glyphIndex = face.GetCharIndex(c);
//					face.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);
//					face.Glyph.RenderGlyph(RenderMode.Normal);
//
//					var A = face.Glyph.Metrics.HorizontalBearingX >> 6;
//					var B = face.Glyph.Metrics.Width >> 6;
//					var C = (face.Glyph.Metrics.HorizontalAdvance >> 6) - (A + B);
//
//					int charWidth = (int)(Math.Abs(A) + B + C);
//
//					if (!input.UseKerning)
//						charWidth = (int)B;
//
//					if (x + charWidth >= outputBitmap.Width)
//					{
//						x = 0;
//						y += largestHeight;
//					}
//
//					var rect = new Microsoft.Xna.Framework.Rectangle(x, y, charWidth, widthsAndHeights[i].Y);
//					output.Glyphs.Add(rect);
//
//					// Characters with a negative a kerning value (like j) need to be adjusted,
//					// so (in the case of j) the bottom curve doesn't render outside our source
//					// rect.
//					var renderPoint = new PointF(x, y);
//					if (A < 0)
//						renderPoint.X += Math.Abs(A);
//
//					if (face.Glyph.Bitmap.Width > 0 && face.Glyph.Bitmap.Rows > 0) {
//
//						BitmapData data = outputBitmap.LockBits(new System.Drawing.Rectangle((int)renderPoint.X, (int)renderPoint.Y , face.Glyph.Bitmap.Width, face.Glyph.Bitmap.Rows), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
//						byte[] pixelAlphas = new byte[face.Glyph.Bitmap.Width * face.Glyph.Bitmap.Rows];
//						Marshal.Copy(face.Glyph.Bitmap.Buffer, pixelAlphas, 0, pixelAlphas.Length);
//
//						for (int j = 0; j < pixelAlphas.Length; j++)
//						{
//							int pixelOffset = (j / data.Width) * data.Stride + (j % data.Width * 4);
//							Marshal.WriteByte(data.Scan0, pixelOffset + 3, pixelAlphas[j]);
//						}
//
//						outputBitmap.UnlockBits(data);
//					}
//					output.Cropping.Add(new Microsoft.Xna.Framework.Rectangle(0, (face.Glyph.Metrics.VerticalAdvance >> 6) - face.Glyph.BitmapTop, charWidth, widthsAndHeights[i].Y));
//
//					if (!input.UseKerning)
//					{
//						A = 0;
//						C = 0;
//					}
//					output.Kerning.Add(new Vector3(A, B, C));
//
//					// Add a 2 pixel spacing between characters
//					x += charWidth + 2;
//                }
//
//                // Drawing against a transparent black background blends
//                // the 'alpha' pixels against black, leaving a black outline.
//                // Interpolate between black and white
//                // based on it's intensity to covert this 'outline' to
//                // it's grayscale equivalent.
//                var transBlack = Color.Transparent;
//                for (var i = 0; i < outputBitmap.Width; i++)
//                {
//                    for (var j = 0; j < outputBitmap.Height; j++)
//                    {
//                        var px = outputBitmap.GetPixel(i, j);
//
//                        if (px.ColorsEqual(transBlack))
//                            continue;
//
//						var val = px.A / (255.0f);
//                        var col = Color.Lerp(Color.Transparent, Color.White, val);
//                        px = System.Drawing.Color.FromArgb(px.A, col.R, col.G, col.B);
//                        outputBitmap.SetPixel(i, j, px);
//                    }
//                }
//				outputBitmap.Save("/Users/Jimmy/Desktop/Cocos2D-XNAImages/test.png");
				output.Texture._bitmap = outputBitmap;

				var bitmapContent = new PixelBitmapContent<Color>(outputBitmap.Width, outputBitmap.Height);
				bitmapContent.SetPixelData(outputBitmap.GetData());
				output.Texture.Faces.Add(new MipmapChain(bitmapContent));

            	GraphicsUtil.CompressTexture(output.Texture, context, false, false);
			}
			catch(Exception ex) {
				context.Logger.LogImportantMessage("{0}", ex.ToString());
			}

            return output;
        }

//        private Point calculateOutputTextureBounds(int estArea, bool forceSquare)
//        {
//            // always generate textures with PoT bounds.
//
//            // Some texture compression requires square PoT textures
//            if (forceSquare)
//            {
//                // get a single dimention of the square
//                var dimention = (int)Math.Ceiling(Math.Sqrt(estArea));
//
//                // get thenext power of two of this dimention
//                dimention = GraphicsUtil.GetNextPowerOfTwo(dimention);
//
//                // return the height and width of our new
//                return new Point(dimention, dimention);
//            }
//            else
//                throw new NotSupportedException("Non Square Textures are not yet supported");
//
//            /*// We don't require a square texture. Get the smallest PoT bounds
//            // that can contain the entire sheet.
//
//            return new Point();*/
//        }

		static Glyph[] ImportFont(FontDescription options, out float lineSpacing, ContentProcessorContext context, string fontName)
		{
			// Which importer knows how to read this source font?
			IFontImporter importer;

			var TrueTypeFileExtensions = new List<string> { ".ttf", ".ttc", ".otf" };
			var BitmapFileExtensions = new List<string> { ".bmp", ".png", ".gif" };

			string fileExtension = Path.GetExtension(fontName).ToLowerInvariant();
			context.Logger.LogMessage ("Building Font {0}", fontName);

			//			if (BitmapFileExtensions.Contains(fileExtension))
			//			{
			//				importer = new BitmapImporter();
			//			}
			//			else
			//			{
			if (TrueTypeFileExtensions.Contains (fileExtension)) 
			{
				importer = new SharpFontImporter ();
			}
			else 
			{
				//importer = new TrueTypeImporter();
				importer = new SharpFontImporter ();
			}

			// Import the source font data.
			importer.Import(options);

			lineSpacing = importer.LineSpacing;

			// Get all glyphs
			var glyphs = new List<Glyph>(importer.Glyphs);

			// Validate.
			if (glyphs.Count == 0)
			{
				throw new Exception("Font does not contain any glyphs.");
			}

			// Sort the glyphs
			glyphs.Sort((left, right) => left.Character.CompareTo(right.Character));


			// Check that the default character is part of the glyphs
			if (options.DefaultCharacter != null)
			{
				bool defaultCharacterFound = false;
				foreach (var glyph in glyphs)
				{
					if (glyph.Character == options.DefaultCharacter)
					{
						defaultCharacterFound = true;
						break;
					}
				}
				if (!defaultCharacterFound)
				{
					throw new InvalidOperationException("The specified DefaultCharacter is not part of this font.");
				}
			}

			return glyphs.ToArray();
		}
    }
}
