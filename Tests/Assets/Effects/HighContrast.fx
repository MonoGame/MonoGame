// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "include.fxh"

DECLARE_TEXTURE(s, 0);

float4 PixelShaderFunction( float4 inPosition : SV_Position,
			    float4 inColor : COLOR0,
			    float2 coords : TEXCOORD0 ) : SV_TARGET0
{
    float4 color = SAMPLE_TEXTURE(s, coords);

	if      (color.r > .65) color.r = 1;
	else if (color.r < .35) color.r = 0;

    return color;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile PS_PROFILE PixelShaderFunction();
    }
}
