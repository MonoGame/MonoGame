#region License
/*
Microsoft Public License (Ms-PL)
XnaTouch - Copyright © 2009 The XnaTouch Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License

using MonoTouch.UIKit;
using OpenTK.Graphics.ES11;
using System;
using System.Text;
using XnaTouch;
using XnaTouch.Framework;
using XnaTouch.Framework.Graphics;

namespace XnaTouch.Framework.Graphics
{
    public class SpriteBatch : IDisposable
    {
		private XnaTouch.Framework.Graphics.GraphicsDevice _device;
		private object _tag;
		private string _name;

        public SpriteBatch(XnaTouch.Framework.Graphics.GraphicsDevice graphicsDevice)
        {
			_device = graphicsDevice;
        }
		
        public void Begin()
        {
			Begin(SpriteBlendMode.AlphaBlend);
        }

        public void Begin(SpriteBlendMode blendMode)
        {
			GraphicsDevice.StartSpriteBatch(blendMode, SpriteSortMode.Deferred);
		}
		
		public void End()
        {
			GraphicsDevice.EndSpriteBatch();
        }

        public void Begin(SpriteBlendMode blendMode, SpriteSortMode sortMode, SaveStateMode stateMode)
        {
			Begin(blendMode, sortMode, SaveStateMode.None,Matrix.Identity);
        }

        public void Begin(SpriteBlendMode blendMode, SpriteSortMode sortMode, SaveStateMode stateMode, Matrix transformMatrix)
        {
			if (stateMode != SaveStateMode.None)
			{
				throw new NotSupportedException();
			}
			GraphicsDevice.StartSpriteBatch(blendMode, sortMode);
        }

        public void Dispose()
        {
        }

        public void Draw(Texture2D texture, Rectangle destinationRectangle, Color color)
        {
			Draw(texture,destinationRectangle,null,color,0.0f,Vector2.Zero,SpriteEffects.None,0);
        }

        public void Draw(Texture2D texture, Vector2 position, Color color)
        {
			Draw(texture,new Rectangle((int)position.X,(int)position.Y,texture.Width,texture.Height),null,color,0.0f,Vector2.Zero,SpriteEffects.None,0);
        }

        public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color)
        { 
			Draw(texture,destinationRectangle,sourceRectangle,color,0.0f,Vector2.Zero,SpriteEffects.None,0);			
        }

        public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color)
        {
			if (texture == null)
			{
				throw new ArgumentException("texture cannot be NULL");
			}			
			
			if (sourceRectangle.HasValue) 
			{
				// adjust filter color
				RenderMode mode = new RenderMode();
				mode.Texture = texture;
				mode.FilterColor = color;	
				mode.LayerDepth = 0.0f;
				// Adjust vertical axis
				position.Y = ((int)UIScreen.MainScreen.Bounds.Height - position.Y)-sourceRectangle.Value.Height;

				//render
				GraphicsDevice.AddToSpriteBuffer(mode, position, sourceRectangle.Value);				
			}
			else 
			{
				Draw(texture,new Rectangle((int)position.X,(int)position.Y,texture.Width,texture.Height),sourceRectangle,color,0.0f,Vector2.Zero,SpriteEffects.None,0);
			}
        }

        public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth)
        {
			if (texture == null)
			{
				throw new ArgumentException("texture cannot be NULL");
			}
			
			RenderMode mode = new RenderMode();
			mode.Texture = texture;
			// set the layer
			mode.LayerDepth = layerDepth;
			// adjust origin
			mode.Origin = origin;
			// adjust Flip
			mode.FlipHorizontal = ((effects & SpriteEffects.FlipHorizontally) != SpriteEffects.None);
			mode.FlipVertical = ((effects & SpriteEffects.FlipVertically) != SpriteEffects.None);			
			// adjust rotation
			mode.Rotation = rotation;
			// adjust filter color
			mode.FilterColor = color;
				
			// Adjust vertical axis
			destinationRectangle.Y = ((int)UIScreen.MainScreen.Bounds.Height - destinationRectangle.Y)-destinationRectangle.Height;			
			
			if (sourceRectangle.HasValue)
			{
				// adjust the scale
				mode.HorizontalScale = (float) destinationRectangle.Width / (float)sourceRectangle.Value.Width;
				mode.VerticalScale = (float)destinationRectangle.Height / (float)sourceRectangle.Value.Height;	
																						
				//render
				GraphicsDevice.AddToSpriteBuffer(mode,new Vector2(destinationRectangle.X,destinationRectangle.Y), sourceRectangle.Value);
			}
			else 
			{
				// adjust the scale
				mode.HorizontalScale = (float) destinationRectangle.Width / (float)texture.Width;
				mode.VerticalScale = (float)destinationRectangle.Height / (float)texture.Height;	
				//render		
				
				GraphicsDevice.AddToSpriteBuffer(mode, new Vector2(destinationRectangle.X, destinationRectangle.Y),texture.SourceRect);
			}
        }

        public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
			throw new NotImplementedException();
        }

        public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
			if (texture == null)
			{
				throw new ArgumentException("texture cannot be NULL");
			}
			
			Rectangle destination = new Rectangle();
			destination.X = (int)position.X;
			destination.Y = (int)position.Y;
			if (sourceRectangle.HasValue)
			{
				destination.Width = sourceRectangle.Value.Width;
				destination.Height =sourceRectangle.Value.Height;				
			}
			else 
			{
				destination.Width = texture.Width;
				destination.Height = texture.Height;
			}
			
			Draw(texture, destination, sourceRectangle, color, rotation, origin, effects, layerDepth);
        }
		
        public void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color)
        {
			DrawString(spriteFont,text,position,color,0.0f,Vector2.Zero,1.0f,SpriteEffects.None,1);
        }

        public void DrawString(SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color)
        {
			DrawString(spriteFont,text.ToString(),position,color,0.0f,Vector2.Zero,1.0f,SpriteEffects.None,1);
        }

        public void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
			Vector2 org = origin;
            foreach (char c in text)
            {
                if (c == '\n')
                {
                    org.Y -= spriteFont.LineSpacing * scale.Y;
                    org.X = origin.X;
                    continue;
                }
                if (spriteFont.characterData.ContainsKey(c) == false) continue;
                GlyphData g = spriteFont.characterData[c];
                Draw(spriteFont._texture, position, g.Glyph, color, rotation, org - new Vector2(g.Cropping.X, g.Cropping.Y), scale, SpriteEffects.None, layerDepth);
                org.X -= (g.Kerning.Y + g.Kerning.Z + spriteFont.Spacing) * scale.X;
            }
        }

        public void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
			Vector2 org = origin;
            foreach (char c in text)
            {
                if (c == '\n')
                {
                    org.Y -= spriteFont.LineSpacing * scale;
                    org.X = origin.X;
                    continue;
                }
                if (spriteFont.characterData.ContainsKey(c) == false) continue;
                GlyphData g = spriteFont.characterData[c];
                Draw(spriteFont._texture, position, g.Glyph, color, rotation, org - new Vector2(g.Cropping.X, g.Cropping.Y), scale, SpriteEffects.None, layerDepth);
                org.X -= (g.Kerning.Y + g.Kerning.Z + spriteFont.Spacing) * scale;
            }
        }

        public void DrawString(SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
			DrawString(spriteFont,text.ToString(),position,color,rotation,origin,scale,effects,layerDepth);
        }

        public void DrawString(SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
			DrawString(spriteFont,text.ToString(),position,color,rotation,origin,scale,effects,layerDepth);
        }

        public XnaTouch.Framework.Graphics.GraphicsDevice GraphicsDevice
        {
            get
            {
                return _device;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
				_name = value;
            }
        }

        public object Tag
        {
            get
            {
                return _tag;
            }
            set
            {
				_tag = value;
            }
        }
    }
}

