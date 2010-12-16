
using System;
using System.Text;
using System.Collections.Generic;
using OpenTK.Graphics.ES11;

using XnaTouch.Framework;

namespace XnaTouch.Framework.Graphics
{
	public class SpriteBatch
	{
		SpriteSortMode _sortMode;
		SpriteBlendMode _blendMode;
		SaveStateMode _saveMode;
		SpriteBatcher _batcher;
		Matrix _matrix;
		GraphicsDevice _graphicsDevice;
		
		public SpriteBatch ( GraphicsDevice graphicsDevice )
		{
			_graphicsDevice = graphicsDevice;
			
			_batcher = new SpriteBatcher();
		}
		
		public void Begin()
		{
			_sortMode = SpriteSortMode.Deferred;
			_blendMode = SpriteBlendMode.AlphaBlend;
			_matrix = Matrix.Identity;
		}
		
		public void Begin(SpriteBlendMode blendMode, SpriteSortMode sortMode, SaveStateMode stateMode)
		{
			_blendMode = blendMode;
			_sortMode = sortMode;
			_saveMode = stateMode;
			_matrix = Matrix.Identity;
		}
		
		public void Begin(SpriteBlendMode blendMode, SpriteSortMode sortMode, SaveStateMode stateMode, Matrix transformMatrix)
		{
			_blendMode = blendMode;
			_sortMode = sortMode;
			_saveMode = stateMode;
			_matrix = transformMatrix;
		}
		
		public void End()
		{			
			// set the blend mode
			switch ( _blendMode )
			{
			case SpriteBlendMode.PreMultiplied :
				GL.Enable(All.Blend);
				GL.BlendFunc(All.One, All.OneMinusSrcAlpha);
				break;
			case SpriteBlendMode.AlphaBlend :
				GL.Enable(All.Blend);
				GL.BlendFunc(All.SrcAlpha, All.OneMinusSrcAlpha);
				break;
			case SpriteBlendMode.Additive :
				GL.Enable(All.Blend);
				GL.BlendFunc(All.SrcAlpha,All.One);
				break;
			case SpriteBlendMode.None :
				GL.Disable(All.Blend);
				break;
			}
			
			// set camera
			GL.MatrixMode(All.Projection);
			GL.LoadIdentity();
			GL.Ortho(0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height, 0, -1, 1);
			GL.MatrixMode(All.Modelview);
			GL.LoadMatrix( ref _matrix.M11 );
			GL.Viewport (0, 0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);
						
			// Initialize OpenGL states (ideally move this to initialize somewhere else)	
			GL.Disable(All.DepthTest);
			GL.TexEnv(All.TextureEnv, All.TextureEnvMode,(int) All.BlendSrc);
			GL.Enable(All.Texture2D);
			GL.EnableClientState(All.VertexArray);
			GL.EnableClientState(All.ColorArray);
			GL.EnableClientState(All.TextureCoordArray);
			
			GL.Disable(All.CullFace);
			GL.Color4(1.0f, 1.0f, 1.0f, 1.0f);
			
			_batcher.DrawBatch ( _sortMode );
		}
		
		public void Draw 
			( 
			 Texture2D texture,
			 Vector2 position,
			 Nullable<Rectangle> sourceRectangle,
			 Color color,
			 float rotation,
			 Vector2 origin,
			 Vector2 scale,
			 SpriteEffects effect,
			 float depth 
			 )
		{
			SpriteBatchItem item = _batcher.CreateBatchItem();
			
			item.Depth = depth;
			item.TextureID = (int) texture.ID;
			
			Rectangle rect;
			if ( sourceRectangle.HasValue)
				rect = sourceRectangle.Value;
			else
				rect = new Rectangle( 0, 0, texture.Image.ImageWidth, texture.Image.ImageHeight );
						
			Vector2 texCoordTL = texture.Image.GetTextureCoord ( rect.X, rect.Y );
			Vector2 texCoordBR = texture.Image.GetTextureCoord ( rect.X+rect.Width, rect.Y+rect.Height );
			
			if ( effect == SpriteEffects.FlipVertically )
			{
				float temp = texCoordBR.Y;
				texCoordBR.Y = texCoordTL.Y;
				texCoordTL.Y = temp;
			}
			else if ( effect == SpriteEffects.FlipHorizontally )
			{
				float temp = texCoordBR.X;
				texCoordBR.X = texCoordTL.X;
				texCoordTL.X = temp;
			}
			
			item.Set
				(
				 position.X,
				 position.Y,
				 -origin.X*scale.X,
				 -origin.Y*scale.Y,
				 rect.Width*scale.X,
				 rect.Height*scale.Y,
				 (float)Math.Sin(rotation),
				 (float)Math.Cos(rotation),
				 color,
				 texCoordTL,
				 texCoordBR
				 );
		}
		
