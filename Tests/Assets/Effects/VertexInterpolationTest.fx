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

// matrix WorldViewProj;

struct VSOutput
{
    float4 PositionPS : SV_Position;
    float4 TexCoord : TEXCOORD0;
};

VSOutput VS_Main(   float4 position : POSITION0,
					float4 texCoord : TEXCOORD0)
{
    VSOutput output;
    output.PositionPS = position;
    output.TexCoord = texCoord;    
    return output;
}

float4 PS_Main(VSOutput input) : SV_TARGET0
{    
    return float4(input.TexCoord.x, input.TexCoord.y, 0, 1);
}

technique
{
    pass
    {
        VertexShader = compile VS_PROFILE VS_Main();
        PixelShader = compile PS_PROFILE PS_Main();
    }
}
