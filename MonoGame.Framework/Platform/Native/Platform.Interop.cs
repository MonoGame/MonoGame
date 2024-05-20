// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Runtime.InteropServices;

namespace MonoGame.Interop;

internal class MGHandleAttribute : System.Attribute
{
}

internal enum EventType : uint
{
    Quit,

    WindowMoved,
    WindowResized,
    WindowGainedFocus,
    WindowLostFocus,
    WindowClose,

    KeyDown,
    KeyUp,
    TextInput,

    MouseMove,
    MouseButtonDown,
    MouseButtonUp,
    MouseWheel,

    DropFile,
    DropComplete,
}


[StructLayout(LayoutKind.Sequential)]
internal struct MGP_WindowEvent
{
    public nint Window;
    public int Data1;
    public int Data2;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MGP_KeyEvent
{
    public nint Window;
    public uint Character;
    public Keys Key;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MGP_MouseMoveEvent
{
    public nint Window;
    public int X;
    public int Y;
}

internal enum MouseButton
{
    Left,
    Middle,
    Right,
    X1,
    X2
}

[StructLayout(LayoutKind.Sequential)]
internal struct MGP_MouseButtonEvent
{
    public nint Window;
    public MouseButton Button;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MGP_MouseWheelEvent
{
    public nint Window;
    public int Scroll;
    public int ScrollH;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MGP_DropEvent
{
    public nint Window;
    public nint File;
}


[StructLayout(LayoutKind.Explicit)]
internal struct MGP_Event
{
    [FieldOffset(0)]
    public EventType Type;

    [FieldOffset(4)]
    public MGP_KeyEvent Key;

    [FieldOffset(4)]
    public MGP_MouseMoveEvent MouseMove;

    [FieldOffset(4)]
    public MGP_MouseButtonEvent MouseButton;

    [FieldOffset(4)]
    public MGP_MouseWheelEvent MouseWheel;

    [FieldOffset(4)]
    public MGP_DropEvent Drop;

    [FieldOffset(4)]
    public MGP_WindowEvent Window;

}

[MGHandle]
internal readonly struct MGP_Platform { }

[MGHandle]
internal readonly struct MGP_Window { }

internal static unsafe partial class MGP
{
    const string MonoGameNativeDLL = "monogame.native";

    #region Platform

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Platform_Create", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MGP_Platform* Platform_Create(out GameRunBehavior behavior);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Platform_Destroy", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Platform_Destroy(MGP_Platform* platform);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Platform_BeforeInitialize", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Platform_BeforeInitialize(MGP_Platform* platform);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Platform_PollEvent", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool Platform_PollEvent(MGP_Platform* platform, out MGP_Event event_);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Platform_StartRunLoop", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Platform_StartRunLoop(MGP_Platform* platform);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Platform_BeforeRun", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool Platform_BeforeRun(MGP_Platform* platform);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Platform_BeforeUpdate", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool Platform_BeforeUpdate(MGP_Platform* platform);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Platform_BeforeDraw", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool Platform_BeforeDraw(MGP_Platform* platform);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Platform_EnterFullScreen", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Platform_EnterFullScreen(MGP_Platform* platform);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Platform_ExitFullScreen", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Platform_ExitFullScreen(MGP_Platform* platform);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Platform_Exit", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Platform_Exit(MGP_Platform* platform);

    #endregion

    #region Window

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_Create", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MGP_Window* Window_Create(
        MGP_Platform* platform,
        int width,
        int height,
        string title);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_Destroy", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Window_Destroy(MGP_Window* window);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_GetNativeHandle", StringMarshalling = StringMarshalling.Utf8)]
    public static partial nint Window_GetNativeHandle(MGP_Window* window);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_GetAllowUserResizing", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool Window_GetAllowUserResizing(MGP_Window* window);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_SetAllowUserResizing", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Window_SetAllowUserResizing(MGP_Window* window, [MarshalAs(UnmanagedType.U1)] bool allow);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_GetIsBoderless", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool Window_GetIsBorderless(MGP_Window* window);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_SetIsBoderless", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Window_SetIsBorderless(MGP_Window* window, [MarshalAs(UnmanagedType.U1)] bool borderless);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_SetTitle", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Window_SetTitle(MGP_Window* window, string title);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_Show", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Window_Show(MGP_Window* window, [MarshalAs(UnmanagedType.U1)] bool show);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_GetPosition", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Window_GetPosition(MGP_Window* window, out int x, out int y);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_SetPosition", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Window_SetPosition(MGP_Window* window, int x, int y);

    #endregion

    #region Input

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Input_SetMouseVisible", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Input_SetMouseVisible(MGP_Platform* platform, [MarshalAs(UnmanagedType.U1)] bool visible);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Input_WarpMousePosition", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Input_WarpMousePosition(MGP_Window* window, int x, int y);

    #endregion
}
