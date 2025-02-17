// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MonoGame.Interop;
using System.Threading;

namespace Microsoft.Xna.Framework;

partial class GamePlatform
{
    internal static GamePlatform PlatformCreate(Game game) => new NativeGamePlatform(game);
}

class NativeGamePlatform : GamePlatform
{
    internal unsafe MGP_Platform* Handle;

    private static unsafe MGG_GraphicsSystem* _system;

    private NativeGameWindow _window;

    private readonly List<string> _dropList = new List<string>(64);

    private int _isExiting;

    public unsafe NativeGamePlatform(Game game) : base(game)
    {
        Handle = MGP.Platform_Create(out GameRunBehavior behavior);

        DefaultRunBehavior = behavior;

        _window = new NativeGameWindow(this, true);

        Window = _window;

        Mouse.WindowHandle = _window.Handle;
        MessageBox._window = _window._handle;
        GamePad.Handle = Handle;
    }

    internal static unsafe MGG_GraphicsSystem* GraphicsSystem
    {
        get
        {
            if (_system == null)
                _system = MGG.GraphicsSystem_Create();

            return _system;
        }
    }

    public override GameRunBehavior DefaultRunBehavior { get; }

    public override unsafe void Exit()
    {
        Interlocked.Increment(ref _isExiting);
    }

    public override unsafe void RunLoop()
    {
        _window.Show(true);

        while (true)
        {
            PollEvents();

            Game.Tick();

            Threading.Run();

            if (_isExiting > 0 && ShouldExit())
                break;
            else
                _isExiting = 0;
        }
    }

    private unsafe void PollEvents()
    {
        while (MGP.Platform_PollEvent(Handle, out MGP_Event event_))
        {
            switch (event_.Type)
            {
                case EventType.Quit:
                    _isExiting++;
                    break;

                case EventType.WindowGainedFocus:
                    IsActive = true;
                    break;

                case EventType.WindowLostFocus:
                    IsActive = false;
                    break;

                case EventType.WindowResized:
                { 
                    var window = NativeGameWindow.FromHandle(event_.Window.Window);
                    if (window != null)
                        window.ClientResize(event_.Window.Data1, event_.Window.Data2);
                    break;
                }

                case EventType.WindowClose:
                { 
                    var window = NativeGameWindow.FromHandle(event_.Window.Window);
                    if (Window == window)
                        _isExiting++;
                    break;
                }

                case EventType.KeyDown:
                {
                    var window = NativeGameWindow.FromHandle(event_.Key.Window);
                    var key = event_.Key.Key;
                    var character = (char)event_.Key.Character;

                    if (!Keyboard.Keys.Contains(key))
                        Keyboard.Keys.Add(key);

                    if (window != null)
                    { 
                        window.OnKeyDown(new InputKeyEventArgs(key));

                        if (window.IsTextInputHandled && char.IsControl(character))
                            window.OnTextInput(new TextInputEventArgs(character, key));
                    }

                    break;
                }

                case EventType.KeyUp:
                {
                    var window = NativeGameWindow.FromHandle(event_.Key.Window);
                    var key = event_.Key.Key;

                    Keyboard.Keys.Remove(key);

                    if (window != null)
                        window.OnKeyUp(new InputKeyEventArgs(key));

                    break;
                }

                case EventType.TextInput:
                {
                    var window = NativeGameWindow.FromHandle(event_.Key.Window);
                    if (window != null && window.IsTextInputHandled)
                    {
                        var key = event_.Key.Key;
                        var character = (char)event_.Key.Character;
                        window.OnTextInput(new TextInputEventArgs(character, key));
                    }
                    break;
                }

                case EventType.MouseMove:
                {
                    var window = NativeGameWindow.FromHandle(event_.MouseMove.Window);
                    if (window != null)
                    {
                        window.MouseState.X = event_.MouseMove.X;
                        window.MouseState.Y = event_.MouseMove.Y;
                    }
                    break;
                }

                case EventType.MouseWheel:
                {
                    var window = NativeGameWindow.FromHandle(event_.MouseWheel.Window);
                    if (window != null)
                    {
                        window.MouseState.ScrollWheelValue = event_.MouseWheel.Scroll;
                        window.MouseState.HorizontalScrollWheelValue = event_.MouseWheel.ScrollH;
                    }
                    break;
                }

                case EventType.MouseButtonUp:
                case EventType.MouseButtonDown:
                {
                    var window = NativeGameWindow.FromHandle(event_.MouseButton.Window);
                    if (window != null)
                    {
                        var state = event_.Type == EventType.MouseButtonDown ? ButtonState.Pressed : ButtonState.Released;

                        switch (event_.MouseButton.Button)
                        {
                            case MouseButton.Left:
                                window.MouseState.LeftButton = state;
                                break;
                            case MouseButton.Right:
                                window.MouseState.RightButton = state;
                                break;
                            case MouseButton.Middle:
                                window.MouseState.MiddleButton = state;
                                break;
                            case MouseButton.X1:
                                window.MouseState.XButton1 = state;
                                break;
                            case MouseButton.X2:
                                window.MouseState.XButton2 = state;
                                break;
                         }
                    }
                    break;
                }

                case EventType.ControllerAdded:
                {
                    GamePad.Add(event_.Controller.Id);
                    break;
                }

                case EventType.ControllerRemoved:
                {
                    GamePad.Remove(event_.Controller.Id);
                    break;
                }

                case EventType.ControllerStateChange:
                {
                    GamePad.ChangeState(event_.Controller.Id, event_.Timestamp, event_.Controller.Input, event_.Controller.Value);
                    break;
                }

                case EventType.DropFile:
                {
                    var file = Marshal.PtrToStringUTF8(event_.Drop.File);
                    _dropList.Add(file);
                    break;
                }

                case EventType.DropComplete:
                {
                    var window = NativeGameWindow.FromHandle(event_.Drop.Window);
                    if (window != null )
                        window.OnFileDrop(new FileDropEventArgs(_dropList.ToArray()));
                    _dropList.Clear();
                    break;
                }
            }
        }
    }

