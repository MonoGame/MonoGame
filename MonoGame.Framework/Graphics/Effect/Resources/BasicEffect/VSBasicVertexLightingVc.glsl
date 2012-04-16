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

uniform vec3 DirLight1Direction;
uniform vec3 DirLight1DiffuseColor;
uniform vec3 DirLight1SpecularColor;

uniform vec3 DirLight2Direction;
uniform vec3 DirLight2DiffuseColor;
uniform vec3 DirLight2SpecularColor;

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
    
    mat3 lightDirections = mat3(0);
    mat3 lightDiffuse = mat3(0);
    mat3 lightSpecular = mat3(0);
    mat3 halfVectors = mat3(0);
    
    for (int i = 0; i < 3; i++)
    {
        lightDirections[i] = mat3(DirLight0Direction,     DirLight1Direction,     DirLight2Direction)    [i];
        lightDiffuse[i]    = mat3(DirLight0DiffuseColor,  DirLight1DiffuseColor,  DirLight2DiffuseColor) [i];
        lightSpecular[i]   = mat3(DirLight0SpecularColor, DirLight1SpecularColor, DirLight2SpecularColor)[i];
        
        halfVectors[i] = normalize(eyeVector - lightDirections[i]);
    }

    vec3 dotL = -lightDirections * worldNormal;
    vec3 dotH = halfVectors * worldNormal;
    
    vec3 zeroL = step(dotL, vec3(0.0));

    vec3 diffuse  = zeroL * dotL;
    vec3 specular = pow(max(dotH, vec3(0.0)) * zeroL, vec3(SpecularPower));

	gl_Position =  WorldViewProj * Position;
    Diffuse  = vec4((lightDiffuse * diffuse)  * DiffuseColor.rgb + EmissiveColor, DiffuseColor.a) * Color;
    Specular = vec4((lightSpecular * specular) * SpecularColor, clamp(dot(Position, FogVector), 0.0, 1.0));
}