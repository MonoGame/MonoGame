uniform sampler2D TextureSampler;
uniform sampler2D OverlaySampler;

uniform vec2 OverlayScroll;

void main()
{
	vec4 tex = gl_Color * texture2D(TextureSampler, gl_TexCoord[0].xy);
	vec4 tex2 = gl_Color * texture2D(OverlaySampler, OverlayScroll + gl_TexCoord[0].xy);
	float fadeSpeed = tex2.x; //texture2D(OverlaySampler, OverlayScroll + gl_TexCoord[1].xy).x;
	vec4 color = tex;
	color *= clamp((tex.a - fadeSpeed) * 2.5 + 1.0,0.0,1.0);
	gl_FragColor = color; //gl_Color * tex; //texture2D(TextureSampler, gl_TexCoord[0].xy);

}
//float2 OverlayScroll;

//sampler TextureSampler : register(s0);
//sampler OverlaySampler : register(s1);


//float4 main(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
//{
    //// Look up the texture color.
    //float4 tex = tex2D(TextureSampler, texCoord);
    
    // Look up the fade speed from the scrolling overlay texture.
    //float fadeSpeed = tex2D(OverlaySampler, OverlayScroll + texCoord).x;
    
    // Apply a combination of the input color alpha and the fade speed.
    //tex *= saturate((color.a - fadeSpeed) * 2.5 + 1);
    
    //return tex;
//}
