// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.Utilities;
using System.Runtime.InteropServices;

namespace MonoGame.Interop;

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

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Platform_Create", ExactSpelling = true)]
    public static extern MGP_Platform* Platform_Create(out GameRunBehavior behavior);

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Platform_Destroy", ExactSpelling = true)]
    public static extern void Platform_Destroy(MGP_Platform* platform);

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Platform_BeforeInitialize", ExactSpelling = true)]
    public static extern void Platform_BeforeInitialize(MGP_Platform* platform);

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Platform_PollEvent", ExactSpelling = true)]
    public static extern byte Platform_PollEvent(MGP_Platform* platform, out MGP_Event event_);

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Platform_StartRunLoop", ExactSpelling = true)]
    public static extern void Platform_StartRunLoop(MGP_Platform* platform);

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Platform_BeforeRun", ExactSpelling = true)]
    public static extern byte Platform_BeforeRun(MGP_Platform* platform);

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Platform_BeforeUpdate", ExactSpelling = true)]
    public static extern byte Platform_BeforeUpdate(MGP_Platform* platform);

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Platform_BeforeDraw", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern byte Platform_BeforeDraw(MGP_Platform* platform);

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Platform_MakePath", ExactSpelling = true)]
    static extern byte* _Platform_MakePath(byte* location, byte* path);

    public static string Platform_MakePath(string location, string path)
    {
        byte* _result = null;

        try
        {
            byte* _location = stackalloc byte[StringInterop.GetMaxSize(location)];
            StringInterop.CopyString(_location, location);

            byte* _path = stackalloc byte[StringInterop.GetMaxSize(path)];
            StringInterop.CopyString(_path, path);

            _result = _Platform_MakePath(_location, _path);

            string result = StringInterop.ToString(_result);

            return result;
        }
        finally
        {
            StringInterop.Free(_result);
        }
    }

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Platform_GetPlatform", ExactSpelling = true)]
    public static extern MonoGamePlatform Platform_GetPlatform();

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Platform_GetGraphicsBackend", ExactSpelling = true)]
    public static extern GraphicsBackend Platform_GetGraphicsBackend();

    #endregion

    #region Window

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_Create", ExactSpelling = true)]
    public static extern MGP_Window* Window_Create(MGP_Platform* platform, ref int width, ref int height, byte* title);

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_Destroy", ExactSpelling = true)]
    public static extern void Window_Destroy(MGP_Window* window);

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_SetIconBitmap", ExactSpelling = true)]
    public static extern void Window_SetIconBitmap(MGP_Window* window, byte* icon, int length);

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_GetNativeHandle", ExactSpelling = true)]
    public static extern nint Window_GetNativeHandle(MGP_Window* window);

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_GetAllowUserResizing", ExactSpelling = true)]
    public static extern byte Window_GetAllowUserResizing(MGP_Window* window);

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_SetAllowUserResizing", ExactSpelling = true)]
    public static extern void Window_SetAllowUserResizing(MGP_Window* window, byte allow);

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_GetIsBorderless", ExactSpelling = true)]
    public static extern byte Window_GetIsBorderless(MGP_Window* window);

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_SetIsBorderless", ExactSpelling = true)]
    public static extern void Window_SetIsBorderless(MGP_Window* window, byte borderless);

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_SetTitle", ExactSpelling = true)]
    public static extern void Window_SetTitle(MGP_Window* window, byte* title);

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_Show", ExactSpelling = true)]
    public static extern void Window_Show(MGP_Window* window, byte show);

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_GetPosition", ExactSpelling = true)]
    public static extern void Window_GetPosition(MGP_Window* window, out int x, out int y);

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_SetPosition", ExactSpelling = true)]
    public static extern void Window_SetPosition(MGP_Window* window, int x, int y);

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_SetClientSize", ExactSpelling = true)]
    public static extern void Window_SetClientSize(MGP_Window* window, int width, int height);

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_SetCursor", ExactSpelling = true)]
    public static extern void Window_SetCursor(MGP_Window* window, MGP_Cursor* cursor);        

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_ShowMessageBox", ExactSpelling = true)]
    public static extern int Window_ShowMessageBox(
         MGP_Window* window,
         byte* title,
         byte* description,
         byte* buttons,
         int count);

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_EnterFullScreen", ExactSpelling = true)]
    public static extern void Window_EnterFullScreen(MGP_Window* window, byte useHardwareModeSwitch);

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Window_ExitFullScreen", ExactSpelling = true)]
    public static extern void Window_ExitFullScreen(MGP_Window* window);

    #endregion

    #region Mouse

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Mouse_SetVisible", ExactSpelling = true)]
    public static extern void Mouse_SetVisible(MGP_Platform* platform, byte visible);

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Mouse_WarpPosition", ExactSpelling = true)]
    public static extern void Mouse_WarpPosition(MGP_Window* window, int x, int y);

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Cursor_Create", ExactSpelling = true)]
    public static extern MGP_Cursor* Cursor_Create(SystemCursor cursor);

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Cursor_CreateCustom", ExactSpelling = true)]
    public static extern MGP_Cursor* Cursor_CreateCustom(byte* rgba, int width, int height, int originx, int originy);

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_Cursor_Destroy", ExactSpelling = true)]
    public static extern void Cursor_Destroy(MGP_Cursor* cursor);

    #endregion

    #region GamePad

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_GamePad_GetMaxSupported", ExactSpelling = true)]
    public static extern int GamePad_GetMaxSupported();

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_GamePad_GetCaps", ExactSpelling = true)]
    public static extern void GamePad_GetCaps(MGP_Platform* platform, int identifer, MGP_ControllerCaps* caps);

    [DllImport(MonoGameNativeDLL, EntryPoint = "MGP_GamePad_SetVibration", ExactSpelling = true)]
    public static extern byte GamePad_SetVibration(MGP_Platform* platform, int identifer, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger);

    #endregion
}
