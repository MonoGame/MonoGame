#include "Include.fxh"

float4 PosOffset;

bool Boolean;
int Int;
int IntArray[3];
float4x4 Mat;
float4x4 MatArray[3];
float4 Quaternion;
float Float;
float FloatArray[3];

DECLARE_TEXTURE(Tex2D, 0);
DECLARE_TEXTURE_3D(Tex3D, 1);
DECLARE_CUBEMAP(TexCube, 2);

float2 Vec2;
float2 Vec2Array[3];
float3 Vec3;
float3 Vec3Array[3];
float4 Vec4;
float4 Vec4Array[3];

struct VInput
{
    float4 pos : POSITION;
    float2 tx : TEXCOORD0;
};

struct PInput
{
    float4 pos : SV_Position;
    float2 tx : TEXCOORD0;
};

PInput VShader(VInput input)
{
    PInput output;
    
    // workaround for VK descriptor set issue.
    // can be removed one that's merged.
    output.pos = input.pos + PosOffset;
    output.tx = input.tx;
    
    return output;
}

float4 PShaderBoolean(PInput input) : SV_TARGET0
{
    if (Boolean)
    {
        return float4(0.2f, 1.0f, 0.3f, 1.0f);
    }
    else
    {
        return float4(1.0f, 0.2f, 0.3f, 1.0f);
    }
}

float4 PShaderInt(PInput input) : SV_TARGET0
{
    float grey = Int / 255.0f;
    return float4(grey.xxx, 1.0f);
}

float4 PShaderIntArray(PInput input) : SV_TARGET0
{
    return float4(IntArray[0] / 255.0f, IntArray[1] / 255.0f, IntArray[2] / 255.0f, 1.0f);
}

float4 PShaderMat(PInput input) : SV_TARGET0
{
    int col = int(input.tx.x * 4.0f);
    
    return Mat[col];
}

float4 PShaderMatArray(PInput input) : SV_TARGET0
{
    int idx = int(input.tx.y * 3.0f);
    int col = int(input.tx.x * 4.0f);
        
    return MatArray[idx][col];
}

float4 PShaderQuat(PInput input) : SV_TARGET0
{
    if (input.tx.x < 0.5f)
    {
        return float4(Quaternion.xy, 0.0f, 1.0f);
    }
        
    return float4(Quaternion.zw, 0.0f, 1.0f);
}

float4 PShaderFloat(PInput input) : SV_TARGET0
{
    return float4(Float.xxx, 1.0f);
}

float4 PShaderFloatArray(PInput input) : SV_TARGET0
{
    return float4(FloatArray[0], FloatArray[1], FloatArray[2], 1.0f);
}

float4 PShaderTex2D(PInput input) : SV_TARGET0
{
    return SAMPLE_TEXTURE(Tex2D, input.tx);
}

float4 PShaderTex3D(PInput input) : SV_TARGET0
{
    float level = floor(input.tx.x * 2.0f);
    float3 tx = float3(frac(input.tx.x * 2.0f), input.tx.y, level);
    
    return SAMPLE_TEXTURE_3D(Tex3D, tx);
}

float4 PShaderTexCube(PInput input) : SV_TARGET0
{
    float3 sampleDir = float3(0, 0, 0);
    
    int axis = floor(input.tx.x * 3.0f);
    sampleDir[axis] = 1.0f;
    
    if (input.tx.y > 0.5f)
    {
        sampleDir[axis] = -1.0f;
    }
    
    return SAMPLE_CUBEMAP(TexCube, sampleDir);
}

float4 PShaderVec2(PInput input) : SV_TARGET0
{
    return float4(Vec2, 0.0f, 1.0f);
}

float4 PShaderVec2Array(PInput input) : SV_TARGET0
{
    int idx = floor(input.tx.x * 3.0f);
    
    return float4(Vec2Array[idx], 0.0f, 1.0f);
}

float4 PShaderVec3(PInput input) : SV_TARGET0
{
    return float4(Vec3, 1.0f);
}

float4 PShaderVec3Array(PInput input) : SV_TARGET0
{
    int idx = floor(input.tx.x * 3.0f);
    
    return float4(Vec3Array[idx], 1.0f);
}

float4 PShaderVec4(PInput input) : SV_TARGET0
{
    int start = floor(input.tx.y * 2.0f) * 2;
    return float4(Vec4[start], Vec4[start + 1], 0.0f, 1.0f);
}

float4 PShaderVec4Array(PInput input) : SV_TARGET0
{
    int idx = floor(input.tx.x * 3.0f);
    int start = floor(input.tx.y * 2.0f) * 2;
    
    return float4(Vec4Array[idx][start], Vec4Array[idx][start + 1], 0.0f, 1.0f);
}

#define TECHNIQUE(name, vertexShader, pixelShader) \
    technique name { pass { VertexShader = compile VS_PROFILE vertexShader(); PixelShader = compile PS_PROFILE pixelShader(); } }

TECHNIQUE(TestBoolean, VShader, PShaderBoolean);
TECHNIQUE(TestInt, VShader, PShaderInt);
TECHNIQUE(TestIntArray, VShader, PShaderIntArray);
TECHNIQUE(TestMat, VShader, PShaderMat);
TECHNIQUE(TestMatArray, VShader, PShaderMatArray);
TECHNIQUE(TestQuat, VShader, PShaderQuat);
TECHNIQUE(TestFloat, VShader, PShaderFloat);
TECHNIQUE(TestFloatArray, VShader, PShaderFloatArray);
TECHNIQUE(TestTex2D, VShader, PShaderTex2D);
TECHNIQUE(TestTex3D, VShader, PShaderTex3D);
TECHNIQUE(TestTexCube, VShader, PShaderTexCube);
TECHNIQUE(TestVec2, VShader, PShaderVec2);
TECHNIQUE(TestVec2Array, VShader, PShaderVec2Array);
TECHNIQUE(TestVec3, VShader, PShaderVec3);
TECHNIQUE(TestVec3Array, VShader, PShaderVec3Array);
TECHNIQUE(TestVec4, VShader, PShaderVec4);
TECHNIQUE(TestVec4Array, VShader, PShaderVec4Array);
