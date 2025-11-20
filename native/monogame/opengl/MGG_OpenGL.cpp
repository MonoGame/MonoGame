// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "api_MGG.h"
#include "mg_common.h"

// Effect includes for OpenGL
// #include "AlphaTestEffect.ogl.mgfxo.h"
// #include "BasicEffect.ogl.mgfxo.h"
// #include "DualTextureEffect.ogl.mgfxo.h"
// #include "EnvironmentMapEffect.ogl.mgfxo.h"
// #include "SkinnedEffect.ogl.mgfxo.h"
// #include "SpriteEffect.ogl.mgfxo.h"
//#include "mg_effect.h"

// Include required headers for OpenGL/Emscripten
#if defined(MG_EMSCRIPTEN)
#include <emscripten.h>
#include <emscripten/html5.h>
// Include OpenGLES 3.0 headers
#include <GLES3/gl3.h>
#else
// #if defined(__APPLE__)
// #include <OpenGL/gl.h>
// #include <OpenGL/glext.h>
// #else
// #include <GL/gl.h>
// #include <GL/glext.h>
// #endif
#endif

#if defined(MG_SDL2)
#define GL_GLEXT_PROTOTYPES 1
#include <SDL.h>
#include <SDL_opengl.h>
#include <SDL_opengl_glext.h>
#endif

#include <vector>
#include <string>
#include <cstring>

// Debug macros
#ifdef DEBUG
void GL_CHECK_ERROR() { \
    GLenum err = glGetError(); \
    if (err != GL_NO_ERROR) { \
        fprintf(stderr, "OpenGL error %d at %s:%d\n", err, __FILE__, __LINE__); \
    } \
}
#else
#define GL_CHECK_ERROR() ((void)0)
#endif
// Structures for OpenGL graphics system

struct MGG_GraphicsAdapter {
    // OpenGL specific adapter info
    char name[128] = { 0 };
    char vendor[128] = { 0 };
    char version[128] = { 0 };
};

struct MGG_GraphicsSystem {
    std::vector<MGG_GraphicsAdapter*> adapters;
};

struct MGG_GraphicsDevice
{
    MGG_GraphicsSystem* system = nullptr;
    MGG_GraphicsAdapter* adapter = nullptr;
    
#if defined(MG_EMSCRIPTEN)
    EMSCRIPTEN_WEBGL_CONTEXT_HANDLE context = 0;
#else
    SDL_GLContext context = nullptr;
    SDL_Window* window = nullptr;
#endif

    // Viewport
    int viewportX = 0;
    int viewportY = 0;
    int viewportWidth = 0;
    int viewportHeight = 0;
    float viewportMinDepth = 0.0f;
    float viewportMaxDepth = 1.0f;
    
    // Scissor rect
    int scissorX = 0;
    int scissorY = 0;
    int scissorWidth = 0;
    int scissorHeight = 0;
};

struct MGG_Buffer {
    MGBufferType type;
    int sizeInBytes;
    std::string name;  // Optional name
};

struct MGG_Texture {
    MGTextureType type;
    MGSurfaceFormat format;
    int width;
    int height;
    int depth;
    int mipmaps;
    int slices;
    bool isRenderTarget;
    MGDepthFormat depthFormat;
    GLuint texture;
};

struct MGG_SamplerState {
    MGG_SamplerState_Info info;
    GLuint sampler;
};

struct MGG_BlendState {
    MGG_BlendState_Info info;
    GLuint state;
};

struct MGG_DepthStencilState {
    MGG_DepthStencilState_Info info;
    GLuint state;
};

struct MGG_RasterizerState {
    MGG_RasterizerState_Info info;
    GLuint state;
};

struct MGG_Shader {
    MGShaderStage stage;
    std::vector<uint8_t> bytecode;
    GLuint shader;
};

struct MGG_InputLayout {
    std::vector<MGG_InputElement> elements;
    std::vector<int> strides;
};

struct MGG_OcclusionQuery {
    bool isActive;
};

// Implementation of API functions

MGG_GraphicsSystem* MGG_GraphicsSystem_Create() {
    printf("Creating OpenGL graphics system\n");
    MGG_GraphicsSystem* system = new MGG_GraphicsSystem();
    
#if defined(MG_EMSCRIPTEN)
    // Create a default adapter
    MGG_GraphicsAdapter* adapter = new MGG_GraphicsAdapter();
    system->adapters.push_back(adapter);
#else
    // Non-Emscripten build - create a dummy adapter for testing
    MGG_GraphicsAdapter* adapter = new MGG_GraphicsAdapter();
    system->adapters.push_back(adapter);
#endif
    return system;
}

void MGG_GraphicsSystem_Destroy(MGG_GraphicsSystem* system) {
    printf("Destroying OpenGL graphics system\n");
    for (auto adapter : system->adapters) {
        delete adapter;
    }
    delete system;
}

MGG_GraphicsAdapter* MGG_GraphicsAdapter_Get(MGG_GraphicsSystem* system, mgint index) {
    printf("Getting OpenGL graphics adapter at index %d\n", index);
    if (!system) return nullptr;
    if (index >= 0 && index < static_cast<mgint>(system->adapters.size())) {
        return system->adapters[index];
    }
    return nullptr;
}

