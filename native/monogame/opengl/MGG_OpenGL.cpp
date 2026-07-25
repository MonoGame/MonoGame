#include "api_MGG.h"

#include "mg_common.h"

#include "OpenGLContext.h"

#include <SDL.h>
#include <SDL_opengl.h>

struct MGG_GraphicsAdapter
{
    std::string deviceName;
    std::string description;
    mgint deviceId = 0;
    mgint revision = 0;
    mgint vendorId = 0;
    mgint subsystemId = 0;
    void* monitorHandle = nullptr;
    MGG_DisplayMode currentDisplayMode = { MGSurfaceFormat::Color, 1280, 720 };
    std::vector<MGG_DisplayMode> modes;
};

struct MGG_GraphicsSystem
{
    std::vector<MGG_GraphicsAdapter*> adapters;
};

struct MGG_BlendState
{
    MGG_BlendState_Info infos[4];
};

struct MGG_DepthStencilState
{
    MGG_DepthStencilState_Info info;
};

struct MGG_RasterizerState
{
    MGG_RasterizerState_Info info;
};

struct MGG_SamplerState
{
    MGG_SamplerState_Info info;
};

struct MGG_GraphicsDevice
{
    SDL_Window* window = nullptr;
    OpenGLContext context;
    mgint frame = 0;
    mgbool isInFrame = false;
    mgint backBufferWidth = 0;
    mgint backBufferHeight = 0;
    MGSurfaceFormat backBufferFormat = MGSurfaceFormat::Color;
    MGDepthFormat depthFormat = MGDepthFormat::None;
    mgint multiSampleCount = 0;
    MGG_BlendState* blendState = nullptr;
    MGG_DepthStencilState* depthStencilState = nullptr;
    MGG_RasterizerState* rasterizerState = nullptr;
};

struct MGG_Buffer
{
};

struct MGG_Texture
{
};

struct MGG_Shader
{
};

struct MGG_InputLayout
{
};

struct MGG_OcclusionQuery
{
};

namespace
{
    constexpr mgint MaxTextureSlots = 16;
    constexpr mgint MaxVertexTextureSlots = 16;
    constexpr mgint MaxVertexBufferSlots = 16;
    constexpr mgint OpenGLShaderProfile = 0;

    [[noreturn]] void MGGL_Fail(const char* file, int line, const char* action, const char* detail)
    {
        char message[512];
        snprintf(message, sizeof(message), "%s, %s", action, detail);
        MG_Print_StdError(file, line, message);
        MG_GENERATE_TRAP();
        abort();
    }

#define MGGL_FAIL(action, detail) MGGL_Fail(__FILE__, __LINE__, action, detail)

    [[noreturn]] void FailNotImplemented(const char* file, int line, const char* functionName)
    {
        char message[256];
        snprintf(message, sizeof(message), "%s is not implemented", functionName);
        MG_Print_StdError(file, line, message);
        MG_GENERATE_TRAP();
        abort();
    }

#define MGGL_NOT_IMPLEMENTED(functionName) FailNotImplemented(__FILE__, __LINE__, functionName);

    MGG_DisplayMode ToDisplayMode(const SDL_DisplayMode& mode)
    {
        MGG_DisplayMode result;
        result.format = MGSurfaceFormat::Color;
        result.width = mode.w;
        result.height = mode.h;
        return result;
    }

    void PopulateDisplayModes(MGG_GraphicsAdapter* adapter, mgint displayIndex)
    {
        adapter->modes.clear();

        SDL_DisplayMode currentMode;
        if (SDL_GetCurrentDisplayMode(displayIndex, &currentMode) == 0)
            adapter->currentDisplayMode = ToDisplayMode(currentMode);
        else
            adapter->currentDisplayMode = { MGSurfaceFormat::Color, 1280, 720 };

        mgint modeCount = SDL_GetNumDisplayModes(displayIndex);
        if (modeCount <= 0)
        {
            adapter->modes.push_back(adapter->currentDisplayMode);
            return;
        }

        adapter->modes.reserve(modeCount);

        for (mgint i = 0; i < modeCount; ++i)
        {
            SDL_DisplayMode mode;
            if (SDL_GetDisplayMode(displayIndex, i, &mode) == 0)
                adapter->modes.push_back(ToDisplayMode(mode));
        }

        if (adapter->modes.empty())
            adapter->modes.push_back(adapter->currentDisplayMode);
    }

