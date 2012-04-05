uniform sampler2D Texture;

uniform vec3 FogColor;
uniform vec4 AlphaTest;

varying vec4 Diffuse;
varying vec4 Specular;
varying vec2 TexCoord;

// less/greater compare function.
// if((color.a < AlphaTest.x) ? AlphaTest.z : AlphaTest.w);
void main()
{
    vec4 color = texture2D(Texture, TexCoord) * Diffuse;

	float aTest = AlphaTest.w;
	
	if (color.a < AlphaTest.x)
		aTest = AlphaTest.z;
	
	if (aTest < 0.0)
		discard;
	
	color.rgb = mix(color.rgb, FogColor * color.a, Specular.w);

    gl_FragColor = color;
}