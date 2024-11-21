// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#include "api_MGP.h"

#include "mg_common.h"

#include <sdl.h>

#if _WIN32
#include <combaseapi.h>
#endif


struct MGP_Platform
{
    std::vector<MGP_Window*> windows;
    std::queue<MGP_Event> queued_events;
    std::map<mgint, SDL_GameController*> controllers;
};

static std::map<int, MGKeys> s_keymap
{
    { 8, MGKeys::Back },
    { 9, MGKeys::Tab },
    { 13, MGKeys::Enter },
    { 27, MGKeys::Escape },
    { 32, MGKeys::Space },
    { 39, MGKeys::OemQuotes },
    { 43, MGKeys::Add },
    { 44, MGKeys::OemComma },
    { 45, MGKeys::OemMinus },
    { 46, MGKeys::OemPeriod },
    { 47, MGKeys::OemQuestion },
    { 48, MGKeys::D0 },
    { 49, MGKeys::D1 },
    { 50, MGKeys::D2 },
    { 51, MGKeys::D3 },
    { 52, MGKeys::D4 },
    { 53, MGKeys::D5 },
    { 54, MGKeys::D6 },
    { 55, MGKeys::D7 },
    { 56, MGKeys::D8 },
    { 57, MGKeys::D9 },
    { 59, MGKeys::OemSemicolon },
    { 60, MGKeys::OemBackslash },
    { 61, MGKeys::OemPlus },
    { 91, MGKeys::OemOpenBrackets },
    { 92, MGKeys::OemPipe },
    { 93, MGKeys::OemCloseBrackets },
    { 96, MGKeys::OemTilde },
    { 97, MGKeys::A },
    { 98, MGKeys::B },
    { 99, MGKeys::C },
    { 100, MGKeys::D },
    { 101, MGKeys::E },
    { 102, MGKeys::F },
    { 103, MGKeys::G },
    { 104, MGKeys::H },
    { 105, MGKeys::I },
    { 106, MGKeys::J },
    { 107, MGKeys::K },
    { 108, MGKeys::L },
    { 109, MGKeys::M },
    { 110, MGKeys::N },
    { 111, MGKeys::O },
    { 112, MGKeys::P },
    { 113, MGKeys::Q },
    { 114, MGKeys::R },
    { 115, MGKeys::S },
    { 116, MGKeys::T },
    { 117, MGKeys::U },
    { 118, MGKeys::V },
    { 119, MGKeys::W },
    { 120, MGKeys::X },
    { 121, MGKeys::Y },
    { 122, MGKeys::Z },
    { 127, MGKeys::Delete },
    { 1073741881, MGKeys::CapsLock },
    { 1073741882, MGKeys::F1 },
    { 1073741883, MGKeys::F2 },
    { 1073741884, MGKeys::F3 },
    { 1073741885, MGKeys::F4 },
    { 1073741886, MGKeys::F5 },
    { 1073741887, MGKeys::F6 },
    { 1073741888, MGKeys::F7 },
    { 1073741889, MGKeys::F8 },
    { 1073741890, MGKeys::F9 },
    { 1073741891, MGKeys::F10 },
    { 1073741892, MGKeys::F11 },
    { 1073741893, MGKeys::F12 },
    { 1073741894, MGKeys::PrintScreen },
    { 1073741895, MGKeys::Scroll },
    { 1073741896, MGKeys::Pause },
    { 1073741897, MGKeys::Insert },
    { 1073741898, MGKeys::Home },
    { 1073741899, MGKeys::PageUp },
    { 1073741901, MGKeys::End },
    { 1073741902, MGKeys::PageDown },
    { 1073741903, MGKeys::Right },
    { 1073741904, MGKeys::Left },
    { 1073741905, MGKeys::Down },
    { 1073741906, MGKeys::Up },
    { 1073741907, MGKeys::NumLock },
    { 1073741908, MGKeys::Divide },
    { 1073741909, MGKeys::Multiply },
    { 1073741910, MGKeys::Subtract },
    { 1073741911, MGKeys::Add },
    { 1073741912, MGKeys::Enter },
    { 1073741913, MGKeys::NumPad1 },
    { 1073741914, MGKeys::NumPad2 },
    { 1073741915, MGKeys::NumPad3 },
    { 1073741916, MGKeys::NumPad4 },
    { 1073741917, MGKeys::NumPad5 },
    { 1073741918, MGKeys::NumPad6 },
    { 1073741919, MGKeys::NumPad7 },
    { 1073741920, MGKeys::NumPad8 },
    { 1073741921, MGKeys::NumPad9 },
    { 1073741922, MGKeys::NumPad0 },
    { 1073741923, MGKeys::Decimal },
    { 1073741925, MGKeys::Apps },
    { 1073741928, MGKeys::F13 },
    { 1073741929, MGKeys::F14 },
    { 1073741930, MGKeys::F15 },
    { 1073741931, MGKeys::F16 },
    { 1073741932, MGKeys::F17 },
    { 1073741933, MGKeys::F18 },
    { 1073741934, MGKeys::F19 },
    { 1073741935, MGKeys::F20 },
    { 1073741936, MGKeys::F21 },
    { 1073741937, MGKeys::F22 },
    { 1073741938, MGKeys::F23 },
    { 1073741939, MGKeys::F24 },
    { 1073741951, MGKeys::VolumeMute },
    { 1073741952, MGKeys::VolumeUp },
    { 1073741953, MGKeys::VolumeDown },
    { 1073742040, MGKeys::OemClear },
    { 1073742044, MGKeys::Decimal },
    { 1073742048, MGKeys::LeftControl },
    { 1073742049, MGKeys::LeftShift },
    { 1073742050, MGKeys::LeftAlt },
    { 1073742051, MGKeys::LeftWindows },
    { 1073742052, MGKeys::RightControl },
    { 1073742053, MGKeys::RightShift },
    { 1073742054, MGKeys::RightAlt },
    { 1073742055, MGKeys::RightWindows },
    { 1073742082, MGKeys::MediaNextTrack },
    { 1073742083, MGKeys::MediaPreviousTrack },
    { 1073742084, MGKeys::MediaStop },
    { 1073742085, MGKeys::MediaPlayPause },
    { 1073742086, MGKeys::VolumeMute },
    { 1073742087, MGKeys::SelectMedia },
    { 1073742089, MGKeys::LaunchMail },
    { 1073742092, MGKeys::BrowserSearch },
    { 1073742093, MGKeys::BrowserHome },
    { 1073742094, MGKeys::BrowserBack },
    { 1073742095, MGKeys::BrowserForward },
    { 1073742096, MGKeys::BrowserStop },
    { 1073742097, MGKeys::BrowserRefresh },
    { 1073742098, MGKeys::BrowserFavorites },
    { 1073742106, MGKeys::Sleep },
};