    void EnsureContext(MGG_GraphicsDevice* device)
    {
        assert(device != nullptr);

        if (device->window == nullptr || device->context.handle == nullptr)
            MGGL_FAIL("OpenGL device not initialized", "ResizeSwapChain must create a window context before use");

        device->context.MakeCurrent();
    }
}

void MGG_EffectResource_GetByteCode(const char* name, mgbyte*& bytecode, mgint& size)
{
    assert(name != nullptr);
    (void)bytecode;
    (void)size;
    MGGL_NOT_IMPLEMENTED("MGG_EffectResource_GetByteCode");
}

MGG_GraphicsSystem* MGG_GraphicsSystem_Create()
{
    MGG_GraphicsSystem* system = new MGG_GraphicsSystem();

    MGG_GraphicsAdapter* adapter = new MGG_GraphicsAdapter();
    adapter->deviceName = "Display0";
    adapter->description = "MonoGame Native OpenGL backend";
    PopulateDisplayModes(adapter, 0);

    system->adapters.push_back(adapter);

    return system;
}

void MGG_GraphicsSystem_Destroy(MGG_GraphicsSystem* system)
{
    assert(system != nullptr);

    for (MGG_GraphicsAdapter* adapter : system->adapters)
        delete adapter;

    delete system;
}

MGG_GraphicsAdapter* MGG_GraphicsAdapter_Get(MGG_GraphicsSystem* system, mgint index)
{
    assert(system != nullptr);

    if (index < 0 || static_cast<size_t>(index) >= system->adapters.size())
        return nullptr;

    return system->adapters[index];
}

void MGG_GraphicsAdapter_GetInfo(MGG_GraphicsAdapter* adapter, MGG_GraphicsAdaptor_Info& info)
{
    assert(adapter != nullptr);

    info.DeviceName = const_cast<char*>(adapter->deviceName.c_str());
    info.Description = const_cast<char*>(adapter->description.c_str());
    info.DeviceId = adapter->deviceId;
    info.Revision = adapter->revision;
    info.VendorId = adapter->vendorId;
    info.SubSystemId = adapter->subsystemId;
    info.MonitorHandle = adapter->monitorHandle;
    info.DisplayModes = adapter->modes.empty() ? nullptr : adapter->modes.data();
    info.DisplayModeCount = static_cast<mgint>(adapter->modes.size());
    info.CurrentDisplayMode = adapter->currentDisplayMode;
}

MGG_GraphicsDevice* MGG_GraphicsDevice_Create(MGG_GraphicsSystem* system, MGG_GraphicsAdapter* adapter)
{
    assert(system != nullptr);
    assert(adapter != nullptr);

    return new MGG_GraphicsDevice();
}

void MGG_GraphicsDevice_Destroy(MGG_GraphicsDevice* device)
{
    assert(device != nullptr);

    device->context.Destroy();
    delete device;
}

void MGG_GraphicsDevice_GetCaps(MGG_GraphicsDevice* device, MGG_GraphicsDevice_Caps& caps)
{
    assert(device != nullptr);

    caps.MaxTextureSlots = MaxTextureSlots;
    caps.MaxVertexTextureSlots = MaxVertexTextureSlots;
    caps.MaxVertexBufferSlots = MaxVertexBufferSlots;
    caps.ShaderProfile = OpenGLShaderProfile;
}

void MGG_GraphicsDevice_ResizeSwapChain(
    MGG_GraphicsDevice* device,
    void* nativeWindowHandle,
    mgint width,
    mgint height,
    MGSurfaceFormat color,
    MGDepthFormat depth,
    mgint multiSampleCount,
    mgint syncInterval)
{
    assert(device != nullptr);
    assert(nativeWindowHandle != nullptr);
    assert(width > 0);
    assert(height > 0);
    assert(syncInterval >= 0);

    SDL_Window* window = static_cast<SDL_Window*>(nativeWindowHandle);

    device->window = window;
    device->context.Create(window);
    device->context.width = width;
    device->context.height = height;
    device->context.SetSwapInterval(syncInterval);

    device->backBufferWidth = width;
    device->backBufferHeight = height;
    device->backBufferFormat = color;
    device->depthFormat = depth;
    device->multiSampleCount = multiSampleCount;
}

