// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

internal static class Sdl
{
    public const string NativeLibName = "SDL2.dll";

    public static int Major;
    public static int Minor;
    public static int Patch;

    private static unsafe string GetString(IntPtr handle)
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

    [Flags]
    public enum InitFlags
    {
        Video = 0x00000020,
        Joystick = 0x00000200,
        Haptic = 0x00001000,
        GameController = 0x00002000,
    }

    public enum EventType : uint
    {
        First = 0,

        Quit = 0x100,

        WindowEvent = 0x200,
        SysWM = 0x201,

        KeyDown = 0x300,
        KeyUp = 0x301,
        TextEditing = 0x302,
        TextInput = 0x303,

        MouseMotion = 0x400,
        MouseButtonDown = 0x401,
        MouseButtonup = 0x402,
        MouseWheel = 0x403,

        JoyAxisMotion = 0x600,
        JoyBallMotion = 0x601,
        JoyHatMotion = 0x602,
        JoyButtonDown = 0x603,
        JoyButtonUp = 0x604,
        JoyDeviceAdded = 0x605,
        JoyDeviceRemoved = 0x606,

        ControllerAxisMotion = 0x650,
        ControllerButtonDown = 0x651,
        ControllerButtonUp = 0x652,
        ControllerDeviceAdded = 0x653,
        ControllerDeviceRemoved = 0x654,
        ControllerDeviceRemapped = 0x654,

        FingerDown = 0x700,
        FingerUp = 0x701,
        FingerMotion = 0x702,

        DollarGesture = 0x800,
        DollarRecord = 0x801,
        MultiGesture = 0x802,

        ClipboardUpdate = 0x900,

        DropFile = 0x1000,

        AudioDeviceAdded = 0x1100,
        AudioDeviceRemoved = 0x1101,

        RenderTargetsReset = 0x2000,
        RenderDeviceReset = 0x2001,

        UserEvent = 0x8000,

        Last = 0xFFFF
    }
    
    public enum EventAction
    {
        AddEvent = 0x0,
        PeekEvent = 0x1,
        GetEvent = 0x2,
    }

    [StructLayout(LayoutKind.Explicit, Size = 56)]
    public struct Event
    {
        [FieldOffset(0)]
        public EventType Type;
        [FieldOffset(0)]
        public Window.Event Window;
        [FieldOffset(0)]
        public Keyboard.Event Key;
        [FieldOffset(0)]
        public Mouse.MotionEvent Motion;
        [FieldOffset(0)]
        public Keyboard.TextEditingEvent Edit;
        [FieldOffset(0)]
        public Keyboard.TextInputEvent Text;
        [FieldOffset(0)]
        public Mouse.WheelEvent Wheel;
        [FieldOffset(0)]
        public Joystick.DeviceEvent JoystickDevice;
        [FieldOffset(0)]
        public GameController.DeviceEvent ControllerDevice;
    }

