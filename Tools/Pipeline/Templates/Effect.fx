matrix WorldViewProjection;

struct VertexShaderInput
{
#ifndef OPENGL
	float4 Position : SV_POSITION;
#else
	float4 Position : POSITION;
#endif
	float4 Color : COLOR0;
};

struct VertexShaderOutput
{
#ifndef OPENGL
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
#ifndef OPENGL
		VertexShader = compile vs_4_0 MainVS();
		PixelShader = compile ps_4_0 MainPS();
#else
		VertexShader = compile vs_3_0 MainVS();
		PixelShader = compile ps_3_0 MainPS();
#endif
	}
};