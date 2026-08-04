#include "api_MGG.h"

#include "mg_common.h"
#include "AlphaTestEffect.ogl.mgfxo.h"
#include "BasicEffect.ogl.mgfxo.h"
#include "DualTextureEffect.ogl.mgfxo.h"
#include "EnvironmentMapEffect.ogl.mgfxo.h"
#include "SkinnedEffect.ogl.mgfxo.h"
#include "SpriteEffect.ogl.mgfxo.h"
#include "mg_effect.h"

#include "OpenGLContext.h"

#include <SDL.h>
#include <SDL_opengl.h>
#include <array>
#include <cstdint>
#include <vector>

#ifndef GL_TEXTURE_MAX_ANISOTROPY_EXT
#define GL_TEXTURE_MAX_ANISOTROPY_EXT 0x84FE
#endif

#ifndef GL_MAX_TEXTURE_MAX_ANISOTROPY_EXT
#define GL_MAX_TEXTURE_MAX_ANISOTROPY_EXT 0x84FF
#endif

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
struct MGG_Shader;
struct MGG_InputLayout;
struct MGG_ShaderProgram;

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
    std::array<MGG_Shader*, static_cast<size_t>(MGShaderStage::Count)> shaders = {};
    std::array<MGG_Texture*, 4> currentRenderTargets = {};
    std::array<mgint, 4> currentRenderTargetSlices = {};
    MGG_InputLayout* inputLayout = nullptr;
    /*
     * Need to defer this until draw because the vertex buffers
     * may get rebound after the layout is set.
     * Chris <aristurtledev>
     */
    mgbool inputLayoutDirty = false;
    MGG_ShaderProgram* currentProgram = nullptr;
    std::vector<MGG_ShaderProgram*> programs;
    std::array<mgbool, 16> enabledAttributes = {};
    MGG_Texture* currentRenderTarget = nullptr;
    mgint currentRenderTargetCount = 0;
    GLuint currentFramebuffer = 0;
    mgint viewportX = 0;
    mgint viewportY = 0;
    mgint viewportWidth = 0;
    mgint viewportHeight = 0;
    mgfloat viewportMinDepth = 0.0f;
    mgfloat viewportMaxDepth = 1.0f;
};

struct MGG_Buffer
{
    GLuint handle = 0;
    GLenum target = 0;
    GLenum usage = GL_STATIC_DRAW;
    MGBufferType type = MGBufferType::Vertex;
    mgbool dynamic = false;
    mgint sizeInBytes = 0;
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
    mgint bytesPerBlock = 0;
    mgint blockWidth = 1;
    mgint blockHeight = 1;
    mgbool isCompressed = false;
    mgbool isRenderTarget = false;
    MGDepthFormat depthFormat = MGDepthFormat::None;
    mgint multiSampleCount = 0;
    MGRenderTargetUsage renderTargetUsage = MGRenderTargetUsage::DiscardContents;
    GLuint framebuffer = 0;
    GLuint resolveFramebuffer = 0;
    GLuint colorRenderbuffer = 0;
    GLuint depthRenderbuffer = 0;
};

struct MGG_Shader
{
    MGShaderStage stage = MGShaderStage::Vertex;
    GLuint handle = 0;
    std::string source;
    std::vector<GLenum> constantBufferTypes;
    mgint attributeCount = 0;
};

struct MGG_InputLayout
{
    struct Binding
    {
        GLuint attributeIndex = 0;
        GLuint vertexBufferSlot = 0;
        GLint elementCount = 0;
        GLenum elementType = GL_FLOAT;
        GLboolean normalized = GL_FALSE;
        GLuint alignedByteOffset = 0;
        GLuint instanceDataStepRate = 0;
    };

    std::vector<Binding> bindings;
    std::vector<mgint> strides;
};

struct MGG_ShaderProgram
{
    MGG_Shader* vertexShader = nullptr;
    MGG_Shader* pixelShader = nullptr;
    GLuint handle = 0;
    GLint posFixupLocation = -1;
    std::vector<GLint> attributeLocations;
    std::array<std::vector<GLint>, static_cast<size_t>(MGShaderStage::Count)> constantBufferLocations = {};
};

struct MGG_OcclusionQuery
{
    GLuint handle = 0;
};

namespace
{
    constexpr mgint MaxRenderTargetBindings = 4;
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
        mgint bytesPerBlock;
        mgint blockWidth;
        mgint blockHeight;
        GLint swizzleR;
        GLint swizzleG;
        GLint swizzleB;
        GLint swizzleA;
        bool usesSwizzle;
        bool isCompressed;
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

    bool HasOpenGLExtension(const char* name)
    {
        assert(name != nullptr);
        return SDL_GL_ExtensionSupported(name) == SDL_TRUE;
    }

    bool SupportsOpenGLDepthClamp(MGG_GraphicsDevice* device)
    {
        assert(device != nullptr);

        if (device->window == nullptr || device->context.handle == nullptr)
            return false;

        EnsureContext(device);

        if (device->context.majorVersion > 3)
            return true;

        if (device->context.majorVersion == 3 && device->context.minorVersion >= 2)
            return true;

        return HasOpenGLExtension("GL_ARB_depth_clamp");
    }

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

    const char* GetStagePrefix(MGShaderStage stage)
    {
        switch (stage)
        {
        case MGShaderStage::Vertex:
            return "vs";
        case MGShaderStage::Pixel:
            return "ps";
        default:
            MGGL_FAIL("Unsupported shader stage", "unknown OpenGL shader prefix");
        }
    }

    std::string GetAttributeName(mgint index)
    {
        assert(index >= 0);

        char name[32];
        snprintf(name, sizeof(name), "vs_v%d", index);
        return name;
    }

    std::string GetSamplerName(MGShaderStage stage, mgint slot)
    {
        assert(slot >= 0);

        char name[32];
        snprintf(name, sizeof(name), "%s_s%d", GetStagePrefix(stage), slot);
        return name;
    }

    std::string GetConstantBufferName(MGShaderStage stage, GLenum type)
    {
        const char* typeSuffix = "vec4";
        if (type == GL_BOOL)
            typeSuffix = "bool";
        else if (type == GL_INT)
            typeSuffix = "ivec4";

        char name[64];
        snprintf(name, sizeof(name), "%s_uniforms_%s", GetStagePrefix(stage), typeSuffix);
        return name;
    }

    mgint GetConstantRegisterCount(const MGG_Buffer* buffer)
    {
        assert(buffer != nullptr);

        if ((buffer->sizeInBytes % 16) != 0)
            MGGL_FAIL("Invalid constant buffer size", "constant buffers must be aligned to 16-byte registers");

        return buffer->sizeInBytes / 16;
    }

    void ToVertexAttribFormat(MGVertexElementFormat format, GLint& elementCount, GLenum& elementType, GLboolean& normalized)
    {
        switch (format)
        {
            case MGVertexElementFormat::Single:
                elementCount = 1;
                elementType = GL_FLOAT;
                normalized = GL_FALSE;
                return;
            case MGVertexElementFormat::Vector2:
                elementCount = 2;
                elementType = GL_FLOAT;
                normalized = GL_FALSE;
                return;
            case MGVertexElementFormat::Vector3:
                elementCount = 3;
                elementType = GL_FLOAT;
                normalized = GL_FALSE;
                return;
            case MGVertexElementFormat::Vector4:
                elementCount = 4;
                elementType = GL_FLOAT;
                normalized = GL_FALSE;
                return;
            case MGVertexElementFormat::Color:
                elementCount = 4;
                elementType = GL_UNSIGNED_BYTE;
                normalized = GL_TRUE;
                return;
            case MGVertexElementFormat::Byte4:
                elementCount = 4;
                elementType = GL_UNSIGNED_BYTE;
                normalized = GL_FALSE;
                return;
            case MGVertexElementFormat::Short2:
                elementCount = 2;
                elementType = GL_SHORT;
                normalized = GL_FALSE;
                return;
            case MGVertexElementFormat::Short4:
                elementCount = 4;
                elementType = GL_SHORT;
                normalized = GL_FALSE;
                return;
            case MGVertexElementFormat::NormalizedShort2:
                elementCount = 2;
                elementType = GL_SHORT;
                normalized = GL_TRUE;
                return;
            case MGVertexElementFormat::NormalizedShort4:
                elementCount = 4;
                elementType = GL_SHORT;
                normalized = GL_TRUE;
                return;
            case MGVertexElementFormat::HalfVector2:
                elementCount = 2;
                elementType = GL_HALF_FLOAT;
                normalized = GL_FALSE;
                return;
            case MGVertexElementFormat::HalfVector4:
                elementCount = 4;
                elementType = GL_HALF_FLOAT;
                normalized = GL_FALSE;
                return;
            default:
                MGGL_FAIL("Unsupported vertex element format", "native OpenGL input layout does not recognize this vertex element format");
        }
    }

    mgint CountSequentialShaderInputs(const std::string& source)
    {
        mgint attributeCount = 0;
        while (source.find(GetAttributeName(attributeCount)) != std::string::npos)
            ++attributeCount;
        return attributeCount;
    }

    void ApplyPosFixup(MGG_GraphicsDevice* device)
    {
        assert(device != nullptr);

        if (device->currentProgram == nullptr || device->currentProgram->posFixupLocation < 0)
            return;

        GLfloat posFixup[4] = { 1.0f, 1.0f, 0.0f, 0.0f };
        if (device->currentRenderTargetCount > 0)
        {
            /*
             * Need to flip render target Y here so it stays opposite
             * of the default framebuffer path.
             * Chris <aristurtledev>
             */
            posFixup[1] = -1.0f;
        }

        device->context.functions.Uniform4fv(device->currentProgram->posFixupLocation, 1, posFixup);
    }