void MGG_GraphicsAdapter_GetInfo(MGG_GraphicsAdapter* adapter, MGG_GraphicsAdaptor_Info& info) {
    assert(adapter);
    printf("Getting info for OpenGL graphics adapter: %s\n", adapter->name);
    // Set adapter properties based on OpenGL capabilities
    // The actual implementation would query these from the OpenGL context
    info.DeviceName = adapter->name;
    info.Description = adapter->vendor;
    info.DeviceId = 0;
    info.Revision = 0;
    info.VendorId = 0;
    info.SubSystemId = 0;
    info.MonitorHandle = nullptr;
    info.DisplayModes = nullptr;
    info.DisplayModeCount = 0;
    // Initialize CurrentDisplayMode with zeros
    info.CurrentDisplayMode = { MGSurfaceFormat::Color, 0, 0 };
}

MGG_GraphicsDevice* MGG_GraphicsDevice_Create(MGG_GraphicsSystem* system, MGG_GraphicsAdapter* adapter) {
    printf("Creating OpenGL graphics device\n");
    MGG_GraphicsDevice* device = new MGG_GraphicsDevice();
    device->system = system;
    device->adapter = adapter;
printf("Creating OpenGL graphics device\n");
#if defined(MG_EMSCRIPTEN)
    // Set up OpenGL context attributes
    EmscriptenWebGLContextAttributes attrs;
    emscripten_webgl_init_context_attributes(&attrs);
    attrs.majorVersion = 2; // OpenGL 2.0 maps to OpenGLES 3.0
    attrs.minorVersion = 0;
    attrs.enableExtensionsByDefault = 1;
    attrs.alpha = 1;
    attrs.depth = 1;
    attrs.stencil = 1;
    attrs.antialias = 1;
    attrs.premultipliedAlpha = 1;
    attrs.preserveDrawingBuffer = 0;
    attrs.powerPreference = EM_WEBGL_POWER_PREFERENCE_DEFAULT;
    attrs.failIfMajorPerformanceCaveat = 0;
    printf("Getting canvas\n");
    
    // Get the canvas element - assuming the default "#canvas" selector
    device->context = emscripten_webgl_create_context("#canvas", &attrs);
    printf("Got canvas %lu\n", device->context);
    if (device->context <= 0) {
        fprintf(stderr, "Failed to create OpenGL context: %lu\n", device->context);
        delete device;
        return nullptr;
    }
     printf("making current\n");
    // Make the context current
    EMSCRIPTEN_RESULT result = emscripten_webgl_make_context_current(device->context);
    if (result != EMSCRIPTEN_RESULT_SUCCESS) {
        fprintf(stderr, "Failed to make OpenGL context current: %d\n", result);
        emscripten_webgl_destroy_context(device->context);
        delete device;
        return nullptr;
    }
#else
    // Set OpenGL attributes for SDL
    SDL_GL_SetAttribute(SDL_GL_CONTEXT_MAJOR_VERSION, 4);
    SDL_GL_SetAttribute(SDL_GL_CONTEXT_MINOR_VERSION, 3);
    SDL_GL_SetAttribute(SDL_GL_CONTEXT_PROFILE_MASK, SDL_GL_CONTEXT_PROFILE_CORE);

    // GLES compatibility
    // SDL_GL_SetAttribute(SDL_GL_CONTEXT_MAJOR_VERSION, 2);
    // SDL_GL_SetAttribute(SDL_GL_CONTEXT_MINOR_VERSION, 0);
    // SDL_GL_SetAttribute(SDL_GL_CONTEXT_PROFILE_MASK, SDL_GL_CONTEXT_PROFILE_ES);
    // dont use this for max compatibility
    //SDL_GL_SetAttribute(SDL_GL_CONTEXT_PROFILE_MASK, SDL_GL_CONTEXT_PROFILE_CORE);
    SDL_GL_SetAttribute(SDL_GL_DOUBLEBUFFER, 1);
    SDL_GL_SetAttribute(SDL_GL_DEPTH_SIZE, 24);
    SDL_GL_SetAttribute(SDL_GL_STENCIL_SIZE, 8);
    SDL_GL_SetAttribute(SDL_GL_ALPHA_SIZE, 8);
    
    // Try to get the current window first
    device->window = SDL_GetWindowFromID(1);

    // Create OpenGL context
    device->context = SDL_GL_CreateContext(device->window);
    // If 4.3 fails, try 4.1 (for macOS)
    if (!device->context) {
        printf("OpenGL 4.3 not available, trying 4.1...\n");
        SDL_GL_SetAttribute(SDL_GL_CONTEXT_MAJOR_VERSION, 4);
        SDL_GL_SetAttribute(SDL_GL_CONTEXT_MINOR_VERSION, 1);
        device->context = SDL_GL_CreateContext(device->window);
    }
    
    if (!device->context) {
        fprintf(stderr, "Failed to create OpenGL context: %s\n", SDL_GetError());
        delete device;
        return nullptr;
    }
    
    // Make the context current
    if (SDL_GL_MakeCurrent(device->window, device->context) < 0) {
        fprintf(stderr, "Failed to make OpenGL context current: %s\n", SDL_GetError());
        SDL_GL_DeleteContext(device->context);
        delete device;
        return nullptr;
    }
#endif
    // Print OpenGL version information
    const GLubyte* version = glGetString(GL_VERSION);
    const GLubyte* vendor = glGetString(GL_VENDOR);
    const GLubyte* renderer = glGetString(GL_RENDERER);
    const GLubyte* glslVersion = glGetString(GL_SHADING_LANGUAGE_VERSION);
    
    printf("=== OpenGL Context Information ===\n");
    printf("OpenGL Version: %s\n", version ? (const char*)version : "Unknown");
    printf("OpenGL Vendor: %s\n", vendor ? (const char*)vendor : "Unknown");
    printf("OpenGL Renderer: %s\n", renderer ? (const char*)renderer : "Unknown");
    printf("GLSL Version: %s\n", glslVersion ? (const char*)glslVersion : "Unknown");
    printf("==================================\n");

    // Initialize default viewport state
    device->viewportX = 0;
    device->viewportY = 0;
    device->viewportWidth = 800;  // Default size
    device->viewportHeight = 600; // Default size
    device->viewportMinDepth = 0.0f;
    device->viewportMaxDepth = 1.0f;
    
    // Initialize default scissor state
    device->scissorX = 0;
    device->scissorY = 0;
    device->scissorWidth = 800;  // Default size
    device->scissorHeight = 600; // Default size
    printf("Created OpenGL graphics device: %lu\n", device->context);
    return device;
}