mgint MGG_GraphicsDevice_BeginFrame(MGG_GraphicsDevice* device)
{
    assert(device != nullptr);

    EnsureContext(device);

    if (device->isInFrame)
        MGGL_FAIL("OpenGL frame ownership error", "BeginFrame called while a frame is already active");

    device->isInFrame = true;
    return device->frame;
}

void MGG_GraphicsDevice_Clear(MGG_GraphicsDevice* device, MGClearOptions options, Vector4& color, mgfloat depth, mgint stencil)
{
    (void)options;
    (void)color;
    (void)depth;
    (void)stencil;
    (void)device;
    MGGL_NOT_IMPLEMENTED("MGG_GraphicsDevice_Clear");
}

void MGG_GraphicsDevice_Present(MGG_GraphicsDevice* device, mgint currentFrame, mgint syncInterval)
{
    assert(device != nullptr);
    assert(currentFrame >= 0);
    assert(syncInterval >= 0);

    EnsureContext(device);

    if (!device->isInFrame)
        MGGL_FAIL("OpenGL frame ownership error", "Present called without an active frame");

    device->context.SetSwapInterval(syncInterval);

    SDL_GL_SwapWindow(device->window);

    ++device->frame;
    device->isInFrame = false;
}

void MGG_GraphicsDevice_SetBlendState(MGG_GraphicsDevice* device, MGG_BlendState* state, mgfloat factorR, mgfloat factorG, mgfloat factorB, mgfloat factorA)
{
    (void)device;
    (void)state;
    (void)factorR;
    (void)factorG;
    (void)factorB;
    (void)factorA;
    MGGL_NOT_IMPLEMENTED("MGG_GraphicsDevice_SetBlendState");
}

void MGG_GraphicsDevice_SetDepthStencilState(MGG_GraphicsDevice* device, MGG_DepthStencilState* state)
{
    (void)device;
    (void)state;
    MGGL_NOT_IMPLEMENTED("MGG_GraphicsDevice_SetDepthStencilState");
}

void MGG_GraphicsDevice_SetRasterizerState(MGG_GraphicsDevice* device, MGG_RasterizerState* state)
{
    (void)device;
    (void)state;
    MGGL_NOT_IMPLEMENTED("MGG_GraphicsDevice_SetRasterizerState");
}

void MGG_GraphicsDevice_GetTitleSafeArea(mgint& x, mgint& y, mgint& width, mgint& height)
{
    SDL_Rect bounds;
    if (SDL_GetDisplayUsableBounds(0, &bounds) == 0)
    {
        x = bounds.x;
        y = bounds.y;
        width = bounds.w;
        height = bounds.h;
        return;
    }

    x = 0;
    y = 0;
    width = 0;
    height = 0;
}

void MGG_GraphicsDevice_SetViewport(MGG_GraphicsDevice* device, mgint x, mgint y, mgint width, mgint height, mgfloat minDepth, mgfloat maxDepth)
{
    (void)device;
    (void)x;
    (void)y;
    (void)width;
    (void)height;
    (void)minDepth;
    (void)maxDepth;
    MGGL_NOT_IMPLEMENTED("MGG_GraphicsDevice_SetViewport");
}

void MGG_GraphicsDevice_SetScissorRectangle(MGG_GraphicsDevice* device, mgint x, mgint y, mgint width, mgint height)
{
    (void)device;
    (void)x;
    (void)y;
    (void)width;
    (void)height;
    MGGL_NOT_IMPLEMENTED("MGG_GraphicsDevice_SetScissorRectangle");
}

void MGG_GraphicsDevice_SetRenderTargets(MGG_GraphicsDevice* device, MGG_Texture** targets, mgint* arraySlices, mgint count)
{
    (void)device;
    (void)targets;
    (void)arraySlices;
    (void)count;
    MGGL_NOT_IMPLEMENTED("MGG_GraphicsDevice_SetRenderTargets");
}

void MGG_GraphicsDevice_SetConstantBuffer(MGG_GraphicsDevice* device, MGShaderStage stage, mgint slot, MGG_Buffer* buffer)
{
    (void)device;
    (void)stage;
    (void)slot;
    (void)buffer;
    MGGL_NOT_IMPLEMENTED("MGG_GraphicsDevice_SetConstantBuffer");
}

