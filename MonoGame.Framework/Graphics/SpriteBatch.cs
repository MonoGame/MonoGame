// #region License
// /*
// Microsoft Public License (Ms-PL)
// XnaTouch - Copyright © 2009 The XnaTouch Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
// #endregion License
// 
using System;
using System.Text;
using System.Collections.Generic;

using OpenTK.Graphics.ES11;

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
		Matrix _matrix;
		
		Rectangle tempRect = new Rectangle(0,0,0,0);
		Vector2 texCoordTL = new Vector2(0,0);
		Vector2 texCoordBR = new Vector2(0,0);

        public SpriteBatch ( GraphicsDevice graphicsDevice )
		{
			if (graphicsDevice == null )
			{
				throw new ArgumentException("graphicsDevice");
			}	
			
			this.graphicsDevice = graphicsDevice;
			
			_batcher = new SpriteBatcher();
		}
		
		public void Begin()
		{
			Begin( SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.Identity );			
		}
		
		public void Begin(SpriteSortMode sortMode, BlendState blendState)
		{
			Begin( sortMode, blendState, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.Identity );			
		}
		
		public void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState )
		{	
			Begin( sortMode, blendState, samplerState, depthStencilState, rasterizerState, null, Matrix.Identity );	
		}
		
		public void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect)
		{
			Begin( sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, Matrix.Identity );			
		}
		
		public void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect, Matrix transformMatrix)
		{
			_sortMode = sortMode;

			_blendState = blendState ?? BlendState.AlphaBlend;
			_depthStencilState = depthStencilState ?? DepthStencilState.None;
			_samplerState = samplerState ?? SamplerState.LinearClamp;
			_rasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;
			
			if(effect != null)
				_effect = effect;
			_matrix = transformMatrix;
		}
		
		public void End()
		{					
			// Disable Blending by default = BlendState.Opaque
			GL.Disable(All.Blend);
			
			// set the blend mode
			if ( _blendState == BlendState.NonPremultiplied )
			{
				GL.BlendFunc(All.SrcAlpha, All.OneMinusSrcAlpha);
				GL.Enable(All.Blend);
			}
			
			if ( _blendState == BlendState.AlphaBlend )
			{
				GL.BlendFunc(All.One, All.OneMinusSrcAlpha);
				GL.Enable(All.Blend);				
			}
			
			if ( _blendState == BlendState.Additive )
			{
				GL.BlendFunc(All.SrcAlpha,All.One);
				GL.Enable(All.Blend);	
			}			
			
			// set camera
			GL.MatrixMode(All.Projection);
			GL.LoadIdentity();							
			
#if ANDROID			
			// Switch on the flags.
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
			switch (this.graphicsDevice.PresentationParameters.DisplayOrientation)
	        {
				case DisplayOrientation.LandscapeLeft:
                {
					GL.Rotate(-90, 0, 0, 1);
					break;
				}
				
				case DisplayOrientation.LandscapeRight:
                {
					GL.Rotate(90, 0, 0, 1);					
					break;
				}
				
			case DisplayOrientation.PortraitUpsideDown:
                {
					GL.Rotate(180, 0, 0, 1); 
					break;
				}
				
				default:
				{
					GL.Ortho(0, this.graphicsDevice.Viewport.Width, this.graphicsDevice.Viewport.Height, 0, -1, 1);
					break;
				}
			}
			
			GL.Ortho(0, this.graphicsDevice.Viewport.Width, this.graphicsDevice.Viewport.Height, 0, -1, 1);
#endif			
			
			// Enable Scissor Tests if necessary
			if ( this.graphicsDevice.RasterizerState.ScissorTestEnable )
			{
				GL.Enable(All.ScissorTest);
			}
			
			
			GL.MatrixMode(All.Modelview);			
			
			// Only swap our viewport if Width is greater than height
			if (this.graphicsDevice.Viewport.Width > this.graphicsDevice.Viewport.Height)
			{
				GL.Viewport(0, 0, this.graphicsDevice.Viewport.Height, this.graphicsDevice.Viewport.Width);
			}
			else
			{
				GL.Viewport(0, 0, this.graphicsDevice.Viewport.Width, this.graphicsDevice.Viewport.Height);
			}
			
			// Enable Scissor Tests if necessary
			if ( this.graphicsDevice.RasterizerState.ScissorTestEnable )
			{
				GL.Scissor(this.graphicsDevice.ScissorRectangle.X, this.graphicsDevice.ScissorRectangle.Y, this.graphicsDevice.ScissorRectangle.Width, this.graphicsDevice.ScissorRectangle.Height );
			}			
			
			GL.LoadMatrix( ref _matrix.M11 );	
			
			// Initialize OpenGL states (ideally move this to initialize somewhere else)	
			GL.Disable(All.DepthTest);
			GL.TexEnv(All.TextureEnv, All.TextureEnvMode,(int) All.BlendSrc);
			GL.Enable(All.Texture2D);
			GL.EnableClientState(All.VertexArray);
			GL.EnableClientState(All.ColorArray);
			GL.EnableClientState(All.TextureCoordArray);
			
			// Enable Culling for better performance
			GL.Enable(All.CullFace);
			GL.FrontFace(All.Cw);
			GL.Color4(1.0f, 1.0f, 1.0f, 1.0f);						
			
			_batcher.DrawBatch (_sortMode, _samplerState);
			
			if (this.graphicsDevice.RasterizerState.ScissorTestEnable)
            {
               GL.Disable(All.ScissorTest);
            }
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
			if (texture == null )
			{
				throw new ArgumentException("texture");
			}
			
			SpriteBatchItem item = _batcher.CreateBatchItem();
			
			item.Depth = depth;
			item.TextureID = (int) texture.ID;
				
			if ( sourceRectangle.HasValue)
			{
				tempRect = sourceRectangle.Value;
			}
			else
			{
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
				texCoordTL.X = tempRect.X*texWidthRatio;
				texCoordTL.Y = (tempRect.Y+tempRect.Height) * texHeightRatio;
				
				texCoordBR.X = (tempRect.X+tempRect.Width)*texWidthRatio;
				texCoordBR.Y = tempRect.Y*texHeightRatio;
				
			}
			else {
				texCoordTL.X = texture.Image.GetTextureCoordX( tempRect.X );
				texCoordTL.Y = texture.Image.GetTextureCoordY( tempRect.Y );
				texCoordBR.X = texture.Image.GetTextureCoordX( tempRect.X+tempRect.Width );
				texCoordBR.Y = texture.Image.GetTextureCoordY( tempRect.Y+tempRect.Height );
			}
			
			if ( (effect & SpriteEffects.FlipVertically) != 0 )
			{
				float temp = texCoordBR.Y;
				texCoordBR.Y = texCoordTL.Y;
				texCoordTL.Y = temp;
			}
			if ( (effect & SpriteEffects.FlipHorizontally) != 0 )
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
				 tempRect.Width*scale.X,
				 tempRect.Height*scale.Y,
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
			if (texture == null )
			{
				throw new ArgumentException("texture");
			}
			
			SpriteBatchItem item = _batcher.CreateBatchItem();
			
			item.Depth = depth;
			item.TextureID = (int) texture.ID;
						
			if ( sourceRectangle.HasValue)
			{
				tempRect = sourceRectangle.Value;
			}
			else
			{
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
				texCoordTL.X = tempRect.X*texWidthRatio;
				texCoordTL.Y = (tempRect.Y+tempRect.Height) * texHeightRatio;
				
				texCoordBR.X = (tempRect.X+tempRect.Width)*texWidthRatio;
				texCoordBR.Y = tempRect.Y*texHeightRatio;
				
			}
			else {
				texCoordTL.X = texture.Image.GetTextureCoordX( tempRect.X );
				texCoordTL.Y = texture.Image.GetTextureCoordY( tempRect.Y );
				texCoordBR.X = texture.Image.GetTextureCoordX( tempRect.X+tempRect.Width );
				texCoordBR.Y = texture.Image.GetTextureCoordY( tempRect.Y+tempRect.Height );
			}
			
			if ( (effect & SpriteEffects.FlipVertically) != 0)
			{
				float temp = texCoordBR.Y;
				texCoordBR.Y = texCoordTL.Y;
				texCoordTL.Y = temp;
			}
			if ( (effect & SpriteEffects.FlipHorizontally) != 0)
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
				 tempRect.Width*scale,
				 tempRect.Height*scale,
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
			if (texture == null )
			{
				throw new ArgumentException("texture");
			}
			
			SpriteBatchItem item = _batcher.CreateBatchItem();
			
			item.Depth = depth;
			item.TextureID = (int) texture.ID;
						
			if ( sourceRectangle.HasValue)
			{
				tempRect = sourceRectangle.Value;
			}
			else
			{
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
				texCoordTL.X = tempRect.X*texWidthRatio;
				texCoordTL.Y = (tempRect.Y+tempRect.Height) * texHeightRatio;
				
				texCoordBR.X = (tempRect.X+tempRect.Width)*texWidthRatio;
				texCoordBR.Y = tempRect.Y*texHeightRatio;
				
			}
			else {
				texCoordTL.X = texture.Image.GetTextureCoordX( tempRect.X );
				texCoordTL.Y = texture.Image.GetTextureCoordY( tempRect.Y );
				texCoordBR.X = texture.Image.GetTextureCoordX( tempRect.X+tempRect.Width );
				texCoordBR.Y = texture.Image.GetTextureCoordY( tempRect.Y+tempRect.Height );
			}
			
			if ( (effect & SpriteEffects.FlipVertically) != 0)
			{
				float temp = texCoordBR.Y;
				texCoordBR.Y = texCoordTL.Y;
				texCoordTL.Y = temp;
			}
			if ( (effect & SpriteEffects.FlipHorizontally) != 0)
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
			if (texture == null )
			{
				throw new ArgumentException("texture");
			}
			
			SpriteBatchItem item = _batcher.CreateBatchItem();
			
			item.Depth = 0.0f;
			item.TextureID = (int) texture.ID;
			
			if ( sourceRectangle.HasValue)
			{
				tempRect = sourceRectangle.Value;
			}
			else
			{
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
				texCoordTL.X = tempRect.X*texWidthRatio;
				texCoordTL.Y = (tempRect.Y+tempRect.Height) * texHeightRatio;
				
				texCoordBR.X = (tempRect.X+tempRect.Width)*texWidthRatio;
				texCoordBR.Y = tempRect.Y*texHeightRatio;
				
			}
			else {
				texCoordTL.X = texture.Image.GetTextureCoordX( tempRect.X );
				texCoordTL.Y = texture.Image.GetTextureCoordY( tempRect.Y );
				texCoordBR.X = texture.Image.GetTextureCoordX( tempRect.X+tempRect.Width );
				texCoordBR.Y = texture.Image.GetTextureCoordY( tempRect.Y+tempRect.Height );
			}
			
			item.Set ( position.X, position.Y, tempRect.Width, tempRect.Height, color, texCoordTL, texCoordBR );
		}
		
		public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color)
		{
			if (texture == null )
			{
				throw new ArgumentException("texture");
			}
			
			SpriteBatchItem item = _batcher.CreateBatchItem();
			
			item.Depth = 0.0f;
			item.TextureID = (int) texture.ID;
			
			if ( sourceRectangle.HasValue)
			{
				tempRect = sourceRectangle.Value;
			}
			else
			{
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
				texCoordTL.X = tempRect.X*texWidthRatio;
				texCoordTL.Y = (tempRect.Y+tempRect.Height) * texHeightRatio;
				
				texCoordBR.X = (tempRect.X+tempRect.Width)*texWidthRatio;
				texCoordBR.Y = tempRect.Y*texHeightRatio;
				
			}
			else {
				texCoordTL.X = texture.Image.GetTextureCoordX( tempRect.X );
				texCoordTL.Y = texture.Image.GetTextureCoordY( tempRect.Y );
				texCoordBR.X = texture.Image.GetTextureCoordX( tempRect.X+tempRect.Width );
				texCoordBR.Y = texture.Image.GetTextureCoordY( tempRect.Y+tempRect.Height );
			}
			
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
			if (texture == null )
			{
				throw new ArgumentException("texture");
			}
			
			SpriteBatchItem item = _batcher.CreateBatchItem();
			
			item.Depth = 0;
			item.TextureID = (int) texture.ID;
			
			tempRect.X = 0;
			tempRect.Y = 0;
			tempRect.Width = texture.Width;
			tempRect.Height = texture.Height;
			
			if (texture.Image == null) {
				float texWidthRatio = 1.0f / (float)texture.Width;
				float texHeightRatio = 1.0f / (float)texture.Height;
				// We are initially flipped vertically so we need to flip the corners so that
				//  the image is bottom side up to display correctly
				texCoordTL.X = tempRect.X*texWidthRatio;
				texCoordTL.Y = (tempRect.Y+tempRect.Height) * texHeightRatio;
				
				texCoordBR.X = (tempRect.X+tempRect.Width)*texWidthRatio;
				texCoordBR.Y = tempRect.Y*texHeightRatio;
				
			}
			else {
				texCoordTL.X = texture.Image.GetTextureCoordX( tempRect.X );
				texCoordTL.Y = texture.Image.GetTextureCoordY( tempRect.Y );
				texCoordBR.X = texture.Image.GetTextureCoordX( tempRect.X+tempRect.Width );
				texCoordBR.Y = texture.Image.GetTextureCoordY( tempRect.Y+tempRect.Height );
			}
			
			item.Set 
				(
				 position.X,
			     position.Y,
				 tempRect.Width,
				 tempRect.Height,
				 color,
				 texCoordTL,
				 texCoordBR
				 );
		}
		
		public void Draw (Texture2D texture, Rectangle rectangle, Color color)
		{
			if (texture == null )
			{
				throw new ArgumentException("texture");
			}
			
			SpriteBatchItem item = _batcher.CreateBatchItem();
			
			item.Depth = 0;
			item.TextureID = (int) texture.ID;
			
			tempRect.X = 0;
			tempRect.Y = 0;
			tempRect.Width = texture.Width;
			tempRect.Height = texture.Height;			
			
			if (texture.Image == null) {
				float texWidthRatio = 1.0f / (float)texture.Width;
				float texHeightRatio = 1.0f / (float)texture.Height;
				// We are initially flipped vertically so we need to flip the corners so that
				//  the image is bottom side up to display correctly
				texCoordTL.X = tempRect.X*texWidthRatio;
				texCoordTL.Y = (tempRect.Y+tempRect.Height) * texHeightRatio;
				
				texCoordBR.X = (tempRect.X+tempRect.Width)*texWidthRatio;
				texCoordBR.Y = tempRect.Y*texHeightRatio;
				
			}
			else {
				texCoordTL.X = texture.Image.GetTextureCoordX( tempRect.X );
				texCoordTL.Y = texture.Image.GetTextureCoordY( tempRect.Y );
				texCoordBR.X = texture.Image.GetTextureCoordX( tempRect.X+tempRect.Width );
				texCoordBR.Y = texture.Image.GetTextureCoordY( tempRect.Y+tempRect.Height );
			}
			
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
			if (spriteFont == null )
			{
				throw new ArgumentException("spriteFont");
			}
			
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

				texCoordTL.X = spriteFont._texture.Image.GetTextureCoordX( g.Glyph.X );
				texCoordTL.Y = spriteFont._texture.Image.GetTextureCoordY( g.Glyph.Y );
				texCoordBR.X = spriteFont._texture.Image.GetTextureCoordX( g.Glyph.X+g.Glyph.Width );
				texCoordBR.Y = spriteFont._texture.Image.GetTextureCoordY( g.Glyph.Y+g.Glyph.Height );

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
			if (spriteFont == null )
			{
				throw new ArgumentException("spriteFont");
			}
			
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

				texCoordTL.X = spriteFont._texture.Image.GetTextureCoordX( g.Glyph.X );
				texCoordTL.Y = spriteFont._texture.Image.GetTextureCoordY( g.Glyph.Y );
				texCoordBR.X = spriteFont._texture.Image.GetTextureCoordX( g.Glyph.X+g.Glyph.Width );
				texCoordBR.Y = spriteFont._texture.Image.GetTextureCoordY( g.Glyph.Y+g.Glyph.Height );
				
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
			if (spriteFont == null )
			{
				throw new ArgumentException("spriteFont");
			}
			
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

				texCoordTL.X = spriteFont._texture.Image.GetTextureCoordX( g.Glyph.X );
				texCoordTL.Y = spriteFont._texture.Image.GetTextureCoordY( g.Glyph.Y );
				texCoordBR.X = spriteFont._texture.Image.GetTextureCoordX( g.Glyph.X+g.Glyph.Width );
				texCoordBR.Y = spriteFont._texture.Image.GetTextureCoordY( g.Glyph.Y+g.Glyph.Height );
				
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

