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

    void* TryLoadProcAddress(const char* name)
    {
        assert(name != nullptr);
        return SDL_GL_GetProcAddress(name);
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
    BlendEquationSeparatei = reinterpret_cast<PFNGLBLENDEQUATIONSEPARATEIPROC>(TryLoadProcAddress("glBlendEquationSeparatei"));
    if (BlendEquationSeparatei == nullptr)
        BlendEquationSeparatei = reinterpret_cast<PFNGLBLENDEQUATIONSEPARATEIPROC>(TryLoadProcAddress("glBlendEquationSeparateiARB"));
    BlendFuncSeparate = reinterpret_cast<PFNGLBLENDFUNCSEPARATEPROC>(LoadProcAddress("glBlendFuncSeparate"));
    BlendFuncSeparatei = reinterpret_cast<PFNGLBLENDFUNCSEPARATEIPROC>(TryLoadProcAddress("glBlendFuncSeparatei"));
    if (BlendFuncSeparatei == nullptr)
        BlendFuncSeparatei = reinterpret_cast<PFNGLBLENDFUNCSEPARATEIPROC>(TryLoadProcAddress("glBlendFuncSeparateiARB"));
    BindBuffer = reinterpret_cast<PFNGLBINDBUFFERPROC>(LoadProcAddress("glBindBuffer"));
    BindFramebuffer = reinterpret_cast<PFNGLBINDFRAMEBUFFERPROC>(LoadProcAddress("glBindFramebuffer"));
    BindRenderbuffer = reinterpret_cast<PFNGLBINDRENDERBUFFERPROC>(LoadProcAddress("glBindRenderbuffer"));
    BindSampler = reinterpret_cast<PFNGLBINDSAMPLERPROC>(LoadProcAddress("glBindSampler"));
    BindVertexArray = reinterpret_cast<PFNGLBINDVERTEXARRAYPROC>(LoadProcAddress("glBindVertexArray"));
    BlitFramebuffer = reinterpret_cast<PFNGLBLITFRAMEBUFFERPROC>(LoadProcAddress("glBlitFramebuffer"));
    BufferData = reinterpret_cast<PFNGLBUFFERDATAPROC>(LoadProcAddress("glBufferData"));
    BufferSubData = reinterpret_cast<PFNGLBUFFERSUBDATAPROC>(LoadProcAddress("glBufferSubData"));
    CheckFramebufferStatus = reinterpret_cast<PFNGLCHECKFRAMEBUFFERSTATUSPROC>(LoadProcAddress("glCheckFramebufferStatus"));
    ColorMaski = reinterpret_cast<PFNGLCOLORMASKIPROC>(LoadProcAddress("glColorMaski"));
    CompileShader = reinterpret_cast<PFNGLCOMPILESHADERPROC>(LoadProcAddress("glCompileShader"));
    CompressedTexImage2D = reinterpret_cast<MGGLCOMPRESSEDTEXIMAGE2DPROC>(LoadProcAddress("glCompressedTexImage2D"));
    CompressedTexSubImage2D = reinterpret_cast<MGGLCOMPRESSEDTEXSUBIMAGE2DPROC>(LoadProcAddress("glCompressedTexSubImage2D"));
    CreateProgram = reinterpret_cast<PFNGLCREATEPROGRAMPROC>(LoadProcAddress("glCreateProgram"));
    CreateShader = reinterpret_cast<PFNGLCREATESHADERPROC>(LoadProcAddress("glCreateShader"));
    DeleteBuffers = reinterpret_cast<PFNGLDELETEBUFFERSPROC>(LoadProcAddress("glDeleteBuffers"));
    DeleteFramebuffers = reinterpret_cast<PFNGLDELETEFRAMEBUFFERSPROC>(LoadProcAddress("glDeleteFramebuffers"));
    DeleteProgram = reinterpret_cast<PFNGLDELETEPROGRAMPROC>(LoadProcAddress("glDeleteProgram"));
    DeleteQueries = reinterpret_cast<PFNGLDELETEQUERIESPROC>(LoadProcAddress("glDeleteQueries"));
    DeleteRenderbuffers = reinterpret_cast<PFNGLDELETERENDERBUFFERSPROC>(LoadProcAddress("glDeleteRenderbuffers"));
    DeleteSamplers = reinterpret_cast<PFNGLDELETESAMPLERSPROC>(LoadProcAddress("glDeleteSamplers"));
    DeleteShader = reinterpret_cast<PFNGLDELETESHADERPROC>(LoadProcAddress("glDeleteShader"));
    DeleteVertexArrays = reinterpret_cast<PFNGLDELETEVERTEXARRAYSPROC>(LoadProcAddress("glDeleteVertexArrays"));
    DetachShader = reinterpret_cast<PFNGLDETACHSHADERPROC>(LoadProcAddress("glDetachShader"));
    DisableVertexAttribArray = reinterpret_cast<PFNGLDISABLEVERTEXATTRIBARRAYPROC>(LoadProcAddress("glDisableVertexAttribArray"));
    BeginQuery = reinterpret_cast<PFNGLBEGINQUERYPROC>(LoadProcAddress("glBeginQuery"));
    DrawBuffers = reinterpret_cast<PFNGLDRAWBUFFERSPROC>(LoadProcAddress("glDrawBuffers"));
    DrawElementsInstanced = reinterpret_cast<PFNGLDRAWELEMENTSINSTANCEDPROC>(LoadProcAddress("glDrawElementsInstanced"));
    EndQuery = reinterpret_cast<PFNGLENDQUERYPROC>(LoadProcAddress("glEndQuery"));
    EnableVertexAttribArray = reinterpret_cast<PFNGLENABLEVERTEXATTRIBARRAYPROC>(LoadProcAddress("glEnableVertexAttribArray"));
    GetTexImage = reinterpret_cast<MGGLGETTEXIMAGEPROC>(LoadProcAddress("glGetTexImage"));
    FramebufferRenderbuffer = reinterpret_cast<PFNGLFRAMEBUFFERRENDERBUFFERPROC>(LoadProcAddress("glFramebufferRenderbuffer"));
    FramebufferTexture2D = reinterpret_cast<PFNGLFRAMEBUFFERTEXTURE2DPROC>(LoadProcAddress("glFramebufferTexture2D"));
    GenerateMipmap = reinterpret_cast<PFNGLGENERATEMIPMAPPROC>(LoadProcAddress("glGenerateMipmap"));
    GenBuffers = reinterpret_cast<PFNGLGENBUFFERSPROC>(LoadProcAddress("glGenBuffers"));
    GenFramebuffers = reinterpret_cast<PFNGLGENFRAMEBUFFERSPROC>(LoadProcAddress("glGenFramebuffers"));
    GenQueries = reinterpret_cast<PFNGLGENQUERIESPROC>(LoadProcAddress("glGenQueries"));
    GenRenderbuffers = reinterpret_cast<PFNGLGENRENDERBUFFERSPROC>(LoadProcAddress("glGenRenderbuffers"));
    GenSamplers = reinterpret_cast<PFNGLGENSAMPLERSPROC>(LoadProcAddress("glGenSamplers"));
    GenVertexArrays = reinterpret_cast<PFNGLGENVERTEXARRAYSPROC>(LoadProcAddress("glGenVertexArrays"));
    GetCompressedTexImage = reinterpret_cast<MGGLGETCOMPRESSEDTEXIMAGEPROC>(LoadProcAddress("glGetCompressedTexImage"));
    GetAttribLocation = reinterpret_cast<PFNGLGETATTRIBLOCATIONPROC>(LoadProcAddress("glGetAttribLocation"));
    GetProgramInfoLog = reinterpret_cast<PFNGLGETPROGRAMINFOLOGPROC>(LoadProcAddress("glGetProgramInfoLog"));
    GetProgramiv = reinterpret_cast<PFNGLGETPROGRAMIVPROC>(LoadProcAddress("glGetProgramiv"));
    GetQueryObjectuiv = reinterpret_cast<PFNGLGETQUERYOBJECTUIVPROC>(LoadProcAddress("glGetQueryObjectuiv"));
    GetShaderInfoLog = reinterpret_cast<PFNGLGETSHADERINFOLOGPROC>(LoadProcAddress("glGetShaderInfoLog"));
    GetShaderiv = reinterpret_cast<PFNGLGETSHADERIVPROC>(LoadProcAddress("glGetShaderiv"));
    GetUniformLocation = reinterpret_cast<PFNGLGETUNIFORMLOCATIONPROC>(LoadProcAddress("glGetUniformLocation"));
    LinkProgram = reinterpret_cast<PFNGLLINKPROGRAMPROC>(LoadProcAddress("glLinkProgram"));
    MapBuffer = reinterpret_cast<PFNGLMAPBUFFERPROC>(LoadProcAddress("glMapBuffer"));
    DrawElementsBaseVertex = reinterpret_cast<PFNGLDRAWELEMENTSBASEVERTEXPROC>(LoadProcAddress("glDrawElementsBaseVertex"));
    RenderbufferStorage = reinterpret_cast<PFNGLRENDERBUFFERSTORAGEPROC>(LoadProcAddress("glRenderbufferStorage"));
    RenderbufferStorageMultisample = reinterpret_cast<PFNGLRENDERBUFFERSTORAGEMULTISAMPLEPROC>(LoadProcAddress("glRenderbufferStorageMultisample"));
    SamplerParameterf = reinterpret_cast<PFNGLSAMPLERPARAMETERFPROC>(LoadProcAddress("glSamplerParameterf"));
    SamplerParameterfv = reinterpret_cast<PFNGLSAMPLERPARAMETERFVPROC>(LoadProcAddress("glSamplerParameterfv"));
    SamplerParameteri = reinterpret_cast<PFNGLSAMPLERPARAMETERIPROC>(LoadProcAddress("glSamplerParameteri"));
    ShaderSource = reinterpret_cast<PFNGLSHADERSOURCEPROC>(LoadProcAddress("glShaderSource"));
    StencilFuncSeparate = reinterpret_cast<PFNGLSTENCILFUNCSEPARATEPROC>(LoadProcAddress("glStencilFuncSeparate"));
    StencilOpSeparate = reinterpret_cast<PFNGLSTENCILOPSEPARATEPROC>(LoadProcAddress("glStencilOpSeparate"));
    TexImage3D = reinterpret_cast<PFNGLTEXIMAGE3DPROC>(LoadProcAddress("glTexImage3D"));
    TexSubImage3D = reinterpret_cast<PFNGLTEXSUBIMAGE3DPROC>(LoadProcAddress("glTexSubImage3D"));
    Uniform1i = reinterpret_cast<PFNGLUNIFORM1IPROC>(LoadProcAddress("glUniform1i"));
    Uniform4fv = reinterpret_cast<PFNGLUNIFORM4FVPROC>(LoadProcAddress("glUniform4fv"));
    Uniform4iv = reinterpret_cast<PFNGLUNIFORM4IVPROC>(LoadProcAddress("glUniform4iv"));
    UnmapBuffer = reinterpret_cast<PFNGLUNMAPBUFFERPROC>(LoadProcAddress("glUnmapBuffer"));
    UseProgram = reinterpret_cast<PFNGLUSEPROGRAMPROC>(LoadProcAddress("glUseProgram"));
    VertexAttribDivisor = reinterpret_cast<PFNGLVERTEXATTRIBDIVISORPROC>(LoadProcAddress("glVertexAttribDivisor"));
    VertexAttribPointer = reinterpret_cast<PFNGLVERTEXATTRIBPOINTERPROC>(LoadProcAddress("glVertexAttribPointer"));
}

