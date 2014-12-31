using System;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Helper class for drawing text strings and sprites in one or more optimized batches.
    /// </summary>
	public class SpriteBatch : GraphicsResource
	{
	    readonly SpriteBatcher _batcher;

		SpriteSortMode _sortMode;
		BlendState _blendState;
		SamplerState _samplerState;
		DepthStencilState _depthStencilState; 
		RasterizerState _rasterizerState;		
		Effect _effect;
        bool _beginCalled;

		Effect _spriteEffect;
	    readonly EffectParameter _matrixTransform;
        readonly EffectPass _spritePass;

		Matrix _matrix;
		Rectangle _tempRect = new Rectangle (0,0,0,0);
		Vector2 _texCoordTL = new Vector2 (0,0);
		Vector2 _texCoordBR = new Vector2 (0,0);

        /// <summary>
        /// Creates a new instance of <see cref="SpriteBatch"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/>, which will be used for sprite rendering.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="graphicsDevice"/> is null.</exception>
		public SpriteBatch (GraphicsDevice graphicsDevice)
		{
			if (graphicsDevice == null) {
				throw new ArgumentException ("graphicsDevice");
			}	

			this.GraphicsDevice = graphicsDevice;

            // Use a custom SpriteEffect so we can control the transformation matrix
            _spriteEffect = new Effect(graphicsDevice, SpriteEffect.Bytecode);
            _matrixTransform = _spriteEffect.Parameters["MatrixTransform"];
            _spritePass = _spriteEffect.CurrentTechnique.Passes[0];

            _batcher = new SpriteBatcher(graphicsDevice);

            _beginCalled = false;
		}

        /// <summary>
        /// Begins a new sprite and text batch with the specified render state.
        /// </summary>
        /// <param name="sortMode">The drawing order for sprite and text drawing.</param>
        /// <param name="blendState">State of the blending.</param>
        /// <param name="samplerState">State of the sampler.</param>
        /// <param name="depthStencilState">State of the depth-stencil buffer.</param>
        /// <param name="rasterizerState">State of the rasterization.</param>
        /// <param name="effect">A custom <see cref="Effect"/> to override the default sprite effect.</param>
        /// <param name="transformMatrix">An optional matrix used to transform the sprite geometry.</param>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="Begin"/> is called next time without previous <see cref="End"/>.</exception>
        /// <remarks>This method uses optional parameters.</remarks>
        /// <remarks>The <see cref="Begin"/> Begin should be called before drawing commands, and you cannot call it again before subsequent <see cref="End"/>.</remarks>
        /// <example>
        /// Simple way of use Begin is.
        /// <code>
        /// spriteBatch.Begin();
        /// -Draw commands-
        /// spriteBatch.End();
        /// </code>
        /// </example>
        /// <example>
        /// The <see cref="SpriteSortMode"/> is control in which order sprites will be rendered. The example below will draw red sprite first, regardless the order they are declared in code.
        /// The order(or layerDepth parameter of drawing command) is ignored in case of <see cref="SpriteSortMode.Deferred"/>, <see cref="SpriteSortMode.Immediate"/> or <see cref="SpriteSortMode.Texture"/>.
        /// The <see cref="SpriteSortMode.Deferred"/> is simply draw sprites in <see cref="End"/> in order the drawing commands invoked.
        /// If you use <see cref="SpriteSortMode.Immediate"/> the sprites will be rendered immediately when drawing command invoked, instead awaiting for the <see cref="End"/> call. 
        /// The <see cref="SpriteSortMode.Texture"/> is tricky - its same as <see cref="SpriteSortMode.Deferred"/>(so it does not affect by layerDepth) but using texture comparsion instead.
        /// So, if you draws same texture several times and another texture several times - there will be two layers for different textures each one have drawing in order the drawing commands invoked.
        /// <code>
        /// spriteBatch.Begin(sortMode: SpriteSortMode.BackToFront);
        /// spriteBatch.Draw(texture, new Vector2(20, 0), null, Color.Red, 0, new Vector2(), 1, SpriteEffects.None, 0.0f);
        /// spriteBatch.Draw(texture, new Vector2(0,0),  null, Color.White, 0, new Vector2(), 1, SpriteEffects.None, 1.0f);
        /// spriteBatch.End();
        /// </code>
        /// </example>
        /// <example>
        /// The <see cref="BlendState"/> is controlling the blending state.
        /// If you want to batch will be rendered with alpha channel you could use <see cref="BlendState.AlphaBlend"/>. Note : make sure that your texture is predefine color data if your texture have semi-transparent regions. 
        /// If you are not use <see cref="BlendState.NonPremultiplied"/> (its slower than <see cref="BlendState.AlphaBlend"/> but perfect for prototyping).
        /// You could use your own blending, instead using predefined one. If you are not defines blending the <see cref="BlendState.Opaque"/> will be used and it can be not desired result if you draws transparent sprites or strings.
        /// <code>
        /// spriteBatch.Begin(blendState: BlendState.AlphaBlend);
        /// -Draw the texture with alpha channel-
        /// spriteBatch.End();
        /// </code>
        /// </example>
        /// <example> 
        /// The <see cref="SamplerState"/> of <param name="samplerState"> is a texture sampler which will be applied to the results of drawing.</param>
        /// Sampler states is used for texture blurring and repeating. Point sampling is used for mimic old games, whereas linear or anisotropic is often used for modern games.
        /// Wrap is uses for create repeating sprite patterns when sourceRectangle parameter of drawing commands is bigger than actual texture size, whereas Clamp strict texture in a single bound.
        /// You could use your own blending, instead using predefined one. If you are not defines blending the <see cref="SamplerState.LinearClamp"/> is used.
        /// The example below uses 300x300 texture. This will draw 4 samples of one texture in one draw call. Thats because Wrap used for acquire this effect.
        /// <code>
        /// spriteBatch.Begin(samplerState: SamplerState.LinearWrap);
        /// spriteBatch.Draw(texture, new Rectangle(0, 0, 300, 300), new Rectangle(0, 0, 300 * 2, 300 * 2), Color.White);
        /// spriteBatch.End();
        /// </code>
        /// </example>
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

        /// <summary>
        /// Flushes all batched text and sprites to the screen.
        /// </summary>
        /// <remarks>This command should be called after <see cref="Begin"/> and drawing commands.</remarks>
		/// <example>
		/// <code>
		/// spriteBatch.Begin();
		/// -Draw commands-
        /// spriteBatch.End();
		/// </code>
		/// </example>
        public void End ()
		{	
			_beginCalled = false;

			if (_sortMode != SpriteSortMode.Immediate)
				Setup();
#if PSM   
            GraphicsDevice.BlendState = _blendState;
            _blendState.PlatformApplyState(GraphicsDevice);
#endif
            
            _batcher.DrawBatch(_sortMode, _effect);
        }
		
		void Setup() 
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
		
        void CheckValid(Texture2D texture)
        {
            if (texture == null)
                throw new ArgumentNullException("texture");
            if (!_beginCalled)
                throw new InvalidOperationException("Draw was called, but Begin has not yet been called. Begin must be called successfully before you can call Draw.");
        }

        void CheckValid(SpriteFont spriteFont, string text)
        {
            if (spriteFont == null)
                throw new ArgumentNullException("spriteFont");
            if (text == null)
                throw new ArgumentNullException("text");
            if (!_beginCalled)
                throw new InvalidOperationException("DrawString was called, but Begin has not yet been called. Begin must be called successfully before you can call DrawString.");
        }

        void CheckValid(SpriteFont spriteFont, StringBuilder text)
        {
            if (spriteFont == null)
                throw new ArgumentNullException("spriteFont");
            if (text == null)
                throw new ArgumentNullException("text");
            if (!_beginCalled)
                throw new InvalidOperationException("DrawString was called, but Begin has not yet been called. Begin must be called successfully before you can call DrawString.");
        }

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="position">An optional drawing location on screen. If null - <param name="destinationRectangle"> using intended.</param></param>
        /// <param name="destinationRectangle">An optional drawing bound. If null - <param name="position"> using intended.</param></param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="origin">An optional center of rotation. Uses <see cref="Vector2.Zero"/> if null.</param>
        /// <param name="rotation">An optional rotation of this sprite. 0 by default.</param>
        /// <param name="scale">An optional scale vector. Uses <see cref="Vector2.One"/> if null.</param>
        /// <param name="color">An optional color mask. Uses <see cref="Color.White"/> if null.</param>
        /// <param name="effects">The optional drawing modificators. <see cref="SpriteEffects.None"/> by default.</param>
        /// <param name="layerDepth">An optional depth of the layer of this sprite. 0 by default.</param>
        /// <exception cref="InvalidOperationException">Throwns if both <paramref name="position"/> and <paramref name="destinationRectangle"/> been used.</exception>
        /// <remarks>This overload uses optional parameters.</remarks>
        /// <remarks>This overload requires only one of <paramref name="position"/> and <paramref name="destinationRectangle"/> been used.</remarks>
        /// <example>
        /// The standart usage of this overload is for skip unnecessary parameters usage. If you want, for example draws texture in 
        /// specified position and with flip effect you can use it like this.
        /// <code>
        /// spriteBatch.Begin();
        /// spriteBatch.Draw(texture, position: new Vector2(100,100), effects: SpriteEffects.FlipVertically);
        /// spriteBatch.End();
        /// </code>
        /// </example>
        public void Draw (Texture2D texture,
                Vector2? position = null,
                Rectangle? destinationRectangle = null,
                Rectangle? sourceRectangle = null,
                Vector2? origin = null,
                float rotation = 0f,
                Vector2? scale = null,
                Color? color = null,
                SpriteEffects effects = SpriteEffects.None,
                float layerDepth = 0f)
        {

            // Assign default values to null parameters here, as they are not compile-time constants
            if(!color.HasValue)
                color = Color.White;
            if(!origin.HasValue)
                origin = Vector2.Zero;
            if(!scale.HasValue)
                scale = Vector2.One;

            // If both drawRectangle and position are null, or if both have been assigned a value, raise an error
            if((destinationRectangle.HasValue) == (position.HasValue))
            {
                throw new InvalidOperationException("Expected drawRectangle or position, but received neither or both.");
            }
            else if(position != null)
            {
                // Call Draw() using position
                Draw(texture, (Vector2)position, sourceRectangle, (Color)color, rotation, (Vector2)origin, (Vector2)scale, effects, layerDepth);
            }
            else
            {
                // Call Draw() using drawRectangle
                Draw(texture, (Rectangle)destinationRectangle, sourceRectangle, (Color)color, rotation, (Vector2)origin, effects, layerDepth);
            }
        }

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this sprite.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this sprite.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this sprite.</param>
        /// <example>
        /// In this example we submit a sprite, using this overload.
        /// <code>
        /// spriteBatch.Begin();
        /// spriteBatch.Draw(texture, new Vector2(), null, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, .0f);
        /// spriteBatch.End(); 
        /// </code>
        /// </example>
		public void Draw (Texture2D texture,
				Vector2 position,
				Rectangle? sourceRectangle,
				Color color,
				float rotation,
				Vector2 origin,
				Vector2 scale,
				SpriteEffects effects,
                float layerDepth)
		{
            CheckValid(texture);

            var w = texture.Width * scale.X;
            var h = texture.Height * scale.Y;
			if (sourceRectangle.HasValue)
            {
				w = sourceRectangle.Value.Width*scale.X;
				h = sourceRectangle.Value.Height*scale.Y;
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
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this sprite.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this sprite.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this sprite.</param>
        /// <example>
        /// In this example we submit a sprite, using this overload.
        /// <code>
        /// spriteBatch.Begin();
        /// spriteBatch.Draw(texture, new Vector2(), null, Color.White, 0, Vector2.Zero, 1.0f, SpriteEffects.None, .0f);
        /// spriteBatch.End(); 
        /// </code>
        /// </example>
		public void Draw (Texture2D texture,
				Vector2 position,
				Rectangle? sourceRectangle,
				Color color,
				float rotation,
				Vector2 origin,
				float scale,
				SpriteEffects effects,
                float layerDepth)
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
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="destinationRectangle">The drawing bounds.</param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this sprite.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this sprite.</param>
        /// <example>
        /// In this example we submit a sprite, using this overload.
        /// <code>
        /// spriteBatch.Begin();
        /// spriteBatch.Draw(texture, new Rectangle(0, 0, texture.Width, texture.Height), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, .0f);
        /// spriteBatch.End(); 
        /// </code>
        /// </example>
		public void Draw (Texture2D texture,
			Rectangle destinationRectangle,
			Rectangle? sourceRectangle,
			Color color,
			float rotation,
			Vector2 origin,
			SpriteEffects effects,
            float layerDepth)
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
			      new Vector2(origin.X * ((float)destinationRectangle.Width / (float)( (sourceRectangle.HasValue && sourceRectangle.Value.Width != 0) ? sourceRectangle.Value.Width : texture.Width)),
                        			origin.Y * ((float)destinationRectangle.Height) / (float)( (sourceRectangle.HasValue && sourceRectangle.Value.Height != 0) ? sourceRectangle.Value.Height : texture.Height)),
			      effects,
                  layerDepth,
			      true);
		}

		internal void DrawInternal (Texture2D texture,
			Vector4 destinationRectangle,
			Rectangle? sourceRectangle,
			Color color,
			float rotation,
			Vector2 origin,
			SpriteEffects effect,
			float depth,
			bool autoFlush)
		{
			var item = _batcher.CreateBatchItem();

			item.Depth = depth;
			item.Texture = texture;

			if (sourceRectangle.HasValue) {
				_tempRect = sourceRectangle.Value;
			} else {
				_tempRect.X = 0;
				_tempRect.Y = 0;
				_tempRect.Width = texture.Width;
				_tempRect.Height = texture.Height;				
			}
			
			_texCoordTL.X = _tempRect.X / (float)texture.Width;
			_texCoordTL.Y = _tempRect.Y / (float)texture.Height;
			_texCoordBR.X = (_tempRect.X + _tempRect.Width) / (float)texture.Width;
			_texCoordBR.Y = (_tempRect.Y + _tempRect.Height) / (float)texture.Height;

			if ((effect & SpriteEffects.FlipVertically) != 0) {
                var temp = _texCoordBR.Y;
				_texCoordBR.Y = _texCoordTL.Y;
				_texCoordTL.Y = temp;
			}
			if ((effect & SpriteEffects.FlipHorizontally) != 0) {
                var temp = _texCoordBR.X;
				_texCoordBR.X = _texCoordTL.X;
				_texCoordTL.X = temp;
			}

			item.Set (destinationRectangle.X,
					destinationRectangle.Y, 
					-origin.X, 
					-origin.Y,
					destinationRectangle.Z,
					destinationRectangle.W,
					(float)Math.Sin (rotation), 
					(float)Math.Cos (rotation), 
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

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="color">A color mask.</param>
        /// <example>
        /// In this example we submit a sprite, full texture will be rendered, using this overload.
        /// <code>
        /// spriteBatch.Begin();
        /// spriteBatch.Draw(texture, Vector2.Zero, null, Color.White);
        /// spriteBatch.End(); 
        /// </code>
        /// </example>
        /// <example>
        /// In this example we submit a sprite with specified source, the result will be half-cutted sprite, using this overload.
        /// <code>
        /// spriteBatch.Begin();
        /// spriteBatch.Draw(texture, Vector2.Zero, new Rectangle(0, 0, texture.Width / 2, texture.Height / 2), Color.White);
        /// spriteBatch.End();
        /// </code>
        /// </example
		public void Draw (Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color)
		{
			Draw (texture, position, sourceRectangle, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
		}

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="destinationRectangle">The drawing bounds.</param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="color">A color mask.</param>
        /// <example>
        /// In this example we submit a sprite using texture bounds, full texture will be rendered, using this overload.
        /// <code>
        /// spriteBatch.Begin();
        /// spriteBatch.Draw(texture, new Rectangle(0,0, texture.Width, texture.Height), null, Color.White);
        /// spriteBatch.End();
        /// </code>
        /// </example>
        /// <example>
        /// In this example we submit a sprite using texture bounds and source, the result will be half-cutted sprite, using this overload.
        /// <code>
        /// spriteBatch.Begin();
        /// spriteBatch.Draw(texture, new Rectangle(0, 0, texture.Width, texture.Height), new Rectangle(0, 0, texture.Width / 2, texture.Height / 2), Color.White);
        /// spriteBatch.End();
        /// </code>
        /// </example>
		public void Draw (Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color)
		{
			Draw (texture, destinationRectangle, sourceRectangle, color, 0, Vector2.Zero, SpriteEffects.None, 0f);
		}

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        /// <example>
        /// In this example we submit a sprite using default position and white color mask, using this overload.
        /// <code>
        /// spriteBatch.Begin();
        /// spriteBatch.Draw(texture, Vector2.Zero, Color.White);
        /// spriteBatch.End();
        /// </code>
        /// </example>
		public void Draw (Texture2D texture, Vector2 position, Color color)
		{
			Draw (texture, position, null, color);
		}

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="destinationRectangle">The drawing bounds.</param>
        /// <param name="color">A color mask.</param>
        /// <example>
        /// In this example we submit a sprite using texture bounds, using this overload.
        /// <code>
        /// spriteBatch.Begin();
        /// spriteBatch.Draw(texture, new Rectangle(0, 0, texture.Width, texture.Height), Color.White);
        /// spriteBatch.End();
        /// </code>
        /// </example>
        public void Draw(Texture2D texture, Rectangle destinationRectangle, Color color)
		{
            Draw(texture, destinationRectangle, null, color);
		}

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="spriteFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        /// <example>
        /// In this example we simply draw a string, using this overload.
        /// <code>
        /// spriteBatch.Begin();
        /// spriteBatch.DrawString(font, "Hello MonoGame !", Vector2.Zero, Color.White);
        /// spriteBatch.End();
        /// </code>  
        /// </example>
        /// <example>
        /// In this example we draw a string with a custom color with transparency, at x=100 y=100, using this overload.
        /// <code>
        /// spriteBatch.Begin(blendState: BlendState.NonPremultiplied);
        /// spriteBatch.DrawString(font, "Hello MonoGame !", new Vector2(100, 100), new Color(1,0,0,.25f));
        /// spriteBatch.End();
        /// </code>  
        /// </example>
		public void DrawString (SpriteFont spriteFont, string text, Vector2 position, Color color)
		{
            CheckValid(spriteFont, text);

            var source = new SpriteFont.CharacterSource(text);
			spriteFont.DrawInto (
                this, ref source, position, color, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
		}

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="spriteFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this string.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this string.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this string.</param>
        /// <example>
        /// In this example we simply submit a string, using this overload.
        /// <code>
        /// spriteBatch.Begin();
        /// spriteBatch.DrawString(font, "Hello MonoGame !", Vector2.Zero, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        /// spriteBatch.End();
        /// </code>
        /// </example>
        /// <example>
        /// In this example we scale string up with scalar float scaling, using this overload.
        /// <code>
        /// scaling += 0.1f * (float)gameTime.ElapsedGameTime.TotalSeconds;
        /// spriteBatch.Begin();
        /// spriteBatch.DrawString(font, "Hello MonoGame !", Vector2.Zero, Color.White, 0, Vector2.Zero, 1 + scaling, SpriteEffects.None, 0);
        /// spriteBatch.End();
        /// </code>
        /// </example>
		public void DrawString (
			SpriteFont spriteFont, string text, Vector2 position, Color color,
            float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
		{
            CheckValid(spriteFont, text);

			var scaleVec = new Vector2(scale, scale);
            var source = new SpriteFont.CharacterSource(text);
            spriteFont.DrawInto(this, ref source, position, color, rotation, origin, scaleVec, effects, layerDepth);
		}

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="spriteFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this string.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this string.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this string.</param>
        /// <example>
        /// In this example we simply submit a string, using this overload.
        /// <code>
        /// spriteBatch.Begin();
        /// spriteBatch.DrawString(font, "Hello MonoGame !", Vector2.Zero, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
        /// spriteBatch.End();
        /// </code>
        /// </example>
        /// <example>
        /// In this example we also modify flip and stretch string vertically, using this overload.
        /// <code>
        /// spriteBatch.Begin();
        /// spriteBatch.DrawString(font, "Hello MonoGame !", Vector2.Zero, Color.White, 0, Vector2.Zero, new Vector2(1,2.5f), SpriteEffects.FlipHorizontally, 0);
        /// spriteBatch.End();
        /// </code>
        /// </example>
        /// <example>
        /// In this example we rotate a string around its center, using this overload.
        /// <code>
        /// rotation += 1.0f * (float)gameTime.ElapsedGameTime.TotalSeconds;
        /// spriteBatch.Begin();
        /// spriteBatch.DrawString(font, "Hello MonoGame !", new Vector2(100, 100), Color.White, rotation, font.MeasureString("Hello MonoGame !") / 2.0f, Vector2.One, SpriteEffects.None, .0f);
        /// spriteBatch.End();
        /// </code>
        /// </example>     
		public void DrawString (
			SpriteFont spriteFont, string text, Vector2 position, Color color,
            float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
		{
            CheckValid(spriteFont, text);

            var source = new SpriteFont.CharacterSource(text);
            spriteFont.DrawInto(this, ref source, position, color, rotation, origin, scale, effects, layerDepth);
		}

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="spriteFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        /// <example>
        /// In this example we simply submit a string, using this overload.
        /// <code>
        /// var text = new StringBuilder("Hello MonoGame !");
        /// 
        /// spriteBatch.Begin();
        /// spriteBatch.DrawString(font, text, Vector2.Zero, Color.White);
        /// spriteBatch.End();
        /// </code>  
        /// </example>
		public void DrawString (SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color)
		{
            CheckValid(spriteFont, text);

            var source = new SpriteFont.CharacterSource(text);
			spriteFont.DrawInto(this, ref source, position, color, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
		}

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="spriteFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this string.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this string.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this string.</param>
        /// <example>
        /// In this example we simply submit a string, using this overload.
        /// <code>
        /// var text = new StringBuilder("Hello MonoGame !");
        /// 
        /// spriteBatch.Begin();
        /// spriteBatch.DrawString(font, text, Vector2.Zero, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        /// spriteBatch.End();
        /// </code>
        /// </example>
        public void DrawString (
			SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color,
            float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
		{
            CheckValid(spriteFont, text);

			var scaleVec = new Vector2 (scale, scale);
            var source = new SpriteFont.CharacterSource(text);
            spriteFont.DrawInto(this, ref source, position, color, rotation, origin, scaleVec, effects, layerDepth);
		}

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="spriteFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this string.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this string.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this string.</param>
        /// <example>
        /// In this example we simply submit a string, using this overload.
        /// <code>
        /// var text = new StringBuilder("Hello MonoGame !");
        /// 
        /// spriteBatch.Begin();
        /// spriteBatch.DrawString(font, text, Vector2.Zero, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
        /// spriteBatch.End();
        /// </code>
        /// </example>
		public void DrawString (
			SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color,
            float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
		{
            CheckValid(spriteFont, text);

            var source = new SpriteFont.CharacterSource(text);
            spriteFont.DrawInto(this, ref source, position, color, rotation, origin, scale, effects, layerDepth);
		}

        /// <summary>
        /// Immediately releases the unmanaged resources used by this object.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
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
	}
}