    void ApplyInputLayout(MGG_GraphicsDevice* device, mgint drawVertexOffset = 0)
    {
        assert(device != nullptr);
        assert(drawVertexOffset >= 0);

        EnsureContext(device);
        EnsureVertexArray(device);

        for (GLuint location = 0; location < device->enabledAttributes.size(); ++location)
        {
            if (!device->enabledAttributes[location])
                continue;

            device->context.functions.DisableVertexAttribArray(location);
            device->enabledAttributes[location] = false;
        }

        if (device->inputLayout == nullptr)
            return;

        if (device->currentProgram == nullptr)
            MGGL_FAIL("Missing shader program", "input layout application requires a linked shader program");

        for (const MGG_InputLayout::Binding& binding : device->inputLayout->bindings)
        {
            if (binding.vertexBufferSlot >= device->vertexBuffers.size())
                MGGL_FAIL("Invalid vertex buffer slot", "input layout references a vertex buffer slot beyond the supported range");

            if (binding.vertexBufferSlot >= device->inputLayout->strides.size())
                MGGL_FAIL("Invalid vertex stride table", "input layout references a vertex stream that does not have a recorded stride");

            MGG_Buffer* buffer = device->vertexBuffers[binding.vertexBufferSlot];
            if (buffer == nullptr)
                MGGL_FAIL("Missing vertex buffer binding", "input layout application requires every referenced vertex buffer slot to be bound");

            mgint stride = device->inputLayout->strides[binding.vertexBufferSlot];
            intptr_t baseOffset = static_cast<intptr_t>(device->vertexOffsets[binding.vertexBufferSlot] + drawVertexOffset) * stride;
            intptr_t byteOffset = baseOffset + binding.alignedByteOffset;
            if (binding.attributeIndex >= device->currentProgram->attributeLocations.size())
                MGGL_FAIL("Invalid shader attribute index", "input layout references a shader attribute index that is not active for the linked program");

            GLint location = device->currentProgram->attributeLocations[binding.attributeIndex];
            if (location < 0)
                continue;

            if (static_cast<size_t>(location) >= device->enabledAttributes.size())
                MGGL_FAIL("Unsupported shader attribute location", "linked OpenGL program reported an attribute location beyond the native OpenGL tracking limit");

            device->context.functions.BindBuffer(GL_ARRAY_BUFFER, buffer->handle);
            device->context.functions.EnableVertexAttribArray(static_cast<GLuint>(location));
            device->context.functions.VertexAttribPointer(
                static_cast<GLuint>(location),
                binding.elementCount,
                binding.elementType,
                binding.normalized,
                stride,
                reinterpret_cast<const void*>(byteOffset));
            device->context.functions.VertexAttribDivisor(static_cast<GLuint>(location), binding.instanceDataStepRate);
            device->enabledAttributes[location] = true;
        }

        device->inputLayoutDirty = false;
    }

    void DestroyProgram(MGG_GraphicsDevice* device, MGG_ShaderProgram* program)
    {
        assert(device != nullptr);

        if (program == nullptr)
            return;

        if (device->currentProgram == program)
        {
            device->context.functions.UseProgram(0);
            device->currentProgram = nullptr;
        }

        if (program->handle != 0)
        {
            if (program->vertexShader != nullptr && program->vertexShader->handle != 0)
                device->context.functions.DetachShader(program->handle, program->vertexShader->handle);

            if (program->pixelShader != nullptr && program->pixelShader->handle != 0)
                device->context.functions.DetachShader(program->handle, program->pixelShader->handle);

            device->context.functions.DeleteProgram(program->handle);
        }

        delete program;
    }

