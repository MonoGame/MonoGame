//-----------------------------------------------------------------------------
// AlphaTestEffect.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#include "Macros.fxh"


DECLARE_TEXTURE(Texture, 0);


BEGIN_CONSTANTS

    float4 DiffuseColor     _vs(c0) _cb(c0);
    float4 AlphaTest        _ps(c0) _cb(c1);
    float3 FogColor         _ps(c1) _cb(c2);
    float4 FogVector        _vs(c5) _cb(c3);

MATRIX_CONSTANTS

    float4x4 WorldViewProj  _vs(c1) _cb(c0);

END_CONSTANTS


#include "Structures.fxh"
#include "Common.fxh"


// Vertex shader: basic.
VSOutputTx VSAlphaTest(VSInputTx vin)
{
    VSOutputTx vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParams;
    
    vout.TexCoord = vin.TexCoord;

    return vout;
}


// Vertex shader: no fog.
VSOutputTxNoFog VSAlphaTestNoFog(VSInputTx vin)
{
    VSOutputTxNoFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParamsNoFog;
    
    vout.TexCoord = vin.TexCoord;

    return vout;
}


// Vertex shader: vertex color.
VSOutputTx VSAlphaTestVc(VSInputTxVc vin)
{
    VSOutputTx vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParams;
    
    vout.TexCoord = vin.TexCoord;
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Vertex shader: vertex color, no fog.
VSOutputTxNoFog VSAlphaTestVcNoFog(VSInputTxVc vin)
{
    VSOutputTxNoFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParamsNoFog;
    
    vout.TexCoord = vin.TexCoord;
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Pixel shader: less/greater compare function.
float4 PSAlphaTestLtGt(PSInputTx pin) : SV_Target0
{
    float4 color = SAMPLE_TEXTURE(Texture, pin.TexCoord) * pin.Diffuse;

    clip((color.a < AlphaTest.x) ? AlphaTest.z : AlphaTest.w);

    ApplyFog(color, pin.Specular.w);

    return color;
}


// Pixel shader: less/greater compare function, no fog.
float4 PSAlphaTestLtGtNoFog(PSInputTxNoFog pin) : SV_Target0
{
    float4 color = SAMPLE_TEXTURE(Texture, pin.TexCoord) * pin.Diffuse;
    
    clip((color.a < AlphaTest.x) ? AlphaTest.z : AlphaTest.w);

    return color;
}


// Pixel shader: equal/notequal compare function.
float4 PSAlphaTestEqNe(PSInputTx pin) : SV_Target0
{
    float4 color = SAMPLE_TEXTURE(Texture, pin.TexCoord) * pin.Diffuse;
    
    clip((abs(color.a - AlphaTest.x) < AlphaTest.y) ? AlphaTest.z : AlphaTest.w);

    ApplyFog(color, pin.Specular.w);
    
    return color;
}


// Pixel shader: equal/notequal compare function, no fog.
float4 PSAlphaTestEqNeNoFog(PSInputTxNoFog pin) : SV_Target0
{
    float4 color = SAMPLE_TEXTURE(Texture, pin.TexCoord) * pin.Diffuse;
    
    clip((abs(color.a - AlphaTest.x) < AlphaTest.y) ? AlphaTest.z : AlphaTest.w);

    return color;
}


VertexShader VSArray[4] =
{
    compile vs_2_0 VSAlphaTest(),
    compile vs_2_0 VSAlphaTestNoFog(),
    compile vs_2_0 VSAlphaTestVc(),
    compile vs_2_0 VSAlphaTestVcNoFog(),
};


int VSIndices[8] =
{
    0,      // lt/gt
    1,      // lt/gt, no fog
    2,      // lt/gt, vertex color
    3,      // lt/gt, vertex color, no fog
    
    0,      // eq/ne
    1,      // eq/ne, no fog
    2,      // eq/ne, vertex color
    3,      // eq/ne, vertex color, no fog
};


PixelShader PSArray[4] =
{
    compile ps_2_0 PSAlphaTestLtGt(),
    compile ps_2_0 PSAlphaTestLtGtNoFog(),
    compile ps_2_0 PSAlphaTestEqNe(),
    compile ps_2_0 PSAlphaTestEqNeNoFog(),
};


int PSIndices[8] =
{
    0,      // lt/gt
    1,      // lt/gt, no fog
    0,      // lt/gt, vertex color
    1,      // lt/gt, vertex color, no fog
    
    2,      // eq/ne
    3,      // eq/ne, no fog
    2,      // eq/ne, vertex color
    3,      // eq/ne, vertex color, no fog
};


int ShaderIndex = 0;


Technique AlphaTestEffect
{
    Pass
    {
        VertexShader = (VSArray[VSIndices[ShaderIndex]]);
        PixelShader  = (PSArray[PSIndices[ShaderIndex]]);
    }
}