void MGG_GraphicsDevice_SetTexture(MGG_GraphicsDevice* device, MGShaderStage stage, mgint slot, MGG_Texture* texture)
{
    (void)device;
    (void)stage;
    (void)slot;
    (void)texture;
    MGGL_NOT_IMPLEMENTED("MGG_GraphicsDevice_SetTexture");
}

void MGG_GraphicsDevice_SetSamplerState(MGG_GraphicsDevice* device, MGShaderStage stage, mgint slot, MGG_SamplerState* state)
{
    (void)device;
    (void)stage;
    (void)slot;
    (void)state;
    MGGL_NOT_IMPLEMENTED("MGG_GraphicsDevice_SetSamplerState");
}

void MGG_GraphicsDevice_SetIndexBuffer(MGG_GraphicsDevice* device, MGIndexElementSize size, MGG_Buffer* buffer)
{
    (void)device;
    (void)size;
    (void)buffer;
    MGGL_NOT_IMPLEMENTED("MGG_GraphicsDevice_SetIndexBuffer");
}

void MGG_GraphicsDevice_SetVertexBuffer(MGG_GraphicsDevice* device, mgint slot, MGG_Buffer* buffer, mgint vertexOffset)
{
    (void)device;
    (void)slot;
    (void)vertexOffset;
    (void)buffer;
    MGGL_NOT_IMPLEMENTED("MGG_GraphicsDevice_SetVertexBuffer");
}

void MGG_GraphicsDevice_SetShader(MGG_GraphicsDevice* device, MGShaderStage stage, MGG_Shader* shader)
{
    (void)device;
    (void)stage;
    (void)shader;
    MGGL_NOT_IMPLEMENTED("MGG_GraphicsDevice_SetShader");
}

void MGG_GraphicsDevice_SetInputLayout(MGG_GraphicsDevice* device, MGG_InputLayout* layout)
{
    (void)device;
    (void)layout;
    MGGL_NOT_IMPLEMENTED("MGG_GraphicsDevice_SetInputLayout");
}

void MGG_GraphicsDevice_Draw(MGG_GraphicsDevice* device, MGPrimitiveType primitiveType, mgint vertexStart, mgint vertexCount)
{
    (void)device;
    (void)primitiveType;
    (void)vertexStart;
    (void)vertexCount;
    MGGL_NOT_IMPLEMENTED("MGG_GraphicsDevice_Draw");
}

void MGG_GraphicsDevice_DrawIndexed(MGG_GraphicsDevice* device, MGPrimitiveType primitiveType, mgint primitiveCount, mgint indexStart, mgint vertexStart)
{
    (void)device;
    (void)primitiveType;
    (void)primitiveCount;
    (void)indexStart;
    (void)vertexStart;
    MGGL_NOT_IMPLEMENTED("MGG_GraphicsDevice_DrawIndexed");
}

void MGG_GraphicsDevice_DrawIndexedInstanced(MGG_GraphicsDevice* device, MGPrimitiveType primitiveType, mgint primitiveCount, mgint indexStart, mgint vertexStart, mgint instanceCount)
{
    (void)device;
    (void)primitiveType;
    (void)primitiveCount;
    (void)indexStart;
    (void)vertexStart;
    (void)instanceCount;
    MGGL_NOT_IMPLEMENTED("MGG_GraphicsDevice_DrawIndexedInstanced");
}

void MGG_GraphicsDevice_ResolveRenderTargets(MGG_GraphicsDevice* device)
{
    (void)device;
    MGGL_NOT_IMPLEMENTED("MGG_GraphicsDevice_ResolveRenderTargets");
}

void MGG_GraphicsDevice_GetBackBufferData(MGG_GraphicsDevice* device, mgint x, mgint y, mgint width, mgint height, void* data, mgint count, mgint dataBytes)
{
    (void)device;
    (void)x;
    (void)y;
    (void)width;
    (void)height;
    (void)data;
    (void)count;
    (void)dataBytes;
    MGGL_NOT_IMPLEMENTED("MGG_GraphicsDevice_GetBackBufferData");
}

