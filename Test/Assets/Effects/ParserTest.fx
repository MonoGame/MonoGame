// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "include.fxh"

uniform float TestFloat;
uniform float2 TestFloat2;
uniform float3 TestFloat3;
uniform float4 TestFloat4;

float4 VS_Main(float4 position : POSITION0) : SV_Position0
{
	return float4(1, 2, 3, 4);
}

float4 PS_Main(float4 position : SV_Position) : COLOR0
{
	return 1;
}

technique Technique1
{
    pass Pass0
    {
		CullMode = NonE;
		CullMode = Ccw;
		CullMode = cw;
		CullMode = cW;

		ColorWriteEnable = true;
		ColorWriteEnable = false;
		ColorWriteEnable = All;
		ColorWriteEnable = None;
		ColorWriteEnable = Red|Green;
		ColorWriteEnable = Red|Green|Blue;
		ColorWriteEnable = Red|Green|Blue|Alpha;

		ZEnable = false;
		ZEnable = true;

		ZWriteEnable = FALSE;
		ZWriteEnable = TRUE;

		VertexShader = compile VS_PROFILE VS_Main();
        PixelShader = compile PS_PROFILE PS_Main();
    }
}

technique
{
    pass
    {
		VertexShader = compile VS_PROFILE VS_Main();
        PixelShader = compile PS_PROFILE PS_Main();
    }
}


