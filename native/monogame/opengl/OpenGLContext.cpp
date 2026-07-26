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
    AttachShader = reinterpret_cast<PFNGLATTACHSHADERPROC>(LoadProcAddress("glAttachShader"));
    BindAttribLocation = reinterpret_cast<PFNGLBINDATTRIBLOCATIONPROC>(LoadProcAddress("glBindAttribLocation"));
    BlendColor = reinterpret_cast<PFNGLBLENDCOLORPROC>(LoadProcAddress("glBlendColor"));
    BlendEquationSeparate = reinterpret_cast<PFNGLBLENDEQUATIONSEPARATEPROC>(LoadProcAddress("glBlendEquationSeparate"));
    BlendFuncSeparate = reinterpret_cast<PFNGLBLENDFUNCSEPARATEPROC>(LoadProcAddress("glBlendFuncSeparate"));
    BindBuffer = reinterpret_cast<PFNGLBINDBUFFERPROC>(LoadProcAddress("glBindBuffer"));
    BindFramebuffer = reinterpret_cast<PFNGLBINDFRAMEBUFFERPROC>(LoadProcAddress("glBindFramebuffer"));
    BindRenderbuffer = reinterpret_cast<PFNGLBINDRENDERBUFFERPROC>(LoadProcAddress("glBindRenderbuffer"));
    BindSampler = reinterpret_cast<PFNGLBINDSAMPLERPROC>(LoadProcAddress("glBindSampler"));
    BindVertexArray = reinterpret_cast<PFNGLBINDVERTEXARRAYPROC>(LoadProcAddress("glBindVertexArray"));
    BufferData = reinterpret_cast<PFNGLBUFFERDATAPROC>(LoadProcAddress("glBufferData"));
    BufferSubData = reinterpret_cast<PFNGLBUFFERSUBDATAPROC>(LoadProcAddress("glBufferSubData"));
    CheckFramebufferStatus = reinterpret_cast<PFNGLCHECKFRAMEBUFFERSTATUSPROC>(LoadProcAddress("glCheckFramebufferStatus"));
    CompileShader = reinterpret_cast<PFNGLCOMPILESHADERPROC>(LoadProcAddress("glCompileShader"));
    CreateProgram = reinterpret_cast<PFNGLCREATEPROGRAMPROC>(LoadProcAddress("glCreateProgram"));
    CreateShader = reinterpret_cast<PFNGLCREATESHADERPROC>(LoadProcAddress("glCreateShader"));
    DeleteBuffers = reinterpret_cast<PFNGLDELETEBUFFERSPROC>(LoadProcAddress("glDeleteBuffers"));
    DeleteFramebuffers = reinterpret_cast<PFNGLDELETEFRAMEBUFFERSPROC>(LoadProcAddress("glDeleteFramebuffers"));
    DeleteProgram = reinterpret_cast<PFNGLDELETEPROGRAMPROC>(LoadProcAddress("glDeleteProgram"));
    DeleteRenderbuffers = reinterpret_cast<PFNGLDELETERENDERBUFFERSPROC>(LoadProcAddress("glDeleteRenderbuffers"));
    DeleteSamplers = reinterpret_cast<PFNGLDELETESAMPLERSPROC>(LoadProcAddress("glDeleteSamplers"));
    DeleteShader = reinterpret_cast<PFNGLDELETESHADERPROC>(LoadProcAddress("glDeleteShader"));
    DeleteVertexArrays = reinterpret_cast<PFNGLDELETEVERTEXARRAYSPROC>(LoadProcAddress("glDeleteVertexArrays"));
    DetachShader = reinterpret_cast<PFNGLDETACHSHADERPROC>(LoadProcAddress("glDetachShader"));
    DisableVertexAttribArray = reinterpret_cast<PFNGLDISABLEVERTEXATTRIBARRAYPROC>(LoadProcAddress("glDisableVertexAttribArray"));
    EnableVertexAttribArray = reinterpret_cast<PFNGLENABLEVERTEXATTRIBARRAYPROC>(LoadProcAddress("glEnableVertexAttribArray"));
    GetTexImage = reinterpret_cast<MGGLGETTEXIMAGEPROC>(LoadProcAddress("glGetTexImage"));
    FramebufferRenderbuffer = reinterpret_cast<PFNGLFRAMEBUFFERRENDERBUFFERPROC>(LoadProcAddress("glFramebufferRenderbuffer"));
    FramebufferTexture2D = reinterpret_cast<PFNGLFRAMEBUFFERTEXTURE2DPROC>(LoadProcAddress("glFramebufferTexture2D"));
    GenBuffers = reinterpret_cast<PFNGLGENBUFFERSPROC>(LoadProcAddress("glGenBuffers"));
    GenFramebuffers = reinterpret_cast<PFNGLGENFRAMEBUFFERSPROC>(LoadProcAddress("glGenFramebuffers"));
    GenRenderbuffers = reinterpret_cast<PFNGLGENRENDERBUFFERSPROC>(LoadProcAddress("glGenRenderbuffers"));
    GenSamplers = reinterpret_cast<PFNGLGENSAMPLERSPROC>(LoadProcAddress("glGenSamplers"));
    GenVertexArrays = reinterpret_cast<PFNGLGENVERTEXARRAYSPROC>(LoadProcAddress("glGenVertexArrays"));
    GetProgramInfoLog = reinterpret_cast<PFNGLGETPROGRAMINFOLOGPROC>(LoadProcAddress("glGetProgramInfoLog"));
    GetProgramiv = reinterpret_cast<PFNGLGETPROGRAMIVPROC>(LoadProcAddress("glGetProgramiv"));
    GetShaderInfoLog = reinterpret_cast<PFNGLGETSHADERINFOLOGPROC>(LoadProcAddress("glGetShaderInfoLog"));
    GetShaderiv = reinterpret_cast<PFNGLGETSHADERIVPROC>(LoadProcAddress("glGetShaderiv"));
    GetUniformLocation = reinterpret_cast<PFNGLGETUNIFORMLOCATIONARBPROC>(LoadProcAddress("glGetUniformLocation"));
    LinkProgram = reinterpret_cast<PFNGLLINKPROGRAMPROC>(LoadProcAddress("glLinkProgram"));
    MapBuffer = reinterpret_cast<PFNGLMAPBUFFERPROC>(LoadProcAddress("glMapBuffer"));
    DrawElementsBaseVertex = reinterpret_cast<PFNGLDRAWELEMENTSBASEVERTEXPROC>(LoadProcAddress("glDrawElementsBaseVertex"));
    RenderbufferStorage = reinterpret_cast<PFNGLRENDERBUFFERSTORAGEPROC>(LoadProcAddress("glRenderbufferStorage"));
    SamplerParameterf = reinterpret_cast<PFNGLSAMPLERPARAMETERFPROC>(LoadProcAddress("glSamplerParameterf"));
    SamplerParameterfv = reinterpret_cast<PFNGLSAMPLERPARAMETERFVPROC>(LoadProcAddress("glSamplerParameterfv"));
    SamplerParameteri = reinterpret_cast<PFNGLSAMPLERPARAMETERIPROC>(LoadProcAddress("glSamplerParameteri"));
    ShaderSource = reinterpret_cast<PFNGLSHADERSOURCEPROC>(LoadProcAddress("glShaderSource"));
    Uniform1i = reinterpret_cast<PFNGLUNIFORM1IPROC>(LoadProcAddress("glUniform1i"));
    Uniform4fv = reinterpret_cast<PFNGLUNIFORM4FVPROC>(LoadProcAddress("glUniform4fv"));
    Uniform4iv = reinterpret_cast<PFNGLUNIFORM4IVPROC>(LoadProcAddress("glUniform4iv"));
    UnmapBuffer = reinterpret_cast<PFNGLUNMAPBUFFERPROC>(LoadProcAddress("glUnmapBuffer"));
    UseProgram = reinterpret_cast<PFNGLUSEPROGRAMPROC>(LoadProcAddress("glUseProgram"));
    VertexAttribDivisor = reinterpret_cast<PFNGLVERTEXATTRIBDIVISORPROC>(LoadProcAddress("glVertexAttribDivisor"));
    VertexAttribPointer = reinterpret_cast<PFNGLVERTEXATTRIBPOINTERPROC>(LoadProcAddress("glVertexAttribPointer"));
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
