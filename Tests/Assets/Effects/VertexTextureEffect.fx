// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if SM6

#define PS_PROFILE ps_6_0
#define VS_PROFILE vs_6_0

#elif SM4

#define PS_PROFILE ps_4_0
#define VS_PROFILE vs_4_0

#else

#define PS_PROFILE ps_3_0
#define VS_PROFILE vs_3_0

#endif

matrix WorldViewProj;

float HeightMapSize;

#if SM6

Texture2D<float4> HeightMapTexture : register(t0);

sampler HeightMapSampler : register(s0) = sampler_state
{
    Texture = (HeightMapTexture);
    MinFilter = POINT;
    MagFilter = POINT;
    MipFilter = NONE;
};

#else

Texture2D HeightMapTexture : register(t0);

sampler2D HeightMapSampler : register(s0) = sampler_state 
{
    Texture = (HeightMapTexture);
    MinFilter = POINT;
    MagFilter = POINT;
    MipFilter = NONE;
};

#endif

struct VSOutput
{
    float4 PositionPS : SV_Position;
    float4 Color : COLOR0;
};

VSOutput VS_Main(float2 xy : POSITION)
{
#if SM6
    float height = HeightMapTexture.SampleLevel(HeightMapSampler, (xy + float2(0.5, 0.5)) / HeightMapSize, 0).r;
#else
    float height = tex2Dlod(HeightMapSampler, float4((xy + float2(0.5, 0.5)) / HeightMapSize, 0, 0)).r;
#endif

    float3 worldPosition = float3(xy.x, height, xy.y);

    VSOutput output;
    output.PositionPS = mul(float4(worldPosition, 1), WorldViewProj);
    output.Color = float4(xy.x / HeightMapSize, xy.y / HeightMapSize, 0, 1);

    return output;
}

float4 PS_Main(VSOutput input) : SV_TARGET0
{
    return input.Color;
}


technique
{
    pass
    {
        VertexShader = compile VS_PROFILE VS_Main();
        PixelShader = compile PS_PROFILE PS_Main();
    }
}