inline MGKeys ToXNA(SDL_Keycode key)
{
    auto pair = s_keymap.find((int)key);
    if (pair == s_keymap.end())
        return MGKeys::None;

    return pair->second;
}

struct MGP_Window
{
	MGP_Platform* platform = nullptr;

    Uint32 windowId = 0;

	std::string identifier;

    SDL_Window* window = nullptr;
};

struct MGP_Cursor
{
    SDL_Cursor* cursor;
};


MGP_Platform* MGP_Platform_Create(MGGameRunBehavior& behavior)
{
	SDL_Init(
		SDL_INIT_VIDEO |
		SDL_INIT_JOYSTICK |
		SDL_INIT_GAMECONTROLLER |
		SDL_INIT_HAPTIC);

	SDL_DisableScreenSaver();

	SDL_SetHint("SDL_VIDEO_MINIMIZE_ON_FOCUS_LOSS", "0");
	SDL_SetHint("SDL_JOYSTICK_ALLOW_BACKGROUND_EVENTS", "1");

	behavior = MGGameRunBehavior::Synchronous;

	auto platform = new MGP_Platform();
	return platform;
}

void MGP_Platform_Destroy(MGP_Platform* platform)
{
	assert(platform != nullptr);

	// Destroy any active windows that may have been leaked.
	for (auto window : platform->windows)
		MGP_Window_Destroy(window);

	delete platform;
}

const char* MGP_Platform_MakePath(const char* location, const char* path)
{
    assert(location != nullptr);
    assert(path != nullptr);

    size_t length = strlen(path) + 1;
    if (location[0])
        length += strlen(location) + 1;

#if _WIN32
    // Windows requires marshaled strings to be allocated like this.
    char* fpath = (char*)CoTaskMemAlloc(length);    
#else
    char* fpath = (char*)malloc(length);
#endif

    if (location[0])
    {
        strcpy_s(fpath, length, location);
        strcat_s(fpath, length, MG_PATH_SEPARATOR);
        strcat_s(fpath, length, path);
    }
    else
    {
        strcpy_s(fpath, length, path);
    }

    return fpath;
}

void MGP_Platform_BeforeInitialize(MGP_Platform* platform)
{
	assert(platform != nullptr);
}

