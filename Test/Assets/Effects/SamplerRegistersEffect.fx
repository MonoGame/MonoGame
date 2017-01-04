// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "include.fxh"

sampler s0 : register(s0);
sampler s1 : register(s1);

struct VS_INPUT
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct PS_INPUT
{
	float4 Position : SV_Position;
	float2 TexCoord : TEXCOORD0;
};

PS_INPUT VertexShaderFunction(VS_INPUT input)
{
	PS_INPUT output;
    output.Position = input.Position;
	output.TexCoord = input.TexCoord; 
    return output;
}

float4 PixelShader_Both(PS_INPUT input) : COLOR0
{	
    return tex2D(s0, input.TexCoord) * tex2D(s1, input.TexCoord);
}

float4 PixelShader_Zero(PS_INPUT input) : COLOR0
{
    return tex2D(s0, input.TexCoord);
}

float4 PixelShader_One(PS_INPUT input) : COLOR0
{
    return tex2D(s1, input.TexCoord);
}

technique Both
{
	pass
	{
		VertexShader = compile VS_PROFILE VertexShaderFunction();
		PixelShader = compile PS_PROFILE PixelShader_Both();
	}
}

technique Zero
{
	pass
	{
		VertexShader = compile VS_PROFILE VertexShaderFunction();
		PixelShader = compile PS_PROFILE PixelShader_Zero();
	}
}

technique One
{
	pass
	{
		VertexShader = compile VS_PROFILE VertexShaderFunction();
		PixelShader = compile PS_PROFILE PixelShader_One();
	}
}
