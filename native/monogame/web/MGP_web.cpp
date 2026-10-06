#include "../include/api_MGP.h"
#include "../sdl/MGP_sdl_browser.h"
#include "MGP_web.h"

#include <cstdlib>
#include <emscripten/emscripten.h>
#include <queue>
#include <string>

static MGP_Platform* s_platform = nullptr;
static bool s_hasPendingCanvasResize = false;
static mgint s_pendingCanvasResizeWidth = 0;
static mgint s_pendingCanvasResizeHeight = 0;
static bool s_hasPendingBrowserFocus = false;
static mgbyte s_pendingBrowserFocus = false;
static bool s_hasPendingBrowserFullscreen = false;
static mgbyte s_pendingBrowserFullscreen = false;
static bool s_pointerLockEnabled = false;

struct MGP_WebPendingDropEvent
{
    bool complete;
    std::string path;
};

static std::queue<MGP_WebPendingDropEvent> s_pendingDropEvents;

enum MGP_WebSensorState : mgint
{
    MGP_WEB_SENSOR_STATE_NOT_SUPPORTED = 0,
    MGP_WEB_SENSOR_STATE_READY = 1,
    MGP_WEB_SENSOR_STATE_INITIALIZING = 2,
    MGP_WEB_SENSOR_STATE_NO_DATA = 3,
    MGP_WEB_SENSOR_STATE_NO_PERMISSIONS = 4,
    MGP_WEB_SENSOR_STATE_DISABLED = 5,
};

static mgint s_accelerometerState = MGP_WEB_SENSOR_STATE_NOT_SUPPORTED;
static mgfloat s_accelerometerX = 0.0f;
static mgfloat s_accelerometerY = 0.0f;
static mgfloat s_accelerometerZ = 0.0f;
static mgint s_accelerometerSequence = 0;

EM_JS(mgint, MGP_Web_GetMaximumTouchCountFromNavigator, (),
{
    if (typeof navigator === "undefined" || typeof navigator.maxTouchPoints !== "number")
        return 0;

    return navigator.maxTouchPoints;
});

EM_JS(char*, MGP_Web_TakeDroppedFilePathFromHost, (),
{
    const path = globalThis.MonoGameWebHost?.takeDroppedFilePath?.();
    return typeof path === "string" ? stringToNewUTF8(path) : 0;
});

static void MGP_Web_QueueDroppedFile(MGP_Platform* platform, const char* path)
{
    if (platform == nullptr)
    {
        s_pendingDropEvents.push({ false, path });
        return;
    }

    MGP_Sdl_QueueBrowserFileDrop(platform, path);
}

static void MGP_Web_QueueDroppedFileComplete(MGP_Platform* platform)
{
    if (platform == nullptr)
    {
        s_pendingDropEvents.push({ true, "" });
        return;
    }

    MGP_Sdl_QueueBrowserFileDropComplete(platform);
}

mgint MGP_Web_GetMaximumTouchCount()
{
    return MGP_Web_GetMaximumTouchCountFromNavigator();
}

extern "C" EMSCRIPTEN_KEEPALIVE void MGP_Web_SetPointerLockEnabled(mgbyte enabled)
{
    s_pointerLockEnabled = enabled != 0;
}

mgbyte MGP_Web_IsPointerLockEnabled()
{
    return s_pointerLockEnabled ? 1 : 0;
}

EM_JS(mgbyte, MGP_Web_RequestFullscreenFromHost, (),
{
    if (typeof globalThis.MonoGameWebHost?.requestFullscreen !== "function")
        return 0;

    globalThis.MonoGameWebHost.requestFullscreen();
    return 1;
});

EM_JS(mgbyte, MGP_Web_ExitFullscreenFromHost, (),
{
    if (typeof globalThis.MonoGameWebHost?.exitFullscreen !== "function")
        return 0;

    globalThis.MonoGameWebHost.exitFullscreen();
    return 1;
});

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

extern "C" EMSCRIPTEN_KEEPALIVE void MGP_Web_NotifyFocusChange(mgbyte focused)
{
    if (s_platform == nullptr)
    {
        s_pendingBrowserFocus = focused;
        s_hasPendingBrowserFocus = true;
        return;
    }

    MGP_Sdl_QueueBrowserFocus(s_platform, focused);
}

