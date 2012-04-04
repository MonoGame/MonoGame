uniform sampler2D Texture;

uniform vec4 AlphaTest;

varying vec4 Diffuse;
varying vec2 TexCoord;

// less/greater compare function.
// if((color.a < AlphaTest.x) ? AlphaTest.z : AlphaTest.w);
void main()
{
    gl_FragColor = texture2D(Texture, TexCoord) * Diffuse;

	float aTest = AlphaTest.w;
	
	if (gl_FragColor.a < AlphaTest.x)
		aTest = AlphaTest.z;
	
	if (aTest < 0)
		discard;
}