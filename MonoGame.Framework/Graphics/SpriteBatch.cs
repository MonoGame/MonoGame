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

using GL11 = OpenTK.Graphics.ES11.GL;
using GL20 = OpenTK.Graphics.ES20.GL;
using ALL11 = OpenTK.Graphics.ES11.All;
using ALL20 = OpenTK.Graphics.ES20.All;

using Microsoft.Xna.Framework;
using OpenTK;

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
		Matrix _matrix = Matrix.Identity;
#if ANDROID
		DisplayOrientation lastDisplayOrientation = DisplayOrientation.Unknown;
#endif
		
		Rectangle tempRect = new Rectangle(0,0,0,0);
		Vector2 texCoordTL = new Vector2(0,0);
		Vector2 texCoordBR = new Vector2(0,0);
		
		//OpenGLES2 variables
		int program;
		Matrix matWVPScreen, matWVPFramebuffer, matProjection, matViewScreen, matViewFramebuffer;
		int uniformWVP, uniformTex;
		
        public SpriteBatch ( GraphicsDevice graphicsDevice )
		{
			if (graphicsDevice == null )
			{
				throw new ArgumentException("graphicsDevice");
			}	
			
			this.graphicsDevice = graphicsDevice;
			
			_batcher = new SpriteBatcher();

#if IPHONE
			if(GraphicsDevice.OpenGLESVersion == MonoTouch.OpenGLES.EAGLRenderingAPI.OpenGLES2)
				InitGL20();
#elif ANDROID
            if (GraphicsDevice.OpenGLESVersion == OpenTK.Graphics.GLContextVersion.Gles2_0)
                InitGL20();
#endif
		}
		
			/// <summary>
			///Initialize shaders and program on OpenGLES2.0
			/// </summary>
			private void InitGL20()
			{
				string vertexShaderSrc = @" uniform mat4 uMVPMatrix;
											attribute vec4 aPosition;
											attribute vec2 aTexCoord;
											attribute vec4 aTint;
											varying vec2 vTexCoord;
											varying vec4 vTint;
											void main()
											{
												vTexCoord = aTexCoord;
												vTint = aTint;
												gl_Position = uMVPMatrix * aPosition;
											}";
            
            string fragmentShaderSrc = @"precision mediump float;
											varying vec2 vTexCoord;
											varying vec4 vTint;
											uniform sampler2D sTexture;
											void main()
											{
												vec4 baseColor = texture2D(sTexture, vTexCoord);
												gl_FragColor = baseColor * vTint;
											}";
				
				int vertexShader = LoadShader (ALL20.VertexShader, vertexShaderSrc );
	            int fragmentShader = LoadShader (ALL20.FragmentShader, fragmentShaderSrc );
			
	            program = GL20.CreateProgram();
			
	            if (program == 0)
	                throw new InvalidOperationException ("Unable to create program");
	
	            GL20.AttachShader (program, vertexShader);
	            GL20.AttachShader (program, fragmentShader);
	            
	            //Set position
	            GL20.BindAttribLocation (program, _batcher.attributePosition, "aPosition");
	            GL20.BindAttribLocation (program, _batcher.attributeTexCoord, "aTexCoord");
          		GL20.BindAttribLocation (program, _batcher.attributeTint, "aTint");
			
	            GL20.LinkProgram (program);
	
	            int linked = 0;
	            GL20.GetProgram (program, ALL20.LinkStatus, ref linked);
	            if (linked == 0) {
	                // link failed
	                int length = 0;
	                GL20.GetProgram (program, ALL20.InfoLogLength, ref length);
	                if (length > 0) {
	                    var log = new StringBuilder (length);
	                    GL20.GetProgramInfoLog (program, length, ref length, log);
#if DEBUG
	                    //Console.WriteLine ("GL2.0 error: " + log.ToString ());
#endif
	                }
	
	                GL20.DeleteProgram (program);
	                throw new InvalidOperationException ("Unable to link program");
	            }
	
			UpdateWorldMatrixOrientation();
				GetUniformVariables();
			
			}
	
			/// <summary>
			/// Build the shaders
			/// </summary>
			private int LoadShader ( ALL20 type, string source )
	        {
	           int shader = GL20.CreateShader(type);
	
	           if ( shader == 0 )
	                   throw new InvalidOperationException(string.Format(
					"Unable to create shader: {0}", GL20.GetError ()));
	        
	           // Load the shader source
	           int length = 0;
	            GL20.ShaderSource(shader, 1, new string[] {source}, (int[])null);
	           
	           // Compile the shader
	           GL20.CompileShader( shader );
	                
	              int compiled = 0;
	            GL20.GetShader (shader, ALL20.CompileStatus, ref compiled);
	            if (compiled == 0) {
	                length = 0;
	                GL20.GetShader (shader, ALL20.InfoLogLength, ref length);
	                if (length > 0) {
	                    var log = new StringBuilder (length);
	                    GL20.GetShaderInfoLog (shader, length, ref length, log);
#if DEBUG					
	                    Console.WriteLine("GL2" + log.ToString ());
#endif
	                }
	
	                GL20.DeleteShader (shader);
	                throw new InvalidOperationException ("Unable to compile shader of type : " + type.ToString ());
	            }
	
	            return shader;
	        
	        }
	
		private void GetUniformVariables()
		{
			uniformWVP = GL20.GetUniformLocation(program, "uMVPMatrix");
			uniformTex = GL20.GetUniformLocation(program, "sTexture");
		}
		
		private void SetUniformMatrix(int location, bool transpose, ref Matrix matrix)
		{
			unsafe
			{				
				fixed (float* matrix_ptr = &matrix.M11)
				{
					GL20.UniformMatrix4(location,1,transpose,matrix_ptr);
				}
			}
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
			// OpenGL ES Version 
#if IPHONE
			if(GraphicsDevice.OpenGLESVersion == MonoTouch.OpenGLES.EAGLRenderingAPI.OpenGLES2)
				EndGL20();
			else
				EndGL11();
#elif ANDROID
            if (GraphicsDevice.OpenGLESVersion == OpenTK.Graphics.GLContextVersion.Gles2_0)
                EndGL20();
            else
                EndGL11();
#else
            EndGL11();
#endif

		}
		
		private void EndGL20()
		{
			// Disable Blending by default = BlendState.Opaque
			GL20.Disable(ALL20.Blend);
			
			// set the blend mode
			if ( _blendState == BlendState.NonPremultiplied )
			{
				GL20.BlendFunc(ALL20.SrcAlpha, ALL20.OneMinusSrcAlpha);
				GL20.Enable(ALL20.Blend);
				GL20.BlendEquation(ALL20.FuncAdd);
			}
			
			if ( _blendState == BlendState.AlphaBlend )
			{
				GL20.BlendFunc(ALL20.One, ALL20.OneMinusSrcAlpha);
				GL20.Enable(ALL20.Blend);
				GL20.BlendEquation(ALL20.FuncAdd);
			}
			
			if ( _blendState == BlendState.Additive )
			{
				GL20.BlendFunc(ALL20.SrcAlpha,ALL20.One);
				GL20.Enable(ALL20.Blend);
				GL20.BlendEquation(ALL20.FuncAdd);
			}

			//CullMode
			GL20.FrontFace(ALL20.Cw);
			GL20.Enable(ALL20.CullFace);
			
			
			UpdateWorldMatrixOrientation();
			
			// Configure ViewPort
			var client = Game.Instance.Window.ClientBounds;
			GL20.Viewport(client.X, client.Y, client.Width, client.Height);
			GL20.UseProgram(program);
			
            // Enable Scissor Tests if necessary
            if (this.graphicsDevice.RasterizerState.ScissorTestEnable)
            {
                GL20.Enable(ALL20.ScissorTest);
                GL20.Scissor(this.graphicsDevice.ScissorRectangle.X, this.graphicsDevice.ScissorRectangle.Y, this.graphicsDevice.ScissorRectangle.Width, this.graphicsDevice.ScissorRectangle.Height);
            }
                        
			if (GraphicsDevice.DefaultFrameBuffer)
			{
				GL20.CullFace(ALL20.Back);
				SetUniformMatrix(uniformWVP, false, ref matWVPScreen);
			}
			else
			{
				GL20.CullFace(ALL20.Front);
				SetUniformMatrix(uniformWVP,false,ref matWVPFramebuffer);

				// FIXME: Why clear the framebuffer during End?
				//        Doing so makes it so that only the
				//        final Begin/End pair in a frame can
				//        ever be shown.  Is that desirable?
				//GL20.ClearColor(0.0f,0.0f,0.0f,0.0f);
				//GL20.Clear((int) (ALL20.ColorBufferBit | ALL20.DepthBufferBit));
			}

			_batcher.DrawBatchGL20 ( _sortMode, _samplerState );

            if (this.graphicsDevice.RasterizerState.ScissorTestEnable)
            {
                GL20.Disable(ALL20.ScissorTest);
            }

            GL20.Disable(ALL20.Texture2D);
		}
		
		public void EndGL11()
		{					
			// Disable Blending by default = BlendState.Opaque
			GL11.Disable(ALL11.Blend);
			
			// set the blend mode
			if ( _blendState == BlendState.NonPremultiplied )
			{
				GL11.BlendFunc(ALL11.SrcAlpha, ALL11.OneMinusSrcAlpha);
				GL11.Enable(ALL11.Blend);
			}
			
			if ( _blendState == BlendState.AlphaBlend )
			{
				GL11.BlendFunc(ALL11.One, ALL11.OneMinusSrcAlpha);
				GL11.Enable(ALL11.Blend);				
			}
			
			if ( _blendState == BlendState.Additive )
			{
				GL11.BlendFunc(ALL11.SrcAlpha,ALL11.One);
				GL11.Enable(ALL11.Blend);	
			}			
			
			// set camera
			GL11.MatrixMode(ALL11.Projection);
			GL11.LoadIdentity();							
			
#if ANDROID			
			// Switch on the flags.
	        switch (this.graphicsDevice.PresentationParameters.DisplayOrientation)
	        {
			
				case DisplayOrientation.LandscapeRight:
                {
					GL11.Rotate(180, 0, 0, 1); 
					GL11.Ortho(0, this.graphicsDevice.Viewport.Width, this.graphicsDevice.Viewport.Height,  0, -1, 1);
					break;
				}
				
				case DisplayOrientation.LandscapeLeft:
				case DisplayOrientation.PortraitUpsideDown:
                default:
				{
					GL11.Ortho(0, this.graphicsDevice.Viewport.Width, this.graphicsDevice.Viewport.Height, 0, -1, 1);
					break;
				}
			}					
#else			
			GL11.Ortho(0, this.graphicsDevice.Viewport.Width, this.graphicsDevice.Viewport.Height, 0, -1, 1);
#endif		
			
			// Enable Scissor Tests if necessary
			if ( this.graphicsDevice.RasterizerState.ScissorTestEnable )
			{
				GL11.Enable(ALL11.ScissorTest);
			}
			
			
			GL11.MatrixMode(ALL11.Modelview);			

			var client = Game.Instance.Window.ClientBounds;
			GL11.Viewport(client.X, client.Y, client.Width, client.Height);
			
			// Enable Scissor Tests if necessary
			if ( this.graphicsDevice.RasterizerState.ScissorTestEnable )
			{
				GL11.Scissor(this.graphicsDevice.ScissorRectangle.X, this.graphicsDevice.ScissorRectangle.Y, this.graphicsDevice.ScissorRectangle.Width, this.graphicsDevice.ScissorRectangle.Height );
			}			
			
			GL11.LoadMatrix( ref _matrix.M11 );	
			
			// Initialize OpenGL states (ideally move this to initialize somewhere else)	
			GL11.Disable(ALL11.DepthTest);
			GL11.TexEnv(ALL11.TextureEnv, ALL11.TextureEnvMode,(int) ALL11.SrcAlpha);
			GL11.Enable(ALL11.Texture2D);
			GL11.EnableClientState(ALL11.VertexArray);
			GL11.EnableClientState(ALL11.ColorArray);
			GL11.EnableClientState(ALL11.TextureCoordArray);
			
			// Enable Culling for better performance
			GL11.Enable(ALL11.CullFace);
			GL11.FrontFace(ALL11.Cw);
			GL11.Color4(1.0f, 1.0f, 1.0f, 1.0f);						
			
			_batcher.DrawBatchGL11(_sortMode, _samplerState);
			
			if (this.graphicsDevice.RasterizerState.ScissorTestEnable)
            {
               GL11.Disable(ALL11.ScissorTest);
            }
		}
		
