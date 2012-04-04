uniform sampler2D Texture;

uniform vec3 FogColor;
uniform vec4 AlphaTest;

varying vec4 Diffuse;
varying vec4 Specular;
varying vec2 TexCoord;

void main()
{
    vec4 color = texture2D(Texture, TexCoord) * Diffuse;

	float aTest = AlphaTest.w;
	
	if (abs(color.a - AlphaTest.x) < AlphaTest.y)
		aTest = AlphaTest.z;
	
	if (aTest < 0)
		discard;
	
	color.rgb = mix(color.rgb, FogColor * color.a, Specular.w);
	
	gl_FragColor = color;
}