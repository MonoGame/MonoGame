// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "Include.fxh"

float4x4 View;
float4x4 Projection;

struct VSInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VSOutput
{
    float2 TexCoord : TEXCOORD0;
    float4 Position : SV_Position;
};

struct PSInput
{
    float2 TexCoord : TEXCOORD0;
};

VSOutput VS(VSInput input, float4x4 worldTransposed : BLENDWEIGHT)
{
    VSOutput output = (VSOutput)0;
    
    // HLSL defaults to column-major packing, so the vertex data appears transposed in the shader.
#if VULKAN
    // DXC already expects the vertex data transposed, so no manual transposition is needed.
    // https://github.com/microsoft/DirectXShaderCompiler/blob/main/docs/SPIR-V.rst#vectors-and-matrices
    // "Conceptually HLSL matrices are row-major while SPIR-V matrices are column-major, thus all HLSL matrices are represented by their transposes."
    // "Matrix multiplication: need to swap the operands. mat1 x mat2 should be translated as transpose(mat2) x transpose(mat1). Then the result is transpose(mat1 x mat2)."
    float4x4 world = worldTransposed;
#else
    // A manual transposition is required to restore the correct layout for FXC.
    // https://learn.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-per-component-math#matrix-ordering
    float4x4 world = transpose(worldTransposed);
#endif
    float4 positionWorld = mul(input.Position, world);
    float4 positionView = mul(positionWorld, View);
    output.Position = mul(positionView, Projection);
    
    output.TexCoord = input.TexCoord;
    
    return output;
}

float4 PS(PSInput input) : SV_TARGET0
{
    return float4(input.TexCoord.xy, 0, 1);
}

technique
{
    pass
    {
        VertexShader = compile VS_PROFILE VS();
        PixelShader = compile PS_PROFILE PS();
    }
}
