#include "../include/api_MGP.h"
#include "../sdl/MGP_sdl_browser.h"
#include "MGP_web.h"

#include <emscripten/emscripten.h>

static MGP_Platform* s_platform = nullptr;
static bool s_hasPendingCanvasResize = false;
static mgint s_pendingCanvasResizeWidth = 0;
static mgint s_pendingCanvasResizeHeight = 0;

extern "C" EMSCRIPTEN_KEEPALIVE void MGP_Web_NotifyCanvasResize(mgint width, mgint height)
{
    if (width <= 0 || height <= 0)
        return;

    if (s_platform == nullptr)
    {
        s_pendingCanvasResizeWidth = width;
        s_pendingCanvasResizeHeight = height;
        s_hasPendingCanvasResize = true;
        return;
    }

    MGP_Sdl_QueueBrowserResize(s_platform, width, height);
}

void MGP_Web_OnPlatformDestroyed(MGP_Platform* platform)
{
    if (s_platform == platform)
        s_platform = nullptr;
}

// browser host owns frame loop
MG_EXPORT void MGP_Platform_StartRunLoop(MGP_Platform* platform)
{
    s_platform = platform;

    if (s_hasPendingCanvasResize)
    {
        MGP_Sdl_QueueBrowserResize(
            platform,
            s_pendingCanvasResizeWidth,
            s_pendingCanvasResizeHeight);
        s_hasPendingCanvasResize = false;
    }
}
