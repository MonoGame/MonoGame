// Effect uses a scrolling displacement texture to offset the position of the main
// texture. Depending on the contents of the displacement texture, this can give a
// wide range of refraction, rippling, warping, and swirling type effects.

float2 DisplacementScroll;

sampler TextureSampler : register(s0);
sampler DisplacementSampler : register(s1);


float4 main(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
    // Look up the displacement amount.
    float2 displacement = tex2D(DisplacementSampler, DisplacementScroll + texCoord / 3);
    
    // Offset the main texture coordinates.
    texCoord += displacement * 0.2 - 0.15;
    
    // Look up into the main texture.
    return tex2D(TextureSampler, texCoord) * color;
}


technique Refraction
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 main();
    }
}
