#include "Macros.fxh"
string description = "Normal mapping shaders for RacingGame";

// Shader techniques in this file, all shaders work with vs/ps 1.1, shaders not
// working with 1.1 have names with 20 at the end:
// Diffuse           : Full vertex ambient+diffuse+specular lighting
// Diffuse20         : Same for ps20, only required for 3DS max to show shader!
//
// Specular           : Full vertex ambient+diffuse+specular lighting
// Specular20         : Nicer effect for ps20, also required for 3DS max to show shader!
//
// DiffuseSpecular    : Same as specular, but adding the specular component
//                        to diffuse (per vertex)
// DiffuseSpecular20  : Nicer effect for ps20, also required for 3DS max to show shader!
BEGIN_CONSTANTS
float4x4 viewProj         : ViewProjection;
float4x4 world            : World;
float3 viewInverse      : ViewInverse;

float3 lightDir : Direction
<
    string UIName = "Light Direction";
    string Object = "DirectionalLight";
    string Space = "World";
> = {-0.65f, 0.65f, -0.39f}; // Normalized by app. FxComposer still uses inverted stuff

// The ambient, diffuse and specular colors are pre-multiplied with the light color!
float4 ambientColor : Ambient
<
    string UIName = "Ambient Color";
    string Space = "material";
> = {0.1f, 0.1f, 0.1f, 1.0f};

float4 diffuseColor : Diffuse
<
    string UIName = "Diffuse Color";
    string Space = "material";
> = {1.0f, 1.0f, 1.0f, 1.0f};

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
> = 16.0;

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
bool UseAlpha = true;
// Special shader for car rendering, which allows to change the car color!
float3 carHueColor = 0.0;
END_CONSTANTS


// Texture and samplers
BEGIN_DECLARE_TEXTURE_TARGET(diffuseTexture, Diffuse)
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Wrap;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
	MipFilter = Linear;
END_DECLARE_TEXTURE;

BEGIN_DECLARE_TEXTURE_TARGET(normalTexture, Diffuse)
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Wrap;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
	MipFilter = Linear;
END_DECLARE_TEXTURE;

BEGIN_DECLARE_TEXTURE_TARGET(reflectionCubeTexture, Environment)
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Wrap;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear; 
END_DECLARE_TEXTURE;

BEGIN_DECLARE_CUBE_TARGET(NormalizeCubeTexture, Environment)
AddressU = Wrap;
AddressV = Wrap;
AddressW = Wrap;
MinFilter = Linear;
MagFilter = Linear;
MipFilter = None;
END_DECLARE_TEXTURE;

//----------------------------------------------------

// Vertex input structure (used for ALL techniques here!)
struct VertexInput
{
    float3 pos      : SV_POSITION;
    float2 texCoord : TEXCOORD0;
    float3 normal   : NORMAL;
    float3 tangent    : TANGENT;
};

