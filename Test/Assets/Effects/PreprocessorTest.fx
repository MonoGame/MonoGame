// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "PreprocessorInclude.fxh"

#define TEST 1

/*
This is a C style comment.
*/

#if foo(TEST)

#endif

#if TEST == 0
Foo
#elif TEST == 1
Bar
#else
Baz
#endif

#if defined(TEST2)
FOO
#elif defined(TEST3)
BAR
#endif

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