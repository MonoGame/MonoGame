#pragma once

#include "api_enums.h"
#include "mg_common.h"

#include <SDL.h>
#include <SDL_opengl.h>
#include <SDL_opengl_glext.h>

struct OpenGLFunctions
{
    PFNGLBINDBUFFERPROC BindBuffer = nullptr;
    PFNGLBUFFERDATAPROC BufferData = nullptr;
    PFNGLBUFFERSUBDATAPROC BufferSubData = nullptr;
    PFNGLDELETEBUFFERSPROC DeleteBuffers = nullptr;
    PFNGLGENBUFFERSPROC GenBuffers = nullptr;
    PFNGLBINDVERTEXARRAYPROC BindVertexArray = nullptr;
    PFNGLDELETEVERTEXARRAYSPROC DeleteVertexArrays = nullptr;
    PFNGLGENVERTEXARRAYSPROC GenVertexArrays = nullptr;
    PFNGLDRAWELEMENTSBASEVERTEXPROC DrawElementsBaseVertex = nullptr;

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
