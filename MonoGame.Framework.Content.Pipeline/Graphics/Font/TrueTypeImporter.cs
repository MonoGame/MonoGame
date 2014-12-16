using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
	// Uses System.Drawing (aka GDI+) to rasterize TrueType fonts into a series of glyph bitmaps.
	internal class TrueTypeImporter : IFontImporter
	{
		// Properties hold the imported font data.
		public IEnumerable<Glyph> Glyphs { get; private set; }

		public float LineSpacing { get; private set; }

		public int YOffsetMin { get; private set; }

		// Size of the temp surface used for GDI+ rasterization.
		const int MaxGlyphSize = 1024;


		public void Import(FontDescription options, string fontName)
		{
			// Create a bunch of GDI+ objects.
			using (Font font = CreateFont(options))
				using (Brush brush = new SolidBrush(System.Drawing.Color.White))
					using (StringFormat stringFormat = new StringFormat(StringFormatFlags.NoFontFallback))
					using (Bitmap bitmap = new Bitmap(MaxGlyphSize, MaxGlyphSize, PixelFormat.Format32bppArgb))
					using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap))
			{
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

				//				var characterRegions = new List<CharacterRegion> ();
				//				foreach (var character in options.Characters) 
				//				{
				//					characterRegions.Add (new CharacterRegion ('a', 'b'));
				//				}

				// Which characters do we want to include?
                var characters = options.Characters;

				var glyphList = new List<Glyph>();

				// Rasterize each character in turn.
				foreach (char character in characters)
				{
					Glyph glyph = ImportGlyph(character, font, brush, stringFormat, bitmap, graphics);

					glyphList.Add(glyph);
				}

				Glyphs = glyphList;

				// Store the font height.
				LineSpacing = font.GetHeight();
				YOffsetMin = (int)LineSpacing;
			}
		}


		// Attempts to instantiate the requested GDI+ font object.
		static Font CreateFont(FontDescription options)
		{
			Font font = new Font(options.FontName, PointsToPixels(options.Size), (System.Drawing.FontStyle)options.Style, GraphicsUnit.Pixel);

			try
			{
				// The font constructor automatically substitutes fonts if it can't find the one requested.
				// But we prefer the caller to know if anything is wrong with their data. A simple string compare
				// isn't sufficient because some fonts (eg. MS Mincho) change names depending on the locale.

				// Early out: in most cases the name will match the current or invariant culture.
				if (options.FontName.Equals(font.FontFamily.GetName(CultureInfo.CurrentCulture.LCID), StringComparison.OrdinalIgnoreCase) ||
				    options.FontName.Equals(font.FontFamily.GetName(CultureInfo.InvariantCulture.LCID), StringComparison.OrdinalIgnoreCase))
				{
					return font;
				}

				// Check the font name in every culture.
				foreach (CultureInfo culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
				{
					if (options.FontName.Equals(font.FontFamily.GetName(culture.LCID), StringComparison.OrdinalIgnoreCase))
					{
						return font;
					}
				}

				// A font substitution must have occurred.
				throw new Exception(string.Format("Can't find font '{0}'.", options.FontName));
			}
			catch
			{
				font.Dispose();
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
		static Glyph ImportGlyph(char character, Font font, Brush brush, StringFormat stringFormat, Bitmap bitmap, System.Drawing.Graphics graphics)
		{
			string characterString = character.ToString();

			// Measure the size of this character.
			SizeF size = graphics.MeasureString(characterString, font, System.Drawing.Point.Empty, stringFormat);

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
			graphics.DrawString(characterString, font, brush, padWidth, padHeight, stringFormat);
			graphics.Flush();

			// Clone the newly rendered image.
			Bitmap glyphBitmap = bitmap.Clone(new System.Drawing.Rectangle(0, 0, bitmapWidth, bitmapHeight), PixelFormat.Format32bppArgb);

			BitmapUtils.ConvertGreyToAlpha(glyphBitmap);

			// Query its ABC spacing.
			float? abc = GetCharacterWidth(character, font, graphics);

			// Construct the output Glyph object.
			return new Glyph(character, glyphBitmap)
			{
				XOffset = -padWidth,
				XAdvance = abc.HasValue ? padWidth - bitmapWidth + abc.Value : -padWidth,
				YOffset = -padHeight,
			};
		}


		// Queries APC spacing for the specified character.
		static float? GetCharacterWidth(char character, Font font, System.Drawing.Graphics graphics)
		{
			// Look up the native device context and font handles.
			IntPtr hdc = graphics.GetHdc();

			try
			{
				IntPtr hFont = font.ToHfont();

				try
				{
					// Select our font into the DC.
					IntPtr oldFont = NativeMethods.SelectObject(hdc, hFont);

					try
					{
						// Query the character spacing.
						var result = new NativeMethods.ABCFloat[1];

						if (NativeMethods.GetCharABCWidthsFloat(hdc, character, character, result))
						{
							return result[0].A + 
								result[0].B + 
									result[0].C;
						}
						else
						{
							return null;
						}
					}
					finally
					{
						NativeMethods.SelectObject(hdc, oldFont);
					}
				}
				finally
				{
					NativeMethods.DeleteObject(hFont);
				}
			}
			finally
			{
				graphics.ReleaseHdc(hdc);
			}
		}


		// Interop to the native GDI GetCharABCWidthsFloat method.
		static class NativeMethods
		{
			[DllImport("gdi32.dll")]
			public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);

			[DllImport("gdi32.dll")]
			public static extern bool DeleteObject(IntPtr hObject);

			[DllImport("gdi32.dll")]
			public static extern bool GetCharABCWidthsFloat(IntPtr hdc, uint iFirstChar, uint iLastChar, [Out] ABCFloat[] lpABCF);


			[StructLayout(LayoutKind.Sequential)]
			public struct ABCFloat
			{
				public float A;
				public float B;
				public float C;
			}
		}
	}
}