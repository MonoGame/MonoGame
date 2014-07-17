sampler s0;

float4 OutlineColor = float4(0,0,0,1);

float4 Main(float2 coords: TEXCOORD0) : COLOR0
{
    float4 color = tex2D(s0, coords);
    return color;
}

float4 Outline(float2 coords: TEXCOORD0) : COLOR0
{
	float4 bordercolor = float4(0,0,1,0.25);

    float4 color = tex2D(s0, coords);

    float4 top = tex2D(s0, float2(coords.x, coords.y - 0.01));
    float4 right = tex2D(s0, float2(coords.x - 0.01, coords.y));
    float4 bottom = tex2D(s0, float2(coords.x, coords.y + 0.01));
    float4 left = tex2D(s0, float2(coords.x + 0.01, coords.y));

	if(color.a == 0)
	{
		if(right.a || top.a || bottom.a || left.a)
		{
			color.rgba = OutlineColor;
		}
	}

    return color;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 Outline();
    }

	pass Pass2
    {
        PixelShader = compile ps_2_0 Outline();
    }
}