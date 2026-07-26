#pragma once

#include "api_enums.h"
#include "mg_common.h"

#include <SDL.h>
#include <SDL_opengl.h>
#include <SDL_opengl_glext.h>

// SDL declares glGetTexImage here, but this header set does not expose PFNGLGETTEXIMAGEPROC.
using MGGLGETTEXIMAGEPROC = decltype(&glGetTexImage);

struct OpenGLFunctions
{
    PFNGLACTIVETEXTUREPROC ActiveTexture = nullptr;
    PFNGLATTACHSHADERPROC AttachShader = nullptr;
    PFNGLBINDATTRIBLOCATIONPROC BindAttribLocation = nullptr;
    PFNGLBLENDCOLORPROC BlendColor = nullptr;
    PFNGLBLENDEQUATIONSEPARATEEXTPROC BlendEquationSeparate = nullptr;
    PFNGLBLENDFUNCSEPARATEPROC BlendFuncSeparate = nullptr;
    PFNGLBINDBUFFERPROC BindBuffer = nullptr;
    PFNGLBINDFRAMEBUFFERPROC BindFramebuffer = nullptr;
    PFNGLBINDRENDERBUFFERPROC BindRenderbuffer = nullptr;
    PFNGLBINDSAMPLERPROC BindSampler = nullptr;
    PFNGLBINDVERTEXARRAYPROC BindVertexArray = nullptr;
    PFNGLBUFFERDATAPROC BufferData = nullptr;
    PFNGLBUFFERSUBDATAPROC BufferSubData = nullptr;
    PFNGLCHECKFRAMEBUFFERSTATUSPROC CheckFramebufferStatus = nullptr;
    PFNGLCOMPILESHADERPROC CompileShader = nullptr;
    PFNGLCREATEPROGRAMPROC CreateProgram = nullptr;
    PFNGLCREATESHADERPROC CreateShader = nullptr;
    PFNGLDELETEBUFFERSPROC DeleteBuffers = nullptr;
    PFNGLDELETEFRAMEBUFFERSPROC DeleteFramebuffers = nullptr;
    PFNGLDELETEPROGRAMPROC DeleteProgram = nullptr;
    PFNGLDELETERENDERBUFFERSPROC DeleteRenderbuffers = nullptr;
    PFNGLDELETESAMPLERSPROC DeleteSamplers = nullptr;
    PFNGLDELETESHADERPROC DeleteShader = nullptr;
    PFNGLDELETEVERTEXARRAYSPROC DeleteVertexArrays = nullptr;
    PFNGLDETACHSHADERPROC DetachShader = nullptr;
    PFNGLDISABLEVERTEXATTRIBARRAYPROC DisableVertexAttribArray = nullptr;
    PFNGLGENVERTEXARRAYSPROC GenVertexArrays = nullptr;
    PFNGLDRAWELEMENTSBASEVERTEXPROC DrawElementsBaseVertex = nullptr;
    PFNGLENABLEVERTEXATTRIBARRAYPROC EnableVertexAttribArray = nullptr;
    MGGLGETTEXIMAGEPROC GetTexImage = nullptr;
    PFNGLFRAMEBUFFERRENDERBUFFERPROC FramebufferRenderbuffer = nullptr;
    PFNGLFRAMEBUFFERTEXTURE2DPROC FramebufferTexture2D = nullptr;
    PFNGLGENBUFFERSPROC GenBuffers = nullptr;
    PFNGLGENFRAMEBUFFERSPROC GenFramebuffers = nullptr;
    PFNGLGENRENDERBUFFERSPROC GenRenderbuffers = nullptr;
    PFNGLGENSAMPLERSPROC GenSamplers = nullptr;
    PFNGLGETPROGRAMINFOLOGPROC GetProgramInfoLog = nullptr;
    PFNGLGETPROGRAMIVPROC GetProgramiv = nullptr;
    PFNGLGETSHADERINFOLOGPROC GetShaderInfoLog = nullptr;
    PFNGLGETSHADERIVPROC GetShaderiv = nullptr;
    PFNGLGETUNIFORMLOCATIONARBPROC GetUniformLocation = nullptr;
    PFNGLLINKPROGRAMPROC LinkProgram = nullptr;
    PFNGLMAPBUFFERPROC MapBuffer = nullptr;
    PFNGLRENDERBUFFERSTORAGEPROC RenderbufferStorage = nullptr;
    PFNGLSAMPLERPARAMETERFPROC SamplerParameterf = nullptr;
    PFNGLSAMPLERPARAMETERFVPROC SamplerParameterfv = nullptr;
    PFNGLSAMPLERPARAMETERIPROC SamplerParameteri = nullptr;
    PFNGLSHADERSOURCEPROC ShaderSource = nullptr;
    PFNGLUNIFORM1IPROC Uniform1i = nullptr;
    PFNGLUNIFORM4FVPROC Uniform4fv = nullptr;
    PFNGLUNIFORM4IVPROC Uniform4iv = nullptr;
    PFNGLUNMAPBUFFERPROC UnmapBuffer = nullptr;
    PFNGLUSEPROGRAMPROC UseProgram = nullptr;
    PFNGLVERTEXATTRIBDIVISORPROC VertexAttribDivisor = nullptr;
    PFNGLVERTEXATTRIBPOINTERPROC VertexAttribPointer = nullptr;

    void Load();
};

struct OpenGLContext
{
    SDL_Window* window = nullptr;
    SDL_GLContext handle = nullptr;
    OpenGLFunctions functions;
    GLuint defaultVertexArray = 0;
    mgint width = 0;
    mgint height = 0;
    mgint syncInterval = 0;
    mgint majorVersion = 0;
    mgint minorVersion = 0;

    void Create(SDL_Window* nextWindow);
    void Destroy();
    void MakeCurrent();
    void SetSwapInterval(mgint nextSyncInterval);
    void BindDefaultVertexArray();
};
