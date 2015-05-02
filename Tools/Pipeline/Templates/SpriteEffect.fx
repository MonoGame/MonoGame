#define DIRECTX // comment this line for compiling to OpenGL platforms

Texture2D SpriteTexture;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
#ifdef DIRECTX
	float4 Position : SV_POSITION;
#else
	float4 Position : POSITION;
#endif
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	return tex2D(SpriteTextureSampler,input.TextureCoordinates) * input.Color;
}

technique SpriteDrawing
{
	pass P0
	{
#ifdef DIRECTX
		PixelShader = compile ps_4_0 MainPS();
#else
		PixelShader = compile ps_3_0 MainPS();
#endif
	}
};