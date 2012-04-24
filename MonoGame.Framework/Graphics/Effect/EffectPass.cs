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
        private Effect _effect;

        private DXEffectObject.d3dx_state[] _pass_states;

		private DXShader _pixelShader;

        private DXShader _vertexShader;

#if OPENGL

        private int _shaderProgram;

		static readonly float[] _posFixup = new float[4];

#endif // OPENGL

        private bool _setBlendState;
        private BlendState _blendState;

        private bool _setDepthStencilState;
        private DepthStencilState _depthStencilState;

        private bool _setRasterizerState;
        private RasterizerState _rasterizerState;

		public string Name { get; private set; }

        public EffectAnnotationCollection Annotations { get; private set; }

		internal EffectPass(Effect effect, DXEffectObject.d3dx_pass pass)
        {
            _effect = effect;

            Name = pass.name;

            Annotations = new EffectAnnotationCollection();

			_blendState = new BlendState();
			_depthStencilState = new DepthStencilState();
			_rasterizerState = new RasterizerState();
			
#if OPENGL
            Threading.Begin();
            try
            {
                _shaderProgram = GL.CreateProgram();

                _pass_states = pass.states;
                foreach (var state in _pass_states)
                {
                    if (state.type != DXEffectObject.STATE_TYPE.CONSTANT)
                        continue;

                    var operation = DXEffectObject.state_table[state.operation];

                    if (operation.class_ == DXEffectObject.STATE_CLASS.PIXELSHADER)
                    {
                        _pixelShader = (DXShader)state.parameter.data;
                        GL.AttachShader(_shaderProgram, _pixelShader.ShaderHandle);
                    }
                    else if (operation.class_ == DXEffectObject.STATE_CLASS.VERTEXSHADER)
                    {
                        _vertexShader = (DXShader)state.parameter.data;
                        GL.AttachShader(_shaderProgram, _vertexShader.ShaderHandle);
                    }
                    else if (operation.class_ == DXEffectObject.STATE_CLASS.RENDERSTATE)
                    {
                        switch (operation.op)
                        {
                            case (uint)DXEffectObject.D3DRENDERSTATETYPE.STENCILENABLE:
                                _depthStencilState.StencilEnable = (bool)state.parameter.data;
                                _setDepthStencilState = true;
                                break;
                            case (uint)DXEffectObject.D3DRENDERSTATETYPE.SCISSORTESTENABLE:
                                _rasterizerState.ScissorTestEnable = (bool)state.parameter.data;
                                _setRasterizerState = true;
                                break;
                            case (uint)DXEffectObject.D3DRENDERSTATETYPE.BLENDOP:
                                _blendState.ColorBlendFunction = (BlendFunction)state.parameter.data;
                                _setBlendState = true;
                                break;
                            case (uint)DXEffectObject.D3DRENDERSTATETYPE.SRCBLEND:
                                _blendState.ColorSourceBlend = (Blend)state.parameter.data;
                                _setBlendState = true;
                                break;
                            case (uint)DXEffectObject.D3DRENDERSTATETYPE.DESTBLEND:
                                _blendState.ColorDestinationBlend = (Blend)state.parameter.data;
                                _setBlendState = true;
                                break;
                            case (uint)DXEffectObject.D3DRENDERSTATETYPE.ALPHABLENDENABLE:
                                break; //not sure what to do
                            case (uint)DXEffectObject.D3DRENDERSTATETYPE.CULLMODE:
                                _rasterizerState.CullMode = (CullMode)state.parameter.data;
                                _setRasterizerState = true;
                                break;
                            case (uint)DXEffectObject.D3DRENDERSTATETYPE.COLORWRITEENABLE:
                                _blendState.ColorWriteChannels = (ColorWriteChannels)state.parameter.data;
                                _setBlendState = true;
                                break;
                            case (uint)DXEffectObject.D3DRENDERSTATETYPE.STENCILFUNC:
                                _depthStencilState.StencilFunction = (CompareFunction)state.parameter.data;
                                _setDepthStencilState = true;
                                break;
                            case (uint)DXEffectObject.D3DRENDERSTATETYPE.STENCILPASS:
                                _depthStencilState.StencilPass = (StencilOperation)state.parameter.data;
                                _setDepthStencilState = true;
                                break;
                            case (uint)DXEffectObject.D3DRENDERSTATETYPE.STENCILFAIL:
                                _depthStencilState.StencilFail = (StencilOperation)state.parameter.data;
                                _setDepthStencilState = true;
                                break;
                            case (uint)DXEffectObject.D3DRENDERSTATETYPE.STENCILREF:
                                _depthStencilState.ReferenceStencil = (int)state.parameter.data;
                                _setDepthStencilState = true;
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                    else
                        throw new NotImplementedException();
                }

                // If we both shaders then link them now.
                if ( _pixelShader != null && _vertexShader != null )
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

            // TODO: This is only nessasary because we allow for "expressions"
            // in shader assignment... we should remove this functionality.
			var relink = false;
            foreach (var state in _pass_states)
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
					if (shader.ShaderType == ShaderType.FragmentShader && shader != _pixelShader) 
                    {
						if (_pixelShader != null)
                            GL.DetachShader(_shaderProgram, _pixelShader.ShaderHandle);

                        relink = true;
						_pixelShader = shader;
                        GL.AttachShader(_shaderProgram, _pixelShader.ShaderHandle);
					}
                    else if (shader.ShaderType == ShaderType.VertexShader && shader != _vertexShader) 
                    {
						if (_vertexShader != null)
                            GL.DetachShader(_shaderProgram, _vertexShader.ShaderHandle);

                        relink = true;
						_vertexShader = shader;
                        GL.AttachShader(_shaderProgram, _vertexShader.ShaderHandle);
					}	
#elif DIRECTX

#endif
				}
			}

			if (relink)
				Link();
			
#if OPENGL
			GL.UseProgram(_shaderProgram);
#elif DIRECTX

#endif

            var device = _effect.GraphicsDevice;

			if (_setRasterizerState)
                device.RasterizerState = _rasterizerState;
			if (_setBlendState)
                device.BlendState = _blendState;
			if (_setDepthStencilState)
                device.DepthStencilState = _depthStencilState;

#if OPENGL

			if (_vertexShader != null) 
				_vertexShader.Apply(_shaderProgram, _effect.Parameters, device);

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

			_posFixup[0] = 1.0f;
			_posFixup[1] = 1.0f;
            _posFixup[2] = (63.0f / 64.0f) / device.Viewport.Width;
            _posFixup[3] = -(63.0f / 64.0f) / device.Viewport.Height;
			//If we have a render target bound (rendering offscreen)
            if (device.GetRenderTargets().Length > 0) 
            {
				//flip vertically
				_posFixup[1] *= -1.0f;
				_posFixup[3] *= -1.0f;
			}
            var posFixupLoc = GL.GetUniformLocation(_shaderProgram, "posFixup"); // TODO: Look this up on link!
			GL.Uniform4 (posFixupLoc, 1, _posFixup);
			
			if (_pixelShader != null)
				_pixelShader.Apply(_shaderProgram, _effect.Parameters, device);

#elif DIRECTX
            
#endif
		}

        private void Link()
		{
#if OPENGL
			if (_vertexShader != null)
				_vertexShader.OnLink(_shaderProgram);
			if (_pixelShader != null)
				_pixelShader.OnLink(_shaderProgram);

			GL.LinkProgram(_shaderProgram);

			var linked = 0;
#if GLES
			GL.GetProgram(shaderProgram, ProgramParameter.LinkStatus, ref linked);
#else
			GL.GetProgram(_shaderProgram, ProgramParameter.LinkStatus, out linked);
#endif
			if (linked == 0) 
            {
#if !GLES
				string log = GL.GetProgramInfoLog(_shaderProgram);
				Console.WriteLine (log);
#endif
				throw new InvalidOperationException("Unable to link effect program");
			}
#endif
		}		
				
    }
}