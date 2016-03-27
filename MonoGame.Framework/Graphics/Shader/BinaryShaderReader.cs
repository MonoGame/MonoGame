// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.IO;

namespace Microsoft.Xna.Framework.Graphics
{
    internal class BinaryShaderReader : IShaderReader
    {
        private ShaderStage _stage;
        private byte[] _shaderBytecode;
        private SamplerInfo[] _samplers;
        private int[] _cbuffers;
        private Attribute[] _attributes;

        private struct Attribute
        {
            public VertexElementUsage usage;
            public int index;
            public string name;
        }

        public BinaryShaderReader(BinaryReader reader)
        {
            var isVertexShader = reader.ReadBoolean();
            _stage = isVertexShader ? ShaderStage.Vertex : ShaderStage.Pixel;

            var shaderLength = reader.ReadInt32();
            _shaderBytecode = reader.ReadBytes(shaderLength);

            var samplerCount = (int)reader.ReadByte();
            _samplers = new SamplerInfo[samplerCount];
            for (var s = 0; s < samplerCount; s++)
            {
                _samplers[s].type = (SamplerType)reader.ReadByte();
                _samplers[s].textureSlot = reader.ReadByte();
                _samplers[s].samplerSlot = reader.ReadByte();

                if (reader.ReadBoolean())
                {
                    _samplers[s].state = new SamplerState();
                    _samplers[s].state.AddressU = (TextureAddressMode)reader.ReadByte();
                    _samplers[s].state.AddressV = (TextureAddressMode)reader.ReadByte();
                    _samplers[s].state.AddressW = (TextureAddressMode)reader.ReadByte();
                    _samplers[s].state.BorderColor = new Color(
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte());
                    _samplers[s].state.Filter = (TextureFilter)reader.ReadByte();
                    _samplers[s].state.MaxAnisotropy = reader.ReadInt32();
                    _samplers[s].state.MaxMipLevel = reader.ReadInt32();
                    _samplers[s].state.MipMapLevelOfDetailBias = reader.ReadSingle();
                }

#if OPENGL
                _samplers[s].name = reader.ReadString();
#else
                _samplers[s].name = null;
#endif
                _samplers[s].parameter = reader.ReadByte();
            }

            var cbufferCount = (int)reader.ReadByte();
            _cbuffers = new int[cbufferCount];
            for (var c = 0; c < cbufferCount; c++)
                _cbuffers[c] = reader.ReadByte();

#if OPENGL
            var attributeCount = (int)reader.ReadByte();
            _attributes = new Attribute[attributeCount];
            for (var a = 0; a < attributeCount; a++)
            {
                _attributes[a].name = reader.ReadString();
                _attributes[a].usage = (VertexElementUsage)reader.ReadByte();
                _attributes[a].index = reader.ReadByte();
                reader.ReadInt16(); //format, unused
            }
#endif
        }

        public ShaderStage GetStage()
        {
            return _stage;
        }

        public byte[] GetBytecode()
        {
            return _shaderBytecode;
        }

        public int GetSamplerCount()
        {
            return _samplers.Length;
        }

        public SamplerType GetSamplerType(int samplerIndex)
        {
            return _samplers[samplerIndex].type;
        }

        public int GetSamplerTextureSlot(int samplerIndex)
        {
            return _samplers[samplerIndex].textureSlot;
        }

        public int GetSamplerSamplerSlot(int samplerIndex)
        {
            return _samplers[samplerIndex].samplerSlot;
        }

        public SamplerState GetSamplerState(int samplerIndex)
        {
            return _samplers[samplerIndex].state;
        }

        public string GetSamplerName(int samplerIndex)
        {
            return _samplers[samplerIndex].name;
        }

        public int GetSamplerParameter(int samplerIndex)
        {
            return _samplers[samplerIndex].parameter;
        }

        public int GetConstantBufferCount()
        {
            return _cbuffers.Length;
        }

        public int GetConstantBufferValue(int constantBufferIndex)
        {
            return _cbuffers[constantBufferIndex];
        }

        public int GetAttributeCount()
        {
            return _attributes.Length;
        }

        public string GetAttributeName(int attributeIndex)
        {
            return _attributes[attributeIndex].name;
        }

        public VertexElementUsage GetAttributeUsage(int attributeIndex)
        {
            return _attributes[attributeIndex].usage;
        }

        public int GetAttributeIndexInShader(int attributeIndex)
        {
            return _attributes[attributeIndex].index;
        }
    }
}