MGMonoGamePlatform MGP_Platform_GetPlatform()
{
#if MG_VULKAN
    return MGMonoGamePlatform::DesktopVK;
#else
    assert(false);
    return (MGMonoGamePlatform)-1;
#endif
}

MGGraphicsBackend MGP_Platform_GetGraphicsBackend()
{
#if MG_VULKAN
    return MGGraphicsBackend::Vulkan;
#else
    assert(false);
    return (MGGraphicsBackend)-1;
#endif

}

static MGP_Window* MGP_WindowFromId(MGP_Platform* platform, Uint32 windowId)
{
    assert(platform != nullptr);

    for (auto window : platform->windows)
    {
        if (window->windowId == windowId)
            return window;
    }

    return nullptr;
}

static int UTF8ToUnicode(int utf8)
{
    int byte4 = utf8 & 0xFF,
        byte3 = (utf8 >> 8) & 0xFF,
        byte2 = (utf8 >> 16) & 0xFF,
        byte1 = (utf8 >> 24) & 0xFF;

    if (byte1 < 0x80)
        return byte1;
    else if (byte1 < 0xC0)
        return -1;
    else if (byte1 < 0xE0 && byte2 >= 0x80 && byte2 < 0xC0)
        return (byte1 % 0x20) * 0x40 + (byte2 % 0x40);
    else if (byte1 < 0xF0 && byte2 >= 0x80 && byte2 < 0xC0 && byte3 >= 0x80 && byte3 < 0xC0)
        return (byte1 % 0x10) * 0x40 * 0x40 + (byte2 % 0x40) * 0x40 + (byte3 % 0x40);
    else if (byte1 < 0xF8 && byte2 >= 0x80 && byte2 < 0xC0 && byte3 >= 0x80 && byte3 < 0xC0 && byte4 >= 0x80 && byte4 < 0xC0)
        return (byte1 % 0x8) * 0x40 * 0x40 * 0x40 + (byte2 % 0x40) * 0x40 * 0x40 + (byte3 % 0x40) * 0x40 + (byte4 % 0x40);
    else
        return -1;
}

static MGControllerInput FromSDLButton(Uint8 button)
{
    switch (button)
    {
    default:
        return MGControllerInput::INVALID;
    case SDL_CONTROLLER_BUTTON_A:
        return MGControllerInput::A;
    case SDL_CONTROLLER_BUTTON_B:
        return MGControllerInput::B;
    case SDL_CONTROLLER_BUTTON_X:
        return MGControllerInput::X;
    case SDL_CONTROLLER_BUTTON_Y:
        return MGControllerInput::Y;
    case SDL_CONTROLLER_BUTTON_BACK:
        return MGControllerInput::Back;
    case SDL_CONTROLLER_BUTTON_GUIDE:
        return MGControllerInput::Guide;
    case SDL_CONTROLLER_BUTTON_START:
        return MGControllerInput::Start;
    case SDL_CONTROLLER_BUTTON_LEFTSTICK:
        return MGControllerInput::LeftStick;
    case SDL_CONTROLLER_BUTTON_RIGHTSTICK:
        return MGControllerInput::RightStick;
    case SDL_CONTROLLER_BUTTON_LEFTSHOULDER:
        return MGControllerInput::LeftShoulder;
    case SDL_CONTROLLER_BUTTON_RIGHTSHOULDER:
        return MGControllerInput::RightShoulder;
    case SDL_CONTROLLER_BUTTON_DPAD_UP:
        return MGControllerInput::DpadUp;
    case SDL_CONTROLLER_BUTTON_DPAD_DOWN:
        return MGControllerInput::DpadDown;
    case SDL_CONTROLLER_BUTTON_DPAD_LEFT:
        return MGControllerInput::DpadLeft;
    case SDL_CONTROLLER_BUTTON_DPAD_RIGHT:
        return MGControllerInput::DpadRight;
    case SDL_CONTROLLER_BUTTON_MISC1:
        return MGControllerInput::Misc1;
    case SDL_CONTROLLER_BUTTON_PADDLE1:
        return MGControllerInput::Paddle1;
    case SDL_CONTROLLER_BUTTON_PADDLE2:
        return MGControllerInput::Paddle2;
    case SDL_CONTROLLER_BUTTON_PADDLE3:
        return MGControllerInput::Paddle3;
    case SDL_CONTROLLER_BUTTON_PADDLE4:
        return MGControllerInput::Paddle4;
    case SDL_CONTROLLER_BUTTON_TOUCHPAD:
        return MGControllerInput::Touchpad;
    }
}


