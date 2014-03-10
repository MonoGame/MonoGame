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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Represents a font texture.
    /// </summary>
    public sealed class SpriteFont
    {
        private static class Errors
        {
            public const string TextContainsUnresolvableCharacters =
                "Text contains characters that cannot be resolved by this SpriteFont.";
        }

        private readonly Dictionary<char, Glyph> _glyphs;
        private readonly ReadOnlyCollection<char> _characters;
        internal readonly Texture2D _texture;

        internal SpriteFont(Texture2D texture, List<Rectangle> glyphBounds, List<Rectangle> cropping, List<char> characters,
            int lineSpacing, float spacing, List<Vector3> kerning, char? defaultCharacter)
        {
            this.LineSpacing = lineSpacing;
            this.Spacing = spacing;
            this.DefaultCharacter = defaultCharacter;

            _characters = new ReadOnlyCollection<char>(characters.ToArray());
            _texture = texture;
            _glyphs = new Dictionary<char, Glyph>(characters.Count);

            for (var i = 0; i < characters.Count; i++)
            {
                var glyph = new Glyph
                {
                    BoundsInTexture = glyphBounds[i],
                    Cropping = cropping[i],
                    Character = characters[i],
                    LeftSideBearing = kerning[i].X,
                    Width = kerning[i].Y,
                    RightSideBearing = kerning[i].Z
                };
                _glyphs.Add(glyph.Character, glyph);
            }
        }

        /// <summary>
        /// Gets a collection of the characters in the font.
        /// </summary>
        public ReadOnlyCollection<char> Characters
        {
            get { return _characters; }
        }

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
            return this.MeasureString(ref source);
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
            return this.MeasureString(ref source);
        }

        private Vector2 MeasureString(ref CharacterSource text)
        {
            var width = 0.0f;
            var finalLineHeight = (float)this.LineSpacing;
            var lineCount = 0;

            this.ProcessText(ref text, (isFirstGlyphOfLine, offset, currentGlyph) =>
            {
                if (isFirstGlyphOfLine)
                {
                    lineCount++;
                    finalLineHeight = this.LineSpacing;
                }

                var proposedWidth = offset.X + currentGlyph.Width;
                if (proposedWidth > width)
                    width = proposedWidth;

                if (currentGlyph.Cropping.Height > finalLineHeight)
                    finalLineHeight = currentGlyph.Cropping.Height;
            });

            return new Vector2(width, (lineCount - 1) * this.LineSpacing + finalLineHeight);
        }

        internal void DrawString(SpriteBatch spriteBatch, string text, Vector2 position, Color color,
            float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth)
        {
            var source = new CharacterSource(text);
            this.DrawString(spriteBatch, ref source, position, color, rotation, origin, scale, effect, depth);
        }

        internal void DrawString(SpriteBatch spriteBatch, StringBuilder text, Vector2 position, Color color,
            float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth)
        {
            var source = new CharacterSource(text);
            this.DrawString(spriteBatch, ref source, position, color, rotation, origin, scale, effect, depth);
        }

        private void DrawString(SpriteBatch spriteBatch, ref CharacterSource text, Vector2 position, Color color,
            float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth)
        {
            var flipAdjustment = Vector2.Zero;
            var isFlippedVertically = (effect & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
            var isFlippedHorizontally = (effect & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;

            if (isFlippedVertically || isFlippedHorizontally)
            {
                Vector2 size = this.MeasureString(ref text);

                if (isFlippedHorizontally)
                {
                    origin.X = -origin.X;
                    flipAdjustment.X = -size.X;
                }

                if (isFlippedVertically)
                {
                    origin.Y = -origin.Y;
                    flipAdjustment.Y = LineSpacing - size.Y;
                }
            }

            // TODO: This looks excessive... i suspect we could do most
            // of this with simple vector math and avoid this much matrix work.

            Matrix transformation, temp;
            Matrix.CreateTranslation(-origin.X, -origin.Y, 0f, out transformation);
            Matrix.CreateScale((isFlippedHorizontally ? -scale.X : scale.X), (isFlippedVertically ? -scale.Y : scale.Y), 1f, out temp);
            Matrix.Multiply(ref transformation, ref temp, out transformation);
            if (flipAdjustment != Vector2.Zero)
            {
                Matrix.CreateTranslation(flipAdjustment.X, flipAdjustment.Y, 0, out temp);
                Matrix.Multiply(ref temp, ref transformation, out transformation);
            }
            if (rotation < -float.Epsilon || float.Epsilon < rotation)
            {
                Matrix.CreateRotationZ(rotation, out temp);
                Matrix.Multiply(ref transformation, ref temp, out transformation);
            }
            Matrix.CreateTranslation(position.X, position.Y, 0f, out temp);
            Matrix.Multiply(ref transformation, ref temp, out transformation);

            this.ProcessText(ref text, (isFirstGlyphOfLine, offset, currentGlyph) =>
            {
                var glyphPosition = offset;
                glyphPosition.X += (isFlippedHorizontally ? currentGlyph.BoundsInTexture.Width : 0) + currentGlyph.Cropping.X;
                glyphPosition.Y += (isFlippedVertically ? currentGlyph.BoundsInTexture.Height - LineSpacing : 0) + currentGlyph.Cropping.Y;

                Vector2.Transform(ref glyphPosition, ref transformation, out glyphPosition);

                var destRect = new Vector4(glyphPosition.X, glyphPosition.Y,
                    currentGlyph.BoundsInTexture.Width * scale.X, currentGlyph.BoundsInTexture.Height * scale.Y);

                spriteBatch.DrawInternal(_texture, destRect, currentGlyph.BoundsInTexture,
                    color, rotation, Vector2.Zero, effect, depth, false);
            });

            // We need to flush if we're using Immediate sort mode.
            spriteBatch.FlushIfNeeded();
        }

        private void ProcessText(ref CharacterSource text, Action<bool, Vector2, Glyph> processGlyph)
        {
            Glyph defaultGlyph = null;
            var offset = Vector2.Zero;
            var isFirstGlyphOfLine = true;

            for (var i = 0; i < text.Length; ++i)
            {
                var currentCharacter = text[i];
                if (currentCharacter == '\r')
                    continue;

                if (currentCharacter == '\n')
                {
                    offset.X = 0;
                    offset.Y += this.LineSpacing;
                    isFirstGlyphOfLine = true;
                    continue;
                }

                Glyph currentGlyph;
                if (!_glyphs.TryGetValue(currentCharacter, out currentGlyph))
                {
                    if (defaultGlyph == null)
                    {
                        if (!this.DefaultCharacter.HasValue || !_glyphs.TryGetValue(this.DefaultCharacter.Value, out defaultGlyph))
                            throw new ArgumentException(Errors.TextContainsUnresolvableCharacters, "text");
                    }

                    currentGlyph = defaultGlyph;
                }

                // The first character on a line might have a negative left side bearing.
                // In this scenario, SpriteBatch/SpriteFont normally offset the text to the right,
                // so that text does not hang off the left side of its rectangle.
                offset.X += (isFirstGlyphOfLine && currentGlyph.LeftSideBearing < 0 ? 0 : currentGlyph.LeftSideBearing);

                processGlyph(isFirstGlyphOfLine, offset, currentGlyph);

                offset.X += currentGlyph.Width + currentGlyph.RightSideBearing + Spacing;
                isFirstGlyphOfLine = false;
            }
        }

        private struct CharacterSource
        {
            private readonly string _string;
            private readonly StringBuilder _builder;

            public readonly int Length;

            public CharacterSource(string s)
            {
                _string = s;
                _builder = null;
                this.Length = s.Length;
            }

            public CharacterSource(StringBuilder builder)
            {
                _builder = builder;
                _string = null;
                this.Length = builder.Length;
            }

            public char this[int index]
            {
                get { return _string != null ? _string[index] : _builder[index]; }
            }
        }

        private class Glyph
        {
            public char Character;
            public Rectangle BoundsInTexture;
            public Rectangle Cropping;
            public float LeftSideBearing;
            public float RightSideBearing;
            public float Width;

            public override string ToString()
            {
                return string.Format(
                    "CharacterIndex={0}, Glyph={1}, Cropping={2}, Kerning={3},{4},{5}",
                    Character, BoundsInTexture, Cropping, LeftSideBearing, Width, RightSideBearing);
            }
        }
    }
}
