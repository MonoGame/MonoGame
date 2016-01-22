#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2012 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software,
you accept this license. If you do not accept the license, do not use the
software.

1. Definitions

The terms "reproduce," "reproduction," "derivative works," and "distribution"
have the same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the
software.

A "contributor" is any person that distributes its contribution under this
license.

"Licensed patents" are a contributor's patent claims that read directly on its
contribution.

2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the
license conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free copyright license to reproduce its
contribution, prepare derivative works of its contribution, and distribute its
contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license
conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free license under its licensed patents to
make, have made, use, sell, offer for sale, import, and/or otherwise dispose of
its contribution in the software or derivative works of the contribution in the
software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any
contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you
claim are infringed by the software, your patent license from such contributor
to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all
copyright, patent, trademark, and attribution notices that are present in the
software.

(D) If you distribute any portion of the software in source code form, you may
do so only under this license by including a complete copy of this license with
your distribution. If you distribute any portion of the software in compiled or
object code form, you may only do so under a license that complies with this
license.

(E) The software is licensed "as-is." You bear the risk of using it. The
contributors give no express warranties, guarantees or conditions. You may have
additional consumer rights under your local laws which this license cannot
change. To the extent permitted under your local laws, the contributors exclude
the implied warranties of merchantability, fitness for a particular purpose and
non-infringement.
*/
#endregion License