void MGG_GraphicsDevice_Destroy(MGG_GraphicsDevice* device) {
    if (!device) return;
    printf("Destroying OpenGL graphics device: %zu\n", (size_t)device->context);
#if defined(MG_EMSCRIPTEN)
    // Destroy the OpenGL context
    if (device->context > 0) {
        emscripten_webgl_destroy_context(device->context);
        device->context = 0;
    }
#else
    if (!device->context) {
        // Dummy context - nothing to destroy
        SDL_GL_DeleteContext(device->context);
        device->context = nullptr;
    }
#endif
}

void MGG_GraphicsDevice_GetCaps(MGG_GraphicsDevice* device, MGG_GraphicsDevice_Caps& caps) {
    // Set device capabilities based on OpenGL capabilities
    printf("Getting capabilities for OpenGL graphics device: %zu\n", (size_t)device->context);
#if defined(MG_EMSCRIPTEN)
    caps.MaxTextureSlots = 16;
    caps.MaxVertexBufferSlots = 8;
    caps.MaxVertexTextureSlots = 8;
#else
    glGetIntegerv(GL_MAX_TEXTURE_IMAGE_UNITS, &caps.MaxTextureSlots);
    GL_CHECK_ERROR();
    glGetIntegerv(GL_MAX_VERTEX_TEXTURE_IMAGE_UNITS, &caps.MaxVertexTextureSlots);
    GL_CHECK_ERROR();
    glGetIntegerv(GL_MAX_VERTEX_ATTRIBS, &caps.MaxVertexBufferSlots);
    GL_CHECK_ERROR();
#endif
    caps.ShaderProfile = 0; // OpenGL MonoGame Shader Profile
    printf("Device capabilities: MaxTextureSlots=%d, MaxVertexBufferSlots=%d, MaxVertexTextureSlots=%d\n",
           caps.MaxTextureSlots, caps.MaxVertexBufferSlots, caps.MaxVertexTextureSlots);
}

