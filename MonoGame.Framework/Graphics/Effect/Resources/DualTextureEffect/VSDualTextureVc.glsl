uniform highp vec4 DiffuseColor;
uniform highp mat4 WorldViewProj;
uniform highp vec4 FogVector;

attribute highp vec4 Position;
attribute lowp vec4 Color;
attribute mediump vec2 TextureCoordinate;
attribute mediump vec2 TextureCoordinate2;

varying lowp vec4 Diffuse;
varying lowp vec4 Specular;
varying mediump vec2 TexCoord;
varying mediump vec2 TexCoord2;

void main()
{
    Diffuse = DiffuseColor * Color;
    Specular = vec4(0, 0, 0, clamp(dot(Position, FogVector), 0.0, 1.0));
    TexCoord = TextureCoordinate;
    TexCoord2 = TextureCoordinate2;
    gl_Position = WorldViewProj * Position;
}