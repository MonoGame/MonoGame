using System;

#if WINRT
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
	internal partial class Shader
	{
#if DIRECTX
		protected VertexShader _vertexShader;

        protected PixelShader _pixelShader;

        public byte[] Bytecode { get; protected set; }
		
        public void Apply(  GraphicsDevice graphicsDevice, 
                            EffectParameterCollection parameters,
                            ConstantBuffer[] cbuffers )
        {
			// NOTE: We make the assumption here that the caller has
			// locked the d3dContext for us to use.
            var d3dContext = graphicsDevice._d3dContext;
            if (_pixelShader != null)
            {
                foreach (var sampler in _samplers)
                {
                    var param = parameters[sampler.parameter];
                    var texture = param.Data as Texture;
                    graphicsDevice.Textures[sampler.index] = texture;
                }

                d3dContext.PixelShader.Set(_pixelShader);
            }
            else
            {
                d3dContext.VertexShader.Set(_vertexShader);

                // Set the shader on the device so it can 
                // apply the correct input layout at draw time.
                graphicsDevice._vertexShader = this;
            }

            // Update and set the constants.
            for (var c = 0; c < _cbuffers.Length; c++)
            {
                var cb = cbuffers[_cbuffers[c]];
                cb.Apply(_vertexShader != null, c, parameters);
            }
        }
		
#endif // DIRECTX
	}
}

