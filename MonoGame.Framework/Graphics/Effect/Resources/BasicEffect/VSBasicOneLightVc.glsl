uniform vec4 FogVector;
uniform mat4 World;
uniform mat3 WorldInverseTranspose;
uniform mat4 WorldViewProj;

uniform vec4 DiffuseColor;
uniform vec3 EmissiveColor;
uniform vec3 SpecularColor;
uniform float SpecularPower;

uniform vec3 DirLight0Direction;
uniform vec3 DirLight0DiffuseColor;
uniform vec3 DirLight0SpecularColor;

uniform vec3 EyePosition;

attribute vec4 Position;
attribute vec3 Normal;
attribute vec4 Color;

varying vec4 Diffuse;
varying vec4 Specular;

void main()
{
    vec3 eyeVector = normalize(EyePosition - (World * Position).xyz);
    vec3 worldNormal = normalize(WorldInverseTranspose * Normal);
    
    vec3 halfVector = normalize(eyeVector - DirLight0Direction);

    float dotL = dot(-DirLight0Direction, worldNormal);
    float dotH = dot(halfVector, worldNormal);
    float zeroL = step(0.0, dotL);

    float diffuse  = zeroL * dotL;
    float specular = pow(max(dotH, 0) * zeroL, SpecularPower);

	gl_Position =  WorldViewProj * Position;
    Diffuse  = vec4((DirLight0DiffuseColor * diffuse)  * DiffuseColor.rgb + EmissiveColor, DiffuseColor.a) * Color;
    Specular = vec4((DirLight0SpecularColor * specular) * SpecularColor, clamp(dot(Position, FogVector), 0.0, 1.0));
}