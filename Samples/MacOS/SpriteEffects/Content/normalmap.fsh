// Effect applies normalmapped lighting to a 2D sprite.

uniform sampler2D TextureSampler;
uniform sampler2D NormalSampler;

uniform vec3 LightDirection;
vec3 LightColor = vec3(1.5);
vec3 AmbientColor = vec3(0.0);

void main()
{
	// Look up the texture and normalmap values.
	vec4 tex = texture2D(TextureSampler, gl_TexCoord[0].xy);
	vec3 normal = vec3(texture2D(NormalSampler, gl_TexCoord[0].xy));
	
	// Compute lighting.
	float lightAmount = max(dot(normal, LightDirection),0.0);
	
	vec3 color = vec3(gl_Color);
	color *= AmbientColor + lightAmount * LightColor;
	gl_FragColor = tex * vec4(color,1);
}

//float3 LightDirection;
//float3 LightColor = 1.5;
//float3 AmbientColor = 0;

//sampler TextureSampler : register(s0);
//sampler NormalSampler : register(s1);


//float4 main(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
//{
    // Look up the texture and normalmap values.
//    float4 tex = tex2D(TextureSampler, texCoord);
//    float3 normal = tex2D(NormalSampler, texCoord);
    
    // Compute lighting.
//    float lightAmount = max(dot(normal, LightDirection), 0);
    
//    color.rgb *= AmbientColor + lightAmount * LightColor;
    
//    return tex * color;
//}


//technique Normalmap
//{
//    pass Pass1
//    {
//        PixelShader = compile ps_2_0 main();
//    }
//}
