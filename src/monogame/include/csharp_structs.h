//
// This code is auto generated, don't modify it by hand.
// To regenerate it run: Tools/MonoGame.Generator.CTypes
//

#pragma once

#include "csharp_common.h"

struct MGG_DisplayMode
{
    mgint width;
    mgint height;
    MGSurfaceFormat format;
};

struct MGG_GraphicsAdaptor_Info
{
    void* DeviceName;
    void* Description;
    mgint DeviceId;
    mgint Revision;
    mgint VendorId;
    mgint SubSystemId;
    void* MonitorHandle;
    MGG_DisplayMode* DisplayModes;
    mgint DisplayModeCount;
    MGG_DisplayMode CurrentDisplayMode;
};

struct MGG_GraphicsDevice_Caps
{
    mgint MaxTextureSlots;
    mgint MaxVertexTextureSlots;
    mgint MaxVertexBufferSlots;
    mgint ShaderProfile;
};

struct Vector4
{
    mgfloat X;
    mgfloat Y;
    mgfloat Z;
    mgfloat W;
};

struct MGG_BlendState_Info
{
    MGBlend colorSourceBlend;
    MGBlend colorDestBlend;
    MGBlendFunction colorBlendFunc;
    MGBlend alphaSourceBlend;
    MGBlend alphaDestBlend;
    MGBlendFunction alphaBlendFunc;
    MGColorWriteChannels colorWriteChannels;
};

struct MGG_DepthStencilState_Info
{
    mgbool depthBufferEnable;
    mgbool depthBufferWriteEnable;
    MGCompareFunction depthBufferFunction;
    mgint referenceStencil;
    mgbool stencilEnable;
    mgint stencilMask;
    mgint stencilWriteMask;
    MGCompareFunction stencilFunction;
    MGStencilOperation stencilDepthBufferFail;
    MGStencilOperation stencilFail;
    MGStencilOperation stencilPass;
};

struct MGG_RasterizerState_Info
{
    MGFillMode fillMode;
    MGCullMode cullMode;
    mgbool scissorTestEnable;
    mgbool depthClipEnable;
    mgfloat depthBias;
    mgfloat slopeScaleDepthBias;
    mgbool multiSampleAntiAlias;
};

struct MGG_SamplerState_Info
{
    MGTextureAddressMode AddressU;
    MGTextureAddressMode AddressV;
    MGTextureAddressMode AddressW;
    mguint BorderColor;
    MGTextureFilter Filter;
    MGTextureFilterMode FilterMode;
    mgint MaximumAnisotropy;
    mgint MaxMipLevel;
    mgfloat MipMapLevelOfDetailBias;
    MGCompareFunction ComparisonFunction;
};

struct MGG_InputElement
{
    void* SemanticName;
    mguint SemanticIndex;
    MGVertexElementFormat Format;
    mguint InputSlot;
    mguint AlignedByteOffset;
    mguint InstanceDataStepRate;
};

struct MGP_KeyEvent
{
    void* Window;
    mguint Character;
    MGKeys Key;
};

struct MGP_MouseMoveEvent
{
    void* Window;
    mgint X;
    mgint Y;
};

struct MGP_MouseButtonEvent
{
    void* Window;
    MGMouseButton Button;
};

struct MGP_MouseWheelEvent
{
    void* Window;
    mgint Scroll;
    mgint ScrollH;
};

struct MGP_DropEvent
{
    void* Window;
    void* File;
};

struct MGP_WindowEvent
{
    void* Window;
    mgint Data1;
    mgint Data2;
};

struct MGP_ControllerEvent
{
    mgint Id;
    MGControllerInput Input;
    mgshort Value;
};

#pragma pack(push,1)
struct MGP_Event
{
union {
    MGEventType Type;
    MG_FIELD_OFFSET(4, mgulong, Timestamp);
    MG_FIELD_OFFSET(12, MGP_KeyEvent, Key);
    MG_FIELD_OFFSET(12, MGP_MouseMoveEvent, MouseMove);
    MG_FIELD_OFFSET(12, MGP_MouseButtonEvent, MouseButton);
    MG_FIELD_OFFSET(12, MGP_MouseWheelEvent, MouseWheel);
    MG_FIELD_OFFSET(12, MGP_DropEvent, Drop);
    MG_FIELD_OFFSET(12, MGP_WindowEvent, Window);
    MG_FIELD_OFFSET(12, MGP_ControllerEvent, Controller);
};
};
#pragma pack(pop)

