uniform sampler2D TextureSampler;

uniform vec2 Position;

void main()
{
    vec4 texCoord = vec4(gl_TexCoord[0].xy, 0, 0);

    texCoord.x += Position.x * 0.0025;
    texCoord.y += Position.y * 0.0025;
    texCoord *= 0.5;    
	
	vec4 results = vec4(0,0,1,0.25) * texCoord;
	
	texCoord.x += Position.x * 0.0025 + 0.25;
	texCoord.y += Position.y * 0.0025 - 0.15;
	texCoord *= 0.4;
	
	results += vec4(0,1,0,0.15) * texture2D(TextureSampler, texCoord);
	
	gl_FragColor = results;

}