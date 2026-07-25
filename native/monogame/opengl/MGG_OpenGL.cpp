#include "api_MGG.h"

#include "mg_common.h"

#include "OpenGLContext.h"

#include <SDL.h>
#include <SDL_opengl.h>
#include <array>
#include <cstdint>

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

struct MGG_Buffer;

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
    GLuint vertexArray = 0;
    MGG_Buffer* indexBuffer = nullptr;
    MGIndexElementSize indexElementSize = MGIndexElementSize::SixteenBits;
    std::array<MGG_Buffer*, 16> vertexBuffers = {};
    std::array<mgint, 16> vertexOffsets = {};
};

struct MGG_Buffer
{
    GLuint handle = 0;
    GLenum target = 0;
    GLenum usage = GL_STATIC_DRAW;
    MGBufferType type = MGBufferType::Vertex;
    mgbool dynamic = false;
    mgint sizeInbytes = 0;
    std::vector<mgbyte> shadowData;
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

    void EnsureVertexArray(MGG_GraphicsDevice* device)
    {
        assert(device != nullptr);

        if (device->vertexArray == 0)
            device->vertexArray = device->context.defaultVertexArray;

        device->context.functions.BindVertexArray(device->vertexArray);
    }

    GLbitfield ToClearMask(MGClearOptions options)
    {
        GLbitfield clearMask = 0;

        if ((static_cast<mgint>(options) & static_cast<mgint>(MGClearOptions::Target)) != 0)
            clearMask |= GL_COLOR_BUFFER_BIT;

        if ((static_cast<mgint>(options) & static_cast<mgint>(MGClearOptions::DepthBuffer)) != 0)
            clearMask |= GL_DEPTH_BUFFER_BIT;

        if ((static_cast<mgint>(options) & static_cast<mgint>(MGClearOptions::Stencil)) != 0)
            clearMask |= GL_STENCIL_BUFFER_BIT;

        return clearMask;
    }

    GLenum ToBufferTarget(MGBufferType type)
    {
        switch (type)
        {
        case MGBufferType::Index:
            return GL_ELEMENT_ARRAY_BUFFER;
        case MGBufferType::Vertex:
            return GL_ARRAY_BUFFER;
        case MGBufferType::Constant:
            return GL_UNIFORM_BUFFER;
        default:
            MGGL_FAIL("Unsupported buffer type", "unknown OpenGl buffer target");
        }
    }

    GLenum ToBufferUsage(mgbool dynamic)
    {
        return dynamic ? GL_DYNAMIC_DRAW : GL_STATIC_DRAW;
    }

    GLenum ToPrimitiveMode(MGPrimitiveType primitiveType)
    {
        switch (primitiveType)
        {
        case MGPrimitiveType::TriangleList:
            return GL_TRIANGLES;
        case MGPrimitiveType::TriangleStrip:
            return GL_TRIANGLE_STRIP;
        case MGPrimitiveType::LineList:
            return GL_LINES;
        case MGPrimitiveType::LineStrip:
            return GL_LINE_STRIP;
        case MGPrimitiveType::PointList:
            return GL_POINTS;
        default:
            MGGL_FAIL("Unsupported primitive type", "unknown OpenGL primitive mode");
        }
    }

    mgint GetIndexedElementCount(MGPrimitiveType primitiveType, mgint primitiveCount)
    {
        switch (primitiveType)
        {
        case MGPrimitiveType::TriangleList:
            return primitiveCount * 3;
        case MGPrimitiveType::TriangleStrip:
            return primitiveCount + 2;
        case MGPrimitiveType::LineList:
            return primitiveCount * 2;
        case MGPrimitiveType::LineStrip:
            return primitiveCount + 1;
        case MGPrimitiveType::PointList:
            return primitiveCount;
        default:
            MGGL_FAIL("Unsupported primitive type", "unknown indexed primitive count mapping");
        }
    }

    GLenum ToIndexType(MGIndexElementSize size)
    {
        switch (size)
        {
        case MGIndexElementSize::SixteenBits:
            return GL_UNSIGNED_SHORT;
        case MGIndexElementSize::ThirtyTwoBits:
            return GL_UNSIGNED_INT;
        default:
            MGGL_FAIL("Unsupported index element size", "unknown OpenGL index type");
        }
    }

    mgint GetIndexElementSizeInBytes(MGIndexElementSize size)
    {
        switch (size)
        {
        case MGIndexElementSize::SixteenBits:
            return static_cast<mgint>(sizeof(uint16_t));
        case MGIndexElementSize::ThirtyTwoBits:
            return static_cast<mgint>(sizeof(uint32_t));
        default:
            MGGL_FAIL("Unsupported index element size", "unknown OpenGL index element width");
        }
    }

    mgint GetCopySpan(mgint itemCount, mgint destinationStride, mgint sourceBytes)
    {
        assert(itemCount > 0);
        assert(destinationStride > 0);
        assert(sourceBytes > 0);

        return sourceBytes + (itemCount - 1) * destinationStride;
    }

