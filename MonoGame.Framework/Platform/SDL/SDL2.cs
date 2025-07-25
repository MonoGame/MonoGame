// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using MonoGame.Framework.Utilities;

internal static class Sdl
{
    const string SDL_DLL = "libSDL2";
    public static IntPtr NativeLibrary = GetNativeLibrary();

    private static IntPtr GetNativeLibrary()
    {
        Console.WriteLine($"GetNativeLibrary {CurrentPlatform.OS}");
        if (CurrentPlatform.OS == OS.Windows)
            return FuncLoader.LoadLibraryExt("SDL2.dll");
        else if (CurrentPlatform.OS == OS.Linux)
            return FuncLoader.LoadLibraryExt("libSDL2-2.0.so.0");
        else if (CurrentPlatform.OS == OS.MacOSX)
            return FuncLoader.LoadLibraryExt("libSDL2-2.0.0.dylib");
        else if (CurrentPlatform.OS == OS.Browser)
            return IntPtr.Zero;
        else
            return FuncLoader.LoadLibraryExt("sdl2");
    }

    public static Version version;

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
        DropText = 0x1001,
        DropBegin = 0x1002,
        DropComplete = 0x1003,

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
        [FieldOffset(0)]
        public Drop.Event Drop;
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

        public static bool operator >(Version version1, Version version2)
        {
            return ConcatenateVersion(version1) > ConcatenateVersion(version2);
        }

        public static bool operator <(Version version1, Version version2)
        {
            return ConcatenateVersion(version1) < ConcatenateVersion(version2);
        }


