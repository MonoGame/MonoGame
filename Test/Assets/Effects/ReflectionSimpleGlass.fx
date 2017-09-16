#include "Macros.fxh"
string description = "Reflection shader for glass materials in RacingGame";

// Variables that are provided by the application.
// Support for UIWidget is also added for FXComposer and 3DS Max :)
BEGIN_CONSTANTS
float4x4 viewProj              : ViewProjection;
float4x4 world                 : World;
float3 viewInverse           : ViewInverse;
float3 lightDir : Direction
<
    string UIName = "Light Direction";
    string Object = "DirectionalLight";
    string Space = "World";
> = {1.0f, -1.0f, 1.0f};

// The ambient, diffuse and specular colors are pre-multiplied with the light color!
float4 ambientColor : Ambient
<
    string UIName = "Ambient Color";
    string Space = "material";
> = {0.15f, 0.15f, 0.15f, 1.0f};

float4 diffuseColor : Diffuse
<
    string UIName = "Diffuse Color";
    string Space = "material";
> = {0.25f, 0.25f, 0.25f, 1.0f};

float4 specularColor : Specular
<
    string UIName = "Specular Color";
    string Space = "material";
> = {1.0f, 1.0f, 1.0f, 1.0f};

float shininess : SpecularPower
<
    string UIName = "Specular Power";
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 128.0;
    float UIStep = 1.0;
> = 24.0;

float alphaFactor
<
    string UIName = "Alpha factor";
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.01;
> = 0.66f;

float fresnelBias = 0.5f;
float fresnelPower = 1.5f;
float reflectionAmount = 1.0f;
END_CONSTANTS

BEGIN_DECLARE_CUBE_TARGET(reflectionCubeTexture, Environment)
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Wrap;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
END_DECLARE_TEXTURE;

//----------------------------------------------------

// Vertex input structure (used for ALL techniques here!)
struct VertexInput
{
    float3 pos      : SV_POSITION;
    float2 texCoord : TEXCOORD0;
    float3 normal   : NORMAL;
    float3 tangent  : TANGENT;
};

//----------------------------------------------------

// Common functions
float4 TransformPosition(float3 pos)
{
    return mul(mul(float4(pos.xyz, 1), world), viewProj);
}

float3 GetWorldPos(float3 pos)
{
    return mul(float4(pos, 1), world).xyz;
}

float3 GetCameraPos()
{
    return viewInverse;
}

float3 CalcNormalVector(float3 nor)
{
    return normalize(mul(nor, (float3x3)world));
}

//----------------------------------------------------

struct VertexOutput20
{
    float4 pos      : SV_POSITION;
    float3 normal   : TEXCOORD0;
    float3 viewVec  : TEXCOORD1;
    float3 halfVec  : TEXCOORD2;
};

// vertex shader
VertexOutput20 VS_ReflectionSpecular20(VertexInput In)
{
    VertexOutput20 Out;
    Out.pos = TransformPosition(In.pos);
    Out.normal = CalcNormalVector(In.normal);
    Out.viewVec = normalize(GetCameraPos() - GetWorldPos(In.pos));
    Out.halfVec = normalize(Out.viewVec + lightDir);
    return Out;
}

float4 PS_ReflectionSpecular20(VertexOutput20 In) : SV_TARGET
{
    half3 N = normalize(In.normal);
    float3 V = normalize(In.viewVec);

    // Reflection
    half3 R = reflect(-V, N);
    R = float3(R.x, R.z, R.y);
    half4 reflColor = SAMPLE_CUBE(reflectionCubeTexture, R);
    
    // Fresnel
    float3 E = -V;
    float facing = 1.0 - max(dot(E, -N), 0);
    float fresnel = fresnelBias + (1.0-fresnelBias)*pow(facing, fresnelPower);

    // Diffuse factor
    float diff = saturate(dot(N, lightDir));

    // Specular factor
    float spec = pow(saturate(dot(N, In.halfVec)), shininess);
    
    // Output the colors
    float4 diffAmbColor = ambientColor + diff * diffuseColor;
    float4 ret;
    ret.rgb = reflColor * reflectionAmount * fresnel * 1.5f +
        diffAmbColor;
    ret.a = alphaFactor;
    ret += spec * specularColor;
    return ret;
}

BEGIN_TECHNIQUE(ReflectionSpecular20)
	BEGIN_PASS(P0)
		AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		SHADERS(VS_ReflectionSpecular20, PS_ReflectionSpecular20)
	END_PASS
END_TECHNIQUE
