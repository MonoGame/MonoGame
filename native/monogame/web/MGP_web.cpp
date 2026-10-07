#include "../include/api_MGP.h"
#include "../sdl/MGP_sdl_browser.h"
#include "MGP_web.h"

#include <climits>
#include <cstdlib>
#include <emscripten/emscripten.h>
#include <queue>
#include <string>
#include <vector>

static MGP_Platform* s_platform = nullptr;
static bool s_hasPendingCanvasResize = false;
static mgint s_pendingCanvasResizeWidth = 0;
static mgint s_pendingCanvasResizeHeight = 0;
static bool s_hasPendingBrowserFocus = false;
static mgbyte s_pendingBrowserFocus = false;
static bool s_hasPendingBrowserFullscreen = false;
static mgbyte s_pendingBrowserFullscreen = false;
static bool s_browserHostControlsResize = false;
static bool s_pointerLockEnabled = false;

struct MGP_WebWindowState
{
    MGP_Window* window = nullptr;
    bool hasNativeWindow = false;
    mgint lastBrowserResizeWidth = -1;
    mgint lastBrowserResizeHeight = -1;
    mgbyte browserFocused = false;
    bool hasBrowserFocusState = false;
    mgbyte browserFullscreen = false;
    bool hasBrowserFullscreenState = false;
    mgbyte browserFullscreenRequestTarget = false;
    bool browserFullscreenRequestPending = false;
};

static std::vector<MGP_WebWindowState> s_browserWindows;

struct MGP_WebPendingDropEvent
{
    bool complete;
    std::string path;
};

static std::queue<MGP_WebPendingDropEvent> s_pendingDropEvents;

static MGP_WebWindowState* MGP_Web_GetWindowState(MGP_Window* window)
{
    for (MGP_WebWindowState& state : s_browserWindows)
    {
        if (state.window == window)
            return &state;
    }

    return nullptr;
}

static MGP_WebWindowState* MGP_Web_GetPrimaryWindowState()
{
    for (MGP_WebWindowState& state : s_browserWindows)
    {
        if (state.hasNativeWindow)
            return &state;
    }

    return nullptr;
}

static void MGP_Web_PushWindowEvent(
    MGP_WebWindowState& state,
    MGEventType type,
    mgint data1 = 0,
    mgint data2 = 0)
{
    MGP_Event event_{};
    event_.Type = type;
    event_.Window.Window = state.window;
    event_.Window.Data1 = data1;
    event_.Window.Data2 = data2;
    MGP_Sdl_PushEvent(s_platform, event_);
}

static void MGP_Web_QueueBrowserResize(mgint width, mgint height)
{
    if (width <= 0 || height <= 0 || s_platform == nullptr)
        return;

    for (MGP_WebWindowState& state : s_browserWindows)
    {
        if (!state.hasNativeWindow
            || (state.lastBrowserResizeWidth == width
                && state.lastBrowserResizeHeight == height))
        {
            continue;
        }

        state.lastBrowserResizeWidth = width;
        state.lastBrowserResizeHeight = height;
        MGP_Web_PushWindowEvent(state, MGEventType::WindowResized, width, height);
    }
}

static void MGP_Web_QueueBrowserFocus(mgbyte focused)
{
    if (s_platform == nullptr)
        return;

    for (MGP_WebWindowState& state : s_browserWindows)
    {
        if (!state.hasNativeWindow
            || (state.hasBrowserFocusState && state.browserFocused == focused))
        {
            continue;
        }

        state.browserFocused = focused;
        state.hasBrowserFocusState = true;
        MGP_Web_PushWindowEvent(
            state,
            focused != 0 ? MGEventType::WindowGainedFocus : MGEventType::WindowLostFocus);
    }
}

static void MGP_Web_QueueBrowserFullscreenChange(mgbyte fullscreen)
{
    if (s_platform == nullptr)
        return;

    for (MGP_WebWindowState& state : s_browserWindows)
    {
        if (!state.hasNativeWindow)
            continue;

        const bool stateChanged = !state.hasBrowserFullscreenState
            || state.browserFullscreen != fullscreen;

        // A browser confirmation can report the previously confirmed state after a
        // failed request. It still completes the request, but must not emit a
        // fullscreen transition that did not occur.
        state.browserFullscreenRequestPending = false;

        if (!stateChanged)
            continue;

        state.browserFullscreen = fullscreen;
        state.hasBrowserFullscreenState = true;
        MGP_Web_PushWindowEvent(state, MGEventType::WindowFullscreenChanged, fullscreen);
    }
}

