using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

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
        Effect _effect = null;
		
        DXEffectObject.d3dx_pass _pass;

		DXShader pixelShader;
		DXShader vertexShader;

#if OPENGL
        int shaderProgram = 0;

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
#endif // OPENGL

        bool setBlendState = false;
		BlendState blendState;
		bool setDepthStencilState = false;
		DepthStencilState depthStencilState;
		bool setRasterizerState = false;
		RasterizerState rasterizerState;

        public string Name { get { return _pass.name; } }

		internal EffectPass(Effect effect, DXEffectObject.d3dx_pass pass)
        {
            _effect = effect;
            _pass = pass;

			blendState = new BlendState();
			depthStencilState = new DepthStencilState();
			rasterizerState = new RasterizerState();
			
#if OPENGL
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
                foreach (var state in _pass.states)
                {
                    var operation = DXEffectObject.state_table[state.operation];

                    if (operation.class_ == DXEffectObject.STATE_CLASS.PIXELSHADER)
                    {
                        needPixelShader = true;
                        if (state.type == DXEffectObject.STATE_TYPE.CONSTANT)
                        {
                            pixelShader = (DXShader)state.parameter.data;
                            GL.AttachShader(shaderProgram, pixelShader.ShaderHandle);
                        }
                    }
                    else if (operation.class_ == DXEffectObject.STATE_CLASS.VERTEXSHADER)
                    {
                        needVertexShader = true;
                        if (state.type == DXEffectObject.STATE_TYPE.CONSTANT)
                        {
                            vertexShader = (DXShader)state.parameter.data;
                            GL.AttachShader(shaderProgram, vertexShader.ShaderHandle);
                        }
                    }
                    else if (operation.class_ == DXEffectObject.STATE_CLASS.RENDERSTATE)
                    {
                        if (state.type != DXEffectObject.STATE_TYPE.CONSTANT)
                            throw new NotImplementedException();

                        switch (operation.op)
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
                        throw new NotImplementedException();
                }

                // If we have what we need then link.
                if (    needPixelShader == (pixelShader != null) && 
                        needVertexShader == (vertexShader != null) )
                    Link();
            }
            finally
            {
                Threading.End();
            }
#elif DIRECTX

#endif
        }

        public void Apply()
		{
            // Set/get the correct shader handle/cleanups.
            _effect.OnApply();

			//Console.WriteLine (_technique._effect.Name+" - "+_technique.Name+" - "+Name);
			var relink = false;
            foreach (var state in _pass.states)
            {				
				// Constants handled during initialization.
				if (state.type == DXEffectObject.STATE_TYPE.CONSTANT) 
                    continue;

                var operation = DXEffectObject.state_table[state.operation];

                if (operation.class_ == DXEffectObject.STATE_CLASS.PIXELSHADER ||
                    operation.class_ == DXEffectObject.STATE_CLASS.VERTEXSHADER)
                {
                    DXShader shader;
					switch (state.type) 
                    {
					case DXEffectObject.STATE_TYPE.EXPRESSIONINDEX:
                        var expression = (DXExpression)state.parameter.data;
                        shader = (DXShader)expression.Evaluate(_effect.Parameters);
						break;
					case DXEffectObject.STATE_TYPE.PARAMETER:
					default:
						throw new NotImplementedException();
					}

#if OPENGL
					if (shader.ShaderType == ShaderType.FragmentShader && shader != pixelShader) 
                    {
						if (pixelShader != null)
                            GL.DetachShader(shaderProgram, pixelShader.ShaderHandle);

                        relink = true;
						pixelShader = shader;
                        GL.AttachShader(shaderProgram, pixelShader.ShaderHandle);
					}
                    else if (shader.ShaderType == ShaderType.VertexShader && shader != vertexShader) 
                    {
						if (vertexShader != null)
                            GL.DetachShader(shaderProgram, vertexShader.ShaderHandle);

                        relink = true;
						vertexShader = shader;
                        GL.AttachShader(shaderProgram, vertexShader.ShaderHandle);
					}	
#elif DIRECTX

#endif
				}
			}

			if (relink)
				Link();
			
#if OPENGL
			GL.UseProgram(shaderProgram);
#elif DIRECTX

#endif

            var device = _effect.GraphicsDevice;

			if (setRasterizerState)
                device.RasterizerState = rasterizerState;
			if (setBlendState)
                device.BlendState = blendState;
			if (setDepthStencilState)
                device.DepthStencilState = depthStencilState;

#if OPENGL
			if (vertexShader != null) 
            {
				vertexShader.Apply(shaderProgram, _effect.Parameters, device);
			} 
            else 
            {
				//passthrough shader is attached
                var vp = device.Viewport;
                var projection = Matrix.CreateOrthographicOffCenter(0, vp.Width, vp.Height, 0, 0, 1);
                var halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);
                var transform = halfPixelOffset * projection;

                var uniform = GL.GetUniformLocation(shaderProgram, "transformMatrix");
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
            posFixup[2] = (63.0f / 64.0f) / device.Viewport.Width;
            posFixup[3] = -(63.0f / 64.0f) / device.Viewport.Height;
			//If we have a render target bound (rendering offscreen)
            if (device.GetRenderTargets().Length > 0) 
            {
				//flip vertically
				posFixup[1] *= -1.0f;
				posFixup[3] *= -1.0f;
			}
            var posFixupLoc = GL.GetUniformLocation(shaderProgram, "posFixup");
			GL.Uniform4 (posFixupLoc, 1, posFixup);
			
			if (pixelShader != null)
				pixelShader.Apply(shaderProgram, _effect.Parameters, device);

#elif DIRECTX
            
#endif
		}

        private void Link()
		{
#if OPENGL
			if (vertexShader == null && !passthroughVertexShaderAttached) 
            {
				if (!passthroughVertexShader.HasValue) 
                {
					var shader = GL.CreateShader(ShaderType.VertexShader);
#if GLES
					GL.ShaderSource (shader, 1,
					                new string[]{passthroughVertexShaderSrc}, (int[])null);
#else
					GL.ShaderSource(shader, passthroughVertexShaderSrc);
#endif
					GL.CompileShader(shader);

					passthroughVertexShader = shader;
				}

				GL.AttachShader(shaderProgram, passthroughVertexShader.Value);
				GL.BindAttribLocation(shaderProgram, GraphicsDevice.attributePosition, "aPosition");
				GL.BindAttribLocation(shaderProgram, GraphicsDevice.attributeTexCoord, "aTexCoord");
				GL.BindAttribLocation(shaderProgram, GraphicsDevice.attributeColor, "aColor");

				passthroughVertexShaderAttached = true;
			} 
            else if (vertexShader != null && passthroughVertexShaderAttached) 
            {
				GL.DetachShader(shaderProgram, passthroughVertexShader.Value);
				passthroughVertexShaderAttached = false;
			}

			if (vertexShader != null)
				vertexShader.OnLink(shaderProgram);
			if (pixelShader != null)
				pixelShader.OnLink(shaderProgram);

			GL.LinkProgram(shaderProgram);

			var linked = 0;
#if GLES
			GL.GetProgram(shaderProgram, ProgramParameter.LinkStatus, ref linked);
#else
			GL.GetProgram(shaderProgram, ProgramParameter.LinkStatus, out linked);
#endif
			if (linked == 0) 
            {
#if !GLES
				string log = GL.GetProgramInfoLog(shaderProgram);
				Console.WriteLine (log);
#endif
				throw new InvalidOperationException("Unable to link effect program");
			}
#endif
		}		
				
    }
}