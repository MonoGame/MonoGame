// Effect uses a scrolling displacement texture to offset the position of the main
// texture. Depending on the contents of the displacement texture, this can give a
// wide range of refraction, rippling, warping, and swirling type effects.

uniform sampler2D TextureSampler;
uniform sampler2D DisplacementSampler;

uniform vec2 DisplacementScroll;

void main()
{
	// place a reference here to TextureSampler so that it is the first texture
	vec4 tex2 = texture2D(TextureSampler, gl_TexCoord[0].xy);

	// Look up the displacement amount.
	vec2 displacement = vec2(texture2D(DisplacementSampler, DisplacementScroll + gl_TexCoord[0].xy / 3.0)  * 0.2 - 0.15);
	
	// Offset the main texture coordinates.
	vec4 tex = texture2D(TextureSampler, gl_TexCoord[0].xy + displacement);
	
	gl_FragColor = tex;// * gl_Color;
}
//float2 DisplacementScroll;

//sampler TextureSampler : register(s0);
//sampler DisplacementSampler : register(s1);


//float4 main(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
//{
    // Look up the displacement amount.
//    float2 displacement = tex2D(DisplacementSampler, DisplacementScroll + texCoord / 3);
    
    // Offset the main texture coordinates.
//    texCoord += displacement * 0.2 - 0.15;
    
    // Look up into the main texture.
//    return tex2D(TextureSampler, texCoord) * color;
//}


//technique Refraction
//{
  //  pass Pass1
  //  {
  //      PixelShader = compile ps_2_0 main();
  //  }
//}

