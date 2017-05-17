// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    public interface IShaderReader
    {
        ShaderStage GetStage();

        byte[] GetBytecode();

        int GetSamplerCount();

        SamplerType GetSamplerType(int samplerIndex);

        int GetSamplerTextureSlot(int samplerIndex);

        int GetSamplerSamplerSlot(int samplerIndex);

        SamplerState GetSamplerState(int samplerIndex);

        string GetSamplerName(int samplerIndex);

        int GetSamplerParameter(int samplerIndex);

        int GetConstantBufferCount();

        int GetConstantBufferValue(int constantBufferIndex);

        int GetAttributeCount();

        string GetAttributeName(int attributeIndex);

        VertexElementUsage GetAttributeUsage(int attributeIndex);

        int GetAttributeIndexInShader(int attributeIndex);

        int GetAttributeLocation(int attributeIndex);
    }
}
