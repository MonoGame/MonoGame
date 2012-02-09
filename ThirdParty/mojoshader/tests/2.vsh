vs.2.0

//------------------------------------------------------------------------------
// Simple Vertex Shader
//
//  Constants Registers:
//
//    c0-c3 = combined model-view-projection matrix
//    c4    = constant color (defined by the application)
//
//  Input Registers:
//
//    v0    = per-vertex position
//    v1    = per-vertex color
//
//  Output Registers:
//
//    oPos  = homogeneous position
//    oD0   = diffuse color
//
//------------------------------------------------------------------------------

dcl_position v0
dcl_color    v1

m4x4 oPos, v0, c0   // Transform the per-vertex position into clip-space
mov oD0, v1         // Use the original per-vertex color specified

//mov oD0, c4         // Uncomment this to use the constant color stored at c4