void MGG_GraphicsDevice_ResizeSwapchain(MGG_GraphicsDevice* device, void* nativeWindowHandle, mgint width, mgint height, MGSurfaceFormat color, MGDepthFormat depth, mgint syncInterval) {
    if (!device) return;
    printf("Resizing OpenGL graphics device: %zu (width=%d, height=%d)\n", (size_t)device->context, width, height);
#if defined(MG_EMSCRIPTEN)
    // In WebGL, we don't need to explicitly resize the swapchain
    // The browser handles canvas resizing
    
    // We can use emscripten_set_canvas_element_size to resize the canvas if needed
    if (width > 0 && height > 0) {
        emscripten_set_canvas_element_size("#canvas", width, height);
    }
#else
    // For desktop OpenGL, we need to recreate the framebuffer
    switch (depth)
    {
        case MGDepthFormat::None:
            // No depth buffer needed
            SDL_GL_SetAttribute(SDL_GL_DEPTH_SIZE, 0);
            SDL_GL_SetAttribute(SDL_GL_STENCIL_SIZE, 0);
            SDL_GL_SetAttribute(SDL_GL_ALPHA_SIZE, 8);
            break;
        case MGDepthFormat::Depth16:
            SDL_GL_SetAttribute(SDL_GL_DEPTH_SIZE, 16);
            SDL_GL_SetAttribute(SDL_GL_STENCIL_SIZE, 0);
            SDL_GL_SetAttribute(SDL_GL_ALPHA_SIZE, 8);
            break;
        case MGDepthFormat::Depth24:
            SDL_GL_SetAttribute(SDL_GL_DEPTH_SIZE, 24);
            SDL_GL_SetAttribute(SDL_GL_STENCIL_SIZE, 0);
            SDL_GL_SetAttribute(SDL_GL_ALPHA_SIZE, 8);
            break;
        case MGDepthFormat::Depth24Stencil8:
            SDL_GL_SetAttribute(SDL_GL_DEPTH_SIZE, 24);
            SDL_GL_SetAttribute(SDL_GL_STENCIL_SIZE, 8);
            SDL_GL_SetAttribute(SDL_GL_ALPHA_SIZE, 8);
            break;
        default:
            fprintf(stderr, "Unsupported depth format for OpenGL: %d\n", (int)depth);
            break;
    }
    if (device->context) {
        SDL_GL_DeleteContext(device->context);
        device->context = nullptr;
    }
    device->context = SDL_GL_CreateContext(device->window);
    if (!device->context) {
        fprintf(stderr, "Failed to create OpenGL context: %s\n", SDL_GetError());
        delete device;
        return;
    }
    // all the tetures will be gone now, so we need to recreate them
    
    // Make the context current
    if (SDL_GL_MakeCurrent(device->window, device->context) < 0) {
        fprintf(stderr, "Failed to make OpenGL context current: %s\n", SDL_GetError());
        SDL_GL_DeleteContext(device->context);
        delete device;
        return;
    }
    // For SDL, we can set the window size
    if (device->window) {
        SDL_SetWindowSize(device->window, width, height);
    }
#endif
    
    // Update viewport to match new size
    device->viewportWidth = width;
    device->viewportHeight = height;
    glViewport(device->viewportX, device->viewportY, width, height);
    GL_CHECK_ERROR();
    
    // Update scissor rectangle to match new size
    device->scissorWidth = width;
    device->scissorHeight = height;
    glScissor(device->scissorX, device->scissorY, width, height);
    GL_CHECK_ERROR();
    printf("Resized OpenGL graphics device: %zu\n", (size_t)device->context);
}

mgint MGG_GraphicsDevice_BeginFrame(MGG_GraphicsDevice* device) {
    printf("Beginning frame for OpenGL graphics device\n");
    if (!device) return 0;
    
#if defined(MG_EMSCRIPTEN)
    // Make sure our context is current
    EMSCRIPTEN_RESULT result = emscripten_webgl_make_context_current(device->context);
    if (result != EMSCRIPTEN_RESULT_SUCCESS) {
        fprintf(stderr, "Failed to make OpenGL context current: %d\n", result);
        return 0;
    }
#else
    SDL_GL_MakeCurrent(device->window, device->context);
#endif
    printf("Ending frame for OpenGL graphics device: %zu\n", (size_t)device->context);

    return 1; // Frame index - OpenGL doesn't use multiple frames like Vulkan
}

void MGG_GraphicsDevice_Clear(MGG_GraphicsDevice* device, MGClearOptions options, Vector4& color, mgfloat depth, mgint stencil) {
    printf("Clearing OpenGL graphics device\n");
    if (!device) return;
    
    // Check which framebuffer is currently bound
    GLint currentFramebuffer = 0;
    glGetIntegerv(GL_FRAMEBUFFER_BINDING, &currentFramebuffer);
    printf("Current framebuffer: %d\n", currentFramebuffer);
    
    GLbitfield clearMask = 0;
    
    // Set clear color if color buffer is being cleared
    if (static_cast<mgint>(options) & static_cast<mgint>(MGClearOptions::Target)) {
        glClearColor(color.X, color.Y, color.Z, color.W);
        GL_CHECK_ERROR();
        clearMask |= GL_COLOR_BUFFER_BIT;
        printf("Clear color: (%f, %f, %f, %f)\n", color.X, color.Y, color.Z, color.W);
    }
    
    // Set clear depth if depth buffer is being cleared
    if (static_cast<mgint>(options) & static_cast<mgint>(MGClearOptions::DepthBuffer)) {
#if defined(MG_EMSCRIPTEN)
        // WebGL/OpenGL ES uses glClearDepthf
        glClearDepthf(depth);
#else
        // Desktop OpenGL uses glClearDepth with double parameters
        glClearDepth((double)depth);
#endif
        GL_CHECK_ERROR();
        clearMask |= GL_DEPTH_BUFFER_BIT;
        printf("Clear depth: %f\n", depth);
    }
    
    // Set clear stencil if stencil buffer is being cleared
    if (static_cast<mgint>(options) & static_cast<mgint>(MGClearOptions::Stencil)) {
        glClearStencil(stencil);
        GL_CHECK_ERROR();
        clearMask |= GL_STENCIL_BUFFER_BIT;
        printf("Clear stencil: %d\n", stencil);
    }
    
    // Perform the clear operation
    if (clearMask != 0) {
        printf("Calling glClear with mask: 0x%x\n", clearMask);
        glClear(clearMask);
        GL_CHECK_ERROR();
    }
    
    GL_CHECK_ERROR();

    printf("Cleared OpenGL graphics device\n");
}

