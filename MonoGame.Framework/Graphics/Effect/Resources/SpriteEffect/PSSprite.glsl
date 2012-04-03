uniform sampler2D Texture;

varying vec4 Diffuse;
varying vec2 TexCoord;

void main()
{
	gl_FragColor = texture2D(Texture, TexCoord) * Diffuse;
}