    MGG_ShaderProgram* GetOrCreateProgram(MGG_GraphicsDevice* device)
    {
        assert(device != nullptr);

        MGG_Shader* vertexShader = device->shaders[ToStageIndex(MGShaderStage::Vertex)];
        MGG_Shader* pixelShader = device->shaders[ToStageIndex(MGShaderStage::Pixel)];
        if (vertexShader == nullptr || pixelShader == nullptr)
            MGGL_FAIL("Shader program binding incomplete", "vertex and pixel shaders must both be set before linking");

        for (MGG_ShaderProgram* program : device->programs)
        {
            if (program->vertexShader == vertexShader && program->pixelShader == pixelShader)
                return program;
        }

        MGG_ShaderProgram* program = new MGG_ShaderProgram();
        program->vertexShader = vertexShader;
        program->pixelShader = pixelShader;
        program->handle = device->context.functions.CreateProgram();
        if (program->handle == 0)
            MGGL_FAIL("glCreateProgram failed", "shader program creation returned 0");

        device->context.functions.AttachShader(program->handle, vertexShader->handle);
        device->context.functions.AttachShader(program->handle, pixelShader->handle);

        device->context.functions.LinkProgram(program->handle);

        GLint linked = GL_FALSE;
        device->context.functions.GetProgramiv(program->handle, GL_LINK_STATUS, &linked);
        if (linked != GL_TRUE)
        {
            char log[2048] = {};
            GLsizei length = 0;
            device->context.functions.GetProgramInfoLog(program->handle, sizeof(log), &length, log);
            MGGL_FAIL("glLinkProgram failed", length > 0 ? log : "program link failed without an info log");
        }

        device->context.functions.UseProgram(program->handle);
        program->posFixupLocation = device->context.functions.GetUniformLocation(program->handle, "posFixup");
        program->attributeLocations.reserve(vertexShader->attributeCount);
        for (mgint index = 0; index < vertexShader->attributeCount; ++index)
        {
            std::string attributeName = GetAttributeName(index);
            program->attributeLocations.push_back(device->context.functions.GetAttribLocation(program->handle, attributeName.c_str()));
        }

        for (mgint slot = 0; slot < GetTextureSlotLimit(MGShaderStage::Vertex); ++slot)
        {
            std::string samplerName = GetSamplerName(MGShaderStage::Vertex, slot);
            GLint location = device->context.functions.GetUniformLocation(program->handle, samplerName.c_str());
            if (location >= 0)
                device->context.functions.Uniform1i(location, static_cast<GLint>(GetTextureUnit(MGShaderStage::Vertex, slot)));
        }

        for (mgint slot = 0; slot < GetTextureSlotLimit(MGShaderStage::Pixel); ++slot)
        {
            std::string samplerName = GetSamplerName(MGShaderStage::Pixel, slot);
            GLint location = device->context.functions.GetUniformLocation(program->handle, samplerName.c_str());
            if (location >= 0)
                device->context.functions.Uniform1i(location, static_cast<GLint>(GetTextureUnit(MGShaderStage::Pixel, slot)));
        }

        for (size_t stageIndex = 0; stageIndex < ShaderStageCount; ++stageIndex)
        {
            MGG_Shader* shader = stageIndex == 0 ? vertexShader : pixelShader;
            MGShaderStage stage = stageIndex == 0 ? MGShaderStage::Vertex : MGShaderStage::Pixel;
            std::vector<GLint>& locations = program->constantBufferLocations[stageIndex];
            locations.reserve(shader->constantBufferTypes.size());

            for (GLenum type : shader->constantBufferTypes)
            {
                std::string uniformName = GetConstantBufferName(stage, type);
                locations.push_back(device->context.functions.GetUniformLocation(program->handle, uniformName.c_str()));
            }
        }

        device->programs.push_back(program);
        return program;
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

    GLenum ToBlendEquation(MGBlendFunction function)
    {
        switch (function)
        {
        case MGBlendFunction::Add:
            return GL_FUNC_ADD;
        case MGBlendFunction::Subtract:
            return GL_FUNC_SUBTRACT;
        case MGBlendFunction::ReverseSubtract:
            return GL_FUNC_REVERSE_SUBTRACT;
        case MGBlendFunction::Min:
            return GL_MIN;
        case MGBlendFunction::Max:
            return GL_MAX;
        default:
            MGGL_FAIL("Unsupported blend function", "unknown OpenGL blend equation");
        }
    }

    GLenum ToBlendFactor(MGBlend blend)
    {
        switch (blend)
        {
        case MGBlend::One:
            return GL_ONE;
        case MGBlend::Zero:
            return GL_ZERO;
        case MGBlend::SourceColor:
            return GL_SRC_COLOR;
        case MGBlend::InverseSourceColor:
            return GL_ONE_MINUS_SRC_COLOR;
        case MGBlend::SourceAlpha:
            return GL_SRC_ALPHA;
        case MGBlend::InverseSourceAlpha:
            return GL_ONE_MINUS_SRC_ALPHA;
        case MGBlend::DestinationColor:
            return GL_DST_COLOR;
        case MGBlend::InverseDestinationColor:
            return GL_ONE_MINUS_DST_COLOR;
        case MGBlend::DestinationAlpha:
            return GL_DST_ALPHA;
        case MGBlend::InverseDestinationAlpha:
            return GL_ONE_MINUS_DST_ALPHA;
        case MGBlend::BlendFactor:
            return GL_CONSTANT_COLOR;
        case MGBlend::InverseBlendFactor:
            return GL_ONE_MINUS_CONSTANT_COLOR;
        case MGBlend::SourceAlphaSaturation:
            return GL_SRC_ALPHA_SATURATE;
        default:
            MGGL_FAIL("Unsupported blend factor", "unknown OpenGL blend factor");
        }
    }

    GLenum ToStencilOperation(MGStencilOperation operation)
    {
        switch (operation)
        {
        case MGStencilOperation::Keep:
            return GL_KEEP;
        case MGStencilOperation::Zero:
            return GL_ZERO;
        case MGStencilOperation::Replace:
            return GL_REPLACE;
        case MGStencilOperation::Increment:
            return GL_INCR_WRAP;
        case MGStencilOperation::Decrement:
            return GL_DECR_WRAP;
        case MGStencilOperation::IncrementSaturation:
            return GL_INCR;
        case MGStencilOperation::DecrementSaturation:
            return GL_DECR;
        case MGStencilOperation::Invert:
            return GL_INVERT;
        default:
            MGGL_FAIL("Unsupported stencil operation", "unknown OpenGL stencil operation");
        }
    }

    GLenum ToPolygonMode(MGFillMode fillMode)
    {
        switch (fillMode)
        {
        case MGFillMode::Solid:
            return GL_FILL;
        case MGFillMode::WireFrame:
            return GL_LINE;
        default:
            MGGL_FAIL("Unsupported fill mode", "unknown OpenGL polygon mode");
        }
    }

    void ToColorMask(MGColorWriteChannels channels, GLboolean& red, GLboolean& green, GLboolean& blue, GLboolean& alpha)
    {
        mgint mask = static_cast<mgint>(channels);
        red = (mask & static_cast<mgint>(MGColorWriteChannels::Red)) != 0 ? GL_TRUE : GL_FALSE;
        green = (mask & static_cast<mgint>(MGColorWriteChannels::Green)) != 0 ? GL_TRUE : GL_FALSE;
        blue = (mask & static_cast<mgint>(MGColorWriteChannels::Blue)) != 0 ? GL_TRUE : GL_FALSE;
        alpha = (mask & static_cast<mgint>(MGColorWriteChannels::Alpha)) != 0 ? GL_TRUE : GL_FALSE;
    }

    MGDepthFormat GetActiveDepthFormat(const MGG_GraphicsDevice* device)
    {
        assert(device != nullptr);
        return device->currentRenderTargetCount > 0 ? device->currentRenderTargets[0]->depthFormat : device->depthFormat;
    }

    mgint ToOpenGLWindowY(const MGG_GraphicsDevice* device, mgint y, mgint height)
    {
        assert(device != nullptr);

        /*
         * Need to flip the default framebuffer Y here because OpenGL
         * uses a bottom-left origin. Render targets already get their
         * Y fixup through ApplyPosFixup.
         * Chris <aristurtledev>
         */
        if (device->currentRenderTargetCount > 0)
            return y;

        return device->backBufferHeight - y - height;
    }

    bool IsRenderTargetBound(const MGG_GraphicsDevice* device, const MGG_Texture* texture)
    {
        assert(device != nullptr);
        assert(texture != nullptr);

        for (mgint i = 0; i < device->currentRenderTargetCount; ++i)
        {
            if (device->currentRenderTargets[i] == texture)
                return true;
        }

        return false;
    }

    void ClearCurrentRenderTargets(MGG_GraphicsDevice* device)
    {
        assert(device != nullptr);

        device->currentRenderTargets.fill(nullptr);
        device->currentRenderTargetSlices.fill(0);
        device->currentRenderTarget = nullptr;
        device->currentRenderTargetCount = 0;
        device->currentFramebuffer = 0;
    }

    bool UsesMultisampledRenderTarget(const MGG_Texture* texture)
    {
        assert(texture != nullptr);
        return texture->isRenderTarget && texture->multiSampleCount > 0;
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
            return { GL_RGBA8, GL_RGBA, GL_UNSIGNED_BYTE, 4, 4, 1, 1, GL_RED, GL_GREEN, GL_BLUE, GL_ALPHA, false, false };
        case MGSurfaceFormat::ColorSRgb:
            return { GL_SRGB8_ALPHA8, GL_RGBA, GL_UNSIGNED_BYTE, 4, 4, 1, 1, GL_RED, GL_GREEN, GL_BLUE, GL_ALPHA, false, false };
        case MGSurfaceFormat::Bgra32:
            return { GL_RGBA8, GL_BGRA, GL_UNSIGNED_BYTE, 4, 4, 1, 1, GL_RED, GL_GREEN, GL_BLUE, GL_ALPHA, false, false };
        case MGSurfaceFormat::Bgra32SRgb:
            return { GL_SRGB8_ALPHA8, GL_BGRA, GL_UNSIGNED_BYTE, 4, 4, 1, 1, GL_RED, GL_GREEN, GL_BLUE, GL_ALPHA, false, false };
        case MGSurfaceFormat::Alpha8:
            return { GL_R8, GL_RED, GL_UNSIGNED_BYTE, 1, 1, 1, 1, GL_ONE, GL_ONE, GL_ONE, GL_RED, true, false };
        case MGSurfaceFormat::Dxt1:
            return { GL_COMPRESSED_RGB_S3TC_DXT1_EXT, 0, 0, 0, 8, 4, 4, GL_RED, GL_GREEN, GL_BLUE, GL_ALPHA, false, true };
        case MGSurfaceFormat::Dxt1SRgb:
            return { GL_COMPRESSED_SRGB_S3TC_DXT1_EXT, 0, 0, 0, 8, 4, 4, GL_RED, GL_GREEN, GL_BLUE, GL_ALPHA, false, true };
        case MGSurfaceFormat::Dxt1a:
            return { GL_COMPRESSED_RGBA_S3TC_DXT1_EXT, 0, 0, 0, 8, 4, 4, GL_RED, GL_GREEN, GL_BLUE, GL_ALPHA, false, true };
        case MGSurfaceFormat::Dxt3:
            return { GL_COMPRESSED_RGBA_S3TC_DXT3_EXT, 0, 0, 0, 16, 4, 4, GL_RED, GL_GREEN, GL_BLUE, GL_ALPHA, false, true };
        case MGSurfaceFormat::Dxt3SRgb:
            return { GL_COMPRESSED_SRGB_ALPHA_S3TC_DXT3_EXT, 0, 0, 0, 16, 4, 4, GL_RED, GL_GREEN, GL_BLUE, GL_ALPHA, false, true };
        case MGSurfaceFormat::Dxt5:
            return { GL_COMPRESSED_RGBA_S3TC_DXT5_EXT, 0, 0, 0, 16, 4, 4, GL_RED, GL_GREEN, GL_BLUE, GL_ALPHA, false, true };
        case MGSurfaceFormat::Dxt5SRgb:
            return { GL_COMPRESSED_SRGB_ALPHA_S3TC_DXT5_EXT, 0, 0, 0, 16, 4, 4, GL_RED, GL_GREEN, GL_BLUE, GL_ALPHA, false, true };
        case MGSurfaceFormat::Single:
            return { GL_R32F, GL_RED, GL_FLOAT, 4, 4, 1, 1, GL_RED, GL_GREEN, GL_BLUE, GL_ALPHA, false, false };
        case MGSurfaceFormat::HalfSingle:
            return { GL_R16F, GL_RED, GL_HALF_FLOAT, 2, 2, 1, 1, GL_RED, GL_GREEN, GL_BLUE, GL_ALPHA, false, false };
        default:
            MGGL_FAIL("Unsupported surface format", "OpenGL surface format is not mapped");
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

    mgint GetTextureByteCount(mgint width, mgint height, mgint depth, mgint bytesPerPixel)
    {
        assert(width > 0);
        assert(height > 0);
        assert(depth > 0);
        assert(bytesPerPixel > 0);
        return width * height * depth * bytesPerPixel;
    }

    mgint GetTextureBlockCount(mgint extent, mgint blockExtent)
    {
        assert(extent > 0);
        assert(blockExtent > 0);
        return (extent + blockExtent - 1) / blockExtent;
    }

    mgint GetTextureByteCount(const MGG_Texture* texture, mgint width, mgint height, mgint depth)
    {
        assert(texture != nullptr);
        assert(width > 0);
        assert(height > 0);
        assert(depth > 0);

        if (!texture->isCompressed)
            return GetTextureByteCount(width, height, depth, texture->bytesPerPixel);

        return GetTextureBlockCount(width, texture->blockWidth)
            * GetTextureBlockCount(height, texture->blockHeight)
            * depth
            * texture->bytesPerBlock;
    }

    mgint GetTextureRowBytes(const MGG_Texture* texture, mgint width)
    {
        assert(texture != nullptr);
        assert(width > 0);

        if (!texture->isCompressed)
            return width * texture->bytesPerPixel;

        return GetTextureBlockCount(width, texture->blockWidth) * texture->bytesPerBlock;
    }

    mgint GetTextureSliceByteCount(const MGG_Texture* texture, mgint width, mgint height)
    {
        assert(texture != nullptr);
        assert(width > 0);
        assert(height > 0);

        if (!texture->isCompressed)
            return width * height * texture->bytesPerPixel;

        return GetTextureRowBytes(texture, width) * GetTextureBlockCount(height, texture->blockHeight);
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

        if (texture->isCompressed)
            return dataBytes;

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
        case GL_TEXTURE_3D:
            return GL_TEXTURE_BINDING_3D;
        case GL_TEXTURE_CUBE_MAP:
            return GL_TEXTURE_BINDING_CUBE_MAP;
        default:
            MGGL_FAIL("Unsupported texture target", "OpenGL texture target is not mapped");
        }
    }

    GLenum GetTextureImageTarget(const MGG_Texture* texture, mgint slice)
    {
        assert(texture != nullptr);
        assert(slice >= 0);
        assert(slice < texture->slices);

        switch (texture->type)
        {
        case MGTextureType::_2D:
            if (slice != 0)
                MGGL_FAIL("Unsupported texture slice", "2D textures only expose slice 0");

            return GL_TEXTURE_2D;
        case MGTextureType::_3D:
            if (slice != 0)
                MGGL_FAIL("Unsupported texture slice", "3D texture depth slices are not separate image targets");

            return GL_TEXTURE_3D;
        case MGTextureType::Cube:
            return GL_TEXTURE_CUBE_MAP_POSITIVE_X + slice;
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

    void AttachFramebufferColorTarget(MGG_GraphicsDevice* device, GLenum target, GLenum attachment, MGG_Texture* texture, mgint slice)
    {
        assert(device != nullptr);
        assert(texture != nullptr);
        assert(slice >= 0);

        if (UsesMultisampledRenderTarget(texture))
        {
            device->context.functions.FramebufferRenderbuffer(
                target,
                attachment,
                GL_RENDERBUFFER,
                texture->colorRenderbuffer);
            return;
        }

        device->context.functions.FramebufferTexture2D(
            target,
            attachment,
            GetTextureImageTarget(texture, slice),
            texture->handle,
            0);
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
        EnsureContext(device);

        if (type != MGTextureType::_3D && depth != 1)
            MGGL_FAIL("Unsupported texture depth", "non-3D textures use depth 1; array textures need separate support");

        TextureFormatInfo formatInfo = GetTextureFormatInfo(format);
        if (formatInfo.isCompressed
            && ((device->context.functions.CompressedTexImage2D == nullptr)
                || (device->context.functions.CompressedTexSubImage2D == nullptr)
                || (device->context.functions.GetCompressedTexImage == nullptr)
                || !(HasOpenGLExtension("GL_EXT_texture_compression_s3tc")
                    || HasOpenGLExtension("GL_OES_texture_compression_S3TC")
                    || HasOpenGLExtension("GL_EXT_texture_compression_dxt3")
                    || HasOpenGLExtension("GL_EXT_texture_compression_dxt5"))))
        {
            MGGL_FAIL("Unsupported surface format", "OpenGL compressed textures require S3TC support from the current driver");
        }

        MGG_Texture* texture = new MGG_Texture();
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
        texture->bytesPerBlock = formatInfo.bytesPerBlock;
        texture->blockWidth = formatInfo.blockWidth;
        texture->blockHeight = formatInfo.blockHeight;
        texture->isCompressed = formatInfo.isCompressed;

        switch (type)
        {
        case MGTextureType::_2D:
            if (slices != 1)
                MGGL_FAIL("Unsupported texture shape", "need to add 2D texture arrays");

            texture->target = GL_TEXTURE_2D;
            break;
        case MGTextureType::_3D:
                if (slices != 1)
                    MGGL_FAIL("Unsupported texture shape", "3D textures use depth, not array slices");

            texture->target = GL_TEXTURE_3D;
            break;
        case MGTextureType::Cube:
            if (width != height)
                MGGL_FAIL("Unsupported cube texture dimensions", "cube textures need square faces");

            if (slices != 6)
                MGGL_FAIL("Unsupported cube texture slice count", "cube textures need six faces");

            texture->target = GL_TEXTURE_CUBE_MAP;
            break;
        default:
            MGGL_FAIL("Unsupported texture shape", "need to add any remaining texture shapes separately");
        }

        if (formatInfo.isCompressed && type == MGTextureType::_3D)
            MGGL_FAIL("Unsupported texture shape", "need a separate path for compressed 3D textures");

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
        glTexParameteri(texture->target, GL_TEXTURE_WRAP_S, type == MGTextureType::Cube ? GL_CLAMP_TO_EDGE : GL_REPEAT);
        glTexParameteri(texture->target, GL_TEXTURE_WRAP_T, type == MGTextureType::Cube ? GL_CLAMP_TO_EDGE : GL_REPEAT);

        if (type == MGTextureType::Cube)
            glTexParameteri(texture->target, GL_TEXTURE_WRAP_R, GL_CLAMP_TO_EDGE);
        else if (type == MGTextureType::_3D)
            glTexParameteri(texture->target, GL_TEXTURE_WRAP_R, GL_REPEAT);

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
            mgint mipDepth = GetMipExtent(depth, level);

            if (type == MGTextureType::Cube)
            {
                for (mgint face = 0; face < 6; ++face)
                {
                    if (formatInfo.isCompressed)
                    {
                        device->context.functions.CompressedTexImage2D(
                            GetTextureImageTarget(texture, face),
                            level,
                            texture->internalFormat,
                            mipWidth,
                            mipHeight,
                            0,
                            GetTextureByteCount(texture, mipWidth, mipHeight, 1),
                            nullptr);
                    }
                    else
                    {
                        glTexImage2D(
                            GetTextureImageTarget(texture, face),
                            level,
                            texture->internalFormat,
                            mipWidth,
                            mipHeight,
                            0,
                            texture->pixelFormat,
                            texture->pixelType,
                            nullptr);
                    }
                }
            }
            else if (type == MGTextureType::_3D)
            {
                device->context.functions.TexImage3D(
                    texture->target,
                    level,
                    texture->internalFormat,
                    mipWidth,
                    mipHeight,
                    mipDepth,
                    0,
                    texture->pixelFormat,
                    texture->pixelType,
                    nullptr);
            }
            else if (formatInfo.isCompressed)
            {
                device->context.functions.CompressedTexImage2D(
                    texture->target,
                    level,
                    texture->internalFormat,
                    mipWidth,
                    mipHeight,
                    0,
                    GetTextureByteCount(texture, mipWidth, mipHeight, 1),
                    nullptr);
            }
            else
            {
                glTexImage2D(
                    texture->target,
                    level,
                    texture->internalFormat,
                    mipWidth,
                    mipHeight,
                    0,
                    texture->pixelFormat,
                    texture->pixelType,
                    nullptr);
            }
        }

        EndTextureEdit(device, texture, previousActiveTexture, previousBinding);
        return texture;
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

    void CopyWithStride(const mgbyte* source, mgbyte* destination, mgint itemCount, mgint sourceStride, mgint destinationStride, mgint elementBytes)
    {
        assert(source != nullptr);
        assert(destination != nullptr);
        assert(itemCount > 0);
        assert(sourceStride > 0);
        assert(destinationStride > 0);
        assert(elementBytes > 0);

        if (sourceStride == destinationStride && sourceStride == elementBytes)
        {
            memcpy(destination, source, itemCount * elementBytes);
            return;
        }

        for (mgint i = 0; i < itemCount; ++i)
        {
            memcpy(destination + i * destinationStride, source + i * sourceStride, elementBytes);
        }
    }
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

    if (device->context.handle != nullptr)
    {
        EnsureContext(device);

        for (MGG_ShaderProgram* program : device->programs)
            DestroyProgram(device, program);
    }
    else
    {
        for (MGG_ShaderProgram* program : device->programs)
            delete program;
    }

    device->programs.clear();

    device->context.Destroy();
    delete device;
}

void MGG_GraphicsDevice_GetCaps(MGG_GraphicsDevice* device, MGG_GraphicsDevice_Caps& caps)
{
    assert(device != nullptr);

    bool supportsNonPowerOfTwo = true;
    bool supportsTextureFilterAnisotropic = false;
    bool supportsDepth24 = true;
    bool supportsPackedDepthStencil = true;
    bool supportsDepthNonLinear = false;
    bool supportsTextureMaxLevel = true;
    bool supportsDxt1 = false;
    bool supportsS3tc = false;
    bool supportsSRgb = true;
    bool supportsDepthClamp = false;
    bool supportsVertexTextures = true;
    mgint maxTextureAnisotropy = 16;

    /*
     * OpenGl can only get texture capabilities if there is a context
     * Chris <aristurtledev>
     */
    if (device->window != nullptr && device->context.handle != nullptr)
    {
        EnsureContext(device);

        supportsS3tc = (device->context.functions.CompressedTexImage2D != nullptr)
            && (device->context.functions.CompressedTexSubImage2D != nullptr)
            && (device->context.functions.GetCompressedTexImage != nullptr)
            && (HasOpenGLExtension("GL_EXT_texture_compression_s3tc")
                || HasOpenGLExtension("GL_OES_texture_compression_S3TC")
                || HasOpenGLExtension("GL_EXT_texture_compression_dxt3")
                || HasOpenGLExtension("GL_EXT_texture_compression_dxt5"));
        supportsDxt1 = supportsS3tc || HasOpenGLExtension("GL_EXT_texture_compression_dxt1");
        supportsDepthClamp = SupportsOpenGLDepthClamp(device);
        supportsNonPowerOfTwo =
            HasOpenGLExtension("GL_ARB_texture_non_power_of_two") ||
            HasOpenGLExtension("GL_OES_texture_npot");
        supportsTextureFilterAnisotropic = HasOpenGLExtension("GL_EXT_texture_filter_anisotropic");

        GLint vertexTextureUnits = 0;
        glGetIntegerv(GL_MAX_VERTEX_TEXTURE_IMAGE_UNITS, &vertexTextureUnits);
        supportsVertexTextures = vertexTextureUnits > 0;

        if (supportsTextureFilterAnisotropic)
        {
            GLfloat reportedMaxAnisotropy = 1.0f;
            glGetFloatv(GL_MAX_TEXTURE_MAX_ANISOTROPY_EXT, &reportedMaxAnisotropy);
            if (reportedMaxAnisotropy > 1.0f)
                maxTextureAnisotropy = static_cast<mgint>(reportedMaxAnisotropy);
            else
                maxTextureAnisotropy = 1;
        }
        else
        {
            maxTextureAnisotropy = 1;
        }
    }

    caps.MaxTextureSlots = MaxTextureSlots;
    caps.MaxVertexTextureSlots = MaxVertexTextureSlots;
    caps.MaxVertexBufferSlots = MaxVertexBufferSlots;
    caps.ShaderProfile = OpenGLShaderProfile;
    caps.MaxTextureAnisotropy = maxTextureAnisotropy;
    caps.SupportsNonPowerOfTwo = supportsNonPowerOfTwo;
    caps.SupportsTextureFilterAnisotropic = supportsTextureFilterAnisotropic;
    caps.SupportsDepth24 = supportsDepth24;
    caps.SupportsPackedDepthStencil = supportsPackedDepthStencil;
    caps.SupportsDepthNonLinear = supportsDepthNonLinear;
    caps.SupportsTextureMaxLevel = supportsTextureMaxLevel;
    caps.SupportsDxt1 = supportsDxt1;
    caps.SupportsS3tc = supportsS3tc;
    caps.SupportsSRgb = supportsSRgb;
    caps.SupportsDepthClamp = supportsDepthClamp;
    caps.SupportsTextureArrays = false;
    caps.SupportsVertexTextures = supportsVertexTextures;
    caps.SupportsFloatTextures = true;
    caps.SupportsHalfFloatTextures = true;
    caps.SupportsNormalized = true;
    caps.SupportsInstancing = true;
    caps.SupportsBaseIndexInstancing = true;
    caps.SupportsSeparateBlendStates = true;
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
    ClearCurrentRenderTargets(device);
    device->inputLayout = nullptr;
    device->currentProgram = nullptr;
    device->context.width = width;
    device->context.height = height;
    device->context.SetSwapInterval(syncInterval);

    device->backBufferWidth = width;
    device->backBufferHeight = height;
    device->backBufferFormat = color;
    device->depthFormat = depth;
    device->multiSampleCount = multiSampleCount;
    device->viewportX = 0;
    device->viewportY = 0;
    device->viewportWidth = width;
    device->viewportHeight = height;
    device->viewportMinDepth = 0.0f;
    device->viewportMaxDepth = 1.0f;

    glViewport(0, 0, width, height);
    glScissor(0, 0, width, height);
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

    GLboolean colorWriteMask[4] = {};
    GLboolean depthWriteMask = GL_FALSE;
    GLint stencilWriteMask = 0;
    GLint scissorBox[4] = {};
    GLboolean depthTestEnabled = glIsEnabled(GL_DEPTH_TEST);
    GLboolean stencilTestEnabled = glIsEnabled(GL_STENCIL_TEST);

    glGetBooleanv(GL_COLOR_WRITEMASK, colorWriteMask);
    glGetBooleanv(GL_DEPTH_WRITEMASK, &depthWriteMask);
    glGetIntegerv(GL_STENCIL_WRITEMASK, &stencilWriteMask);
    glGetIntegerv(GL_SCISSOR_BOX, scissorBox);

    glColorMask(GL_TRUE, GL_TRUE, GL_TRUE, GL_TRUE);
    glDepthMask(GL_TRUE);
    glStencilMask(static_cast<GLuint>(~0u));
    glScissor(device->viewportX, ToOpenGLWindowY(device, device->viewportY, device->viewportHeight), device->viewportWidth, device->viewportHeight);

    if ((static_cast<mgint>(options) & static_cast<mgint>(MGClearOptions::DepthBuffer)) != 0)
        glEnable(GL_DEPTH_TEST);

    if ((static_cast<mgint>(options) & static_cast<mgint>(MGClearOptions::Stencil)) != 0)
        glEnable(GL_STENCIL_TEST);

    glClearColor(color.X, color.Y, color.Z, color.W);
    glClearDepth(depth);
    glClearStencil(stencil);
    glClear(ToClearMask(options));

    glColorMask(colorWriteMask[0], colorWriteMask[1], colorWriteMask[2], colorWriteMask[3]);
    glDepthMask(depthWriteMask);
    glStencilMask(static_cast<GLuint>(stencilWriteMask));
    glScissor(scissorBox[0], scissorBox[1], scissorBox[2], scissorBox[3]);

    if (depthTestEnabled)
        glEnable(GL_DEPTH_TEST);
    else
        glDisable(GL_DEPTH_TEST);

    if (stencilTestEnabled)
        glEnable(GL_STENCIL_TEST);
    else
        glDisable(GL_STENCIL_TEST);
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

    if (state == nullptr)
        return;

    EnsureContext(device);

    const MGG_BlendState_Info& info = state->infos[0];
    bool blendEnabled = !(info.colorSourceBlend == MGBlend::One &&
        info.colorDestBlend == MGBlend::Zero &&
        info.alphaSourceBlend == MGBlend::One &&
        info.alphaDestBlend == MGBlend::Zero);

    if (blendEnabled)
        glEnable(GL_BLEND);
    else
        glDisable(GL_BLEND);

    device->context.functions.BlendColor(factorR, factorG, factorB, factorA);
    device->context.functions.BlendEquationSeparate(
        ToBlendEquation(info.colorBlendFunc),
        ToBlendEquation(info.alphaBlendFunc));
    device->context.functions.BlendFuncSeparate(
        ToBlendFactor(info.colorSourceBlend),
        ToBlendFactor(info.colorDestBlend),
        ToBlendFactor(info.alphaSourceBlend),
        ToBlendFactor(info.alphaDestBlend));

    GLboolean writeRed = GL_TRUE;
    GLboolean writeGreen = GL_TRUE;
    GLboolean writeBlue = GL_TRUE;
    GLboolean writeAlpha = GL_TRUE;
    ToColorMask(info.colorWriteChannels, writeRed, writeGreen, writeBlue, writeAlpha);
    glColorMask(writeRed, writeGreen, writeBlue, writeAlpha);

    device->blendState = state;
}

void MGG_GraphicsDevice_SetDepthStencilState(MGG_GraphicsDevice* device, MGG_DepthStencilState* state)
{
    assert(device != nullptr);

    if (state == nullptr)
        return;

    EnsureContext(device);

    const MGG_DepthStencilState_Info& info = state->info;

    if (info.depthBufferEnable)
        glEnable(GL_DEPTH_TEST);
    else
        glDisable(GL_DEPTH_TEST);

    glDepthFunc(ToCompareFunction(info.depthBufferFunction));
    glDepthMask(info.depthBufferWriteEnable ? GL_TRUE : GL_FALSE);

    if (info.stencilEnable)
        glEnable(GL_STENCIL_TEST);
    else
        glDisable(GL_STENCIL_TEST);

    glStencilFunc(
        ToCompareFunction(info.stencilFunction),
        info.referenceStencil,
        static_cast<GLuint>(info.stencilMask));
    glStencilOp(
        ToStencilOperation(info.stencilFail),
        ToStencilOperation(info.stencilDepthBufferFail),
        ToStencilOperation(info.stencilPass));
    glStencilMask(static_cast<GLuint>(info.stencilWriteMask));

    device->depthStencilState = state;
}

void MGG_GraphicsDevice_SetRasterizerState(MGG_GraphicsDevice* device, MGG_RasterizerState* state)
{
    assert(device != nullptr);

    if (state == nullptr)
        return;

    EnsureContext(device);

    const MGG_RasterizerState_Info& info = state->info;
    bool offscreen = device->currentRenderTargetCount > 0;

    glDisable(GL_DITHER);

    if (info.cullMode == MGCullMode::None)
    {
        glDisable(GL_CULL_FACE);
    }
    else
    {
        glEnable(GL_CULL_FACE);
        glCullFace(GL_BACK);

        if (info.cullMode == MGCullMode::CullClockwiseFace)
            glFrontFace(offscreen ? GL_CW : GL_CCW);
        else
            glFrontFace(offscreen ? GL_CCW : GL_CW);
    }

    glPolygonMode(GL_FRONT_AND_BACK, ToPolygonMode(info.fillMode));

    if (info.scissorTestEnable)
        glEnable(GL_SCISSOR_TEST);
    else
        glDisable(GL_SCISSOR_TEST);

    if (info.depthBias != 0.0f || info.slopeScaleDepthBias != 0.0f)
    {
        GLint depthMul = 0;
        switch (GetActiveDepthFormat(device))
        {
        case MGDepthFormat::None:
            depthMul = 0;
            break;
        case MGDepthFormat::Depth16:
            depthMul = 65535;
            break;
        case MGDepthFormat::Depth24:
        case MGDepthFormat::Depth24Stencil8:
            depthMul = 16777215;
            break;
        default:
            MGGL_FAIL("Unsupported depth format", "unknown OpenGL depth format for polygon offset");
        }

        glEnable(GL_POLYGON_OFFSET_FILL);
        glPolygonOffset(info.slopeScaleDepthBias, info.depthBias * depthMul);
    }
    else
    {
        glDisable(GL_POLYGON_OFFSET_FILL);
    }

    if (SupportsOpenGLDepthClamp(device))
    {
        if (!info.depthClipEnable)
            glEnable(GL_DEPTH_CLAMP);
        else
            glDisable(GL_DEPTH_CLAMP);
    }

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

    device->viewportX = x;
    device->viewportY = y;
    device->viewportWidth = width;
    device->viewportHeight = height;
    device->viewportMinDepth = minDepth;
    device->viewportMaxDepth = maxDepth;

    glViewport(x, ToOpenGLWindowY(device, y, height), width, height);
    glDepthRange(minDepth, maxDepth);
    ApplyPosFixup(device);
}

void MGG_GraphicsDevice_SetScissorRectangle(MGG_GraphicsDevice* device, mgint x, mgint y, mgint width, mgint height)
{
    assert(device != nullptr);

    EnsureContext(device);

    glScissor(x, ToOpenGLWindowY(device, y, height), width, height);
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
        ClearCurrentRenderTargets(device);
        ApplyPosFixup(device);
        return;
    }

    if (targets == nullptr || arraySlices == nullptr || targets[0] == nullptr)
        MGGL_FAIL("Invalid render target binding", "need at least one valid render target");

    /*
     * Keep this capped at 4 for now so I can get the binding path in
     * place first before trying to widen the implementation
     * Chris <aristurtledev>
     */
    if (count > MaxRenderTargetBindings)
        MGGL_FAIL("Unsupported render target count", "need to widen the native render target binding path past 4");

    MGG_Texture* firstTarget = targets[0];
    if (!firstTarget->isRenderTarget || firstTarget->framebuffer == 0)
        MGGL_FAIL("Invalid render target binding", "texture does not own a framebuffer");

    device->context.functions.BindFramebuffer(GL_FRAMEBUFFER, firstTarget->framebuffer);
    mgint multiSampleCount = firstTarget->multiSampleCount;

    std::array<GLenum, MaxRenderTargetBindings> drawBuffers = {};

    for (mgint i = 0; i < count; ++i)
    {
        MGG_Texture* target = targets[i];
        if (target == nullptr)
            MGGL_FAIL("Invalid render target binding", "need every render target slot to have a texture");

        if (!target->isRenderTarget || target->framebuffer == 0)
            MGGL_FAIL("Invalid render target binding", "texture does not own a framebuffer");

        if (target->multiSampleCount != multiSampleCount)
            MGGL_FAIL("Unsupported render target multisampling", "need every bound render target to use the same sample count");

        mgint arraySlice = arraySlices[i];
        if (arraySlice < 0 || arraySlice >= target->slices)
            MGGL_FAIL("Unsupported render target slice", "need the bound render target slice to stay in range");

        if (target->type == MGTextureType::_2D && arraySlice != 0)
            MGGL_FAIL("Unsupported render target slice", "2D render targets only expose slice 0");

        // TODO: add array render target binding
        if (target->type != MGTextureType::_2D && target->type != MGTextureType::Cube)
            MGGL_FAIL("Unsupported render target shape", "need array render target binding");

        GLenum attachment = GL_COLOR_ATTACHMENT0 + i;
        AttachFramebufferColorTarget(device, GL_FRAMEBUFFER, attachment, target, arraySlice);
        drawBuffers[i] = attachment;
        device->currentRenderTargets[i] = target;
        device->currentRenderTargetSlices[i] = arraySlice;
    }

    for (mgint i = count; i < MaxRenderTargetBindings; ++i)
    {
        device->context.functions.FramebufferRenderbuffer(
            GL_FRAMEBUFFER,
            GL_COLOR_ATTACHMENT0 + i,
            GL_RENDERBUFFER,
            0);
        device->currentRenderTargets[i] = nullptr;
        device->currentRenderTargetSlices[i] = 0;
    }

    GLenum framebufferStatus = device->context.functions.CheckFramebufferStatus(GL_FRAMEBUFFER);
    if (framebufferStatus != GL_FRAMEBUFFER_COMPLETE)
        MGGL_FAIL("OpenGL framebuffer incomplete", "render target binding left the framebuffer incomplete");

    device->context.functions.DrawBuffers(count, drawBuffers.data());
    glReadBuffer(GL_COLOR_ATTACHMENT0);
    device->currentRenderTarget = firstTarget;
    device->currentRenderTargetCount = count;
    device->currentFramebuffer = firstTarget->framebuffer;
    ApplyPosFixup(device);
}

void MGG_GraphicsDevice_SetConstantBuffer(MGG_GraphicsDevice* device, MGShaderStage stage, mgint slot, MGG_Buffer* buffer)
{
    assert(device != nullptr);

    if (buffer == nullptr)
        return;

    EnsureContext(device);

    size_t stageIndex = ToStageIndex(stage);
    if (device->currentProgram == nullptr)
        MGGL_FAIL("Constant buffer binding requires a linked program", "set vertex and pixel shaders before applying effect constants");

    if (slot < 0 || static_cast<size_t>(slot) >= device->currentProgram->constantBufferLocations[stageIndex].size())
        MGGL_FAIL("Invalid constant buffer slot", "shader constant buffer slot is outside the linked program range");

    GLint location = device->currentProgram->constantBufferLocations[stageIndex][slot];
    if (location < 0)
        return;

    GLenum constantType = device->shaders[stageIndex]->constantBufferTypes[slot];
    mgint registerCount = GetConstantRegisterCount(buffer);
    device->context.functions.BindBuffer(buffer->target, buffer->handle);
    void* mapped = device->context.functions.MapBuffer(buffer->target, GL_READ_ONLY);
    if (mapped == nullptr)
        MGGL_FAIL("glMapBuffer failed", "constant buffer upload could not map buffer contents");

    if (constantType == GL_BOOL || constantType == GL_INT)
    {
        device->context.functions.Uniform4iv(
            location,
            registerCount,
            reinterpret_cast<const GLint*>(mapped));
        if (device->context.functions.UnmapBuffer(buffer->target) != GL_TRUE)
            MGGL_FAIL("glUnmapBuffer failed", "constant buffer upload could not unmap buffer contents");
        return;
    }

    device->context.functions.Uniform4fv(
        location,
        registerCount,
        reinterpret_cast<const GLfloat*>(mapped));
    if (device->context.functions.UnmapBuffer(buffer->target) != GL_TRUE)
        MGGL_FAIL("glUnmapBuffer failed", "constant buffer upload could not unmap buffer contents");
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
    device->inputLayoutDirty = true;
}

void MGG_GraphicsDevice_SetShader(MGG_GraphicsDevice* device, MGShaderStage stage, MGG_Shader* shader)
{
    assert(device != nullptr);
    assert(shader != nullptr);
    assert(shader->stage == stage);

    EnsureContext(device);

    size_t stageIndex = ToStageIndex(stage);
    device->shaders[stageIndex] = shader;

    if (device->shaders[ToStageIndex(MGShaderStage::Vertex)] == nullptr ||
        device->shaders[ToStageIndex(MGShaderStage::Pixel)] == nullptr)
    {
        return;
    }

    MGG_ShaderProgram* program = GetOrCreateProgram(device);
    if (device->currentProgram != program)
    {
        device->context.functions.UseProgram(program->handle);
        device->currentProgram = program;
        device->inputLayoutDirty = true;
        ApplyPosFixup(device);
    }
}

void MGG_GraphicsDevice_SetInputLayout(MGG_GraphicsDevice* device, MGG_InputLayout* layout)
{
    assert(device != nullptr);

    if (layout == nullptr)
        return;

    device->inputLayout = layout;
    
    /*
     * Do we need to call ApplyLayout here???
     */

    device->inputLayoutDirty = true;
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

    if (device->inputLayoutDirty)
        ApplyInputLayout(device, 0);

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

    /*
     * Need to fold the vertex start into the attribute offsets here
     * instead of passing it through the draw call.
     * Chris <aristurtledev>
     */
    ApplyInputLayout(device, vertexStart);

    if (device->indexBuffer == nullptr)
        MGGL_FAIL("Indexed draw requires an index buffer", "MGG_GraphicsDevice_SetIndexBuffer must bind a buffer before DrawIndexed");

    mgint indexCount = GetIndexedElementCount(primitiveType, primitiveCount);
    mgint indexSizeInBytes = GetIndexElementSizeInBytes(device->indexElementSize);
    intptr_t indexByteOffset = static_cast<intptr_t>(indexStart) * indexSizeInBytes;

    glDrawElements(
        ToPrimitiveMode(primitiveType),
        indexCount,
        ToIndexType(device->indexElementSize),
        reinterpret_cast<const void*>(indexByteOffset));
}

void MGG_GraphicsDevice_DrawIndexedInstanced(MGG_GraphicsDevice* device, MGPrimitiveType primitiveType, mgint primitiveCount, mgint indexStart, mgint vertexStart, mgint instanceCount)
{
    assert(device != nullptr);
    assert(indexStart >= 0);
    assert(vertexStart >= 0);
    assert(instanceCount >= 0);

    if (primitiveCount <= 0 || instanceCount <= 0)
        return;

    EnsureContext(device);
    EnsureVertexArray(device);
    assert(device->isInFrame);

    /*
     * Need to fold the vertex start into the attribute offsets here
     * instead of passing it through the draw call.
     * Chris <aristurtledev>
     */
    ApplyInputLayout(device, vertexStart);

    if (device->indexBuffer == nullptr)
        MGGL_FAIL("Indexed instanced draw requires an index buffer", "MGG_GraphicsDevice_SetIndexBuffer must bind a buffer before DrawIndexedInstanced");

    mgint indexCount = GetIndexedElementCount(primitiveType, primitiveCount);
    mgint indexSizeInBytes = GetIndexElementSizeInBytes(device->indexElementSize);
    intptr_t indexByteOffset = static_cast<intptr_t>(indexStart) * indexSizeInBytes;

    device->context.functions.DrawElementsInstanced(
        ToPrimitiveMode(primitiveType),
        indexCount,
        ToIndexType(device->indexElementSize),
        reinterpret_cast<const void*>(indexByteOffset),
        instanceCount);
}

void MGG_GraphicsDevice_ResolveRenderTargets(MGG_GraphicsDevice* device)
{
    assert(device != nullptr);

    EnsureContext(device);

    if (device->currentRenderTargetCount == 0)
        return;

    if (UsesMultisampledRenderTarget(device->currentRenderTargets[0]))
    {
        bool restoreScissor = device->rasterizerState != nullptr && device->rasterizerState->info.scissorTestEnable;
        if (restoreScissor)
            glDisable(GL_SCISSOR_TEST);

        device->context.functions.BindFramebuffer(GL_READ_FRAMEBUFFER, device->currentFramebuffer);

        for (mgint i = 0; i < device->currentRenderTargetCount; ++i)
        {
            MGG_Texture* renderTarget = device->currentRenderTargets[i];
            assert(renderTarget != nullptr);
            assert(renderTarget->resolveFramebuffer != 0);

            device->context.functions.BindFramebuffer(GL_DRAW_FRAMEBUFFER, renderTarget->resolveFramebuffer);
            device->context.functions.FramebufferTexture2D(
                GL_DRAW_FRAMEBUFFER,
                GL_COLOR_ATTACHMENT0,
                GetTextureImageTarget(renderTarget, device->currentRenderTargetSlices[i]),
                renderTarget->handle,
                0);
            glReadBuffer(GL_COLOR_ATTACHMENT0 + i);
            glDrawBuffer(GL_COLOR_ATTACHMENT0);
            device->context.functions.BlitFramebuffer(
                0,
                0,
                renderTarget->width,
                renderTarget->height,
                0,
                0,
                renderTarget->width,
                renderTarget->height,
                GL_COLOR_BUFFER_BIT,
                GL_NEAREST);
        }

        device->context.functions.BindFramebuffer(GL_FRAMEBUFFER, device->currentFramebuffer);
        glReadBuffer(GL_COLOR_ATTACHMENT0);
        if (restoreScissor)
            glEnable(GL_SCISSOR_TEST);
    }

    for (mgint i = 0; i < device->currentRenderTargetCount; ++i)
    {
        MGG_Texture* renderTarget = device->currentRenderTargets[i];
        assert(renderTarget != nullptr);
        assert(renderTarget->isRenderTarget);
        assert(renderTarget->mipmaps > 0);

        if (renderTarget->mipmaps <= 1)
            continue;

        GLint previousActiveTexture = 0;
        GLint previousBinding = 0;
        BeginTextureEdit(device, renderTarget, previousActiveTexture, previousBinding);
        device->context.functions.GenerateMipmap(renderTarget->target);
        EndTextureEdit(device, renderTarget, previousActiveTexture, previousBinding);
    }
}

void MGG_GraphicsDevice_GetBackBufferData(MGG_GraphicsDevice* device, mgint x, mgint y, mgint width, mgint height, void* data, mgint count, mgint dataBytes)
{
    assert(device != nullptr);
    assert(data != nullptr);
    assert(count > 0);
    assert(dataBytes > 0);

    EnsureContext(device);

    if (width <= 0 || height <= 0)
        MGGL_FAIL("Invalid backbuffer readback region", "requested backbuffer readback region must be positive");

    if (x < 0 || y < 0 || x + width > device->backBufferWidth || y + height > device->backBufferHeight)
        MGGL_FAIL("Invalid backbuffer readback region", "requested backbuffer readback region exceeds the current backbuffer bounds");

    TextureFormatInfo formatInfo = GetTextureFormatInfo(device->backBufferFormat);
    size_t rowBytes = static_cast<size_t>(width) * static_cast<size_t>(formatInfo.bytesPerPixel);
    size_t totalBytes = rowBytes * static_cast<size_t>(height);
    size_t destinationBytes = static_cast<size_t>(count) * static_cast<size_t>(dataBytes);
    if (destinationBytes < totalBytes)
        MGGL_FAIL("Backbuffer readback size mismatch", "destination buffer is smaller than the requested region");

    GLint previousFramebuffer = 0;
    GLint previousReadBuffer = 0;
    GLint previousPackAlignment = 0;
    glGetIntegerv(GL_FRAMEBUFFER_BINDING, &previousFramebuffer);
    glGetIntegerv(GL_READ_BUFFER, &previousReadBuffer);
    glGetIntegerv(GL_PACK_ALIGNMENT, &previousPackAlignment);

    device->context.functions.BindFramebuffer(GL_FRAMEBUFFER, 0);
    glReadBuffer(GL_BACK);
    glPixelStorei(GL_PACK_ALIGNMENT, 1);

    std::vector<mgbyte> pixels(totalBytes);
    mgint flippedY = device->backBufferHeight - y - height;
    glReadPixels(
        x,
        flippedY,
        width,
        height,
        formatInfo.pixelFormat,
        formatInfo.pixelType,
        pixels.data());

    mgbyte* destination = static_cast<mgbyte*>(data);
    for (mgint row = 0; row < height; ++row)
    {
        size_t sourceOffset = static_cast<size_t>(height - row - 1) * rowBytes;
        size_t destinationOffset = static_cast<size_t>(row) * rowBytes;
        memcpy(destination + destinationOffset, pixels.data() + sourceOffset, rowBytes);
    }

    glPixelStorei(GL_PACK_ALIGNMENT, previousPackAlignment);
    glReadBuffer(previousReadBuffer);
    device->context.functions.BindFramebuffer(GL_FRAMEBUFFER, static_cast<GLuint>(previousFramebuffer));
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

    device->context.functions.GenBuffers(1, &buffer->handle);
    if (buffer->handle == 0)
        MGGL_FAIL("glGenBuffers failed", "buffer creation returned 0");

    device->context.functions.BindBuffer(buffer->target, buffer->handle);
    device->context.functions.BufferData(buffer->target, sizeInBytes, nullptr, buffer->usage);

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

    EnsureContext(device);
    device->context.functions.BindBuffer(buffer->target, buffer->handle);

    if (discard)
    {
        device->context.functions.BufferData(buffer->target, buffer->sizeInBytes, nullptr, buffer->usage);
    }

    if (elementSizeInBytes == vertexStride || elementSizeInBytes % vertexStride == 0)
    {
        mgint copySpan = elementCount * elementSizeInBytes;
        assert(offset + copySpan <= buffer->sizeInBytes);
        device->context.functions.BufferSubData(buffer->target, offset, copySpan, data);
        return;
    }

    mgint copySpan = GetCopySpan(elementCount, vertexStride, elementSizeInBytes);
    assert(offset + copySpan <= buffer->sizeInBytes);

    for (mgint elementIndex = 0; elementIndex < elementCount; ++elementIndex)
    {
        mgint destinationOffset = offset + elementIndex * vertexStride;
        const mgbyte* source = data + elementIndex * elementSizeInBytes;
        device->context.functions.BufferSubData(buffer->target, destinationOffset, elementSizeInBytes, source);
    }
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

    EnsureContext(device);
    device->context.functions.BindBuffer(buffer->target, buffer->handle);
    void* mapped = device->context.functions.MapBuffer(buffer->target, GL_READ_ONLY);
    if (mapped == nullptr)
        MGGL_FAIL("glMapBuffer failed", "buffer readback could not map buffer contents");

    const mgbyte* source = reinterpret_cast<const mgbyte*>(mapped) + offset;
    CopyWithStride(source, data, dataCount, dataStride, dataBytes, dataBytes < dataStride ? dataBytes : dataStride);

    if (device->context.functions.UnmapBuffer(buffer->target) != GL_TRUE)
        MGGL_FAIL("glUnmapBuffer failed", "buffer readback could not unmap buffer contents");
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

    if (type == MGTextureType::_3D)
        // TODO: add 3D render target support once there is a layer aware framebuffer attachment path
        MGGL_FAIL("Unsupported render target shape", "need a layer aware framebuffer attachment path for 3D render targets");

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

    if (multiSampleCount > 0)
    {
        device->context.functions.GenFramebuffers(1, &texture->resolveFramebuffer);
        if (texture->resolveFramebuffer == 0)
            MGGL_FAIL("glGenFramebuffers failed", "resolve framebuffer creation returned 0");
    }

    device->context.functions.BindFramebuffer(GL_FRAMEBUFFER, texture->framebuffer);
    if (multiSampleCount > 0)
    {
        device->context.functions.GenRenderbuffers(1, &texture->colorRenderbuffer);
        if (texture->colorRenderbuffer == 0)
            MGGL_FAIL("glGenRenderbuffers failed", "color attachment creation returned 0");

        device->context.functions.BindRenderbuffer(GL_RENDERBUFFER, texture->colorRenderbuffer);
        device->context.functions.RenderbufferStorageMultisample(
            GL_RENDERBUFFER,
            multiSampleCount,
            texture->internalFormat,
            width,
            height);
        device->context.functions.FramebufferRenderbuffer(
            GL_FRAMEBUFFER,
            GL_COLOR_ATTACHMENT0,
            GL_RENDERBUFFER,
            texture->colorRenderbuffer);
    }
    else
    {
        device->context.functions.FramebufferTexture2D(
            GL_FRAMEBUFFER,
            GL_COLOR_ATTACHMENT0,
            GetTextureImageTarget(texture, 0),
            texture->handle,
            0);
    }
    glDrawBuffer(GL_COLOR_ATTACHMENT0);
    glReadBuffer(GL_COLOR_ATTACHMENT0);

    if (depthFormat != MGDepthFormat::None)
    {
        device->context.functions.GenRenderbuffers(1, &texture->depthRenderbuffer);
        if (texture->depthRenderbuffer == 0)
            MGGL_FAIL("glGenRenderbuffers failed", "depth renderbuffer creation returned 0");

        device->context.functions.BindRenderbuffer(GL_RENDERBUFFER, texture->depthRenderbuffer);
        if (multiSampleCount > 0)
        {
            device->context.functions.RenderbufferStorageMultisample(
                GL_RENDERBUFFER,
                multiSampleCount,
                ToDepthRenderbufferFormat(depthFormat),
                width,
                height);
        }
        else
        {
            device->context.functions.RenderbufferStorage(
                GL_RENDERBUFFER,
                ToDepthRenderbufferFormat(depthFormat),
                width,
                height);
        }
        device->context.functions.FramebufferRenderbuffer(
            GL_FRAMEBUFFER,
            ToDepthAttachment(depthFormat),
            GL_RENDERBUFFER,
            texture->depthRenderbuffer);
    }

    GLenum framebufferStatus = device->context.functions.CheckFramebufferStatus(GL_FRAMEBUFFER);
    if (framebufferStatus != GL_FRAMEBUFFER_COMPLETE)
        MGGL_FAIL("OpenGL framebuffer incomplete", "framebuffer status check failed");

    if (multiSampleCount > 0)
    {
        device->context.functions.BindFramebuffer(GL_FRAMEBUFFER, texture->resolveFramebuffer);
        device->context.functions.FramebufferTexture2D(
            GL_FRAMEBUFFER,
            GL_COLOR_ATTACHMENT0,
            GetTextureImageTarget(texture, 0),
            texture->handle,
            0);
        glDrawBuffer(GL_COLOR_ATTACHMENT0);
        glReadBuffer(GL_COLOR_ATTACHMENT0);

        framebufferStatus = device->context.functions.CheckFramebufferStatus(GL_FRAMEBUFFER);
        if (framebufferStatus != GL_FRAMEBUFFER_COMPLETE)
            MGGL_FAIL("OpenGL framebuffer incomplete", "resolve framebuffer status check failed");
    }

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

    if (IsRenderTargetBound(device, texture))
    {
        device->context.functions.BindFramebuffer(GL_FRAMEBUFFER, 0);
        glDrawBuffer(GL_BACK);
        glReadBuffer(GL_BACK);
        ClearCurrentRenderTargets(device);
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

    if (texture->colorRenderbuffer != 0)
        device->context.functions.DeleteRenderbuffers(1, &texture->colorRenderbuffer);

    if (texture->resolveFramebuffer != 0)
        device->context.functions.DeleteFramebuffers(1, &texture->resolveFramebuffer);

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

    mgint expectedBytes = GetTextureByteCount(texture, resolvedWidth, resolvedHeight, resolvedDepth);
    mgint normalizedBytes = NormalizeUploadByteCount(texture, resolvedWidth, resolvedHeight, resolvedDepth, dataBytes);
    if (normalizedBytes != expectedBytes)
        MGGL_FAIL("Texture upload size mismatch", "byte count does not match the upload region");

    GLint previousActiveTexture = 0;
    GLint previousBinding = 0;
    glPixelStorei(GL_UNPACK_ALIGNMENT, 1);
    BeginTextureEdit(device, texture, previousActiveTexture, previousBinding);
    GLenum imageTarget = GetTextureImageTarget(texture, slice);
    if (texture->isCompressed)
    {
        mgint mipWidth = GetMipExtent(texture->width, level);
        mgint mipHeight = GetMipExtent(texture->height, level);
        mgint mipDepth = GetMipExtent(texture->depth, level);
        bool isFullLevelRegion = x == 0
            && y == 0
            && z == 0
            && resolvedWidth == mipWidth
            && resolvedHeight == mipHeight
            && resolvedDepth == mipDepth;

        if (isFullLevelRegion)
        {
            device->context.functions.CompressedTexImage2D(
                imageTarget,
                level,
                texture->internalFormat,
                mipWidth,
                mipHeight,
                0,
                normalizedBytes,
                data);
        }
        else
        {
            device->context.functions.CompressedTexSubImage2D(
                imageTarget,
                level,
                x,
                y,
                resolvedWidth,
                resolvedHeight,
                texture->internalFormat,
                normalizedBytes,
                data);
        }
    }
    else if (texture->type == MGTextureType::_3D)
    {
        device->context.functions.TexSubImage3D(
            imageTarget,
            level,
            x,
            y,
            z,
            resolvedWidth,
            resolvedHeight,
            resolvedDepth,
            texture->pixelFormat,
            texture->pixelType,
            data);
    }
    else
    {
        glTexSubImage2D(
            imageTarget,
            level,
            x,
            y,
            resolvedWidth,
            resolvedHeight,
            texture->pixelFormat,
            texture->pixelType,
            data);
    }
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

    if (texture->isRenderTarget && IsRenderTargetBound(device, texture))
        MGG_GraphicsDevice_ResolveRenderTargets(device);

    mgint resolvedWidth = 0;
    mgint resolvedHeight = 0;
    mgint resolvedDepth = 0;
    ResolveTextureRegion(texture, level, slice, x, y, z, width, height, depth, resolvedWidth, resolvedHeight, resolvedDepth);

    mgint expectedBytes = GetTextureByteCount(texture, resolvedWidth, resolvedHeight, resolvedDepth);
    if (dataBytes < expectedBytes)
        MGGL_FAIL("Texture readback size mismatch", "destination buffer is smaller than the requested region");

    GLint previousActiveTexture = 0;
    GLint previousBinding = 0;
    glPixelStorei(GL_PACK_ALIGNMENT, 1);
    BeginTextureEdit(device, texture, previousActiveTexture, previousBinding);
    GLenum imageTarget = GetTextureImageTarget(texture, slice);

    mgint mipWidth = GetMipExtent(texture->width, level);
    mgint mipHeight = GetMipExtent(texture->height, level);
    mgint mipDepth = GetMipExtent(texture->depth, level);
    mgint fullLevelBytes = GetTextureByteCount(texture, mipWidth, mipHeight, mipDepth);

    if (x == 0 && y == 0 && z == 0 && resolvedWidth == mipWidth && resolvedHeight == mipHeight && resolvedDepth == mipDepth)
    {
        if (texture->isCompressed)
            device->context.functions.GetCompressedTexImage(imageTarget, level, data);
        else
            device->context.functions.GetTexImage(imageTarget, level, texture->pixelFormat, texture->pixelType, data);
        EndTextureEdit(device, texture, previousActiveTexture, previousBinding);
        return;
    }

    std::vector<mgbyte> fullLevelData(static_cast<size_t>(fullLevelBytes));
    if (texture->isCompressed)
    {
        device->context.functions.GetCompressedTexImage(imageTarget, level, fullLevelData.data());

        mgint sourceRowBytes = GetTextureRowBytes(texture, mipWidth);
        mgint destinationRowBytes = GetTextureRowBytes(texture, resolvedWidth);
        mgint sourceSliceBytes = GetTextureSliceByteCount(texture, mipWidth, mipHeight);
        mgint destinationSliceBytes = GetTextureSliceByteCount(texture, resolvedWidth, resolvedHeight);
        mgint sourceBlockX = x / texture->blockWidth;
        mgint sourceBlockY = y / texture->blockHeight;
        mgint rowCount = GetTextureBlockCount(resolvedHeight, texture->blockHeight);
        mgint blockOffsetBytes = sourceBlockX * texture->bytesPerBlock;

        for (mgint depthIndex = 0; depthIndex < resolvedDepth; ++depthIndex)
        {
            const mgbyte* sourceSlice = fullLevelData.data() + depthIndex * sourceSliceBytes + sourceBlockY * sourceRowBytes + blockOffsetBytes;
            mgbyte* destinationSlice = data + depthIndex * destinationSliceBytes;

            for (mgint row = 0; row < rowCount; ++row)
                memcpy(destinationSlice + row * destinationRowBytes, sourceSlice + row * sourceRowBytes, destinationRowBytes);
        }
    }
    else
    {
        device->context.functions.GetTexImage(
            imageTarget,
            level,
            texture->pixelFormat,
            texture->pixelType,
            fullLevelData.data());

        mgint sourceRowBytes = GetTextureRowBytes(texture, mipWidth);
        mgint destinationRowBytes = GetTextureRowBytes(texture, resolvedWidth);
        mgint sourceSliceBytes = GetTextureSliceByteCount(texture, mipWidth, mipHeight);
        mgint destinationSliceBytes = GetTextureSliceByteCount(texture, resolvedWidth, resolvedHeight);
        const mgbyte* source = fullLevelData.data() + (((z * mipHeight + y) * mipWidth) + x) * texture->bytesPerPixel;
        for (mgint depthIndex = 0; depthIndex < resolvedDepth; ++depthIndex)
        {
            const mgbyte* sourceSlice = source + depthIndex * sourceSliceBytes;
            mgbyte* destinationSlice = data + depthIndex * destinationSliceBytes;

            for (mgint row = 0; row < resolvedHeight; ++row)
                memcpy(destinationSlice + row * destinationRowBytes, sourceSlice + row * sourceRowBytes, destinationRowBytes);
        }
    }

    EndTextureEdit(device, texture, previousActiveTexture, previousBinding);
}

MGG_InputLayout* MGG_InputLayout_Create(MGG_GraphicsDevice* device, MGG_Shader* vertexShader, mgint* strides, mgint streamCount, MGG_InputElement* elements, mgint elementCount)
{
    assert(device != nullptr);
    assert(vertexShader != nullptr);
    assert(vertexShader->stage == MGShaderStage::Vertex);
    assert(strides != nullptr);
    assert(streamCount >= 0);
    assert(elements != nullptr);
    assert(elementCount >= 0);

    MGG_InputLayout* layout = new MGG_InputLayout();
    layout->strides.resize(streamCount);
    for (mgint streamIndex = 0; streamIndex < streamCount; ++streamIndex)
        layout->strides[streamIndex] = strides[streamIndex];

    layout->bindings.reserve(elementCount);

    for (mgint elementIndex = 0; elementIndex < elementCount; ++elementIndex)
    {
        MGG_InputLayout::Binding binding;
        binding.attributeIndex = static_cast<GLuint>(elementIndex);
        binding.vertexBufferSlot = elements[elementIndex].VertexBufferSlot;
        binding.alignedByteOffset = elements[elementIndex].AlignedByteOffset;
        binding.instanceDataStepRate = elements[elementIndex].InstanceDataStepRate;
        ToVertexAttribFormat(elements[elementIndex].Format, binding.elementCount, binding.elementType, binding.normalized);

        layout->bindings.push_back(binding);
    }

    return layout;
}

void MGG_InputLayout_Destroy(MGG_GraphicsDevice* device, MGG_InputLayout* layout)
{
    assert(device != nullptr);

    if (device->inputLayout == layout)
        device->inputLayout = nullptr;

    delete layout;
}

MGG_Shader* MGG_Shader_Create(MGG_GraphicsDevice* device, MGShaderStage stage, mgbyte* bytecode, mgint sizeInBytes)
{
    assert(device != nullptr);
    assert(bytecode != nullptr);
    assert(sizeInBytes > 0);

    EnsureContext(device);

    GLenum shaderType = GL_VERTEX_SHADER;
    switch (stage)
    {
    case MGShaderStage::Vertex:
        shaderType = GL_VERTEX_SHADER;
        break;
    case MGShaderStage::Pixel:
        shaderType = GL_FRAGMENT_SHADER;
        break;
    default:
        MGGL_FAIL("Unsupported shader stage", "native OpenGL shader creation only supports vertex and pixel stages");
    }

    MGG_Shader* shader = new MGG_Shader();
    shader->stage = stage;
    shader->source.assign(reinterpret_cast<const char*>(bytecode), sizeInBytes);
    shader->attributeCount = CountSequentialShaderInputs(shader->source);

    if (shader->source.find(GetConstantBufferName(stage, GL_BOOL)) != std::string::npos)
        shader->constantBufferTypes.push_back(GL_BOOL);

    if (shader->source.find(GetConstantBufferName(stage, GL_INT)) != std::string::npos)
        shader->constantBufferTypes.push_back(GL_INT);

    if (shader->source.find(GetConstantBufferName(stage, GL_FLOAT)) != std::string::npos)
        shader->constantBufferTypes.push_back(GL_FLOAT);

    shader->handle = device->context.functions.CreateShader(shaderType);
    if (shader->handle == 0)
        MGGL_FAIL("glCreateShader failed", "shader creation returned 0");

    const GLchar* source = shader->source.c_str();
    GLint sourceLength = static_cast<GLint>(shader->source.size());
    device->context.functions.ShaderSource(shader->handle, 1, &source, &sourceLength);
    device->context.functions.CompileShader(shader->handle);

    GLint compiled = GL_FALSE;
    device->context.functions.GetShaderiv(shader->handle, GL_COMPILE_STATUS, &compiled);
    if (compiled != GL_TRUE)
    {
        char infoLog[2048] = {};
        device->context.functions.GetShaderInfoLog(
            shader->handle,
            static_cast<GLsizei>(sizeof(infoLog)),
            nullptr,
            infoLog);
        device->context.functions.DeleteShader(shader->handle);
        delete shader;
        MGGL_FAIL("OpenGL shader compile failed", infoLog[0] != '\0' ? infoLog : "shader compilation failed without an info log");
    }

    return shader;
}

void MGG_Shader_Destroy(MGG_GraphicsDevice* device, MGG_Shader* shader)
{
    assert(device != nullptr);

    if (shader == nullptr)
        return;

    EnsureContext(device);

    size_t stageIndex = ToStageIndex(shader->stage);
    if (device->shaders[stageIndex] == shader)
        device->shaders[stageIndex] = nullptr;

    for (size_t i = 0; i < device->programs.size();)
    {
        MGG_ShaderProgram* program = device->programs[i];
        if (program->vertexShader == shader || program->pixelShader == shader)
        {
            DestroyProgram(device, program);
            device->programs.erase(device->programs.begin() + static_cast<ptrdiff_t>(i));
            continue;
        }

        ++i;
    }

    if (shader->handle != 0)
        device->context.functions.DeleteShader(shader->handle);

    delete shader;
}

MGG_OcclusionQuery* MGG_OcclusionQuery_Create(MGG_GraphicsDevice* device)
{
    assert(device != nullptr);

    EnsureContext(device);

    MGG_OcclusionQuery* query = new MGG_OcclusionQuery();
    device->context.functions.GenQueries(1, &query->handle);
    if (query->handle == 0)
        MGGL_FAIL("glGenQueries failed", "occlusion query creation returned 0");

    return query;
}

void MGG_OcclusionQuery_Destroy(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query)
{
    assert(device != nullptr);

    if (query == nullptr)
        return;

    EnsureContext(device);

    if (query->handle != 0)
        device->context.functions.DeleteQueries(1, &query->handle);

    delete query;
}

void MGG_OcclusionQuery_Begin(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query)
{
    assert(device != nullptr);
    assert(query != nullptr);

    EnsureContext(device);
    device->context.functions.BeginQuery(GL_SAMPLES_PASSED, query->handle);
}

void MGG_OcclusionQuery_End(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query)
{
    assert(device != nullptr);
    assert(query != nullptr);

    EnsureContext(device);
    device->context.functions.EndQuery(GL_SAMPLES_PASSED);
}

mgbyte MGG_OcclusionQuery_GetResult(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query, mgint& pixelCount)
{
    assert(device != nullptr);
    assert(query != nullptr);

    EnsureContext(device);

    GLuint resultAvailable = 0;
    device->context.functions.GetQueryObjectuiv(query->handle, GL_QUERY_RESULT_AVAILABLE, &resultAvailable);
    if (resultAvailable == 0)
    {
        pixelCount = 0;
        return false;
    }

    GLuint result = 0;
    device->context.functions.GetQueryObjectuiv(query->handle, GL_QUERY_RESULT, &result);
    pixelCount = static_cast<mgint>(result);
    return true;
}
