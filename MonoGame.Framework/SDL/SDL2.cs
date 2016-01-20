// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Text;
using System.Runtime.InteropServices;

internal class SDL
{
    private const string nativeLibName = "SDL2.dll";

    public const uint   SDL_INIT_VIDEO          = 0x00000020;
    public const int    SDL_INIT_JOYSTICK       = 0x00000200;
    public const uint   SDL_INIT_HAPTIC         = 0x00001000;
    public const uint   SDL_INIT_GAMECONTROLLER = 0x00002000;

    public const ushort SDL_HAPTIC_LEFTRIGHT    = 1 << 2;
    public const uint   SDL_HAPTIC_INFINITY     = uint.MaxValue;

    public const int    SDL_WINDOWPOS_CENTERED  = 0x2FFF0000;

    public const int    SDL_BUTTON_LMASK        = 1 << 0;
    public const int    SDL_BUTTON_MMASK        = 1 << 1;
    public const int    SDL_BUTTON_RMASK        = 1 << 2;
    public const int    SDL_BUTTON_X1MASK       = 1 << 3;
    public const int    SDL_BUTTON_X2MASK       = 1 << 4;

    private unsafe static string SDL_GetString(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            return "";
        
        var ptr = (byte*)handle;
        while (*ptr != 0)
            ptr++;
        
        var bytes = new byte[ptr - (byte*)handle];
        Marshal.Copy(handle, bytes, 0, bytes.Length);

        return Encoding.UTF8.GetString(bytes);
    }

    public enum SDL_WindowEventID : byte
    {
        SDL_WINDOWEVENT_NONE,
        SDL_WINDOWEVENT_SHOWN,
        SDL_WINDOWEVENT_HIDDEN,
        SDL_WINDOWEVENT_EXPOSED,
        SDL_WINDOWEVENT_MOVED,
        SDL_WINDOWEVENT_RESIZED,
        SDL_WINDOWEVENT_SIZE_CHANGED,
        SDL_WINDOWEVENT_MINIMIZED,
        SDL_WINDOWEVENT_MAXIMIZED,
        SDL_WINDOWEVENT_RESTORED,
        SDL_WINDOWEVENT_ENTER,
        SDL_WINDOWEVENT_LEAVE,
        SDL_WINDOWEVENT_FOCUS_GAINED,
        SDL_WINDOWEVENT_FOCUS_LOST,
        SDL_WINDOWEVENT_CLOSE,
    }

    public enum SDL_EventType
    {
        SDL_QUIT             = 0x100,
        SDL_WINDOWEVENT      = 0x200,
        SDL_KEYDOWN          = 0x300,
        SDL_KEYUP            = 0x301,
        SDL_TEXTINPUT        = 0x303,
        SDL_JOYDEVICEADDED   = 0x605,
        SDL_JOYDEVICEREMOVED = 0x606,
        SDL_MOUSEWHEEL       = 0x403,
    }
 
    public enum SDL_HAT
    {
        SDL_HAT_CENTERED  = 0,
        SDL_HAT_UP        = 1,
        SDL_HAT_RIGHT     = 2,
        SDL_HAT_RIGHTUP   = 3,
        SDL_HAT_DOWN      = 4,
        SDL_HAT_RIGHTDOWN = 6,
        SDL_HAT_LEFT      = 8,
        SDL_HAT_LEFTUP    = 9,
        SDL_HAT_LEFTDOWN  = 12,
    }

    public enum SDL_GameControllerButton
    {
        SDL_CONTROLLER_BUTTON_INVALID = -1,
        SDL_CONTROLLER_BUTTON_A,
        SDL_CONTROLLER_BUTTON_B,
        SDL_CONTROLLER_BUTTON_X,
        SDL_CONTROLLER_BUTTON_Y,
        SDL_CONTROLLER_BUTTON_BACK,
        SDL_CONTROLLER_BUTTON_GUIDE,
        SDL_CONTROLLER_BUTTON_START,
        SDL_CONTROLLER_BUTTON_LEFTSTICK,
        SDL_CONTROLLER_BUTTON_RIGHTSTICK,
        SDL_CONTROLLER_BUTTON_LEFTSHOULDER,
        SDL_CONTROLLER_BUTTON_RIGHTSHOULDER,
        SDL_CONTROLLER_BUTTON_DPAD_UP,
        SDL_CONTROLLER_BUTTON_DPAD_DOWN,
        SDL_CONTROLLER_BUTTON_DPAD_LEFT,
        SDL_CONTROLLER_BUTTON_DPAD_RIGHT,
        SDL_CONTROLLER_BUTTON_MAX,
    }

    public enum SDL_GameControllerAxis
    {
        SDL_CONTROLLER_AXIS_INVALID = -1,
        SDL_CONTROLLER_AXIS_LEFTX,
        SDL_CONTROLLER_AXIS_LEFTY,
        SDL_CONTROLLER_AXIS_RIGHTX,
        SDL_CONTROLLER_AXIS_RIGHTY,
        SDL_CONTROLLER_AXIS_TRIGGERLEFT,
        SDL_CONTROLLER_AXIS_TRIGGERRIGHT,
        SDL_CONTROLLER_AXIS_MAX,
    }

