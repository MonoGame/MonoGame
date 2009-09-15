// Original code from SilverSprite Project
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using OpenTK.Graphics.ES11;
using MonoTouch.UIKit;

namespace XnaTouch.Framework.Graphics
{
	struct GlyphData
    {
        public char CharacterIndex;
        public Rectangle Glyph;
        public Rectangle Cropping;
        public Vector3 Kerning;
		
		public override string ToString ()
		{
			return string.Format("CharacterIndex:{0}, Glyph:{1}, Cropping{2}, Kerning{3}",CharacterIndex,Glyph,Cropping,Kerning);
		}

    }
	
	internal class charEqualityComparer : IEqualityComparer<char>
	{
		public bool Equals (char x, char y)
		{
			return x == y;
		}	
		public int GetHashCode (char obj)
		{
			return obj.GetHashCode();
		}		
	}		
	
    public class SpriteFont
    {
        public int LineSpacing { get; set; }
        public float Spacing { get; set; }
        double size = 11;
        bool bold = false;
        bool italic = false;
        public string AssetName;
		public Texture2D _texture;
        private char? _defaultCharacter;
        private Dictionary<char, GlyphData> characterData = new Dictionary<char, GlyphData>(new charEqualityComparer());

        public char? DefaultCharacter
        {
            set { throw new NotImplementedException(); }
            get { throw new NotImplementedException(); }
        }

        public Vector2 MeasureString(string text)
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
                float newHeight = g.Glyph.Height + g.Cropping.Top + yoffset;
				if ( newHeight > v.Y)
                {
                    v.Y = newHeight;
                }
                if (xoffset > v.X) v.X = xoffset;
            }
            return v;
        }

        public Vector2 MeasureString(StringBuilder text)
        {
            return MeasureString(text.ToString());
		}

        public SpriteFont(Texture2D texture, List<Rectangle>glyphs, List<Rectangle>cropping, List<char>charMap, int lineSpacing, float spacing, List<Vector3>kerning, char? defaultCharacter)
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
		
        public double FontSize
        {
            get
            {
                return size;
            }
        }

        public bool Bold
        {
            get
            {
                return bold;
            }
        }

        public bool Italic
        {
            get
            {
                return italic;
            }
        }
		
		internal void Draw(SpriteBatch spriteBatchCanvas, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
			float textHeight = MeasureString(text).Y;

			Vector2 org = origin;	
			
			// adjust origin
			_texture.Image.Origin = origin;
			// adjust Flip
			_texture.Image.FlipHorizontal = ((effects & SpriteEffects.FlipHorizontally) != SpriteEffects.None);
			_texture.Image.FlipVertical = ((effects & SpriteEffects.FlipVertically) != SpriteEffects.None);			
			// adjust rotation
			_texture.Image.Rotation = rotation;
	
			// adjust the scale
			_texture.Image.HorizontalScale = scale;
			_texture.Image.VerticalScale = scale;
			
			int textLength = 0;
			foreach (char c in text)
            {
                if (characterData.ContainsKey(c) )
					textLength++;
			}
			
			float[] Vertices = new float[textLength*8];
			float[] TextureCoords = new float[textLength*8];
			
			float yoffset=0;
			int i=0;
            foreach (char c in text)
            {
                if (c == '\n')
                {
					yoffset += LineSpacing * scale;
                    org.Y -= yoffset;
                    org.X = origin.X;
                    continue;
                }
                if (characterData.ContainsKey(c) )
				{
	                GlyphData g = characterData[c];				
							
					float ox = org.X - g.Cropping.X;
					float oy = org.Y - g.Cropping.Y + (textHeight-g.Glyph.Height);
					
					_texture.Image.Origin = new Vector2(ox,-oy);	
					_texture.Image.GetTextureVertices(Vertices,i,(int) g.Glyph.Width,(int)g.Glyph.Height);
					_texture.Image.GetTextureCoordinates(TextureCoords,i,g.Glyph);
					
					org.X -= (g.Kerning.Y + g.Kerning.Z + Spacing) * scale;	
					
					i+=8;
				}
            }
			
			position.Y = ((int)UIScreen.MainScreen.Bounds.Height - position.Y)-textHeight;
			
			// Enable Texture_2D
			GL.Enable(All.Texture2D);
		
			if (GraphicsDevice.ActiveTexture != _texture.Image.Name) 
			{
				// Bind to the texture that is associated with this image
				GL.BindTexture(All.Texture2D, _texture.Image.Name);		
				
				// Update the actual Texture
				GraphicsDevice.ActiveTexture = (int) _texture.Image.Name;
			}	
			
			// Set the glColor to apply alpha to the image
			GL.Color4(color.R, color.G, color.B, color.A);								

			// Save the current matrix to the stack
			GL.PushMatrix();

			// Rotate around the Z axis by the angle define for this image
			// we cannot use radians
			GL.Rotate(MathHelper.ToDegrees(-rotation), 0.0f, 0.0f, 1.0f);
			
			// Set client states so that the Texture Coordinate Array will be used during rendering
			GL.EnableClientState(All.TextureCoordArray);
			
			// Move to where we want to draw the image
			GL.Translate(position.X, position.Y, 0.0f);

			// Set up the VertexPointer to point to the vertices we have defined
			GL.VertexPointer(2, All.Float, 0, Vertices);

			// Set up the TexCoordPointer to point to the texture coordinates we want to use
			GL.TexCoordPointer(2, All.Float, 0, TextureCoords);
			
			// Draw the vertices to the screen
			ushort[] indices = new ushort[textLength*6];
			for (i=0;i<textLength;i++)
			{
				indices[i*6+0] = (ushort) (i*4+0);
				indices[i*6+1] = (ushort) (i*4+1);
				indices[i*6+2] = (ushort) (i*4+2);
				indices[i*6+5] = (ushort) (i*4+1);
				indices[i*6+4] = (ushort) (i*4+2);
				indices[i*6+3] = (ushort) (i*4+3);			
			}
			// Draw triangles
			GL.DrawElements(All.Triangles,textLength*6,All.UnsignedShort,indices);
			
			GL.DisableClientState(All.TextureCoordArray);
		
			// Restore the saved matrix from the stack
			GL.PopMatrix();
			
			// Disable as necessary
			GL.Disable(All.Texture2D);		
						
			// back to normal
			_texture.Image.HorizontalScale = 1.0f;
			_texture.Image.VerticalScale = 1.0f;
			_texture.Image.Rotation = 0.0f;
			_texture.Image.FlipHorizontal = false;
			_texture.Image.FlipVertical = false;	
        }

        internal void Draw(SpriteBatch spriteBatchCanvas, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
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
