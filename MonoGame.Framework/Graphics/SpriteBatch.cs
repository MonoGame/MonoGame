using System;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
	/// <summary>
    /// Class for drawing 2d graphical elements in a group of sprites.
	/// </summary>
	public class SpriteBatch : GraphicsResource
	{
		#region Private Fields

		readonly SpriteBatcher _batcher;
		bool _beginCalled;
		BlendState _blendState;
		DepthStencilState _depthStencilState;
		Effect _effect;
		Matrix _matrix;
		readonly EffectParameter _matrixTransform;
		RasterizerState _rasterizerState;
		SamplerState _samplerState;
		SpriteSortMode _sortMode;
		Effect _spriteEffect;
		readonly EffectPass _spritePass;
		Rectangle _tempRect = new Rectangle(0, 0, 0, 0);
		Vector2 _texCoordBR = new Vector2(0, 0);
		Vector2 _texCoordTL = new Vector2(0, 0);

		#endregion

		#region Constructors
 
		/// <summary>
        /// Creates a new instance of <see cref="SpriteBatch"/> class.
		/// </summary>
        /// <param name="graphicsDevice">The graphics device, which will be used for sprite rendering.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="graphicsDevice"/> is null.</exception>
		public SpriteBatch(GraphicsDevice graphicsDevice)
		{
			if (graphicsDevice == null)
			{
				throw new ArgumentNullException("graphicsDevice");
			}

			this.GraphicsDevice = graphicsDevice;

			// Use a custom SpriteEffect so we can control the transformation matrix
			_spriteEffect = new Effect(graphicsDevice, SpriteEffect.Bytecode);
			_matrixTransform = _spriteEffect.Parameters["MatrixTransform"];
			_spritePass = _spriteEffect.CurrentTechnique.Passes[0];

			_batcher = new SpriteBatcher(graphicsDevice);

			_beginCalled = false;
		}

		#endregion

		#region Public Methods

		#region Begin

        /// <summary>
        /// Begins batching and drawing sprites, using optional parameters.
        /// </summary>
        /// <param name="sortMode">Sorting mode.</param>
        /// <param name="blendState">State of the blending.</param>
        /// <param name="samplerState">State of the sampler.</param>
        /// <param name="depthStencilState">State of the depth-stencil buffer.</param>
        /// <param name="rasterizerState">State of the rasterization.</param>
        /// <param name="effect">Custom <see cref="Effect"/> for applying to pixel shader.</param>
        /// <param name="transformMatrix">Transformation matrix of position,rotation and scale.</param>
        /// <exception cref="InvalidOperationException">Thrown if Begin is called next time without previous End.</exception>
        public void Begin
        (
              SpriteSortMode sortMode = SpriteSortMode.Deferred,
              BlendState blendState = null,
              SamplerState samplerState = null,
              DepthStencilState depthStencilState = null,
              RasterizerState rasterizerState = null,
              Effect effect = null,
              Matrix? transformMatrix = null
        )
        {
            if (_beginCalled)
                throw new InvalidOperationException("Begin cannot be called again until End has been successfully called.");

            // defaults
            _sortMode = sortMode;
            _blendState = blendState ?? BlendState.AlphaBlend;
            _samplerState = samplerState ?? SamplerState.LinearClamp;
            _depthStencilState = depthStencilState ?? DepthStencilState.None;
            _rasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;
            _effect = effect;
            _matrix = transformMatrix ?? Matrix.Identity;

            // Setup things now so a user can change them.
            if (sortMode == SpriteSortMode.Immediate)
            {
                Setup();
            }

            _beginCalled = true;
        } 

		#endregion

		#region Draw

		/// <summary>
		/// Pushes a sprite to a batch of sprites using specified texture, position and color.
		/// </summary>
		/// <param name="texture">A texture.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="color">Mask color.</param>
		public void Draw(Texture2D texture, Vector2 position, Color color)
		{
			Draw(texture, position, null, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
		}

		/// <summary>
		/// Pushes a sprite to a batch of sprites using specified texture, destination and color.
		/// </summary>
		/// <param name="texture">A texture.</param>
		/// <param name="destinationRectangle">Drawing bounds.</param>
		/// <param name="color">Mask color.</param>
		public void Draw(Texture2D texture, Rectangle destinationRectangle, Color color)
		{
			Draw(texture, destinationRectangle, null, color, 0, Vector2.Zero, SpriteEffects.None, 0f);
		}

		/// <summary>
		/// Pushes a sprite to a batch of sprites using specified texture, position, source and color.
		/// </summary>
		/// <param name="texture">A texture.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
		/// <param name="color">Mask color.</param>
		public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color)
		{
			Draw(texture, position, sourceRectangle, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
		}

		/// <summary>
		/// Pushes a sprite to a batch of sprites using specified texture, destination, source and color.
		/// </summary>
		/// <param name="texture">A texture.</param>
		/// <param name="destinationRectangle">Drawing bounds.</param>
		/// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
		/// <param name="color">Mask color.</param>
		public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color)
		{
			Draw(texture, destinationRectangle, sourceRectangle, color, 0, Vector2.Zero, SpriteEffects.None, 0f);
		}

		/// <summary>
		/// Pushes a sprite to a batch of sprites using specified texture, position, source, color, rotation, origin, scale, effect and depth.
		/// </summary>
		/// <param name="texture">A texture.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
		/// <param name="color">Mask color.</param>
		/// <param name="rotation">A rotation of 2D sprite.</param>
		/// <param name="origin">Center of the rotation.</param>
		/// <param name="scale">A scaling of the sprite.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
		/// <param name="layerDepth">A depth.</param>
		public void Draw
		(
			Texture2D texture,
			Vector2 position,
			Rectangle? sourceRectangle,
			Color color,
			float rotation,
			Vector2 origin,
			Vector2 scale,
			SpriteEffects effects,
			float layerDepth
		)
		{
			CheckValid(texture);

			var w = texture.Width * scale.X;
			var h = texture.Height * scale.Y;
			if (sourceRectangle.HasValue)
			{
				w = sourceRectangle.Value.Width * scale.X;
				h = sourceRectangle.Value.Height * scale.Y;
			}

			DrawInternal(texture,
				new Vector4(position.X, position.Y, w, h),
				sourceRectangle,
				color,
				rotation,
				origin * scale,
                effects,
				layerDepth,
				true);
		}

		/// <summary>
		/// Pushes a sprite to a batch of sprites using specified texture, optional source, color, rotation, origin, scale, effect and depth.
		/// </summary>
		/// <param name="texture">A texture.</param>
		/// <param name="position">The drawing location on screen.</param>
		/// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
		/// <param name="color">Mask color.</param>
		/// <param name="rotation">A rotation of sprite.</param>
		/// <param name="origin">Center of the rotation.</param>
		/// <param name="scale">A scaling of the sprite.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
		/// <param name="layerDepth">A depth.</param>
		public void Draw
		(
			Texture2D texture,
			Vector2 position,
			Rectangle? sourceRectangle,
			Color color,
			float rotation,
			Vector2 origin,
			float scale,
			SpriteEffects effects,
			float layerDepth
		)
		{
			CheckValid(texture);

			var w = texture.Width * scale;
			var h = texture.Height * scale;
			if (sourceRectangle.HasValue)
			{
				w = sourceRectangle.Value.Width * scale;
				h = sourceRectangle.Value.Height * scale;
			}

			DrawInternal(texture,
				new Vector4(position.X, position.Y, w, h),
				sourceRectangle,
				color,
				rotation,
				origin * scale,
				effects,
				layerDepth,
				true);
		}

		/// <summary>
		/// Pushes a sprite to a batch of sprites using specified texture, destination, optional source, color, rotation, origin, scale, effect and depth.
		/// </summary>
		/// <param name="texture">A texture.</param>
		/// <param name="destinationRectangle">Drawing bounds.</param>
		/// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
		/// <param name="color">Mask color.</param>
		/// <param name="rotation">A rotation of sprite.</param>
		/// <param name="origin">Center of the rotation.</param>
		/// <param name="effects">Modificators for drawing. Can be combined.</param>
		/// <param name="layerDepth">A depth.</param>
		public void Draw
		(
			Texture2D texture,
			Rectangle destinationRectangle,
			Rectangle? sourceRectangle,
			Color color,
			float rotation,
			Vector2 origin,
			SpriteEffects effects,
			float layerDepth
		)
		{
			CheckValid(texture);

			DrawInternal(texture,
				  new Vector4(destinationRectangle.X,
							  destinationRectangle.Y,
							  destinationRectangle.Width,
							  destinationRectangle.Height),
				  sourceRectangle,
				  color,
				  rotation,
				  new Vector2(origin.X * ((float)destinationRectangle.Width / (float)((sourceRectangle.HasValue && sourceRectangle.Value.Width != 0) ? sourceRectangle.Value.Width : texture.Width)),
									origin.Y * ((float)destinationRectangle.Height) / (float)((sourceRectangle.HasValue && sourceRectangle.Value.Height != 0) ? sourceRectangle.Value.Height : texture.Height)),
				  effects,
				  layerDepth,
				  true);
		}


		/// <summary>
		/// Pushes a sprite to a batch of sprites, using optional parameters.
		/// </summary>
		/// <param name="texture">A texture.</param>
		/// <param name="position">An optional drawing location on screen.</param>
		/// <param name="drawRectangle">An optional drawing bounds.</param>
		/// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
		/// <param name="origin">An optional center of rotation. If null - <see cref="Vector2.Zero"/> used.</param>
		/// <param name="rotation">An optional rotation of sprite. 0 by default.</param>
		/// <param name="scale">An optional scale vector. If null - <see cref="Vector2.One"/> used.</param>
		/// <param name="color">An optional color mask. If null - <see cref="Color.White"/> used.</param>
		/// <param name="effects">The optional drawing modificators. Uses <see cref="SpriteEffects.None"/> by default.</param>
		/// <param name="layerDepth">An optional depth. 0 by default.</param>
		/// <exception cref="InvalidOperationException">Throwns if both <paramref name="position"/> and <paramref name="drawRectangle"/> been used.</exception>
		/// <remarks>This overload requires only one of <paramref name="position"/> and <paramref name="drawRectangle"/> been used.</remarks>
		public void Draw
		(
		   Texture2D texture,
		   Vector2? position = null,
		   Rectangle? drawRectangle = null, 
		   Rectangle? sourceRectangle = null,
		   Vector2? origin = null,
		   float rotation = 0f,
		   Vector2? scale = null,
		   Color? color = null,
		   SpriteEffects effects = SpriteEffects.None,
		   float layerDepth = 0f
		)
		{

			// Assign default values to null parameters here, as they are not compile-time constants
			if (!color.HasValue)
				color = Color.White;
			if (!origin.HasValue)
				origin = Vector2.Zero;
			if (!scale.HasValue)
				scale = Vector2.One;

			// If both drawRectangle and position are null, or if both have been assigned a value, raise an error
			if ((drawRectangle.HasValue) == (position.HasValue))
			{
				throw new InvalidOperationException("Expected drawRectangle or position, but received neither or both.");
			}

		    if (position != null)
		    {
		        // Call Draw() using position
		        Draw(texture, (Vector2)position, sourceRectangle, (Color)color, rotation, (Vector2)origin, (Vector2)scale, effects, layerDepth);
		    }
		    else
		    {
		        // Call Draw() using drawRectangle
		        Draw(texture, (Rectangle)drawRectangle, sourceRectangle, (Color)color, rotation, (Vector2)origin, effects, layerDepth);
		    }
		}

		#endregion

		#region DrawString

		/// <summary>
        /// Pushes a string to a batch of sprites to be rendered.
		/// </summary>
		/// <param name="spriteFont">The specific <see cref="SpriteFont"/> for displaying text.</param>
		/// <param name="text">A text which will be drawn.</param>
		/// <param name="position">A position on screen.</param>
		/// <param name="color">Foreground color.</param>
		public void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color)
		{
			CheckValid(spriteFont, text);

			var source = new SpriteFont.CharacterSource(text);
			spriteFont.DrawInto(this, ref source, position, color, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
		}

        /// <summary>
        /// Pushes a string to a batch of sprites to be rendered.
        /// </summary>
        /// <param name="spriteFont">The specific <see cref="SpriteFont"/> for displaying text.</param>
        /// <param name="text">A text which will be drawn.</param>
        /// <param name="position">A position on screen.</param>
        /// <param name="color">Foreground color.</param>
		public void DrawString(SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color)
		{
			CheckValid(spriteFont, text);

			var source = new SpriteFont.CharacterSource(text);
			spriteFont.DrawInto(this, ref source, position, color, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
		}

        /// <summary>
        /// Pushes a string to a batch of sprites to be rendered.
        /// </summary>
        /// <param name="spriteFont">The specific <see cref="SpriteFont"/> for displaying text.</param>
        /// <param name="text">A text which will be drawn.</param>
        /// <param name="position">A position on screen.</param>
        /// <param name="color">Foreground color.</param>
        /// <param name="rotation">A rotation of text.</param>
        /// <param name="origin">Center of the text rotation.</param>
        /// <param name="scale">A scaling factor.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth.</param>
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
            float layerDepth
		)
		{
			CheckValid(spriteFont, text);

			var scaleVec = new Vector2(scale, scale);
			var source = new SpriteFont.CharacterSource(text);
            spriteFont.DrawInto(this, ref source, position, color, rotation, origin, scaleVec, effects, layerDepth);
		}

        /// <summary>
        /// Pushes a string to a batch of sprites to be rendered.
        /// </summary>
        /// <param name="spriteFont">The specific <see cref="SpriteFont"/> for displaying text.</param>
        /// <param name="text">A text which will be drawn.</param>
        /// <param name="position">A position on screen.</param>
        /// <param name="color">Foreground color.</param>
        /// <param name="rotation">A rotation of text.</param>
        /// <param name="origin">Center of the text rotation.</param>
        /// <param name="scale">A scaling factor.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth.</param>
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
            float layerDepth
		)
		{
			CheckValid(spriteFont, text);

			var source = new SpriteFont.CharacterSource(text);
            spriteFont.DrawInto(this, ref source, position, color, rotation, origin, scale, effects, layerDepth);
		}

        /// <summary>
        /// Pushes a string to a batch of sprites to be rendered.
        /// </summary>
        /// <param name="spriteFont">The specific <see cref="SpriteFont"/> for displaying text.</param>
        /// <param name="text">A text which will be drawn.</param>
        /// <param name="position">A position on screen.</param>
        /// <param name="color">Foreground color.</param>
        /// <param name="rotation">A rotation of text.</param>
        /// <param name="origin">Center of the text rotation.</param>
        /// <param name="scale">A scaling factor.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth.</param>
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
            float layerDepth
		)
		{
			CheckValid(spriteFont, text);

			var source = new SpriteFont.CharacterSource(text);
            spriteFont.DrawInto(this, ref source, position, color, rotation, origin, new Vector2(scale, scale), effects, layerDepth);
		}

        /// <summary>
        /// Pushes a string to a batch of sprites to be rendered.
        /// </summary>
        /// <param name="spriteFont">The specific <see cref="SpriteFont"/> for displaying text.</param>
        /// <param name="text">A text which will be drawn.</param>
        /// <param name="position">A position on screen.</param>
        /// <param name="color">Foreground color.</param>
        /// <param name="rotation">A rotation of text.</param>
        /// <param name="origin">Center of the text rotation.</param>
        /// <param name="scale">A scaling factor.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth.</param>
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
            float layerDepth
		)
		{
			CheckValid(spriteFont, text);

			var source = new SpriteFont.CharacterSource(text);
            spriteFont.DrawInto(this, ref source, position, color, rotation, origin, scale, effects, layerDepth);
		}

		#endregion

		/// <summary>
		/// Flushes the sprite batches to a screen and restores device states.
		/// </summary>
		public void End()
		{
			_beginCalled = false;

		    if (_sortMode != SpriteSortMode.Immediate)
		    {
		        Setup();
		    }
#if PSM   
            GraphicsDevice.BlendState = _blendState;
            _blendState.PlatformApplyState(GraphicsDevice);
#endif

			_batcher.DrawBatch(_sortMode, _effect);
		}

		#endregion

		#region Internal Methods

		internal void DrawInternal
		(
			Texture2D texture,
			Vector4 destinationRectangle,
			Rectangle? sourceRectangle,
			Color color,
			float rotation,
			Vector2 origin,
			SpriteEffects effect,
			float layerDepth,
			bool autoFlush
		)
		{
			var item = _batcher.CreateBatchItem();

			item.Depth = layerDepth;
			item.Texture = texture;

			if (sourceRectangle.HasValue)
			{
				_tempRect = sourceRectangle.Value;
			}
			else
			{
				_tempRect.X = 0;
				_tempRect.Y = 0;
				_tempRect.Width = texture.Width;
				_tempRect.Height = texture.Height;
			}

			_texCoordTL.X = _tempRect.X / (float)texture.Width;
			_texCoordTL.Y = _tempRect.Y / (float)texture.Height;
			_texCoordBR.X = (_tempRect.X + _tempRect.Width) / (float)texture.Width;
			_texCoordBR.Y = (_tempRect.Y + _tempRect.Height) / (float)texture.Height;

			if ((effect & SpriteEffects.FlipVertically) != 0)
			{
				var temp = _texCoordBR.Y;
				_texCoordBR.Y = _texCoordTL.Y;
				_texCoordTL.Y = temp;
			}
			if ((effect & SpriteEffects.FlipHorizontally) != 0)
			{
				var temp = _texCoordBR.X;
				_texCoordBR.X = _texCoordTL.X;
				_texCoordTL.X = temp;
			}

			item.Set(destinationRectangle.X,
					destinationRectangle.Y,
					-origin.X,
					-origin.Y,
					destinationRectangle.Z,
					destinationRectangle.W,
					(float)Math.Sin(rotation),
					(float)Math.Cos(rotation),
					color,
					_texCoordTL,
					_texCoordBR);

			if (autoFlush)
			{
				FlushIfNeeded();
			}
		}

		// Mark the end of a draw operation for Immediate SpriteSortMode.
		internal void FlushIfNeeded()
		{
			if (_sortMode == SpriteSortMode.Immediate)
			{
				_batcher.DrawBatch(_sortMode, _effect);
			}
		}

		#endregion

		#region Protected Methods

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					if (_spriteEffect != null)
					{
						_spriteEffect.Dispose();
						_spriteEffect = null;
					}
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Private Methods

		private void CheckValid(Texture2D texture)
		{
			if (texture == null)
				throw new ArgumentNullException("texture");
			if (!_beginCalled)
				throw new InvalidOperationException("Draw was called, but Begin has not yet been called. Begin must be called successfully before you can call Draw.");
		}

		private void CheckValid(SpriteFont spriteFont, string text)
		{
			if (spriteFont == null)
				throw new ArgumentNullException("spriteFont");
			if (text == null)
				throw new ArgumentNullException("text");
			if (!_beginCalled)
				throw new InvalidOperationException("DrawString was called, but Begin has not yet been called. Begin must be called successfully before you can call DrawString.");
		}

		private void CheckValid(SpriteFont spriteFont, StringBuilder text)
		{
			if (spriteFont == null)
				throw new ArgumentNullException("spriteFont");
			if (text == null)
				throw new ArgumentNullException("text");
			if (!_beginCalled)
				throw new InvalidOperationException("DrawString was called, but Begin has not yet been called. Begin must be called successfully before you can call DrawString.");
		}

		private void Setup()
		{
			var gd = GraphicsDevice;
			gd.BlendState = _blendState;
			gd.DepthStencilState = _depthStencilState;
			gd.RasterizerState = _rasterizerState;
			gd.SamplerStates[0] = _samplerState;

			// Setup the default sprite effect.
			var vp = gd.Viewport;

			Matrix projection;
#if PSM || DIRECTX
			Matrix.CreateOrthographicOffCenter(0, vp.Width, vp.Height, 0, -1, 0, out projection);
#else
            // GL requires a half pixel offset to match DX.
            Matrix.CreateOrthographicOffCenter(0, vp.Width, vp.Height, 0, 0, 1, out projection);
            projection.M41 += -0.5f * projection.M11;
            projection.M42 += -0.5f * projection.M22;
#endif
			Matrix.Multiply(ref _matrix, ref projection, out projection);

			_matrixTransform.SetValue(projection);
			_spritePass.Apply();
		}

		#endregion
	}
}