static MGControllerInput FromSDLAxis(Uint8 axis)
{
    switch (axis)
    {
    default:
        return MGControllerInput::INVALID;
    case SDL_CONTROLLER_AXIS_LEFTX:
        return MGControllerInput::LeftStickX;
    case SDL_CONTROLLER_AXIS_LEFTY:
        return MGControllerInput::LeftStickY;
    case SDL_CONTROLLER_AXIS_RIGHTX:
        return MGControllerInput::RightStickX;
    case SDL_CONTROLLER_AXIS_RIGHTY:
        return MGControllerInput::RightStickY;
    case SDL_CONTROLLER_AXIS_TRIGGERLEFT:
        return MGControllerInput::LeftTrigger;
    case SDL_CONTROLLER_AXIS_TRIGGERRIGHT:
        return MGControllerInput::RightTrigger;
    }
}

mgbool MGP_Platform_PollEvent(MGP_Platform* platform, MGP_Event& event_)
{
	assert(platform != nullptr);

    SDL_Event ev;

    const int MOUSE_WHEEL_DELTA = 120;

    // If we had previous queued events then return those.
    if (platform->queued_events.size() > 0)
    {
        event_ = platform->queued_events.front();
        platform->queued_events.pop();
        return true;
    }

    while (SDL_PollEvent(&ev) == 1)
    {
        // TODO: We cannot call SDL_GetTicks64 as it is different from
        // SDL_GetTicks() and controller and keyboard events return SDL_GetTicks().
        event_.Timestamp = SDL_GetTicks();

        switch (ev.type)
        {
        case SDL_QUIT:
            event_.Type = MGEventType::Quit;
            return true;

        case SDL_EventType::SDL_JOYDEVICEADDED:
            break;
        case SDL_EventType::SDL_JOYDEVICEREMOVED:
            break;

        case SDL_EventType::SDL_CONTROLLERDEVICEADDED:
        {
            auto controller = SDL_GameControllerOpen(ev.cdevice.which);
            platform->controllers.emplace(ev.cdevice.which, controller);
            event_.Type = MGEventType::ControllerAdded;
            event_.Timestamp = ev.cdevice.timestamp;
            event_.Controller.Id = ev.cdevice.which;
            event_.Controller.Input = MGControllerInput::INVALID;
            event_.Controller.Value = 0;
            return true;
        }
        case SDL_EventType::SDL_CONTROLLERDEVICEREMOVED:
        {
            auto controller = platform->controllers[ev.cdevice.which];
            platform->controllers.erase(ev.cdevice.which);
            SDL_GameControllerClose(controller);
            event_.Type = MGEventType::ControllerRemoved;
            event_.Timestamp = ev.cdevice.timestamp;
            event_.Controller.Id = ev.cdevice.which;
            event_.Controller.Input = MGControllerInput::INVALID;
            event_.Controller.Value = 0;
            return true;
        }
        case SDL_EventType::SDL_CONTROLLERBUTTONUP:
            event_.Type = MGEventType::ControllerStateChange;
            event_.Timestamp = ev.cbutton.timestamp;
            event_.Controller.Id = ev.cbutton.which;
            event_.Controller.Input = FromSDLButton(ev.cbutton.button);
            event_.Controller.Value = 0;
            return true;
        case SDL_EventType::SDL_CONTROLLERBUTTONDOWN:
            event_.Type = MGEventType::ControllerStateChange;
            event_.Timestamp = ev.cbutton.timestamp;
            event_.Controller.Id = ev.cbutton.which;
            event_.Controller.Input = FromSDLButton(ev.cbutton.button);
            event_.Controller.Value = 1;
            return true;
        case SDL_EventType::SDL_CONTROLLERAXISMOTION:
            event_.Type = MGEventType::ControllerStateChange;
            event_.Timestamp = ev.caxis.timestamp;
            event_.Controller.Id = ev.caxis.which;
            event_.Controller.Input = FromSDLAxis(ev.caxis.axis);
            event_.Controller.Value = ev.caxis.value;
            return true;
            break;

        case SDL_EventType::SDL_MOUSEMOTION:
            event_.Type = MGEventType::MouseMove;
            event_.MouseMove.Window = MGP_WindowFromId(platform, ev.motion.windowID);
            event_.MouseMove.X = ev.motion.x;
            event_.MouseMove.Y = ev.motion.y;
            return true;

        case SDL_EventType::SDL_MOUSEWHEEL:
            event_.Type = MGEventType::MouseWheel;
            event_.MouseWheel.Window = MGP_WindowFromId(platform, ev.wheel.windowID);
            event_.MouseWheel.Scroll = ev.wheel.y * MOUSE_WHEEL_DELTA;
            event_.MouseWheel.ScrollH = ev.wheel.x * MOUSE_WHEEL_DELTA;
            return true;
        
        case SDL_EventType::SDL_MOUSEBUTTONUP:
        case SDL_EventType::SDL_MOUSEBUTTONDOWN:
            event_.Type = ev.type == SDL_EventType::SDL_MOUSEBUTTONDOWN ? MGEventType::MouseButtonDown : MGEventType::MouseButtonUp;
            event_.MouseButton.Window = MGP_WindowFromId(platform, ev.button.windowID);
            switch (ev.button.button)
            {
                default:
                case SDL_BUTTON_LEFT:
                    event_.MouseButton.Button = MGMouseButton::Left;
                    break;
                case SDL_BUTTON_RIGHT:
                    event_.MouseButton.Button = MGMouseButton::Right;
                    break;
                case SDL_BUTTON_MIDDLE:
                    event_.MouseButton.Button = MGMouseButton::Middle;
                    break;
                case SDL_BUTTON_X1:
                    event_.MouseButton.Button = MGMouseButton::X1;
                    break;
                case SDL_BUTTON_X2:
                    event_.MouseButton.Button = MGMouseButton::X2;
                    break;
            }
            return true;

        case SDL_EventType::SDL_KEYDOWN:
        {
            event_.Type = MGEventType::KeyDown;

            event_.Key.Window = MGP_WindowFromId(platform, ev.key.windowID);
            event_.Key.Character = ev.key.keysym.sym;
            event_.Key.Key = ToXNA(ev.key.keysym.sym);

            return true;
        }
        case SDL_EventType::SDL_KEYUP:
        {
            event_.Type = MGEventType::KeyUp;

            event_.Key.Window = MGP_WindowFromId(platform, ev.key.windowID);
            event_.Key.Character = ev.key.keysym.sym;
            event_.Key.Key = ToXNA(ev.key.keysym.sym);

            return true;
        }

        case SDL_TEXTINPUT:
        {
            event_.Type = MGEventType::TextInput;
            event_.Key.Window = MGP_WindowFromId(platform, ev.text.windowID);

            int len = 0;
            int utf8character = 0; // using an int to encode multibyte characters longer than 2 bytes
            mgbyte currentByte = 0;
            int charByteSize = 0; // UTF8 char length to decode
            int remainingShift = 0;
            while ((currentByte = ev.text.text[len]) != 0)
            {
                // we're reading the first UTF8 byte, we need to check if it's multibyte
                if (charByteSize == 0)
                {
                    if (currentByte < 192)
                        charByteSize = 1;
                    else if (currentByte < 224)
                        charByteSize = 2;
                    else if (currentByte < 240)
                        charByteSize = 3;
                    else
                        charByteSize = 4;

                    utf8character = 0;
                    remainingShift = 4;
                }

                // assembling the character
                utf8character <<= 8;
                utf8character |= currentByte;

                charByteSize--;
                remainingShift--;

                if (charByteSize == 0) // finished decoding the current character
                {
                    utf8character <<= remainingShift * 8; // shifting it to full UTF8 scope

                    // SDL returns UTF8-encoded characters while C# char type is UTF16-encoded (and limited to the 0-FFFF range / does not support surrogate pairs)
                    // so we need to convert it to Unicode codepoint and check if it's within the supported range
                    int codepoint = UTF8ToUnicode(utf8character);
                    if (codepoint >= 0 && codepoint < 0xFFFF)
                    {
                        event_.Key.Character = codepoint;
                        event_.Key.Key = ToXNA(codepoint);

                        platform->queued_events.push(event_);

                        // UTF16 characters beyond 0xFFFF are not supported (and would require a surrogate encoding that is not supported by the char type)
                    }
                }

                len++;
            }

            if (platform->queued_events.size() > 0)
            {
                event_ = platform->queued_events.front();
                platform->queued_events.pop();
                return true;
            }

            break;
        }

        case SDL_WINDOWEVENT:
        {
            event_.Window.Window = MGP_WindowFromId(platform, ev.window.windowID);

            switch (ev.window.event)
            {
            case SDL_WINDOWEVENT_RESIZED:
            case SDL_WINDOWEVENT_SIZE_CHANGED:
                event_.Type = MGEventType::WindowResized;
                event_.Window.Data1 = ev.window.data1;
                event_.Window.Data2 = ev.window.data2;
                return true;
                break;
            case SDL_WINDOWEVENT_FOCUS_GAINED:
                event_.Type = MGEventType::WindowGainedFocus;
                return true;
            case SDL_WINDOWEVENT_FOCUS_LOST:
                event_.Type = MGEventType::WindowLostFocus;
                return true;
            case SDL_WINDOWEVENT_MOVED:
                event_.Type = MGEventType::WindowMoved;
                event_.Window.Data1 = ev.window.data1;
                event_.Window.Data2 = ev.window.data2;
                return true;
            case SDL_WINDOWEVENT_CLOSE:
                event_.Type = MGEventType::WindowClose;
                return true;
            }

            break;
        }

        case SDL_DROPFILE:
        {
            event_.Type = MGEventType::DropFile;
            event_.Drop.Window = MGP_WindowFromId(platform, ev.drop.windowID);

            static char TempPath[_MAX_PATH];
            strcpy_s(TempPath, _MAX_PATH, ev.drop.file);
            SDL_free(ev.drop.file);

            event_.Drop.File = TempPath;
            return true;
        }

        case SDL_DROPCOMPLETE:
            event_.Type = MGEventType::DropComplete;
            event_.Drop.Window = MGP_WindowFromId(platform, ev.drop.windowID);
            event_.Drop.File = nullptr;
            return true;
        }
    }

    return false;
}

