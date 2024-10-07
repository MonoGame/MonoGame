// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.Utilities;
using System.Collections.Generic;
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

    ControllerAdded,
    ControllerRemoved,
    ControllerStateChange,

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

internal enum SystemCursor : int
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

[MGHandle]
internal readonly struct MGP_Cursor { }

internal enum ControllerInput : int
{
    INVALID = -1,

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
    Misc1,
    Paddle1,
    Paddle2,
    Paddle3,
    Paddle4,
    Touchpad,
    LAST_BUTTON = Touchpad,

    LeftStickX,
    LeftStickY,
    RightStickX,
    RightStickY,
    LeftTrigger,
    RightTrigger,
    LAST_TRIGGER = RightTrigger,
}

[StructLayout(LayoutKind.Sequential)]
internal struct MGP_ControllerEvent
{
    public int Id;

    public ControllerInput Input;

    /// <summary>
    /// For a axis input this can range from -32768 to 32767.
    /// For a button input the value is 0 or 1.
    /// </summary>
    public short Value;
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
    public ulong Timestamp;

    [FieldOffset(12)]
    public MGP_KeyEvent Key;

    [FieldOffset(12)]
    public MGP_MouseMoveEvent MouseMove;

    [FieldOffset(12)]
    public MGP_MouseButtonEvent MouseButton;

    [FieldOffset(12)]
    public MGP_MouseWheelEvent MouseWheel;

    [FieldOffset(12)]
    public MGP_DropEvent Drop;

    [FieldOffset(12)]
    public MGP_WindowEvent Window;

    [FieldOffset(12)]
    public MGP_ControllerEvent Controller;
}


[StructLayout(LayoutKind.Sequential)]
internal struct MGP_ControllerCaps
{
    public nint Identifier;
    public nint DisplayName;
    public GamePadType GamePadType;
    public uint InputFlags;
    public bool HasLeftVibrationMotor;
    public bool HasRightVibrationMotor;
    public bool HasVoiceSupport;
}


[MGHandle]
internal readonly struct MGP_Platform { }

[MGHandle]
internal readonly struct MGP_Window { }

internal static unsafe partial class MGP
{
    public const string MonoGameNativeDLL = "monogame.native";

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

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Platform_MakePath", StringMarshalling = StringMarshalling.Utf8)]
    public static partial string Platform_MakePath(string location, string path);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Platform_GetPlatform", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MonoGamePlatform Platform_GetPlatform();

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Platform_GetGraphicsBackend", StringMarshalling = StringMarshalling.Utf8)]
    public static partial GraphicsBackend Platform_GetGraphicsBackend();

    #endregion

    #region Window

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_Create", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MGP_Window* Window_Create(
        MGP_Platform* platform,
        ref int width,
        ref int height,
        string title);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_Destroy", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Window_Destroy(MGP_Window* window);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_SetIconBitmap", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Window_SetIconBitmap(MGP_Window* window, byte[] icon, int length);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_GetNativeHandle", StringMarshalling = StringMarshalling.Utf8)]
    public static partial nint Window_GetNativeHandle(MGP_Window* window);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_GetAllowUserResizing", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool Window_GetAllowUserResizing(MGP_Window* window);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_SetAllowUserResizing", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Window_SetAllowUserResizing(MGP_Window* window, [MarshalAs(UnmanagedType.U1)] bool allow);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_GetIsBorderless", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool Window_GetIsBorderless(MGP_Window* window);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_SetIsBorderless", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Window_SetIsBorderless(MGP_Window* window, [MarshalAs(UnmanagedType.U1)] bool borderless);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_SetTitle", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Window_SetTitle(MGP_Window* window, string title);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_Show", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Window_Show(MGP_Window* window, [MarshalAs(UnmanagedType.U1)] bool show);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_GetPosition", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Window_GetPosition(MGP_Window* window, out int x, out int y);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_SetPosition", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Window_SetPosition(MGP_Window* window, int x, int y);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_SetClientSize", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Window_SetClientSize(MGP_Window* window, int width, int height);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_SetCursor", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Window_SetCursor(MGP_Window* window, MGP_Cursor* cursor);        

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_ShowMessageBox", StringMarshalling = StringMarshalling.Utf8)]
    public static partial int Window_ShowMessageBox(
         MGP_Window* window,
         string title,
         string description,
         string[] buttons,
         int count);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_EnterFullScreen", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Window_EnterFullScreen(MGP_Window* window, [MarshalAs(UnmanagedType.U1)] bool useHardwareModeSwitch);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_ExitFullScreen", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Window_ExitFullScreen(MGP_Window* window);

    #endregion

    #region Mouse

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Mouse_SetVisible", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Mouse_SetVisible(MGP_Platform* platform, [MarshalAs(UnmanagedType.U1)] bool visible);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Mouse_WarpPosition", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Mouse_WarpPosition(MGP_Window* window, int x, int y);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Cursor_Create", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MGP_Cursor* Cursor_Create(SystemCursor cursor);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Cursor_CreateCustom", StringMarshalling = StringMarshalling.Utf8)]
    public static partial MGP_Cursor* Cursor_CreateCustom(byte[] rgba, int width, int height, int originx, int originy);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_Cursor_Destroy", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void Cursor_Destroy(MGP_Cursor* cursor);

    #endregion

    #region GamePad

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_GamePad_GetMaxSupported", StringMarshalling = StringMarshalling.Utf8)]
    public static partial int GamePad_GetMaxSupported();

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_GamePad_GetCaps", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void GamePad_GetCaps(MGP_Platform* platform, int identifer, MGP_ControllerCaps* caps);

    [LibraryImport(MonoGameNativeDLL, EntryPoint = "MGP_GamePad_SetVibration", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool GamePad_SetVibration(MGP_Platform* platform, int identifer, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger);

    #endregion
}
