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
using System.Collections.Generic;

namespace XnaTouch.Framework.Graphics
{
    public class SpriteBatch : GraphicsResource
    {
		private XnaTouch.Framework.Graphics.GraphicsDevice _device;
		private object _tag;
		private string _name;
		private Matrix? _currentTransformMatrix;

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
			_currentTransformMatrix = transformMatrix;
			GraphicsDevice.StartSpriteBatch(blendMode, sortMode);
        }
		
		public void Begin (
         SpriteSortMode sortMode,
         BlendState blendState,
         SamplerState samplerState,
         DepthStencilState depthStencilState,
         RasterizerState rasterizerState,
         Effect effect,
         Matrix transformMatrix)
		{
			_currentTransformMatrix = transformMatrix;
			// TODO GraphicsDevice.StartSpriteBatch(blendMode, sortMode);
		}
		
		public void End()
        {
			_currentTransformMatrix = null;
			GraphicsDevice.EndSpriteBatch();
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

				//render
				AddToSpriteRender(mode,position,sourceRectangle.Value);
				
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
							
			if (sourceRectangle.HasValue)
			{
				// adjust the scale
				mode.HorizontalScale = (float) destinationRectangle.Width / (float)sourceRectangle.Value.Width;
				mode.VerticalScale = (float)destinationRectangle.Height / (float)sourceRectangle.Value.Height;	
																						
				//render
				AddToSpriteRender(mode,new Vector2(destinationRectangle.X,destinationRectangle.Y), sourceRectangle.Value);
			}
			else 
			{
				// adjust the scale
				mode.HorizontalScale = (float) destinationRectangle.Width / (float)texture.Width;
				mode.VerticalScale = (float)destinationRectangle.Height / (float)texture.Height;	
				
				//render		
				AddToSpriteRender(mode, new Vector2(destinationRectangle.X, destinationRectangle.Y),texture.SourceRect);
			}
        }