// vertex shader output structure
struct VertexOutput
{
    float4 pos          : SV_POSITION;
    float2 diffTexCoord : TEXCOORD0;
    float2 normTexCoord : TEXCOORD1;
    float3 lightVec     : COLOR0;
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

// Get light direction
float3 GetLightDir()
{
    return lightDir;
}
    
float3x3 ComputeTangentMatrix(float3 tangent, float3 normal)
{
    // Compute the 3x3 tranform from tangent space to object space
    float3x3 worldToTangentSpace;
    worldToTangentSpace[0] =
        //left handed: mul(cross(tangent, normal), world);
        mul(cross(normal, tangent), world);
    worldToTangentSpace[1] = mul(tangent, world);
    worldToTangentSpace[2] = mul(normal, world);
    return worldToTangentSpace;
}

//----------------------------------------------------

// Vertex shader function
VertexOutput VS_Diffuse(VertexInput In)
{
    VertexOutput Out = (VertexOutput) 0; 
    Out.pos = TransformPosition(In.pos);
    // Duplicate texture coordinates for diffuse and normal maps
    Out.diffTexCoord = In.texCoord;
    Out.normTexCoord = In.texCoord;

    // Compute the 3x3 tranform from tangent space to object space
    float3x3 worldToTangentSpace =
        ComputeTangentMatrix(In.tangent, In.normal);

    // Transform light vector and pass it as a color (clamped from 0 to 1)
    Out.lightVec = 0.5 + 0.5 *
        normalize(mul(worldToTangentSpace, GetLightDir()));

    // And pass everything to the pixel shader
    return Out;
}

// Pixel shader function, only used to ps2.0 because of .agb
float4 PS_Diffuse(VertexOutput In) : SV_TARGET
{
    // Grab texture data
    float4 diffusePixel = SAMPLE_TEXTURE(diffuseTexture, In.diffTexCoord);
    float3 normalPixel = SAMPLE_TEXTURE(normalTexture, In.normTexCoord).agb;
    float3 normalVector =
        (2.0 * normalPixel) - 1.0;
    // Normalize normal to fix blocky errors
    normalVector = normalize(normalVector);

    // Unpack the light vector to -1 - 1
    float3 lightVector =
        (2.0 * In.lightVec) - 1.0;

    // Compute the angle to the light
    float bump = saturate(dot(normalVector, lightVector));
    
    float4 ambDiffColor = ambientColor + bump * diffuseColor;
    return diffusePixel * ambDiffColor;
}

TECHNIQUE (Diffuse, VS_Diffuse, PS_Diffuse)
TECHNIQUE(Diffuse20, VS_Diffuse, PS_Diffuse)

// Pixel shader function, only used to ps2.0 because of .agb
float4 PS_Diffuse_Transparent(VertexOutput In) : SV_TARGET
{
    // Grab texture data
    float4 diffusePixel = SAMPLE_TEXTURE(diffuseTexture, In.diffTexCoord);
    float3 normalPixel = SAMPLE_TEXTURE(normalTexture, In.normTexCoord).agb;
    float3 normalVector =
        (2.0 * normalPixel) - 1.0;
    // Normalize normal to fix blocky errors
    normalVector = normalize(normalVector);

    // Unpack the light vector to -1 - 1
    float3 lightVector =
        (2.0 * In.lightVec) - 1.0;

    // Compute the angle to the light
    float bump = saturate(dot(normalVector, lightVector));
    
    float4 ambDiffColor = ambientColor + bump * diffuseColor;
    ambDiffColor.a = 0.33f;
    return diffusePixel * ambDiffColor;
}

// Helper technique to display stuff with transparency in max.
BEGIN_TECHNIQUE(Diffuse20Transparent)
	BEGIN_PASS(P0)
		AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		SHADERS(VS_Diffuse, PS_Diffuse_Transparent)
	END_PASS
END_TECHNIQUE

//------------------------------------------------

// vertex shader output structure (optimized for ps_1_1)
struct VertexOutput_Specular
{
    float4 pos          : SV_POSITION;
    float2 diffTexCoord : TEXCOORD0;
    float2 normTexCoord : TEXCOORD1;
    float3 viewVec      : TEXCOORD2;
    float3 lightVec     : TEXCOORD3;
    float3 lightVecDiv3 : COLOR0;
};

// Vertex shader function
VertexOutput_Specular VS_Specular(VertexInput In)
{
    VertexOutput_Specular Out = (VertexOutput_Specular) 0; 
    Out.pos = TransformPosition(In.pos);
    // Duplicate texture coordinates for diffuse and normal maps
    Out.diffTexCoord = In.texCoord;
    Out.normTexCoord = In.texCoord;

    // Compute the 3x3 tranform from tangent space to object space
    float3x3 worldToTangentSpace =
        ComputeTangentMatrix(In.tangent, In.normal);

    float3 worldEyePos = GetCameraPos();
    float3 worldVertPos = GetWorldPos(In.pos);

    // Transform light vector and pass it as a color (clamped from 0 to 1)
    // For ps_2_0 we don't need to clamp form 0 to 1
    float3 lightVec = normalize(mul(worldToTangentSpace, GetLightDir()));
    Out.lightVec = 0.5 + 0.5 * lightVec;
    Out.lightVecDiv3 = 0.5 + 0.5 * lightVec / 3;
    Out.viewVec = mul(worldToTangentSpace, worldEyePos - worldVertPos);

    // And pass everything to the pixel shader
    return Out;
}

float4 PS_Specular(VertexOutput_Specular In) : SV_TARGET
{
	return SAMPLE_TEXTURE(diffuseTexture, In.diffTexCoord);
}

TECHNIQUE(Specular, VS_Specular, PS_Specular)

//----------------------------------------

// vertex shader output structure
struct VertexOutput_Specular20
{
    float4 pos          : SV_POSITION;
    float2 diffTexCoord : TEXCOORD0;
    float2 normTexCoord : TEXCOORD1;
    float3 lightVec     : TEXCOORD2;
    float3 viewVec      : TEXCOORD3;
};

// Vertex shader function
VertexOutput_Specular20 VS_Specular20(VertexInput In)
{
    VertexOutput_Specular20 Out = (VertexOutput_Specular20) 0; 
    Out.pos = TransformPosition(In.pos);
    // Duplicate texture coordinates for diffuse and normal maps
    Out.diffTexCoord = In.texCoord;
    Out.normTexCoord = In.texCoord;

    // Compute the 3x3 tranform from tangent space to object space
    float3x3 worldToTangentSpace =
        ComputeTangentMatrix(In.tangent, In.normal);

    float3 worldEyePos = GetCameraPos();
    float3 worldVertPos = GetWorldPos(In.pos);

    Out.lightVec = mul(worldToTangentSpace, GetLightDir());
    Out.viewVec = mul(worldToTangentSpace, worldEyePos - worldVertPos);

    // And pass everything to the pixel shader
    return Out;
}

// Pixel shader function
float4 PS_Specular20(VertexOutput_Specular20 In) : SV_TARGET
{
    // Grab texture data
    float4 diffusePixel = SAMPLE_TEXTURE(diffuseTexture, In.diffTexCoord);
    float3 normalVector = (2.0 * SAMPLE_TEXTURE(normalTexture, In.normTexCoord).agb) - 1.0;
    // Normalize normal to fix blocky errors
    normalVector = normalize(normalVector);

    // Additionally normalize the vectors
    float3 lightVector = normalize(In.lightVec);
    float3 viewVector = normalize(In.viewVec);
    // For ps_2_0 we don't need to unpack the vectors to -1 - 1

    // Compute the angle to the light
    float bump = saturate(dot(normalVector, lightVector));
    // Specular factor
    float3 reflect = normalize(2 * bump * normalVector - lightVector);
    float spec = pow(saturate(dot(reflect, viewVector)), shininess);

    float4 ambDiffColor = ambientColor + bump * diffuseColor;
    if (UseAlpha)
    {
        return diffusePixel * ambDiffColor +
            bump * spec * specularColor * diffusePixel.a;
    }
    else
    {
        return float4(diffusePixel.rgb * ambDiffColor +
            bump * spec * specularColor, 1.0f);
    }
}

//----------------------------------------

// Pixel shader function
float4 PS_DiffuseSpecular20(VertexOutput_Specular20 In) : SV_TARGET
{
	// Grab texture data
	float4 diffusePixel = SAMPLE_TEXTURE(diffuseTexture, In.diffTexCoord);
	float3 normalVector = (2.0 * SAMPLE_TEXTURE(normalTexture, In.normTexCoord).agb) - 1.0;
	// Normalize normal to fix blocky errors
	normalVector = normalize(normalVector);

	// Additionally normalize the vectors
	float3 lightVector = normalize(In.lightVec);
	float3 viewVector = normalize(In.viewVec);
	// For ps_2_0 we don't need to unpack the vectors to -1 - 1

	// Compute the angle to the light
	float bump = saturate(dot(normalVector, lightVector));
	// Specular factor
	float3 reflect = normalize(2 * bump * normalVector - lightVector);
	float spec = pow(saturate(dot(reflect, viewVector)), shininess);

	return diffusePixel * (ambientColor +
		bump * (diffuseColor + spec * specularColor));
}

TECHNIQUE(Specular20, VS_Specular20, PS_Specular20)
TECHNIQUE(DiffuseSpecular, VS_Specular20, PS_DiffuseSpecular20)
TECHNIQUE(DiffuseSpecular20, VS_Specular20, PS_DiffuseSpecular20)
// ------------------------------

// vertex shader output structure (optimized for ps_1_1)
struct VertexOutput_SpecularWithReflection
{
    float4 pos          : SV_POSITION;
    float2 diffTexCoord : TEXCOORD0;
    float2 normTexCoord : TEXCOORD1;
    float3 viewVec      : TEXCOORD2;
    float3 cubeTexCoord : TEXCOORD3;
    float3 lightVec     : COLOR0;
    float3 lightVecDiv3 : COLOR1;
};

// Vertex shader function
VertexOutput_SpecularWithReflection VS_SpecularWithReflection(VertexInput In)
{
    VertexOutput_SpecularWithReflection Out = (VertexOutput_SpecularWithReflection) 0; 
    Out.pos = TransformPosition(In.pos);
    // Duplicate texture coordinates for diffuse and normal maps
    Out.diffTexCoord = In.texCoord;
    Out.normTexCoord = In.texCoord;

    // Compute the 3x3 tranform from tangent space to object space
    float3x3 worldToTangentSpace =
        ComputeTangentMatrix(In.tangent, In.normal);

    float3 worldEyePos = GetCameraPos();
    float3 worldVertPos = GetWorldPos(In.pos);

    // Transform light vector and pass it as a color (clamped from 0 to 1)
    // For ps_2_0 we don't need to clamp form 0 to 1
    float3 lightVec = normalize(mul(worldToTangentSpace, GetLightDir()));
    Out.lightVec = 0.5 + 0.5 * lightVec;
    Out.lightVecDiv3 = 0.5 + 0.5 * lightVec / 3;
    Out.viewVec = mul(worldToTangentSpace, worldEyePos - worldVertPos);

    float3 normal = CalcNormalVector(In.normal);
    float3 viewVec = normalize(GetCameraPos() - GetWorldPos(In.pos));
    float3 R = reflect(-viewVec, normal);
    R = float3(R.x, R.z, R.y);
    Out.cubeTexCoord = R;
    
    // And pass everything to the pixel shader
    return Out;
}

// vertex shader output structure
struct VertexOutput_SpecularWithReflection20
{
	float4 pos          : SV_POSITION;
	float2 texCoord     : TEXCOORD0;
	float3 lightVec     : TEXCOORD1;
	float3 viewVec      : TEXCOORD2;
	float3 cubeTexCoord : TEXCOORD3;
};

// Vertex shader function
VertexOutput_SpecularWithReflection20
VS_SpecularWithReflection20(VertexInput In)
{
	VertexOutput_SpecularWithReflection20 Out =
		(VertexOutput_SpecularWithReflection20)0;

	float4 worldVertPos = mul(float4(In.pos.xyz, 1), world);
	Out.pos = mul(worldVertPos, viewProj);

	// Copy texture coordinates for diffuse and normal maps
	Out.texCoord = In.texCoord;

	// Compute the 3x3 tranform from tangent space to object space
	float3x3 worldToTangentSpace =
		ComputeTangentMatrix(In.tangent, In.normal);

	float3 worldEyePos = GetCameraPos();

	// Transform light vector and pass it as a color (clamped from 0 to 1)
	// For ps_2_0 we don't need to clamp form 0 to 1
	Out.lightVec = normalize(mul(worldToTangentSpace, GetLightDir()));
	Out.viewVec = mul(worldToTangentSpace, worldEyePos - worldVertPos);

	float3 normal = CalcNormalVector(In.normal);
	float3 viewVec = normalize(worldEyePos - worldVertPos);
	float3 R = reflect(-viewVec, normal);
	Out.cubeTexCoord = R;

	// And pass everything to the pixel shader
	return Out;
}

// Pixel shader function
float4 PS_SpecularWithReflection20(VertexOutput_SpecularWithReflection20 In) : SV_TARGET
{
	// Grab texture data
	float4 diffusePixel = SAMPLE_TEXTURE(diffuseTexture, In.texCoord);
	float3 normalVector = (2.0 * SAMPLE_TEXTURE(normalTexture, In.texCoord).agb) - 1.0;
	// Normalize normal to fix blocky errors
	normalVector = normalize(normalVector);

	// Additionally normalize the vectors
	float3 lightVector = normalize(In.lightVec);
	float3 viewVector = normalize(In.viewVec);
	// Compute the angle to the light
	float bump = saturate(dot(normalVector, lightVector));
	// Specular factor
	float3 reflect = normalize(2 * bump * normalVector - lightVector);
	float spec = pow(saturate(dot(reflect, viewVector)), shininess);

	// Darken down bump factor on back faces
	float4 reflection = SAMPLE_CUBE(reflectionCubeTexture,
		In.cubeTexCoord);
	float3 ambDiffColor = ambientColor + bump * diffuseColor;
	float4 ret;
	ret.rgb = diffusePixel * ambDiffColor +
		bump * spec * specularColor * diffusePixel.a;
	ret.rgb *= (0.85f + reflection * 0.75f);
	// Apply color
	ret.a = 1.0f;
	return ret;
}

TECHNIQUE(SpecularWithReflection, VS_SpecularWithReflection20, PS_SpecularWithReflection20)
TECHNIQUE(SpecularWithReflection20, VS_SpecularWithReflection20, PS_SpecularWithReflection20)
//----------------------------------------------------

// For ps1.1 we can't do this advanced stuff,
// just render the material with the reflection and basic lighting
struct VertexOutput_Texture
{
    float4 pos          : SV_POSITION;
    float3 cubeTexCoord : TEXCOORD0;
    float3 normal       : TEXCOORD1;
    float3 halfVec        : TEXCOORD2;
};

// vertex shader
VertexOutput_Texture VS_ReflectionSpecular(VertexInput In)
{
    VertexOutput_Texture Out;
    Out.pos = TransformPosition(In.pos);
    float3 normal = CalcNormalVector(In.normal);
    float3 viewVec = normalize(GetCameraPos() - GetWorldPos(In.pos));
    float3 R = reflect(-viewVec, normal);
    R = float3(R.x, R.z, R.y);
    Out.cubeTexCoord = R;
    
    // Determine the eye vector
    float3 worldEyePos = GetCameraPos();
    float3 worldVertPos = GetWorldPos(In.pos);
    
    // Calc normal vector
    Out.normal = 0.5 + 0.5 * CalcNormalVector(In.normal);
    // Eye vector
    float3 eyeVec = normalize(worldEyePos - worldVertPos);
    // Half angle vector
    Out.halfVec = 0.5 + 0.5 * normalize(eyeVec + lightDir);

    return Out;
}

float4 PS_ReflectionSpecular(VertexOutput_Texture In) : SV_TARGET
{
    // Convert colors back to vectors. Without normalization it is
    // a bit faster (2 instructions less), but not as correct!
    float3 normal = 2.0 * (saturate(In.normal)-0.5);
    float3 halfVec = 2.0 * (saturate(In.halfVec)-0.5);

    // Diffuse factor
    float diff = saturate(dot(normal, lightDir));
    // Specular factor
    float spec = saturate(dot(normal, halfVec));
    //max. possible pow fake with mults here: spec = pow(spec, 8);
    //same as: spec = spec*spec*spec*spec*spec*spec*spec*spec;

    // (saturate(4*(dot(N,H)^2-0.75))^2*2 is a close approximation
    // to pow(dot(N,H), 16). I use something like
    // (saturate(4*(dot(N,H)^4-0.75))^2*2 for approx. pow(dot(N,H), 32)
    spec = pow(saturate(4*(pow(spec, 2)-0.795)), 2);

    // Output the color
    float4 diffAmbColor = ambientColor + diff * diffuseColor;

    float3 reflect = In.cubeTexCoord;
    half4 reflColor = SAMPLE_CUBE(reflectionCubeTexture, reflect);
    float4 ret = reflColor * reflectionAmount +
        diffAmbColor;
    ret.a = alphaFactor;
    return ret +
        spec * specularColor;
}

TECHNIQUE(ReflectionSpecular, VS_ReflectionSpecular, PS_ReflectionSpecular)

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
    Out.normal = mul(In.normal, (float3x3)world);
    Out.viewVec = GetCameraPos() - GetWorldPos(In.pos);
    Out.halfVec = Out.viewVec + lightDir;
    return Out;
}

