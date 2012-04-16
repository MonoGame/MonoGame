uniform highp vec4 DiffuseColor;
uniform highp mat4 WorldViewProj;

attribute highp vec4 Position;
attribute mediump vec2 TextureCoordinate;
attribute mediump vec2 TextureCoordinate2;

varying lowp vec4 Diffuse;
varying mediump vec2 TexCoord;
varying mediump vec2 TexCoord2;

void main()
{
    Diffuse = DiffuseColor;
    TexCoord = TextureCoordinate;
    TexCoord2 = TextureCoordinate2;
    gl_Position = WorldViewProj * Position;
}