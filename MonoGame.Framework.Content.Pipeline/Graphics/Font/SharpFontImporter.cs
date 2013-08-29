using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using SharpFont;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
	// Uses System.Drawing (aka GDI+) to rasterize TrueType fonts into a series of glyph bitmaps.
	internal class SharpFontImporter : IFontImporter
	{
		// Properties hold the imported font data.
		public IEnumerable<Glyph> Glyphs { get; private set; }

		public float LineSpacing { get; private set; }


		// Size of the temp surface used for GDI+ rasterization.
		const int MaxGlyphSize = 1024;

		Library lib = null;

		public void Import(FontDescription options, string fontName)
		{
			lib = new Library ();
			// Create a bunch of GDI+ objects.
			Face face = CreateFontFace (options, fontName);
			try {
				using (Brush brush = new SolidBrush(System.Drawing.Color.White))
					using (StringFormat stringFormat = new StringFormat(StringFormatFlags.NoFontFallback))
					using (Bitmap bitmap = new Bitmap(MaxGlyphSize, MaxGlyphSize, PixelFormat.Format32bppArgb))
					using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap))
				{
					graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
					graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
					graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

					// Which characters do we want to include?
					var characters = CharacterRegion.Flatten(options.CharacterRegions);

					var glyphList = new List<Glyph>();
					// Rasterize each character in turn.
					foreach (char character in characters)
					{
						Glyph glyph = ImportGlyph(character, face, brush, stringFormat, bitmap, graphics);
						glyphList.Add(glyph);

					}
					Glyphs = glyphList;

					// Store the font height.
					LineSpacing = 0;
					foreach (var glyph in Glyphs) 
					{
						LineSpacing = (glyph.Subrect.Height > LineSpacing) ? glyph.Subrect.Height : LineSpacing;
					}
				}
			} finally {
				if (face != null)
					face.Dispose ();
				if (lib != null) {
					lib.Dispose ();
					lib = null;
				}
			}
		}


		// Attempts to instantiate the requested GDI+ font object.
		Face CreateFontFace(FontDescription options, string fontName)
		{

			try {
				Face face = lib.NewFace (fontName, 0);
				face.SetCharSize(0, (int)options.Size * 64, 0, 96);

				if (face.FamilyName == "Microsoft Sans Serif" && options.FontName != "Microsoft Sans Serif")
					throw new PipelineException(string.Format("Font {0} is not installed on this computer.", options.FontName));

				return face;

				// A font substitution must have occurred.
				//throw new Exception(string.Format("Can't find font '{0}'.", options.FontName));
			}
			catch
			{
				throw;
			}
		}


		// Converts a font size from points to pixels. Can't just let GDI+ do this for us,
		// because we want identical results on every machine regardless of system DPI settings.
		static float PointsToPixels(float points)
		{
			return points * 96 / 72;
		}


		// Rasterizes a single character glyph.
		static Glyph ImportGlyph(char character, Face face, Brush brush, StringFormat stringFormat, Bitmap bitmap, System.Drawing.Graphics graphics)
		{
			string characterString = character.ToString();

			uint glyphIndex = face.GetCharIndex(character);
			face.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);
			face.Glyph.RenderGlyph(RenderMode.Normal);

			// Measure the size of this character.
			var width = (int)face.Glyph.Advance.X >> 6;
			var height = (int)face.Glyph.Metrics.Height >> 6;

			SizeF size = new SizeF(width, height);

			int characterWidth = (int)Math.Ceiling(size.Width);
			int characterHeight = (int)Math.Ceiling(size.Height);

			// Pad to make sure we capture any overhangs (negative ABC spacing, etc.)
			int padWidth = characterWidth;
			int padHeight = characterHeight / 2;

			int bitmapWidth = characterWidth + padWidth * 2;
			int bitmapHeight = characterHeight + padHeight * 2;

			if (bitmapWidth > MaxGlyphSize || bitmapHeight > MaxGlyphSize)
				throw new Exception("Excessively large glyph won't fit in my lazily implemented fixed size temp surface.");

			// Render the character.
			graphics.Clear(System.Drawing.Color.Black);

			if (face.Glyph.Bitmap.Width > 0 && face.Glyph.Bitmap.Rows > 0) {
				BitmapData data = bitmap.LockBits (new System.Drawing.Rectangle (0, 0, face.Glyph.Bitmap.Width, face.Glyph.Bitmap.Rows), 
				                                 ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
				byte[] gpixelAlphas = new byte[face.Glyph.Bitmap.Width * face.Glyph.Bitmap.Rows];
				Marshal.Copy (face.Glyph.Bitmap.Buffer, gpixelAlphas, 0, gpixelAlphas.Length);

				for (int j = 0; j < gpixelAlphas.Length; j++) {
					int pixelOffset = (j / data.Width) * data.Stride + (j % data.Width * 4);
					Marshal.WriteByte (data.Scan0, pixelOffset + 3, gpixelAlphas [j]);
				}

				bitmap.UnlockBits (data);

			}

			// Clone the newly rendered image.
			Bitmap glyphBitmap = null;
			if (face.Glyph.Bitmap.Width > 0 || face.Glyph.Bitmap.Rows > 0)
				glyphBitmap = bitmap.Clone(new System.Drawing.Rectangle(0, 0, face.Glyph.Bitmap.Width, face.Glyph.Bitmap.Rows), PixelFormat.Format32bppArgb);
			else 
			{
				var gHA = face.Glyph.Metrics.HorizontalAdvance >> 6;
				var gVA = face.Glyph.Metrics.VerticalAdvance >> 6;

				gHA = gHA > 0 ? gHA : gVA;
				gVA = gVA > 0 ? gVA : gHA;

				glyphBitmap = new Bitmap (gHA, gVA);
			}

			BitmapUtils.ConvertToGrey(glyphBitmap);

			// not sure about this at all
			var abc = new ABCFloat ();
			abc.A = face.Glyph.Metrics.HorizontalBearingX >> 6;
			abc.B = face.Glyph.Metrics.Width >> 6;
			abc.C = (face.Glyph.Metrics.HorizontalAdvance >> 6) - (abc.A + abc.B);

			// Query its ABC spacing.
			//float? abc = GetCharacterWidth(character, font, graphics);
//			if (glyphBitmap == null)
//				Console.WriteLine("null");


			// Construct the output Glyph object.
			return new Glyph(character, glyphBitmap)
			{
				XOffset = -padWidth,
				XAdvance = face.Glyph.Metrics.HorizontalAdvance >> 6,
				YOffset = -padHeight,
				CharacterWidths = abc
			};
		}
	}
}