void MGG_GraphicsDevice_Present(MGG_GraphicsDevice* device, mgint currentFrame, mgint syncInterval) {
    if (!device) return;
    printf("Presenting OpenGL graphics device: %zu (syncInterval=%d)\n", (size_t)device->context, syncInterval);
   
#if defined(MG_EMSCRIPTEN)
    // In WebGL, the browser handles buffer swapping with requestAnimationFrame
    // We don't need to do anything special here, as opposed to other APIs
    
    // We could potentially use emscripten_set_main_loop_timing to control frame rate
    // But typically the browser's requestAnimationFrame handles this well
#else
    // Ensure we're swapping the correct window
    SDL_Window* currentWindow = SDL_GL_GetCurrentWindow();
    printf("Current SDL window: %p, device window: %p\n", currentWindow, device->window);
    
    if (currentWindow != device->window) {
        fprintf(stderr, "Warning: Current window doesn't match device window!\n");
    }
    
    printf("Calling SDL_GL_SwapWindow...\n");
    SDL_GL_SwapWindow(device->window);
    printf("SDL_GL_SwapWindow completed\n");
#endif
    
    GL_CHECK_ERROR();
    printf("Present completed\n");
}

void MGG_GraphicsDevice_SetBlendState(MGG_GraphicsDevice* device, MGG_BlendState* state, mgfloat factorR, mgfloat factorG, mgfloat factorB, mgfloat factorA) {
    printf("Setting blend state for OpenGL graphics device\n");
    if (!device || !state) return;
    printf("End Set blend state for OpenGL graphics device\n");
}

void MGG_GraphicsDevice_SetDepthStencilState(MGG_GraphicsDevice* device, MGG_DepthStencilState* state) {
    printf("Setting depth stencil state for OpenGL graphics device\n");
    if (!device || !state) return;
    printf("End Set depth stencil state for OpenGL graphics device\n");
}

void MGG_GraphicsDevice_SetRasterizerState(MGG_GraphicsDevice* device, MGG_RasterizerState* state) {
    printf("Setting rasterizer state for OpenGL graphics device\n");
    if (!device || !state) return;
    printf("End Set rasterizer state for OpenGL graphics device\n");
}

void MGG_GraphicsDevice_GetTitleSafeArea(mgint& x, mgint& y, mgint& width, mgint& height) {
    printf("Getting title safe area for OpenGL graphics device\n");
#if defined(MG_EMSCRIPTEN)
    // In WebGL, the entire canvas is considered safe
    // So we'll just return default values
    double cssWidth, cssHeight;
    emscripten_get_element_css_size("#canvas", &cssWidth, &cssHeight);
    
    x = 0;
    y = 0;
    width = static_cast<mgint>(cssWidth);
    height = static_cast<mgint>(cssHeight);
#else
    // Default values
    x = 0;
    y = 0;
    width = 0; 
    height = 0;
#endif
    printf("Got title safe area for OpenGL graphics device: (%d, %d, %d, %d)\n", x, y, width, height);
}

void MGG_GraphicsDevice_SetViewport(MGG_GraphicsDevice* device, mgint x, mgint y, mgint width, mgint height, mgfloat minDepth, mgfloat maxDepth) {
    printf("Setting viewport for OpenGL graphics device: %zu (%d, %d, %d, %d)\n", (size_t)device->context, x, y, width, height);
    if (!device) return;
    
    // Store viewport values
    device->viewportX = x;
    device->viewportY = y;
    device->viewportWidth = width;
    device->viewportHeight = height;
    device->viewportMinDepth = minDepth;
    device->viewportMaxDepth = maxDepth;
    
    // Set the viewport in OpenGLES
    glViewport(x, y, width, height);
    GL_CHECK_ERROR();
    
    // Note: OpenGL depth range is [0,1], but Direct3D is [-1,1]
    // minDepth and maxDepth are in the Direct3D range, so we need to map them
#if defined(MG_EMSCRIPTEN)
    // WebGL/OpenGL ES uses glDepthRangef
    glDepthRangef(minDepth, maxDepth);
#else
    // Desktop OpenGL uses glDepthRange with double parameters
    glDepthRange((double)minDepth, (double)maxDepth);
#endif
    GL_CHECK_ERROR();
}

void MGG_GraphicsDevice_SetScissorRectangle(MGG_GraphicsDevice* device, mgint x, mgint y, mgint width, mgint height) {
    if (!device) return;
    printf("Setting scissor rectangle for OpenGL graphics device: %zu (%d, %d, %d, %d)\n", (size_t)device->context, x, y, width, height);
    // Store scissor values
    device->scissorX = x;
    device->scissorY = y;
    device->scissorWidth = width;
    device->scissorHeight = height;
    
    // Enable scissor test
    glEnable(GL_SCISSOR_TEST);
    GL_CHECK_ERROR();
    
    // Set scissor rectangle
    // Note: OpenGL has (0,0) at bottom-left, need to flip Y coordinate
    int flippedY = device->viewportHeight - (y + height);
    glScissor(x, flippedY, width, height);
    
    GL_CHECK_ERROR();
}

