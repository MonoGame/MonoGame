// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;

namespace MonoGame.Utilities
{
    public class SDL
    {
        private const string nativeLibName = "SDL2.dll";

        public const int SDL_INIT_JOYSTICK = 0x00000200;

        internal enum SDL_WindowEventID : byte
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

        internal enum SDL_EventType
        {
            SDL_QUIT             = 0x100,
            SDL_WINDOWEVENT      = 0x200,
            SDL_JOYAXISMOTION    = 0x600,
            SDL_JOYBALLMOTION    = 0x601,
            SDL_JOYHATMOTION     = 0x602,
            SDL_JOYBUTTONDOWN    = 0x603,
            SDL_JOYBUTTONUP      = 0x604,
            SDL_JOYDEVICEADDED   = 0x605,
            SDL_JOYDEVICEREMOVED = 0x606,
        }

        internal enum SDL_HAT
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

        internal enum SDL_Scancode
        {
            SDL_SCANCODE_UNKNOWN = 0
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SDL_WindowEvent
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
        internal struct SDL_JoyDeviceEvent
        {
            public SDL_EventType type;
            public UInt32 timestamp;
            public Int32 which;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct SDL_Event
        {
            [FieldOffset(0)]
            public SDL_EventType type;
            [FieldOffset(0)]
            public SDL_WindowEvent window;
            [FieldOffset(0)]
            public SDL_JoyDeviceEvent jdevice;
        }

        // Main

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int     SDL_Init(int flags);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int     SDL_PollEvent(out SDL_Event _event);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern string  SDL_GetError();

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void      SDL_Quit();

        // Joystick

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr  SDL_JoystickOpen(int device_index);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void      SDL_JoystickClose(IntPtr joystick);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern Guid    SDL_JoystickGetGUID(IntPtr joystick);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern Int16   SDL_JoystickGetAxis(IntPtr joystick, int axis);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern byte    SDL_JoystickGetButton(IntPtr joystick, int button);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern SDL_HAT SDL_JoystickGetHat(IntPtr joystick, int hat);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int     SDL_JoystickNumAxes(IntPtr joystick);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int     SDL_JoystickNumButtons(IntPtr joystick);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int     SDL_JoystickNumHats(IntPtr joystick);

        [DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int     SDL_NumJoysticks();
    }
}

