// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Runtime.InteropServices;
using SharpFont;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
	// Uses FreeType to rasterize TrueType fonts into a series of glyph bitmaps.
	internal class SharpFontImporter : IFontImporter
	{
		// Properties hold the imported font data.
		public IEnumerable<Glyph> Glyphs { get; private set; }

		public float LineSpacing { get; private set; }

		public int YOffsetMin { get; private set; }

		// Size of the temp surface used for GDI+ rasterization.
		const int MaxGlyphSize = 1024;

		Library lib = null;

		public void Import(FontDescription options, string fontName)
		{
			lib = new Library();
			// Create a bunch of GDI+ objects.
			var face = CreateFontFace(options, fontName);
			try
            {
					// Which characters do we want to include?
                    var characters = options.Characters;

					var glyphList = new List<Glyph>();
					// Rasterize each character in turn.
					foreach (char character in characters)
					{
						var glyph = ImportGlyph(character, face);
						glyphList.Add(glyph);
					}
					Glyphs = glyphList;

					// Store the font height.
					LineSpacing = face.Size.Metrics.Height >> 6;

					// The height used to calculate the Y offset for each character.
					YOffsetMin = -face.Size.Metrics.Ascender >> 6;
			}
            finally
            {
				if (face != null)
					face.Dispose();
				if (lib != null)
                {
					lib.Dispose();
					lib = null;
				}
			}
		}


		// Attempts to instantiate the requested GDI+ font object.
		private Face CreateFontFace(FontDescription options, string fontName)
		{
			try
            {
				const uint dpi = 96;
				var face = lib.NewFace(fontName, 0);
				var fixedSize = ((int)options.Size) << 6;
				face.SetCharSize(0, fixedSize, dpi, dpi);

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

		// Rasterizes a single character glyph.
		static Glyph ImportGlyph(char character, Face face)
		{
			uint glyphIndex = face.GetCharIndex(character);
			face.LoadGlyph(glyphIndex, LoadFlags.Default, LoadTarget.Normal);
			face.Glyph.RenderGlyph(RenderMode.Normal);

			// Render the character.
            BitmapContent glyphBitmap = null;
			if (face.Glyph.Bitmap.Width > 0 && face.Glyph.Bitmap.Rows > 0)
            {
                glyphBitmap = new PixelBitmapContent<byte>(face.Glyph.Bitmap.Width, face.Glyph.Bitmap.Rows);
				byte[] gpixelAlphas = new byte[face.Glyph.Bitmap.Width * face.Glyph.Bitmap.Rows];
				Marshal.Copy(face.Glyph.Bitmap.Buffer, gpixelAlphas, 0, gpixelAlphas.Length);
                glyphBitmap.SetPixelData(gpixelAlphas);
			}

            if (glyphBitmap == null) 
			{
				var gHA = face.Glyph.Metrics.HorizontalAdvance >> 6;
				var gVA = face.Size.Metrics.Height >> 6;

				gHA = gHA > 0 ? gHA : gVA;
				gVA = gVA > 0 ? gVA : gHA;

                glyphBitmap = new PixelBitmapContent<byte>(gHA, gVA);
			}

			// not sure about this at all
			var abc = new ABCFloat ();
			abc.A = face.Glyph.Metrics.HorizontalBearingX >> 6;
			abc.B = face.Glyph.Metrics.Width >> 6;
			abc.C = (face.Glyph.Metrics.HorizontalAdvance >> 6) - (abc.A + abc.B);

			// Construct the output Glyph object.
			return new Glyph(character, glyphBitmap)
			{
				XOffset = -(face.Glyph.Advance.X >> 6),
				XAdvance = face.Glyph.Metrics.HorizontalAdvance >> 6,
                YOffset = -(face.Glyph.Metrics.HorizontalBearingY >> 6),
				CharacterWidths = abc
			};
		}
	}
}