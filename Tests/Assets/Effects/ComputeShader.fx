struct Input
{
    float2 pos;
};

struct Output
{
    int collisions;
};

StructuredBuffer<Input> Inputs;
RWStructuredBuffer<Output> Outputs;

int ObjectCount;
float ObjectSize;

[numthreads(10, 1, 1)]
void CS(uint3 localID : SV_GroupThreadID, uint3 dispatchID : SV_GroupID,
	    uint localIndex : SV_GroupIndex, uint3 globalID : SV_DispatchThreadID)
{
    float2 pos = Inputs[globalID.x].pos;
    int collisions = 0;
    
    for (int i = 0; i < ObjectCount; i++)
    {
        if ((uint) i == globalID.x)
            continue;
        
        float2 posOther = Inputs[i].pos;
        float dist = distance(pos, posOther);
        if (dist < ObjectSize)
            collisions++;
    }
    
    Outputs[globalID.x].collisions = collisions;
}

technique Tech0
{
    pass Pass0
    {
        ComputeShader = compile cs_5_0 CS();
    }
}