mgbool MGP_Platform_BeforeRun(MGP_Platform* platform)
{
	assert(platform != nullptr);
	return true;
}

mgbool MGP_Platform_BeforeUpdate(MGP_Platform* platform)
{
	assert(platform != nullptr);
	return true;
}

mgbool MGP_Platform_BeforeDraw(MGP_Platform* platform)
{
	assert(platform != nullptr);
	return true;
}

MGP_Window* MGP_Window_Create(
    MGP_Platform* platform,
    mgint& width,
    mgint& height,
    const char* title)
{
	assert(platform != nullptr);
    assert(width > 0);
    assert(height > 0);

	auto window = new MGP_Window();
	window->platform = platform;

    // TODO: Why did this start with SDL_WINDOW_FULLSCREEN_DESKTOP in the old C# SDL?
    // We should write coments to document odd behaviors like this.

	Uint32 flags = SDL_WINDOW_HIDDEN;// | SDL_WINDOW_FULLSCREEN_DESKTOP;

#if MG_VULKAN
	flags |= SDL_WINDOW_VULKAN;
#else
	#error Not implemented
#endif

    title = title ? title : "";

	window->window = SDL_CreateWindow(title, SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, width, height, flags);
    window->windowId = SDL_GetWindowID(window->window);

	platform->windows.push_back(window);

	return window;
}

