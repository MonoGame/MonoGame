#pragma once

#include "api_enums.h"
#include "mg_common.h"

#include <SDL.h>

struct OpenGLContext
{
    SDL_Window* window = nullptr;
    SDL_GLContext handle = nullptr;
    mgint width = 0;
    mgint height = 0;
    mgint syncInterval = 0;
    mgint majorVersion = 0;
    mgint minorVersion = 0;

    void Create(SDL_Window* nextWindow);
    void Destroy();
    void MakeCurrent();
    void SetSwapInterval(mgint nextSyncInterval);
};