    void CopyToShadowData(mgbyte* destination, mgbyte* source, mgint itemCount, mgint destinationStride, mgint sourceBytes)
    {
        assert(destination != nullptr);
        assert(source != nullptr);

        if (destinationStride == sourceBytes)
        {
            memcpy(destination, source, itemCount * sourceBytes);
            return;
        }

        for (mgint i = 0; i < itemCount; ++i)
        {
            memcpy(destination + i * destinationStride, source + i * sourceBytes, sourceBytes);
        }
    }

    void CopyFromShadowData(mgbyte* source, mgbyte* destination, mgint itemCount, mgint destinationBytes, mgint sourceStride)
    {
        assert(source != nullptr);
        assert(destination != nullptr);

        if (sourceStride == destinationBytes)
        {
            memcpy(destination, source, itemCount * destinationBytes);
            return;
        }

        mgint bytesToCopy = destinationBytes < sourceStride ? destinationBytes : sourceStride;
        for (mgint i = 0; i < itemCount; ++i)
        {
            memcpy(destination, source, bytesToCopy);
            destination += destinationBytes;
            source += sourceStride;
        }
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
    device->vertexArray = device->context.defaultVertexArray;
    device->context.BindDefaultVertexArray();
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
    assert(device != nullptr);

    if (static_cast<mgint>(options) == 0)
        return;

    EnsureContext(device);
    assert(device->isInFrame);

    glClearColor(color.X, color.Y, color.Z, color.W);
    glClearDepth(depth);
    glClearStencil(stencil);
    glClear(ToClearMask(options));
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
    assert(device != nullptr);
    (void)factorR;
    (void)factorG;
    (void)factorB;
    (void)factorA;
    device->blendState = state;
}

void MGG_GraphicsDevice_SetDepthStencilState(MGG_GraphicsDevice* device, MGG_DepthStencilState* state)
{
    assert(device != nullptr);
    device->depthStencilState = state;
}

void MGG_GraphicsDevice_SetRasterizerState(MGG_GraphicsDevice* device, MGG_RasterizerState* state)
{
    assert(device != nullptr);
    device->rasterizerState = state;
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
    assert(device != nullptr);

    EnsureContext(device);

    glViewport(x, y, width, height);
    glDepthRange(minDepth, maxDepth);
}

void MGG_GraphicsDevice_SetScissorRectangle(MGG_GraphicsDevice* device, mgint x, mgint y, mgint width, mgint height)
{
    assert(device != nullptr);

    EnsureContext(device);

    glScissor(x, y, width, height);
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
    assert(device != nullptr);

    EnsureContext(device);
    EnsureVertexArray(device);

    device->indexBuffer = buffer;
    device->indexElementSize = size;

    GLuint handle = buffer != nullptr ? buffer->handle : 0;
    device->context.functions.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, handle);
}

void MGG_GraphicsDevice_SetVertexBuffer(MGG_GraphicsDevice* device, mgint slot, MGG_Buffer* buffer, mgint vertexOffset)
{
    assert(device != nullptr);
    assert(slot >= 0);
    assert(slot < MaxVertexBufferSlots);
    assert(vertexOffset >= 0);

    device->vertexBuffers[slot] = buffer;
    device->vertexOffsets[slot] = vertexOffset;
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
    assert(device != nullptr);
    assert(vertexStart >= 0);

    if (vertexCount <= 0)
        return;

    EnsureContext(device);
    EnsureVertexArray(device);
    assert(device->isInFrame);

    glDrawArrays(ToPrimitiveMode(primitiveType), vertexStart, vertexCount);
}

void MGG_GraphicsDevice_DrawIndexed(MGG_GraphicsDevice* device, MGPrimitiveType primitiveType, mgint primitiveCount, mgint indexStart, mgint vertexStart)
{
    assert(device != nullptr);
    assert(indexStart >= 0);
    assert(vertexStart >= 0);

    if (primitiveCount <= 0)
        return;

    EnsureContext(device);
    EnsureVertexArray(device);
    assert(device->isInFrame);

    if (device->indexBuffer == nullptr)
        MGGL_FAIL("Indexed draw requires an index buffer", "MGG_GraphicsDevice_SetIndexBuffer must bind a buffer before DrawIndexed");

    mgint indexCount = GetIndexedElementCount(primitiveType, primitiveCount);
    mgint indexSizeIntBytes = GetIndexElementSizeInBytes(device->indexElementSize);
    intptr_t indexByteOffset = static_cast<intptr_t>(indexStart) * indexSizeIntBytes;

    device->context.functions.DrawElementsBaseVertex(
        ToPrimitiveMode(primitiveType),
        indexCount,
        ToIndexType(device->indexElementSize),
        reinterpret_cast<const void*>(indexByteOffset),
        vertexStart);
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
    assert(device != nullptr);
    assert(infos != nullptr);

    MGG_BlendState* state = new MGG_BlendState();
    memcpy(state->infos, infos, sizeof(state->infos));
    return state;
}

void MGG_BlendState_Destroy(MGG_GraphicsDevice* device, MGG_BlendState* state)
{
    assert(device != nullptr);
    delete state;
}

