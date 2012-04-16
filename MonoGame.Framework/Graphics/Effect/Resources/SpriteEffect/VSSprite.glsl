uniform highp mat4 MatrixTransform;

attribute highp vec4 Position;
attribute lowp vec4 Color;
attribute mediump vec2 TextureCoordinate;

varying lowp vec4 Diffuse;
varying mediump vec2 TexCoord;

void main()
{
	gl_Position = MatrixTransform * Position;
	TexCoord = TextureCoordinate;
	Diffuse = Color;
}