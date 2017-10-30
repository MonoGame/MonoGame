// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "include.fxh"

float4 VS_Main(float4 position : POSITION0) : SV_Position0
{
	return float4(1, 2, 3, 4);
}

float4 PS_Main(float4 position : SV_Position) : COLOR0
{
	return 1;
}

technique
{
    pass
    {
		VertexShader = compile VS_PROFILE VS_Main();
        PixelShader = compile PS_PROFILE PS_Main();
    }
}

#if defined(INVALID_SYNTAX)
Foo;
#endif

#if MACRO_DEFINE_TEST != 3
Bar;
#endif
