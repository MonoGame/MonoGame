using System.Diagnostics;

namespace Microsoft.Xna.Framework.Graphics
{
    public class EffectPass
    {
        private readonly Effect _effect;

        private readonly Shader _vertexShader;
        private readonly Shader _pixelShader;
        private readonly Shader _hullShader;
        private readonly Shader _domainShader;
        private readonly Shader _geometryShader;
        private readonly Shader _computeShader;

        private readonly BlendState _blendState;
        private readonly DepthStencilState _depthStencilState;
        private readonly RasterizerState _rasterizerState;

		public string Name { get; private set; }

        public EffectAnnotationCollection Annotations { get; private set; }

        internal EffectPass(    Effect effect, 
                                string name,
                                Shader vertexShader, 
                                Shader pixelShader,  
                                Shader hullShader,
                                Shader domainShader,
                                Shader geometryShader,
                                Shader computeShader,
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
            _hullShader = hullShader;
            _domainShader = domainShader;
            _geometryShader = geometryShader;
            _computeShader = computeShader;

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
            _hullShader = cloneSource._hullShader;
            _domainShader = cloneSource._domainShader;
            _geometryShader = cloneSource._geometryShader;
            _computeShader = cloneSource._computeShader;
        }

        public void Apply()
        {
            // Set/get the correct shader handle/cleanups.

            var current = _effect.CurrentTechnique;
            _effect.OnApply();
            if (_effect.CurrentTechnique != current)
            {
                _effect.CurrentTechnique.Passes[0].Apply();
                return;
            }

            var device = _effect.GraphicsDevice;

            if (_vertexShader != null)
            {
                device.VertexShader = _vertexShader;

				// Update the texture parameters.
                SetShaderSamplers(_vertexShader, device.VertexTextures, device.VertexSamplerStates);
                SetConstantBuffers(_vertexShader, device);
                SetShaderResources(_vertexShader, device);
            }

            if (_pixelShader != null)
            {
                device.PixelShader = _pixelShader;

                // Update the texture parameters.
                SetShaderSamplers(_pixelShader, device.Textures, device.SamplerStates);
                SetConstantBuffers(_pixelShader, device);
                SetShaderResources(_pixelShader, device);
            }

            device.HullShader = _hullShader;

            if (_hullShader != null)
            {
                // Update the texture parameters.
                SetShaderSamplers(_hullShader, device.HullTextures, device.HullSamplerStates);
                SetConstantBuffers(_hullShader, device);
                SetShaderResources(_hullShader, device);
            }

            device.DomainShader = _domainShader;

            if (_domainShader != null)
            {
                // Update the texture parameters.
                SetShaderSamplers(_domainShader, device.DomainTextures, device.DomainSamplerStates);
                SetConstantBuffers(_domainShader, device);
                SetShaderResources(_domainShader, device);
            }

            device.GeometryShader = _geometryShader;

            if (_geometryShader != null)
            {
                // Update the texture parameters.
                SetShaderSamplers(_geometryShader, device.GeometryTextures, device.GeometrySamplerStates);
                SetConstantBuffers(_geometryShader, device);
                SetShaderResources(_geometryShader, device);
            }

            // no compute shader during normal rendering, compute shader is set in ApplyCompute()
            device.ComputeShader = null;

            // Set the render states if we have some.
            if (_rasterizerState != null)
                device.RasterizerState = _rasterizerState;
            if (_blendState != null)
                device.BlendState = _blendState;
            if (_depthStencilState != null)
                device.DepthStencilState = _depthStencilState;
        }

        public void ApplyCompute()
        {
            if (_computeShader == null)
                return;

            var device = _effect.GraphicsDevice;
            device.ComputeShader = _computeShader;
            device.VertexShader = null;
            device.PixelShader = null;
            device.HullShader = null;
            device.DomainShader = null;
            device.GeometryShader = null;

            // Update the texture parameters.
            SetShaderSamplers(_computeShader, device.ComputeTextures, device.ComputeSamplerStates);
            SetConstantBuffers(_computeShader, device);
            SetShaderResources(_computeShader, device);
        }

        private void SetShaderSamplers(Shader shader, TextureCollection textures, SamplerStateCollection samplerStates)
        {
            foreach (var sampler in shader.Samplers)
            {
                var param = _effect.Parameters[sampler.parameter];
                var texture = param.Data as Texture;

                textures[sampler.textureSlot] = texture;

                // If there is a sampler state set it.
                if (sampler.state != null)
                    samplerStates[sampler.samplerSlot] = sampler.state;
            }
        }

        private void SetConstantBuffers(Shader shader, GraphicsDevice device)
        {
            for (var c = 0; c < shader.CBuffers.Length; c++)
            {
                var cb = _effect.ConstantBuffers[shader.CBuffers[c]];
                cb.Update(_effect.Parameters);
                device.SetConstantBuffer(shader.Stage, c, cb);
            }
        }

        private void SetShaderResources(Shader shader, GraphicsDevice device)
        {
            for (var b = 0; b < shader.ShaderResources.Length; b++)
            {
                var sr = shader.ShaderResources[b];
                var param = _effect.Parameters[sr.parameter];
                var resource = param.Data as ShaderResource;
                device.SetShaderResource(shader.Stage, sr.slot, resource, sr.name, sr.writeAccess);
            }
        }
    }
}
