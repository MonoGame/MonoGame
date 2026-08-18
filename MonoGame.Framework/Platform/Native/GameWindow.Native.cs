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
    // Logical native window object.
    internal unsafe MGP_Window* _handle;

    // Actual created native window handle.
    private IntPtr _nativeHandle;

    private static readonly Dictionary<nint, NativeGameWindow> _windows = new Dictionary<nint, NativeGameWindow>();

    private NativeGamePlatform _platform;

    private bool _primaryWindow;
    private bool _visible;
    private bool _allowUserResizing;
    private bool _borderless;
    private bool _hasPendingPosition;
    private int _positionX;
    private int _positionY;
    private bool _hasWindowCreateInfo;
    private bool _hasPendingWindowCreateInfo;

    private int _width;
    private int _height;
    private byte[] _icon;
    private MGP_WindowCreateInfo _windowCreateInfo;
    private MGP_WindowCreateInfo _pendingWindowCreateInfo;

    private bool HasCreatedWindow => _nativeHandle != IntPtr.Zero;
    internal static NativeGameWindow Instance { get; private set; }

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
            if (!HasCreatedWindow)
                return _allowUserResizing;

            return MGP.Window_GetAllowUserResizing(_handle) == 0 ? false : true;
        }

        set
        {
            _allowUserResizing = value;

            if (HasCreatedWindow)
                MGP.Window_SetAllowUserResizing(_handle, (byte)(value ? 1 : 0));
        }
    }
    public override unsafe bool IsBorderless
    {
        get
        {
            if (!HasCreatedWindow)
                return _borderless;

            return MGP.Window_GetIsBorderless(_handle) == 0 ? false : true;
        }

        set
        {
            _borderless = value;

            if (HasCreatedWindow)
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

            if (!HasCreatedWindow)
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

            if (HasCreatedWindow)
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

        Instance = this;

        CreateWindow();
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

        if (Instance == this)
            Instance = null;
    }

    internal unsafe void CreateWindow()
    {
        if (_handle != null)
            return;

        string title = Title == null ? AssemblyHelper.GetDefaultWindowTitle() : Title;

        // Create the window which size may be changed by the platform
        _handle = MGP.Window_Create(_platform.Handle, ref _width, ref _height, title);
        if (_handle == null)
            throw new NoSuitableGraphicsDeviceException("Failed to initialize SDL window!");

        _windows[(nint)_handle] = this;
        TryAttachWindow();
    }

    internal unsafe void CreateWindow(MGP_WindowCreateInfo windowCreateInfo)
    {
        if (_handle == null)
            CreateWindow();

        if (HasCreatedWindow)
        {
            if (!NeedsNativeWindowRecreation(windowCreateInfo))
                return;

            DestroyNativeWindow();
        }

        string title = Title == null ? AssemblyHelper.GetDefaultWindowTitle() : Title;

        if (MGP.Window_CreateNativeWindow(_handle, ref _width, ref _height, title, ref windowCreateInfo) == 0)
            throw new NoSuitableGraphicsDeviceException("Failed to initialize native window!");

        if (!TryAttachWindow())
            throw new NoSuitableGraphicsDeviceException("Failed to initialize native window!");

        _windowCreateInfo = windowCreateInfo;
        _hasWindowCreateInfo = true;
        _hasPendingWindowCreateInfo = false;
    }

    internal void QueueNativeWindowRecreationIfNeeded(PresentationParameters pp, MGP_WindowCreateInfo windowCreateInfo)
    {
        if (!NeedsNativeWindowRecreation(windowCreateInfo))
            return;

        _width = pp.BackBufferWidth;
        _height = pp.BackBufferHeight;
        _pendingWindowCreateInfo = windowCreateInfo;
        _hasPendingWindowCreateInfo = true;
    }

    internal unsafe void ApplyPendingNativeWindowChanges(PresentationParameters pp)
    {
        if (!_hasPendingWindowCreateInfo)
        {
            pp.DeviceWindowHandle = Handle;
            return;
        }

        RecreateNativeWindow(pp, _pendingWindowCreateInfo);
        _hasPendingWindowCreateInfo = false;
    }

    private unsafe void RecreateNativeWindow(PresentationParameters pp, MGP_WindowCreateInfo windowCreateInfo)
    {
        if (_handle == null)
            CreateWindow();

        _width = pp.BackBufferWidth;
        _height = pp.BackBufferHeight;

        DestroyNativeWindow();
        CreateWindow(windowCreateInfo);
        pp.DeviceWindowHandle = Handle;
    }

    private unsafe void DestroyNativeWindow()
    {
        if (!HasCreatedWindow)
            return;

        if (!IsFullScreen)
        {
            Point position = Position;
            _hasPendingPosition = true;
            _positionX = position.X;
            _positionY = position.Y;
        }

        MGP.Window_DestroyNativeWindow(_handle);
        _nativeHandle = IntPtr.Zero;
    }

    private bool NeedsNativeWindowRecreation(MGP_WindowCreateInfo windowCreateInfo)
    {
        if (!HasCreatedWindow || !_hasWindowCreateInfo)
            return true;

        return
            _windowCreateInfo.RedSize != windowCreateInfo.RedSize ||
            _windowCreateInfo.GreenSize != windowCreateInfo.GreenSize ||
            _windowCreateInfo.BlueSize != windowCreateInfo.BlueSize ||
            _windowCreateInfo.AlphaSize != windowCreateInfo.AlphaSize ||
            _windowCreateInfo.FramebufferSrgbCapable != windowCreateInfo.FramebufferSrgbCapable ||
            _windowCreateInfo.DepthSize != windowCreateInfo.DepthSize ||
            _windowCreateInfo.StencilSize != windowCreateInfo.StencilSize ||
            _windowCreateInfo.MultiSampleBuffers != windowCreateInfo.MultiSampleBuffers ||
            _windowCreateInfo.MultiSampleSamples != windowCreateInfo.MultiSampleSamples;
    }

    private unsafe bool TryAttachWindow()
    {
        if (_handle == null)
            return false;

        if (HasCreatedWindow)
            return true;

        _nativeHandle = MGP.Window_GetNativeHandle(_handle);
        if (_nativeHandle == IntPtr.Zero)
            return false;

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

        return true;
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

            if (HasCreatedWindow)
                MGP.Window_EnterFullScreen(_handle, (byte)(HardwareModeSwitch ? 1 : 0));
        }
        else if (!pp.IsFullScreen && IsFullScreen)
        {
            IsFullScreen = pp.IsFullScreen;

            if (HasCreatedWindow)
                MGP.Window_ExitFullScreen(_handle);
        }

        if (_width == pp.BackBufferWidth && _height == pp.BackBufferHeight)
            return;

        _width = pp.BackBufferWidth;
        _height = pp.BackBufferHeight;

        if (HasCreatedWindow)
            MGP.Window_SetClientSize(_handle, pp.BackBufferWidth, pp.BackBufferHeight);
    }

    public unsafe void ClientResize(int width, int height)
    {
        if (_width == width && _height == height)
            return;

        _width = width;
        _height = height;

        if (HasCreatedWindow)
            MGP.Window_SetClientSize(_handle, width, height);

        _platform.Game.GraphicsDevice.PresentationParameters.BackBufferWidth = width;
        _platform.Game.GraphicsDevice.PresentationParameters.BackBufferHeight = height;
        _platform.Game.GraphicsDevice.Viewport = new Viewport(0, 0, width, height);

        OnClientSizeChanged();
    }

    protected override unsafe void SetTitle(string title)
    {
        if (HasCreatedWindow)
            MGP.Window_SetTitle(_handle, title);
    }

    internal unsafe void Show(bool show)
    {
        _visible = show;

        if (HasCreatedWindow)
            MGP.Window_Show(_handle, (byte)(show ? 1 : 0));
    }
}