void MGG_GraphicsDevice_SetRenderTargets(MGG_GraphicsDevice* device, MGG_Texture** targets, mgint* arraySlices, mgint count) {
    if (!device) return;
    printf("Setting render targets for OpenGL graphics device: %zu (count=%d)\n", (size_t)device->context, count);
    printf("Ending SetRenderTargets for OpenGL graphics device: %zu\n", (size_t)device->context);
}

void MGG_GraphicsDevice_SetConstantBuffer(MGG_GraphicsDevice* device, MGShaderStage stage, mgint slot, MGG_Buffer* buffer) {
    if (!device) return;
    printf("Setting constant buffer for OpenGL graphics device: %zu (stage=%d, slot=%d)\n", (size_t)device->context, stage, slot);
    printf("Ending SetConstantBuffer for OpenGL graphics device: %zu\n", (size_t)device->context);
}

void MGG_GraphicsDevice_SetTexture(MGG_GraphicsDevice* device, MGShaderStage stage, mgint slot, MGG_Texture* texture) {
    if (!device) return;
    printf("Setting texture for OpenGL graphics device: %zu (stage=%d, slot=%d)\n", (size_t)device->context, stage, slot);
    printf("Ending SetTexture for OpenGL graphics device: %zu\n", (size_t)device->context);
}

void MGG_GraphicsDevice_SetSamplerState(MGG_GraphicsDevice* device, MGShaderStage stage, mgint slot, MGG_SamplerState* state) {
    if (!device) return;
    printf("Setting sampler state for OpenGL graphics device: %zu (stage=%d, slot=%d)\n", (size_t)device->context, stage, slot);
    
    if (!state) return;
    printf("Ending SetSamplerState for OpenGL graphics device: %zu\n", (size_t)device->context);
}

void MGG_GraphicsDevice_SetIndexBuffer(MGG_GraphicsDevice* device, MGIndexElementSize size, MGG_Buffer* buffer) {
    if (!device || !buffer) return;
    printf("Setting index buffer for OpenGL graphics device: %zu (size=%d)\n", (size_t)device->context, size);
    printf("Ending SetIndexBuffer for OpenGL graphics device: %zu\n", (size_t)device->context);
}

void MGG_GraphicsDevice_SetVertexBuffer(MGG_GraphicsDevice* device, mgint slot, MGG_Buffer* buffer, mgint vertexOffset) {
    if (!device || !buffer) return;
    printf("Setting vertex buffer for OpenGL graphics device: %zu (slot=%d, offset=%d)\n", (size_t)device->context, slot, vertexOffset);
    printf("Ending SetVertexBuffer for OpenGL graphics device: %zu\n", (size_t)device->context);
}

void MGG_GraphicsDevice_SetShader(MGG_GraphicsDevice* device, MGShaderStage stage, MGG_Shader* shader) {
    assert(device != nullptr);
	assert(shader != nullptr);
	assert(shader->stage == stage);

    printf("Start Set shader for OpenGL graphics device: %zu (stage=%d)\n", (size_t)device->context, stage);
    printf("End Set shader for OpenGL graphics device: %zu\n", (size_t)device->context);
}

void MGG_GraphicsDevice_SetInputLayout(MGG_GraphicsDevice* device, MGG_InputLayout* layout) {
    if (!device) return;
    printf("Setting input layout for OpenGL graphics device: %zu\n", (size_t)device->context);
    printf("Ending SetInputLayout for OpenGL graphics device: %zu\n", (size_t)device->context);
}

void MGG_GraphicsDevice_Draw(MGG_GraphicsDevice* device, MGPrimitiveType primitiveType, mgint vertexStart, mgint vertexCount) {
    if (!device || vertexCount <= 0) return;
    printf("Drawing OpenGL graphics device: %zu (primitiveType=%d, vertexStart=%d, vertexCount=%d)\n", (size_t)device->context, primitiveType, vertexStart, vertexCount);
    printf("Ending Draw for OpenGL graphics device: %zu\n", (size_t)device->context);
}

void MGG_GraphicsDevice_DrawIndexed(MGG_GraphicsDevice* device, MGPrimitiveType primitiveType, mgint primitiveCount, mgint indexStart, mgint vertexStart) {
    if (!device || primitiveCount <= 0) return;
    printf("Drawing indexed OpenGL graphics device: %zu (primitiveType=%d, primitiveCount=%d, indexStart=%d, vertexStart=%d)\n", (size_t)device->context, primitiveType, primitiveCount, indexStart, vertexStart);
    printf("Ending DrawIndexed for OpenGL graphics device: %zu\n", (size_t)device->context);
}

void MGG_GraphicsDevice_DrawIndexedInstanced(MGG_GraphicsDevice* device, MGPrimitiveType primitiveType, mgint primitiveCount, mgint indexStart, mgint vertexStart, mgint instanceCount) {
    if (!device || primitiveCount <= 0 || instanceCount <= 0) return;
    printf("Drawing instanced indexed OpenGL graphics device: %zu (primitiveType=%d, primitiveCount=%d, indexStart=%d, vertexStart=%d, instanceCount=%d)\n", (size_t)device->context, primitiveType, primitiveCount, indexStart, vertexStart, instanceCount);
    printf("Ending DrawIndexedInstanced for OpenGL graphics device: %zu\n", (size_t)device->context);
}

