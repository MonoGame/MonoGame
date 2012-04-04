uniform mat4 MatrixTransform;

attribute vec4 Position;
attribute vec4 Color;
attribute vec2 TextureCoordinate;

varying vec4 Diffuse;
varying vec2 TexCoord;

void main()
{
	gl_Position = MatrixTransform * Position;
	TexCoord = TextureCoordinate;
	Diffuse = Color;
}