    public struct Rectangle
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
    }

    public struct Version
    {
        public byte Major;
        public byte Minor;
        public byte Patch;
    }

    [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_Init")]
    public static extern int SDL_Init(int flags);

    public static void Init(int flags)
    {
        GetError(SDL_Init(flags));
    }

    [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_DisableScreenSaver")]
    public static extern void DisableScreenSaver();

    [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetVersion")]
    public static extern void GetVersion(out Version version);

    [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_PollEvent")]
    public static extern int PollEvent([Out] out Event _event);

    [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_PumpEvents")]
    public static extern int PumpEvents();

    [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_PeepEvents")]
    public static extern int PeepEvents(
        [Out()] [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct, SizeParamIndex = 1)]
        Event[] events,
        int numevents, 
        EventAction action,
        EventType minType,
        EventType maxType);

    [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_CreateRGBSurfaceFrom")]
    private static extern IntPtr SDL_CreateRGBSurfaceFrom(IntPtr pixels, int width, int height, int depth, int pitch, uint rMask, uint gMask, uint bMask, uint aMask);
    public static IntPtr CreateRGBSurfaceFrom(byte[] pixels, int width, int height, int depth, int pitch, uint rMask, uint gMask, uint bMask, uint aMask)
    {
        var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        try
        {
            return SDL_CreateRGBSurfaceFrom(handle.AddrOfPinnedObject(), width, height, depth, pitch, rMask, gMask, bMask, aMask);
        }
        finally
        {
            handle.Free();
        }
    }

    [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_FreeSurface")]
    public static extern void FreeSurface(IntPtr surface);

    [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetError")]
    private static extern IntPtr SDL_GetError();

    public static string GetError()
    {
        return GetString(SDL_GetError());
    }

    public static int GetError(int value)
    {
        if (value < 0)
            Debug.WriteLine(GetError());

        return value;
    }

    public static IntPtr GetError(IntPtr pointer)
    {
        if (pointer == IntPtr.Zero)
            Debug.WriteLine(GetError());

        return pointer;
    }

    [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_ClearError")]
    public static extern void ClearError();

    [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetHint")]
    public static extern IntPtr SDL_GetHint(string name);

    public static string GetHint(string name)
    {
        return GetString(SDL_GetHint(name));
    }

    [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_LoadBMP_RW")]
    private static extern IntPtr SDL_LoadBMP_RW(IntPtr src, int freesrc);

    public static IntPtr LoadBMP_RW(IntPtr src, int freesrc)
    {
        return GetError(SDL_LoadBMP_RW(src, freesrc));
    }

    [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_Quit")]
    public static extern void Quit();

    [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_RWFromMem")]
    private static extern IntPtr SDL_RWFromMem(byte[] mem, int size);

    public static IntPtr RwFromMem(byte[] mem, int size)
    {
        return GetError(SDL_RWFromMem(mem, size));
    }

    [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_SetHint")]
    public static extern int SetHint(string name, string value);

    public static class Window
    {
        public const int PosUndefined = 0x1FFF0000;
        public const int PosCentered = 0x2FFF0000;

        public enum EventId : byte
        {
            None,
            Shown,
            Hidden,
            Exposed,
            Moved,
            Resized,
            SizeChanged,
            Minimized,
            Maximized,
            Restored,
            Enter,
            Leave,
            FocusGained,
            FocusLost,
            Close,
        }

        public static class State
        {
            public const int Fullscreen = 0x00000001;
            public const int OpenGL = 0x00000002;
            public const int Shown = 0x00000004;
            public const int Hidden = 0x00000008;
            public const int Borderless = 0x00000010;
            public const int Resizable = 0x00000020;
            public const int Minimized = 0x00000040;
            public const int Maximized = 0x00000080;
            public const int Grabbed = 0x00000100;
            public const int InputFocus = 0x00000200;
            public const int MouseFocus = 0x00000400;
            public const int FullscreenDesktop = 0x00001001;
            public const int Foreign = 0x00000800;
            public const int AllowHighDPI = 0x00002000;
            public const int MouseCapture = 0x00004000;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Event
        {
            public EventType Type;
            public uint TimeStamp;
            public uint WindowID;
            public EventId EventID;
            private byte padding1;
            private byte padding2;
            private byte padding3;
            public int Data1;
            public int Data2;
        }

        public enum SysWMType
        {
            Unknow,
            Windows,
            X11,
            Directfb,
            Cocoa,
            UiKit,
            Wayland,
            Mir,
            WinRt,
            Android
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SDL_SysWMinfo
        {
            public Version version;
            public SysWMType subsystem;
            public IntPtr window;
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_CreateWindow")]
        private static extern IntPtr SDL_CreateWindow(string title, int x, int y, int w, int h, int flags);

        public static IntPtr Create(string title, int x, int y, int w, int h, int flags)
        {
            return GetError(SDL_CreateWindow(title, x, y, w, h, flags));
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_DestroyWindow")]
        public static extern void Destroy(IntPtr window);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetWindowDisplayIndex")
        ]
        private static extern int SDL_GetWindowDisplayIndex(IntPtr window);

        public static int GetDisplayIndex(IntPtr window)
        {
            return GetError(SDL_GetWindowDisplayIndex(window));
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetWindowFlags")]
        public static extern int GetWindowFlags(IntPtr window);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_SetWindowIcon")]
        public static extern void SetIcon(IntPtr window, IntPtr icon);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetWindowPosition")]
        public static extern void GetPosition(IntPtr window, out int x, out int y);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetWindowSize")]
        public static extern void GetSize(IntPtr window, out int w, out int h);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_SetWindowBordered")]
        public static extern void SetBordered(IntPtr window, int bordered);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_SetWindowFullscreen")]
        private static extern int SDL_SetWindowFullscreen(IntPtr window, int flags);

        public static void SetFullscreen(IntPtr window, int flags)
        {
            GetError(SDL_SetWindowFullscreen(window, flags));
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_SetWindowPosition")]
        public static extern void SetPosition(IntPtr window, int x, int y);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_SetWindowResizable")]
        public static extern void SetResizable(IntPtr window, bool resizable);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_SetWindowSize")]
        public static extern void SetSize(IntPtr window, int w, int h);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_SetWindowTitle")]
        public static extern void SetTitle(IntPtr window, string title);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_ShowWindow")]
        public static extern void Show(IntPtr window);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetWindowWMInfo")]
        public static extern bool GetWindowWMInfo(IntPtr window, ref SDL_SysWMinfo sysWMinfo);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetWindowBordersSize")]
        public static extern int GetBorderSize(IntPtr window, out int top, out int left, out int right, out int bottom);
    }

    public static class Display
    {
        public struct Mode
        {
            public uint Format;
            public int Width;
            public int Height;
            public int RefreshRate;
            public IntPtr DriverData;
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetDisplayBounds")]
        private static extern int SDL_GetDisplayBounds(int displayIndex, out Rectangle rect);

        public static void GetBounds(int displayIndex, out Rectangle rect)
        {
            GetError(SDL_GetDisplayBounds(displayIndex, out rect));
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetCurrentDisplayMode")
        ]
        private static extern int SDL_GetCurrentDisplayMode(int displayIndex, out Mode mode);

        public static void GetCurrentDisplayMode(int displayIndex, out Mode mode)
        {
            GetError(SDL_GetCurrentDisplayMode(displayIndex, out mode));
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetDisplayMode")]
        private static extern int SDL_GetDisplayMode(int displayIndex, int modeIndex, out Mode mode);

        public static void GetDisplayMode(int displayIndex, int modeIndex, out Mode mode)
        {
            GetError(SDL_GetDisplayMode(displayIndex, modeIndex, out mode));
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetClosestDisplayMode")]
        private static extern int SDL_GetClosestDisplayMode(int displayIndex, Mode mode, out Mode closest);

        public static void GetClosestDisplayMode(int displayIndex, Mode mode, out Mode closest)
        {
            GetError(SDL_GetClosestDisplayMode(displayIndex, mode, out closest));
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetDisplayName")]
        private static extern IntPtr SDL_GetDisplayName(int index);

        public static string GetDisplayName(int index)
        {
            return GetString(GetError(SDL_GetDisplayName(index)));
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetNumDisplayModes")]
        private static extern int SDL_GetNumDisplayModes(int displayIndex);

        public static int GetNumDisplayModes(int displayIndex)
        {
            return GetError(SDL_GetNumDisplayModes(displayIndex));
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetNumVideoDisplays")]
        private static extern int SDL_GetNumVideoDisplays();

        public static int GetNumVideoDisplays()
        {
            return GetError(SDL_GetNumVideoDisplays());
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetWindowDisplayIndex")]
        private static extern int SDL_GetWindowDisplayIndex(IntPtr window);

        public static int GetWindowDisplayIndex(IntPtr window)
        {
            return GetError(SDL_GetWindowDisplayIndex(window));
        }
    }

    public static class GL
    {
        public enum Attribute
        {
            RedSize,
            GreenSize,
            BlueSize,
            AlphaSize,
            BufferSize,
            DoubleBuffer,
            DepthSize,
            StencilSize,
            AccumRedSize,
            AccumGreenSize,
            AccumBlueSize,
            AccumAlphaSize,
            Stereo,
            MultiSampleBuffers,
            MultiSampleSamples,
            AcceleratedVisual,
            RetainedBacking,
            ContextMajorVersion,
            ContextMinorVersion,
            ContextEgl,
            ContextFlags,
            ContextProfileMAsl,
            ShareWithCurrentContext,
            FramebufferSRGBCapable,
            ContextReleaseBehaviour,
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GL_CreateContext", ExactSpelling = true)]
        private static extern IntPtr SDL_GL_CreateContext(IntPtr window);

        public static IntPtr CreateContext(IntPtr window)
        {
            return GetError(SDL_GL_CreateContext(window));
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GL_DeleteContext", ExactSpelling = true)]
        public static extern void DeleteContext(IntPtr context);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GL_GetCurrentContext", ExactSpelling = true)]
        private static extern IntPtr SDL_GL_GetCurrentContext();

        public static IntPtr GetCurrentContext()
        {
            return GetError(SDL_GL_GetCurrentContext());
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GL_GetSwapInterval", ExactSpelling = true)]
        public static extern int GetSwapInterval();

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GL_MakeCurrent", ExactSpelling = true)]
        public static extern int MakeCurrent(IntPtr window, IntPtr context);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GL_SetAttribute", ExactSpelling = true)]
        private static extern int SDL_GL_SetAttribute(Attribute attr, int value);

        public static int SetAttribute(Attribute attr, int value)
        {
            return GetError(SDL_GL_SetAttribute(attr, value));
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GL_SetSwapInterval", ExactSpelling = true)]
        public static extern int SetSwapInterval(int interval);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GL_SwapWindow", ExactSpelling = true)]
        public static extern void SwapWindow(IntPtr window);
    }

    public static class Mouse
    {
        [Flags]
        public enum Button
        {
            Left = 1 << 0,
            Middle = 1 << 1,
            Right = 1 << 2,
            X1Mask = 1 << 3,
            X2Mask = 1 << 4
        }

        public enum SystemCursor
        {
            Arrow,
            IBeam,
            Wait,
            Crosshair,
            WaitArrow,
            SizeNWSE,
            SizeNESW,
            SizeWE,
            SizeNS,
            SizeAll,
            No,
            Hand
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MotionEvent
        {
            public EventType Type;
            public uint Timestamp;
            public uint WindowID;
            public uint Which;
            public byte State;
            private byte _padding1;
            private byte _padding2;
            private byte _padding3;
            public int X;
            public int Y;
            public int Xrel;
            public int Yrel;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WheelEvent
        {
            public EventType Type;
            public uint TimeStamp;
            public uint WindowId;
            public uint Which;
            public int X;
            public int Y;
            public uint Direction;
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_CreateColorCursor")]
        private static extern IntPtr SDL_CreateColorCursor(IntPtr surface, int x, int y);

        public static IntPtr CreateColorCursor(IntPtr surface, int x, int y)
        {
            return GetError(SDL_CreateColorCursor(surface, x, y));
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_CreateSystemCursor")]
        private static extern IntPtr SDL_CreateSystemCursor(SystemCursor id);

        public static IntPtr CreateSystemCursor(SystemCursor id)
        {
            return GetError(SDL_CreateSystemCursor(id));
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_FreeCursor")]
        public static extern void FreeCursor(IntPtr cursor);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetGlobalMouseState")]
        public static extern Button GetGlobalState(out int x, out int y);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetMouseState")]
        public static extern Button GetState(out int x, out int y);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_SetCursor")]
        public static extern void SetCursor(IntPtr cursor);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_ShowCursor")]
        public static extern int ShowCursor(int toggle);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_WarpMouseInWindow")]
        public static extern void WarpInWindow(IntPtr window, int x, int y);
    }

    public static class Keyboard
    {
        public struct Keysym
        {
            public int Scancode;
            public int Sym;
            public Keymod Mod;
            public uint Unicode;
        }

        [Flags]
        public enum Keymod : ushort
        {
            None = 0x0000,
            LeftShift = 0x0001,
            RightShift = 0x0002,
            LeftCtrl = 0x0040,
            RightCtrl = 0x0080,
            LeftAlt = 0x0100,
            RightAlt = 0x0200,
            LeftGui = 0x0400,
            RightGui = 0x0800,
            NumLock = 0x1000,
            CapsLock = 0x2000,
            AltGr = 0x4000,
            Reserved = 0x8000,
            Ctrl = (LeftCtrl | RightCtrl),
            Shift = (LeftShift | RightShift),
            Alt = (LeftAlt | RightAlt),
            Gui = (LeftGui | RightGui)
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Event
        {
            public EventType Type;
            public uint TimeStamp;
            public uint WindowId;
            public byte State;
            public byte Repeat;
            private byte padding2;
            private byte padding3;
            public Keysym Keysym;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct TextEditingEvent
        {
            public EventType Type;
            public uint Timestamp;
            public uint WindowId;
            public fixed byte Text [32];
            public int Start;
            public int Length;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct TextInputEvent
        {
            public EventType Type;
            public uint Timestamp;
            public uint WindowId;
            public fixed byte Text [32];
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GetModState")]
        public static extern Keymod GetModState();
    }

    public static class Joystick
    {
        [Flags]
        public enum Hat : byte
        {
            Centered = 0,
            Up = 1 << 0,
            Right = 1 << 1,
            Down = 1 << 2,
            Left = 1 << 3
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DeviceEvent
        {
            public EventType Type;
            public uint TimeStamp;
            public int Which;
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_JoystickClose")]
        public static extern void Close(IntPtr joystick);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_JoystickFromInstanceID")]
        private static extern IntPtr SDL_JoystickFromInstanceID(int joyid);

        public static IntPtr FromInstanceID(int joyid)
        {
            return GetError(SDL_JoystickFromInstanceID(joyid));
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_JoystickGetAxis")]
        public static extern short GetAxis(IntPtr joystick, int axis);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_JoystickGetButton")]
        public static extern byte GetButton(IntPtr joystick, int button);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_JoystickGetGUID")]
        public static extern Guid GetGUID(IntPtr joystick);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_JoystickGetHat")]
        public static extern Hat GetHat(IntPtr joystick, int hat);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_JoystickInstanceID")]
        public static extern int InstanceID(IntPtr joystick);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_JoystickOpen")]
        private static extern IntPtr SDL_JoystickOpen(int deviceIndex);

        public static IntPtr Open(int deviceIndex)
        {
            return GetError(SDL_JoystickOpen(deviceIndex));
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_JoystickNumAxes")]
        private static extern int SDL_JoystickNumAxes(IntPtr joystick);

        public static int NumAxes(IntPtr joystick)
        {
            return GetError(SDL_JoystickNumAxes(joystick));
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_JoystickNumButtons")]
        private static extern int SDL_JoystickNumButtons(IntPtr joystick);

        public static int NumButtons(IntPtr joystick)
        {
            return GetError(SDL_JoystickNumButtons(joystick));
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_JoystickNumHats")]
        private static extern int SDL_JoystickNumHats(IntPtr joystick);

        public static int NumHats(IntPtr joystick)
        {
            return GetError(SDL_JoystickNumHats(joystick));
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_NumJoysticks")]
        private static extern int SDL_NumJoysticks();

        public static int NumJoysticks()
        {
            return GetError(SDL_NumJoysticks());
        }
    }

    public static class GameController
    {
        public enum Axis
        {
            Invalid = -1,
            LeftX,
            LeftY,
            RightX,
            RightY,
            TriggerLeft,
            TriggerRight,
            Max,
        }

        public enum Button
        {
            Invalid = -1,
            A,
            B,
            X,
            Y,
            Back,
            Guide,
            Start,
            LeftStick,
            RightStick,
            LeftShoulder,
            RightShoulder,
            DpadUp,
            DpadDown,
            DpadLeft,
            DpadRight,
            Max,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DeviceEvent
        {
            public EventType Type;
            public uint TimeStamp;
            public int Which;
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GameControllerAddMapping")]
        public static extern int AddMapping(string mappingString);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GameControllerAddMappingsFromRW")]
        public static extern int AddMappingFromRw(IntPtr rw, int freew);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GameControllerClose")]
        public static extern void Close(IntPtr gamecontroller);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_JoystickFromInstanceID")]
        private static extern IntPtr SDL_GameControllerFromInstanceID(int joyid);

        public static IntPtr FromInstanceID(int joyid)
        {
            return GetError(SDL_GameControllerFromInstanceID(joyid));
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GameControllerGetAxis")]
        public static extern short GetAxis(IntPtr gamecontroller, Axis axis);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "SDL_GameControllerGetButton")]
        public static extern byte GetButton(IntPtr gamecontroller, Button button);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "SDL_GameControllerGetJoystick")]
        private static extern IntPtr SDL_GameControllerGetJoystick(IntPtr gamecontroller);

        public static IntPtr GetJoystick(IntPtr gamecontroller)
        {
            return GetError(SDL_GameControllerGetJoystick(gamecontroller));
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_IsGameController")]
        public static extern byte IsGameController(int joystickIndex);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GameControllerMapping")]
        private static extern IntPtr SDL_GameControllerMapping(IntPtr gamecontroller);

        public static string GetMapping(IntPtr gamecontroller)
        {
            return GetString(SDL_GameControllerMapping(gamecontroller));
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GameControllerOpen")]
        private static extern IntPtr SDL_GameControllerOpen(int joystickIndex);

        public static IntPtr Open(int joystickIndex)
        {
            return GetError(SDL_GameControllerOpen(joystickIndex));
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_GameControllerName")]
        private static extern IntPtr SDL_GameControllerName(IntPtr gamecontroller);

        public static string GetName(IntPtr gamecontroller)
        {
            return GetString(SDL_GameControllerName(gamecontroller));
        }
    }

    public static class Haptic
    {
        public const uint Infinity = 4292967295U;

        public enum EffectId : ushort
        {
            LeftRight = (1 << 2),
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LeftRight
        {
            public EffectId Type;
            public uint Length;
            public ushort LargeMagnitude;
            public ushort SmallMagnitude;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct Effect
        {
            [FieldOffset(0)] public EffectId type;
            [FieldOffset(0)] public LeftRight leftright;
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_HapticClose")]
        public static extern void Close(IntPtr haptic);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_HapticEffectSupported")]
        public static extern int EffectSupported(IntPtr haptic, ref Effect effect);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_JoystickIsHaptic")]
        public static extern int IsHaptic(IntPtr joystick);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_HapticNewEffect")]
        private static extern int SDL_HapticNewEffect(IntPtr haptic, ref Effect effect);

        public static void NewEffect(IntPtr haptic, ref Effect effect)
        {
            GetError(SDL_HapticNewEffect(haptic, ref effect));
        }
        
        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_HapticOpen")]
        public static extern IntPtr Open(int device_index);

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_HapticOpenFromJoystick")]
        private static extern IntPtr SDL_HapticOpenFromJoystick(IntPtr joystick);

        public static IntPtr OpenFromJoystick(IntPtr joystick)
        {
            return GetError(SDL_HapticOpenFromJoystick(joystick));
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_HapticRumbleInit")]
        private static extern int SDL_HapticRumbleInit(IntPtr haptic);

        public static void RumbleInit(IntPtr haptic)
        {
            GetError(SDL_HapticRumbleInit(haptic));
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_HapticRumblePlay")]
        private static extern int SDL_HapticRumblePlay(IntPtr haptic, float strength, uint length);

        public static void RumblePlay(IntPtr haptic, float strength, uint length)
        {
            GetError(SDL_HapticRumblePlay(haptic, strength, length));
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_HapticRumbleSupported")]
        private static extern int SDL_HapticRumbleSupported(IntPtr haptic);

        public static int RumbleSupported(IntPtr haptic)
        {
            return GetError(SDL_HapticRumbleSupported(haptic));
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_HapticRunEffect")]
        private static extern int SDL_HapticRunEffect(IntPtr haptic, int effect, uint iterations);

        public static void RunEffect(IntPtr haptic, int effect, uint iterations)
        {
            GetError(SDL_HapticRunEffect(haptic, effect, iterations));
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_HapticStopAll")]
        private static extern int SDL_HapticStopAll(IntPtr haptic);

        public static void StopAll(IntPtr haptic)
        {
            GetError(SDL_HapticStopAll(haptic));
        }

        [DllImport(NativeLibName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SDL_HapticUpdateEffect")]
        private static extern int SDL_HapticUpdateEffect(IntPtr haptic, int effect, ref Effect data);

        public static void UpdateEffect(IntPtr haptic, int effect, ref Effect data)
        {
            GetError(SDL_HapticUpdateEffect(haptic, effect, ref data));
        }
    }
}
