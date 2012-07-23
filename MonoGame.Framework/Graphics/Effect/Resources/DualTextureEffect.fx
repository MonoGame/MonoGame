//-----------------------------------------------------------------------------
// DualTextureEffect.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#include "Macros.fxh"


DECLARE_TEXTURE(Texture, 0);
DECLARE_TEXTURE(Texture2, 1);


BEGIN_CONSTANTS

    float4 DiffuseColor     _vs(c0) _cb(c0);
    float3 FogColor         _ps(c0) _cb(c1);
    float4 FogVector        _vs(c5) _cb(c2);

MATRIX_CONSTANTS

    float4x4 WorldViewProj  _vs(c1) _cb(c0);

END_CONSTANTS


#include "Structures.fxh"
#include "Common.fxh"


// Vertex shader: basic.
VSOutputTx2 VSDualTexture(VSInputTx2 vin)
{
    VSOutputTx2 vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParams;
    
    vout.TexCoord = vin.TexCoord;
    vout.TexCoord2 = vin.TexCoord2;

    return vout;
}


// Vertex shader: no fog.
VSOutputTx2NoFog VSDualTextureNoFog(VSInputTx2 vin)
{
    VSOutputTx2NoFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParamsNoFog;
    
    vout.TexCoord = vin.TexCoord;
    vout.TexCoord2 = vin.TexCoord2;

    return vout;
}


// Vertex shader: vertex color.
VSOutputTx2 VSDualTextureVc(VSInputTx2Vc vin)
{
    VSOutputTx2 vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParams;
    
    vout.TexCoord = vin.TexCoord;
    vout.TexCoord2 = vin.TexCoord2;
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Vertex shader: vertex color, no fog.
VSOutputTx2NoFog VSDualTextureVcNoFog(VSInputTx2Vc vin)
{
    VSOutputTx2NoFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParamsNoFog;
    
    vout.TexCoord = vin.TexCoord;
    vout.TexCoord2 = vin.TexCoord2;
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Pixel shader: basic.
float4 PSDualTexture(VSOutputTx2 pin) : SV_Target0
{
    float4 color = SAMPLE_TEXTURE(Texture, pin.TexCoord);
    float4 overlay = SAMPLE_TEXTURE(Texture2, pin.TexCoord2);

    color.rgb *= 2;    
    color *= overlay * pin.Diffuse;
    
    ApplyFog(color, pin.Specular.w);
    
    return color;
}


// Pixel shader: no fog.
float4 PSDualTextureNoFog(VSOutputTx2NoFog pin) : SV_Target0
{
    float4 color = SAMPLE_TEXTURE(Texture, pin.TexCoord);
    float4 overlay = SAMPLE_TEXTURE(Texture2, pin.TexCoord2);
    
    color.rgb *= 2;    
    color *= overlay * pin.Diffuse;
    
    return color;
}


// NOTE: The order of the techniques here are
// defined to match the indexing in DualTextureEffect.cs.

TECHNIQUE( DualTextureEffect,					VSDualTexture,			PSDualTexture );
TECHNIQUE( DualTextureEffect_NoFog,				VSDualTextureNoFog,		PSDualTextureNoFog );
TECHNIQUE( DualTextureEffect_VertexColor,		VSDualTextureVc,		PSDualTexture );
TECHNIQUE( DualTextureEffect_VertexColor_NoFog,	VSDualTextureVcNoFog,	PSDualTextureNoFog );
