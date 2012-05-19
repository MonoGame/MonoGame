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

		private Dictionary<char, Glyph> _glyphs;
		
		private Texture2D _texture;

		internal SpriteFont (
			Texture2D texture, List<Rectangle> glyphBounds, List<Rectangle> cropping, List<char> characters,
			int lineSpacing, float spacing, List<Vector3> kerning, char? defaultCharacter)
		{
			_characters = new ReadOnlyCollection<char> (characters.ToArray ());
			_texture = texture;
			LineSpacing = lineSpacing;
			Spacing = spacing;
			DefaultCharacter = defaultCharacter;

			_glyphs = new Dictionary<char, Glyph>(characters.Count);

			for (var i = 0; i < characters.Count; i++) 
            {
				var glyph = new Glyph 
                {
					BoundsInTexture = glyphBounds [i],
					Cropping = cropping [i],
					Kerning = kerning [i],
					Character = characters [i]
				};
				_glyphs.Add (glyph.Character, glyph);
			}
		}

		private ReadOnlyCollection<char> _characters;

		/// <summary>
		/// Gets a collection of the characters in the font.
		/// </summary>
		public ReadOnlyCollection<char> Characters { get { return _characters; } }

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

			var width = 0.0f;
			var finalLineHeight = (float)LineSpacing;
			var fullLineCount = 0;
            var currentGlyph = Glyph.Empty;
			var offset = Vector2.Zero;
            var hasCurrentGlyph = false;

            for (var i = 0; i < text.Length; ++i)
            {
                var c = text[i];
                if (c == '\r')
                {
                    hasCurrentGlyph = false;
                    continue;
                }

                if (c == '\n')
                {
                    fullLineCount++;
                    finalLineHeight = LineSpacing;

                    offset.X = 0;
                    offset.Y = LineSpacing * fullLineCount;
                    hasCurrentGlyph = false;
                    continue;
                }

                if (hasCurrentGlyph)
                    offset.X += Spacing + currentGlyph.WidthIncludingBearings;

                hasCurrentGlyph =   _glyphs.TryGetValue(c, out currentGlyph) ||
                                    ( DefaultCharacter.HasValue && _glyphs.TryGetValue(DefaultCharacter.Value, out currentGlyph) );
                if (!hasCurrentGlyph)
                    throw new ArgumentException(Errors.TextContainsUnresolvableCharacters, "text");

                var proposedWidth = offset.X + currentGlyph.WidthIncludingBearings;
                if (proposedWidth > width)
                    width = proposedWidth;

                if (currentGlyph.Cropping.Height > finalLineHeight)
                    finalLineHeight = currentGlyph.Cropping.Height;
            }

            size.X = width;
            size.Y = fullLineCount * LineSpacing + finalLineHeight;
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
                    scale.X *= -1;
                    flipAdjustment.X = -size.X;
                }

                if (flippedVert)
                {
                    origin.Y *= -1;
                    scale.Y *= -1;
                    flipAdjustment.Y = LineSpacing - size.Y;
                }
            }

            Matrix transformation, temp;
            Matrix.CreateTranslation(-origin.X, -origin.Y, 0f, out transformation);
            Matrix.CreateScale(scale.X, scale.Y, 1f, out temp);
            Matrix.Multiply(ref transformation, ref temp, out transformation);
            Matrix.CreateTranslation(flipAdjustment.X, flipAdjustment.Y, 0, out temp);
            Matrix.Multiply(ref temp, ref transformation, out transformation);
            Matrix.CreateRotationZ(rotation, out temp);
            Matrix.Multiply(ref transformation, ref temp, out transformation);
            Matrix.CreateTranslation(position.X, position.Y, 0f, out temp);
            Matrix.Multiply(ref transformation, ref temp, out transformation);

            var width = 0.0f;
            var finalLineHeight = (float)LineSpacing;
            var fullLineCount = 0;
            var currentGlyph = Glyph.Empty;
            var offset = Vector2.Zero;
            var hasCurrentGlyph = false;

			for (var i = 0; i < text.Length; ++i)
            {
                var c = text[i];
                if (c == '\r')
                {
                    hasCurrentGlyph = false;
                    continue;
                }

                if (c == '\n')
                {
                    fullLineCount++;
                    finalLineHeight = LineSpacing;

                    offset.X = 0;
                    offset.Y = LineSpacing * fullLineCount;
                    hasCurrentGlyph = false;
                    continue;
                }

                if (hasCurrentGlyph)
                    offset.X += Spacing + currentGlyph.WidthIncludingBearings;

                hasCurrentGlyph =   _glyphs.TryGetValue(c, out currentGlyph) ||
                                    (DefaultCharacter.HasValue && _glyphs.TryGetValue(DefaultCharacter.Value, out currentGlyph));
                if (!hasCurrentGlyph)
                    throw new ArgumentException(Errors.TextContainsUnresolvableCharacters, "text");

                var proposedWidth = offset.X + currentGlyph.WidthIncludingBearings;
                if (proposedWidth > width)
                    width = proposedWidth;

                if (currentGlyph.Cropping.Height > finalLineHeight)
                    finalLineHeight = currentGlyph.Cropping.Height;

                var p = offset;

				if (flippedHorz)
                    p.X += currentGlyph.BoundsInTexture.Width;
                p.X += currentGlyph.LeftSideBearing;

				if (flippedVert)
                    p.Y += currentGlyph.BoundsInTexture.Height - LineSpacing;
                p.Y += currentGlyph.Cropping.Y;

				Vector2.Transform(ref p, ref transformation, out p);

				spriteBatch.Draw(
                    _texture, p, currentGlyph.BoundsInTexture,
					color, rotation, Vector2.Zero, scale, effect, depth);
			}
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

		struct Glyph 
        {
			public char Character;
			public Rectangle BoundsInTexture;
			public Rectangle Cropping;
			public Vector3 Kerning;

			private static readonly Glyph _empty = new Glyph ();

			public static Glyph Empty { get { return _empty; } }

			public float LeftSideBearing { get { return Kerning.X; } }

			public float RightSideBearing { get { return Kerning.Z; } }

			public float Width { get { return Kerning.Y; } }

			public float WidthIncludingBearings 
            {
				get { return Kerning.X + Kerning.Y + Kerning.Z; }
			}

			public override string ToString ()
			{
				return string.Format(
					"CharacterIndex={0}, Glyph={1}, Cropping={2}, Kerning={3}",
					Character, BoundsInTexture, Cropping, Kerning);
			}
		}

		#region Deprecated

		[Obsolete ("AssetName does not seem to be in use.", true)]
		internal string AssetName;

		[Obsolete ("SpriteFont.FontSize does not exist in XNA and is deprecated.")]
		public double FontSize
		{
			get { return 11; }
		}

		[Obsolete ("SpriteFont.Bold does not exist in XNA and is deprecated.")]
		public bool Bold
		{
			get { return false; }
		}

		[Obsolete ("SpriteFont.Italic does not exist in XNA and is deprecated.")]
		public bool Italic
		{
			get { return false; }
		}

		#endregion Deprecated
	}
}