MGG_BlendState* MGG_BlendState_Create(MGG_GraphicsDevice* device, MGG_BlendState_Info* infos)
{
    (void)device;
    (void)infos;
    MGGL_NOT_IMPLEMENTED("MGG_BlendState_Create");
}

void MGG_BlendState_Destroy(MGG_GraphicsDevice* device, MGG_BlendState* state)
{
    (void)device;
    (void)state;
    MGGL_NOT_IMPLEMENTED("MGG_BlendState_Destroy");
}

MGG_DepthStencilState* MGG_DepthStencilState_Create(MGG_GraphicsDevice* device, MGG_DepthStencilState_Info* info)
{
    (void)device;
    (void)info;
    MGGL_NOT_IMPLEMENTED("MGG_DepthStencilState_Create");
}

void MGG_DepthStencilState_Destroy(MGG_GraphicsDevice* device, MGG_DepthStencilState* state)
{
    (void)device;
    (void)state;
    MGGL_NOT_IMPLEMENTED("MGG_DepthStencilState_Destroy");
}

MGG_RasterizerState* MGG_RasterizerState_Create(MGG_GraphicsDevice* device, MGG_RasterizerState_Info* info)
{
    (void)device;
    (void)info;
    MGGL_NOT_IMPLEMENTED("MGG_RasterizerState_Create");
}

void MGG_RasterizerState_Destroy(MGG_GraphicsDevice* device, MGG_RasterizerState* state)
{
    (void)device;
    (void)state;
    MGGL_NOT_IMPLEMENTED("MGG_RasterizerState_Destroy");
}

MGG_SamplerState* MGG_SamplerState_Create(MGG_GraphicsDevice* device, MGG_SamplerState_Info* info)
{
    (void)device;
    (void)info;
    MGGL_NOT_IMPLEMENTED("MGG_SamplerState_Create");
}

void MGG_SamplerState_Destroy(MGG_GraphicsDevice* device, MGG_SamplerState* state)
{
    (void)device;
    (void)state;
    MGGL_NOT_IMPLEMENTED("MGG_SamplerState_Destroy");
}

MGG_Buffer* MGG_Buffer_Create(MGG_GraphicsDevice* device, MGBufferType type, mgbool dynamic, mgint sizeInBytes)
{
    (void)device;
    (void)type;
    (void)dynamic;
    (void)sizeInBytes;
    MGGL_NOT_IMPLEMENTED("MGG_Buffer_Create");
}

void MGG_Buffer_Destroy(MGG_GraphicsDevice* device, MGG_Buffer* buffer)
{
    (void)device;
    (void)buffer;
    MGGL_NOT_IMPLEMENTED("MGG_Buffer_Destroy");
}

void MGG_Buffer_SetData(MGG_GraphicsDevice* device, MGG_Buffer*& buffer, mgint offset, mgbyte* data, mgint elementCount, mgint vertexStride, mgint elementSizeInBytes, mgbool discard)
{
    (void)device;
    (void)buffer;
    (void)offset;
    (void)data;
    (void)elementCount;
    (void)vertexStride;
    (void)elementSizeInBytes;
    (void)discard;
    MGGL_NOT_IMPLEMENTED("MGG_Buffer_SetData");
}

void MGG_Buffer_GetData(MGG_GraphicsDevice* device, MGG_Buffer* buffer, mgint offset, mgbyte* data, mgint dataCount, mgint dataBytes, mgint dataStride)
{
    (void)device;
    (void)buffer;
    (void)offset;
    (void)data;
    (void)dataCount;
    (void)dataBytes;
    (void)dataStride;
    MGGL_NOT_IMPLEMENTED("MGG_Buffer_GetData");
}

MGG_Texture* MGG_Texture_Create(MGG_GraphicsDevice* device, MGTextureType type, MGSurfaceFormat format, mgint width, mgint height, mgint depth, mgint mipmaps, mgint slices)
{
    (void)device;
    (void)type;
    (void)format;
    (void)width;
    (void)height;
    (void)depth;
    (void)mipmaps;
    (void)slices;
    MGGL_NOT_IMPLEMENTED("MGG_Texture_Create");
}

