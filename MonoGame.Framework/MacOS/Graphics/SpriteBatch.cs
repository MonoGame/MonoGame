using System;
using System.Text;
using System.Collections.Generic;

using MonoMac.OpenGL;

using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Framework.Graphics
{
	public class SpriteBatch : GraphicsResource
	{
		SpriteBatcher _batcher;
		SpriteSortMode _sortMode;
		BlendState _blendState;
		SamplerState _samplerState;
		DepthStencilState _depthStencilState; 
		RasterizerState _rasterizerState;		
		Effect _effect;	
		SpriteEffect spriteEffect;
		Matrix _matrix;
		Rectangle tempRect = new Rectangle (0,0,0,0);
		Vector2 texCoordTL = new Vector2 (0,0);
		Vector2 texCoordBR = new Vector2 (0,0);

		public SpriteBatch (GraphicsDevice graphicsDevice)
		{
			if (graphicsDevice == null) {
				throw new ArgumentException ("graphicsDevice");
			}	

			this.graphicsDevice = graphicsDevice;
			spriteEffect = new SpriteEffect (this.graphicsDevice);	
			_batcher = new SpriteBatcher ();
		}

		public void Begin ()
		{
			Begin (SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.Identity);			
		}

		public void Begin (SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect, Matrix transformMatrix)
		{

			// defaults
			_sortMode = sortMode;
			_blendState = blendState ?? BlendState.AlphaBlend;
			_samplerState = samplerState ?? SamplerState.LinearClamp;
			_depthStencilState = depthStencilState ?? DepthStencilState.None;
			_rasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;

			//if (effect != null)
				_effect = effect;

			_matrix = transformMatrix;
		}

		public void Begin (SpriteSortMode sortMode, BlendState blendState)
		{
			Begin (sortMode, blendState, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.Identity);			
		}

		public void Begin (SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState)
		{
			Begin (sortMode, blendState, samplerState, depthStencilState, rasterizerState, null, Matrix.Identity);	
		}

		public void Begin (SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect)
		{

			Begin (sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, Matrix.Identity);			
		}

		public void End ()
		{	

			// apply the custom effect if there is one
			if (_effect != null) {
				_effect.CurrentTechnique.Passes [0].Apply ();

				if (graphicsDevice.Textures._textures.Count > 0) {
					foreach (EffectParameter ep in _effect._textureMappings) {
						// if user didn't inform the texture index, we can't bind it
						if (ep.UserInedx == -1)
							continue;

						Texture tex = graphicsDevice.Textures [ep.UserInedx];

						// Need to support multiple passes as well
						GL.ActiveTexture ((TextureUnit)((int)TextureUnit.Texture0 + ep.UserInedx));
						GL.BindTexture (TextureTarget.Texture2D, tex._textureId);
						GL.Uniform1 (ep.UniformLocation, ep.UserInedx);
					}
				}
			}

			// Disable Blending by default = BlendState.Opaque
			//GL.Disable (EnableCap.Blend);

			// set the blend mode
//			if (_blendState == BlendState.NonPremultiplied) {
//				GL.BlendFunc (BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
//				GL.Enable (EnableCap.Blend);
//			}
//
//			if (_blendState == BlendState.AlphaBlend) {
//				GL.BlendFunc (BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
//				GL.Enable (EnableCap.Blend);
//			}
//
//			if (_blendState == BlendState.Additive) {
//				GL.BlendFunc (BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
//				GL.Enable (EnableCap.Blend);
//			}
			graphicsDevice.BlendState = _blendState;
			graphicsDevice.SetGraphicsStates();
			// set camera
			GL.MatrixMode (MatrixMode.Projection);
			GL.LoadIdentity ();		

			// Switch on the flags.
			switch (this.graphicsDevice.PresentationParameters.DisplayOrientation) {
			case DisplayOrientation.LandscapeLeft:
				{
					GL.Rotate (-90, 0, 0, 1); 
					GL.Ortho (0, this.graphicsDevice.Viewport.Height, this.graphicsDevice.Viewport.Width, 0, -1, 1);
					break;
				}

			case DisplayOrientation.LandscapeRight:
				{
					GL.Rotate (90, 0, 0, 1); 
					GL.Ortho (0, this.graphicsDevice.Viewport.Height, this.graphicsDevice.Viewport.Width, 0, -1, 1);
					break;
				}

			case DisplayOrientation.PortraitUpsideDown:
				{
					GL.Rotate (180, 0, 0, 1); 
					GL.Ortho (0, this.graphicsDevice.Viewport.Width, this.graphicsDevice.Viewport.Height, 0, -1, 1);
					break;
				}

			default:
				{
					GL.Ortho (0, this.graphicsDevice.Viewport.Width, this.graphicsDevice.Viewport.Height, 0, -1, 1);
					break;
				}
			}

			// Enable Scissor Tests if necessary
			if (this.graphicsDevice.RasterizerState.ScissorTestEnable) {
				GL.Enable (EnableCap.ScissorTest);				
			}

			GL.MatrixMode (MatrixMode.Modelview);

			GL.Viewport (0, 0, this.graphicsDevice.Viewport.Width, this.graphicsDevice.Viewport.Height);

			// Enable Scissor Tests if necessary
			if (this.graphicsDevice.RasterizerState.ScissorTestEnable) {
				GL.Scissor (this.graphicsDevice.ScissorRectangle.X, this.graphicsDevice.ScissorRectangle.Y, this.graphicsDevice.ScissorRectangle.Width, this.graphicsDevice.ScissorRectangle.Height);
			}

			GL.LoadMatrix (ref _matrix.M11);

			// Initialize OpenGL states (ideally move this to initialize somewhere else)
			GLStateManager.SetDepthStencilState(_depthStencilState);

			//GL.Disable (EnableCap.DepthTest);
			
			GL.TexEnv (TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)All.BlendSrc);
			GL.Enable (EnableCap.Texture2D);
			GL.EnableClientState (ArrayCap.VertexArray);
			GL.EnableClientState (ArrayCap.ColorArray);
			GL.EnableClientState (ArrayCap.TextureCoordArray);

			// Enable Culling for better performance
			GL.Enable (EnableCap.CullFace);
			GL.FrontFace (FrontFaceDirection.Cw);
			GL.Color4 (1.0f, 1.0f, 1.0f, 1.0f);

			_batcher.DrawBatch (_sortMode, _samplerState);
	
			// Disable Scissor Tests if necessary
			if (this.graphicsDevice.RasterizerState.ScissorTestEnable) {
				GL.Disable (EnableCap.ScissorTest);
			}

			// clear out the textures
			graphicsDevice.Textures._textures.Clear ();
			
			// unbinds shader
			if (_effect != null) {
				GL.UseProgram (0);
				_effect = null;
			}
			
			spriteEffect.CurrentTechnique.Passes [0].Apply ();


		}

		public void Draw (Texture2D texture,
				Vector2 position,
				Nullable<Rectangle> sourceRectangle,
				Color color,
				float rotation,
				Vector2 origin,
				Vector2 scale,
				SpriteEffects effect,
				float depth)
		{
			if (texture == null) {
				throw new ArgumentException ("texture");
			}
			
			// texture 0 is the texture beeing draw
			graphicsDevice.Textures [0] = texture;
			
			SpriteBatchItem item = _batcher.CreateBatchItem ();

			item.Depth = depth;
			item.TextureID = (int)texture.ID;

			if (sourceRectangle.HasValue) {
				tempRect = sourceRectangle.Value;
			} else {
				tempRect.X = 0;
				tempRect.Y = 0;
				tempRect.Width = texture.Width;
				tempRect.Height = texture.Height;				
			}

			if (texture.Image == null) {
				float texWidthRatio = 1.0f / (float)texture.Width;
				float texHeightRatio = 1.0f / (float)texture.Height;
				// We are initially flipped vertically so we need to flip the corners so that
				//  the image is bottom side up to display correctly
				texCoordTL.X = tempRect.X * texWidthRatio;
				//texCoordTL.Y = (tempRect.Y + tempRect.Height) * texHeightRatio;
				texCoordTL.Y = 1.0f - tempRect.Y * texHeightRatio;

				texCoordBR.X = (tempRect.X + tempRect.Width) * texWidthRatio;
				//texCoordBR.Y = tempRect.Y * texHeightRatio;
				texCoordBR.Y = 1.0f - ( tempRect.Y+tempRect.Height )*texHeightRatio;


			} else {
				texCoordTL.X = texture.Image.GetTextureCoordX (tempRect.X);
				texCoordTL.Y = texture.Image.GetTextureCoordY (tempRect.Y);
				texCoordBR.X = texture.Image.GetTextureCoordX (tempRect.X + tempRect.Width);
				texCoordBR.Y = texture.Image.GetTextureCoordY (tempRect.Y + tempRect.Height);
			}

			if ((effect & SpriteEffects.FlipVertically) != 0) {
				float temp = texCoordBR.Y;
				texCoordBR.Y = texCoordTL.Y;
				texCoordTL.Y = temp;
			}
			if ((effect & SpriteEffects.FlipHorizontally) != 0) {
				float temp = texCoordBR.X;
				texCoordBR.X = texCoordTL.X;
				texCoordTL.X = temp;
			}

			item.Set (position.X, 
					position.Y, 
					-origin.X * scale.X, 
					-origin.Y * scale.Y, 
					tempRect.Width * scale.X, 
					tempRect.Height * scale.Y, 
					(float)Math.Sin (rotation), 
					(float)Math.Cos (rotation), 
					color, 
					texCoordTL, 
					texCoordBR);
		}

		public void Draw (Texture2D texture,
				Vector2 position,
				Nullable<Rectangle> sourceRectangle,
				Color color,
				float rotation,
				Vector2 origin,
				float scale,
				SpriteEffects effect,
				float depth)
		{
			if (texture == null) {
				throw new ArgumentException ("texture");
			}
			
			// texture 0 is the texture beeing draw
			graphicsDevice.Textures [0] = texture;			
			
			SpriteBatchItem item = _batcher.CreateBatchItem ();

			item.Depth = depth;
			item.TextureID = (int)texture.ID;

			if (sourceRectangle.HasValue) {
				tempRect = sourceRectangle.Value;
			} else {
				tempRect.X = 0;
				tempRect.Y = 0;
				tempRect.Width = texture.Width;
				tempRect.Height = texture.Height;				
			}

			if (texture.Image == null) {
				float texWidthRatio = 1.0f / (float)texture.Width;
				float texHeightRatio = 1.0f / (float)texture.Height;
				// We are initially flipped vertically so we need to flip the corners so that
				//  the image is bottom side up to display correctly
				texCoordTL.X = tempRect.X * texWidthRatio;
				//texCoordTL.Y = (tempRect.Y + tempRect.Height) * texHeightRatio;
				texCoordTL.Y = 1.0f - tempRect.Y * texHeightRatio;

				texCoordBR.X = (tempRect.X + tempRect.Width) * texWidthRatio;
				//texCoordBR.Y = tempRect.Y * texHeightRatio;
				texCoordBR.Y = 1.0f - ( tempRect.Y+tempRect.Height )*texHeightRatio;


			} else {
				texCoordTL.X = texture.Image.GetTextureCoordX (tempRect.X);
				texCoordTL.Y = texture.Image.GetTextureCoordY (tempRect.Y);
				texCoordBR.X = texture.Image.GetTextureCoordX (tempRect.X + tempRect.Width);
				texCoordBR.Y = texture.Image.GetTextureCoordY (tempRect.Y + tempRect.Height);
			}
			
			if ((effect & SpriteEffects.FlipVertically) != 0) {
				float temp = texCoordBR.Y;
				texCoordBR.Y = texCoordTL.Y;
				texCoordTL.Y = temp;
			}
			if ((effect & SpriteEffects.FlipHorizontally) != 0) {
				float temp = texCoordBR.X;
				texCoordBR.X = texCoordTL.X;
				texCoordTL.X = temp;
			}

			item.Set (position.X, 
					position.Y, 
					-origin.X * scale, 
					-origin.Y * scale, 
					tempRect.Width * scale, 
					tempRect.Height * scale, 
					(float)Math.Sin (rotation), 
					(float)Math.Cos (rotation), 
					color, 
					texCoordTL, 
					texCoordBR);
		}

		public void Draw (Texture2D texture,
			Rectangle destinationRectangle,
			Nullable<Rectangle> sourceRectangle,
			Color color,
			float rotation,
			Vector2 origin,
			SpriteEffects effect,
			float depth)
		{
			if (texture == null) {
				throw new ArgumentException ("texture");
			}
			
			// texture 0 is the texture beeing draw
			graphicsDevice.Textures [0] = texture;			
			
			SpriteBatchItem item = _batcher.CreateBatchItem ();

			item.Depth = depth;
			item.TextureID = (int)texture.ID;

			if (sourceRectangle.HasValue) {
				tempRect = sourceRectangle.Value;
			} else {
				tempRect.X = 0;
				tempRect.Y = 0;
				tempRect.Width = texture.Width;
				tempRect.Height = texture.Height;				
			}

			if (texture.Image == null) {
				float texWidthRatio = 1.0f / (float)texture.Width;
				float texHeightRatio = 1.0f / (float)texture.Height;
				// We are initially flipped vertically so we need to flip the corners so that
				//  the image is bottom side up to display correctly
				texCoordTL.X = tempRect.X * texWidthRatio;
				//texCoordTL.Y = (tempRect.Y + tempRect.Height) * texHeightRatio;
				texCoordTL.Y = 1.0f - tempRect.Y * texHeightRatio;

				texCoordBR.X = (tempRect.X + tempRect.Width) * texWidthRatio;
				//texCoordBR.Y = tempRect.Y * texHeightRatio;
				texCoordBR.Y = 1.0f - ( tempRect.Y+tempRect.Height )*texHeightRatio;


			} else {
				texCoordTL.X = texture.Image.GetTextureCoordX (tempRect.X);
				texCoordTL.Y = texture.Image.GetTextureCoordY (tempRect.Y);
				texCoordBR.X = texture.Image.GetTextureCoordX (tempRect.X + tempRect.Width);
				texCoordBR.Y = texture.Image.GetTextureCoordY (tempRect.Y + tempRect.Height);
			}

			if ((effect & SpriteEffects.FlipVertically) != 0) {
				float temp = texCoordBR.Y;
				texCoordBR.Y = texCoordTL.Y;
				texCoordTL.Y = temp;
			}
			if ((effect & SpriteEffects.FlipHorizontally) != 0) {
				float temp = texCoordBR.X;
				texCoordBR.X = texCoordTL.X;
				texCoordTL.X = temp;
			}

			item.Set (destinationRectangle.X, 
					destinationRectangle.Y, 
					-origin.X, 
					-origin.Y, 
					destinationRectangle.Width, 
					destinationRectangle.Height, 
					(float)Math.Sin (rotation), 
					(float)Math.Cos (rotation), 
					color, 
					texCoordTL, 
					texCoordBR);			
		}

		public void Draw (Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color)
		{
			if (texture == null) {
				throw new ArgumentException ("texture");
			}
			
			// texture 0 is the texture beeing draw
			graphicsDevice.Textures [0] = texture;			
			
			SpriteBatchItem item = _batcher.CreateBatchItem ();

			item.Depth = 0.0f;
			item.TextureID = (int)texture.ID;

			if (sourceRectangle.HasValue) {
				tempRect = sourceRectangle.Value;
			} else {
				tempRect.X = 0;
				tempRect.Y = 0;
				tempRect.Width = texture.Width;
				tempRect.Height = texture.Height;				
			}

			if (texture.Image == null) {
				float texWidthRatio = 1.0f / (float)texture.Width;
				float texHeightRatio = 1.0f / (float)texture.Height;
				// We are initially flipped vertically so we need to flip the corners so that
				//  the image is bottom side up to display correctly
				texCoordTL.X = tempRect.X * texWidthRatio;
				//texCoordTL.Y = (tempRect.Y + tempRect.Height) * texHeightRatio;
				texCoordTL.Y = 1.0f - tempRect.Y * texHeightRatio;

				texCoordBR.X = (tempRect.X + tempRect.Width) * texWidthRatio;
				//texCoordBR.Y = tempRect.Y * texHeightRatio;
				texCoordBR.Y = 1.0f - ( tempRect.Y+tempRect.Height )*texHeightRatio;


			} else {
				texCoordTL.X = texture.Image.GetTextureCoordX (tempRect.X);
				texCoordTL.Y = texture.Image.GetTextureCoordY (tempRect.Y);
				texCoordBR.X = texture.Image.GetTextureCoordX (tempRect.X + tempRect.Width);
				texCoordBR.Y = texture.Image.GetTextureCoordY (tempRect.Y + tempRect.Height);
			}

			item.Set (position.X, position.Y, tempRect.Width, tempRect.Height, color, texCoordTL, texCoordBR);
		}

		public void Draw (Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color)
		{
			if (texture == null) {
				throw new ArgumentException ("texture");
			}
			
			// texture 0 is the texture beeing draw
			graphicsDevice.Textures [0] = texture;			
			
			SpriteBatchItem item = _batcher.CreateBatchItem ();

			item.Depth = 0.0f;
			item.TextureID = (int)texture.ID;

			if (sourceRectangle.HasValue) {
				tempRect = sourceRectangle.Value;
			} else {
				tempRect.X = 0;
				tempRect.Y = 0;
				tempRect.Width = texture.Width;
				tempRect.Height = texture.Height;				
			}

			if (texture.Image == null) {
				float texWidthRatio = 1.0f / (float)texture.Width;
				float texHeightRatio = 1.0f / (float)texture.Height;
				// We are initially flipped vertically so we need to flip the corners so that
				//  the image is bottom side up to display correctly
				texCoordTL.X = tempRect.X * texWidthRatio;
				//texCoordTL.Y = (tempRect.Y + tempRect.Height) * texHeightRatio;
				texCoordTL.Y = 1.0f - tempRect.Y * texHeightRatio;

				texCoordBR.X = (tempRect.X + tempRect.Width) * texWidthRatio;
				//texCoordBR.Y = tempRect.Y * texHeightRatio;
				texCoordBR.Y = 1.0f - ( tempRect.Y+tempRect.Height )*texHeightRatio;


			} else {
				texCoordTL.X = texture.Image.GetTextureCoordX (tempRect.X);
				texCoordTL.Y = texture.Image.GetTextureCoordY (tempRect.Y);
				texCoordBR.X = texture.Image.GetTextureCoordX (tempRect.X + tempRect.Width);
				texCoordBR.Y = texture.Image.GetTextureCoordY (tempRect.Y + tempRect.Height);
			}

			item.Set (destinationRectangle.X, 
					destinationRectangle.Y, 
					destinationRectangle.Width, 
					destinationRectangle.Height, 
					color, 
					texCoordTL, 
					texCoordBR);
		}

		public void Draw (Texture2D texture,
				Vector2 position,
				Color color)
		{
			if (texture == null) {
				throw new ArgumentException ("texture");
			}
			
			// texture 0 is the texture beeing draw
			graphicsDevice.Textures [0] = texture;			
			
			SpriteBatchItem item = _batcher.CreateBatchItem ();

			item.Depth = 0;
			item.TextureID = (int)texture.ID;

			tempRect.X = 0;
			tempRect.Y = 0;
			tempRect.Width = texture.Width;
			tempRect.Height = texture.Height;

			if (texture.Image == null) {
				float texWidthRatio = 1.0f / (float)texture.Width;
				float texHeightRatio = 1.0f / (float)texture.Height;
				// We are initially flipped vertically so we need to flip the corners so that
				//  the image is bottom side up to display correctly
				texCoordTL.X = tempRect.X * texWidthRatio;
				//texCoordTL.Y = (tempRect.Y + tempRect.Height) * texHeightRatio;
				texCoordTL.Y = 1.0f - tempRect.Y * texHeightRatio;

				texCoordBR.X = (tempRect.X + tempRect.Width) * texWidthRatio;
				//texCoordBR.Y = tempRect.Y * texHeightRatio;
				texCoordBR.Y = 1.0f - ( tempRect.Y+tempRect.Height )*texHeightRatio;


			} else {
				texCoordTL.X = texture.Image.GetTextureCoordX (tempRect.X);
				texCoordTL.Y = texture.Image.GetTextureCoordY (tempRect.Y);
				texCoordBR.X = texture.Image.GetTextureCoordX (tempRect.X + tempRect.Width);
				texCoordBR.Y = texture.Image.GetTextureCoordY (tempRect.Y + tempRect.Height);
			}

			item.Set (position.X, 
				position.Y, 
				tempRect.Width, 
				tempRect.Height, 
				color, 
				texCoordTL, 
				texCoordBR);
		}

		public void Draw (Texture2D texture, Rectangle rectangle, Color color)
		{
			if (texture == null) {
				throw new ArgumentException ("texture");
			}
			
			// texture 0 is the texture beeing draw
			graphicsDevice.Textures [0] = texture;			
			
			SpriteBatchItem item = _batcher.CreateBatchItem ();

			item.Depth = 0;
			item.TextureID = (int)texture.ID;

			tempRect.X = 0;
			tempRect.Y = 0;
			tempRect.Width = texture.Width;
			tempRect.Height = texture.Height;			

			if (texture.Image == null) {
				float texWidthRatio = 1.0f / (float)texture.Width;
				float texHeightRatio = 1.0f / (float)texture.Height;
				// We are initially flipped vertically so we need to flip the corners so that
				//  the image is bottom side up to display correctly
				texCoordTL.X = tempRect.X * texWidthRatio;
				//texCoordTL.Y = (tempRect.Y + tempRect.Height) * texHeightRatio;
				texCoordTL.Y = 1.0f - tempRect.Y * texHeightRatio;

				texCoordBR.X = (tempRect.X + tempRect.Width) * texWidthRatio;
				//texCoordBR.Y = tempRect.Y * texHeightRatio;
				texCoordBR.Y = 1.0f - (tempRect.Y + tempRect.Height) * texHeightRatio;


			} else {
				texCoordTL.X = texture.Image.GetTextureCoordX (tempRect.X);
				texCoordTL.Y = texture.Image.GetTextureCoordY (tempRect.Y);
				texCoordBR.X = texture.Image.GetTextureCoordX (tempRect.X + tempRect.Width);
				texCoordBR.Y = texture.Image.GetTextureCoordY (tempRect.Y + tempRect.Height);
			}

			item.Set (rectangle.X, 
					rectangle.Y, 
					rectangle.Width, 
					rectangle.Height, 
					color, 
					texCoordTL, 
					texCoordBR);
		}

		public void DrawString (SpriteFont spriteFont, string text, Vector2 position, Color color)
		{
			if (spriteFont == null)
				throw new ArgumentNullException ("spriteFont");

			spriteFont.DrawInto (
				this, text, position, color, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
		}

		public void DrawString (
			SpriteFont spriteFont, string text, Vector2 position, Color color,
			float rotation, Vector2 origin, float scale, SpriteEffects effects, float depth)
		{
			if (spriteFont == null)
				throw new ArgumentNullException ("spriteFont");

			var scaleVec = new Vector2 (scale, scale);
			spriteFont.DrawInto (this, text, position, color, rotation, origin, scaleVec, effects, depth);
		}

		public void DrawString (
			SpriteFont spriteFont, string text, Vector2 position, Color color,
			float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth)
		{
			if (spriteFont == null)
				throw new ArgumentNullException ("spriteFont");

			spriteFont.DrawInto (this, text, position, color, rotation, origin, scale, effect, depth);
		}

		public void DrawString (SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color)
		{
			if (spriteFont == null)
				throw new ArgumentNullException ("spriteFont");

			spriteFont.DrawInto (
				this, text, position, color, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
		}

		public void DrawString (
			SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color,
			float rotation, Vector2 origin, float scale, SpriteEffects effects, float depth)
		{
			if (spriteFont == null)
				throw new ArgumentNullException ("spriteFont");

			var scaleVec = new Vector2 (scale, scale);
			spriteFont.DrawInto (this, text, position, color, rotation, origin, scaleVec, effects, depth);
		}

		public void DrawString (
			SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color,
			float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth)
		{
			if (spriteFont == null)
				throw new ArgumentNullException ("spriteFont");

			spriteFont.DrawInto (this, text, position, color, rotation, origin, scale, effect, depth);
		}
	}
}
