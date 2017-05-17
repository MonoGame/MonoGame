// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Graphics
{

    // TODO: We should convert the types below 
    // into the start of a Shader reflection API.

    public enum SamplerType
    {
        Sampler2D = 0,
        SamplerCube = 1,
        SamplerVolume = 2,
        Sampler1D = 3,
    }

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

    internal struct VertexAttribute
    {
        public VertexElementUsage usage;
        public int index;
        public string name;
        public int location;
    }

    internal partial class Shader : GraphicsResource
	{
        /// <summary>
        /// Returns the platform specific shader profile identifier.
        /// </summary>
        public static int Profile { get { return PlatformProfile(); } }

        /// <summary>
        /// A hash value which can be used to compare shaders.
        /// </summary>
        internal int HashKey { get; private set; }

        public SamplerInfo[] Samplers { get; private set; }

	    public int[] CBuffers { get; private set; }

        public ShaderStage Stage { get; private set; }

        public VertexAttribute[] Attributes { get; private set; }

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

                Samplers[s].name = reader.GetSamplerName(s);
                Samplers[s].parameter = reader.GetSamplerParameter(s);
            }

            var cbufferCount = reader.GetConstantBufferCount();
            CBuffers = new int[cbufferCount];
            for (var c = 0; c < cbufferCount; c++)
                CBuffers[c] = reader.GetConstantBufferValue(c);

            var attributeCount = reader.GetAttributeCount();
            Attributes = new VertexAttribute[attributeCount];
            for (var a = 0; a < attributeCount; a++)
            {
                Attributes[a].name = reader.GetAttributeName(a);
                Attributes[a].usage = reader.GetAttributeUsage(a);
                Attributes[a].index = reader.GetAttributeIndexInShader(a);
                Attributes[a].location = reader.GetAttributeLocation(a);
            }

            PlatformConstruct(Stage == ShaderStage.Vertex, shaderBytecode);
        }

        internal protected override void GraphicsDeviceResetting()
        {
            PlatformGraphicsDeviceResetting();
        }
	}
}

