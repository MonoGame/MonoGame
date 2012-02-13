using System;
using System.Text;
using System.Collections.Generic;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS
using OpenTK.Graphics.OpenGL;
#else
#if ES11
using OpenTK.Graphics.ES11;
using MatrixMode = OpenTK.Graphics.ES11.All;
#else
using OpenTK.Graphics.ES20;
#endif
#endif

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
#if !ES11
		Effect spriteEffect;
#endif
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

            // Use a custom SpriteEffect so we can control the transformation matrix
            spriteEffect = new Effect(this.graphicsDevice, Effect.LoadEffectResource("SpriteEffect"));
            
            _batcher = new SpriteBatcher();
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

			_effect = effect;
			
			_matrix = transformMatrix;
			
			
			if (sortMode == SpriteSortMode.Immediate) {
				//setup things now so a user can chage them
				Setup();
			}
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
			if (_sortMode != SpriteSortMode.Immediate) {
				Setup ();
			}
			Flush ();
			
			// clear out the textures
			graphicsDevice.Textures._textures.Clear ();
			
#if !ES11
			// unbinds shader
			if (_effect != null) {
				GL.UseProgram (0);
				_effect = null;
			}
#endif

		}
		
		void Setup () {
			graphicsDevice.BlendState = _blendState;
			graphicsDevice.DepthStencilState = _depthStencilState;
			graphicsDevice.RasterizerState = _rasterizerState;
			graphicsDevice.SamplerStates[0] = _samplerState;
			
#if ES11
			// set camera
			GL.MatrixMode (MatrixMode.Projection);
			GL.LoadIdentity ();		
			
			// Switch on the flags.
 #if ANDROID
	        switch (this.graphicsDevice.PresentationParameters.DisplayOrientation)
	        {
			
				case DisplayOrientation.LandscapeRight:
                {
					GL.Rotate(180, 0, 0, 1); 
					GL.Ortho(0, this.graphicsDevice.Viewport.Width, this.graphicsDevice.Viewport.Height,  0, -1, 1);
					break;
				}
				
				case DisplayOrientation.LandscapeLeft:
				case DisplayOrientation.PortraitUpsideDown:
                default:
				{
					GL.Ortho(0, this.graphicsDevice.Viewport.Width, this.graphicsDevice.Viewport.Height, 0, -1, 1);
					break;
				}
			}
 #else
			GL.Ortho(0, this.graphicsDevice.Viewport.Width, this.graphicsDevice.Viewport.Height, 0, -1, 1);
 #endif
			
			
			//These needed?
			GL.MatrixMode(MatrixMode.Modelview);
			GL.Viewport (graphicsDevice.Viewport.X, graphicsDevice.Viewport.Y,
			             graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);
			GL.LoadMatrix (Matrix.ToFloatArray(_matrix));

#else
			if (_effect == null) {
				Viewport vp = graphicsDevice.Viewport;
				Matrix projection = Matrix.CreateOrthographicOffCenter(0, vp.Width, vp.Height, 0, 0, 1);
				Matrix halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);
				Matrix transform = _matrix * (halfPixelOffset * projection);
				spriteEffect.Parameters["MatrixTransform"].SetValue (transform);
				
				spriteEffect.CurrentTechnique.Passes[0].Apply();
			} else {
				// apply the custom effect if there is one
				_effect.CurrentTechnique.Passes[0].Apply ();
			}
#endif
		}
		
		void Flush() {
			_batcher.DrawBatch (_sortMode, graphicsDevice.SamplerStates[0]);

		}

		public void Draw (Texture2D texture,
				Vector2 position,
				Rectangle? sourceRectangle,
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
			float w = texture.Width*scale.X;
			float h = texture.Height*scale.Y;
			if (sourceRectangle.HasValue) {
				w = sourceRectangle.Value.Width*scale.X;
				h = sourceRectangle.Value.Height*scale.Y;
			}
			Draw (texture,
				new Rectangle((int)position.X, (int)position.Y, (int)w, (int)h),
				sourceRectangle,
				color,
				rotation,
				origin * scale,
				effect,
				depth);
		}

		public void Draw (Texture2D texture,
				Vector2 position,
				Rectangle? sourceRectangle,
				Color color,
				float rotation,
				Vector2 origin,
				float scale,
				SpriteEffects effect,
				float depth)
		{
			Draw (texture,
				position,
				sourceRectangle,
				color,
				rotation,
				origin,
				new Vector2(scale, scale),
				effect,
				depth);
		}

		public void Draw (Texture2D texture,
			Rectangle destinationRectangle,
			Rectangle? sourceRectangle,
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
			item.TextureID = texture.glTexture;

			if (sourceRectangle.HasValue) {
				tempRect = sourceRectangle.Value;
			} else {
				tempRect.X = 0;
				tempRect.Y = 0;
				tempRect.Width = texture.Width;
				tempRect.Height = texture.Height;				
			}
			
			texCoordTL.X = tempRect.X / (float)texture.Width;
			texCoordTL.Y = tempRect.Y / (float)texture.Height;
			texCoordBR.X = (tempRect.X + tempRect.Width) / (float)texture.Width;
			texCoordBR.Y = (tempRect.Y + tempRect.Height) / (float)texture.Height;

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
			
			if (_sortMode == SpriteSortMode.Immediate) {
				Flush ();
			}
			
		}

		public void Draw (Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color)
		{
			Draw (texture, position, sourceRectangle, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
		}

		public void Draw (Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color)
		{
			Draw (texture, destinationRectangle, sourceRectangle, color, 0, Vector2.Zero, SpriteEffects.None, 0f);
		}

		public void Draw (Texture2D texture,
				Vector2 position,
				Color color)
		{
			Draw (texture, position, null, color);
		}

		public void Draw (Texture2D texture, Rectangle rectangle, Color color)
		{
			Draw (texture, rectangle, null, color);
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

