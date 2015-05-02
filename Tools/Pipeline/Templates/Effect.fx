
#define DIRECTX // comment this line for compiling to OpenGL platforms

matrix WorldViewProjection;

struct VertexShaderInput
{
#ifdef DIRECTX
	float4 Position : SV_POSITION;
#else
	float4 Position : POSITION;
#endif
	float4 Color : COLOR0;
};

struct VertexShaderOutput
{
#ifdef DIRECTX
	float4 Position : SV_POSITION;
#else
	float4 Position : POSITION;
#endif
	float4 Color : COLOR0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = mul(input.Position, WorldViewProjection);
	output.Color = input.Color;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	return input.Color;
}

technique BasicColorDrawing
{
	pass P0
	{
#ifdef DIRECTX
		VertexShader = compile vs_4_0 MainVS();
		PixelShader = compile ps_4_0 MainPS();
#else
		VertexShader = compile vs_3_0 MainVS();
		PixelShader = compile ps_3_0 MainPS();
#endif
	}
};