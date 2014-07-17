sampler s0;

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
    float4 color = tex2D(s0, coords);
	color -= tex2D(s0, coords - 0.002) * 2.5f;
	color += tex2D(s0, coords + 0.002) * 2.5f;

    return color;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}