// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.Utilities;
using MonoGame.Interop;

namespace Microsoft.Xna.Framework;

internal class NativeGameWindow : GameWindow
{
    internal unsafe MGP_Window* _handle;

    private static readonly Dictionary<nint, NativeGameWindow> _windows = new Dictionary<nint, NativeGameWindow>();

    private NativeGamePlatform _platform;

    private bool _primaryWindow;
    private bool _visible;
    private bool _allowUserResizing;
    private bool _borderless;
    private bool _hasPendingPosition;
    private int _positionX;
    private int _positionY;

    private int _width;
    private int _height;
    private IntPtr _nativeHandle;
    private byte[] _icon;

    public static NativeGameWindow FromHandle(nint handle)
    {
        if (_windows.TryGetValue(handle, out var window))
            return window;

        return null;
    }

    public override unsafe bool AllowUserResizing
    {
        get
        {
            if (_handle == null)
                return _allowUserResizing;

            return MGP.Window_GetAllowUserResizing(_handle) == 0 ? false : true;
        }

        set
        {
            _allowUserResizing = value;

            if (_handle != null)
                MGP.Window_SetAllowUserResizing(_handle, (byte)(value ? 1 : 0));
        }
    }
    public override unsafe bool IsBorderless
    {
        get
        {
            if (_handle == null)
                return _borderless;

            return MGP.Window_GetIsBorderless(_handle) == 0 ? false : true;
        }

        set
        {
            _borderless = value;

            if (_handle != null)
                MGP.Window_SetIsBorderless(_handle, (byte)(value ? 1 : 0));
        }
    }

    public bool IsFullScreen { get; private set; }

    public bool HardwareModeSwitch { get; private set; }

    public override DisplayOrientation CurrentOrientation { get; }

    public override IntPtr Handle
    {
        get
        {
            return _nativeHandle;
        }
    }

    public override string ScreenDeviceName { get; }

    public override unsafe Point Position
    {
        get
        {
            int x = 0;
            int y = 0;

            if (_handle == null)
            {
                if (_hasPendingPosition)
                {
                    x = _positionX;
                    y = _positionY;
                }
            }
            else if (!IsFullScreen)
            {
                MGP.Window_GetPosition(_handle, out x, out y);
            }

            return new Point(x, y);
        }
        set
        {
            _hasPendingPosition = true;
            _positionX = value.X;
            _positionY = value.Y;

            if (_handle != null)
                MGP.Window_SetPosition(_handle, value.X, value.Y);
        }
    }

    public override Rectangle ClientBounds
    {
        get
        {
            var position = Position;
            return new Rectangle(position.X, position.Y, _width, _height);
        }
    }

    public unsafe NativeGameWindow(NativeGamePlatform platform, bool primaryWindow)
    {
        _platform = platform;
        _primaryWindow = primaryWindow;
        _visible = false;

        // Assume the backbuffer size as the default client size.
        _width = GraphicsDeviceManager.DefaultBackBufferWidth;
        _height = GraphicsDeviceManager.DefaultBackBufferHeight;

        _icon = AssemblyHelper.GetDefaultWindowIcon();

#if OPENGL
        /*
         * OpenGL needs the presentation parameters first so the window can be
         * created later with the right SDL_GL attributes
         *
         * So we skip window creation for OpenGL path here and delay it
         * - Chris <aristurtledev>
         */
#else
        CreateWindow();
#endif
    }

    internal unsafe void Destroy()
    {
        if (_handle != null)
        {
            _windows.Remove((nint)_handle);
            MGP.Window_Destroy(_handle);
            _handle = null;
        }

        _nativeHandle = IntPtr.Zero;

        if (_primaryWindow)
        {
            Mouse.WindowHandle = IntPtr.Zero;
            MessageBox._window = null;
        }
    }

    internal unsafe void CreateWindow()
    {
        if (_handle != null)
            return;

        string title = Title == null ? AssemblyHelper.GetDefaultWindowTitle() : Title;

        // Create the window which size may be changed by the platform
        _handle = MGP.Window_Create(_platform.Handle, ref _width, ref _height, title);
        AttachWindow();
    }

    internal unsafe void CreateWindow(MGP_OpenGLWindowCreateInfo openGLCreateInfo)
    {
        if (_handle != null)
            return;

        string title = Title == null ? AssemblyHelper.GetDefaultWindowTitle() : Title;

        // Create the window which size may be changed by the platform
        _handle = MGP.Window_Create(_platform.Handle, ref _width, ref _height, title, ref openGLCreateInfo);
        AttachWindow();
    }

    private unsafe void AttachWindow()
    {
        if (_handle == null)
            throw new NoSuitableGraphicsDeviceException("Failed to initialize SDL window!");

        _windows[(nint)_handle] = this;
        _nativeHandle = MGP.Window_GetNativeHandle(_handle);

        if (_icon != null)
        {
            fixed (byte* i = _icon)
                MGP.Window_SetIconBitmap(_handle, i, _icon.Length);
        }

        if (_borderless)
            MGP.Window_SetIsBorderless(_handle, 1);

        if (_allowUserResizing)
            MGP.Window_SetAllowUserResizing(_handle, 1);

        if (_hasPendingPosition)
            MGP.Window_SetPosition(_handle, _positionX, _positionY);

        if (IsFullScreen)
            MGP.Window_EnterFullScreen(_handle, (byte)(HardwareModeSwitch ? 1 : 0));

        if (_visible)
            MGP.Window_Show(_handle, 1);

        if (_primaryWindow)
        {
            Mouse.WindowHandle = Handle;
            MessageBox._window = _handle;
        }
    }

    public override void BeginScreenDeviceChange(bool willBeFullScreen)
    {
    }

    public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
    {
    }

    protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
    {
    }

    public unsafe void OnPresentationChanged(PresentationParameters pp)
    {
        if (pp.IsFullScreen && pp.HardwareModeSwitch && IsFullScreen && HardwareModeSwitch)
        {
            // Nothing changed... what do we do here?
        }
        else if (pp.IsFullScreen && (!IsFullScreen || pp.HardwareModeSwitch != HardwareModeSwitch))
        {
            IsFullScreen = pp.IsFullScreen;
            HardwareModeSwitch = pp.HardwareModeSwitch;

            if (_handle != null)
                MGP.Window_EnterFullScreen(_handle, (byte)(HardwareModeSwitch ? 1 : 0));
        }
        else if (!pp.IsFullScreen && IsFullScreen)
        {
            IsFullScreen = pp.IsFullScreen;

            if (_handle != null)
                MGP.Window_ExitFullScreen(_handle);
        }

        if (_width == pp.BackBufferWidth && _height == pp.BackBufferHeight)
            return;

        _width = pp.BackBufferWidth;
        _height = pp.BackBufferHeight;

        if (_handle != null)
            MGP.Window_SetClientSize(_handle, pp.BackBufferWidth, pp.BackBufferHeight);
    }

    public unsafe void ClientResize(int width, int height)
    {
        if (_width == width && _height == height)
            return;

        _width = width;
        _height = height;

        if (_handle != null)
            MGP.Window_SetClientSize(_handle, width, height);

        OnClientSizeChanged();
    }

    protected override unsafe void SetTitle(string title)
    {
        if (_handle != null)
            MGP.Window_SetTitle(_handle, title);
    }

    internal unsafe void Show(bool show)
    {
        _visible = show;

        if (_handle != null)
            MGP.Window_Show(_handle, (byte)(show ? 1 : 0));
    }
}
