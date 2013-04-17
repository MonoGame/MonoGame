using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

#if PSM
using Sce.PlayStation.Core.Graphics;
#endif


namespace Microsoft.Xna.Framework.Graphics
{
    public class EffectPass
    {
        private readonly Effect _effect;

		private readonly Shader _pixelShader;
        private readonly Shader _vertexShader;

        private readonly BlendState _blendState;
        private readonly DepthStencilState _depthStencilState;
        private readonly RasterizerState _rasterizerState;

		public string Name { get; private set; }

        public EffectAnnotationCollection Annotations { get; private set; }

#if PSM
        internal ShaderProgram _shaderProgram;
#endif

        internal EffectPass(    Effect effect, 
                                string name,
                                Shader vertexShader, 
                                Shader pixelShader, 
                                BlendState blendState, 
                                DepthStencilState depthStencilState, 
                                RasterizerState rasterizerState,
                                EffectAnnotationCollection annotations )
        {
            Debug.Assert(effect != null, "Got a null effect!");
            Debug.Assert(annotations != null, "Got a null annotation collection!");

            _effect = effect;

            Name = name;

            _vertexShader = vertexShader;
            _pixelShader = pixelShader;

            _blendState = blendState;
            _depthStencilState = depthStencilState;
            _rasterizerState = rasterizerState;

            Annotations = annotations;
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
#if PSM
            _shaderProgram = cloneSource._shaderProgram;
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

            var device = _effect.GraphicsDevice;

#if OPENGL || DIRECTX

            if (_vertexShader != null)
            {
                device.VertexShader = _vertexShader;

                // Update the constant buffers.
                for (var c = 0; c < _vertexShader.CBuffers.Length; c++)
                {
                    var cb = _effect.ConstantBuffers[_vertexShader.CBuffers[c]];
                    cb.Update(_effect.Parameters);
                    device.SetConstantBuffer(ShaderStage.Vertex, c, cb);
                }
            }

            if (_pixelShader != null)
            {
                device.PixelShader = _pixelShader;

                // Update the texture parameters.
                foreach (var sampler in _pixelShader.Samplers)
                {
                    var param = _effect.Parameters[sampler.parameter];
                    var texture = param.Data as Texture;
										
					// If there is no texture assigned then skip it
					// and leave whatever set directly on the device.
					if (texture != null)
						device.Textures[sampler.textureSlot] = texture;

                    // If there is a sampler state set it.
                    if (sampler.state != null)
                        device.SamplerStates[sampler.samplerSlot] = sampler.state;
                }
                
                // Update the constant buffers.
                for (var c = 0; c < _pixelShader.CBuffers.Length; c++)
                {
                    var cb = _effect.ConstantBuffers[_pixelShader.CBuffers[c]];
                    cb.Update(_effect.Parameters);
                    device.SetConstantBuffer(ShaderStage.Pixel, c, cb);
                }
            }

#endif

            // Set the render states if we have some.
            if (_rasterizerState != null)
                device.RasterizerState = _rasterizerState;
            if (_blendState != null)
                device.BlendState = _blendState;
            if (_depthStencilState != null)
                device.DepthStencilState = _depthStencilState;
            
#if PSM
            _effect.GraphicsDevice._graphics.SetShaderProgram(_shaderProgram);

            #warning We are only setting one hardcoded parameter here. Need to do this properly by iterating _effect.Parameters (Happens in Shader)

            float[] data;
            if (_effect.Parameters["WorldViewProj"] != null) 
                data = (float[])_effect.Parameters["WorldViewProj"].Data;
            else
                data = (float[])_effect.Parameters["MatrixTransform"].Data;
            Sce.PlayStation.Core.Matrix4 matrix4 = PSSHelper.ToPssMatrix4(data);
            matrix4 = matrix4.Transpose (); //When .Data is set the matrix is transposed, we need to do it again to undo it
            _shaderProgram.SetUniformValue(0, ref matrix4);
#endif
        }
		
    }
}