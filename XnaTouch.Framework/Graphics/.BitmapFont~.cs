// Original code from SilverSprite Project
using System;

namespace XnaTouch.Framework.Graphics
{
    struct GlyphData
    {
        public char CharacterIndex;
        public Rectangle Glyph;
        public Rectangle Cropping;
        public Vector3 Kerning;
    }

    public class BitmapSpriteFont : SpriteFont
    {
        Texture2D _texture;
        char? _defaultCharacter;
        Dictionary<char, GlyphData> characterData = new Dictionary<char, GlyphData>();

        public BitmapSpriteFont(Texture2D texture, List<Rectangle>glyphs, List<Rectangle>cropping, List<char>charMap, int lineSpacing, float spacing, List<Vector3>kerning, char? defaultCharacter)
        {
            _texture = texture;
            LineSpacing = lineSpacing;
            Spacing = spacing;
            _defaultCharacter = defaultCharacter;
            for (int i = 0; i < charMap.Count; i++)
            {
                GlyphData g = new GlyphData();
                g.Glyph = glyphs[i];
                g.Cropping = cropping[i];
                g.Kerning = kerning[i];
                g.CharacterIndex = charMap[i];
                characterData.Add(g.CharacterIndex, g);
            }
        }

        internal Vector2 InternalMeasureString(string text)
        {
            Vector2 v = Vector2.Zero;
            float xoffset=0;
            float yoffset=0;

            foreach (char c in text)
            {
                if (c == '\n')
                {
                    yoffset += LineSpacing;
                    xoffset = 0;
                    continue;
                }
                if (characterData.ContainsKey(c) == false) continue;
                GlyphData g = characterData[c];
                xoffset += g.Kerning.Y + g.Kerning.Z + Spacing;
                if (g.Glyph.Height + g.Cropping.Top + yoffset > v.Y)
                {
                    v.Y = yoffset + g.Glyph.Height + g.Cropping.Top;
                }
                if (xoffset > v.X) v.X = xoffset;
            }
            return v;
        }

        public void Draw(SpriteBatchCanvas spriteBatchCanvas, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            Vector2 org = origin;
            foreach (char c in text)
            {
                if (c == '\n')
                {
                    org.Y -= LineSpacing * scale;
                    org.X = origin.X;
                    continue;
                }
                if (characterData.ContainsKey(c) == false) continue;
                GlyphData g = characterData[c];
                spriteBatchCanvas.Draw(_texture, position, g.Glyph, color, rotation, org - new Vector2(g.Cropping.X, g.Cropping.Y), scale, SpriteEffects.None, layerDepth);
                org.X -= (g.Kerning.Y + g.Kerning.Z + Spacing) * scale;
            }
        }

        public void Draw(SpriteBatchCanvas spriteBatchCanvas, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            Vector2 org = origin;
            foreach (char c in text)
            {
                if (c == '\n')
                {
                    org.Y -= LineSpacing * scale.Y;
                    org.X = origin.X;
                    continue;
                }
                if (characterData.ContainsKey(c) == false) continue;
                GlyphData g = characterData[c];
                spriteBatchCanvas.Draw(_texture, position, g.Glyph, color, rotation, org - new Vector2(g.Cropping.X, g.Cropping.Y), scale, SpriteEffects.None, layerDepth);
                org.X -= (g.Kerning.Y + g.Kerning.Z + Spacing) * scale.X;
            }
        }
    }
}
