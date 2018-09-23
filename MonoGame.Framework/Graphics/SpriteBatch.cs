// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Helper class for drawing text strings and sprites in one or more optimized batches.
    /// </summary>
	public class SpriteBatch : GraphicsResource
	{
        #region Private Fields
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

		Matrix? _matrix;
	    private Viewport _lastViewport;
	    private Matrix _projection;
		Rectangle _tempRect = new Rectangle (0,0,0,0);
		Vector2 _texCoordTL = new Vector2 (0,0);
		Vector2 _texCoordBR = new Vector2 (0,0);
        #endregion

        internal static bool NeedsHalfPixelOffset;

        /// <summary>
        /// Constructs a <see cref="SpriteBatch"/>.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/>, which will be used for sprite rendering.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="graphicsDevice"/> is null.</exception>
        public SpriteBatch (GraphicsDevice graphicsDevice)
		{
			if (graphicsDevice == null)
            {
				throw new ArgumentNullException ("graphicsDevice", FrameworkResources.ResourceCreationWhenDeviceIsNull);
			}	

			this.GraphicsDevice = graphicsDevice;

            // Use a custom SpriteEffect so we can control the transformation matrix
            _spriteEffect = new Effect(graphicsDevice, EffectResource.SpriteEffect.Bytecode);
            _matrixTransform = _spriteEffect.Parameters["MatrixTransform"];
            _spritePass = _spriteEffect.CurrentTechnique.Passes[0];

            _batcher = new SpriteBatcher(graphicsDevice);

            _beginCalled = false;
		}

        /// <summary>
        /// Begins a new sprite and text batch with the specified render state.
        /// </summary>
        /// <param name="sortMode">The drawing order for sprite and text drawing. <see cref="SpriteSortMode.Deferred"/> by default.</param>
        /// <param name="blendState">State of the blending. Uses <see cref="BlendState.AlphaBlend"/> if null.</param>
        /// <param name="samplerState">State of the sampler. Uses <see cref="SamplerState.LinearClamp"/> if null.</param>
        /// <param name="depthStencilState">State of the depth-stencil buffer. Uses <see cref="DepthStencilState.None"/> if null.</param>
        /// <param name="rasterizerState">State of the rasterization. Uses <see cref="RasterizerState.CullCounterClockwise"/> if null.</param>
        /// <param name="effect">A custom <see cref="Effect"/> to override the default sprite effect. Uses default sprite effect if null.</param>
        /// <param name="transformMatrix">An optional matrix used to transform the sprite geometry. Uses <see cref="Matrix.Identity"/> if null.</param>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="Begin"/> is called next time without previous <see cref="End"/>.</exception>
        /// <remarks>This method uses optional parameters.</remarks>
        /// <remarks>The <see cref="Begin"/> Begin should be called before drawing commands, and you cannot call it again before subsequent <see cref="End"/>.</remarks>
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
            _matrix = transformMatrix;

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
		public void End ()
		{
            if (!_beginCalled)
                throw new InvalidOperationException("Begin must be called before calling End.");

			_beginCalled = false;

			if (_sortMode != SpriteSortMode.Immediate)
				Setup();
            
            _batcher.DrawBatch(_sortMode, _effect);
        }
		
		void Setup() 
        {
            var gd = GraphicsDevice;
			gd.BlendState = _blendState;
			gd.DepthStencilState = _depthStencilState;
			gd.RasterizerState = _rasterizerState;
			gd.SamplerStates[0] = _samplerState;
			
			var vp = gd.Viewport;
            if ((vp.Width != _lastViewport.Width) || (vp.Height != _lastViewport.Height))
            {
                // Normal 3D cameras look into the -z direction (z = 1 is in front of z = 0). The
                // sprite batch layer depth is the opposite (z = 0 is in front of z = 1).
                // --> We get the correct matrix with near plane 0 and far plane -1.
                Matrix.CreateOrthographicOffCenter(0, vp.Width, vp.Height, 0, 0, -1, out _projection);

                // Some platforms require a half pixel offset to match DX.
                if (NeedsHalfPixelOffset)
                {
                    _projection.M41 += -0.5f * _projection.M11;
                    _projection.M42 += -0.5f * _projection.M22;
                }

                _lastViewport = vp;
            }

            if (_matrix.HasValue)
                _matrixTransform.SetValue(_matrix.GetValueOrDefault() * _projection);
            else
                _matrixTransform.SetValue(_projection);

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
        /// <param name="position">The drawing location on screen or null if <paramref name="destinationRectangle"> is used.</paramref></param>
        /// <param name="destinationRectangle">The drawing bounds on screen or null if <paramref name="position"> is used.</paramref></param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="origin">An optional center of rotation. Uses <see cref="Vector2.Zero"/> if null.</param>
        /// <param name="rotation">An optional rotation of this sprite. 0 by default.</param>
        /// <param name="scale">An optional scale vector. Uses <see cref="Vector2.One"/> if null.</param>
        /// <param name="color">An optional color mask. Uses <see cref="Color.White"/> if null.</param>
        /// <param name="effects">The optional drawing modificators. <see cref="SpriteEffects.None"/> by default.</param>
        /// <param name="layerDepth">An optional depth of the layer of this sprite. 0 by default.</param>
        /// <exception cref="InvalidOperationException">Throwns if both <paramref name="position"/> and <paramref name="destinationRectangle"/> been used.</exception>
        /// <remarks>This overload uses optional parameters. This overload requires only one of <paramref name="position"/> and <paramref name="destinationRectangle"/> been used.</remarks>
        [Obsolete("In future versions this method can be removed.")]
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

            var item = _batcher.CreateBatchItem();
            item.Texture = texture;

            // set SortKey based on SpriteSortMode.
            switch ( _sortMode )
            {
                // Comparison of Texture objects.
                case SpriteSortMode.Texture:
                    item.SortKey = texture.SortingKey;
                    break;
                // Comparison of Depth
                case SpriteSortMode.FrontToBack:
                    item.SortKey = layerDepth;
                    break;
                // Comparison of Depth in reverse
                case SpriteSortMode.BackToFront:
                    item.SortKey = -layerDepth;
                    break;
            }
                        
            origin = origin * scale;
            
            float w, h;
            if (sourceRectangle.HasValue)
            {
                var srcRect = sourceRectangle.GetValueOrDefault();
                w = srcRect.Width * scale.X;
                h = srcRect.Height * scale.Y;
                _texCoordTL.X = srcRect.X * texture.TexelWidth;
                _texCoordTL.Y = srcRect.Y * texture.TexelHeight;
                _texCoordBR.X = (srcRect.X + srcRect.Width) * texture.TexelWidth;
                _texCoordBR.Y = (srcRect.Y + srcRect.Height) * texture.TexelHeight;
            }
            else
            {
                w = texture.Width * scale.X;
                h = texture.Height * scale.Y;
                _texCoordTL = Vector2.Zero;
                _texCoordBR = Vector2.One;
            }
            
            if ((effects & SpriteEffects.FlipVertically) != 0)
            {
                var temp = _texCoordBR.Y;
				_texCoordBR.Y = _texCoordTL.Y;
				_texCoordTL.Y = temp;
            }
            if ((effects & SpriteEffects.FlipHorizontally) != 0)
            {
                var temp = _texCoordBR.X;
				_texCoordBR.X = _texCoordTL.X;
				_texCoordTL.X = temp;
            }
            
            if (rotation == 0f)
            {
                item.Set(position.X - origin.X,
                        position.Y - origin.Y,
                        w,
                        h,
                        color,
                        _texCoordTL,
                        _texCoordBR,
                        layerDepth);
            }
            else
            {
                item.Set(position.X,
                        position.Y,
                        -origin.X,
                        -origin.Y,
                        w,
                        h,
                        (float)Math.Sin(rotation),
                        (float)Math.Cos(rotation),
                        color,
                        _texCoordTL,
                        _texCoordBR,
                        layerDepth);
            }
            
            FlushIfNeeded();
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
            var scaleVec = new Vector2(scale, scale);
            Draw(texture, position, sourceRectangle, color, rotation, origin, scaleVec, effects, layerDepth);
		}

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="destinationRectangle">The drawing bounds on screen.</param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this sprite.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this sprite.</param>
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
            
            var item = _batcher.CreateBatchItem();
            item.Texture = texture;

            // set SortKey based on SpriteSortMode.
            switch ( _sortMode )
            {
                // Comparison of Texture objects.
                case SpriteSortMode.Texture:
                    item.SortKey = texture.SortingKey;
                    break;
                // Comparison of Depth
                case SpriteSortMode.FrontToBack:
                    item.SortKey = layerDepth;
                    break;
                // Comparison of Depth in reverse
                case SpriteSortMode.BackToFront:
                    item.SortKey = -layerDepth;
                    break;
            }

            if (sourceRectangle.HasValue)
            {
                var srcRect = sourceRectangle.GetValueOrDefault();
                _texCoordTL.X = srcRect.X * texture.TexelWidth;
                _texCoordTL.Y = srcRect.Y * texture.TexelHeight;
                _texCoordBR.X = (srcRect.X + srcRect.Width) * texture.TexelWidth;
                _texCoordBR.Y = (srcRect.Y + srcRect.Height) * texture.TexelHeight;

                if(srcRect.Width != 0)
                    origin.X = origin.X * (float)destinationRectangle.Width / (float)srcRect.Width;
                else
                    origin.X = origin.X * (float)destinationRectangle.Width * texture.TexelWidth;
                if(srcRect.Height != 0)
                    origin.Y = origin.Y * (float)destinationRectangle.Height / (float)srcRect.Height; 
                else
                    origin.Y = origin.Y * (float)destinationRectangle.Height * texture.TexelHeight;
            }
            else
            {
                _texCoordTL = Vector2.Zero;
                _texCoordBR = Vector2.One;
                
                origin.X = origin.X * (float)destinationRectangle.Width  * texture.TexelWidth;
                origin.Y = origin.Y * (float)destinationRectangle.Height * texture.TexelHeight;
            }
            
			if ((effects & SpriteEffects.FlipVertically) != 0)
            {
                var temp = _texCoordBR.Y;
				_texCoordBR.Y = _texCoordTL.Y;
				_texCoordTL.Y = temp;
			}
			if ((effects & SpriteEffects.FlipHorizontally) != 0)
            {
                var temp = _texCoordBR.X;
				_texCoordBR.X = _texCoordTL.X;
				_texCoordTL.X = temp;
			}

		    if (rotation == 0f)
		    {
                item.Set(destinationRectangle.X - origin.X,
                        destinationRectangle.Y - origin.Y,
                        destinationRectangle.Width,
                        destinationRectangle.Height,
                        color,
                        _texCoordTL,
                        _texCoordBR,
                        layerDepth);
            }
            else
		    {
                item.Set(destinationRectangle.X,
                        destinationRectangle.Y,
                        -origin.X,
                        -origin.Y,
                        destinationRectangle.Width,
                        destinationRectangle.Height,
                        (float)Math.Sin(rotation),
                        (float)Math.Cos(rotation),
                        color,
                        _texCoordTL,
                        _texCoordBR,
                        layerDepth);
            }

			FlushIfNeeded();
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
		public void Draw (Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color)
		{
			CheckValid(texture);
            
			var item = _batcher.CreateBatchItem();
			item.Texture = texture;
            
            // set SortKey based on SpriteSortMode.
            item.SortKey = _sortMode == SpriteSortMode.Texture ? texture.SortingKey : 0;

            Vector2 size;

            if (sourceRectangle.HasValue)
            {
                var srcRect = sourceRectangle.GetValueOrDefault();
                size = new Vector2(srcRect.Width, srcRect.Height);
                _texCoordTL.X = srcRect.X * texture.TexelWidth;
                _texCoordTL.Y = srcRect.Y * texture.TexelHeight;
                _texCoordBR.X = (srcRect.X + srcRect.Width)  * texture.TexelWidth;
                _texCoordBR.Y = (srcRect.Y + srcRect.Height) * texture.TexelHeight;
            }
            else
            {
                size = new Vector2(texture.width, texture.height);
                _texCoordTL = Vector2.Zero;
                _texCoordBR = Vector2.One;
            }

            item.Set(position.X,
                     position.Y,
                     size.X,
                     size.Y,
                     color,
                     _texCoordTL,
                     _texCoordBR,
                     0);

            FlushIfNeeded();
		}

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="destinationRectangle">The drawing bounds on screen.</param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="color">A color mask.</param>
		public void Draw (Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color)
		{
            CheckValid(texture);
            
			var item = _batcher.CreateBatchItem();
			item.Texture = texture;
            
            // set SortKey based on SpriteSortMode.
            item.SortKey = _sortMode == SpriteSortMode.Texture ? texture.SortingKey : 0;
            
            if (sourceRectangle.HasValue)
            {
                var srcRect = sourceRectangle.GetValueOrDefault();
                _texCoordTL.X = srcRect.X * texture.TexelWidth;
                _texCoordTL.Y = srcRect.Y * texture.TexelHeight;
                _texCoordBR.X = (srcRect.X + srcRect.Width) * texture.TexelWidth;
                _texCoordBR.Y = (srcRect.Y + srcRect.Height) * texture.TexelHeight;
            }
            else
            {
                _texCoordTL = Vector2.Zero;
                _texCoordBR = Vector2.One;
            }

            item.Set(destinationRectangle.X,
                     destinationRectangle.Y,
                     destinationRectangle.Width,
                     destinationRectangle.Height,
                     color,
                     _texCoordTL,
                     _texCoordBR,
                     0);
            
            FlushIfNeeded();
		}

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
		public void Draw (Texture2D texture, Vector2 position, Color color)
		{
			CheckValid(texture);
            
			var item = _batcher.CreateBatchItem();
			item.Texture = texture;
            
            // set SortKey based on SpriteSortMode.
            item.SortKey = _sortMode == SpriteSortMode.Texture ? texture.SortingKey : 0;
            
            item.Set(position.X,
                     position.Y,
                     texture.Width,
                     texture.Height,
                     color,
                     Vector2.Zero,
                     Vector2.One,
                     0);

            FlushIfNeeded();
		}

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="destinationRectangle">The drawing bounds on screen.</param>
        /// <param name="color">A color mask.</param>
        public void Draw(Texture2D texture, Rectangle destinationRectangle, Color color)
		{
            CheckValid(texture);
            
            var item = _batcher.CreateBatchItem();
            item.Texture = texture;
            
            // set SortKey based on SpriteSortMode.
            item.SortKey = _sortMode == SpriteSortMode.Texture ? texture.SortingKey : 0;
            
            item.Set(destinationRectangle.X,
                     destinationRectangle.Y,
                     destinationRectangle.Width,
                     destinationRectangle.Height,
                     color,
                     Vector2.Zero,
                     Vector2.One,
                     0);
            
            FlushIfNeeded();
		}

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="spriteFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
		public unsafe void DrawString (SpriteFont spriteFont, string text, Vector2 position, Color color)
		{
            CheckValid(spriteFont, text);
            
            float sortKey = (_sortMode == SpriteSortMode.Texture) ? spriteFont.Texture.SortingKey : 0;

            var offset = Vector2.Zero;
            var firstGlyphOfLine = true;

            fixed (SpriteFont.Glyph* pGlyphs = spriteFont.Glyphs)
            for (var i = 0; i < text.Length; ++i)
            {
                var c = text[i];

                if (c == '\r')
                    continue;

                if (c == '\n')
                {
                    offset.X = 0;
                    offset.Y += spriteFont.LineSpacing;
                    firstGlyphOfLine = true;
                    continue;
                }
 
                var currentGlyphIndex = spriteFont.GetGlyphIndexOrDefault(c);
                var pCurrentGlyph = pGlyphs + currentGlyphIndex;

                // The first character on a line might have a negative left side bearing.
                // In this scenario, SpriteBatch/SpriteFont normally offset the text to the right,
                //  so that text does not hang off the left side of its rectangle.
                if (firstGlyphOfLine)
                {
                    offset.X = Math.Max(pCurrentGlyph->LeftSideBearing, 0);
                    firstGlyphOfLine = false;
                }
                else
                {
                    offset.X += spriteFont.Spacing + pCurrentGlyph->LeftSideBearing;
                }

                var p = offset;                
                p.X += pCurrentGlyph->Cropping.X;
                p.Y += pCurrentGlyph->Cropping.Y;
                p += position;

                var item = _batcher.CreateBatchItem();
                item.Texture = spriteFont.Texture;
                item.SortKey = sortKey;
            
                _texCoordTL.X = pCurrentGlyph->BoundsInTexture.X * spriteFont.Texture.TexelWidth;
                _texCoordTL.Y = pCurrentGlyph->BoundsInTexture.Y * spriteFont.Texture.TexelHeight;
                _texCoordBR.X = (pCurrentGlyph->BoundsInTexture.X + pCurrentGlyph->BoundsInTexture.Width) * spriteFont.Texture.TexelWidth;
                _texCoordBR.Y = (pCurrentGlyph->BoundsInTexture.Y + pCurrentGlyph->BoundsInTexture.Height) * spriteFont.Texture.TexelHeight;

                item.Set(p.X,
                         p.Y,
                         pCurrentGlyph->BoundsInTexture.Width,
                         pCurrentGlyph->BoundsInTexture.Height,
                         color,
                         _texCoordTL,
                         _texCoordBR,
                         0);
                
                offset.X += pCurrentGlyph->Width + pCurrentGlyph->RightSideBearing;
            }

			// We need to flush if we're using Immediate sort mode.
			FlushIfNeeded();
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
		public void DrawString (
			SpriteFont spriteFont, string text, Vector2 position, Color color,
            float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
		{
			var scaleVec = new Vector2(scale, scale);
            DrawString(spriteFont, text, position, color, rotation, origin, scaleVec, effects, layerDepth);
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
		public unsafe void DrawString (
			SpriteFont spriteFont, string text, Vector2 position, Color color,
            float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
		{
            CheckValid(spriteFont, text);
            
            float sortKey = 0;
            // set SortKey based on SpriteSortMode.
            switch (_sortMode)
            {
                    // Comparison of Texture objects.
                    case SpriteSortMode.Texture:
                        sortKey = spriteFont.Texture.SortingKey;
                        break;
                    // Comparison of Depth
                    case SpriteSortMode.FrontToBack:
                        sortKey = layerDepth;
                        break;
                    // Comparison of Depth in reverse
                    case SpriteSortMode.BackToFront:
                        sortKey = -layerDepth;
                        break;
            }

            var flipAdjustment = Vector2.Zero;

            var flippedVert = (effects & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
            var flippedHorz = (effects & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;

            if (flippedVert || flippedHorz)
            {
                Vector2 size;
                
                var source = new SpriteFont.CharacterSource(text);
                spriteFont.MeasureString(ref source, out size);

                if (flippedHorz)
                {
                    origin.X *= -1;
                    flipAdjustment.X = -size.X;
                }

                if (flippedVert)
                {
                    origin.Y *= -1;
                    flipAdjustment.Y = spriteFont.LineSpacing - size.Y;
                }
            }
            
            Matrix transformation = Matrix.Identity;
            float cos = 0, sin = 0;
            if (rotation == 0)
            {
                transformation.M11 = (flippedHorz ? -scale.X : scale.X);
                transformation.M22 = (flippedVert ? -scale.Y : scale.Y);
                transformation.M41 = ((flipAdjustment.X - origin.X) * transformation.M11) + position.X;
                transformation.M42 = ((flipAdjustment.Y - origin.Y) * transformation.M22) + position.Y;
            }
            else
            {
                cos = (float)Math.Cos(rotation);
                sin = (float)Math.Sin(rotation);
                transformation.M11 = (flippedHorz ? -scale.X : scale.X) * cos;
                transformation.M12 = (flippedHorz ? -scale.X : scale.X) * sin;
                transformation.M21 = (flippedVert ? -scale.Y : scale.Y) * (-sin);
                transformation.M22 = (flippedVert ? -scale.Y : scale.Y) * cos;
                transformation.M41 = (((flipAdjustment.X - origin.X) * transformation.M11) + (flipAdjustment.Y - origin.Y) * transformation.M21) + position.X;
                transformation.M42 = (((flipAdjustment.X - origin.X) * transformation.M12) + (flipAdjustment.Y - origin.Y) * transformation.M22) + position.Y; 
            }

            var offset = Vector2.Zero;
            var firstGlyphOfLine = true;

            fixed (SpriteFont.Glyph* pGlyphs = spriteFont.Glyphs)
            for (var i = 0; i < text.Length; ++i)
            {
                var c = text[i];

                if (c == '\r')
                    continue;

                if (c == '\n')
                {
                    offset.X = 0;
                    offset.Y += spriteFont.LineSpacing;
                    firstGlyphOfLine = true;
                    continue;
                }

                var currentGlyphIndex = spriteFont.GetGlyphIndexOrDefault(c);
                var pCurrentGlyph = pGlyphs + currentGlyphIndex;

                // The first character on a line might have a negative left side bearing.
                // In this scenario, SpriteBatch/SpriteFont normally offset the text to the right,
                //  so that text does not hang off the left side of its rectangle.
                if (firstGlyphOfLine)
                {
                    offset.X = Math.Max(pCurrentGlyph->LeftSideBearing, 0);
                    firstGlyphOfLine = false;
                }
                else
                {
                    offset.X += spriteFont.Spacing + pCurrentGlyph->LeftSideBearing;
                }

                var p = offset;

                if (flippedHorz)
                    p.X += pCurrentGlyph->BoundsInTexture.Width;
                p.X += pCurrentGlyph->Cropping.X;

                if (flippedVert)
                    p.Y += pCurrentGlyph->BoundsInTexture.Height - spriteFont.LineSpacing;
                p.Y += pCurrentGlyph->Cropping.Y;

                Vector2.Transform(ref p, ref transformation, out p);

                var item = _batcher.CreateBatchItem();               
                item.Texture = spriteFont.Texture;
                item.SortKey = sortKey;
                
                _texCoordTL.X = pCurrentGlyph->BoundsInTexture.X * spriteFont.Texture.TexelWidth;
                _texCoordTL.Y = pCurrentGlyph->BoundsInTexture.Y * spriteFont.Texture.TexelHeight;
                _texCoordBR.X = (pCurrentGlyph->BoundsInTexture.X + pCurrentGlyph->BoundsInTexture.Width) * spriteFont.Texture.TexelWidth;
                _texCoordBR.Y = (pCurrentGlyph->BoundsInTexture.Y + pCurrentGlyph->BoundsInTexture.Height) * spriteFont.Texture.TexelHeight;
                            
                if ((effects & SpriteEffects.FlipVertically) != 0)
                {
                    var temp = _texCoordBR.Y;
				    _texCoordBR.Y = _texCoordTL.Y;
				    _texCoordTL.Y = temp;
			    }
                if ((effects & SpriteEffects.FlipHorizontally) != 0)
                {
                    var temp = _texCoordBR.X;
				    _texCoordBR.X = _texCoordTL.X;
				    _texCoordTL.X = temp;
			    }

                if (rotation == 0f)
                {
                    item.Set(p.X,
                            p.Y,
                            pCurrentGlyph->BoundsInTexture.Width * scale.X,
                            pCurrentGlyph->BoundsInTexture.Height * scale.Y,
                            color,
                            _texCoordTL,
                            _texCoordBR,
                            layerDepth);
                }
                else
                {
                    item.Set(p.X,
                            p.Y,
                            0,
                            0,
                            pCurrentGlyph->BoundsInTexture.Width * scale.X,
                            pCurrentGlyph->BoundsInTexture.Height * scale.Y,
                            sin,
                            cos,
                            color,
                            _texCoordTL,
                            _texCoordBR,
                            layerDepth);
                }
                
                offset.X += pCurrentGlyph->Width + pCurrentGlyph->RightSideBearing;
            }

			// We need to flush if we're using Immediate sort mode.
			FlushIfNeeded();
		}

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="spriteFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
		public unsafe void DrawString (SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color)
		{
            CheckValid(spriteFont, text);
            
            float sortKey =  (_sortMode == SpriteSortMode.Texture) ? spriteFont.Texture.SortingKey : 0;

            var offset = Vector2.Zero;
            var firstGlyphOfLine = true;

            fixed (SpriteFont.Glyph* pGlyphs = spriteFont.Glyphs)
            for (var i = 0; i < text.Length; ++i)
            {
                var c = text[i];

                if (c == '\r')
                    continue;

                if (c == '\n')
                {
                    offset.X = 0;
                    offset.Y += spriteFont.LineSpacing;
                    firstGlyphOfLine = true;
                    continue;
                }

                var currentGlyphIndex = spriteFont.GetGlyphIndexOrDefault(c);
                var pCurrentGlyph = pGlyphs + currentGlyphIndex;

                // The first character on a line might have a negative left side bearing.
                // In this scenario, SpriteBatch/SpriteFont normally offset the text to the right,
                //  so that text does not hang off the left side of its rectangle.
                if (firstGlyphOfLine)
                {
                    offset.X = Math.Max(pCurrentGlyph->LeftSideBearing, 0);
                    firstGlyphOfLine = false;
                }
                else
                {
                    offset.X += spriteFont.Spacing + pCurrentGlyph->LeftSideBearing;
                }

                var p = offset;                
                p.X += pCurrentGlyph->Cropping.X;
                p.Y += pCurrentGlyph->Cropping.Y;
                p += position;
                
                var item = _batcher.CreateBatchItem();
                item.Texture = spriteFont.Texture;
                item.SortKey = sortKey;
            
                _texCoordTL.X = pCurrentGlyph->BoundsInTexture.X * spriteFont.Texture.TexelWidth;
                _texCoordTL.Y = pCurrentGlyph->BoundsInTexture.Y * spriteFont.Texture.TexelHeight;
                _texCoordBR.X = (pCurrentGlyph->BoundsInTexture.X + pCurrentGlyph->BoundsInTexture.Width) * spriteFont.Texture.TexelWidth;
                _texCoordBR.Y = (pCurrentGlyph->BoundsInTexture.Y + pCurrentGlyph->BoundsInTexture.Height) * spriteFont.Texture.TexelHeight;

                item.Set(p.X,
                         p.Y,
                         pCurrentGlyph->BoundsInTexture.Width,
                         pCurrentGlyph->BoundsInTexture.Height,
                         color,
                         _texCoordTL,
                         _texCoordBR,
                         0);

                offset.X += pCurrentGlyph->Width + pCurrentGlyph->RightSideBearing;
            }

			// We need to flush if we're using Immediate sort mode.
			FlushIfNeeded();
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
		public void DrawString (
			SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color,
            float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
		{
			var scaleVec = new Vector2 (scale, scale);
            DrawString(spriteFont, text, position, color, rotation, origin, scaleVec, effects, layerDepth);
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
		public unsafe void DrawString (
			SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color,
            float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
		{
            CheckValid(spriteFont, text);
            
            float sortKey = 0;
            // set SortKey based on SpriteSortMode.
            switch (_sortMode)
            {
                    // Comparison of Texture objects.
                    case SpriteSortMode.Texture:
                        sortKey = spriteFont.Texture.SortingKey;
                        break;
                    // Comparison of Depth
                    case SpriteSortMode.FrontToBack:
                        sortKey = layerDepth;
                        break;
                    // Comparison of Depth in reverse
                    case SpriteSortMode.BackToFront:
                        sortKey = -layerDepth;
                        break;
            }

            var flipAdjustment = Vector2.Zero;

            var flippedVert = (effects & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
            var flippedHorz = (effects & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;

            if (flippedVert || flippedHorz)
            {
                var source = new SpriteFont.CharacterSource(text);
                Vector2 size;
                spriteFont.MeasureString(ref source, out size);

                if (flippedHorz)
                {
                    origin.X *= -1;
                    flipAdjustment.X = -size.X;
                }

                if (flippedVert)
                {
                    origin.Y *= -1;
                    flipAdjustment.Y = spriteFont.LineSpacing - size.Y;
                }
            }
            
            Matrix transformation = Matrix.Identity;
            float cos = 0, sin = 0;
            if (rotation == 0)
            {
                transformation.M11 = (flippedHorz ? -scale.X : scale.X);
                transformation.M22 = (flippedVert ? -scale.Y : scale.Y);
                transformation.M41 = ((flipAdjustment.X - origin.X) * transformation.M11) + position.X;
                transformation.M42 = ((flipAdjustment.Y - origin.Y) * transformation.M22) + position.Y;
            }
            else
            {
                cos = (float)Math.Cos(rotation);
                sin = (float)Math.Sin(rotation);
                transformation.M11 = (flippedHorz ? -scale.X : scale.X) * cos;
                transformation.M12 = (flippedHorz ? -scale.X : scale.X) * sin;
                transformation.M21 = (flippedVert ? -scale.Y : scale.Y) * (-sin);
                transformation.M22 = (flippedVert ? -scale.Y : scale.Y) * cos;
                transformation.M41 = (((flipAdjustment.X - origin.X) * transformation.M11) + (flipAdjustment.Y - origin.Y) * transformation.M21) + position.X;
                transformation.M42 = (((flipAdjustment.X - origin.X) * transformation.M12) + (flipAdjustment.Y - origin.Y) * transformation.M22) + position.Y; 
            }

            var offset = Vector2.Zero;
            var firstGlyphOfLine = true;

            fixed (SpriteFont.Glyph* pGlyphs = spriteFont.Glyphs)
            for (var i = 0; i < text.Length; ++i)
            {
                var c = text[i];

                if (c == '\r')
                    continue;

                if (c == '\n')
                {
                    offset.X = 0;
                    offset.Y += spriteFont.LineSpacing;
                    firstGlyphOfLine = true;
                    continue;
                }

                var currentGlyphIndex = spriteFont.GetGlyphIndexOrDefault(c);
                var pCurrentGlyph = pGlyphs + currentGlyphIndex;

                // The first character on a line might have a negative left side bearing.
                // In this scenario, SpriteBatch/SpriteFont normally offset the text to the right,
                //  so that text does not hang off the left side of its rectangle.
                if (firstGlyphOfLine)
                {
                    offset.X = Math.Max(pCurrentGlyph->LeftSideBearing, 0);
                    firstGlyphOfLine = false;
                }
                else
                {
                    offset.X += spriteFont.Spacing + pCurrentGlyph->LeftSideBearing;
                }

                var p = offset;

                if (flippedHorz)
                    p.X += pCurrentGlyph->BoundsInTexture.Width;
                p.X += pCurrentGlyph->Cropping.X;

                if (flippedVert)
                    p.Y += pCurrentGlyph->BoundsInTexture.Height - spriteFont.LineSpacing;
                p.Y += pCurrentGlyph->Cropping.Y;

                Vector2.Transform(ref p, ref transformation, out p);
                
                var item = _batcher.CreateBatchItem();               
                item.Texture = spriteFont.Texture;
                item.SortKey = sortKey;
                
                _texCoordTL.X = pCurrentGlyph->BoundsInTexture.X * (float)spriteFont.Texture.TexelWidth;
                _texCoordTL.Y = pCurrentGlyph->BoundsInTexture.Y * (float)spriteFont.Texture.TexelHeight;
                _texCoordBR.X = (pCurrentGlyph->BoundsInTexture.X + pCurrentGlyph->BoundsInTexture.Width) * (float)spriteFont.Texture.TexelWidth;
                _texCoordBR.Y = (pCurrentGlyph->BoundsInTexture.Y + pCurrentGlyph->BoundsInTexture.Height) * (float)spriteFont.Texture.TexelHeight;
                            
                if ((effects & SpriteEffects.FlipVertically) != 0)
                {
                    var temp = _texCoordBR.Y;
				    _texCoordBR.Y = _texCoordTL.Y;
				    _texCoordTL.Y = temp;
			    }
                if ((effects & SpriteEffects.FlipHorizontally) != 0)
                {
                    var temp = _texCoordBR.X;
				    _texCoordBR.X = _texCoordTL.X;
				    _texCoordTL.X = temp;
			    }

                if (rotation == 0f)
                {
                    item.Set(p.X,
                            p.Y,
                            pCurrentGlyph->BoundsInTexture.Width * scale.X,
                            pCurrentGlyph->BoundsInTexture.Height * scale.Y,
                            color,
                            _texCoordTL,
                            _texCoordBR,
                            layerDepth);
                }
                else
                {
                    item.Set(p.X,
                            p.Y,
                            0,
                            0,
                            pCurrentGlyph->BoundsInTexture.Width * scale.X,
                            pCurrentGlyph->BoundsInTexture.Height * scale.Y,
                            sin,
                            cos,
                            color,
                            _texCoordTL,
                            _texCoordBR,
                            layerDepth);
                }

                offset.X += pCurrentGlyph->Width + pCurrentGlyph->RightSideBearing;
			}

			// We need to flush if we're using Immediate sort mode.
			FlushIfNeeded();
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