MGG_Texture* MGG_RenderTarget_Create(MGG_GraphicsDevice* device, MGTextureType type, MGSurfaceFormat format, mgint width, mgint height, mgint depth, mgint mipmaps, mgint slices, MGDepthFormat depthFormat, mgint multiSampleCount, MGRenderTargetUsage usage)
{
    (void)device;
    (void)type;
    (void)format;
    (void)width;
    (void)height;
    (void)depth;
    (void)mipmaps;
    (void)slices;
    (void)depthFormat;
    (void)multiSampleCount;
    (void)usage;
    MGGL_NOT_IMPLEMENTED("MGG_RenderTarget_Create");
}

void MGG_Texture_Destroy(MGG_GraphicsDevice* device, MGG_Texture* texture)
{
    (void)device;
    (void)texture;
    MGGL_NOT_IMPLEMENTED("MGG_Texture_Destroy");
}

void MGG_Texture_SetData(MGG_GraphicsDevice* device, MGG_Texture* texture, mgint level, mgint slice, mgint x, mgint y, mgint z, mgint width, mgint height, mgint depth, mgbyte* data, mgint dataBytes)
{
    (void)device;
    (void)texture;
    (void)level;
    (void)slice;
    (void)x;
    (void)y;
    (void)z;
    (void)width;
    (void)height;
    (void)depth;
    (void)data;
    (void)dataBytes;
    MGGL_NOT_IMPLEMENTED("MGG_Texture_SetData");
}

void MGG_Texture_GetData(MGG_GraphicsDevice* device, MGG_Texture* texture, mgint level, mgint slice, mgint x, mgint y, mgint z, mgint width, mgint height, mgint depth, mgbyte* data, mgint dataBytes)
{
    (void)device;
    (void)texture;
    (void)level;
    (void)slice;
    (void)x;
    (void)y;
    (void)z;
    (void)width;
    (void)height;
    (void)depth;
    (void)data;
    (void)dataBytes;
    MGGL_NOT_IMPLEMENTED("MGG_Texture_GetData");
}

MGG_InputLayout* MGG_InputLayout_Create(MGG_GraphicsDevice* device, MGG_Shader* vertexShader, mgint* strides, mgint streamCount, MGG_InputElement* elements, mgint elementCount)
{
    (void)device;
    (void)vertexShader;
    (void)strides;
    (void)streamCount;
    (void)elements;
    (void)elementCount;
    MGGL_NOT_IMPLEMENTED("MGG_InputLayout_Create");
}

void MGG_InputLayout_Destroy(MGG_GraphicsDevice* device, MGG_InputLayout* layout)
{
    (void)device;
    (void)layout;
    MGGL_NOT_IMPLEMENTED("MGG_InputLayout_Destroy");
}

MGG_Shader* MGG_Shader_Create(MGG_GraphicsDevice* device, MGShaderStage stage, mgbyte* bytecode, mgint sizeInBytes)
{
    (void)device;
    (void)stage;
    (void)bytecode;
    (void)sizeInBytes;
    MGGL_NOT_IMPLEMENTED("MGG_Shader_Create");
}

void MGG_Shader_Destroy(MGG_GraphicsDevice* device, MGG_Shader* shader)
{
    (void)device;
    (void)shader;
    MGGL_NOT_IMPLEMENTED("MGG_Shader_Destroy");
}

MGG_OcclusionQuery* MGG_OcclusionQuery_Create(MGG_GraphicsDevice* device)
{
    (void)device;
    MGGL_NOT_IMPLEMENTED("MGG_OcclusionQuery_Create");
}

void MGG_OcclusionQuery_Destroy(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query)
{
    (void)device;
    (void)query;
    MGGL_NOT_IMPLEMENTED("MGG_OcclusionQuery_Destroy");
}

void MGG_OcclusionQuery_Begin(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query)
{
    (void)device;
    (void)query;
    MGGL_NOT_IMPLEMENTED("MGG_OcclusionQuery_Begin");
}

void MGG_OcclusionQuery_End(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query)
{
    (void)device;
    (void)query;
    MGGL_NOT_IMPLEMENTED("MGG_OcclusionQuery_End");
}

mgbyte MGG_OcclusionQuery_GetResult(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query, mgint& pixelCount)
{
    (void)device;
    (void)query;
    (void)pixelCount;
    MGGL_NOT_IMPLEMENTED("MGG_OcclusionQuery_GetResult");
}