float4 PS_ReflectionSpecular20(VertexOutput20 In) : SV_TARGET
{
    half3 N = normalize(In.normal);
    float3 V = normalize(In.viewVec);
	float3 hV = normalize(In.halfVec);

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
    float spec = pow(saturate(dot(N, hV)), shininess);
    
    // Output the colors
    float4 diffAmbColor = ambientColor + diff * diffuseColor;
    float4 ret;
    ret.rgb = reflColor * reflectionAmount * fresnel * 1.5f +
        diffAmbColor;
    ret.a = alphaFactor;
    ret += spec * specularColor;
    return ret;
}

TECHNIQUE(ReflectionSpecular20, VS_ReflectionSpecular20, PS_ReflectionSpecular20)
//---------------------------------------------------

// vertex shader output structure
struct VertexOutput_SpecularWithReflectionForCar20
{
    float4 pos          : SV_POSITION;
    float2 texCoord     : TEXCOORD0;
    float3 lightVec     : TEXCOORD1;
    float3 viewVec      : TEXCOORD2;
    float3 cubeTexCoord : TEXCOORD3;
};

// Vertex shader function
VertexOutput_SpecularWithReflectionForCar20
    VS_SpecularWithReflectionForCar20(VertexInput In)
{
    VertexOutput_SpecularWithReflectionForCar20 Out =
        (VertexOutput_SpecularWithReflectionForCar20) 0;
    
    float4 worldVertPos = mul(float4(In.pos.xyz, 1), world);
    Out.pos = TransformPosition(In.pos);
    
    // Copy texture coordinates for diffuse and normal maps
    Out.texCoord = In.texCoord;

    // Compute the 3x3 tranform from tangent space to object space
    float3x3 worldToTangentSpace =
        ComputeTangentMatrix(In.tangent, In.normal);

    float3 worldEyePos = GetCameraPos();

    Out.lightVec = mul(worldToTangentSpace, GetLightDir());
    Out.viewVec = mul(worldToTangentSpace, worldEyePos - worldVertPos);

    float3 normal = CalcNormalVector(In.normal);
    float3 viewVec = normalize(worldEyePos - worldVertPos);
    float3 R = reflect(-viewVec, normal);
    Out.cubeTexCoord = R;
    
    // And pass everything to the pixel shader
    return Out;
}