void MGG_GraphicsDevice_ResolveRenderTargets(MGG_GraphicsDevice* device) {
    if (!device) return;
    printf("Resolving render targets for OpenGL graphics device: %zu\n", (size_t)device->context);
    printf("Ending ResolveRenderTargets for OpenGL graphics device: %zu\n", (size_t)device->context);
}

void MGG_GraphicsDevice_GetBackBufferData(MGG_GraphicsDevice* device, mgint x, mgint y, mgint width, mgint height, void* data, mgint count, mgint dataBytes) {
    if (!device || !data) return;
    printf("Getting back buffer data for OpenGL graphics device: %zu (x=%d, y=%d, width=%d, height=%d, count=%d, dataBytes=%d)\n", (size_t)device->context, x, y, width, height, count, dataBytes);
    printf("Ending GetBackBufferData for OpenGL graphics device: %zu\n", (size_t)device->context);
}

MGG_BlendState* MGG_BlendState_Create(MGG_GraphicsDevice* device, MGG_BlendState_Info* info) {
    printf("Creating blend state for OpenGL graphics device: %zu\n", (size_t)device->context);
    MGG_BlendState* state = new MGG_BlendState();
    state->info = *info;
    
    // OpenGL doesn't have explicit state objects like Direct3D
    // Instead, we just store the state information and apply it when needed
    state->state = 0; // Not used in WebGL
    
    return state;
}

void MGG_BlendState_Destroy(MGG_GraphicsDevice* device, MGG_BlendState* state) {
    delete state;
}

MGG_DepthStencilState* MGG_DepthStencilState_Create(MGG_GraphicsDevice* device, MGG_DepthStencilState_Info* info) {
    MGG_DepthStencilState* state = new MGG_DepthStencilState();
    state->info = *info;
    state->state = 0; // Not used in WebGL
    return state;
}

void MGG_DepthStencilState_Destroy(MGG_GraphicsDevice* device, MGG_DepthStencilState* state) {
    // No OpenGL resources to clean up
    delete state;
}

MGG_RasterizerState* MGG_RasterizerState_Create(MGG_GraphicsDevice* device, MGG_RasterizerState_Info* info) {
    MGG_RasterizerState* state = new MGG_RasterizerState();
    state->info = *info;
    state->state = 0; // Not used in WebGL
    return state;
}

void MGG_RasterizerState_Destroy(MGG_GraphicsDevice* device, MGG_RasterizerState* state) {
    // No OpenGL resources to clean up
    delete state;
}

MGG_SamplerState* MGG_SamplerState_Create(MGG_GraphicsDevice* device, MGG_SamplerState_Info* info) {
    MGG_SamplerState* state = new MGG_SamplerState();
    state->info = *info;
    state->sampler = 0; // Not used in WebGL
    return state;
}

void MGG_SamplerState_Destroy(MGG_GraphicsDevice* device, MGG_SamplerState* state) {
    // No OpenGL resources to clean up
    delete state;
}

MGG_Buffer* MGG_Buffer_Create(MGG_GraphicsDevice* device, MGBufferType type, mgint sizeInBytes) {
    printf("Creating buffer for OpenGL graphics device: %zu (type=%d, size=%d)\n", (size_t)device->context, type, sizeInBytes);
    if (!device || sizeInBytes <= 0) return nullptr;
    
    MGG_Buffer* buffer = new MGG_Buffer();
    printf("Created buffer for OpenGL graphics device: %zu (type=%d, size=%d)\n", (size_t)device->context, type, sizeInBytes);
    return buffer;
}

void MGG_Buffer_Destroy(MGG_GraphicsDevice* device, MGG_Buffer* buffer) {
    printf("Destroying buffer for OpenGL graphics device: %zu\n", (size_t)device->context);
    if (!device || !buffer) return;
    delete buffer;
}

void MGG_Buffer_SetData(MGG_GraphicsDevice* device, MGG_Buffer*& buffer, mgint offset, mgbyte* data, mgint elementCount, mgint vertexStride, mgint elementSizeInBytes, mgbool discard) {
    printf("Setting buffer data for OpenGL graphics device: %zu\n", (size_t)device->context);
    if (!device || !buffer || !data) return;
    printf("Ended Set buffer data for OpenGL graphics device: %zu\n", (size_t)device->context);
}

void MGG_Buffer_GetData(MGG_GraphicsDevice* device, MGG_Buffer* buffer, mgint offset, mgbyte* data, mgint dataCount, mgint dataBytes, mgint dataStride) {
    if (!device || !buffer || !data) return;
    printf("Getting buffer data for OpenGL graphics device: %zu\n", (size_t)device->context);
    printf("Ended Get buffer data for OpenGL graphics device: %zu\n", (size_t)device->context);
}

MGG_Texture* MGG_Texture_Create(MGG_GraphicsDevice* device, MGTextureType type, MGSurfaceFormat format, mgint width, mgint height, mgint depth, mgint mipmaps, mgint slices) {
    if (!device || width <= 0 || height <= 0) return nullptr;
    printf("Creating texture: %d x %d x %d\n", width, height, depth);

    MGG_Texture* texture = new MGG_Texture();
    printf("Created texture for OpenGL graphics device: %zu\n", (size_t)device->context);
    return texture;
}

