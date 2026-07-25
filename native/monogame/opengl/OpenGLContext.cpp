#include "OpenGLContext.h"

namespace
{
    [[noreturn]] void FailNotImplemented(const char* file, int line, const char* functionName)
    {
        char message[256];
        snprintf(message, sizeof(message), "%s is not implemented", functionName);
        MG_Print_StdError(file, line, message);
        MG_GENERATE_TRAP();
        abort();
    }

#define MGGL_NOT_IMPLEMENTED(functionName) FailNotImplemented(__FILE__, __LINE__, functionName)
}

void OpenGLContext::Create(SDL_Window* nextWindow)
{
    (void)nextWindow;
    MGGL_NOT_IMPLEMENTED("OpenGLContext::Create");
}

void OpenGLContext::Destroy()
{
    MGGL_NOT_IMPLEMENTED("OpenGLContext::Destroy");
}

void OpenGLContext::MakeCurrent()
{
    MGGL_NOT_IMPLEMENTED("OpenGLContext::MakeCurrent");
}

void OpenGLContext::SetSwapInterval(mgint nextSyncInterval)
{
    (void)nextSyncInterval;
    MGGL_NOT_IMPLEMENTED("OpenGLContext::SetSwapInterval");
}
