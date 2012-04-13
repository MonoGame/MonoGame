uniform sampler2D Texture;

uniform vec4 AlphaTest;

varying vec4 Diffuse;
varying vec2 TexCoord;

void main()
{
	gl_FragColor = texture2D(Texture, TexCoord) * Diffuse;

	float aTest = AlphaTest.w;
	
	if (abs(gl_FragColor.a - AlphaTest.x) < AlphaTest.y)
		aTest = AlphaTest.z;
	
	if (aTest < 0.0)
		discard;
}