void MGP_Window_Destroy(MGP_Window* window)
{
	assert(window != nullptr);
	assert(window->platform != nullptr);

	assert(window->window != nullptr);
	SDL_DestroyWindow(window->window);

	mg_remove(window->platform->windows, window);
	delete window;
}

void MGP_Window_SetIconBitmap(MGP_Window* window, mgbyte* icon, mgint length)
{
    assert(window != nullptr);

    auto src = SDL_RWFromConstMem(icon, length);
    auto surface = SDL_LoadBMP_RW(src, 1);

    SDL_SetWindowIcon(window->window, surface);
}

void* MGP_Window_GetNativeHandle(MGP_Window* window)
{
	assert(window != nullptr);
	assert(window->window != nullptr);
	return window->window;
}

mgbool MGP_Window_GetAllowUserResizing(MGP_Window* window)
{
	assert(window != nullptr);
	assert(window->window != nullptr);

	auto flags = SDL_GetWindowFlags(window->window);

	if ((flags & SDL_WINDOW_RESIZABLE) != 0)
		return true;

	return false;
}

void MGP_Window_SetAllowUserResizing(MGP_Window* window, mgbool allow)
{
	assert(window != nullptr);
	assert(window->window != nullptr);

	SDL_SetWindowResizable(window->window, allow ? SDL_TRUE : SDL_FALSE);
}

mgbool MGP_Window_GetIsBoderless(MGP_Window* window)
{
	assert(window != nullptr);

	auto flags = SDL_GetWindowFlags(window->window);

	if ((flags & SDL_WINDOW_BORDERLESS) != 0)
		return true;

	return false;
}

void MGP_Window_SetIsBoderless(MGP_Window* window, mgbool borderless)
{
	assert(window != nullptr);

	SDL_SetWindowBordered(window->window, borderless ? SDL_FALSE : SDL_TRUE);
}

