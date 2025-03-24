// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "include.fxh"

DECLARE_TEXTURE(SourceTexture, 0);
DECLARE_TEXTURE(OtherTexture, 1);

float4 PS_Main(float2 uv : TEXCOORD0) : SV_TARGET0
{
    return SAMPLE_TEXTURE(SourceTexture, uv) + SAMPLE_TEXTURE(OtherTexture, uv);
}

technique
{
    pass
    {
        PixelShader = compile PS_PROFILE PS_Main();
    }
}