		public void Draw 
			( 
			 Texture2D texture,
			 Vector2 position,
			 Nullable<Rectangle> sourceRectangle,
			 Color color,
			 float rotation,
			 Vector2 origin,
			 float scale,
			 SpriteEffects effect,
			 float depth 
			 )
		{
			SpriteBatchItem item = _batcher.CreateBatchItem();
			
			item.Depth = depth;
			item.TextureID = (int) texture.ID;
			
			Rectangle rect;
			if ( sourceRectangle.HasValue)
				rect = sourceRectangle.Value;
			else
				rect = new Rectangle( 0, 0, texture.Image.ImageWidth, texture.Image.ImageHeight );
			
			Vector2 texCoordTL = texture.Image.GetTextureCoord ( rect.X, rect.Y );
			Vector2 texCoordBR = texture.Image.GetTextureCoord ( rect.X+rect.Width, rect.Y+rect.Height );
			
			if ( effect == SpriteEffects.FlipVertically )
			{
				float temp = texCoordBR.Y;
				texCoordBR.Y = texCoordTL.Y;
				texCoordTL.Y = temp;
			}
			else if ( effect == SpriteEffects.FlipHorizontally )
			{
				float temp = texCoordBR.X;
				texCoordBR.X = texCoordTL.X;
				texCoordTL.X = temp;
			}
			item.Set
				(
				 position.X,
				 position.Y,
				 -origin.X*scale,
				 -origin.Y*scale,
				 rect.Width*scale,
				 rect.Height*scale,
				 (float)Math.Sin(rotation),
				 (float)Math.Cos(rotation),
				 color,
				 texCoordTL,
				 texCoordBR
				 );
		}
		
		public void Draw (
         	Texture2D texture,
         	Rectangle destinationRectangle,
         	Nullable<Rectangle> sourceRectangle,
         	Color color,
         	float rotation,
         	Vector2 origin,
         	SpriteEffects effect,
         	float depth
			)
		{
			SpriteBatchItem item = _batcher.CreateBatchItem();
			
			item.Depth = depth;
			item.TextureID = (int) texture.ID;
			
			Rectangle rect;
			if ( sourceRectangle.HasValue)
				rect = sourceRectangle.Value;
			else
				rect = new Rectangle( 0, 0, texture.Image.ImageWidth, texture.Image.ImageHeight );

			Vector2 texCoordTL = texture.Image.GetTextureCoord ( rect.X, rect.Y );
			Vector2 texCoordBR = texture.Image.GetTextureCoord ( rect.X+rect.Width, rect.Y+rect.Height );
			if ( effect == SpriteEffects.FlipVertically )
			{
				float temp = texCoordBR.Y;
				texCoordBR.Y = texCoordTL.Y;
				texCoordTL.Y = temp;
			}
			else if ( effect == SpriteEffects.FlipHorizontally )
			{
				float temp = texCoordBR.X;
				texCoordBR.X = texCoordTL.X;
				texCoordTL.X = temp;
			}
			
			item.Set 
				( 
				 destinationRectangle.X, 
				 destinationRectangle.Y, 
				 -origin.X, 
				 -origin.Y, 
				 destinationRectangle.Width,
				 destinationRectangle.Height,
				 (float)Math.Sin(rotation),
				 (float)Math.Cos(rotation),
				 color,
				 texCoordTL,
				 texCoordBR );			
		}
		
        public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color)
		{
			SpriteBatchItem item = _batcher.CreateBatchItem();
			
			item.Depth = 0.0f;
			item.TextureID = (int) texture.ID;
			
			Rectangle rect;
			if ( sourceRectangle.HasValue)
				rect = sourceRectangle.Value;
			else
				rect = new Rectangle( 0, 0, texture.Image.ImageWidth, texture.Image.ImageHeight );
			
			Vector2 texCoordTL = texture.Image.GetTextureCoord ( rect.X, rect.Y );
			Vector2 texCoordBR = texture.Image.GetTextureCoord ( rect.X+rect.Width, rect.Y+rect.Height );
			
			item.Set ( position.X, position.Y, rect.Width, rect.Height, color, texCoordTL, texCoordBR );
		}
		
