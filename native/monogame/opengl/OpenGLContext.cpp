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

    QueryVersion(majorVersion, minorVersion);
    if (majorVersion < 4 || (majorVersion == 4 && minorVersion < 1))
        MGGL_FAIL("OpenGL 4.1 core context required", "created context is below 4.1");
}

void OpenGLContext::Destroy()
{
    if (handle != nullptr)
        SDL_GL_DeleteContext(handle);

    window = nullptr;
    handle = nullptr;
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
