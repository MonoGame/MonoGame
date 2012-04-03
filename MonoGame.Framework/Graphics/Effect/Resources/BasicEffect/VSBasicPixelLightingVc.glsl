uniform vec4 DiffuseColor;
uniform vec4 FogVector;
uniform mat4 World;
uniform mat3 WorldInverseTranspose;
uniform mat4 WorldViewProj;

attribute vec4 Position;
attribute vec3 Normal;
attribute vec4 Color;

varying vec4 PositionWS;
varying vec3 NormalWS;
varying vec4 Diffuse;

void main()
{
    gl_Position = WorldViewProj * Position;
    PositionWS = vec4((World * Position).xyz, clamp(dot(Position, FogVector), 0.0, 1.0));
    NormalWS = normalize(Normal * WorldInverseTranspose);
    Diffuse = vec4(Color.rgb, Color.a * DiffuseColor.a);
}