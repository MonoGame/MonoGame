using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif WINRT

#else
using OpenTK.Graphics.ES20;

#if IPHONE || ANDROID
using ActiveUniformType = OpenTK.Graphics.ES20.All;
using ShaderType = OpenTK.Graphics.ES20.All;
using ProgramParameter = OpenTK.Graphics.ES20.All;
#endif
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public class EffectPass
    {
        EffectTechnique _technique = null;
		GraphicsDevice _graphicsDevice;
		
		string name;
		int shaderProgram = 0;
#if WINRT

#else
		DXEffectObject.d3dx_state[] states;
		DXShader pixelShader;
		DXShader vertexShader;

		GLSLEffectObject.glsl_state[] glslStates;
		GLSLShader glslPixelShader;
		GLSLShader glslVertexShader;
		bool isGLSL = false;
#endif

		bool setBlendState = false;
		BlendState blendState;
		bool setDepthStencilState = false;
		DepthStencilState depthStencilState;
		bool setRasterizerState = false;
		RasterizerState rasterizerState;

		static float[] posFixup = new float[4];

		static string passthroughVertexShaderSrc = @"
				uniform mat4 transformMatrix;
				uniform vec4 posFixup;

				attribute vec4 aPosition;
				attribute vec4 aTexCoord;
				attribute vec4 aColor;

				varying vec4 vTexCoord0;
				varying vec4 vFrontColor;
				void main() {
					vTexCoord0.xy = aTexCoord.xy;
					vFrontColor = aColor;

					gl_Position = transformMatrix * aPosition;

					gl_Position.y = gl_Position.y * posFixup.y;
					gl_Position.xy += posFixup.zw * gl_Position.ww;
				}";
		internal static int? passthroughVertexShader;

		bool passthroughVertexShaderAttached = false;

#if NOMOJO
        public EffectPass(EffectTechnique technique)
        {
            _technique = technique;

            _graphicsDevice = _technique._effect.GraphicsDevice;

            blendState = new BlendState();
            depthStencilState = new DepthStencilState();
            rasterizerState = new RasterizerState();
        }

        public void Apply()
        {
            // Set/get the correct shader handle/cleanups.
			_technique._effect.OnApply();

            var effect = _technique._effect;

            var currentProgram = effect.shaderIndexLookupTable[effect.Parameters["ShaderIndex"].GetValueInt32()];

            // No work to do if we don't need to switch shaders.
            //if (currentProgram == _technique._effect.CurrentProgram && shaderProgram != 0)
               //return;

            shaderProgram = _technique._effect.CurrentProgram = currentProgram;

            GL.UseProgram(shaderProgram);

            if (setRasterizerState)
                _graphicsDevice.RasterizerState = rasterizerState;

            if (setBlendState)
                _graphicsDevice.BlendState = blendState;

            if (setDepthStencilState)
                _graphicsDevice.DepthStencilState = depthStencilState;

            glslVertexShader = Effect.shaderObjectLookup[shaderProgram][0];

            // Set Program Attributes.
            int location = 0;
            //bind attributes
            foreach (var attribute in glslVertexShader.attributeList)
            {
                if (attribute.Contains("Position"))
                    location = GraphicsDevice.attributePosition;
                else if (attribute.Contains("Normal"))
                    location = GraphicsDevice.attributeNormal;
                else if (attribute.Contains("TextureCoordinate"))
                    location = GraphicsDevice.attributeTexCoord;
                else if (attribute.Contains("Color"))
                    location = GraphicsDevice.attributeColor;
                else
                    throw new NotSupportedException();

                GL.BindAttribLocation(shaderProgram, location, attribute);
            }

            var samplerState = _graphicsDevice.SamplerStates[0];
            // Set Program Uniforms
            foreach (var param in _technique._effect.Parameters)
            {
                int uniformLocation = GL.GetUniformLocation(shaderProgram, param.Name);

                var error = GL.GetError();

                if (uniformLocation == -1 || param.Name.Contains("ShaderIndex"))
                    continue;

                switch (param.activeUniformType)
                {
                    case ActiveUniformType.FloatVec2:
                        var v2 = param.GetValueVector2();
                        GL.Uniform2(uniformLocation, v2.X, v2.Y);
                        break;

                    case ActiveUniformType.FloatVec3:
                        var v3 = param.GetValueVector3();
                        GL.Uniform3(uniformLocation, v3.X, v3.Y, v3.Z);
                        break;

                    case ActiveUniformType.FloatVec4:
                        var v4 = param.GetValueVector4();
                        GL.Uniform4(uniformLocation, v4.X, v4.Y, v4.Z, v4.W);
                        break;

                    case ActiveUniformType.Float:
                        GL.Uniform1(uniformLocation, param.GetValueSingle());
                        break;

                        // RAY TODO: Temporary. Can handle data as matrix...?
                    case ActiveUniformType.FloatMat4:
                        var mat4 = (Matrix)param.data;
                        var sa4 = new float[] { mat4.M11, mat4.M12, mat4.M13, mat4.M14, mat4.M21, mat4.M22, mat4.M23, mat4.M24, mat4.M31, mat4.M32, mat4.M33, mat4.M34, mat4.M41, mat4.M42, mat4.M43, mat4.M44 };
                        GL.UniformMatrix4(uniformLocation, 1, false, sa4);
                        break;

                    case ActiveUniformType.FloatMat3:
                        var mat = (Matrix)param.data;
                        var mat3 = new float[] { mat.M11, mat.M12, mat.M13, mat.M21, mat.M22, mat.M23, mat.M31, mat.M32, mat.M33 };
                        GL.UniformMatrix3(uniformLocation, 1, false, mat3);
                        break;

                    case ActiveUniformType.Bool:
                        GL.Uniform1(uniformLocation, (param.GetValueBoolean()) ? 1 : 0);
                        break;

                    case ActiveUniformType.Int:
                        GL.Uniform1(uniformLocation, param.GetValueInt32());
                        break;

                    case ActiveUniformType.Sampler2D:
                        Texture tex = (Texture)param.data;
                        tex.Activate();
                        samplerState.Activate(tex.glTarget, tex.LevelCount > 1);
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
        }
#else
        public void Apply()
		{
            // Set/get the correct shader handle/cleanups.
			_technique._effect.OnApply();

#if WINRT

#else
			if (isGLSL) {
				GLSLApply();
				return;
			}

			//Console.WriteLine (_technique._effect.Name+" - "+_technique.Name+" - "+Name);
			bool relink = false;
			foreach ( DXEffectObject.d3dx_state state in states) {
				
				//constants handled on init
				if (state.type == DXEffectObject.STATE_TYPE.CONSTANT) continue;
				
				if (state.operation.class_ == DXEffectObject.STATE_CLASS.PIXELSHADER ||
					state.operation.class_ == DXEffectObject.STATE_CLASS.VERTEXSHADER) {
					
					DXShader shader;
					switch (state.type) {
					case DXEffectObject.STATE_TYPE.EXPRESSIONINDEX:
						shader = (DXShader) (((DXExpression)state.parameter.data)
							.Evaluate (_technique._effect.Parameters));
						break;
					case DXEffectObject.STATE_TYPE.PARAMETER:
						//should be easy, but haven't seen it
					default:
						throw new NotImplementedException();
					}
					
					if (shader.shaderType == ShaderType.FragmentShader) {
						if (shader != pixelShader) {
							if (pixelShader != null) {
                                GL.DetachShader(shaderProgram, pixelShader.shaderHandle);
							}
							relink = true;
							pixelShader = shader;
                            GL.AttachShader(shaderProgram, pixelShader.shaderHandle);
						}
					} else if (shader.shaderType == ShaderType.VertexShader) {
						if (shader != vertexShader) {
							if (vertexShader != null) {
                                GL.DetachShader(shaderProgram, vertexShader.shaderHandle);
							}
							relink = true;
							vertexShader = shader;
                            GL.AttachShader(shaderProgram, vertexShader.shaderHandle);
						}
					}
					
				}
				
			}
			
			if (relink) {
				Link ();
			}
			
			GL.UseProgram (shaderProgram);
			
			
			if (setRasterizerState) {
				_graphicsDevice.RasterizerState = rasterizerState;
			}
			if (setBlendState) {
				_graphicsDevice.BlendState = blendState;
			}
			if (setDepthStencilState) {
				_graphicsDevice.DepthStencilState = depthStencilState;
			}

			if (vertexShader != null) {
				vertexShader.Apply(shaderProgram,
				                  _technique._effect.Parameters,
				                  _graphicsDevice);
			} else {
				//passthrough shader is attached
				Viewport vp = _graphicsDevice.Viewport;
				Matrix projection = Matrix.CreateOrthographicOffCenter(0, vp.Width, vp.Height, 0, 0, 1);
				Matrix halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);
				Matrix transform = halfPixelOffset * projection;

				int uniform = GL.GetUniformLocation(shaderProgram, "transformMatrix");
				GL.UniformMatrix4(uniform, 1, false, Matrix.ToFloatArray(transform));
			}


			// Apply vertex shader fix:
			// The following two lines are appended to the end of vertex shaders
			// to account for rendering differences between OpenGL and DirectX:
			//
			// gl_Position.y = gl_Position.y * posFixup.y;
			// gl_Position.xy += posFixup.zw * gl_Position.ww;
			//
			// (the following paraphrased from wine, wined3d/state.c and wined3d/glsl_shader.c)
			//
			// - We need to flip along the y-axis in case of offscreen rendering.
			// - D3D coordinates refer to pixel centers while GL coordinates refer
			//   to pixel corners.
			// - D3D has a top-left filling convention. We need to maintain this
			//   even after the y-flip mentioned above.
			// In order to handle the last two points, we translate by
			// (63.0 / 128.0) / VPw and (63.0 / 128.0) / VPh. This is equivalent to
			// translating slightly less than half a pixel. We want the difference to
			// be large enough that it doesn't get lost due to rounding inside the
			// driver, but small enough to prevent it from interfering with any
			// anti-aliasing.
			//
			// OpenGL coordinates specify the center of the pixel while d3d coords specify
			// the corner. The offsets are stored in z and w in posFixup. posFixup.y contains
			// 1.0 or -1.0 to turn the rendering upside down for offscreen rendering. PosFixup.x
			// contains 1.0 to allow a mad.

			posFixup[0] = 1.0f;
			posFixup[1] = 1.0f;
			posFixup[2] = (63.0f / 64.0f) / _graphicsDevice.Viewport.Width;
			posFixup[3] = -(63.0f / 64.0f) / _graphicsDevice.Viewport.Height;
			//If we have a render target bound (rendering offscreen)
			if (_graphicsDevice.GetRenderTargets().Length > 0) {
				//flip vertically
				posFixup[1] *= -1.0f;
				posFixup[3] *= -1.0f;
			}
			int posFixupLoc = GL.GetUniformLocation(shaderProgram, "posFixup");
			GL.Uniform4 (posFixupLoc, 1, posFixup);

			

			if (pixelShader != null) {
				pixelShader.Apply(shaderProgram,
				                  _technique._effect.Parameters,
				                  _graphicsDevice);
			}

#endif
		}
#endif // NOMOJO


#if WINRT

#else
		public EffectPass(EffectTechnique technique, DXEffectObject.d3dx_pass pass)
        {
            _technique = technique;
			_graphicsDevice = _technique._effect.GraphicsDevice;
			
			name = pass.name;
			states = pass.states;
			
			blendState = new BlendState();
			depthStencilState = new DepthStencilState();
			rasterizerState = new RasterizerState();
			
			Console.WriteLine (technique.Name);
            Threading.Begin();
            try
            {
                shaderProgram = GL.CreateProgram();

                // Set the parameters
                //is this nesesary, or just for VR?
                /*GL.ProgramParameter (shaderProgram,
                    AssemblyProgramParameterArb.GeometryInputType,(int)All.Lines);
                GL.ProgramParameter (shaderProgram,
                    AssemblyProgramParameterArb.GeometryOutputType, (int)All.Line);*/

                // Set the max vertices
                /*int maxVertices;
                GL.GetInteger (GetPName.MaxGeometryOutputVertices, out maxVertices);
                GL.ProgramParameter (shaderProgram,
                    AssemblyProgramParameterArb.GeometryVerticesOut, maxVertices);*/

                bool needPixelShader = false;
                bool needVertexShader = false;
                foreach (DXEffectObject.d3dx_state state in states)
                {
                    if (state.operation.class_ == DXEffectObject.STATE_CLASS.PIXELSHADER)
                    {
                        needPixelShader = true;
                        if (state.type == DXEffectObject.STATE_TYPE.CONSTANT)
                        {
                            pixelShader = (DXShader)state.parameter.data;
                            GL.AttachShader(shaderProgram, pixelShader.shaderHandle);
                        }
                    }
                    else if (state.operation.class_ == DXEffectObject.STATE_CLASS.VERTEXSHADER)
                    {
                        needVertexShader = true;
                        if (state.type == DXEffectObject.STATE_TYPE.CONSTANT)
                        {
                            vertexShader = (DXShader)state.parameter.data;
                            GL.AttachShader(shaderProgram, vertexShader.shaderHandle);
                        }
                    }
                    else if (state.operation.class_ == DXEffectObject.STATE_CLASS.RENDERSTATE)
                    {
                        if (state.type != DXEffectObject.STATE_TYPE.CONSTANT)
                        {
                            throw new NotImplementedException();
                        }
                        switch (state.operation.op)
                        {
                            case (uint)DXEffectObject.D3DRENDERSTATETYPE.STENCILENABLE:
                                depthStencilState.StencilEnable = (bool)state.parameter.data;
                                setDepthStencilState = true;
                                break;
                            case (uint)DXEffectObject.D3DRENDERSTATETYPE.SCISSORTESTENABLE:
                                rasterizerState.ScissorTestEnable = (bool)state.parameter.data;
                                setRasterizerState = true;
                                break;
                            case (uint)DXEffectObject.D3DRENDERSTATETYPE.BLENDOP:
                                blendState.ColorBlendFunction = (BlendFunction)state.parameter.data;
                                setBlendState = true;
                                break;
                            case (uint)DXEffectObject.D3DRENDERSTATETYPE.SRCBLEND:
                                blendState.ColorSourceBlend = (Blend)state.parameter.data;
                                setBlendState = true;
                                break;
                            case (uint)DXEffectObject.D3DRENDERSTATETYPE.DESTBLEND:
                                blendState.ColorDestinationBlend = (Blend)state.parameter.data;
                                setBlendState = true;
                                break;
                            case (uint)DXEffectObject.D3DRENDERSTATETYPE.ALPHABLENDENABLE:
                                break; //not sure what to do
                            case (uint)DXEffectObject.D3DRENDERSTATETYPE.CULLMODE:
                                rasterizerState.CullMode = (CullMode)state.parameter.data;
                                setRasterizerState = true;
                                break;
                            case (uint)DXEffectObject.D3DRENDERSTATETYPE.COLORWRITEENABLE:
                                blendState.ColorWriteChannels = (ColorWriteChannels)state.parameter.data;
                                setBlendState = true;
                                break;
                            case (uint)DXEffectObject.D3DRENDERSTATETYPE.STENCILFUNC:
                                depthStencilState.StencilFunction = (CompareFunction)state.parameter.data;
                                setDepthStencilState = true;
                                break;
                            case (uint)DXEffectObject.D3DRENDERSTATETYPE.STENCILPASS:
                                depthStencilState.StencilPass = (StencilOperation)state.parameter.data;
                                setDepthStencilState = true;
                                break;
                            case (uint)DXEffectObject.D3DRENDERSTATETYPE.STENCILFAIL:
                                depthStencilState.StencilFail = (StencilOperation)state.parameter.data;
                                setDepthStencilState = true;
                                break;
                            case (uint)DXEffectObject.D3DRENDERSTATETYPE.STENCILREF:
                                depthStencilState.ReferenceStencil = (int)state.parameter.data;
                                setDepthStencilState = true;
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }

                //If we have what we need, link now
                if ((needPixelShader == (pixelShader != null)) &&
                        (needVertexShader == (vertexShader != null)))
                {
                    Link();
                }
            }
            finally
            {
                Threading.End();
            }
        }

		public EffectPass(EffectTechnique technique, GLSLEffectObject.glslPass pass)
		{
			isGLSL = true;
			_technique = technique;
			_graphicsDevice = _technique._effect.GraphicsDevice;

			name = pass.name;
			glslStates = pass.states;

			blendState = new BlendState();
			depthStencilState = new DepthStencilState();
			rasterizerState = new RasterizerState();

			Console.WriteLine ("GLSL: " + technique.Name);

			shaderProgram = GL.CreateProgram ();

			// Set the parameters
			//is this nesesary, or just for VR?
			/*GL.ProgramParameter (shaderProgram,
				AssemblyProgramParameterArb.GeometryInputType,(int)All.Lines);
			GL.ProgramParameter (shaderProgram,
				AssemblyProgramParameterArb.GeometryOutputType, (int)All.Line);*/

			// Set the max vertices
			/*int maxVertices;
			GL.GetInteger (GetPName.MaxGeometryOutputVertices, out maxVertices);
			GL.ProgramParameter (shaderProgram,
				AssemblyProgramParameterArb.GeometryVerticesOut, maxVertices);*/

			bool needPixelShader = false;
			bool needVertexShader = false;
			foreach ( GLSLEffectObject.glsl_state state in glslStates) {
				if (state.operation.class_ == GLSLEffectObject.STATE_CLASS.PIXELSHADER) {
					needPixelShader = true;
					if (state.type == GLSLEffectObject.STATE_TYPE.CONSTANT) {
						glslPixelShader = ((GLSLEffectObject.ShaderProg)state.parameter.data).glslShaderObject;
                        GL.AttachShader(shaderProgram, glslPixelShader.shaderHandle);
					}
				} else if (state.operation.class_ == GLSLEffectObject.STATE_CLASS.VERTEXSHADER) {
					needVertexShader = true;
					if (state.type == GLSLEffectObject.STATE_TYPE.CONSTANT) {
						glslVertexShader = ((GLSLEffectObject.ShaderProg)state.parameter.data).glslShaderObject;
                        GL.AttachShader(shaderProgram, glslVertexShader.shaderHandle);
					}
				} else if (state.operation.class_ == GLSLEffectObject.STATE_CLASS.RENDERSTATE) {
					if (state.type != GLSLEffectObject.STATE_TYPE.CONSTANT) {
						throw new NotImplementedException();
					}
					switch (state.operation.op) {
					case (uint)GLSLEffectObject.GLSLRENDERSTATETYPE.STENCILENABLE:
						depthStencilState.StencilEnable = (bool)state.parameter.data;
						setDepthStencilState = true;
						break;
					case (uint)GLSLEffectObject.GLSLRENDERSTATETYPE.SCISSORTESTENABLE:
						rasterizerState.ScissorTestEnable = (bool)state.parameter.data;
						setRasterizerState = true;
						break;
					case (uint)GLSLEffectObject.GLSLRENDERSTATETYPE.BLENDOP:
						blendState.ColorBlendFunction = (BlendFunction)state.parameter.data;
						setBlendState = true;
						break;
					case (uint)GLSLEffectObject.GLSLRENDERSTATETYPE.SRCBLEND:
						blendState.ColorSourceBlend = (Blend)state.parameter.data;
						setBlendState = true;
						break;
					case (uint)GLSLEffectObject.GLSLRENDERSTATETYPE.DESTBLEND:
						blendState.ColorDestinationBlend = (Blend)state.parameter.data;
						setBlendState = true;
						break;
					case (uint)GLSLEffectObject.GLSLRENDERSTATETYPE.ALPHABLENDENABLE:
						break; //not sure what to do
					case (uint)GLSLEffectObject.GLSLRENDERSTATETYPE.CULLMODE:
						rasterizerState.CullMode = (CullMode)state.parameter.data;
						setRasterizerState = true;
						break;
					case (uint)GLSLEffectObject.GLSLRENDERSTATETYPE.COLORWRITEENABLE:
						blendState.ColorWriteChannels = (ColorWriteChannels)state.parameter.data;
						setBlendState = true;
						break;
					case (uint)GLSLEffectObject.GLSLRENDERSTATETYPE.STENCILFUNC:
						depthStencilState.StencilFunction = (CompareFunction)state.parameter.data;
						setDepthStencilState = true;
						break;
					case (uint)GLSLEffectObject.GLSLRENDERSTATETYPE.STENCILPASS:
						depthStencilState.StencilPass = (StencilOperation)state.parameter.data;
						setDepthStencilState = true;
						break;
					case (uint)GLSLEffectObject.GLSLRENDERSTATETYPE.STENCILFAIL:
						depthStencilState.StencilFail = (StencilOperation)state.parameter.data;
						setDepthStencilState = true;
						break;
					case (uint)GLSLEffectObject.GLSLRENDERSTATETYPE.STENCILREF:
						depthStencilState.ReferenceStencil = (int)state.parameter.data;
						setDepthStencilState = true;
						break;
					default:
						throw new NotImplementedException();
					}
				} else {
					throw new NotImplementedException();
				}
			}
			
			//If we have what we need, link now
			if ( (needPixelShader == (glslPixelShader != null)) &&
				 (needVertexShader == (glslVertexShader != null))) {
				glslLink();
			}

        }
#endif

        private void Link ()
		{
#if WINRT

#else
			if (vertexShader == null && !passthroughVertexShaderAttached) {
				if (!passthroughVertexShader.HasValue) {
					int shader = GL.CreateShader(ShaderType.VertexShader);
#if IPHONE || ANDROID
					GL.ShaderSource (shader, 1,
					                new string[]{passthroughVertexShaderSrc}, (int[])null);
#else
					GL.ShaderSource(shader, passthroughVertexShaderSrc);
#endif

					GL.CompileShader(shader);

					passthroughVertexShader = shader;
				}

				GL.AttachShader(shaderProgram, passthroughVertexShader.Value);
#if !ES11
				GL.BindAttribLocation(shaderProgram, GraphicsDevice.attributePosition, "aPosition");
				GL.BindAttribLocation(shaderProgram, GraphicsDevice.attributeTexCoord, "aTexCoord");
				GL.BindAttribLocation(shaderProgram, GraphicsDevice.attributeColor, "aColor");
#endif

				passthroughVertexShaderAttached = true;
			} else if (vertexShader != null && passthroughVertexShaderAttached) {
				GL.DetachShader(shaderProgram, passthroughVertexShader.Value);
				passthroughVertexShaderAttached = false;
			}

			if (vertexShader != null) {
				vertexShader.OnLink(shaderProgram);
			}
			if (pixelShader != null) {
				pixelShader.OnLink (shaderProgram);
			}

			GL.LinkProgram (shaderProgram);

			int linked = 0;
#if IPHONE || ANDROID
			GL.GetProgram (shaderProgram, ProgramParameter.LinkStatus, ref linked);
#else
			GL.GetProgram (shaderProgram, ProgramParameter.LinkStatus, out linked);
#endif
			if (linked == 0) {
#if !IPHONE && !ANDROID
				string log = GL.GetProgramInfoLog(shaderProgram);
				Console.WriteLine (log);
#endif
				throw new InvalidOperationException("Unable to link effect program");
			}
#endif
		}
		
		
		
		public string Name { get { return name; } }

#if !WINRT
		private void glslLink ()
		{
			if (glslVertexShader == null && !passthroughVertexShaderAttached) {
				if (!passthroughVertexShader.HasValue) {
					int shader = GL.CreateShader(ShaderType.VertexShader);
#if IPHONE || ANDROID
					GL.ShaderSource (shader, 1,
					                new string[]{passthroughVertexShaderSrc}, (int[])null);
#else
					GL.ShaderSource(shader, passthroughVertexShaderSrc);
#endif

					GL.CompileShader(shader);

					passthroughVertexShader = shader;
				}

				GL.AttachShader(shaderProgram, passthroughVertexShader.Value);
#if !ES11
				GL.BindAttribLocation(shaderProgram, GraphicsDevice.attributePosition, "aPosition");
				GL.BindAttribLocation(shaderProgram, GraphicsDevice.attributeTexCoord, "aTexCoord");
				GL.BindAttribLocation(shaderProgram, GraphicsDevice.attributeColor, "aColor");
#endif

				passthroughVertexShaderAttached = true;
			} else if (glslVertexShader != null && passthroughVertexShaderAttached) {
				GL.DetachShader(shaderProgram, passthroughVertexShader.Value);
				passthroughVertexShaderAttached = false;
			}

			if (glslVertexShader != null) {
				glslVertexShader.OnLink(shaderProgram);
			}
			if (glslPixelShader != null) {
				glslPixelShader.OnLink (shaderProgram);
			}

			GL.LinkProgram (shaderProgram);

			int linked = 0;
#if IPHONE || ANDROID
			GL.GetProgram (shaderProgram, ProgramParameter.LinkStatus, ref linked);
#else
			GL.GetProgram (shaderProgram, ProgramParameter.LinkStatus, out linked);
#endif
			if (linked == 0) {
#if !IPHONE && !ANDROID
				string log = GL.GetProgramInfoLog(shaderProgram);
				Console.WriteLine (log);
#endif
				throw new InvalidOperationException("Unable to link effect program");
			}

		}

		public void GLSLApply ()
		{
#if DEBUG
			Console.WriteLine (_technique._effect.Name+" - GLSL -> "+_technique.Name+" - "+Name);
#endif
			bool relink = false;
			foreach ( GLSLEffectObject.glsl_state state in glslStates) {

				//constants handled on init
				if (state.type == GLSLEffectObject.STATE_TYPE.CONSTANT) continue;

				if (state.operation.class_ == GLSLEffectObject.STATE_CLASS.PIXELSHADER ||
					state.operation.class_ == GLSLEffectObject.STATE_CLASS.VERTEXSHADER) {

					GLSLShader shader;
					shader = ((GLSLEffectObject.ShaderProg)state.parameter.data).glslShaderObject;

//					switch (state.type) {
//					case DXEffectObject.STATE_TYPE.EXPRESSIONINDEX:
//						shader = (DXShader) (((DXExpression)state.parameter.data)
//							.Evaluate (_technique._effect.Parameters));
//						break;
//					case DXEffectObject.STATE_TYPE.PARAMETER:
//						//should be easy, but haven't seen it
//					default:
//						throw new NotImplementedException();
//					}

					if (shader.shaderType == ShaderType.FragmentShader) {
						if (shader != glslPixelShader) {
							if (glslPixelShader != null) {
								GL.DetachShader (shaderProgram, glslPixelShader.shaderHandle);
							}
							relink = true;
							glslPixelShader = shader;
                            GL.AttachShader(shaderProgram, glslPixelShader.shaderHandle);
						}
					} else if (shader.shaderType == ShaderType.VertexShader) {
						if (shader != glslVertexShader) {
							if (glslVertexShader != null) {
                                GL.DetachShader(shaderProgram, glslVertexShader.shaderHandle);
							}
							relink = true;
							glslVertexShader = shader;
                            GL.AttachShader(shaderProgram, glslVertexShader.shaderHandle);
						}
					}

				}
				
			}
			
			if (relink)
				Link ();
			
			GL.UseProgram (shaderProgram);
			
			if (setRasterizerState) {
				_graphicsDevice.RasterizerState = rasterizerState;
			}
			if (setBlendState) {
				_graphicsDevice.BlendState = blendState;
			}
			if (setDepthStencilState) {
				_graphicsDevice.DepthStencilState = depthStencilState;
			}

			if (glslVertexShader != null) {
//				glslVertexShader.Apply(shaderProgram,
//				                  _technique._effect.Parameters,
//				                  _graphicsDevice);
			} else {
				//passthrough shader is attached
				Viewport vp = _graphicsDevice.Viewport;
				Matrix projection = Matrix.CreateOrthographicOffCenter(0, vp.Width, vp.Height, 0, 0, 1);
				Matrix halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);
				Matrix transform = halfPixelOffset * projection;

				int uniform = GL.GetUniformLocation(shaderProgram, "transformMatrix");
				GL.UniformMatrix4(uniform, 1, false, Matrix.ToFloatArray(transform));
			}

			// Apply vertex shader fix:
			// The following two lines are appended to the end of vertex shaders
			// to account for rendering differences between OpenGL and DirectX:
			//
			// gl_Position.y = gl_Position.y * posFixup.y;
			// gl_Position.xy += posFixup.zw * gl_Position.ww;
			//
			// (the following paraphrased from wine, wined3d/state.c and wined3d/glsl_shader.c)
			//
			// - We need to flip along the y-axis in case of offscreen rendering.
			// - D3D coordinates refer to pixel centers while GL coordinates refer
			//   to pixel corners.
			// - D3D has a top-left filling convention. We need to maintain this
			//   even after the y-flip mentioned above.
			// In order to handle the last two points, we translate by
			// (63.0 / 128.0) / VPw and (63.0 / 128.0) / VPh. This is equivalent to
			// translating slightly less than half a pixel. We want the difference to
			// be large enough that it doesn't get lost due to rounding inside the
			// driver, but small enough to prevent it from interfering with any
			// anti-aliasing.
			//
			// OpenGL coordinates specify the center of the pixel while d3d coords specify
			// the corner. The offsets are stored in z and w in posFixup. posFixup.y contains
			// 1.0 or -1.0 to turn the rendering upside down for offscreen rendering. PosFixup.x
			// contains 1.0 to allow a mad.

			posFixup[0] = 1.0f;
			posFixup[1] = 1.0f;
			posFixup[2] = (63.0f / 64.0f) / _graphicsDevice.Viewport.Width;
			posFixup[3] = -(63.0f / 64.0f) / _graphicsDevice.Viewport.Height;
			//If we have a render target bound (rendering offscreen)
			if (_graphicsDevice.GetRenderTargets().Length > 0) {
				//flip vertically
				posFixup[1] *= -1.0f;
				posFixup[3] *= -1.0f;
			}
			int posFixupLoc = GL.GetUniformLocation(shaderProgram, "posFixup");
			GL.Uniform4 (posFixupLoc, 1, posFixup);

			if (glslPixelShader != null) {
				glslPixelShader.Apply(shaderProgram,
				                  _technique._effect.Parameters,
				                  _graphicsDevice);
			}

		}
#endif
    }
}