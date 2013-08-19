using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
	// Importer interface allows the conversion tool to support multiple source font formats.
	internal interface IFontImporter
	{
		void Import(FontDescription options);

		IEnumerable<Glyph> Glyphs { get; }

		float LineSpacing { get; }
	}
}
