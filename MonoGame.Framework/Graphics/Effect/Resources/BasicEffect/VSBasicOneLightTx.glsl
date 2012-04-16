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
attribute vec2 TextureCoordinate;

varying vec4 Diffuse;
varying vec4 Specular;
varying vec2 TexCoord;

void main()
{
    vec3 eyeVector = normalize(EyePosition - (World * Position).xyz);
    vec3 tmpNorm = normalize(Normal);
    vec3 worldNormal = normalize(WorldInverseTranspose * tmpNorm);
	
	float diffuse = clamp(-dot(worldNormal, DirLight0Direction), 0.0, 1.0);

	vec3 Half = normalize(-DirLight0Direction + eyeVector);    
    float d = dot(worldNormal,Half);
    float specularVal = pow( max(d, 0.0), SpecularPower);
	gl_Position =  WorldViewProj * Position;
	
   	Diffuse  = vec4((DirLight0DiffuseColor * diffuse) * DiffuseColor.rgb + EmissiveColor, DiffuseColor.a);
    Specular = vec4((DirLight0SpecularColor * specularVal) * SpecularColor, clamp(dot(Position, FogVector), 0.0, 1.0));
    TexCoord = TextureCoordinate;
	
}

