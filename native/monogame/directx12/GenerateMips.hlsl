// Taken from DirectXTK12
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//
// http://go.microsoft.com/fwlink/?LinkID=615561

#define GenerateMipsRS \
"RootFlags ( DENY_VERTEX_SHADER_ROOT_ACCESS |" \
"            DENY_DOMAIN_SHADER_ROOT_ACCESS |" \
"            DENY_GEOMETRY_SHADER_ROOT_ACCESS |" \
"            DENY_HULL_SHADER_ROOT_ACCESS |" \
"            DENY_PIXEL_SHADER_ROOT_ACCESS )," \
"RootConstants(num32BitConstants=3, b0)," \
"DescriptorTable ( SRV(t0) )," \
"DescriptorTable ( UAV(u0) )," \
"StaticSampler(s0,"\
"           filter =   FILTER_MIN_MAG_LINEAR_MIP_POINT,"\
"           addressU = TEXTURE_ADDRESS_CLAMP,"\
"           addressV = TEXTURE_ADDRESS_CLAMP,"\
"           addressW = TEXTURE_ADDRESS_CLAMP )"

SamplerState Sampler       : register(s0);
Texture2D<float4> SrcMip   : register(t0);
RWTexture2D<float4> OutMip : register(u0);

cbuffer MipConstants : register(b0)
{
    float2 InvOutTexelSize; // texel size for OutMip (NOT SrcMip)
    uint SrcMipIndex;
}

float4 Mip(uint2 coord)
{
    float2 uv = (coord.xy + 0.5) * InvOutTexelSize;
    return SrcMip.SampleLevel(Sampler, uv, SrcMipIndex);
}

[RootSignature(GenerateMipsRS)]
[numthreads(8, 8, 1)]
void main(uint3 DTid : SV_DispatchThreadID)
{
    OutMip[DTid.xy] = Mip(DTid.xy);
}