		public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color)
		{
			SpriteBatchItem item = _batcher.CreateBatchItem();
			
			item.Depth = 0.0f;
			item.TextureID = (int) texture.ID;
			
			Rectangle rect;
			if ( sourceRectangle.HasValue)
				rect = sourceRectangle.Value;
			else
				rect = new Rectangle( 0, 0, texture.Image.ImageWidth, texture.Image.ImageHeight );
			
			Vector2 texCoordTL = texture.Image.GetTextureCoord ( rect.X, rect.Y );
			Vector2 texCoordBR = texture.Image.GetTextureCoord ( rect.X+rect.Width, rect.Y+rect.Height );
			
			item.Set 
				( 
				 destinationRectangle.X, 
				 destinationRectangle.Y, 
				 destinationRectangle.Width, 
				 destinationRectangle.Height, 
				 color, 
				 texCoordTL, 
				 texCoordBR );
		}
		
		public void Draw 
			( 
			 Texture2D texture,
			 Vector2 position,
			 Color color
			 )
		{
			SpriteBatchItem item = _batcher.CreateBatchItem();
			
			item.Depth = 0;
			item.TextureID = (int) texture.ID;
			
			Rectangle rect = new Rectangle( 0, 0, texture.Image.ImageWidth, texture.Image.ImageHeight );
			
			Vector2 texCoordTL = texture.Image.GetTextureCoord ( rect.X, rect.Y );
			Vector2 texCoordBR = texture.Image.GetTextureCoord ( rect.X+rect.Width, rect.Y+rect.Height );
			
			item.Set 
				(
				 position.X,
			     position.Y,
				 rect.Width,
				 rect.Height,
				 color,
				 texCoordTL,
				 texCoordBR
				 );
		}
		
		public void Draw (Texture2D texture, Rectangle rectangle, Color color)
		{
			SpriteBatchItem item = _batcher.CreateBatchItem();
			
			item.Depth = 0;
			item.TextureID = (int) texture.ID;
			
			Vector2 texCoordTL = texture.Image.GetTextureCoord ( 0, 0 );
			Vector2 texCoordBR = texture.Image.GetTextureCoord ( texture.Image.ImageWidth, texture.Image.ImageHeight );
			
			item.Set
				(
				 rectangle.X,
				 rectangle.Y,
				 rectangle.Width,
				 rectangle.Height,
				 color,
				 texCoordTL,
				 texCoordBR
			    );
		}
		
		
		public void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color)
		{
			Vector2 p = position;
			
            foreach (char c in text)
            {
                if (c == '\n')
                {
                    p.Y += spriteFont.LineSpacing;
                    p.X = position.X;
                    continue;
                }
                if (spriteFont.characterData.ContainsKey(c) == false) 
					continue;
                GlyphData g = spriteFont.characterData[c];
				
				SpriteBatchItem item = _batcher.CreateBatchItem();
				
				item.Depth = 0.0f;
				item.TextureID = (int) spriteFont._texture.ID;

				Vector2 texCoordTL = spriteFont._texture.Image.GetTextureCoord ( g.Glyph.X, g.Glyph.Y );
				Vector2 texCoordBR = spriteFont._texture.Image.GetTextureCoord ( g.Glyph.X+g.Glyph.Width, g.Glyph.Y+g.Glyph.Height );

				item.Set
					(
					 p.X,
					 p.Y+g.Cropping.Y,
					 g.Glyph.Width,
					 g.Glyph.Height,
					 color,
					 texCoordTL,
					 texCoordBR
					 );
		                
				p.X += (g.Kerning.Y + g.Kerning.Z + spriteFont.Spacing);
            }			
		}
		
		public void DrawString
			(
			SpriteFont spriteFont, 
			string text, 
			Vector2 position,
			Color color,
			float rotation,
			Vector2 origin,
			float scale,
			SpriteEffects effects,
			float depth
			)
		{
			Vector2 p = new Vector2(-origin.X,-origin.Y);
			
			float sin = (float)Math.Sin(rotation);
			float cos = (float)Math.Cos(rotation);
			
            foreach (char c in text)
            {
                if (c == '\n')
                {
                    p.Y += spriteFont.LineSpacing;
                    p.X = -origin.X;
                    continue;
                }
                if (spriteFont.characterData.ContainsKey(c) == false) 
					continue;
                GlyphData g = spriteFont.characterData[c];
				
				SpriteBatchItem item = _batcher.CreateBatchItem();
				
				item.Depth = depth;
				item.TextureID = (int) spriteFont._texture.ID;

				Vector2 texCoordTL = spriteFont._texture.Image.GetTextureCoord ( g.Glyph.X, g.Glyph.Y );
				Vector2 texCoordBR = spriteFont._texture.Image.GetTextureCoord ( g.Glyph.X+g.Glyph.Width, g.Glyph.Y+g.Glyph.Height );
				
				if ( effects == SpriteEffects.FlipVertically )
				{
					float temp = texCoordBR.Y;
					texCoordBR.Y = texCoordTL.Y;
					texCoordTL.Y = temp;
				}
				else if ( effects == SpriteEffects.FlipHorizontally )
				{
					float temp = texCoordBR.X;
					texCoordBR.X = texCoordTL.X;
					texCoordTL.X = temp;
				}
				
				item.Set
					(
					 position.X,
					 position.Y,
					 p.X*scale,
					 (p.Y+g.Cropping.Y)*scale,
					 g.Glyph.Width*scale,
					 g.Glyph.Height*scale,
					 sin,
					 cos,
					 color,
					 texCoordTL,
					 texCoordBR
					 );

				p.X += (g.Kerning.Y + g.Kerning.Z + spriteFont.Spacing);
            }			
		}
		
		public void DrawString
			(
			SpriteFont spriteFont, 
			string text, 
			Vector2 position,
			Color color,
			float rotation,
			Vector2 origin,
			Vector2 scale,
			SpriteEffects effects,
			float depth
			)
		{
			Vector2 p = new Vector2(-origin.X,-origin.Y);
			
			float sin = (float)Math.Sin(rotation);
			float cos = (float)Math.Cos(rotation);
			
            foreach (char c in text)
            {
                if (c == '\n')
                {
                    p.Y += spriteFont.LineSpacing;
                    p.X = -origin.X;
                    continue;
                }
                if (spriteFont.characterData.ContainsKey(c) == false) 
					continue;
                GlyphData g = spriteFont.characterData[c];
				
				SpriteBatchItem item = _batcher.CreateBatchItem();
				
				item.Depth = depth;
				item.TextureID = (int) spriteFont._texture.ID;

				Vector2 texCoordTL = spriteFont._texture.Image.GetTextureCoord ( g.Glyph.X, g.Glyph.Y );
				Vector2 texCoordBR = spriteFont._texture.Image.GetTextureCoord ( g.Glyph.X+g.Glyph.Width, g.Glyph.Y+g.Glyph.Height );
				
				if ( effects == SpriteEffects.FlipVertically )
				{
					float temp = texCoordBR.Y;
					texCoordBR.Y = texCoordTL.Y;
					texCoordTL.Y = temp;
				}
				else if ( effects == SpriteEffects.FlipHorizontally )
				{
					float temp = texCoordBR.X;
					texCoordBR.X = texCoordTL.X;
					texCoordTL.X = temp;
				}
				
				item.Set
					(
					 position.X,
					 position.Y,
					 p.X*scale.X,
					 (p.Y+g.Cropping.Y)*scale.Y,
					 g.Glyph.Width*scale.X,
					 g.Glyph.Height*scale.Y,
					 sin,
					 cos,
					 color,
					 texCoordTL,
					 texCoordBR
					 );

				p.X += (g.Kerning.Y + g.Kerning.Z + spriteFont.Spacing);
            }			
		}
		
		public void DrawString(SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color)
		{
			DrawString ( spriteFont, text.ToString(), position, color );
		}
		
		public void DrawString
			(
			SpriteFont spriteFont, 
			StringBuilder text, 
			Vector2 position,
			Color color,
			float rotation,
			Vector2 origin,
			float scale,
			SpriteEffects effects,
			float depth
			)
		{
			DrawString ( spriteFont, text.ToString(), position, color, rotation, origin, scale, effects, depth );
		}
		
		public void DrawString
			(
			SpriteFont spriteFont, 
			StringBuilder text, 
			Vector2 position,
			Color color,
			float rotation,
			Vector2 origin,
			Vector2 scale,
			SpriteEffects effects,
			float depth
			)
		{
			DrawString ( spriteFont, text.ToString(), position, color, rotation, origin, scale, effects, depth );
		}
	}
}

