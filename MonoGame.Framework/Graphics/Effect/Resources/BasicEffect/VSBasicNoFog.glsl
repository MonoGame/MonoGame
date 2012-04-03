uniform vec4 DiffuseColor;
uniform mat4 WorldViewProj;

attribute vec4 Position;

varying vec4 Diffuse;

void main()
{
    gl_Position = WorldViewProj * Position;
    Diffuse = DiffuseColor;
}