#if ANDROID
		private void UpdateWorldMatrixOrientation()
		{
			// Configure Display Orientation:
			if(lastDisplayOrientation != graphicsDevice.PresentationParameters.DisplayOrientation)
			{
				bool update = true;
				// check the display width and height make sure it matches the target orientation
				// before updating the matrix
				if (graphicsDevice.PresentationParameters.DisplayOrientation == DisplayOrientation.Portrait)
				{
				   if (graphicsDevice.DisplayMode.Width > graphicsDevice.DisplayMode.Height) update = false;	
				}
				else
				{
					if (graphicsDevice.DisplayMode.Width < graphicsDevice.DisplayMode.Height) update = false;
				}
				if (update)
				{
					// updates last display orientation (optimization)				
					lastDisplayOrientation = graphicsDevice.PresentationParameters.DisplayOrientation;
	
	                var deviceManager = (IGraphicsDeviceManager)Game.Instance.Services.GetService(typeof(IGraphicsDeviceManager));
	                if (deviceManager == null)
	                    return;
	
	                (deviceManager as GraphicsDeviceManager).ResetClientBounds();
	
					// make sure the viewport is correct
					this.graphicsDevice.SetViewPort(Game.Instance.Window.ClientBounds.Width, Game.Instance.Window.ClientBounds.Height);
						
					/*
					AndroidGameActivity.Game.Log("--------------- Start Change -----------");
					AndroidGameActivity.Game.Log(String.Format("DisplayMode = {0}", this.graphicsDevice.DisplayMode.ToString()));
					AndroidGameActivity.Game.Log(String.Format("Orientation = {0}", this.graphicsDevice.PresentationParameters.DisplayOrientation.ToString()));
					AndroidGameActivity.Game.Log(String.Format("ViewPort = {0}", this.graphicsDevice.Viewport.ToString()));
					AndroidGameActivity.Game.Log(String.Format("ViewScreen = {0}", matViewScreen.ToString()));
					AndroidGameActivity.Game.Log(String.Format("Projection = {0}", matProjection.ToString()));
					AndroidGameActivity.Game.Log(String.Format("ViewFramebuffer = {0}", matViewFramebuffer.ToString()));
					AndroidGameActivity.Game.Log("--------------- End Change -------------");
					*/
				}
			}
			
			if (!GraphicsDevice.DefaultFrameBuffer)
			{
				matProjection = Matrix.CreateOrthographic(this.graphicsDevice.Viewport.Width,
							this.graphicsDevice.Viewport.Height,
							-1f,1f);
				// we are using a render target so update
				matViewFramebuffer = Matrix.CreateTranslation(-this.graphicsDevice.Viewport.Width/2,
							-this.graphicsDevice.Viewport.Height/2,
							1);	
				
				matWVPFramebuffer = _matrix * matViewFramebuffer *  matProjection;
			}
			else
			{
					matViewScreen = Matrix.CreateRotationZ((float)Math.PI)*
								     	Matrix.CreateRotationY((float)Math.PI)*
										Matrix.CreateTranslation(-this.graphicsDevice.Viewport.Width/2,
										this.graphicsDevice.Viewport.Height/2,
										1);
					matProjection = Matrix.CreateOrthographic(this.graphicsDevice.Viewport.Width,
								this.graphicsDevice.Viewport.Height,
								-1f,1f);
					if (graphicsDevice.PresentationParameters.DisplayOrientation == DisplayOrientation.LandscapeRight)
					{
						// flip the viewport	
						matProjection = Matrix.CreateOrthographic(-this.graphicsDevice.Viewport.Width,
								-this.graphicsDevice.Viewport.Height,
								-1f,1f);
					}
						
					matWVPScreen = _matrix * matViewScreen * matProjection;								    
			}
			
			
		}