void MGP_Window_SetTitle(MGP_Window* window, const char* title)
{
    assert(window != nullptr);

    title = title ? title : "";
    SDL_SetWindowTitle(window->window, title);
}

void MGP_Window_Show(MGP_Window* window, mgbool show)
{
	assert(window != nullptr);

	if (show)
		SDL_ShowWindow(window->window);
	else
		SDL_HideWindow(window->window);
}

void MGP_Window_GetPosition(MGP_Window* window, mgint& x, mgint& y)
{
	assert(window != nullptr);
	SDL_GetWindowPosition(window->window, &x, &y);
}

void MGP_Window_SetPosition(MGP_Window* window, mgint x, mgint y)
{
	assert(window != nullptr);
	SDL_SetWindowPosition(window->window, x, y);
}

void MGP_Window_SetClientSize(MGP_Window* window, mgint width, mgint height)
{
    assert(window != nullptr);
    SDL_SetWindowSize(window->window, width, height);
}

void MGP_Window_SetCursor(MGP_Window* window, MGP_Cursor* cursor)
{
    assert(window != nullptr);
    assert(cursor != nullptr);
    
    SDL_SetCursor(cursor->cursor);
}

void MGP_Window_EnterFullScreen(MGP_Window* window, mgbool useHardwareModeSwitch)
{
    assert(window != nullptr);

    Uint32 flags;
    if (useHardwareModeSwitch)
        flags = SDL_WINDOW_FULLSCREEN;
    else
        flags = SDL_WINDOW_FULLSCREEN_DESKTOP;

    SDL_SetWindowFullscreen(window->window, flags);
}

void MGP_Window_ExitFullScreen(MGP_Window* window)
{
    assert(window != nullptr);
    SDL_SetWindowFullscreen(window->window, 0);
}

mgint MGP_Window_ShowMessageBox(MGP_Window* window, const char* title, const char* description, const char** buttons, mgint count)
{
    SDL_MessageBoxData data;
    data.window = window->window;
    data.title = title;
    data.message = description;
    data.colorScheme = nullptr;
    data.flags = SDL_MESSAGEBOX_BUTTONS_LEFT_TO_RIGHT;

    auto bdata = new SDL_MessageBoxButtonData[count];
    for (int i = 0; i < count; i++)
    {
        bdata[i].buttonid = i;
        bdata[i].text = buttons[i];
        bdata[i].flags = 0;
    }

    data.numbuttons = count;
    data.buttons = bdata;

    int hit = -1;
    int error = SDL_ShowMessageBox(&data, &hit);

    delete[] bdata;

    if (error == 0)
        return hit;

    return -1;
}

void MGP_Mouse_SetVisible(MGP_Platform* platform, mgbool visible)
{
    assert(platform != nullptr);
    SDL_ShowCursor(visible ? SDL_ENABLE : SDL_DISABLE);
}

void MGP_Mouse_WarpPosition(MGP_Window* window, mgint x, mgint y)
{
    assert(window != nullptr);
    SDL_WarpMouseInWindow(window->window, x, y);
}

MGP_Cursor* MGP_Cursor_Create(MGSystemCursor cursor_)
{
    auto cursor = new MGP_Cursor();
    cursor->cursor = SDL_CreateSystemCursor((SDL_SystemCursor)cursor_);
    return cursor;
}

MGP_Cursor* MGP_Cursor_CreateCustom(mgbyte* rgba, mgint width, mgint height, mgint originx, mgint originy)
{
    assert(rgba != nullptr);

    auto cursor = new MGP_Cursor();

    auto surface = SDL_CreateRGBSurfaceFrom(rgba, width, height, 32, width * 4, 0x000000ff, 0x0000FF00, 0x00FF0000, 0xFF000000);
    cursor->cursor = SDL_CreateColorCursor(surface, originx, originy);

    return cursor;
}

void MGP_Cursor_Destroy(MGP_Cursor* cursor)
{
    assert(cursor != nullptr);
    SDL_FreeCursor(cursor->cursor);
    delete cursor;
}

mgint MGP_GamePad_GetMaxSupported()
{
    return 16;
}

inline uint32_t HasSDLButton(SDL_GameController* controller, SDL_GameControllerButton button)
{
    return SDL_GameControllerHasButton(controller, button) ? (1 << (uint32_t)FromSDLButton(button)) : 0;
}

inline uint32_t HasSDLAxis(SDL_GameController* controller, SDL_GameControllerAxis axis)
{
    return SDL_GameControllerHasAxis(controller, axis) ? (1 << (uint32_t)FromSDLAxis(axis)) : 0;
}