MGG_DepthStencilState* MGG_DepthStencilState_Create(MGG_GraphicsDevice* device, MGG_DepthStencilState_Info* info)
{
    assert(device != nullptr);
    assert(info != nullptr);

    MGG_DepthStencilState* state = new MGG_DepthStencilState();
    state->info = *info;
    return state;
}

void MGG_DepthStencilState_Destroy(MGG_GraphicsDevice* device, MGG_DepthStencilState* state)
{
    assert(device != nullptr);
    delete state;
}

MGG_RasterizerState* MGG_RasterizerState_Create(MGG_GraphicsDevice* device, MGG_RasterizerState_Info* info)
{
    assert(device != nullptr);
    assert(info != nullptr);

    MGG_RasterizerState* state = new MGG_RasterizerState();
    state->info = *info;
    return state;
}

void MGG_RasterizerState_Destroy(MGG_GraphicsDevice* device, MGG_RasterizerState* state)
{
    assert(device != nullptr);
    delete state;
}

MGG_SamplerState* MGG_SamplerState_Create(MGG_GraphicsDevice* device, MGG_SamplerState_Info* info)
{
    assert(device != nullptr);
    assert(info != nullptr);

    MGG_SamplerState* state = new MGG_SamplerState();
    state->info = *info;
    return state;
}

void MGG_SamplerState_Destroy(MGG_GraphicsDevice* device, MGG_SamplerState* state)
{
    assert(device != nullptr);
    delete state;
}

MGG_Buffer* MGG_Buffer_Create(MGG_GraphicsDevice* device, MGBufferType type, mgbool dynamic, mgint sizeInBytes)
{
    assert(device != nullptr);
    assert(sizeInBytes > 0);

    EnsureContext(device);

    MGG_Buffer* buffer = new MGG_Buffer();
    buffer->target = ToBufferTarget(type);
    buffer->usage = ToBufferUsage(dynamic);
    buffer->type = type;
    buffer->dynamic = dynamic;
    buffer->sizeInbytes = sizeInBytes;
    buffer->shadowData.resize(sizeInBytes);

    device->context.functions.GenBuffers(1, &buffer->handle);
    if (buffer->handle == 0)
        MGGL_FAIL("glGenBuffers failed", "buffer creation returned 0");

    device->context.functions.BindBuffer(buffer->target, buffer->handle);
    device->context.functions.BufferData(buffer->target, sizeInBytes, buffer->shadowData.data(), buffer->usage);

    return buffer;
}

void MGG_Buffer_Destroy(MGG_GraphicsDevice* device, MGG_Buffer* buffer)
{
    assert(device != nullptr);

    if (buffer == nullptr)
        return;

    EnsureContext(device);

    if (device->indexBuffer == buffer)
    {
        EnsureVertexArray(device);
        device->context.functions.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        device->indexBuffer = nullptr;
    }

    for (mgint slot = 0; slot < MaxVertexBufferSlots; ++slot)
    {
        if (device->vertexBuffers[slot] == buffer)
        {
            device->vertexBuffers[slot] = nullptr;
            device->vertexOffsets[slot] = 0;
        }
    }

    if (buffer->handle != 0)
        device->context.functions.DeleteBuffers(1, &buffer->handle);

    delete buffer;
}

void MGG_Buffer_SetData(MGG_GraphicsDevice* device, MGG_Buffer*& buffer, mgint offset, mgbyte* data, mgint elementCount, mgint vertexStride, mgint elementSizeInBytes, mgbool discard)
{
    assert(device != nullptr);
    assert(buffer != nullptr);
    assert(data != nullptr);
    assert(offset >= 0);
    assert(elementCount > 0);
    assert(vertexStride > 0);
    assert(elementSizeInBytes > 0);

    mgint copySpan = GetCopySpan(elementCount, vertexStride, elementSizeInBytes);
    assert(offset + copySpan <= buffer->sizeInbytes);

    CopyToShadowData(buffer->shadowData.data() + offset, data, elementCount, vertexStride, elementSizeInBytes);

    EnsureContext(device);
    device->context.functions.BindBuffer(buffer->target, buffer->handle);

    if (discard)
    {
        device->context.functions.BufferData(
            buffer->target,
            buffer->sizeInbytes,
            buffer->shadowData.data(),
            buffer->usage);
        return;
    }

    device->context.functions.BufferSubData(
        buffer->target,
        offset,
        copySpan,
        buffer->shadowData.data() + offset);
}

void MGG_Buffer_GetData(MGG_GraphicsDevice* device, MGG_Buffer* buffer, mgint offset, mgbyte* data, mgint dataCount, mgint dataBytes, mgint dataStride)
{
    assert(device != nullptr);
    assert(buffer != nullptr);
    assert(data != nullptr);
    assert(offset >= 0);
    assert(dataCount > 0);
    assert(dataBytes > 0);
    assert(dataStride > 0);

    mgint copySpan = GetCopySpan(dataCount, dataStride, dataBytes);
    assert(offset + copySpan <= buffer->sizeInbytes);

    CopyFromShadowData(buffer->shadowData.data() + offset, data, dataCount, dataBytes, dataStride);
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
