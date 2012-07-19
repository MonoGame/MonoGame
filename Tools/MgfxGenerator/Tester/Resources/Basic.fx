#ifndef GLOBAL_MATRICES
#define GLOBAL_MATRICES

float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldViewProjection;

#endif

generate an error here
// TODO: add effect parameters here.

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;

    // TODO: add input channels such as texture
    // coordinates and vertex colors here.
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;asdfasd

    // TODO: add vertex shader outputs such as colors and texture
    // coordinates here. These values will automatically be interpolated
    // over the triangle, and provided as input to your pixel shader.
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	output.Position = mul(input.Position,WorldViewProjection);
	output.Color=input.Color;
    // TODO: add your vertex shader code here.

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // TODO: add your pixel shader code here.
	return float4(1,1,1,1);

    return input.Color;
}

technique Technique1 asdfasd
{
    pass pass0
    {
		/*FillMode=WIREFRAME;
		CullMode=CCW;*/

        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