static void MGP_Web_QueueBrowserFullscreenFailure()
{
    for (MGP_WebWindowState& state : s_browserWindows)
    {
        if (state.hasNativeWindow)
            state.browserFullscreenRequestPending = false;
    }
}

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

    MGP_WebWindowState* state = MGP_Web_GetPrimaryWindowState();
    if (state == nullptr)
        return;

    MGP_Event event_{};
    event_.Type = MGEventType::DropFile;
    event_.Drop.Window = state->window;
    event_.Drop.File = const_cast<char*>(path);
    MGP_Sdl_PushEvent(platform, event_);
}

static void MGP_Web_QueueDroppedFileComplete(MGP_Platform* platform)
{
    if (platform == nullptr)
    {
        s_pendingDropEvents.push({ true, "" });
        return;
    }

    MGP_WebWindowState* state = MGP_Web_GetPrimaryWindowState();
    if (state == nullptr)
        return;

    MGP_Event event_{};
    event_.Type = MGEventType::DropComplete;
    event_.Drop.Window = state->window;
    MGP_Sdl_PushEvent(platform, event_);
}

mgint MGP_Web_GetMaximumTouchCount()
{
    return MGP_Web_GetMaximumTouchCountFromNavigator();
}

extern "C" EMSCRIPTEN_KEEPALIVE void MGP_Web_SetPointerLockEnabled(mgbyte enabled)
{
    s_pointerLockEnabled = enabled != 0;
}

extern "C" EMSCRIPTEN_KEEPALIVE void MGP_Web_SetCanvasResizeManaged(mgbyte managed)
{
    s_browserHostControlsResize = managed != 0;
}

mgbyte MGP_Web_IsPointerLockEnabled()
{
    return s_pointerLockEnabled ? 1 : 0;
}

mgbyte MGP_Web_IsBrowserCanvasResizeManaged()
{
    return s_browserHostControlsResize ? 1 : 0;
}

mgbyte MGP_Web_IsDuplicateBrowserResize(MGP_Window* window, mgint width, mgint height)
{
    MGP_WebWindowState* state = MGP_Web_GetWindowState(window);
    if (state == nullptr || !state->hasNativeWindow)
        return 0;

    if (state->lastBrowserResizeWidth == width
        && state->lastBrowserResizeHeight == height)
    {
        return 1;
    }

    state->lastBrowserResizeWidth = width;
    state->lastBrowserResizeHeight = height;
    return 0;
}

mgbyte MGP_Web_GetBrowserFullscreen(MGP_Window* window)
{
    MGP_WebWindowState* state = MGP_Web_GetWindowState(window);
    return state != nullptr ? state->browserFullscreen : 0;
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

    MGP_Web_QueueBrowserResize(width, height);
}

extern "C" EMSCRIPTEN_KEEPALIVE void MGP_Web_NotifyFocusChange(mgbyte focused)
{
    if (s_platform == nullptr)
    {
        s_pendingBrowserFocus = focused;
        s_hasPendingBrowserFocus = true;
        return;
    }

    MGP_Web_QueueBrowserFocus(focused);
}

extern "C" EMSCRIPTEN_KEEPALIVE void MGP_Web_NotifyFullscreenChange(mgbyte fullscreen)
{
    if (s_platform == nullptr)
    {
        s_pendingBrowserFullscreen = fullscreen;
        s_hasPendingBrowserFullscreen = true;
        return;
    }

    MGP_Web_QueueBrowserFullscreenChange(fullscreen);
}

