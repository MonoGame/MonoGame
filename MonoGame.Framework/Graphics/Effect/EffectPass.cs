using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif PSS
using Sce.Pss.Core.Graphics;
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

		private DXShader _pixelShader;
        private DXShader _vertexShader;

        private BlendState _blendState;
        private DepthStencilState _depthStencilState;
        private RasterizerState _rasterizerState;

		public string Name { get; private set; }

        public EffectAnnotationCollection Annotations { get; private set; }

#if OPENGL

        private int _shaderProgram;

        static readonly float[] _posFixup = new float[4];

#endif // OPENGL

#if PSS
        internal ShaderProgram _shaderProgram;
#endif

        internal EffectPass(    Effect effect, 
                                string name,
                                DXShader vertexShader, 
                                DXShader pixelShader, 
                                BlendState blendState, 
                                DepthStencilState depthStencilState, 
                                RasterizerState rasterizerState,
                                EffectAnnotationCollection annotations )
        {
            Debug.Assert(effect != null, "Got a null effect!");
            Debug.Assert(vertexShader != null, "Got a null vertex shader!");
            Debug.Assert(pixelShader != null, "Got a null pixel shader!");
            Debug.Assert(annotations != null, "Got a null annotation collection!");

            _effect = effect;

            Name = name;

            _vertexShader = vertexShader;
            _pixelShader = pixelShader;

            _blendState = blendState;
            _depthStencilState = depthStencilState;
            _rasterizerState = rasterizerState;

            Annotations = annotations;

            Initialize();
        }

        internal EffectPass(Effect effect, EffectPass cloneSource)
        {
            Debug.Assert(effect != null, "Got a null effect!");
            Debug.Assert(cloneSource != null, "Got a null cloneSource!");

            _effect = effect;

            // Share all the immutable types.
            Name = cloneSource.Name;
            _blendState = cloneSource._blendState;
            _depthStencilState = cloneSource._depthStencilState;
            _rasterizerState = cloneSource._rasterizerState;
            Annotations = cloneSource.Annotations;
            _vertexShader = cloneSource._vertexShader;
            _pixelShader = cloneSource._pixelShader;
#if OPENGL || PSS
            _shaderProgram = cloneSource._shaderProgram;
#endif
        }

        private void Initialize()
        {
#if OPENGL
            Threading.BlockOnUIThread(() =>
            {
                // TODO: Shouldn't we be calling GL.DeleteProgram() somewhere?

                // TODO: We could cache the various program combinations 
                // of vertex/pixel shaders and share them across effects.

                _shaderProgram = GL.CreateProgram();

                GL.AttachShader(_shaderProgram, _vertexShader.ShaderHandle);
                GL.AttachShader(_shaderProgram, _pixelShader.ShaderHandle);

                _vertexShader.OnLink(_shaderProgram);
                _pixelShader.OnLink(_shaderProgram);
                GL.LinkProgram(_shaderProgram);

                var linked = 0;

#if GLES
    			GL.GetProgram(_shaderProgram, ProgramParameter.LinkStatus, ref linked);
#else
                GL.GetProgram(_shaderProgram, ProgramParameter.LinkStatus, out linked);
#endif
                if (linked == 0)
                {
#if !GLES
                    string log = GL.GetProgramInfoLog(_shaderProgram);
                    Console.WriteLine(log);
#endif
                    throw new InvalidOperationException("Unable to link effect program");
                }
            });

#elif DIRECTX

#endif
        }

        public void Apply()
        {
            // Set/get the correct shader handle/cleanups.
            //
            // TODO: This "reapply" if the shader index changes
            // trick is sort of ugly.  We should probably rework
            // this to use some sort of "technique/pass redirect".
            //
            if (_effect.OnApply())
            {
                _effect.CurrentTechnique.Passes[0].Apply();
                return;
            }

#if OPENGL
            GL.UseProgram(_shaderProgram);
#elif PSS
            _effect.GraphicsDevice._graphics.SetShaderProgram(_shaderProgram);
#endif

            var device = _effect.GraphicsDevice;

            if (_rasterizerState != null)
                device.RasterizerState = _rasterizerState;
            if (_blendState != null)
                device.BlendState = _blendState;
            if (_depthStencilState != null)
                device.DepthStencilState = _depthStencilState;

            Debug.Assert(_vertexShader != null, "Got a null vertex shader!");
            Debug.Assert(_pixelShader != null, "Got a null vertex shader!");

#if PSS
#warning We are only setting one hardcoded parameter here. Need to do this properly by iterating _effect.Parameters (Happens in DXShader)
            float[] data;
            if (_effect.Parameters["WorldViewProj"] != null) 
                data = (float[])_effect.Parameters["WorldViewProj"].Data;
            else
                data = (float[])_effect.Parameters["MatrixTransform"].Data;
            Sce.Pss.Core.Matrix4 matrix4 = PSSHelper.ToPssMatrix4(data);
            matrix4 = matrix4.Transpose (); //When .Data is set the matrix is transposed, we need to do it again to undo it
            _shaderProgram.SetUniformValue(0, ref matrix4);
#elif OPENGL

            // Apply the vertex shader.
            _vertexShader.Apply(device, _shaderProgram, _effect.Parameters, _effect.ConstantBuffers);

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
            GL.Uniform4(posFixupLoc, 1, _posFixup);

            // Apply the pixel shader.
            _pixelShader.Apply(device, _shaderProgram, _effect.Parameters, _effect.ConstantBuffers);

#elif DIRECTX

            lock (device._d3dContext)
            {
                // Apply the shaders which will in turn set the 
                // constant buffers and texture samplers.
                _vertexShader.Apply(device, _effect.Parameters, _effect.ConstantBuffers);
                _pixelShader.Apply(device, _effect.Parameters, _effect.ConstantBuffers);
            }

#endif
        }
		
    }
}