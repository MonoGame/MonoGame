//-----------------------------------------------------------------------------
// EnvironmentMapEffect.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#include "Macros.fxh"


DECLARE_TEXTURE(Texture, 0);
DECLARE_CUBEMAP(EnvironmentMap, 1);


BEGIN_CONSTANTS

    float3 EnvironmentMapSpecular   _ps(c0)  _cb(c0);
    float  FresnelFactor            _vs(c0)  _cb(c0.w);
    float  EnvironmentMapAmount     _vs(c1)  _cb(c2.w);

    float4 DiffuseColor             _vs(c2)  _cb(c1);
    float3 EmissiveColor            _vs(c3)  _cb(c2);

    float3 DirLight0Direction       _vs(c4)  _cb(c3);
    float3 DirLight0DiffuseColor    _vs(c5)  _cb(c4);

    float3 DirLight1Direction       _vs(c6)  _cb(c5);
    float3 DirLight1DiffuseColor    _vs(c7)  _cb(c6);

    float3 DirLight2Direction       _vs(c8)  _cb(c7);
    float3 DirLight2DiffuseColor    _vs(c9)  _cb(c8);

    float3 EyePosition              _vs(c10) _cb(c9);

    float3 FogColor                 _ps(c1)  _cb(c10);
    float4 FogVector                _vs(c11) _cb(c11);

    float4x4 World                  _vs(c16) _cb(c12);
    float3x3 WorldInverseTranspose  _vs(c19) _cb(c16);

MATRIX_CONSTANTS

    float4x4 WorldViewProj          _vs(c12) _cb(c0);

END_CONSTANTS


// We don't use these parameters, but Lighting.fxh won't compile without them.
#define SpecularPower           0
#define SpecularColor           float3(0, 0, 0)
#define DirLight0SpecularColor  float3(0, 0, 0)
#define DirLight1SpecularColor  float3(0, 0, 0)
#define DirLight2SpecularColor  float3(0, 0, 0)


#include "Structures.fxh"
#include "Common.fxh"
#include "Lighting.fxh"


float ComputeFresnelFactor(float3 eyeVector, float3 worldNormal)
{
    float viewAngle = dot(eyeVector, worldNormal);
    
    return pow(max(1 - abs(viewAngle), 0), FresnelFactor) * EnvironmentMapAmount;
}


VSOutputTxEnvMap ComputeEnvMapVSOutput(VSInputNmTx vin, uniform bool useFresnel, uniform int numLights)
{
    VSOutputTxEnvMap vout;
    
    float4 pos_ws = mul(vin.Position, World);
    float3 eyeVector = normalize(EyePosition - pos_ws.xyz);
    float3 worldNormal = normalize(mul(vin.Normal, WorldInverseTranspose));

    ColorPair lightResult = ComputeLights(eyeVector, worldNormal, numLights);
    
    vout.PositionPS = mul(vin.Position, WorldViewProj);
    vout.Diffuse = float4(lightResult.Diffuse, DiffuseColor.a);
    
    if (useFresnel)
        vout.Specular.rgb = ComputeFresnelFactor(eyeVector, worldNormal);
    else
        vout.Specular.rgb = EnvironmentMapAmount;
    
    vout.Specular.a = ComputeFogFactor(vin.Position);
    vout.TexCoord = vin.TexCoord;
    vout.EnvCoord = reflect(-eyeVector, worldNormal);

    return vout;
}


// Vertex shader: basic.
VSOutputTxEnvMap VSEnvMap(VSInputNmTx vin)
{
    return ComputeEnvMapVSOutput(vin, false, 3);
}


// Vertex shader: fresnel.
VSOutputTxEnvMap VSEnvMapFresnel(VSInputNmTx vin)
{
    return ComputeEnvMapVSOutput(vin, true, 3);
}


// Vertex shader: one light.
VSOutputTxEnvMap VSEnvMapOneLight(VSInputNmTx vin)
{
    return ComputeEnvMapVSOutput(vin, false, 1);
}


// Vertex shader: one light, fresnel.
VSOutputTxEnvMap VSEnvMapOneLightFresnel(VSInputNmTx vin)
{
    return ComputeEnvMapVSOutput(vin, true, 1);
}


