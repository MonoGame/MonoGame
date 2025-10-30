// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "Include.fxh"

DECLARE_TEXTURE(Texture, 0);

cbuffer _MG_Globals : register(b0)
{
    float3x3 Layer1Colors;
}

float4 PixelShaderFunction(float4 inPosition : SV_Position,
                            float4 inColor : COLOR0,
                            float2 coords : TEXCOORD0) : SV_TARGET0
{
    float4 color = SAMPLE_TEXTURE(Texture, coords);
    
    float3 adjustment = Layer1Colors[0] + Layer1Colors[1] + Layer1Colors[2];
    
    return color * inColor - float4(adjustment.r, adjustment.g, adjustment.b, 0);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile PS_PROFILE PixelShaderFunction();
    }
}
