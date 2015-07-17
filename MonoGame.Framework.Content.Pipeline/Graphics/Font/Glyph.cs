// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

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
        public Glyph(char character, BitmapContent bitmap, Rectangle? subrect = null)
		{
			this.Character = character;
			this.Bitmap = bitmap;
			this.Subrect = subrect.GetValueOrDefault(new Rectangle(0, 0, bitmap.Width, bitmap.Height));
			this.Width = bitmap.Width;
			this.Height = bitmap.Height;
		}
            
		// Unicode codepoint.
		public char Character;

		// Glyph image data (may only use a portion of a larger bitmap).
        public BitmapContent Bitmap;
		public Rectangle Subrect;

		// Layout information.
		public float XOffset;
		public float YOffset;
        public int Width;
        public int Height;

		public float XAdvance;

		public ABCFloat CharacterWidths;
	}
}