MGG_Texture* MGG_RenderTarget_Create(MGG_GraphicsDevice* device, MGTextureType type, MGSurfaceFormat format, mgint width, mgint height, mgint depth, mgint mipmaps, mgint slices, MGDepthFormat depthFormat, mgint multiSampleCount, MGRenderTargetUsage usage) {
    if (!device || width <= 0 || height <= 0) return nullptr;
    printf("Creating render target: %d x %d x %d\n", width, height, depth);
    MGG_Texture* texture = new MGG_Texture();
    printf("Created render target for OpenGL graphics device: %zu\n", (size_t)device->context);
    return texture;
}

void MGG_Texture_Destroy(MGG_GraphicsDevice* device, MGG_Texture* texture) {
    printf("Destroying texture for OpenGL graphics device: %zu\n", (size_t)device->context);
    if (!device || !texture) return;
    delete texture;
}

void MGG_Texture_SetData(MGG_GraphicsDevice* device, MGG_Texture* texture, mgint level, mgint slice, mgint x, mgint y, mgint z, mgint width, mgint height, mgint depth, mgbyte* data, mgint dataBytes) {
    printf("Setting texture data: %zu, %u, %d, %d, %d, %d, %d, %d\n", (size_t)device->context, texture->texture, x, y, z, width, height, depth);
    if (!device || !texture || !data) return;
    printf("Setting texture data: %d x %d x %d\n", width, height, depth);
    printf("Ended Set texture data for OpenGL graphics device: %zu\n", (size_t)device->context);
}

void MGG_Texture_GetData(MGG_GraphicsDevice* device, MGG_Texture* texture, mgint level, mgint slice, mgint x, mgint y, mgint z, mgint width, mgint height, mgint depth, mgbyte* data, mgint dataBytes) {
    if (!device || !texture || !data) return;
    printf("Getting texture data: %zu, %u, %d, %d, %d, %d, %d, %d\n", (size_t)device->context, texture->texture, x, y, z, width, height, depth);
    printf("Ended Get texture data for OpenGL graphics device: %zu\n", (size_t)device->context);
}

MGG_InputLayout* MGG_InputLayout_Create(MGG_GraphicsDevice* device, MGG_Shader* vertexShader, mgint* strides, mgint streamCount, MGG_InputElement* elements, mgint elementCount) {
    printf("Creating input layout for OpenGL graphics device: %zu\n", (size_t)device->context);
    if (!device || !vertexShader || elementCount <= 0) return nullptr;
    
    MGG_InputLayout* layout = new MGG_InputLayout();
    printf("Created input layout for OpenGL graphics device: %zu\n", (size_t)device->context);
    return layout;
}

void MGG_InputLayout_Destroy(MGG_GraphicsDevice* device, MGG_InputLayout* layout) {
    if (!device || !layout) return;
    printf("Destroying input layout for OpenGL graphics device: %zu\n", (size_t)device->context);
    delete layout;
}

MGG_Shader* MGG_Shader_Create(MGG_GraphicsDevice* device, MGShaderStage stage, mgbyte* bytecode, mgint sizeInBytes) {
    printf("Creating shader for OpenGL graphics device: %zu (stage=%d, size=%d)\n", (size_t)device->context, stage, sizeInBytes);
    if (!device || !bytecode || sizeInBytes <= 0) return nullptr;
    
    MGG_Shader* shader = new MGG_Shader();
    printf("Created shader for OpenGL graphics device: %zu\n", (size_t)device->context);
    return shader;
}

void MGG_Shader_Destroy(MGG_GraphicsDevice* device, MGG_Shader* shader) {
    if (!device || !shader) return;
    printf("Destroying shader for OpenGL graphics device: %zu\n", (size_t)device->context);
    delete shader;
}

MGG_OcclusionQuery* MGG_OcclusionQuery_Create(MGG_GraphicsDevice* device) {
    if (!device) return nullptr;
    printf("Creating occlusion query for OpenGL graphics device: %zu\n", (size_t)device->context);
    MGG_OcclusionQuery* query = new MGG_OcclusionQuery();
    printf("Created occlusion query for OpenGL graphics device: %zu\n", (size_t)device->context);
    return query;
}

void MGG_OcclusionQuery_Destroy(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query) {
    if (!device || !query) return;
    printf("Destroying occlusion query for OpenGL graphics device: %zu\n", (size_t)device->context);
    delete query;
}

void MGG_OcclusionQuery_Begin(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query) {
    if (!device || !query) return;
    printf("Beginning occlusion query for OpenGL graphics device: %zu\n", (size_t)device->context);
}

void MGG_OcclusionQuery_End(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query) {
    if (!device || !query || !query->isActive) return;
    printf("Ending occlusion query for OpenGL graphics device: %zu\n", (size_t)device->context);
}

mgbyte MGG_OcclusionQuery_GetResult(MGG_GraphicsDevice* device, MGG_OcclusionQuery* query, mgint& pixelCount) {
    if (!device || !query) {
        pixelCount = 0;
        return false;
    }    
    return true;
}
