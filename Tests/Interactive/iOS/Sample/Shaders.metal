#include <metal_stdlib>
#include <simd/simd.h>

using namespace metal;

// Variables in constant address space
constant float3 light_position = float3(0.0, 1.0, -1.0);
constant float4 ambient_color  = float4(0.18, 0.24, 0.8, 1.0);
constant float4 diffuse_color  = float4(0.4, 0.4, 1.0, 1.0);

typedef struct
{
    float4x4 modelview_projection_matrix;
    float4x4 normal_matrix;
} uniforms_t;

typedef struct
{
    packed_float3 position;
    packed_float3 normal;
} vertex_t;

typedef struct {
    float4 position [[position]];
    half4  color;
} ColorInOut;

// Vertex shader function
vertex ColorInOut lighting_vertex(device vertex_t* vertex_array [[ buffer(0) ]],
                                  constant uniforms_t& uniforms [[ buffer(1) ]],
                                  unsigned int vid [[ vertex_id ]])
{
    ColorInOut out;
    
    float4 in_position = float4(float3(vertex_array[vid].position), 1.0);
    out.position = uniforms.modelview_projection_matrix * in_position;
    
    
    float3 normal = vertex_array[vid].normal;
    float4 eye_normal = normalize(uniforms.normal_matrix * float4(normal, 0.0));
    float n_dot_l = dot(eye_normal.rgb, normalize(light_position));
    n_dot_l = fmax(0.0, n_dot_l);
    
    out.color = half4(ambient_color + diffuse_color * n_dot_l);
    
    return out;
}

// Fragment shader function
fragment half4 lighting_fragment(ColorInOut in [[stage_in]])
{
    return in.color;
}
