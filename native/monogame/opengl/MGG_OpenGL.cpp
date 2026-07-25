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
    GLuint handle = 0;
};

struct MGG_Buffer;
struct MGG_Texture;

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
    std::array<std::array<MGG_Texture*, 16>, static_cast<size_t>(MGShaderStage::Count)> textures = {};
    std::array<std::array<MGG_SamplerState*, 16>, static_cast<size_t>(MGShaderStage::Count)> samplers = {};
    std::array<std::array<GLenum, 16>, static_cast<size_t>(MGShaderStage::Count)> textureTargets = {};
    MGG_Texture* currentRenderTarget = nullptr;
    GLuint currentFramebuffer = 0;
};

struct MGG_Buffer
{
    GLuint handle = 0;
    GLenum target = 0;
    GLenum usage = GL_STATIC_DRAW;
    MGBufferType type = MGBufferType::Vertex;
    mgbool dynamic = false;
    mgint sizeInBytes = 0;
    std::vector<mgbyte> shadowData;
};

struct MGG_Texture
{
    GLuint handle = 0;
    GLenum target = GL_TEXTURE_2D;
    MGTextureType type = MGTextureType::_2D;
    MGSurfaceFormat format = MGSurfaceFormat::Color;
    GLenum internalFormat = GL_RGBA8;
    GLenum pixelFormat = GL_RGBA;
    GLenum pixelType = GL_UNSIGNED_BYTE;
    mgint width = 0;
    mgint height = 0;
    mgint depth = 0;
    mgint mipmaps = 0;
    mgint slices = 0;
    mgint bytesPerPixel = 0;
    mgbool isRenderTarget = false;
    MGDepthFormat depthFormat = MGDepthFormat::None;
    mgint multiSampleCount = 0;
    MGRenderTargetUsage renderTargetUsage = MGRenderTargetUsage::DiscardContents;
    GLuint framebuffer = 0;
    GLuint depthRenderbuffer = 0;
    std::vector<std::vector<mgbyte>> shadowData;
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
    constexpr size_t ShaderStageCount = static_cast<size_t>(MGShaderStage::Count);

    struct TextureFormatInfo
    {
        GLenum internalFormat;
        GLenum pixelFormat;
        GLenum pixelType;
        mgint bytesPerPixel;
        GLint swizzleR;
        GLint swizzleG;
        GLint swizzleB;
        GLint swizzleA;
        bool usesSwizzle;
    };

