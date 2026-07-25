#include "OpenGLContext.h"

#include <SDL_opengl.h>

namespace
{
    [[noreturn]] void MGGL_Fail(const char* file, int line, const char* action, const char* detail)
    {
        char message[512];
        snprintf(message, sizeof(message), "%s: %s", action, detail);
        MG_Print_StdError(file, line, message);
        MG_GENERATE_TRAP();
        abort();
    }

#define MGGL_FAIL(action, detail) MGGL_Fail(__FILE__, __LINE__, action, detail)

    [[noreturn]] void FailWithSdlError(const char* file, int line, const char* action)
    {
        const char* error = SDL_GetError();
        MGGL_Fail(file, line, action, (error != nullptr && error[0] != '\0') ? error : "unknown SDL error");
    }

#define MGGL_FAIL_SDL(action) FailWithSdlError(__FILE__, __LINE__, action)

    void* LoadProcAddress(const char* name)
    {
        assert(name != nullptr);

        void* address = SDL_GL_GetProcAddress(name);
        if (address == nullptr)
        {
            char message[256];
            snprintf(message, sizeof(message), "required OpenGL procedure is unavailable: %s", name);
            MGGL_FAIL("SDL_GL_GetProcAddress failed", message);
        }

        return address;
    }

    void QueryVersion(mgint& majorVersion, mgint& minorVersion)
    {
        GLint major = 0;
        GLint minor = 0;
        glGetIntegerv(GL_MAJOR_VERSION, &major);
        glGetIntegerv(GL_MINOR_VERSION, &minor);

        if (major <= 0)
        {
            SDL_GL_GetAttribute(SDL_GL_CONTEXT_MAJOR_VERSION, &major);
            SDL_GL_GetAttribute(SDL_GL_CONTEXT_MINOR_VERSION, &minor);
        }

        majorVersion = major;
        minorVersion = minor;
    }
}

void OpenGLFunctions::Load()
{
    ActiveTexture = reinterpret_cast<PFNGLACTIVETEXTUREPROC>(LoadProcAddress("glActiveTexture"));
    BindBuffer = reinterpret_cast<PFNGLBINDBUFFERPROC>(LoadProcAddress("glBindBuffer"));
    BindSampler = reinterpret_cast<PFNGLBINDSAMPLERPROC>(LoadProcAddress("glBindSampler"));
    BufferData = reinterpret_cast<PFNGLBUFFERDATAPROC>(LoadProcAddress("glBufferData"));
    BufferSubData = reinterpret_cast<PFNGLBUFFERSUBDATAPROC>(LoadProcAddress("glBufferSubData"));
    DeleteBuffers = reinterpret_cast<PFNGLDELETEBUFFERSPROC>(LoadProcAddress("glDeleteBuffers"));
    DeleteSamplers = reinterpret_cast<PFNGLDELETESAMPLERSPROC>(LoadProcAddress("glDeleteSamplers"));
    GenBuffers = reinterpret_cast<PFNGLGENBUFFERSPROC>(LoadProcAddress("glGenBuffers"));
    GenSamplers = reinterpret_cast<PFNGLGENSAMPLERSPROC>(LoadProcAddress("glGenSamplers"));
    BindVertexArray = reinterpret_cast<PFNGLBINDVERTEXARRAYPROC>(LoadProcAddress("glBindVertexArray"));
    DeleteVertexArrays = reinterpret_cast<PFNGLDELETEVERTEXARRAYSPROC>(LoadProcAddress("glDeleteVertexArrays"));
    GenVertexArrays = reinterpret_cast<PFNGLGENVERTEXARRAYSPROC>(LoadProcAddress("glGenVertexArrays"));
    DrawElementsBaseVertex = reinterpret_cast<PFNGLDRAWELEMENTSBASEVERTEXPROC>(LoadProcAddress("glDrawElementsBaseVertex"));
    SamplerParameterf = reinterpret_cast<PFNGLSAMPLERPARAMETERFPROC>(LoadProcAddress("glSamplerParameterf"));
    SamplerParameterfv = reinterpret_cast<PFNGLSAMPLERPARAMETERFVPROC>(LoadProcAddress("glSamplerParameterfv"));
    SamplerParameteri = reinterpret_cast<PFNGLSAMPLERPARAMETERIPROC>(LoadProcAddress("glSamplerParameteri"));
}

void OpenGLContext::Create(SDL_Window* nextWindow)
{
    assert(nextWindow != nullptr);

    if (handle != nullptr && window == nextWindow)
        return;

    if (handle != nullptr)
        Destroy();

    handle = SDL_GL_CreateContext(nextWindow);
    if (handle == nullptr)
        MGGL_FAIL_SDL("SDL_GL_CreateContext failed");

    window = nextWindow;
    MakeCurrent();
    functions.Load();

    QueryVersion(majorVersion, minorVersion);
    if (majorVersion < 4 || (majorVersion == 4 && minorVersion < 1))
        MGGL_FAIL("OpenGL 4.1 core context required", "created context is below 4.1");

    functions.GenVertexArrays(1, &defaultVertexArray);
    if (defaultVertexArray == 0)
        MGGL_FAIL("glGenVertexArrays failed", "default vertex array creation returned 0");

    functions.BindVertexArray(defaultVertexArray);
}

void OpenGLContext::Destroy()
{
    if (handle != nullptr && defaultVertexArray != 0)
    {
        MakeCurrent();
        functions.DeleteVertexArrays(1, &defaultVertexArray);
    }

    if (handle != nullptr)
        SDL_GL_DeleteContext(handle);

    window = nullptr;
    handle = nullptr;
    defaultVertexArray = 0;
    width = 0;
    height = -0;
    syncInterval = 0;
    majorVersion = 0;
    minorVersion = 0;
}

void OpenGLContext::MakeCurrent()
{
    assert(window != nullptr);
    assert(handle != nullptr);

    if (SDL_GL_MakeCurrent(window, handle) < 0)
        MGGL_FAIL_SDL("SDL_GL_MakeCurrent failed");
}

void OpenGLContext::SetSwapInterval(mgint nextSyncInterval)
{
    assert(nextSyncInterval >= 0);

    if (syncInterval == nextSyncInterval)
        return;

    MakeCurrent();

    if (SDL_GL_SetSwapInterval(nextSyncInterval > 0 ? 1 : 0) < 0)
        MGGL_FAIL_SDL("SDL_GL_SetSwapInterval failed");

    syncInterval = nextSyncInterval;
}

void OpenGLContext::BindDefaultVertexArray()
{
    assert(defaultVertexArray != 0);
    functions.BindVertexArray(defaultVertexArray);
}