// Pixel shader function
float4 PS_SpecularWithReflectionForCar20(
  VertexOutput_SpecularWithReflectionForCar20 In) : SV_TARGET
{
    // Grab texture data
    float4 diffusePixel = SAMPLE_TEXTURE(diffuseTexture, In.texCoord);
    diffusePixel.rgb = lerp(diffusePixel.rgb, carHueColor, diffusePixel.a);
    
    float3 normalVector = (2.0 * SAMPLE_TEXTURE(normalTexture, In.texCoord).agb) - 1.0;
    // Normalize normal to fix blocky errors
    normalVector = normalize(normalVector);

    // Additionally normalize the vectors
    float3 lightVector = normalize(In.lightVec);
    float3 viewVector = normalize(In.viewVec);
    // Compute the angle to the light
    float bump = saturate(dot(normalVector, lightVector));
    // Specular factor
    float3 reflect = normalize(2 * bump * normalVector - lightVector);
    float spec = pow(saturate(dot(reflect, viewVector)), shininess);

    // Darken down bump factor on back faces
    float4 reflection = SAMPLE_CUBE(reflectionCubeTexture,
        In.cubeTexCoord);
    float3 ambDiffColor = ambientColor + bump * diffuseColor;
    float4 ret;
    ret.rgb = diffusePixel * ambDiffColor +
        bump * spec * specularColor * diffusePixel.a;
    ret.rgb *= (0.85f + reflection * 0.75f);    
    
    // Apply color
    ret.a = 1.0f;
    return ret;
}

