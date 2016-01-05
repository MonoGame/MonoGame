// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "include.fxh"

Texture2D SourceTexture;

SamplerComparisonState SourceSampler;

struct VSOutput
{
    float4 position : SV_Position;
    float4 color    : COLOR0;
    float2 uv       : TEXCOORD0;
};

float4 PS_Main(VSOutput input) : COLOR0
{
    float comparisonResult = SourceTexture.SampleCmpLevelZero(SourceSampler, input.uv, 0.5f);
    return float4(comparisonResult, 0, 0, 1);
}

technique
{
    pass
    {
        PixelShader = compile ps_4_0 PS_Main();
    }
}