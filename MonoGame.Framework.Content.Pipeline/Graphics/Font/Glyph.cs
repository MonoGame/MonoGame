using System;
using System.Runtime.InteropServices;
using System.Drawing;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{

	[StructLayout(LayoutKind.Sequential)]
	public struct ABCFloat
	{
		public float A;
		public float B;
		public float C;
	}

	// Represents a single character within a font.
	internal class Glyph
	{
		// Constructor.
		public Glyph(char character, Bitmap bitmap, System.Drawing.Rectangle? subrect = null)
		{
			this.Character = character;
			this.Bitmap = bitmap;
			this.Subrect = subrect.GetValueOrDefault(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height));
		}


		// Unicode codepoint.
		public char Character;


		// Glyph image data (may only use a portion of a larger bitmap).
		public Bitmap Bitmap;
		public System.Drawing.Rectangle Subrect;


		// Layout information.
		public float XOffset;
		public float YOffset;

		public float XAdvance;

		public ABCFloat CharacterWidths;
	}
}