void MGP_GamePad_GetCaps(MGP_Platform* platform, mgint identifer, MGP_ControllerCaps* caps)
{
    assert(platform);

    auto pair = platform->controllers.find(identifer);
    if (pair == platform->controllers.end())
    {
        // Not connected or unknown... so nothing to set.
        caps->Identifier = nullptr;
        caps->DisplayName = nullptr;
        caps->GamePadType = MGGamePadType::Unknown;
        caps->InputFlags = 0;
        caps->HasLeftVibrationMotor = false;
        caps->HasRightVibrationMotor = false;
        caps->HasVoiceSupport = false;
        return;
    }

    auto controller = pair->second;

    // This doesn't need to be thread safe, but be valid
    // long enough for the caller to copy it.
    static char identifier[34];
    {
        auto joystick = SDL_GameControllerGetJoystick(controller);
        auto guid = SDL_JoystickGetGUID(joystick);
        SDL_GUIDToString(guid, identifier, 34);
    }

    // Not connected or unknown... so nothing to set.
    caps->Identifier = (void*)identifier;
    caps->DisplayName = (void*)SDL_GameControllerName(controller);
    caps->GamePadType = MGGamePadType::GamePad;
    caps->HasRightVibrationMotor = caps->HasLeftVibrationMotor = SDL_GameControllerHasRumble(controller);
    caps->HasVoiceSupport = false;

    caps->InputFlags = 0;
    caps->InputFlags |= HasSDLButton(controller, SDL_CONTROLLER_BUTTON_A);
    caps->InputFlags |= HasSDLButton(controller, SDL_CONTROLLER_BUTTON_B);
    caps->InputFlags |= HasSDLButton(controller, SDL_CONTROLLER_BUTTON_X);
    caps->InputFlags |= HasSDLButton(controller, SDL_CONTROLLER_BUTTON_Y);
    caps->InputFlags |= HasSDLButton(controller, SDL_CONTROLLER_BUTTON_DPAD_UP);
    caps->InputFlags |= HasSDLButton(controller, SDL_CONTROLLER_BUTTON_DPAD_DOWN);
    caps->InputFlags |= HasSDLButton(controller, SDL_CONTROLLER_BUTTON_DPAD_LEFT);
    caps->InputFlags |= HasSDLButton(controller, SDL_CONTROLLER_BUTTON_DPAD_RIGHT);
    caps->InputFlags |= HasSDLButton(controller, SDL_CONTROLLER_BUTTON_BACK);
    caps->InputFlags |= HasSDLButton(controller, SDL_CONTROLLER_BUTTON_START);
    caps->InputFlags |= HasSDLButton(controller, SDL_CONTROLLER_BUTTON_GUIDE);
    caps->InputFlags |= HasSDLButton(controller, SDL_CONTROLLER_BUTTON_LEFTSHOULDER);
    caps->InputFlags |= HasSDLButton(controller, SDL_CONTROLLER_BUTTON_RIGHTSHOULDER);
    caps->InputFlags |= HasSDLButton(controller, SDL_CONTROLLER_BUTTON_LEFTSTICK);
    caps->InputFlags |= HasSDLButton(controller, SDL_CONTROLLER_BUTTON_RIGHTSTICK);
    caps->InputFlags |= HasSDLAxis(controller, SDL_CONTROLLER_AXIS_TRIGGERLEFT);
    caps->InputFlags |= HasSDLAxis(controller, SDL_CONTROLLER_AXIS_TRIGGERRIGHT);
    caps->InputFlags |= HasSDLAxis(controller, SDL_CONTROLLER_AXIS_LEFTX);
    caps->InputFlags |= HasSDLAxis(controller, SDL_CONTROLLER_AXIS_LEFTY);
    caps->InputFlags |= HasSDLAxis(controller, SDL_CONTROLLER_AXIS_RIGHTX);
    caps->InputFlags |= HasSDLAxis(controller, SDL_CONTROLLER_AXIS_RIGHTY);
}

mgbool MGP_GamePad_SetVibration(MGP_Platform* platform, mgint identifer, mgfloat leftMotor, mgfloat rightMotor, mgfloat leftTrigger, mgfloat rightTrigger)
{
    assert(platform);

    auto pair = platform->controllers.find(identifer);
    if (pair == platform->controllers.end())
        return false;

    auto supported = SDL_GameControllerRumble(pair->second, (UINT16)(leftMotor * 0xFFFF), (UINT16)(rightMotor * 0xFFFF), INT_MAX);
    return supported == 0;
}