    [Flags]
    public enum SDL_WindowFlags
    {
        SDL_WINDOW_FULLSCREEN         = 0x00000001,
        SDL_WINDOW_OPENGL             = 0x00000002,
        SDL_WINDOW_SHOWN              = 0x00000004,
        SDL_WINDOW_HIDDEN             = 0x00000008,
        SDL_WINDOW_BORDERLESS         = 0x00000010,
        SDL_WINDOW_RESIZABLE          = 0x00000020,
        SDL_WINDOW_INPUT_FOCUS        = 0x00000200,
        SDL_WINDOW_MOUSE_FOCUS        = 0x00000400,
        SDL_WINDOW_FULLSCREEN_DESKTOP = 0x00001001,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_WindowEvent
    {
        public SDL_EventType type;
        public UInt32 timestamp;
        public UInt32 windowID;
        public SDL_WindowEventID windowEvent;
        private byte padding1;
        private byte padding2;
        private byte padding3;
        public Int32 data1;
        public Int32 data2;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_MouseWheelEvent
    {
        public SDL_EventType type;
        public uint timestamp;
        public uint windowID;
        public uint which;
        public int x;
        public int y;
        public uint direction;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_JoyDeviceEvent
    {
        public SDL_EventType type;
        public UInt32 timestamp;
        public Int32 which;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_KeyboardEvent
    {
        public SDL_EventType type;
        public UInt32 timestamp;
        public UInt32 windowID;
        public byte state;
        public byte repeat;
        private byte padding2;
        private byte padding3;
        public SDL_Keysym keysym;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_Keysym
    {
        public int scancode;
        public int sym;
        public short mod;
        public int unicode;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct SDL_Event
    {
        [FieldOffset(0)]
        public SDL_EventType type;
        [FieldOffset(0)]
        public SDL_WindowEvent window;
        [FieldOffset(0)]
        public SDL_KeyboardEvent key;
        [FieldOffset(0)]
        public SDL_MouseWheelEvent wheel;
        [FieldOffset(0)]
        public SDL_JoyDeviceEvent jdevice;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_HapticLeftRight
    {
        public ushort type;
        public uint length;
        public ushort large_magnitude;
        public ushort small_magnitude;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct SDL_HapticEffect
    {
        [FieldOffset(0)]
        public ushort type;
        [FieldOffset(0)]
        public SDL_HapticLeftRight leftright;
    }

    // Main

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int       SDL_Init(int flags);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int       SDL_PollEvent(out SDL_Event _event);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetError")]
    private static extern IntPtr __SDL_GetError();

    public static string SDL_GetError()
    {
        return SDL_GetString(__SDL_GetError());
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void      SDL_Quit();

    // Window

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr    SDL_CreateWindow(string title, int x, int y, int w, int h, SDL_WindowFlags flags);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void      SDL_DestroyWindow(IntPtr window);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void      SDL_DisableScreenSaver();

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void      SDL_GetWindowPosition(IntPtr window, out int x, out int y);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void      SDL_GetWindowSize(IntPtr window, out int w, out int h);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void      SDL_SetWindowBordered(IntPtr window, int bordered);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int       SDL_SetWindowFullscreen(IntPtr window, SDL_WindowFlags flags);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void      SDL_SetWindowPosition(IntPtr window, int x, int y);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void      SDL_SetWindowSize(IntPtr window, int w, int h);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void      SDL_SetWindowTitle(IntPtr window, string title);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void      SDL_ShowWindow(IntPtr window);

    // Mouse

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int       SDL_GetGlobalMouseState(out int x, out int y);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int       SDL_GetMouseState(out int x, out int y);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int       SDL_ShowCursor(int toggle);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void      SDL_WarpMouseInWindow(IntPtr window, int x, int y);

    // Joystick

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr    SDL_JoystickOpen(int device_index);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void      SDL_JoystickClose(IntPtr joystick);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern Guid      SDL_JoystickGetGUID(IntPtr joystick);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern short     SDL_JoystickGetAxis(IntPtr joystick, int axis);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern byte      SDL_JoystickGetButton(IntPtr joystick, int button);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SDL_HAT   SDL_JoystickGetHat(IntPtr joystick, int hat);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int       SDL_JoystickNumAxes(IntPtr joystick);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int       SDL_JoystickNumButtons(IntPtr joystick);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int       SDL_JoystickNumHats(IntPtr joystick);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int       SDL_NumJoysticks();

    // GameController

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr    SDL_GameControllerOpen(int joystick_index);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GameControllerName")]
    private static extern IntPtr __SDL_GameControllerName(IntPtr gamecontroller);

    public static string SDL_GameControllerName(IntPtr gamecontroller)
    {
        return SDL_GetString(__SDL_GameControllerName(gamecontroller));
    }

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int       SDL_GameControllerAddMapping(string mappingString);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr    SDL_GameControllerGetJoystick(IntPtr gamecontroller);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern byte      SDL_GameControllerGetButton(IntPtr gamecontroller, SDL_GameControllerButton button);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern short     SDL_GameControllerGetAxis(IntPtr gamecontroller, SDL_GameControllerAxis axis);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern byte      SDL_IsGameController(int joystick_index);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void      SDL_GameControllerClose(IntPtr gamecontroller);

    // Haptic (aka. Vibrations for Joystick and GameController)

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int       SDL_JoystickIsHaptic(IntPtr joystick);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr    SDL_HapticOpenFromJoystick(IntPtr joystick);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int       SDL_HapticRumbleInit(IntPtr haptic);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int       SDL_HapticRumbleSupported(IntPtr haptic);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int       SDL_HapticRumblePlay(IntPtr haptic, float strength, uint length);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int       SDL_HapticEffectSupported(IntPtr haptic, ref SDL_HapticEffect effect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int       SDL_HapticNewEffect(IntPtr haptic, ref SDL_HapticEffect effect);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int       SDL_HapticUpdateEffect(IntPtr haptic, int effect, ref SDL_HapticEffect data);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int       SDL_HapticRunEffect(IntPtr haptic, int effect, uint iterations);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int       SDL_HapticStopAll(IntPtr haptic);

    [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void      SDL_HapticClose(IntPtr haptic);
}
