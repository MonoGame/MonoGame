// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "include.fxh"

DECLARE_TEXTURE(s, 0);

float4 PixelShaderFunction( float4 inPosition : SV_Position,
			    float4 inColor : COLOR0,
			    float2 coords : TEXCOORD0) : SV_TARGET0
{
    float4 color = SAMPLE_TEXTURE(s, coords);
    
    color -= SAMPLE_TEXTURE(s, coords - 0.002) * 2.5f;
    color += SAMPLE_TEXTURE(s, coords + 0.002) * 2.5f;

    return color;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile PS_PROFILE PixelShaderFunction();
    }
}