        public static bool operator ==(Version version1, Version version2)
        {
            return version1.Major == version2.Major &&
                version1.Minor == version2.Minor &&
                version1.Patch == version2.Patch;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Version))
                return false;

            return version == (Version)obj;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Major.GetHashCode();
                hash = hash * 23 + Minor.GetHashCode();
                hash = hash * 23 + Patch.GetHashCode();
                return hash;
            }
        }

        public static bool operator !=(Version version1, Version version2)
        {
            return !(version1 == version2);
        }

        public static bool operator >=(Version version1, Version version2)
        {
            return version1 == version2 || version1 > version2;
        }

        public static bool operator <=(Version version1, Version version2)
        {
            return version1 == version2 || version1 < version2;
        }

        public override string ToString()
        {
            return Major + "." + Minor + "." + Patch;
        }

        private static int ConcatenateVersion(Version version)
        {
            // Account for a change in SDL2 version convention. After version 2.0.22,
            // SDL switched formats from 2.0.x to 2.x.y (with y being optional)
            if (version.Major == 2 && version.Minor == 0 && version.Patch < 23)
            {
                return version.Major * 1_000_000 + version.Patch * 1000;
            }
            else
            {
                return version.Major * 1_000_000 + version.Minor * 1000 + version.Patch;
            }
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int d_sdl_init(int flags);
    public static d_sdl_init SDL_Init = FuncLoader.LoadFunction<d_sdl_init>(NativeLibrary, "SDL_Init");
    [DllImport(SDL_DLL,EntryPoint="SDL_Init", CallingConvention = CallingConvention.Cdecl)]
    public static extern int Init_Import(int flags);

    public static void Init(int flags)
    {
        GetError(SDL_Init(flags));
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_sdl_disablescreensaver();
    public static d_sdl_disablescreensaver DisableScreenSaver = FuncLoader.LoadFunction<d_sdl_disablescreensaver>(NativeLibrary, "SDL_DisableScreenSaver");
    [DllImport(SDL_DLL,EntryPoint="SDL_DisableScreenSaver", CallingConvention = CallingConvention.Cdecl)]
    public static extern void DisableScreenSaver_Import();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_sdl_getversion(out Version version);
    public static d_sdl_getversion GetVersion = FuncLoader.LoadFunction<d_sdl_getversion>(NativeLibrary, "SDL_GetVersion");
    [DllImport(SDL_DLL,EntryPoint="SDL_GetVersion", CallingConvention = CallingConvention.Cdecl)]
    public static extern void GetVersion_Import(out Version version);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int d_sdl_pollevent([Out] out Event _event);
    public static d_sdl_pollevent PollEvent = FuncLoader.LoadFunction<d_sdl_pollevent>(NativeLibrary, "SDL_PollEvent");
    [DllImport(SDL_DLL,EntryPoint="SDL_PollEvent", CallingConvention = CallingConvention.Cdecl)]
    public static extern int PollEvent_Import([Out] out Event _event);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int d_sdl_pumpevents();
    public static d_sdl_pumpevents PumpEvents = FuncLoader.LoadFunction<d_sdl_pumpevents>(NativeLibrary, "SDL_PumpEvents");
    [DllImport(SDL_DLL,EntryPoint="SDL_PumpEvents", CallingConvention = CallingConvention.Cdecl)]
    public static extern int PumpEvents_Import();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr d_sdl_creatergbsurfacefrom(IntPtr pixels, int width, int height, int depth, int pitch, uint rMask, uint gMask, uint bMask, uint aMask);
    private static d_sdl_creatergbsurfacefrom SDL_CreateRGBSurfaceFrom = FuncLoader.LoadFunction<d_sdl_creatergbsurfacefrom>(NativeLibrary, "SDL_CreateRGBSurfaceFrom");
    [DllImport(SDL_DLL,EntryPoint="SDL_CreateRGBSurfaceFrom", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr CreateRGBSurfaceFrom_Import(IntPtr pixels, int width, int height, int depth, int pitch, uint rMask, uint gMask, uint bMask, uint aMask);

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

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_sdl_freesurface(IntPtr surface);
    public static d_sdl_freesurface FreeSurface = FuncLoader.LoadFunction<d_sdl_freesurface>(NativeLibrary, "SDL_FreeSurface");
    [DllImport(SDL_DLL,EntryPoint="SDL_FreeSurface", CallingConvention = CallingConvention.Cdecl)]
    public static extern void FreeSurface_Import(IntPtr surface);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr d_sdl_geterror();
    private static d_sdl_geterror SDL_GetError = FuncLoader.LoadFunction<d_sdl_geterror>(NativeLibrary, "SDL_GetError");
    [DllImport(SDL_DLL,EntryPoint="SDL_GetError", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr GetError_Import();

    public static string GetError()
    {
        return InteropHelpers.Utf8ToString(SDL_GetError());
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

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_sdl_clearerror();
    public static d_sdl_clearerror ClearError = FuncLoader.LoadFunction<d_sdl_clearerror>(NativeLibrary, "SDL_ClearError");
    [DllImport(SDL_DLL,EntryPoint="SDL_ClearError", CallingConvention = CallingConvention.Cdecl)]
    public static extern void ClearError_Import();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr d_sdl_gethint(string name);
    public static d_sdl_gethint SDL_GetHint = FuncLoader.LoadFunction<d_sdl_gethint>(NativeLibrary, "SDL_GetHint");
    [DllImport(SDL_DLL,EntryPoint="SDL_GetHint", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr GetHint_Import(string name);

    public static string GetHint(string name)
    {
        return InteropHelpers.Utf8ToString(SDL_GetHint(name));
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr d_sdl_loadbmp_rw(IntPtr src, int freesrc);
    private static d_sdl_loadbmp_rw SDL_LoadBMP_RW = FuncLoader.LoadFunction<d_sdl_loadbmp_rw>(NativeLibrary, "SDL_LoadBMP_RW");
    [DllImport(SDL_DLL,EntryPoint="SDL_LoadBMP_RW", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr LoadBMP_RW_Import(IntPtr src, int freesrc);

    public static IntPtr LoadBMP_RW(IntPtr src, int freesrc)
    {
        return GetError(SDL_LoadBMP_RW(src, freesrc));
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_sdl_quit();
    public static d_sdl_quit Quit = FuncLoader.LoadFunction<d_sdl_quit>(NativeLibrary, "SDL_Quit");
    [DllImport(SDL_DLL,EntryPoint="SDL_Quit", CallingConvention = CallingConvention.Cdecl)]
    public static extern void Quit_Import();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr d_sdl_rwfrommem(byte[] mem, int size);
    private static d_sdl_rwfrommem SDL_RWFromMem = FuncLoader.LoadFunction<d_sdl_rwfrommem>(NativeLibrary, "SDL_RWFromMem");
    [DllImport(SDL_DLL,EntryPoint="SDL_RWFromMem", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr RWFromMem_Import(byte[] mem, int size);

    public static IntPtr RwFromMem(byte[] mem, int size)
    {
        return GetError(SDL_RWFromMem(mem, size));
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int d_sdl_sethint(string name, string value);
    public static d_sdl_sethint SetHint = FuncLoader.LoadFunction<d_sdl_sethint>(NativeLibrary, "SDL_SetHint");
    [DllImport(SDL_DLL,EntryPoint="SDL_SetHint", CallingConvention = CallingConvention.Cdecl)]
    public static extern int SetHint_Import(string name, string value);

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
            Android
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SDL_SysWMinfo
        {
            public Version version;
            public SysWMType subsystem;
            public IntPtr window;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_createwindow(string title, int x, int y, int w, int h, int flags);
        private static d_sdl_createwindow SDL_CreateWindow = FuncLoader.LoadFunction<d_sdl_createwindow>(NativeLibrary, "SDL_CreateWindow");
        [DllImport(SDL_DLL,EntryPoint="SDL_CreateWindow", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateWindow_Import(string title, int x, int y, int w, int h, int flags);

        public static IntPtr Create(string title, int x, int y, int w, int h, int flags)
        {
            return GetError(SDL_CreateWindow(title, x, y, w, h, flags));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_destroywindow(IntPtr window);
        public static d_sdl_destroywindow Destroy = FuncLoader.LoadFunction<d_sdl_destroywindow>(NativeLibrary, "SDL_DestroyWindow");
        [DllImport(SDL_DLL,EntryPoint="SDL_DestroyWindow", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyWindow_Import(IntPtr window);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate uint d_sdl_getwindowid(IntPtr window);
        public static d_sdl_getwindowid GetWindowId = FuncLoader.LoadFunction<d_sdl_getwindowid>(NativeLibrary, "SDL_GetWindowID");
        [DllImport(SDL_DLL,EntryPoint="SDL_GetWindowID", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint GetWindowID_Import(IntPtr window);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_getwindowdisplayindex(IntPtr window);
        private static d_sdl_getwindowdisplayindex SDL_GetWindowDisplayIndex = FuncLoader.LoadFunction<d_sdl_getwindowdisplayindex>(NativeLibrary, "SDL_GetWindowDisplayIndex");
        [DllImport(SDL_DLL,EntryPoint="SDL_GetWindowDisplayIndex", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetWindowDisplayIndex_Import(IntPtr window);

        public static int GetDisplayIndex(IntPtr window)
        {
            return GetError(SDL_GetWindowDisplayIndex(window));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_getwindowflags(IntPtr window);
        public static d_sdl_getwindowflags GetWindowFlags = FuncLoader.LoadFunction<d_sdl_getwindowflags>(NativeLibrary, "SDL_GetWindowFlags");
        [DllImport(SDL_DLL,EntryPoint="SDL_GetWindowFlags", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetWindowFlags_Import(IntPtr window);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_setwindowicon(IntPtr window, IntPtr icon);
        public static d_sdl_setwindowicon SetIcon = FuncLoader.LoadFunction<d_sdl_setwindowicon>(NativeLibrary, "SDL_SetWindowIcon");
        [DllImport(SDL_DLL,EntryPoint="SDL_SetWindowIcon", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetWindowIcon_Import(IntPtr window, IntPtr icon);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_getwindowposition(IntPtr window, out int x, out int y);
        public static d_sdl_getwindowposition GetPosition = FuncLoader.LoadFunction<d_sdl_getwindowposition>(NativeLibrary, "SDL_GetWindowPosition");
        [DllImport(SDL_DLL,EntryPoint="SDL_GetWindowPosition", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetWindowPosition_Import(IntPtr window, out int x, out int y);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_getwindowsize(IntPtr window, out int w, out int h);
        public static d_sdl_getwindowsize GetSize = FuncLoader.LoadFunction<d_sdl_getwindowsize>(NativeLibrary, "SDL_GetWindowSize");
        [DllImport(SDL_DLL,EntryPoint="SDL_GetWindowSize", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetWindowSize_Import(IntPtr window, out int w, out int h);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_setwindowbordered(IntPtr window, int bordered);
        public static d_sdl_setwindowbordered SetBordered = FuncLoader.LoadFunction<d_sdl_setwindowbordered>(NativeLibrary, "SDL_SetWindowBordered");
        [DllImport(SDL_DLL,EntryPoint="SDL_SetWindowBordered", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetWindowBordered_Import(IntPtr window, int bordered);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_setwindowfullscreen(IntPtr window, int flags);
        private static d_sdl_setwindowfullscreen SDL_SetWindowFullscreen = FuncLoader.LoadFunction<d_sdl_setwindowfullscreen>(NativeLibrary, "SDL_SetWindowFullscreen");
        [DllImport(SDL_DLL,EntryPoint="SDL_SetWindowFullscreen", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetWindowFullscreen_Import(IntPtr window, int flags);

        public static void SetFullscreen(IntPtr window, int flags)
        {
            GetError(SDL_SetWindowFullscreen(window, flags));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_setwindowposition(IntPtr window, int x, int y);
        public static d_sdl_setwindowposition SetPosition = FuncLoader.LoadFunction<d_sdl_setwindowposition>(NativeLibrary, "SDL_SetWindowPosition");
        [DllImport(SDL_DLL,EntryPoint="SDL_SetWindowPosition", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetWindowPosition_Import(IntPtr window, int x, int y);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_setwindowresizable(IntPtr window, bool resizable);
        public static d_sdl_setwindowresizable SetResizable = FuncLoader.LoadFunction<d_sdl_setwindowresizable>(NativeLibrary, "SDL_SetWindowResizable");
        [DllImport(SDL_DLL,EntryPoint="SDL_SetWindowResizable", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetWindowResizable_Import(IntPtr window, bool resizable);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_setwindowsize(IntPtr window, int w, int h);
        public static d_sdl_setwindowsize SetSize = FuncLoader.LoadFunction<d_sdl_setwindowsize>(NativeLibrary, "SDL_SetWindowSize");
        [DllImport(SDL_DLL,EntryPoint="SDL_SetWindowSize", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetWindowSize_Import(IntPtr window, int w, int h);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void d_sdl_setwindowtitle(IntPtr window, ref byte value);
        private static d_sdl_setwindowtitle SDL_SetWindowTitle = FuncLoader.LoadFunction<d_sdl_setwindowtitle>(NativeLibrary, "SDL_SetWindowTitle");
        [DllImport(SDL_DLL,EntryPoint="SDL_SetWindowTitle", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetWindowTitle_Import(IntPtr window, ref byte value);

        public static void SetTitle(IntPtr handle, string title)
        {
            var bytes = Encoding.UTF8.GetBytes(title + '\0');
            SDL_SetWindowTitle(handle, ref bytes[0]);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_showwindow(IntPtr window);
        public static d_sdl_showwindow Show = FuncLoader.LoadFunction<d_sdl_showwindow>(NativeLibrary, "SDL_ShowWindow");
        [DllImport(SDL_DLL,EntryPoint="SDL_ShowWindow", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ShowWindow_Import(IntPtr window);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool d_sdl_getwindowwminfo(IntPtr window, ref SDL_SysWMinfo sysWMinfo);
        public static d_sdl_getwindowwminfo GetWindowWMInfo = FuncLoader.LoadFunction<d_sdl_getwindowwminfo>(NativeLibrary, "SDL_GetWindowWMInfo");
        [DllImport(SDL_DLL,EntryPoint="SDL_GetWindowWMInfo", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetWindowWMInfo_Import(IntPtr window, ref SDL_SysWMinfo sysWMinfo);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_getwindowborderssize(IntPtr window, out int top, out int left, out int right, out int bottom);
        public static d_sdl_getwindowborderssize GetBorderSize = FuncLoader.LoadFunction<d_sdl_getwindowborderssize>(NativeLibrary, "SDL_GetWindowBordersSize");
        [DllImport(SDL_DLL,EntryPoint="SDL_GetWindowBordersSize", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetWindowBordersSize_Import(IntPtr window, out int top, out int left, out int right, out int bottom);

        [StructLayout(LayoutKind.Sequential)]
        public struct MessageBoxData
        {
            public uint flags;
            public IntPtr window;
            public string title;
            public string message;
            public int numbuttons;
            public IntPtr buttons;
            public IntPtr colorScheme;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MessageBoxButtonData
        {
            public uint flags;
            public int buttonid;
            public IntPtr text;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_showmessagebox(ref MessageBoxData messageboxdata, out int buttonid);
        public static d_sdl_showmessagebox ShowMessageBox = FuncLoader.LoadFunction<d_sdl_showmessagebox>(NativeLibrary, "SDL_ShowMessageBox");
        [DllImport(SDL_DLL,EntryPoint="SDL_ShowMessageBox", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ShowMessageBox_Import(ref MessageBoxData messageboxdata, out int buttonid);
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

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_getdisplaybounds(int displayIndex, out Rectangle rect);
        private static d_sdl_getdisplaybounds SDL_GetDisplayBounds = FuncLoader.LoadFunction<d_sdl_getdisplaybounds>(NativeLibrary, "SDL_GetDisplayBounds");
        [DllImport(SDL_DLL,EntryPoint="SDL_GetDisplayBounds", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetDisplayBounds_Import(int displayIndex, out Rectangle rect);

        public static void GetBounds(int displayIndex, out Rectangle rect)
        {
            GetError(SDL_GetDisplayBounds(displayIndex, out rect));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_getcurrentdisplaymode(int displayIndex, out Mode mode);
        private static d_sdl_getcurrentdisplaymode SDL_GetCurrentDisplayMode = FuncLoader.LoadFunction<d_sdl_getcurrentdisplaymode>(NativeLibrary, "SDL_GetCurrentDisplayMode");
        [DllImport(SDL_DLL,EntryPoint="SDL_GetCurrentDisplayMode", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetCurrentDisplayMode_Import(int displayIndex, out Mode mode);

        public static void GetCurrentDisplayMode(int displayIndex, out Mode mode)
        {
            GetError(SDL_GetCurrentDisplayMode(displayIndex, out mode));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_getdisplaymode(int displayIndex, int modeIndex, out Mode mode);
        private static d_sdl_getdisplaymode SDL_GetDisplayMode = FuncLoader.LoadFunction<d_sdl_getdisplaymode>(NativeLibrary, "SDL_GetDisplayMode");
        [DllImport(SDL_DLL,EntryPoint="SDL_GetDisplayMode", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetDisplayMode_Import(int displayIndex, int modeIndex, out Mode mode);

        public static void GetDisplayMode(int displayIndex, int modeIndex, out Mode mode)
        {
            GetError(SDL_GetDisplayMode(displayIndex, modeIndex, out mode));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_getclosestdisplaymode(int displayIndex, Mode mode, out Mode closest);
        private static d_sdl_getclosestdisplaymode SDL_GetClosestDisplayMode = FuncLoader.LoadFunction<d_sdl_getclosestdisplaymode>(NativeLibrary, "SDL_GetClosestDisplayMode");
        [DllImport(SDL_DLL,EntryPoint="SDL_GetClosestDisplayMode", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetClosestDisplayMode_Import(int displayIndex, Mode mode, out Mode closest);

        public static void GetClosestDisplayMode(int displayIndex, Mode mode, out Mode closest)
        {
            GetError(SDL_GetClosestDisplayMode(displayIndex, mode, out closest));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_getdisplayname(int index);
        private static d_sdl_getdisplayname SDL_GetDisplayName = FuncLoader.LoadFunction<d_sdl_getdisplayname>(NativeLibrary, "SDL_GetDisplayName");
        [DllImport(SDL_DLL,EntryPoint="SDL_GetDisplayName", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetDisplayName_Import(int index);

        public static string GetDisplayName(int index)
        {
            return InteropHelpers.Utf8ToString(GetError(SDL_GetDisplayName(index)));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_getnumdisplaymodes(int displayIndex);
        private static d_sdl_getnumdisplaymodes SDL_GetNumDisplayModes = FuncLoader.LoadFunction<d_sdl_getnumdisplaymodes>(NativeLibrary, "SDL_GetNumDisplayModes");
        [DllImport(SDL_DLL,EntryPoint="SDL_GetNumDisplayModes", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetNumDisplayModes_Import(int displayIndex);

        public static int GetNumDisplayModes(int displayIndex)
        {
            return GetError(SDL_GetNumDisplayModes(displayIndex));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_getnumvideodisplays();
        private static d_sdl_getnumvideodisplays SDL_GetNumVideoDisplays = FuncLoader.LoadFunction<d_sdl_getnumvideodisplays>(NativeLibrary, "SDL_GetNumVideoDisplays");
        [DllImport(SDL_DLL,EntryPoint="SDL_GetNumVideoDisplays", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetNumVideoDisplays_Import();

        public static int GetNumVideoDisplays()
        {
            return GetError(SDL_GetNumVideoDisplays());
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_getwindowdisplayindex(IntPtr window);
        private static d_sdl_getwindowdisplayindex SDL_GetWindowDisplayIndex = FuncLoader.LoadFunction<d_sdl_getwindowdisplayindex>(NativeLibrary, "SDL_GetWindowDisplayIndex");
        [DllImport(SDL_DLL,EntryPoint="SDL_GetWindowDisplayIndex", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetWindowDisplayIndex_Import(IntPtr window);

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

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_gl_createcontext(IntPtr window);
        private static d_sdl_gl_createcontext SDL_GL_CreateContext = FuncLoader.LoadFunction<d_sdl_gl_createcontext>(NativeLibrary, "SDL_GL_CreateContext");
        [DllImport(SDL_DLL,EntryPoint="SDL_GL_CreateContext", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GL_CreateContext_Import(IntPtr window);

        public static IntPtr CreateContext(IntPtr window)
        {
            return GetError(SDL_GL_CreateContext(window));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_gl_deletecontext(IntPtr context);
        public static d_sdl_gl_deletecontext DeleteContext = FuncLoader.LoadFunction<d_sdl_gl_deletecontext>(NativeLibrary, "SDL_GL_DeleteContext");
        [DllImport(SDL_DLL,EntryPoint="SDL_GL_DeleteContext", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GL_DeleteContext_Import(IntPtr context);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_gl_getcurrentcontext();
        private static d_sdl_gl_getcurrentcontext SDL_GL_GetCurrentContext = FuncLoader.LoadFunction<d_sdl_gl_getcurrentcontext>(NativeLibrary, "SDL_GL_GetCurrentContext");
        [DllImport(SDL_DLL,EntryPoint="SDL_GL_GetCurrentContext", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GL_GetCurrentContext_Import();

        public static IntPtr GetCurrentContext()
        {
            return GetError(SDL_GL_GetCurrentContext());
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr d_sdl_gl_getprocaddress(string proc);
        public static d_sdl_gl_getprocaddress GetProcAddress = FuncLoader.LoadFunction<d_sdl_gl_getprocaddress>(NativeLibrary, "SDL_GL_GetProcAddress");
        [DllImport(SDL_DLL,EntryPoint="SDL_GL_GetProcAddress", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GL_GetProcAddress_Import(string proc);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_gl_getswapinterval();
        public static d_sdl_gl_getswapinterval GetSwapInterval = FuncLoader.LoadFunction<d_sdl_gl_getswapinterval>(NativeLibrary, "SDL_GL_GetSwapInterval");
        [DllImport(SDL_DLL,EntryPoint="SDL_GL_GetSwapInterval", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GL_GetSwapInterval_Import();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_gl_makecurrent(IntPtr window, IntPtr context);
        public static d_sdl_gl_makecurrent MakeCurrent = FuncLoader.LoadFunction<d_sdl_gl_makecurrent>(NativeLibrary, "SDL_GL_MakeCurrent");
        [DllImport(SDL_DLL,EntryPoint="SDL_GL_MakeCurrent", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GL_MakeCurrent_Import(IntPtr window, IntPtr context);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_gl_setattribute(Attribute attr, int value);
        private static d_sdl_gl_setattribute SDL_GL_SetAttribute = FuncLoader.LoadFunction<d_sdl_gl_setattribute>(NativeLibrary, "SDL_GL_SetAttribute");
        [DllImport(SDL_DLL,EntryPoint="SDL_GL_SetAttribute", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GL_SetAttribute_Import(Attribute attr, int value);

        public static int SetAttribute(Attribute attr, int value)
        {
            return GetError(SDL_GL_SetAttribute(attr, value));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_gl_setswapinterval(int interval);
        public static d_sdl_gl_setswapinterval SetSwapInterval = FuncLoader.LoadFunction<d_sdl_gl_setswapinterval>(NativeLibrary, "SDL_GL_SetSwapInterval");
        [DllImport(SDL_DLL,EntryPoint="SDL_GL_SetSwapInterval", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GL_SetSwapInterval_Import(int interval);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_gl_swapwindow(IntPtr window);
        public static d_sdl_gl_swapwindow SwapWindow = FuncLoader.LoadFunction<d_sdl_gl_swapwindow>(NativeLibrary, "SDL_GL_SwapWindow");
        [DllImport(SDL_DLL,EntryPoint="SDL_GL_SwapWindow", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GL_SwapWindow_Import(IntPtr window);
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

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_createcolorcursor(IntPtr surface, int x, int y);
        private static d_sdl_createcolorcursor SDL_CreateColorCursor = FuncLoader.LoadFunction<d_sdl_createcolorcursor>(NativeLibrary, "SDL_CreateColorCursor");
        [DllImport(SDL_DLL,EntryPoint="SDL_CreateColorCursor", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateColorCursor_Import(IntPtr surface, int x, int y);

        public static IntPtr CreateColorCursor(IntPtr surface, int x, int y)
        {
            return GetError(SDL_CreateColorCursor(surface, x, y));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_createsystemcursor(SystemCursor id);
        private static d_sdl_createsystemcursor SDL_CreateSystemCursor = FuncLoader.LoadFunction<d_sdl_createsystemcursor>(NativeLibrary, "SDL_CreateSystemCursor");
        [DllImport(SDL_DLL,EntryPoint="SDL_CreateSystemCursor", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateSystemCursor_Import(SystemCursor id);

        public static IntPtr CreateSystemCursor(SystemCursor id)
        {
            return GetError(SDL_CreateSystemCursor(id));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_freecursor(IntPtr cursor);
        public static d_sdl_freecursor FreeCursor = FuncLoader.LoadFunction<d_sdl_freecursor>(NativeLibrary, "SDL_FreeCursor");
        [DllImport(SDL_DLL,EntryPoint="SDL_FreeCursor", CallingConvention = CallingConvention.Cdecl)]
        public static extern void FreeCursor_Import(IntPtr cursor);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Button d_sdl_getglobalmousestate(out int x, out int y);
        public static d_sdl_getglobalmousestate GetGlobalState = FuncLoader.LoadFunction<d_sdl_getglobalmousestate>(NativeLibrary, "SDL_GetGlobalMouseState");
        [DllImport(SDL_DLL,EntryPoint="SDL_GetGlobalMouseState", CallingConvention = CallingConvention.Cdecl)]
        public static extern Button GetGlobalMouseState_Import(out int x, out int y);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Button d_sdl_getmousestate(out int x, out int y);
        public static d_sdl_getmousestate GetState = FuncLoader.LoadFunction<d_sdl_getmousestate>(NativeLibrary, "SDL_GetMouseState");
        [DllImport(SDL_DLL,EntryPoint="SDL_GetMouseState", CallingConvention = CallingConvention.Cdecl)]
        public static extern Button GetMouseState_Import(out int x, out int y);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_setcursor(IntPtr cursor);
        public static d_sdl_setcursor SetCursor = FuncLoader.LoadFunction<d_sdl_setcursor>(NativeLibrary, "SDL_SetCursor");
        [DllImport(SDL_DLL,EntryPoint="SDL_SetCursor", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCursor_Import(IntPtr cursor);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_showcursor(int toggle);
        public static d_sdl_showcursor ShowCursor = FuncLoader.LoadFunction<d_sdl_showcursor>(NativeLibrary, "SDL_ShowCursor");
        [DllImport(SDL_DLL,EntryPoint="SDL_ShowCursor", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ShowCursor_Import(int toggle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_warpmouseinwindow(IntPtr window, int x, int y);
        public static d_sdl_warpmouseinwindow WarpInWindow = FuncLoader.LoadFunction<d_sdl_warpmouseinwindow>(NativeLibrary, "SDL_WarpMouseInWindow");
        [DllImport(SDL_DLL,EntryPoint="SDL_WarpMouseInWindow", CallingConvention = CallingConvention.Cdecl)]
        public static extern void WarpMouseInWindow_Import(IntPtr window, int x, int y);
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
            public fixed byte Text[32];
            public int Start;
            public int Length;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct TextInputEvent
        {
            public EventType Type;
            public uint Timestamp;
            public uint WindowId;
            public fixed byte Text[32];
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Keymod d_sdl_getmodstate();
        public static d_sdl_getmodstate GetModState = FuncLoader.LoadFunction<d_sdl_getmodstate>(NativeLibrary, "SDL_GetModState");
        [DllImport(SDL_DLL,EntryPoint="SDL_GetModState", CallingConvention = CallingConvention.Cdecl)]
        public static extern Keymod GetModState_Import();
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

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_joystickclose(IntPtr joystick);
        public static d_sdl_joystickclose Close = FuncLoader.LoadFunction<d_sdl_joystickclose>(NativeLibrary, "SDL_JoystickClose");
        [DllImport(SDL_DLL,EntryPoint="SDL_JoystickClose", CallingConvention = CallingConvention.Cdecl)]
        public static extern void JoystickClose_Import(IntPtr joystick);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_joystickfrominstanceid(int joyid);
        private static d_sdl_joystickfrominstanceid SDL_JoystickFromInstanceID = FuncLoader.LoadFunction<d_sdl_joystickfrominstanceid>(NativeLibrary, "SDL_JoystickFromInstanceID");
        [DllImport(SDL_DLL,EntryPoint="SDL_JoystickFromInstanceID", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr JoystickFromInstanceID_Import(int joyid);

        public static IntPtr FromInstanceID(int joyid)
        {
            return GetError(SDL_JoystickFromInstanceID(joyid));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate short d_sdl_joystickgetaxis(IntPtr joystick, int axis);
        public static d_sdl_joystickgetaxis GetAxis = FuncLoader.LoadFunction<d_sdl_joystickgetaxis>(NativeLibrary, "SDL_JoystickGetAxis");
        [DllImport(SDL_DLL,EntryPoint="SDL_JoystickGetAxis", CallingConvention = CallingConvention.Cdecl)]
        public static extern short JoystickGetAxis_Import(IntPtr joystick, int axis);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate byte d_sdl_joystickgetbutton(IntPtr joystick, int button);
        public static d_sdl_joystickgetbutton GetButton = FuncLoader.LoadFunction<d_sdl_joystickgetbutton>(NativeLibrary, "SDL_JoystickGetButton");
        [DllImport(SDL_DLL,EntryPoint="SDL_JoystickGetButton", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte JoystickGetButton_Import(IntPtr joystick, int button);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_joystickname(IntPtr joystick);
        private static d_sdl_joystickname JoystickName = FuncLoader.LoadFunction<d_sdl_joystickname>(NativeLibrary, "SDL_JoystickName");
        [DllImport(SDL_DLL,EntryPoint="SDL_JoystickName", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr JoystickName_Import(IntPtr joystick);

        public static string GetJoystickName(IntPtr joystick)
        {
            return InteropHelpers.Utf8ToString(JoystickName(joystick));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Guid d_sdl_joystickgetguid(IntPtr joystick);
        public static d_sdl_joystickgetguid GetGUID = FuncLoader.LoadFunction<d_sdl_joystickgetguid>(NativeLibrary, "SDL_JoystickGetGUID");
        [DllImport(SDL_DLL,EntryPoint="SDL_JoystickGetGUID", CallingConvention = CallingConvention.Cdecl)]
        public static extern Guid JoystickGetGUID_Import(IntPtr joystick);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Hat d_sdl_joystickgethat(IntPtr joystick, int hat);
        public static d_sdl_joystickgethat GetHat = FuncLoader.LoadFunction<d_sdl_joystickgethat>(NativeLibrary, "SDL_JoystickGetHat");
        [DllImport(SDL_DLL,EntryPoint="SDL_JoystickGetHat", CallingConvention = CallingConvention.Cdecl)]
        public static extern Hat JoystickGetHat_Import(IntPtr joystick, int hat);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_joystickinstanceid(IntPtr joystick);
        public static d_sdl_joystickinstanceid InstanceID = FuncLoader.LoadFunction<d_sdl_joystickinstanceid>(NativeLibrary, "SDL_JoystickInstanceID");
        [DllImport(SDL_DLL,EntryPoint="SDL_JoystickInstanceID", CallingConvention = CallingConvention.Cdecl)]
        public static extern int JoystickInstanceID_Import(IntPtr joystick);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_joystickopen(int deviceIndex);
        private static d_sdl_joystickopen SDL_JoystickOpen = FuncLoader.LoadFunction<d_sdl_joystickopen>(NativeLibrary, "SDL_JoystickOpen");
        [DllImport(SDL_DLL,EntryPoint="SDL_JoystickOpen", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr JoystickOpen_Import(int deviceIndex);

        public static IntPtr Open(int deviceIndex)
        {
            return GetError(SDL_JoystickOpen(deviceIndex));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_joysticknumaxes(IntPtr joystick);
        private static d_sdl_joysticknumaxes SDL_JoystickNumAxes = FuncLoader.LoadFunction<d_sdl_joysticknumaxes>(NativeLibrary, "SDL_JoystickNumAxes");
        [DllImport(SDL_DLL,EntryPoint="SDL_JoystickNumAxes", CallingConvention = CallingConvention.Cdecl)]
        public static extern int JoystickNumAxes_Import(IntPtr joystick);

        public static int NumAxes(IntPtr joystick)
        {
            return GetError(SDL_JoystickNumAxes(joystick));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_joysticknumbuttons(IntPtr joystick);
        private static d_sdl_joysticknumbuttons SDL_JoystickNumButtons = FuncLoader.LoadFunction<d_sdl_joysticknumbuttons>(NativeLibrary, "SDL_JoystickNumButtons");
        [DllImport(SDL_DLL,EntryPoint="SDL_JoystickNumButtons", CallingConvention = CallingConvention.Cdecl)]
        public static extern int JoystickNumButtons_Import(IntPtr joystick);

        public static int NumButtons(IntPtr joystick)
        {
            return GetError(SDL_JoystickNumButtons(joystick));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_joysticknumhats(IntPtr joystick);
        private static d_sdl_joysticknumhats SDL_JoystickNumHats = FuncLoader.LoadFunction<d_sdl_joysticknumhats>(NativeLibrary, "SDL_JoystickNumHats");
        [DllImport(SDL_DLL,EntryPoint="SDL_JoystickNumHats", CallingConvention = CallingConvention.Cdecl)]
        public static extern int JoystickNumHats_Import(IntPtr joystick);

        public static int NumHats(IntPtr joystick)
        {
            return GetError(SDL_JoystickNumHats(joystick));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_numjoysticks();
        private static d_sdl_numjoysticks SDL_NumJoysticks = FuncLoader.LoadFunction<d_sdl_numjoysticks>(NativeLibrary, "SDL_NumJoysticks");
        [DllImport(SDL_DLL,EntryPoint="SDL_NumJoysticks", CallingConvention = CallingConvention.Cdecl)]
        public static extern int NumJoysticks_Import();

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

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_free(IntPtr ptr);
        public static d_sdl_free SDL_Free = FuncLoader.LoadFunction<d_sdl_free>(NativeLibrary, "SDL_free");
        [DllImport(SDL_DLL,EntryPoint="SDL_free", CallingConvention = CallingConvention.Cdecl)]
        public static extern void free_Import(IntPtr ptr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_gamecontrolleraddmapping(string mappingString);
        public static d_sdl_gamecontrolleraddmapping AddMapping = FuncLoader.LoadFunction<d_sdl_gamecontrolleraddmapping>(NativeLibrary, "SDL_GameControllerAddMapping");
        [DllImport(SDL_DLL,EntryPoint="SDL_GameControllerAddMapping", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GameControllerAddMapping_Import(string mappingString);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_gamecontrolleraddmappingsfromrw(IntPtr rw, int freew);
        public static d_sdl_gamecontrolleraddmappingsfromrw AddMappingFromRw = FuncLoader.LoadFunction<d_sdl_gamecontrolleraddmappingsfromrw>(NativeLibrary, "SDL_GameControllerAddMappingsFromRW");
        [DllImport(SDL_DLL,EntryPoint="SDL_GameControllerAddMappingsFromRW", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GameControllerAddMappingsFromRW_Import(IntPtr rw, int freew);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool d_sdl_gamecontrollerhasbutton(IntPtr gamecontroller, Button button);
        public static d_sdl_gamecontrollerhasbutton HasButton = FuncLoader.LoadFunction<d_sdl_gamecontrollerhasbutton>(NativeLibrary, "SDL_GameControllerHasButton");
        [DllImport(SDL_DLL,EntryPoint="SDL_GameControllerHasButton", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GameControllerHasButton_Import(IntPtr gamecontroller, Button button);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool d_sdl_gamecontrollerhasaxis(IntPtr gamecontroller, Axis axis);
        public static d_sdl_gamecontrollerhasaxis HasAxis = FuncLoader.LoadFunction<d_sdl_gamecontrollerhasaxis>(NativeLibrary, "SDL_GameControllerHasAxis");
        [DllImport(SDL_DLL,EntryPoint="SDL_GameControllerHasAxis", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GameControllerHasAxis_Import(IntPtr gamecontroller, Axis axis);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_gamecontrollerclose(IntPtr gamecontroller);
        public static d_sdl_gamecontrollerclose Close = FuncLoader.LoadFunction<d_sdl_gamecontrollerclose>(NativeLibrary, "SDL_GameControllerClose");
        [DllImport(SDL_DLL,EntryPoint="SDL_GameControllerClose", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GameControllerClose_Import(IntPtr gamecontroller);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_joystickfrominstanceid(int joyid);
        private static d_sdl_joystickfrominstanceid SDL_GameControllerFromInstanceID = FuncLoader.LoadFunction<d_sdl_joystickfrominstanceid>(NativeLibrary, "SDL_JoystickFromInstanceID");
        [DllImport(SDL_DLL,EntryPoint="SDL_JoystickFromInstanceID", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr JoystickFromInstanceID_Import(int joyid);

        public static IntPtr FromInstanceID(int joyid)
        {
            return GetError(SDL_GameControllerFromInstanceID(joyid));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate short d_sdl_gamecontrollergetaxis(IntPtr gamecontroller, Axis axis);
        public static d_sdl_gamecontrollergetaxis GetAxis = FuncLoader.LoadFunction<d_sdl_gamecontrollergetaxis>(NativeLibrary, "SDL_GameControllerGetAxis");
        [DllImport(SDL_DLL,EntryPoint="SDL_GameControllerGetAxis", CallingConvention = CallingConvention.Cdecl)]
        public static extern short GameControllerGetAxis_Import(IntPtr gamecontroller, Axis axis);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate byte d_sdl_gamecontrollergetbutton(IntPtr gamecontroller, Button button);
        public static d_sdl_gamecontrollergetbutton GetButton = FuncLoader.LoadFunction<d_sdl_gamecontrollergetbutton>(NativeLibrary, "SDL_GameControllerGetButton");
        [DllImport(SDL_DLL,EntryPoint="SDL_GameControllerGetButton", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte GameControllerGetButton_Import(IntPtr gamecontroller, Button button);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_gamecontrollergetjoystick(IntPtr gamecontroller);
        private static d_sdl_gamecontrollergetjoystick SDL_GameControllerGetJoystick = FuncLoader.LoadFunction<d_sdl_gamecontrollergetjoystick>(NativeLibrary, "SDL_GameControllerGetJoystick");
        [DllImport(SDL_DLL,EntryPoint="SDL_GameControllerGetJoystick", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GameControllerGetJoystick_Import(IntPtr gamecontroller);

        public static IntPtr GetJoystick(IntPtr gamecontroller)
        {
            return GetError(SDL_GameControllerGetJoystick(gamecontroller));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate byte d_sdl_isgamecontroller(int joystickIndex);
        public static d_sdl_isgamecontroller IsGameController = FuncLoader.LoadFunction<d_sdl_isgamecontroller>(NativeLibrary, "SDL_IsGameController");
        [DllImport(SDL_DLL,EntryPoint="SDL_IsGameController", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte IsGameController_Import(int joystickIndex);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_gamecontrollermapping(IntPtr gamecontroller);
        private static d_sdl_gamecontrollermapping SDL_GameControllerMapping = FuncLoader.LoadFunction<d_sdl_gamecontrollermapping>(NativeLibrary, "SDL_GameControllerMapping");
        [DllImport(SDL_DLL,EntryPoint="SDL_GameControllerMapping", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GameControllerMapping_Import(IntPtr gamecontroller);

        public static string GetMapping(IntPtr gamecontroller)
        {
            IntPtr nativeStr = SDL_GameControllerMapping(gamecontroller);
            if (nativeStr == IntPtr.Zero)
                return string.Empty;

            string mappingStr = InteropHelpers.Utf8ToString(nativeStr);

            //The mapping string returned by SDL is owned by us and thus must be freed
            SDL_Free(nativeStr);

            return mappingStr;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_gamecontrolleropen(int joystickIndex);
        private static d_sdl_gamecontrolleropen SDL_GameControllerOpen = FuncLoader.LoadFunction<d_sdl_gamecontrolleropen>(NativeLibrary, "SDL_GameControllerOpen");
        [DllImport(SDL_DLL,EntryPoint="SDL_GameControllerOpen", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GameControllerOpen_Import(int joystickIndex);

        public static IntPtr Open(int joystickIndex)
        {
            return GetError(SDL_GameControllerOpen(joystickIndex));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_gamecontrollername(IntPtr gamecontroller);
        private static d_sdl_gamecontrollername SDL_GameControllerName = FuncLoader.LoadFunction<d_sdl_gamecontrollername>(NativeLibrary, "SDL_GameControllerName");
        [DllImport(SDL_DLL,EntryPoint="SDL_GameControllerName", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GameControllerName_Import(IntPtr gamecontroller);

        public static string GetName(IntPtr gamecontroller)
        {
            return InteropHelpers.Utf8ToString(SDL_GameControllerName(gamecontroller));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_gamecontrollerrumble(IntPtr gamecontroller, ushort left, ushort right, uint duration);
        public static d_sdl_gamecontrollerrumble Rumble = FuncLoader.LoadFunction<d_sdl_gamecontrollerrumble>(NativeLibrary, "SDL_GameControllerRumble");
        [DllImport(SDL_DLL,EntryPoint="SDL_GameControllerRumble", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GameControllerRumble_Import(IntPtr gamecontroller, ushort left, ushort right, uint duration);
        public static d_sdl_gamecontrollerrumble RumbleTriggers = FuncLoader.LoadFunction<d_sdl_gamecontrollerrumble>(NativeLibrary, "SDL_GameControllerRumbleTriggers");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate byte d_sdl_gamecontrollerhasrumble(IntPtr gamecontroller);
        public static d_sdl_gamecontrollerhasrumble HasRumble = FuncLoader.LoadFunction<d_sdl_gamecontrollerhasrumble>(NativeLibrary, "SDL_GameControllerHasRumble");
        [DllImport(SDL_DLL,EntryPoint="SDL_GameControllerHasRumble", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte GameControllerHasRumble_Import(IntPtr gamecontroller);
        public static d_sdl_gamecontrollerhasrumble HasRumbleTriggers = FuncLoader.LoadFunction<d_sdl_gamecontrollerhasrumble>(NativeLibrary, "SDL_GameControllerHasRumbleTriggers");
    }

    public static class Haptic
    {
        // For some reason, different game controllers support different maximum values
        // Also, the closer a given value is to the maximum, the more likely the value will be ignored
        // Hence, we're setting an arbitrary safe value as a maximum
        public const uint Infinity = 1000000U;

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

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_hapticclose(IntPtr haptic);
        public static d_sdl_hapticclose Close = FuncLoader.LoadFunction<d_sdl_hapticclose>(NativeLibrary, "SDL_HapticClose");
        [DllImport(SDL_DLL,EntryPoint="SDL_HapticClose", CallingConvention = CallingConvention.Cdecl)]
        public static extern void HapticClose_Import(IntPtr haptic);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_hapticeffectsupported(IntPtr haptic, ref Effect effect);
        public static d_sdl_hapticeffectsupported EffectSupported = FuncLoader.LoadFunction<d_sdl_hapticeffectsupported>(NativeLibrary, "SDL_HapticEffectSupported");
        [DllImport(SDL_DLL,EntryPoint="SDL_HapticEffectSupported", CallingConvention = CallingConvention.Cdecl)]
        public static extern int HapticEffectSupported_Import(IntPtr haptic, ref Effect effect);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_joystickishaptic(IntPtr joystick);
        public static d_sdl_joystickishaptic IsHaptic = FuncLoader.LoadFunction<d_sdl_joystickishaptic>(NativeLibrary, "SDL_JoystickIsHaptic");
        [DllImport(SDL_DLL,EntryPoint="SDL_JoystickIsHaptic", CallingConvention = CallingConvention.Cdecl)]
        public static extern int JoystickIsHaptic_Import(IntPtr joystick);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_hapticneweffect(IntPtr haptic, ref Effect effect);
        private static d_sdl_hapticneweffect SDL_HapticNewEffect = FuncLoader.LoadFunction<d_sdl_hapticneweffect>(NativeLibrary, "SDL_HapticNewEffect");
        [DllImport(SDL_DLL,EntryPoint="SDL_HapticNewEffect", CallingConvention = CallingConvention.Cdecl)]
        public static extern int HapticNewEffect_Import(IntPtr haptic, ref Effect effect);

        public static void NewEffect(IntPtr haptic, ref Effect effect)
        {
            GetError(SDL_HapticNewEffect(haptic, ref effect));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr d_sdl_hapticopen(int device_index);
        public static d_sdl_hapticopen Open = FuncLoader.LoadFunction<d_sdl_hapticopen>(NativeLibrary, "SDL_HapticOpen");
        [DllImport(SDL_DLL,EntryPoint="SDL_HapticOpen", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr HapticOpen_Import(int device_index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_hapticopenfromjoystick(IntPtr joystick);
        private static d_sdl_hapticopenfromjoystick SDL_HapticOpenFromJoystick = FuncLoader.LoadFunction<d_sdl_hapticopenfromjoystick>(NativeLibrary, "SDL_HapticOpenFromJoystick");
        [DllImport(SDL_DLL,EntryPoint="SDL_HapticOpenFromJoystick", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr HapticOpenFromJoystick_Import(IntPtr joystick);

        public static IntPtr OpenFromJoystick(IntPtr joystick)
        {
            return GetError(SDL_HapticOpenFromJoystick(joystick));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_hapticrumbleinit(IntPtr haptic);
        private static d_sdl_hapticrumbleinit SDL_HapticRumbleInit = FuncLoader.LoadFunction<d_sdl_hapticrumbleinit>(NativeLibrary, "SDL_HapticRumbleInit");
        [DllImport(SDL_DLL,EntryPoint="SDL_HapticRumbleInit", CallingConvention = CallingConvention.Cdecl)]
        public static extern int HapticRumbleInit_Import(IntPtr haptic);

        public static void RumbleInit(IntPtr haptic)
        {
            GetError(SDL_HapticRumbleInit(haptic));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_hapticrumbleplay(IntPtr haptic, float strength, uint length);
        private static d_sdl_hapticrumbleplay SDL_HapticRumblePlay = FuncLoader.LoadFunction<d_sdl_hapticrumbleplay>(NativeLibrary, "SDL_HapticRumblePlay");
        [DllImport(SDL_DLL,EntryPoint="SDL_HapticRumblePlay", CallingConvention = CallingConvention.Cdecl)]
        public static extern int HapticRumblePlay_Import(IntPtr haptic, float strength, uint length);

        public static void RumblePlay(IntPtr haptic, float strength, uint length)
        {
            GetError(SDL_HapticRumblePlay(haptic, strength, length));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_hapticrumblesupported(IntPtr haptic);
        private static d_sdl_hapticrumblesupported SDL_HapticRumbleSupported = FuncLoader.LoadFunction<d_sdl_hapticrumblesupported>(NativeLibrary, "SDL_HapticRumbleSupported");
        [DllImport(SDL_DLL,EntryPoint="SDL_HapticRumbleSupported", CallingConvention = CallingConvention.Cdecl)]
        public static extern int HapticRumbleSupported_Import(IntPtr haptic);

        public static int RumbleSupported(IntPtr haptic)
        {
            return GetError(SDL_HapticRumbleSupported(haptic));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_hapticruneffect(IntPtr haptic, int effect, uint iterations);
        private static d_sdl_hapticruneffect SDL_HapticRunEffect = FuncLoader.LoadFunction<d_sdl_hapticruneffect>(NativeLibrary, "SDL_HapticRunEffect");
        [DllImport(SDL_DLL,EntryPoint= "SDL_HapticRunEffect", CallingConvention = CallingConvention.Cdecl)]
        public static extern int HapticRunEffect_Import(IntPtr haptic, int effect, uint iterations);

        public static void RunEffect(IntPtr haptic, int effect, uint iterations)
        {
            GetError(SDL_HapticRunEffect(haptic, effect, iterations));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_hapticstopall(IntPtr haptic);
        private static d_sdl_hapticstopall SDL_HapticStopAll = FuncLoader.LoadFunction<d_sdl_hapticstopall>(NativeLibrary, "SDL_HapticStopAll");
        [DllImport(SDL_DLL,EntryPoint="SDL_HapticStopAll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int HapticStopAll_Import(IntPtr haptic);

        public static void StopAll(IntPtr haptic)
        {
            GetError(SDL_HapticStopAll(haptic));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_hapticupdateeffect(IntPtr haptic, int effect, ref Effect data);
        private static d_sdl_hapticupdateeffect SDL_HapticUpdateEffect = FuncLoader.LoadFunction<d_sdl_hapticupdateeffect>(NativeLibrary, "SDL_HapticUpdateEffect");
        [DllImport(SDL_DLL,EntryPoint="SDL_HapticUpdateEffect", CallingConvention = CallingConvention.Cdecl)]
        public static extern int HapticUpdateEffect_Import(IntPtr haptic, int effect, ref Effect data);

        public static void UpdateEffect(IntPtr haptic, int effect, ref Effect data)
        {
            GetError(SDL_HapticUpdateEffect(haptic, effect, ref data));
        }
    }

    public static class Drop
    {
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct Event
        {
            public EventType Type;
            public uint TimeStamp;
            public IntPtr File;
            public uint WindowId;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate void d_sdl_free(IntPtr ptr);

        [DllImport(SDL_DLL,EntryPoint="SDL_free", CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static void SDL_free(IntPtr ptr);
        public static d_sdl_free SDL_Free = FuncLoader.LoadFunction<d_sdl_free>(NativeLibrary, "SDL_free");
    }
}