        public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
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
				destination.Width = (int)(sourceRectangle.Value.Width*scale.X);
				destination.Height =(int)(sourceRectangle.Value.Height*scale.Y);				
			}
			else 
			{
				destination.Width = (int)(texture.Width*scale.X);
				destination.Height = (int)(texture.Height*scale.Y);
			}
			
			Draw(texture, destination, sourceRectangle, color, rotation, origin, effects, layerDepth);
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
				destination.Width = (int)(sourceRectangle.Value.Width*scale);
				destination.Height = (int)(sourceRectangle.Value.Height*scale);				
			}
			else 
			{
				destination.Width = (int)(texture.Width*scale);
				destination.Height = (int)(texture.Height*scale);
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
			
			List<Vector2> spriteVertices = new List<Vector2>();
			List<Vector2> textureVertices = new List<Vector2>();
			
			RenderMode mode = new RenderMode();
			mode.Texture = spriteFont._texture;
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
			
			float textWidth = 0;
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
				
				g.Glyph.Width = (int)(g.Glyph.Width * scale.X);
				g.Glyph.Height =(int)(g.Glyph.Height * scale.Y);				
				
				textWidth += (g.Kerning.Y + g.Kerning.Z + spriteFont.Spacing) * scale.X;
				
				mode.Origin = new Vector2(org.X, org.Y - g.Cropping.Y);
		
				Vector2[] glyphVertices = GetSpriteVertices(g.Glyph.Width,g.Glyph.Height,mode);	
				// Scale origin before rendering/transform
				Vector2 scaledOrigin = new Vector2(mode.Origin.X * mode.HorizontalScale, 
			                                mode.Origin.Y * mode.VerticalScale);
				for (int i = 0; i < 4; i++)
				{
					glyphVertices[i] -= scaledOrigin;
				}
				spriteVertices.AddRange(glyphVertices);
				textureVertices.AddRange(mode.Texture.Image.GetTextureCoordinates(g.Glyph));
                
				org.X -= (g.Kerning.Y + g.Kerning.Z + spriteFont.Spacing) * scale.X;
            }
			
			Vector2[] temp = spriteVertices.ToArray();
			
			mode.Origin  = origin * new Vector2(mode.HorizontalScale, mode.VerticalScale);
			ApplyTransformations(temp,position,textWidth,0,mode);			
			
			SpriteBatchRenderItem sbi = new SpriteBatchRenderItem();
			sbi.Position = position;
			sbi.RenderData = mode;
			sbi.TextureCoordinates = textureVertices.ToArray();
			sbi.Vertices = temp;
			
			GraphicsDevice.AddToSpriteBuffer(sbi);
        }

        public void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
			DrawString(spriteFont,text,position,color,rotation,origin,new Vector2(scale,scale),effects,layerDepth);
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
		
		private Vector2[] GetSpriteVertices(float Width, float Height, RenderMode renderMode)
		{
			Vector2[] SpriteVertices = new Vector2[4];
			
			float quadWidth = Width * renderMode.HorizontalScale;
			float quadHeight = Height * renderMode.VerticalScale;
			
			if (!renderMode.FlipVertical && !renderMode.FlipHorizontal) 
			{
				SpriteVertices[0] = new Vector2(quadWidth,0);
				SpriteVertices[1] = new Vector2(quadWidth,quadHeight);
				SpriteVertices[2] = new Vector2(0,0);				
				SpriteVertices[3] = new Vector2(0,quadHeight);
			}
			if (!renderMode.FlipVertical && renderMode.FlipHorizontal) 
			{				
				SpriteVertices[0] = new Vector2(0,0);
				SpriteVertices[1] = new Vector2(0,quadHeight);
				SpriteVertices[2] = new Vector2(quadWidth,0);	
				SpriteVertices[3] = new Vector2(quadWidth,quadHeight);
			}
			if (renderMode.FlipVertical && !renderMode.FlipHorizontal) 
			{				
				SpriteVertices[0] = new Vector2(quadWidth,quadHeight);
				SpriteVertices[1] = new Vector2(quadWidth,0);
				SpriteVertices[2] = new Vector2(0,quadHeight);	
				SpriteVertices[3] = new Vector2(0,0);
			}
			if (renderMode.FlipVertical && renderMode.FlipHorizontal) 
			{
				SpriteVertices[0] = new Vector2(0,quadHeight);
				SpriteVertices[1] = new Vector2(0,0);
				SpriteVertices[2] = new Vector2(quadWidth,quadHeight);
				SpriteVertices[3] = new Vector2(quadWidth,0);
			}
			return SpriteVertices;
		}
		
		private Vector2[] ApplyTransformations(Vector2[] SpriteVertices, Vector2 position, float Width, float Height, RenderMode renderMode)
		{
			// Scale origin before rendering/transform
			renderMode.Origin = new Vector2(renderMode.Origin.X * renderMode.HorizontalScale, 
			                                renderMode.Origin.Y * renderMode.VerticalScale);
			// Translate origin
			Matrix matrix = Matrix.CreateTranslation (-renderMode.Origin.X, -renderMode.Origin.Y, 0);
			//Matrix matrix = Matrix.CreateTranslation (0, renderMode.Origin.Y * 2, 0);
			// Rotate if needed
			if (renderMode.Rotation != 0.0f) 
			{
				matrix *= Matrix.CreateRotationZ (renderMode.Rotation);
			}
			// Translate location
			matrix *= Matrix.CreateTranslation (position.X, position.Y, 0);
			// Apply the SpriteBatch's transformMatrix if one was specified
			if (_currentTransformMatrix.HasValue) 
			{
				matrix *= _currentTransformMatrix.Value;;
			}		
			// Apply the matrix transformation to each vertice
			for (int i = 0; i < SpriteVertices.Length; i++) 
			{
				SpriteVertices[i] = Vector2.Transform (SpriteVertices[i], matrix);
			}			
			return SpriteVertices;
		}
	
		private void AddToSpriteRender(RenderMode mode, Vector2 position, Rectangle rect)
		{
			// Get Vertices
			Vector2[] spriteVertices = GetSpriteVertices(rect.Width, rect.Height,mode);
			Vector2[] textureCoordinates = mode.Texture.Image.GetTextureCoordinates(rect);
			// Apply Transformations
			ApplyTransformations(spriteVertices, position,rect.Width,rect.Height,mode);

			SpriteBatchRenderItem sbi = new SpriteBatchRenderItem();
			sbi.Position = position;
			sbi.RenderData = mode;
			sbi.TextureCoordinates = textureCoordinates;
			sbi.Vertices = spriteVertices;
			
			GraphicsDevice.AddToSpriteBuffer(sbi);
		}
    }
}