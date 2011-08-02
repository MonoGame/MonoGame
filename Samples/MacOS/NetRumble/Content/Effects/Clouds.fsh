uniform sampler2D TextureSampler;

uniform vec2 Position;

void main()
{

	vec4 texCoord = texture2D(TextureSampler, gl_TexCoord[0].xy);
	vec2 pos = Position;
	
//	texCoord.x += pos.x * 0.0025f;
//	texCoord.y += pox.y * 0.0025f;
//	texCoord *= 0.5f;
	
//	vec4 results = vec4(0,0,1,0.25) * texCoord;
	
//	texCoord.x += pos.x * 0.0025f + 0.25f;
//	texCoord.y += pox.y * 0.0025f - 0.15f;
//	texCoord *= 0.4f;
	
//	results += vec4(0,1,0,0.15) * texCoord;
	
	gl_FragColor = gl_Color * texCoord; //texture2D(TextureSampler, gl_TexCoord[0].xy);
	//gl_FragColor = results;
}


// the relative position for the texture coordinates, inducing a slight parallax
//float2 Position;


// modify the sampler state on the zero texture sampler, used by SpriteBatch
//sampler TextureSampler : register(s0) = 
//sampler_state
//{
//    AddressU = Wrap;
//    AddressV = Wrap;
//};


//float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
//{
    // sample from the cloud texture for the blue component
//    texCoord.x += Position.x * 0.00025f;
//    texCoord.y += Position.y * 0.00025f;
//    texCoord *= 0.5f;
//    float4 results = float4(0,0,1,0.25) * tex2D(TextureSampler, texCoord);

    // sample from the cloud texture for the green component
//    texCoord.x += Position.x * 0.00025f + 0.25f;
//    texCoord.y += Position.y * 0.00025f - 0.15;
//    texCoord *= 0.4f;
//    results += float4(0,1,0,0.15) * tex2D(TextureSampler, texCoord);
//        
//    return results;
//}


//technique Technique1
//{
//    pass Pass1
//    {
//        PixelShader = compile ps_2_0 PixelShaderFunction();
//    }
//}