    private bool ShouldExit()
    {
        if (    Keyboard.Keys.Contains(Keys.F4) &&
                (   Keyboard.Keys.Contains(Keys.LeftAlt) ||
                    Keyboard.Keys.Contains(Keys.RightAlt)))
        {
            return Window.AllowAltF4;
        }

        return true;
    }

    public override void Present()
    {
        if (Game.GraphicsDevice != null)
            Game.GraphicsDevice.Present();
    }

    public override unsafe void StartRunLoop()
    {
        MGP.Platform_StartRunLoop(Handle);
    }

    public override unsafe void BeforeInitialize()
    {
        var gdm = Game.graphicsDeviceManager;
        if (gdm == null)
        {
            // TODO: ???
        }
        else
        {
            var pp = Game.GraphicsDevice.PresentationParameters;
            _window.OnPresentationChanged(pp);
        }

        base.BeforeInitialize();        
    }

    public override unsafe bool BeforeRun()
    {        
        return MGP.Platform_BeforeRun(Handle);
    }

    public override unsafe bool BeforeUpdate(GameTime gameTime)
    {
        return MGP.Platform_BeforeUpdate(Handle);
    }

    public override unsafe bool BeforeDraw(GameTime gameTime)
    {
        return MGP.Platform_BeforeDraw(Handle);
    }

    public override unsafe void EnterFullScreen()
    {
    }

    public override unsafe void ExitFullScreen()
    {
    }

    public override void BeginScreenDeviceChange(bool willBeFullScreen)
    {
    }
    public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
    {

    }

    internal override void OnPresentationChanged(PresentationParameters pp)
    {
        _window.OnPresentationChanged(pp);
    }

    protected override unsafe void OnIsMouseVisibleChanged()
    {
        MGP.Mouse_SetVisible(Handle, Game.IsMouseVisible);
    }

    protected unsafe override void Dispose(bool disposing)
    {
        if (_window != null)
        {
            _window.Destroy();
            _window = null;
            Window = null;
        }

        if (Handle != null)
        {
            MGP.Platform_Destroy(Handle);
            Handle = null;
        }

        base.Dispose(disposing);
    }
}