bool OpenGLContext::Create(SDL_Window* nextWindow)
{
    assert(nextWindow != nullptr);

    if (handle != nullptr && window == nextWindow)
        return true;

    SDL_GLContext previousHandle = handle;
    SDL_Window* previousWindow = window;

    if (previousHandle != nullptr)
    {
        // On Windows the GL conext is tied to the pixel format of the window it was
        // created for, so create the new context with resource sharing enabled before
        // getting rid of the old one.
        if (SDL_GL_GetCurrentContext() != previousHandle || SDL_GL_GetCurrentWindow() != previousWindow)
        {
            if (SDL_GL_MakeCurrent(previousWindow, previousHandle) < 0)
                MGGL_FAIL_SDL("SDL_GL_MakeCurrent failed");
        }

        SDL_GL_SetAttribute(SDL_GL_SHARE_WITH_CURRENT_CONTEXT, 1);
    }

    SDL_GLContext nextHandle = SDL_GL_CreateContext(nextWindow);
    if (nextHandle == nullptr && previousHandle == nullptr)
        return false;

    if (previousHandle != nullptr)
        SDL_GL_SetAttribute(SDL_GL_SHARE_WITH_CURRENT_CONTEXT, 0);

    if (nextHandle == nullptr)
        MGGL_FAIL_SDL("SDL_GL_CreateContext failed");

    handle = nextHandle;
    window = nextWindow;
    MakeCurrent();

    QueryVersion(majorVersion, minorVersion);
    if (majorVersion < 3 || (majorVersion == 3 && minorVersion < 1))
        MGGL_FAIL("OpenGL 3.1 core context required", "created context is below 3.1");

    functions.Load();

    // The new context is current and shares resources with the old context, so we
    // dont' need to keep the old context around anymore.
    if (previousHandle != nullptr)
        SDL_GL_DeleteContext(previousHandle);

    functions.GenVertexArrays(1, &defaultVertexArray);
    if (defaultVertexArray == 0)
        MGGL_FAIL("glGenVertexArrays failed", "default vertex array creation returned 0");

    functions.BindVertexArray(defaultVertexArray);
    return true;
}

void OpenGLContext::Destroy()
{
    if (handle != nullptr)
    {
        // The SDl window may have already been recreated, so clear the current
        // context instead of trying to make it current on the old window again.
        if (SDL_GL_GetCurrentContext() == handle && SDL_GL_MakeCurrent(nullptr, nullptr) < 0)
            MGGL_FAIL_SDL("SDL_GL_MakeCurrent failed");

        SDL_GL_DeleteContext(handle);
    }

    window = nullptr;
    handle = nullptr;
    defaultVertexArray = 0;
    width = 0;
    height = 0;
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
