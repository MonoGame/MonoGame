#pragma once

#include "api_enums.h"
#include "mg_common.h"

#include <SDL.h>
#include <SDL_opengl.h>
#include <SDL_opengl_glext.h>

struct OpenGLFunctions
{
    PFNGLACTIVETEXTUREPROC ActiveTexture = nullptr;
    PFNGLBINDBUFFERPROC BindBuffer = nullptr;
    PFNGLBINDFRAMEBUFFERPROC BindFramebuffer = nullptr;
    PFNGLBINDRENDERBUFFERPROC BindRenderbuffer = nullptr;
    PFNGLBINDSAMPLERPROC BindSampler = nullptr;
    PFNGLBUFFERDATAPROC BufferData = nullptr;
    PFNGLBUFFERSUBDATAPROC BufferSubData = nullptr;
    PFNGLCHECKFRAMEBUFFERSTATUSPROC CheckFramebufferStatus = nullptr;
    PFNGLDELETEBUFFERSPROC DeleteBuffers = nullptr;
    PFNGLDELETEFRAMEBUFFERSPROC DeleteFramebuffers = nullptr;
    PFNGLDELETERENDERBUFFERSPROC DeleteRenderbuffers = nullptr;
    PFNGLDELETESAMPLERSPROC DeleteSamplers = nullptr;
    PFNGLFRAMEBUFFERRENDERBUFFERPROC FramebufferRenderbuffer = nullptr;
    PFNGLFRAMEBUFFERTEXTURE2DPROC FramebufferTexture2D = nullptr;
    PFNGLGENBUFFERSPROC GenBuffers = nullptr;
    PFNGLGENFRAMEBUFFERSPROC GenFramebuffers = nullptr;
    PFNGLGENRENDERBUFFERSPROC GenRenderbuffers = nullptr;
    PFNGLGENSAMPLERSPROC GenSamplers = nullptr;
    PFNGLBINDVERTEXARRAYPROC BindVertexArray = nullptr;
    PFNGLDELETEVERTEXARRAYSPROC DeleteVertexArrays = nullptr;
    PFNGLGENVERTEXARRAYSPROC GenVertexArrays = nullptr;
    PFNGLDRAWELEMENTSBASEVERTEXPROC DrawElementsBaseVertex = nullptr;
    PFNGLRENDERBUFFERSTORAGEPROC RenderbufferStorage = nullptr;
    PFNGLSAMPLERPARAMETERFPROC SamplerParameterf = nullptr;
    PFNGLSAMPLERPARAMETERFVPROC SamplerParameterfv = nullptr;
    PFNGLSAMPLERPARAMETERIPROC SamplerParameteri = nullptr;

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
