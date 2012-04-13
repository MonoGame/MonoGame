uniform vec4 DiffuseColor;
uniform mat4 WorldViewProj;

attribute vec4 Position;
attribute vec4 Color;
attribute vec2 TextureCoordinate;

varying vec4 Diffuse;
varying vec2 TexCoord;

void main()
{
    gl_Position = WorldViewProj * Position;
    Diffuse = Color * DiffuseColor;
    TexCoord = TextureCoordinate;
}