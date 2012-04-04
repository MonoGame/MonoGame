uniform vec4 DiffuseColor;
uniform vec4 FogVector;
uniform mat4 WorldViewProj;

attribute vec4 Position;
attribute vec4 Color;
attribute vec2 TextureCoordinate;

varying vec4 Diffuse;
varying vec4 Specular;
varying vec2 TexCoord;

void main()
{
    gl_Position = WorldViewProj * Position;
    Diffuse = Color * DiffuseColor;
    Specular = vec4(vec3(0), clamp(dot(Position, FogVector), 0.0, 1.0));
    TexCoord = TextureCoordinate;
}