uniform sampler2D Texture;

void main()
{
	gl_FragColor = gl_Color * texture2D(Texture, gl_TexCoord[0].xy);
}
