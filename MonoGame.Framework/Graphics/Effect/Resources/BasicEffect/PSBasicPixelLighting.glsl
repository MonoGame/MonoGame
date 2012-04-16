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

uniform vec3 FogColor;

varying vec4 PositionWS;
varying vec3 NormalWS;
varying vec4 Diffuse;

void main()
{
    vec4 color = Diffuse; 
    vec3 eyeVector = normalize(EyePosition - PositionWS.xyz);
    vec3 worldNormal = normalize(NormalWS);
    
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
    
    vec3 zeroL = step(vec3(0), dotL);

    vec3 diffuse  = zeroL * dotL;
    vec3 specular = pow(max(dotH, vec3(0)) * zeroL, vec3(SpecularPower));
 
    diffuse  = (lightDiffuse * diffuse)  * DiffuseColor.rgb + EmissiveColor;
    specular = (lightSpecular * specular) * SpecularColor;
    
    color.rgb *= diffuse;
    color.rgb += specular * color.a;
    color.rgb = mix(color.rgb, FogColor * color.a, PositionWS.w);
    
    gl_FragColor = color;
}