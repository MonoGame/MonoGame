//-----------------------------------------------------------------------------
// DistanceFieldSpriteEffect.fx
//
// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
//
// Distance field font rendering effect for MonoGame SpriteFont.
// Supports single-channel SDF (grayscale distance in R channel).
//
// AA width is derived from screen-space derivatives (fwidth) for crisp edges
// at any scale without needing a separate PxRangeScale uniform.
//
// Shader model targets:
//   OGL  (OPENGL=1):           vs_3_0 / ps_3_0  (ps_3_0 supports ddx/ddy)
//   DX11 (SM4=1):              vs_4_0 / ps_4_0  (ps_4_0 supports ddx/ddy)
//   SM6 / Vulkan:              vs_6_0 / ps_6_0
//
// NOTE: We deliberately do NOT use ps_4_0_level_9_1 because that feature
// level disables derivative instructions (ddx/ddy/fwidth).
//-----------------------------------------------------------------------------

#include "Macros.fxh"

DECLARE_TEXTURE(Texture, 0);

BEGIN_CONSTANTS
MATRIX_CONSTANTS

    float4x4 MatrixTransform    _vs(c0) _cb(c0);

END_CONSTANTS

// Distance-field uniforms live outside the constant buffer so they resolve
// to ps constants in GLSL (no cbuffer support in the OGL/SM3 path).
float  Spread           = 8.0;   // half-range in texels
float  OutlineThickness = 0.0;   // outline depth in SDF texel units
float4 OutlineColor     = float4(0, 0, 0, 1);


struct VSOutput
{
    float4 position : SV_Position;
    float4 color    : COLOR0;
    float2 texCoord : TEXCOORD0;
};


VSOutput SpriteVertexShader(
    float4 position : POSITION0,
    float4 color    : COLOR0,
    float2 texCoord : TEXCOORD0)
{
    VSOutput output;
    output.position = mul(position, MatrixTransform);
    output.color    = color;
    output.texCoord = texCoord;
    return output;
}


float4 SpritePixelShader(VSOutput input) : SV_Target0
{
    float4 t = SAMPLE_TEXTURE(Texture, input.texCoord);

    // ------------------------------------------------------------------
    // Distance field path.
    // Atlas encodes: texel value 0.5 = glyph edge, >0.5 = inside glyph.
    // ------------------------------------------------------------------
    float dist = t.r;

    // Convert to signed distance in SDF texel units (positive = inside glyph).
    float d = (dist - 0.5) * max(Spread, 0.0001);

    // Derivative-based AA in the same distance domain as d.
    float w = clamp(fwidth(d), 0.0001, 1.5);

    // Fill alpha: 1 inside glyph, feathered over ~1px at the edge.
    float fillAlpha = smoothstep(-w, w, d);

    // Branchless optional outline ring: band of OutlineThickness outside fill.
    float outlineMask  = step(0.0001, OutlineThickness);
    float outD         = d + max(OutlineThickness, 0.0);
    float outlineAlpha = smoothstep(-w, w, outD) * (1.0 - fillAlpha) * outlineMask;

    // Composite fill (input.color) over outline (OutlineColor).
    float totalAlpha = fillAlpha + outlineAlpha * OutlineColor.a;
    float blend      = (totalAlpha > 0.0001) ? fillAlpha / totalAlpha : 0.0;
    float3 rgb       = lerp(OutlineColor.rgb, input.color.rgb, blend);
    float  a         = saturate(totalAlpha) * input.color.a;

    // Premultiplied alpha output to match SpriteBatch.
    return float4(rgb * a, a);
}


// ----- Technique: shader model chosen per-platform -------------------------
// OGL target (GLSL macro set, no SM4/SM6): compile as ps_3_0 which supports
// derivative instructions and is translated to GLSL by MojoShader.
#if defined(SM6) || defined(VULKAN)
technique SpriteBatch
{
    pass
    {
        VertexShader = compile vs_6_0 SpriteVertexShader();
        PixelShader  = compile ps_6_0 SpritePixelShader();
    }
}
#elif defined(SM4)
technique SpriteBatch
{
    pass
    {
        VertexShader = compile vs_4_0 SpriteVertexShader();
        PixelShader  = compile ps_4_0 SpritePixelShader();
    }
}
#else
// OpenGL / GLSL path: ps_3_0 is the highest profile MojoShader accepts,
// and it supports ddx/ddy derivative instructions.
technique SpriteBatch
{
    pass
    {
        VertexShader = compile vs_3_0 SpriteVertexShader();
        PixelShader  = compile ps_3_0 SpritePixelShader();
    }
}
#endif