    [[noreturn]] void MGGL_Fail(const char* file, int line, const char* action, const char* detail)
    {
        char message[512];
        snprintf(message, sizeof(message), "%s: %s", action, detail);
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

#define MGGL_NOT_IMPLEMENTED(functionName) FailNotImplemented(__FILE__, __LINE__, functionName)

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

    size_t ToStageIndex(MGShaderStage stage)
    {
        switch (stage)
        {
        case MGShaderStage::Vertex:
            return 0;
        case MGShaderStage::Pixel:
            return 1;
        default:
            MGGL_FAIL("Unsupported shader stage", "unknown OpenGL shader stage");
        }
    }

    mgint GetTextureSlotLimit(MGShaderStage stage)
    {
        switch (stage)
        {
        case MGShaderStage::Vertex:
            return MaxVertexTextureSlots;
        case MGShaderStage::Pixel:
            return MaxTextureSlots;
        default:
            MGGL_FAIL("Unsupported shader stage", "unknown OpenGL texture slot range");
        }
    }

    GLuint GetTextureUnit(MGShaderStage stage, mgint slot)
    {
        assert(slot >= 0);

        switch (stage)
        {
        case MGShaderStage::Vertex:
            return static_cast<GLuint>(MaxTextureSlots + slot);
        case MGShaderStage::Pixel:
            return static_cast<GLuint>(slot);
        default:
            MGGL_FAIL("Unsupported shader stage", "unknown OpenGL texture unit mapping");
        }
    }

    GLenum ToTextureAddressMode(MGTextureAddressMode mode)
    {
        switch (mode)
        {
        case MGTextureAddressMode::Wrap:
            return GL_REPEAT;
        case MGTextureAddressMode::Clamp:
            return GL_CLAMP_TO_EDGE;
        case MGTextureAddressMode::Mirror:
            return GL_MIRRORED_REPEAT;
        case MGTextureAddressMode::Border:
            return GL_CLAMP_TO_BORDER;
        default:
            MGGL_FAIL("Unsupported texture address mode", "unknown OpenGL texture wrap mode");
        }
    }

    GLenum ToCompareFunction(MGCompareFunction function)
    {
        switch (function)
        {
        case MGCompareFunction::Always:
            return GL_ALWAYS;
        case MGCompareFunction::Never:
            return GL_NEVER;
        case MGCompareFunction::Less:
            return GL_LESS;
        case MGCompareFunction::LessEqual:
            return GL_LEQUAL;
        case MGCompareFunction::Equal:
            return GL_EQUAL;
        case MGCompareFunction::GreaterEqual:
            return GL_GEQUAL;
        case MGCompareFunction::Greater:
            return GL_GREATER;
        case MGCompareFunction::NotEqual:
            return GL_NOTEQUAL;
        default:
            MGGL_FAIL("Unsupported comparison function", "unknown OpenGL comparison function");
        }
    }

    void ToTextureFilters(MGTextureFilter filter, GLenum& minFilter, GLenum& magFilter)
    {
        switch (filter)
        {
        case MGTextureFilter::Point:
            minFilter = GL_NEAREST_MIPMAP_NEAREST;
            magFilter = GL_NEAREST;
            return;
        case MGTextureFilter::Linear:
        case MGTextureFilter::Anisotropic:
            minFilter = GL_LINEAR_MIPMAP_LINEAR;
            magFilter = GL_LINEAR;
            return;
        case MGTextureFilter::LinearMipPoint:
            minFilter = GL_LINEAR_MIPMAP_NEAREST;
            magFilter = GL_LINEAR;
            return;
        case MGTextureFilter::PointMipLinear:
            minFilter = GL_NEAREST_MIPMAP_LINEAR;
            magFilter = GL_NEAREST;
            return;
        case MGTextureFilter::MinLinearMagPointMipLinear:
            minFilter = GL_LINEAR_MIPMAP_LINEAR;
            magFilter = GL_NEAREST;
            return;
        case MGTextureFilter::MinLinearMagPointMipPoint:
            minFilter = GL_LINEAR_MIPMAP_NEAREST;
            magFilter = GL_NEAREST;
            return;
        case MGTextureFilter::MinPointMagLinearMipLinear:
            minFilter = GL_NEAREST_MIPMAP_LINEAR;
            magFilter = GL_LINEAR;
            return;
        case MGTextureFilter::MinPointMagLinearMipPoint:
            minFilter = GL_NEAREST_MIPMAP_NEAREST;
            magFilter = GL_LINEAR;
            return;
        default:
            MGGL_FAIL("Unsupported texture filter", "unknown OpenGL texture filter");
        }
    }

    TextureFormatInfo GetTextureFormatInfo(MGSurfaceFormat format)
    {
        switch (format)
        {
        case MGSurfaceFormat::Color:
            return { GL_RGBA8, GL_RGBA, GL_UNSIGNED_BYTE, 4, GL_RED, GL_GREEN, GL_BLUE, GL_ALPHA, false };
        case MGSurfaceFormat::ColorSRgb:
            return { GL_SRGB8_ALPHA8, GL_RGBA, GL_UNSIGNED_BYTE, 4, GL_RED, GL_GREEN, GL_BLUE, GL_ALPHA, false };
        case MGSurfaceFormat::Bgra32:
            return { GL_RGBA8, GL_BGRA, GL_UNSIGNED_BYTE, 4, GL_RED, GL_GREEN, GL_BLUE, GL_ALPHA, false };
        case MGSurfaceFormat::Bgra32SRgb:
            return { GL_SRGB8_ALPHA8, GL_BGRA, GL_UNSIGNED_BYTE, 4, GL_RED, GL_GREEN, GL_BLUE, GL_ALPHA, false };
        case MGSurfaceFormat::Alpha8:
            return { GL_R8, GL_RED, GL_UNSIGNED_BYTE, 1, GL_ONE, GL_ONE, GL_ONE, GL_RED, true };
        default:
            MGGL_FAIL("Unsupported surface format", "OpenGL texture format is not mapped");
        }
    }

    GLenum ToDepthRenderbufferFormat(MGDepthFormat format)
    {
        switch (format)
        {
        case MGDepthFormat::None:
            return 0;
        case MGDepthFormat::Depth16:
            return GL_DEPTH_COMPONENT16;
        case MGDepthFormat::Depth24:
            return GL_DEPTH_COMPONENT24;
        case MGDepthFormat::Depth24Stencil8:
            return GL_DEPTH24_STENCIL8;
        default:
            MGGL_FAIL("Unsupported depth format", "depth renderbuffer format is not mapped");
        }
    }

    GLenum ToDepthAttachment(MGDepthFormat format)
    {
        switch (format)
        {
        case MGDepthFormat::None:
            return 0;
        case MGDepthFormat::Depth16:
        case MGDepthFormat::Depth24:
            return GL_DEPTH_ATTACHMENT;
        case MGDepthFormat::Depth24Stencil8:
            return GL_DEPTH_STENCIL_ATTACHMENT;
        default:
            MGGL_FAIL("Unsupported depth format", "unknown OpenGL framebuffer attachment");
        }
    }

    mgint GetMipExtent(mgint baseExtent, mgint level)
    {
        assert(baseExtent > 0);
        assert(level >= 0);

        mgint extent = baseExtent >> level;
        return extent > 0 ? extent : 1;
    }

    mgint GetTextureSubresourceIndex(const MGG_Texture* texture, mgint level, mgint slice)
    {
        assert(texture != nullptr);
        assert(level >= 0);
        assert(level < texture->mipmaps);
        assert(slice >= 0);
        assert(slice < texture->slices);

        return slice * texture->mipmaps + level;
    }

    mgint GetTextureByteCount(mgint width, mgint height, mgint depth, mgint bytesPerPixel)
    {
        assert(width > 0);
        assert(height > 0);
        assert(depth > 0);
        assert(bytesPerPixel > 0);
        return width * height * depth * bytesPerPixel;
    }

    void ResolveTextureRegion(
        const MGG_Texture* texture,
        mgint level,
        mgint slice,
        mgint x,
        mgint y,
        mgint z,
        mgint width,
        mgint height,
        mgint depth,
        mgint& resolvedWidth,
        mgint& resolvedHeight,
        mgint& resolvedDepth)
    {
        assert(texture != nullptr);
        assert(level >= 0);
        assert(level < texture->mipmaps);
        assert(slice >= 0);
        assert(slice < texture->slices);
        assert(x >= 0);
        assert(y >= 0);
        assert(z >= 0);

        mgint mipWidth = GetMipExtent(texture->width, level);
        mgint mipHeight = GetMipExtent(texture->height, level);
        mgint mipDepth = GetMipExtent(texture->depth, level);

        resolvedWidth = width > 0 ? width : mipWidth - x;
        resolvedHeight = height > 0 ? height : mipHeight - y;
        resolvedDepth = depth > 0 ? depth : mipDepth - z;

        if (resolvedWidth <= 0 || resolvedHeight <= 0 || resolvedDepth <= 0)
            MGGL_FAIL("Invalid texture region", "resolved upload region must be positive");

        if (x + resolvedWidth > mipWidth || y + resolvedHeight > mipHeight || z + resolvedDepth > mipDepth)
            MGGL_FAIL("Invalid texture region", "requested upload region exceeds texture bounds");
    }

    mgint NormalizeUploadByteCount(const MGG_Texture* texture, mgint width, mgint height, mgint depth, mgint dataBytes)
    {
        assert(texture != nullptr);
        assert(dataBytes > 0);

        mgint pixelCount = width * height * depth;
        mgint expectedBytes = GetTextureByteCount(width, height, depth, texture->bytesPerPixel);

        if (texture->bytesPerPixel > 1 && dataBytes == pixelCount)
            return expectedBytes;

        return dataBytes;
    }

    GLenum GetTextureBindingEnum(GLenum target)
    {
        switch (target)
        {
        case GL_TEXTURE_2D:
            return GL_TEXTURE_BINDING_2D;
        default:
            MGGL_FAIL("Unsupported texture target", "OpenGL texture target is not mapped");
        }
    }

    void BeginTextureEdit(MGG_GraphicsDevice* device, MGG_Texture* texture, GLint& previousActiveTexture, GLint& previousBinding)
    {
        assert(device != nullptr);
        assert(texture != nullptr);

        glGetIntegerv(GL_ACTIVE_TEXTURE, &previousActiveTexture);
        device->context.functions.ActiveTexture(GL_TEXTURE0);
        glGetIntegerv(GetTextureBindingEnum(texture->target), &previousBinding);
        glBindTexture(texture->target, texture->handle);
    }

    void EndTextureEdit(MGG_GraphicsDevice* device, MGG_Texture* texture, GLint previousActiveTexture, GLint previousBinding)
    {
        assert(device != nullptr);
        assert(texture != nullptr);

        glBindTexture(texture->target, static_cast<GLuint>(previousBinding));
        device->context.functions.ActiveTexture(static_cast<GLenum>(previousActiveTexture));
    }

    void BeginFramebufferEdit(MGG_GraphicsDevice* device, GLint& previousFramebuffer, GLint& previousRenderbuffer)
    {
        assert(device != nullptr);

        glGetIntegerv(GL_FRAMEBUFFER_BINDING, &previousFramebuffer);
        glGetIntegerv(GL_RENDERBUFFER_BINDING, &previousRenderbuffer);
        device->context.functions.BindFramebuffer(GL_FRAMEBUFFER, 0);
        device->context.functions.BindRenderbuffer(GL_RENDERBUFFER, 0);
    }

    void EndFramebufferEdit(MGG_GraphicsDevice* device, GLint previousFramebuffer, GLint previousRenderbuffer)
    {
        assert(device != nullptr);

        device->context.functions.BindFramebuffer(GL_FRAMEBUFFER, static_cast<GLuint>(previousFramebuffer));
        device->context.functions.BindRenderbuffer(GL_RENDERBUFFER, static_cast<GLuint>(previousRenderbuffer));
    }

    MGG_Texture* CreateTextureResource(
        MGG_GraphicsDevice* device,
        MGTextureType type,
        MGSurfaceFormat format,
        mgint width,
        mgint height,
        mgint depth,
        mgint mipmaps,
        mgint slices)
    {
        assert(device != nullptr);

        // TODO: add 3D, cube, and array texture creation
        if (type != MGTextureType::_2D || depth != 1 || slices != 1)
            MGGL_FAIL("Unsupported texture shape", "need to add 3D, cube, and array texture support");

        TextureFormatInfo formatInfo = GetTextureFormatInfo(format);

        MGG_Texture* texture = new MGG_Texture();
        texture->target = GL_TEXTURE_2D;
        texture->type = type;
        texture->format = format;
        texture->internalFormat = formatInfo.internalFormat;
        texture->pixelFormat = formatInfo.pixelFormat;
        texture->pixelType = formatInfo.pixelType;
        texture->width = width;
        texture->height = height;
        texture->depth = depth;
        texture->mipmaps = mipmaps;
        texture->slices = slices;
        texture->bytesPerPixel = formatInfo.bytesPerPixel;
        texture->shadowData.resize(static_cast<size_t>(mipmaps * slices));

        glGenTextures(1, &texture->handle);
        if (texture->handle == 0)
            MGGL_FAIL("glGenTextures failed", "texture creation returned 0");

        GLint previousActiveTexture = 0;
        GLint previousBinding = 0;
        glPixelStorei(GL_UNPACK_ALIGNMENT, 1);
        BeginTextureEdit(device, texture, previousActiveTexture, previousBinding);
        glTexParameteri(texture->target, GL_TEXTURE_BASE_LEVEL, 0);
        glTexParameteri(texture->target, GL_TEXTURE_MAX_LEVEL, mipmaps - 1);
        glTexParameteri(texture->target, GL_TEXTURE_MIN_FILTER, mipmaps > 1 ? GL_LINEAR_MIPMAP_LINEAR : GL_LINEAR);
        glTexParameteri(texture->target, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
        glTexParameteri(texture->target, GL_TEXTURE_WRAP_S, GL_REPEAT);
        glTexParameteri(texture->target, GL_TEXTURE_WRAP_T, GL_REPEAT);

        if (formatInfo.usesSwizzle)
        {
            glTexParameteri(texture->target, GL_TEXTURE_SWIZZLE_R, formatInfo.swizzleR);
            glTexParameteri(texture->target, GL_TEXTURE_SWIZZLE_G, formatInfo.swizzleG);
            glTexParameteri(texture->target, GL_TEXTURE_SWIZZLE_B, formatInfo.swizzleB);
            glTexParameteri(texture->target, GL_TEXTURE_SWIZZLE_A, formatInfo.swizzleA);
        }

        for (mgint level = 0; level < mipmaps; ++level)
        {
            mgint mipWidth = GetMipExtent(width, level);
            mgint mipHeight = GetMipExtent(height, level);
            std::vector<mgbyte>& shadow = texture->shadowData[GetTextureSubresourceIndex(texture, level, 0)];
            shadow.resize(GetTextureByteCount(mipWidth, mipHeight, 1, texture->bytesPerPixel));

            glTexImage2D(
                texture->target,
                level,
                texture->internalFormat,
                mipWidth,
                mipHeight,
                0,
                texture->pixelFormat,
                texture->pixelType,
                shadow.data());
        }

        EndTextureEdit(device, texture, previousActiveTexture, previousBinding);
        return texture;
    }

    void CopyTextureRegionToShadow(
        MGG_Texture* texture,
        mgint level,
        mgint slice,
        mgint x,
        mgint y,
        mgint z,
        mgint width,
        mgint height,
        mgint depth,
        const mgbyte* data)
    {
        assert(texture != nullptr);
        assert(data != nullptr);
        assert(z == 0);
        assert(depth == 1);

        std::vector<mgbyte>& shadow = texture->shadowData[GetTextureSubresourceIndex(texture, level, slice)];
        mgint mipWidth = GetMipExtent(texture->width, level);
        mgint bytesPerPixel = texture->bytesPerPixel;
        mgint sourceRowBytes = width * bytesPerPixel;
        mgint destinationRowBytes = mipWidth * bytesPerPixel;
        mgbyte* destination = shadow.data() + ((y * mipWidth) + x) * bytesPerPixel;

        for (mgint row = 0; row < height; ++row)
        {
            memcpy(destination + row * destinationRowBytes, data + row * sourceRowBytes, sourceRowBytes);
        }
    }

    void CopyTextureRegionFromShadow(
        const MGG_Texture* texture,
        mgint level,
        mgint slice,
        mgint x,
        mgint y,
        mgint z,
        mgint width,
        mgint height,
        mgint depth,
        mgbyte* data)
    {
        assert(texture != nullptr);
        assert(data != nullptr);
        assert(z == 0);
        assert(depth == 1);

        const std::vector<mgbyte>& shadow = texture->shadowData[GetTextureSubresourceIndex(texture, level, slice)];
        mgint mipWidth = GetMipExtent(texture->width, level);
        mgint bytesPerPixel = texture->bytesPerPixel;
        mgint sourceRowBytes = mipWidth * bytesPerPixel;
        mgint destinationRowBytes = width * bytesPerPixel;
        const mgbyte* source = shadow.data() + ((y * mipWidth) + x) * bytesPerPixel;

        for (mgint row = 0; row < height; ++row)
        {
            memcpy(data + row * destinationRowBytes, source + row * sourceRowBytes, destinationRowBytes);
        }
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

void MGG_EffectResource_GetBytecode(const char* name, mgbyte*& bytecode, mgint& size)
{
    assert(name != nullptr);
    (void)bytecode;
    (void)size;
    MGGL_NOT_IMPLEMENTED("MGG_EffectResource_GetBytecode");
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

void MGG_GraphicsDevice_ResizeSwapchain(
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
    device->currentRenderTarget = nullptr;
    device->currentFramebuffer = 0;
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
    assert(device != nullptr);
    assert(count >= 0);

    EnsureContext(device);

    if (count == 0)
    {
        device->context.functions.BindFramebuffer(GL_FRAMEBUFFER, 0);
        glDrawBuffer(GL_BACK);
        glReadBuffer(GL_BACK);
        device->currentRenderTarget = nullptr;
        device->currentFramebuffer = 0;
        return;
    }

    // TODO: add multiple render target binding
    if (count != 1)
        MGGL_FAIL("Unsupported render target count", "need to add multiple render target binding");

    if (targets == nullptr || arraySlices == nullptr || targets[0] == nullptr)
        MGGL_FAIL("Invalid render target binding", "one valid render target is required");

    // TODO: add render target array and cube slice binding
    if (arraySlices[0] != 0)
        MGGL_FAIL("Unsupported render target slice", "need to add non-zero render target slice binding");

    MGG_Texture* target = targets[0];
    if (!target->isRenderTarget || target->framebuffer == 0)
        MGGL_FAIL("Invalid render target binding", "texture does not own a framebuffer");

    device->context.functions.BindFramebuffer(GL_FRAMEBUFFER, target->framebuffer);
    glDrawBuffer(GL_COLOR_ATTACHMENT0);
    glReadBuffer(GL_COLOR_ATTACHMENT0);
    device->currentRenderTarget = target;
    device->currentFramebuffer = target->framebuffer;
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
    assert(device != nullptr);
    assert(slot >= 0);
    assert(slot < GetTextureSlotLimit(stage));

    EnsureContext(device);

    size_t stageIndex = ToStageIndex(stage);
    GLuint textureUnit = GetTextureUnit(stage, slot);
    GLenum previousTarget = device->textureTargets[stageIndex][slot];

    device->context.functions.ActiveTexture(GL_TEXTURE0 + textureUnit);

    if (previousTarget != 0 && (texture == nullptr || previousTarget != texture->target))
        glBindTexture(previousTarget, 0);

    if (texture != nullptr)
        glBindTexture(texture->target, texture->handle);

    device->textures[stageIndex][slot] = texture;
    device->textureTargets[stageIndex][slot] = texture != nullptr ? texture->target : 0;
}

void MGG_GraphicsDevice_SetSamplerState(MGG_GraphicsDevice* device, MGShaderStage stage, mgint slot, MGG_SamplerState* state)
{
    assert(device != nullptr);
    assert(slot >= 0);
    assert(slot < GetTextureSlotLimit(stage));

    EnsureContext(device);

    size_t stageIndex = ToStageIndex(stage);
    GLuint textureUnit = GetTextureUnit(stage, slot);

    device->context.functions.ActiveTexture(GL_TEXTURE0 + textureUnit);
    device->context.functions.BindSampler(textureUnit, state != nullptr ? state->handle : 0);
    device->samplers[stageIndex][slot] = state;
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
    mgint indexSizeInBytes = GetIndexElementSizeInBytes(device->indexElementSize);
    intptr_t indexByteOffset = static_cast<intptr_t>(indexStart) * indexSizeInBytes;

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
    assert(device != nullptr);

    EnsureContext(device);

    if (device->currentRenderTarget == nullptr)
        return;
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

    EnsureContext(device);

    MGG_SamplerState* state = new MGG_SamplerState();
    state->info = *info;

    device->context.functions.GenSamplers(1, &state->handle);
    if (state->handle == 0)
        MGGL_FAIL("glGenSamplers failed", "sampler creation returned 0");

    GLenum minFilter = GL_LINEAR_MIPMAP_LINEAR;
    GLenum magFilter = GL_LINEAR;
    ToTextureFilters(info->Filter, minFilter, magFilter);

    device->context.functions.SamplerParameteri(state->handle, GL_TEXTURE_WRAP_S, ToTextureAddressMode(info->AddressU));
    device->context.functions.SamplerParameteri(state->handle, GL_TEXTURE_WRAP_T, ToTextureAddressMode(info->AddressV));
    device->context.functions.SamplerParameteri(state->handle, GL_TEXTURE_WRAP_R, ToTextureAddressMode(info->AddressW));
    device->context.functions.SamplerParameteri(state->handle, GL_TEXTURE_MIN_FILTER, minFilter);
    device->context.functions.SamplerParameteri(state->handle, GL_TEXTURE_MAG_FILTER, magFilter);

    GLfloat borderColor[4];
    borderColor[0] = static_cast<GLfloat>(info->BorderColor & 0xFF) / 255.0f;
    borderColor[1] = static_cast<GLfloat>((info->BorderColor >> 8) & 0xFF) / 255.0f;
    borderColor[2] = static_cast<GLfloat>((info->BorderColor >> 16) & 0xFF) / 255.0f;
    borderColor[3] = static_cast<GLfloat>((info->BorderColor >> 24) & 0xFF) / 255.0f;

    device->context.functions.SamplerParameterfv(state->handle, GL_TEXTURE_BORDER_COLOR, borderColor);
    device->context.functions.SamplerParameterf(state->handle, GL_TEXTURE_LOD_BIAS, info->MipMapLevelOfDetailBias);
    device->context.functions.SamplerParameterf(
        state->handle,
        GL_TEXTURE_MAX_LOD,
        info->MaxMipLevel > 0 ? static_cast<GLfloat>(info->MaxMipLevel) : 1000.0f);

    if (info->FilterMode == MGTextureFilterMode::Comparison)
    {
        device->context.functions.SamplerParameteri(state->handle, GL_TEXTURE_COMPARE_MODE, GL_COMPARE_REF_TO_TEXTURE);
        device->context.functions.SamplerParameteri(state->handle, GL_TEXTURE_COMPARE_FUNC, ToCompareFunction(info->ComparisonFunction));
    }
    else
    {
        device->context.functions.SamplerParameteri(state->handle, GL_TEXTURE_COMPARE_MODE, GL_NONE);
    }

    return state;
}

void MGG_SamplerState_Destroy(MGG_GraphicsDevice* device, MGG_SamplerState* state)
{
    assert(device != nullptr);

    if (state == nullptr)
        return;

    EnsureContext(device);

    GLint previousActiveTexture = 0;
    glGetIntegerv(GL_ACTIVE_TEXTURE, &previousActiveTexture);

    for (size_t stageIndex = 0; stageIndex < ShaderStageCount; ++stageIndex)
    {
        MGShaderStage stage = stageIndex == 0 ? MGShaderStage::Vertex : MGShaderStage::Pixel;
        mgint slotLimit = GetTextureSlotLimit(stage);

        for (mgint slot = 0; slot < slotLimit; ++slot)
        {
            if (device->samplers[stageIndex][slot] != state)
                continue;

            GLuint textureUnit = GetTextureUnit(stage, slot);
            device->context.functions.ActiveTexture(GL_TEXTURE0 + textureUnit);
            device->context.functions.BindSampler(textureUnit, 0);
            device->samplers[stageIndex][slot] = nullptr;
        }
    }

    device->context.functions.ActiveTexture(static_cast<GLenum>(previousActiveTexture));

    if (state->handle != 0)
        device->context.functions.DeleteSamplers(1, &state->handle);
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
    buffer->sizeInBytes = sizeInBytes;
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
    assert(offset + copySpan <= buffer->sizeInBytes);

    CopyToShadowData(buffer->shadowData.data() + offset, data, elementCount, vertexStride, elementSizeInBytes);

    EnsureContext(device);
    device->context.functions.BindBuffer(buffer->target, buffer->handle);

    if (discard)
    {
        device->context.functions.BufferData(
            buffer->target,
            buffer->sizeInBytes,
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
    assert(offset + copySpan <= buffer->sizeInBytes);

    CopyFromShadowData(buffer->shadowData.data() + offset, data, dataCount, dataBytes, dataStride);
}

MGG_Texture* MGG_Texture_Create(MGG_GraphicsDevice* device, MGTextureType type, MGSurfaceFormat format, mgint width, mgint height, mgint depth, mgint mipmaps, mgint slices)
{
    assert(device != nullptr);
    assert(width > 0);
    assert(height > 0);
    assert(depth > 0);
    assert(mipmaps > 0);
    assert(slices > 0);

    EnsureContext(device);
    return CreateTextureResource(device, type, format, width, height, depth, mipmaps, slices);
}

MGG_Texture* MGG_RenderTarget_Create(MGG_GraphicsDevice* device, MGTextureType type, MGSurfaceFormat format, mgint width, mgint height, mgint depth, mgint mipmaps, mgint slices, MGDepthFormat depthFormat, mgint multiSampleCount, MGRenderTargetUsage usage)
{
    assert(device != nullptr);
    assert(width > 0);
    assert(height > 0);
    assert(depth > 0);
    assert(mipmaps > 0);
    assert(slices > 0);
    assert(multiSampleCount >= 0);

    EnsureContext(device);

    // TODO: add multisampled render target allocation and resolve
    if (multiSampleCount > 0)
        MGGL_FAIL("Unsupported render target multisampling", "need to add multisampled render target support");

    // TODO: add render target mip allocation
    if (mipmaps != 1)
        MGGL_FAIL("Unsupported render target mipmaps", "need to add render target mip levels");

    MGG_Texture* texture = CreateTextureResource(device, type, format, width, height, depth, mipmaps, slices);
    texture->isRenderTarget = true;
    texture->depthFormat = depthFormat;
    texture->multiSampleCount = multiSampleCount;
    texture->renderTargetUsage = usage;

    GLint previousFramebuffer = 0;
    GLint previousRenderbuffer = 0;
    BeginFramebufferEdit(device, previousFramebuffer, previousRenderbuffer);

    device->context.functions.GenFramebuffers(1, &texture->framebuffer);
    if (texture->framebuffer == 0)
        MGGL_FAIL("glGenFramebuffers failed", "framebuffer creation returned 0");

    device->context.functions.BindFramebuffer(GL_FRAMEBUFFER, texture->framebuffer);
    device->context.functions.FramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, texture->target, texture->handle, 0);
    glDrawBuffer(GL_COLOR_ATTACHMENT0);
    glReadBuffer(GL_COLOR_ATTACHMENT0);

    if (depthFormat != MGDepthFormat::None)
    {
        device->context.functions.GenRenderbuffers(1, &texture->depthRenderbuffer);
        if (texture->depthRenderbuffer == 0)
            MGGL_FAIL("glGenRenderbuffers failed", "depth renderbuffer creation returned 0");

        device->context.functions.BindRenderbuffer(GL_RENDERBUFFER, texture->depthRenderbuffer);
        device->context.functions.RenderbufferStorage(
            GL_RENDERBUFFER,
            ToDepthRenderbufferFormat(depthFormat),
            width,
            height);
        device->context.functions.FramebufferRenderbuffer(
            GL_FRAMEBUFFER,
            ToDepthAttachment(depthFormat),
            GL_RENDERBUFFER,
            texture->depthRenderbuffer);
    }

    GLenum framebufferStatus = device->context.functions.CheckFramebufferStatus(GL_FRAMEBUFFER);
    if (framebufferStatus != GL_FRAMEBUFFER_COMPLETE)
        MGGL_FAIL("OpenGL framebuffer incomplete", "framebuffer status check failed");

    EndFramebufferEdit(device, previousFramebuffer, previousRenderbuffer);
    return texture;
}

void MGG_Texture_Destroy(MGG_GraphicsDevice* device, MGG_Texture* texture)
{
    assert(device != nullptr);

    if (texture == nullptr)
        return;

    EnsureContext(device);

    GLint previousActiveTexture = 0;
    glGetIntegerv(GL_ACTIVE_TEXTURE, &previousActiveTexture);

    if (device->currentRenderTarget == texture)
    {
        device->context.functions.BindFramebuffer(GL_FRAMEBUFFER, 0);
        glDrawBuffer(GL_BACK);
        glReadBuffer(GL_BACK);
        device->currentRenderTarget = nullptr;
        device->currentFramebuffer = 0;
    }

    for (size_t stageIndex = 0; stageIndex < ShaderStageCount; ++stageIndex)
    {
        MGShaderStage stage = stageIndex == 0 ? MGShaderStage::Vertex : MGShaderStage::Pixel;
        mgint slotLimit = GetTextureSlotLimit(stage);

        for (mgint slot = 0; slot < slotLimit; ++slot)
        {
            if (device->textures[stageIndex][slot] != texture)
                continue;

            GLuint textureUnit = GetTextureUnit(stage, slot);
            device->context.functions.ActiveTexture(GL_TEXTURE0 + textureUnit);

            if (device->textureTargets[stageIndex][slot] != 0)
                glBindTexture(device->textureTargets[stageIndex][slot], 0);

            device->textures[stageIndex][slot] = nullptr;
            device->textureTargets[stageIndex][slot] = 0;
        }
    }

    device->context.functions.ActiveTexture(static_cast<GLenum>(previousActiveTexture));

    if (texture->depthRenderbuffer != 0)
        device->context.functions.DeleteRenderbuffers(1, &texture->depthRenderbuffer);

    if (texture->framebuffer != 0)
        device->context.functions.DeleteFramebuffers(1, &texture->framebuffer);

    if (texture->handle != 0)
        glDeleteTextures(1, &texture->handle);

    delete texture;
}

void MGG_Texture_SetData(MGG_GraphicsDevice* device, MGG_Texture* texture, mgint level, mgint slice, mgint x, mgint y, mgint z, mgint width, mgint height, mgint depth, mgbyte* data, mgint dataBytes)
{
    assert(device != nullptr);
    assert(texture != nullptr);
    assert(data != nullptr);
    assert(level >= 0);
    assert(level < texture->mipmaps);
    assert(slice >= 0);
    assert(slice < texture->slices);
    assert(dataBytes > 0);

    EnsureContext(device);

    mgint resolvedWidth = 0;
    mgint resolvedHeight = 0;
    mgint resolvedDepth = 0;
    ResolveTextureRegion(texture, level, slice, x, y, z, width, height, depth, resolvedWidth, resolvedHeight, resolvedDepth);

    mgint expectedBytes = GetTextureByteCount(resolvedWidth, resolvedHeight, resolvedDepth, texture->bytesPerPixel);
    mgint normalizedBytes = NormalizeUploadByteCount(texture, resolvedWidth, resolvedHeight, resolvedDepth, dataBytes);
    if (normalizedBytes != expectedBytes)
        MGGL_FAIL("Texture upload size mismatch", "byte count does not match the upload region");

    CopyTextureRegionToShadow(texture, level, slice, x, y, z, resolvedWidth, resolvedHeight, resolvedDepth, data);

    GLint previousActiveTexture = 0;
    GLint previousBinding = 0;
    glPixelStorei(GL_UNPACK_ALIGNMENT, 1);
    BeginTextureEdit(device, texture, previousActiveTexture, previousBinding);
    glTexSubImage2D(
        texture->target,
        level,
        x,
        y,
        resolvedWidth,
        resolvedHeight,
        texture->pixelFormat,
        texture->pixelType,
        data);
    EndTextureEdit(device, texture, previousActiveTexture, previousBinding);
}

void MGG_Texture_GetData(MGG_GraphicsDevice* device, MGG_Texture* texture, mgint level, mgint slice, mgint x, mgint y, mgint z, mgint width, mgint height, mgint depth, mgbyte* data, mgint dataBytes)
{
    assert(device != nullptr);
    assert(texture != nullptr);
    assert(data != nullptr);
    assert(level >= 0);
    assert(level < texture->mipmaps);
    assert(slice >= 0);
    assert(slice < texture->slices);
    assert(dataBytes > 0);

    // TODO: add GPU readback for render target textures
    if (texture->isRenderTarget)
        MGGL_FAIL("Render target readback not implemented", "need to add render target readback");

    mgint resolvedWidth = 0;
    mgint resolvedHeight = 0;
    mgint resolvedDepth = 0;
    ResolveTextureRegion(texture, level, slice, x, y, z, width, height, depth, resolvedWidth, resolvedHeight, resolvedDepth);

    mgint expectedBytes = GetTextureByteCount(resolvedWidth, resolvedHeight, resolvedDepth, texture->bytesPerPixel);
    if (dataBytes < expectedBytes)
        MGGL_FAIL("Texture readback size mismatch", "destination buffer is smaller than the requested region");

    CopyTextureRegionFromShadow(texture, level, slice, x, y, z, resolvedWidth, resolvedHeight, resolvedDepth, data);
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