#else		
		private void UpdateWorldMatrixOrientation()
		{
			// TODO: It may be desirable to do a last display
			//       orientation here like Android does, to
			//       avoid calculating these matrices again
			//       and again.
			var viewport = graphicsDevice.Viewport;
			matViewScreen =
				Matrix.CreateRotationZ((float)Math.PI) *
				Matrix.CreateRotationY((float)Math.PI) *
				Matrix.CreateTranslation(-viewport.Width / 2, viewport.Height / 2, 1);

			matViewFramebuffer = Matrix.CreateTranslation(-viewport.Width / 2, -viewport.Height/2, 1);

			matProjection = Matrix.CreateOrthographic(viewport.Width, viewport.Height, -1f, 1f);

			// FIXME: It is a significant weakness to have separate
			//        matrices for the screen and framebuffer.  It
			//        means that visual unit tests can't
			//        necessarily be trusted.  Screens are just
			//        another kind of framebuffer.
			matWVPScreen = _matrix * matViewScreen * matProjection;
			matWVPFramebuffer = _matrix * matViewFramebuffer * matProjection;
		}
#endif		
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
				//texCoordTL.Y = (tempRect.Y + tempRect.Height) * texHeightRatio;
				texCoordTL.Y = 1.0f - tempRect.Y * texHeightRatio;
				
				texCoordBR.X = (tempRect.X+tempRect.Width)*texWidthRatio;
				//texCoordBR.Y = tempRect.Y * texHeightRatio;
				texCoordBR.Y = 1.0f - ( tempRect.Y+tempRect.Height )*texHeightRatio;
				
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
				//texCoordTL.Y = (tempRect.Y + tempRect.Height) * texHeightRatio;
				texCoordTL.Y = 1.0f - tempRect.Y * texHeightRatio;
				
				texCoordBR.X = (tempRect.X+tempRect.Width)*texWidthRatio;
				//texCoordBR.Y = tempRect.Y * texHeightRatio;
				texCoordBR.Y = 1.0f - ( tempRect.Y+tempRect.Height )*texHeightRatio;
				
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
				//texCoordTL.Y = (tempRect.Y + tempRect.Height) * texHeightRatio;
				texCoordTL.Y = 1.0f - tempRect.Y * texHeightRatio;
				
				texCoordBR.X = (tempRect.X+tempRect.Width)*texWidthRatio;
				//texCoordBR.Y = tempRect.Y * texHeightRatio;
				texCoordBR.Y = 1.0f - ( tempRect.Y+tempRect.Height )*texHeightRatio;
				
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
				 -origin.X * ((float)destinationRectangle.Width  / (float)tempRect.Width),  //JEPJ
		         -origin.Y * ((float)destinationRectangle.Height / (float)tempRect.Height), //JEPJ 
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
				//texCoordTL.Y = (tempRect.Y + tempRect.Height) * texHeightRatio;
				texCoordTL.Y = 1.0f - tempRect.Y * texHeightRatio;
				
				texCoordBR.X = (tempRect.X+tempRect.Width)*texWidthRatio;
				//texCoordBR.Y = tempRect.Y * texHeightRatio;
				texCoordBR.Y = 1.0f - ( tempRect.Y+tempRect.Height )*texHeightRatio;
				
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
				//texCoordTL.Y = (tempRect.Y + tempRect.Height) * texHeightRatio;
				texCoordTL.Y = 1.0f - tempRect.Y * texHeightRatio;
				
				texCoordBR.X = (tempRect.X+tempRect.Width)*texWidthRatio;
				//texCoordBR.Y = tempRect.Y * texHeightRatio;
				texCoordBR.Y = 1.0f - ( tempRect.Y+tempRect.Height )*texHeightRatio;
				
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
				//texCoordTL.Y = (tempRect.Y + tempRect.Height) * texHeightRatio;
				texCoordTL.Y = 1.0f - tempRect.Y * texHeightRatio;
				
				texCoordBR.X = (tempRect.X+tempRect.Width)*texWidthRatio;
				//texCoordBR.Y = tempRect.Y * texHeightRatio;
				texCoordBR.Y = 1.0f - ( tempRect.Y+tempRect.Height )*texHeightRatio;
				
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
				//texCoordTL.Y = (tempRect.Y + tempRect.Height) * texHeightRatio;
				texCoordTL.Y = 1.0f - tempRect.Y * texHeightRatio;
				
				texCoordBR.X = (tempRect.X+tempRect.Width)*texWidthRatio;
				//texCoordBR.Y = tempRect.Y * texHeightRatio;
				texCoordBR.Y = 1.0f - ( tempRect.Y+tempRect.Height )*texHeightRatio;
				
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