extern "C" EMSCRIPTEN_KEEPALIVE void MGP_Web_NotifyFullscreenChange(mgbyte fullscreen)
{
    if (s_platform == nullptr)
    {
        s_pendingBrowserFullscreen = fullscreen;
        s_hasPendingBrowserFullscreen = true;
        return;
    }

    MGP_Sdl_QueueBrowserFullscreenChange(s_platform, fullscreen);
}

extern "C" EMSCRIPTEN_KEEPALIVE void MGP_Web_NotifyFullscreenFailure()
{
    if (s_platform != nullptr)
        MGP_Sdl_QueueBrowserFullscreenFailure(s_platform);
}

extern "C" EMSCRIPTEN_KEEPALIVE void MGP_Web_NotifyFileDrop()
{
    char* path = MGP_Web_TakeDroppedFilePathFromHost();
    if (path == nullptr)
        return;

    MGP_Web_QueueDroppedFile(s_platform, path);
    free(path);
}

extern "C" EMSCRIPTEN_KEEPALIVE void MGP_Web_NotifyFileDropComplete()
{
    MGP_Web_QueueDroppedFileComplete(s_platform);
}

void MGP_Web_RequestFullscreen()
{
    if (MGP_Web_RequestFullscreenFromHost() != 0)
        return;

    if (s_platform != nullptr)
        MGP_Sdl_QueueBrowserFullscreenFailure(s_platform);
}

void MGP_Web_ExitFullscreen()
{
    if (MGP_Web_ExitFullscreenFromHost() != 0)
        return;

    if (s_platform != nullptr)
        MGP_Sdl_QueueBrowserFullscreenFailure(s_platform);
}

EM_JS(mgbyte, MGP_Web_AccelerometerIsSupportedFromHost, (),
{
    return globalThis.MonoGameWebHost?.getActiveHost?.().isAccelerometerSupported?.() ? 1 : 0;
});

EM_JS(mgint, MGP_Web_AccelerometerStartFromHost, (),
{
    return globalThis.MonoGameWebHost?.getActiveHost?.().requestAccelerometer?.() ?? 0;
});

EM_JS(void, MGP_Web_AccelerometerStopFromHost, (),
{
    globalThis.MonoGameWebHost?.getActiveHost?.().stopAccelerometer?.();
});

mgbyte MGP_Web_Accelerometer_IsSupported()
{
    return MGP_Web_AccelerometerIsSupportedFromHost();
}

mgint MGP_Web_Accelerometer_Start()
{
    s_accelerometerState = MGP_Web_AccelerometerStartFromHost();
    return s_accelerometerState;
}

void MGP_Web_Accelerometer_Stop()
{
    MGP_Web_AccelerometerStopFromHost();
    s_accelerometerState = MGP_WEB_SENSOR_STATE_DISABLED;
}

mgint MGP_Web_Accelerometer_GetState()
{
    return s_accelerometerState;
}

mgbyte MGP_Web_Accelerometer_GetReading(mgfloat& x, mgfloat& y, mgfloat& z, mgint& sequence)
{
    if (s_accelerometerSequence == 0)
        return 0;

    x = s_accelerometerX;
    y = s_accelerometerY;
    z = s_accelerometerZ;
    sequence = s_accelerometerSequence;
    return 1;
}

extern "C" EMSCRIPTEN_KEEPALIVE void MGP_Web_NotifyAccelerometerState(mgint state)
{
    s_accelerometerState = state;
}

extern "C" EMSCRIPTEN_KEEPALIVE void MGP_Web_NotifyAccelerometerReading(mgfloat x, mgfloat y, mgfloat z)
{
    s_accelerometerX = x;
    s_accelerometerY = y;
    s_accelerometerZ = z;
    ++s_accelerometerSequence;
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

    if (s_hasPendingBrowserFocus)
    {
        MGP_Sdl_QueueBrowserFocus(platform, s_pendingBrowserFocus);
        s_hasPendingBrowserFocus = false;
    }

    if (s_hasPendingBrowserFullscreen)
    {
        MGP_Sdl_QueueBrowserFullscreenChange(platform, s_pendingBrowserFullscreen);
        s_hasPendingBrowserFullscreen = false;
    }

    while (!s_pendingDropEvents.empty())
    {
        const MGP_WebPendingDropEvent& event_ = s_pendingDropEvents.front();
        if (event_.complete)
            MGP_Sdl_QueueBrowserFileDropComplete(platform);
        else
            MGP_Sdl_QueueBrowserFileDrop(platform, event_.path.c_str());

        s_pendingDropEvents.pop();
    }
}