TECHNIQUE(SpecularWithReflectionForCar20, VS_SpecularWithReflectionForCar20, PS_SpecularWithReflectionForCar20)

//----------------------------------------------


sampler diffuseTextureRoadSampler = sampler_state
{
    Texture = <diffuseTexture>;
    AddressU  = Wrap;
    AddressV  = Wrap;
    AddressW  = Wrap;
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    MipFilter = Linear;
};

sampler normalTextureRoadSampler = sampler_state
{
    Texture = <normalTexture>;
    AddressU  = Wrap;
    AddressV  = Wrap;
    AddressW  = Wrap;
    MinFilter = Anisotropic;
    MagFilter = Anisotropic;
    MipFilter = Linear;
};

// Pixel shader function
float4 PS_SpecularRoad20(VertexOutput_Specular20 In) : SV_TARGET
{
	// Grab texture data
	float4 diffusePixel = SAMPLE_SAMPLER(diffuseTexture, diffuseTextureRoadSampler, In.diffTexCoord);
	float3 normalVector = (2.0*SAMPLE_SAMPLER(normalTexture, normalTextureRoadSampler, In.normTexCoord).agb) - 1.0;
	// Normalize normal to fix blocky errors
	normalVector = normalize(normalVector);

	// Additionally normalize the vectors
	float3 lightVector = normalize(In.lightVec);
	float3 viewVector = normalize(In.viewVec);
	// For ps_2_0 we don't need to unpack the vectors to -1 - 1

	// Compute the angle to the light
	float bump = saturate(dot(normalVector, lightVector));
	// Specular factor
	float3 reflect = normalize(2 * bump * normalVector - lightVector);
	float spec = pow(saturate(dot(reflect, viewVector)), shininess);

	float4 ambDiffColor = ambientColor + bump * diffuseColor;
	return diffusePixel * ambDiffColor +
		bump * spec * specularColor * diffusePixel.a;
}

TECHNIQUE(SpecularRoad, VS_Specular20, PS_SpecularRoad20)
TECHNIQUE(SpecularRoad20, VS_Specular20, PS_SpecularRoad20)