// Pixel shader: basic.
float4 PSEnvMap(VSOutputTxEnvMap pin) : SV_Target0
{
    float4 color = SAMPLE_TEXTURE(Texture, pin.TexCoord) * pin.Diffuse;
    float4 envmap = SAMPLE_CUBEMAP(EnvironmentMap, pin.EnvCoord) * color.a;

    color.rgb = lerp(color.rgb, envmap.rgb, pin.Specular.rgb);
    
    ApplyFog(color, pin.Specular.w);
    
    return color;
}


// Pixel shader: no fog.
float4 PSEnvMapNoFog(VSOutputTxEnvMap pin) : SV_Target0
{
    float4 color = SAMPLE_TEXTURE(Texture, pin.TexCoord) * pin.Diffuse;
    float4 envmap = SAMPLE_CUBEMAP(EnvironmentMap, pin.EnvCoord) * color.a;

    color.rgb = lerp(color.rgb, envmap.rgb, pin.Specular.rgb);
    
    return color;
}


// Pixel shader: specular.
float4 PSEnvMapSpecular(VSOutputTxEnvMap pin) : SV_Target0
{
    float4 color = SAMPLE_TEXTURE(Texture, pin.TexCoord) * pin.Diffuse;
    float4 envmap = SAMPLE_CUBEMAP(EnvironmentMap, pin.EnvCoord) * color.a;

    color.rgb = lerp(color.rgb, envmap.rgb, pin.Specular.rgb);
    color.rgb += EnvironmentMapSpecular * envmap.a;
    
    ApplyFog(color, pin.Specular.w);
    
    return color;
}


// Pixel shader: specular, no fog.
float4 PSEnvMapSpecularNoFog(VSOutputTxEnvMap pin) : SV_Target0
{
    float4 color = SAMPLE_TEXTURE(Texture, pin.TexCoord) * pin.Diffuse;
    float4 envmap = SAMPLE_CUBEMAP(EnvironmentMap, pin.EnvCoord) * color.a;

    color.rgb = lerp(color.rgb, envmap.rgb, pin.Specular.rgb);
    color.rgb += EnvironmentMapSpecular * envmap.a;
    
    return color;
}


// NOTE: The order of the techniques here are
// defined to match the indexing in EnvironmentMapEffect.cs.

TECHNIQUE( EnvironmentMapEffect,						VSEnvMap,			PSEnvMap );
TECHNIQUE( EnvironmentMapEffect_NoFog,					VSEnvMap,			PSEnvMapNoFog );
TECHNIQUE( EnvironmentMapEffect_Fresnel,				VSEnvMapFresnel,	PSEnvMap );
TECHNIQUE( EnvironmentMapEffect_Fresnel_NoFog,			VSEnvMapFresnel,	PSEnvMapNoFog );
TECHNIQUE( EnvironmentMapEffect_Specular,				VSEnvMap,			PSEnvMapSpecular );
TECHNIQUE( EnvironmentMapEffect_Specular_NoFog,			VSEnvMap,			PSEnvMapSpecularNoFog );
TECHNIQUE( EnvironmentMapEffect_Fresnel_Specular,		VSEnvMapFresnel,	PSEnvMapSpecular );
TECHNIQUE( EnvironmentMapEffect_Fresnel_Specular_NoFog,	VSEnvMapFresnel,	PSEnvMapSpecularNoFog );

TECHNIQUE( EnvironmentMapEffect_OneLight,							VSEnvMapOneLight,			PSEnvMap );
TECHNIQUE( EnvironmentMapEffect_OneLight_NoFog,						VSEnvMapOneLight,			PSEnvMapNoFog );
TECHNIQUE( EnvironmentMapEffect_OneLight_Fresnel,					VSEnvMapOneLightFresnel,	PSEnvMap );
TECHNIQUE( EnvironmentMapEffect_OneLight_Fresnel_NoFog,				VSEnvMapOneLightFresnel,	PSEnvMapNoFog );
TECHNIQUE( EnvironmentMapEffect_OneLight_Specular,					VSEnvMapOneLight,			PSEnvMapSpecular );
TECHNIQUE( EnvironmentMapEffect_OneLight_Specular_NoFog,			VSEnvMapOneLight,			PSEnvMapSpecularNoFog );
TECHNIQUE( EnvironmentMapEffect_OneLight_Fresnel_Specular,			VSEnvMapOneLightFresnel,	PSEnvMapSpecular );
TECHNIQUE( EnvironmentMapEffect_OneLight_Fresnel_Specular_NoFog,	VSEnvMapOneLightFresnel,	PSEnvMapSpecularNoFog );