extern "C" EMSCRIPTEN_KEEPALIVE void MGP_Web_NotifyFullscreenFailure()
{
    MGP_Web_QueueBrowserFullscreenFailure();
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

void MGP_Web_RequestBrowserFullscreen(MGP_Window* window, mgbyte fullscreen)
{
    MGP_WebWindowState* state = MGP_Web_GetWindowState(window);
    if (state == nullptr || !state->hasNativeWindow)
        return;

    if (state->browserFullscreenRequestPending)
    {
        if (state->browserFullscreenRequestTarget == fullscreen)
            return;
    }
    else if (!state->hasBrowserFullscreenState)
    {
        if (fullscreen == 0)
            return;
    }
    else if (state->browserFullscreen == fullscreen)
    {
        return;
    }

    state->browserFullscreenRequestTarget = fullscreen;
    state->browserFullscreenRequestPending = true;

    if (fullscreen != 0 && MGP_Web_RequestFullscreenFromHost() != 0)
        return;
    if (fullscreen == 0 && MGP_Web_ExitFullscreenFromHost() != 0)
        return;

    MGP_Web_QueueBrowserFullscreenFailure();
}

EM_JS(mgbyte, MGP_Web_AccelerometerIsSupportedFromHost, (),
{
    return globalThis.MonoGameWebHost?.getActiveHost?.()?.isAccelerometerSupported?.() ? 1 : 0;
});

EM_JS(mgint, MGP_Web_AccelerometerStartFromHost, (),
{
    return globalThis.MonoGameWebHost?.getActiveHost?.()?.requestAccelerometer?.() ?? 0;
});

EM_JS(void, MGP_Web_AccelerometerStopFromHost, (),
{
    globalThis.MonoGameWebHost?.getActiveHost?.()?.stopAccelerometer?.();
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
    if (s_accelerometerState != MGP_WEB_SENSOR_STATE_READY
        || s_accelerometerSequence == 0)
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

    // Zero means that no reading has been received. Wrap before signed overflow
    // so the sequence remains a valid, non-zero value for the managed caller.
    s_accelerometerSequence = s_accelerometerSequence == INT_MAX
        ? 1
        : s_accelerometerSequence + 1;
}

void MGP_Web_OnPlatformDestroyed(MGP_Platform* platform)
{
    if (s_platform == platform)
    {
        s_platform = nullptr;
        s_browserWindows.clear();
    }
}

void MGP_Web_OnWindowCreated(MGP_Window* window)
{
    MGP_WebWindowState* state = MGP_Web_GetWindowState(window);
    if (state != nullptr)
    {
        state->hasNativeWindow = true;
        return;
    }

    MGP_WebWindowState newState{};
    newState.window = window;
    newState.hasNativeWindow = true;
    s_browserWindows.push_back(newState);
}

void MGP_Web_OnNativeWindowDestroyed(MGP_Window* window)
{
    MGP_WebWindowState* state = MGP_Web_GetWindowState(window);
    if (state != nullptr)
        state->hasNativeWindow = false;
}

void MGP_Web_OnWindowDestroyed(MGP_Window* window)
{
    for (auto it = s_browserWindows.begin();
        it != s_browserWindows.end();
        ++it)
    {
        if (it->window == window)
        {
            s_browserWindows.erase(it);
            return;
        }
    }
}

// browser host owns frame loop
MG_EXPORT void MGP_Platform_StartRunLoop(MGP_Platform* platform)
{
    s_platform = platform;

    if (s_hasPendingCanvasResize)
    {
        MGP_Web_QueueBrowserResize(
            s_pendingCanvasResizeWidth,
            s_pendingCanvasResizeHeight);
        s_hasPendingCanvasResize = false;
    }

    if (s_hasPendingBrowserFocus)
    {
        MGP_Web_QueueBrowserFocus(s_pendingBrowserFocus);
        s_hasPendingBrowserFocus = false;
    }

    if (s_hasPendingBrowserFullscreen)
    {
        MGP_Web_QueueBrowserFullscreenChange(s_pendingBrowserFullscreen);
        s_hasPendingBrowserFullscreen = false;
    }

    while (!s_pendingDropEvents.empty())
    {
        const MGP_WebPendingDropEvent& event_ = s_pendingDropEvents.front();
        if (event_.complete)
            MGP_Web_QueueDroppedFileComplete(platform);
        else
            MGP_Web_QueueDroppedFile(platform, event_.path.c_str());

        s_pendingDropEvents.pop();
    }
}