// Original code from SilverSprite Project
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics 
{

	public sealed class SpriteFont 
    {
		static class Errors 
        {
			public const string TextContainsUnresolvableCharacters =
				"Text contains characters that cannot be resolved by this SpriteFont.";
		}

		private readonly Dictionary<char, Glyph> _glyphs;
		
		private readonly Texture2D _texture;

		class CharComparer: IEqualityComparer<char>
		{
			public bool Equals(char x, char y)
			{
				return x == y;
			}

			public int GetHashCode(char b)
			{
				return (b | (b << 16));
			}

			static public readonly CharComparer Default = new CharComparer();
		}

		internal SpriteFont (
			Texture2D texture, List<Rectangle> glyphBounds, List<Rectangle> cropping, List<char> characters,
			int lineSpacing, float spacing, List<Vector3> kerning, char? defaultCharacter)
		{
			Characters = new ReadOnlyCollection<char>(characters.ToArray());
			_texture = texture;
			LineSpacing = lineSpacing;
			Spacing = spacing;
			DefaultCharacter = defaultCharacter;

			_glyphs = new Dictionary<char, Glyph>(characters.Count, CharComparer.Default);

			for (var i = 0; i < characters.Count; i++) 
            {
				var glyph = new Glyph 
                {
					BoundsInTexture = glyphBounds[i],
					Cropping = cropping[i],
                    Character = characters[i],

                    LeftSideBearing = kerning[i].X,
                    Width = kerning[i].Y,
                    RightSideBearing = kerning[i].Z,

                    WidthIncludingBearings = kerning[i].X + kerning[i].Y + kerning[i].Z
				};
				_glyphs.Add (glyph.Character, glyph);
			}
		}

        /// <summary>
        /// Gets the texture that this SpriteFont draws from.
        /// </summary>
        /// <remarks>Can be used to implement custom rendering of a SpriteFont</remarks>
        public Texture2D Texture { get { return _texture; } }

        /// <summary>
        /// Returns a copy of the dictionary containing the glyphs in this SpriteFont.
        /// </summary>
        /// <returns>A new Dictionary containing all of the glyphs inthis SpriteFont</returns>
        /// <remarks>Can be used to calculate character bounds when implementing custom SpriteFont rendering.</remarks>
        public Dictionary<char, Glyph> GetGlyphs()
        {
            return new Dictionary<char, Glyph>(_glyphs, _glyphs.Comparer);
        }

		/// <summary>
		/// Gets a collection of the characters in the font.
		/// </summary>
		public ReadOnlyCollection<char> Characters { get; private set; }

		/// <summary>
		/// Gets or sets the character that will be substituted when a
		/// given character is not included in the font.
		/// </summary>
		public char? DefaultCharacter { get; set; }

		/// <summary>
		/// Gets or sets the line spacing (the distance from baseline
		/// to baseline) of the font.
		/// </summary>
		public int LineSpacing { get; set; }

		/// <summary>
		/// Gets or sets the spacing (tracking) between characters in
		/// the font.
		/// </summary>
		public float Spacing { get; set; }

		/// <summary>
		/// Returns the size of a string when rendered in this font.
		/// </summary>
		/// <param name="text">The text to measure.</param>
		/// <returns>The size, in pixels, of 'text' when rendered in
		/// this font.</returns>
		public Vector2 MeasureString(string text)
		{
			var source = new CharacterSource(text);
			Vector2 size;
			MeasureString(ref source, out size);
			return size;
		}

		/// <summary>
		/// Returns the size of the contents of a StringBuilder when
		/// rendered in this font.
		/// </summary>
		/// <param name="text">The text to measure.</param>
		/// <returns>The size, in pixels, of 'text' when rendered in
		/// this font.</returns>
		public Vector2 MeasureString(StringBuilder text)
		{
			var source = new CharacterSource(text);
			Vector2 size;
			MeasureString(ref source, out size);
			return size;
		}

		private void MeasureString(ref CharacterSource text, out Vector2 size)
		{
			if (text.Length == 0)
            {
				size = Vector2.Zero;
				return;
			}

            // Get the default glyph here once.
            Glyph? defaultGlyph = null;
            if ( DefaultCharacter.HasValue )
                defaultGlyph = _glyphs[DefaultCharacter.Value];

			var width = 0.0f;
			var finalLineHeight = (float)LineSpacing;

            var currentGlyph = Glyph.Empty;
			var offset = Vector2.Zero;
            var firstGlyphOfLine = true;

            for (var i = 0; i < text.Length; ++i)
            {
                var c = text[i];

                if (c == '\r')
                    continue;

                if (c == '\n')
                {
                    finalLineHeight = LineSpacing;

                    offset.X = 0;
                    offset.Y += LineSpacing;
                    firstGlyphOfLine = true;
                    continue;
                }

                if (!_glyphs.TryGetValue(c, out currentGlyph))
                {
                    if (!defaultGlyph.HasValue)
                        throw new ArgumentException(Errors.TextContainsUnresolvableCharacters, "text");

                    currentGlyph = defaultGlyph.Value;
                }

                // The first character on a line might have a negative left side bearing.
                // In this scenario, SpriteBatch/SpriteFont normally offset the text to the right,
                //  so that text does not hang off the left side of its rectangle.
                if (firstGlyphOfLine) {
                    offset.X = Math.Max(currentGlyph.LeftSideBearing, 0);
                    firstGlyphOfLine = false;
                } else {
                    offset.X += Spacing + currentGlyph.LeftSideBearing;
                }

                offset.X += currentGlyph.Width;

                var proposedWidth = offset.X + Math.Max(currentGlyph.RightSideBearing, 0);
                if (proposedWidth > width)
                    width = proposedWidth;

                offset.X += currentGlyph.RightSideBearing;

                if (currentGlyph.Cropping.Height > finalLineHeight)
                    finalLineHeight = currentGlyph.Cropping.Height;
            }

            size.X = width;
            size.Y = offset.Y + finalLineHeight;
		}

        internal void DrawInto( SpriteBatch spriteBatch, ref CharacterSource text, Vector2 position, Color color,
			                    float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth)
		{
            var flipAdjustment = Vector2.Zero;

            var flippedVert = (effect & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
            var flippedHorz = (effect & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;

            if (flippedVert || flippedHorz)
            {
                Vector2 size;
                MeasureString(ref text, out size);

                if (flippedHorz)
                {
                    origin.X *= -1;
                    flipAdjustment.X = -size.X;
                }

                if (flippedVert)
                {
                    origin.Y *= -1;
                    flipAdjustment.Y = LineSpacing - size.Y;
                }
            }

            // TODO: This looks excessive... i suspect we could do most
            // of this with simple vector math and avoid this much matrix work.

            Matrix transformation, temp;
            Matrix.CreateTranslation(-origin.X, -origin.Y, 0f, out transformation);
            Matrix.CreateScale((flippedHorz ? -scale.X : scale.X), (flippedVert ? -scale.Y : scale.Y), 1f, out temp);
            Matrix.Multiply(ref transformation, ref temp, out transformation);
            Matrix.CreateTranslation(flipAdjustment.X, flipAdjustment.Y, 0, out temp);
            Matrix.Multiply(ref temp, ref transformation, out transformation);
            Matrix.CreateRotationZ(rotation, out temp);
            Matrix.Multiply(ref transformation, ref temp, out transformation);
            Matrix.CreateTranslation(position.X, position.Y, 0f, out temp);
            Matrix.Multiply(ref transformation, ref temp, out transformation);

            // Get the default glyph here once.
            Glyph? defaultGlyph = null;
            if (DefaultCharacter.HasValue)
                defaultGlyph = _glyphs[DefaultCharacter.Value];

            var currentGlyph = Glyph.Empty;
            var offset = Vector2.Zero;
            var firstGlyphOfLine = true;

			for (var i = 0; i < text.Length; ++i)
            {
                var c = text[i];

                if (c == '\r')
                    continue;

                if (c == '\n')
                {
                    offset.X = 0;
                    offset.Y += LineSpacing;
                    firstGlyphOfLine = true;
                    continue;
                }

                if (!_glyphs.TryGetValue(c, out currentGlyph))
                {
                    if (!defaultGlyph.HasValue)
                        throw new ArgumentException(Errors.TextContainsUnresolvableCharacters, "text");

                    currentGlyph = defaultGlyph.Value;
                }

                // The first character on a line might have a negative left side bearing.
                // In this scenario, SpriteBatch/SpriteFont normally offset the text to the right,
                //  so that text does not hang off the left side of its rectangle.
                if (firstGlyphOfLine) {
                    offset.X = Math.Max(currentGlyph.LeftSideBearing, 0);
                    firstGlyphOfLine = false;
                } else {
                    offset.X += Spacing + currentGlyph.LeftSideBearing;
                }

                var p = offset;

				if (flippedHorz)
                    p.X += currentGlyph.BoundsInTexture.Width;
                p.X += currentGlyph.Cropping.X;

				if (flippedVert)
                    p.Y += currentGlyph.BoundsInTexture.Height - LineSpacing;
                p.Y += currentGlyph.Cropping.Y;

				Vector2.Transform(ref p, ref transformation, out p);

                var destRect = new Vector4( p.X, p.Y, 
                                            currentGlyph.BoundsInTexture.Width * scale.X,
                                            currentGlyph.BoundsInTexture.Height * scale.Y);

				spriteBatch.DrawInternal(
                    _texture, destRect, currentGlyph.BoundsInTexture,
					color, rotation, Vector2.Zero, effect, depth, false);

                offset.X += currentGlyph.Width + currentGlyph.RightSideBearing;
			}

			// We need to flush if we're using Immediate sort mode.
			spriteBatch.FlushIfNeeded();
		}

        internal struct CharacterSource 
        {
			private readonly string _string;
			private readonly StringBuilder _builder;

			public CharacterSource(string s)
			{
				_string = s;
				_builder = null;
				Length = s.Length;
			}

			public CharacterSource(StringBuilder builder)
			{
				_builder = builder;
				_string = null;
				Length = _builder.Length;
			}

			public readonly int Length;
			public char this [int index] 
            {
				get 
                {
					if (_string != null)
						return _string[index];
					return _builder[index];
				}
			}
		}

        /// <summary>
        /// Struct that defines the spacing, Kerning, and bounds of a character.
        /// </summary>
        /// <remarks>Provides the data necessary to implement custom SpriteFont rendering.</remarks>
		public struct Glyph 
        {
            /// <summary>
            /// The char associated with this glyph.
            /// </summary>
			public char Character;
            /// <summary>
            /// Rectangle in the font texture where this letter exists.
            /// </summary>
			public Rectangle BoundsInTexture;
            /// <summary>
            /// Cropping applied to the BoundsInTexture to calculate the bounds of the actual character.
            /// </summary>
			public Rectangle Cropping;
            /// <summary>
            /// The amount of space between the left side ofthe character and its first pixel in the X dimention.
            /// </summary>
            public float LeftSideBearing;
            /// <summary>
            /// The amount of space between the right side of the character and its last pixel in the X dimention.
            /// </summary>
            public float RightSideBearing;
            /// <summary>
            /// Width of the character before kerning is applied. 
            /// </summary>
            public float Width;
            /// <summary>
            /// Width of the character before kerning is applied. 
            /// </summary>
            public float WidthIncludingBearings;

			public static readonly Glyph Empty = new Glyph();

			public override string ToString ()
			{
                return "CharacterIndex=" + Character + ", Glyph=" + BoundsInTexture + ", Cropping=" + Cropping + ", Kerning=" + LeftSideBearing + "," + Width + "," + RightSideBearing;
			}
		}
	}
}
