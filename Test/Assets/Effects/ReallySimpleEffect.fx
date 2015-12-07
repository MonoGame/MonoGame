// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "include.fxh"

#define NUM_ARRAY_SIZE 4

float TestFloat;
float2 TestFloat2;
float3 TestFloat3;
float4 TestFloat4;

float4x3 TestFloat4x3;

float3 TestFloat3Array[NUM_ARRAY_SIZE];
float4x3 TestFloat4x3Array[NUM_ARRAY_SIZE];

float4x4 View;
float4x4 Projection;
#define NUM_TEXTURES 8
#define MAX_OBJECTS 12
float4x3 WorldTransforms[MAX_OBJECTS];
float TextureIndices[MAX_OBJECTS];

texture Colors;

float3 LightDirection1;
float3 DiffuseColor1;
float3 LightDirection2;
float3 DiffuseColor2;
float AmbientAmount;


sampler ColorSampler = sampler_state
{
	Texture = (Colors);

	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Point;

	AddressU = Clamp;
	AddressV = Clamp;
};


//The texture coordinates aren't actually used in this shader but it makes things marginally simpler outside.  Not exactly optimized!
struct VertexShaderInput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 TextureCoordinates : TEXCOORD0;
	float Index : TEXCOORD1;
	float TextureIndex : TEXCOORD2;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float3 Normal : TEXCOORD0;
	float TextureIndex : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	int index = (int)round(input.Index);
	float3 worldPosition = mul(input.Position, WorldTransforms[index]);
	output.Position = mul(mul(float4(worldPosition, 1), View), Projection);
	output.Normal = mul(input.Normal, WorldTransforms[index]);
	output.TextureIndex = input.TextureIndex;

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 surfaceColor;

	float halfPixel = 0.5f / NUM_TEXTURES;

	surfaceColor = tex2D(ColorSampler, float2(halfPixel + halfPixel * 2 * input.TextureIndex, halfPixel));


	float3 normal = normalize(input.Normal);
	float diffuseAmount1 = saturate(-dot(normal, LightDirection1));
	float diffuseAmount2 = saturate(-dot(normal, LightDirection2));


	surfaceColor = float4(AmbientAmount * surfaceColor + surfaceColor * (diffuseAmount1 * DiffuseColor1 + diffuseAmount2 * DiffuseColor2), 1);
	return surfaceColor;
}

technique Technique1
{
	pass Pass1
	{
        VertexShader = compile VS_PROFILE VertexShaderFunction();
        PixelShader = compile PS_PROFILE PixelShaderFunction();
    }
}
