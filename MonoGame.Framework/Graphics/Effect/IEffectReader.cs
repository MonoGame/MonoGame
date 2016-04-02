// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    public interface IEffectReader
    {
        int GetEffectKey();

        int GetConstantBufferCount();

        string GetConstantBufferName(int constantBufferIndex);

        int GetConstantBufferSize(int constantBufferIndex);

        int GetConstantBufferParameterCount(int constantBufferIndex);

        int GetConstantBufferParameterValue(int constantBufferIndex, int parameterIndex);

        int GetConstantBufferParameterOffset(int constantBufferIndex, int parameterIndex);

        int GetShaderCount();

        IShaderReader GetShaderReader(int shaderIndex);

        int GetParameterCount(object parameterContext);

        EffectParameterClass GetParameterClass(object parameterContext, int parameterIndex);

        EffectParameterType GetParameterType(object parameterContext, int parameterIndex);

        string GetParameterName(object parameterContext, int parameterIndex);

        string GetParameterSemantic(object parameterContext, int parameterIndex);

        int GetParameterAnnotationCount(object parameterContext, int parameterIndex);

        int GetParameterRowCount(object parameterContext, int parameterIndex);

        int GetParameterColumnCount(object parameterContext, int parameterIndex);

        int GetParameterInt32Buffer(object parameterContext, int parameterIndex, int bufferIndex);

        float GetParameterFloatBuffer(object parameterContext, int parameterIndex, int bufferIndex);

        object GetParameterElementsContext(object parameterContext, int parameterIndex);

        object GetParameterStructMembersContext(object parameterContext, int parameterIndex);

        int GetTechniqueCount();

        string GetTechniqueName(int techniqueIndex);

        int GetTechniqueAnnotationCount(int techniqueIndex);

        int GetPassCount(int techniqueIndex);

        string GetPassName(int techniqueIndex, int passIndex);

        int GetPassAnnotationCount(int techniqueIndex, int passIndex);

        int? GetPassVertexShaderIndex(int techniqueIndex, int passIndex);

        int? GetPassPixelShaderIndex(int techniqueIndex, int passIndex);

        BlendState GetPassBlendState(int techniqueIndex, int passIndex);

        DepthStencilState GetPassDepthStencilState(int techniqueIndex, int passIndex);

        RasterizerState GetPassRasterizerState(int techniqueIndex, int passIndex);
    }
}
