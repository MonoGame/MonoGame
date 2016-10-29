// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "include.fxh"

Texture2D SourceTexture;
float4x4 TransformationMatrix;

sampler2D SourceSampler = sampler_state
{
    Texture = (SourceTexture);
};

struct VSdat {
    float2 uv : TEXCOORD0;
    float4 pos : SV_Position;
};

VSdat VS_Main(float2 uv : TEXCOORD0, float4 pos : POSITION0)
{
    VSdat dat;
    dat.uv = uv;
    dat.pos = mul(pos, TransformationMatrix);
    return dat;
}

float4 PS_Main(float2 uv : TEXCOORD0) : COLOR0
{
    return tex2D(SourceSampler, uv);
}

technique
{
    pass
    {
        VertexShader = compile VS_PROFILE VS_Main();
        PixelShader = compile PS_PROFILE PS_Main();
    }
}