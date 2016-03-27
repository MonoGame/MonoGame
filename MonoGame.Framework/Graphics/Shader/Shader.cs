// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.IO;

namespace Microsoft.Xna.Framework.Graphics
{
    public enum SamplerType
    {
        Sampler2D = 0,
        SamplerCube = 1,
        SamplerVolume = 2,
        Sampler1D = 3,
    }

    // TODO: We should convert the sampler info below 
    // into the start of a Shader reflection API.

    internal struct SamplerInfo
    {
        public SamplerType type;
        public int textureSlot;
        public int samplerSlot;
        public string name;
		public SamplerState state;

        // TODO: This should be moved to EffectPass.
        public int parameter;
    }

    internal partial class Shader : GraphicsResource
	{
        /// <summary>
        /// A hash value which can be used to compare shaders.
        /// </summary>
        internal int HashKey { get; private set; }

        public SamplerInfo[] Samplers { get; private set; }

	    public int[] CBuffers { get; private set; }

        public ShaderStage Stage { get; private set; }
		
        internal Shader(GraphicsDevice device, IShaderReader reader)
        {
            GraphicsDevice = device;

            Stage = reader.GetStage();

            var shaderBytecode = reader.GetBytecode();

            var samplerCount = reader.GetSamplerCount();
            Samplers = new SamplerInfo[samplerCount];
            for (var s = 0; s < samplerCount; s++)
            {
                Samplers[s].type = reader.GetSamplerType(s);
                Samplers[s].textureSlot = reader.GetSamplerTextureSlot(s);
                Samplers[s].samplerSlot = reader.GetSamplerSamplerSlot(s);
                Samplers[s].state = reader.GetSamplerState(s);

#if OPENGL
                Samplers[s].name = reader.GetSamplerName(s);
#else
                Samplers[s].name = null;
#endif
                Samplers[s].parameter = reader.GetSamplerParameter(s);
            }

            var cbufferCount = reader.GetConstantBufferCount();
            CBuffers = new int[cbufferCount];
            for (var c = 0; c < cbufferCount; c++)
                CBuffers[c] = reader.GetConstantBufferValue(c);

            PlatformConstruct(reader, Stage == ShaderStage.Vertex, shaderBytecode);
        }

        internal protected override void GraphicsDeviceResetting()
        {
            PlatformGraphicsDeviceResetting();
        }
	}
}

