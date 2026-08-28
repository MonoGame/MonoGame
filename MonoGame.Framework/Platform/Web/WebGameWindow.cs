// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Interop;

namespace Microsoft.Xna.Framework;

internal sealed class WebGameWindow : GameWindow
{
    internal unsafe MGP_Window* _handle;

    private readonly WebGamePlatform _platform;
    private readonly bool _primaryWindow;
    private Rectangle _clientBounds;
    private string _screenDeviceName;
    private Point _position;
    private bool _allowUserResizing;
    private bool _isBorderless;
    private bool _visible;
    private bool _hasWindowCreateInfo;
    private bool _hasPendingWindowCreateInfo;
    private IntPtr _nativeHandle;
    private MGP_WindowCreateInfo _windowCreateInfo;
    private MGP_WindowCreateInfo _pendingWindowCreateInfo;

    private bool HasCreatedWindow => _nativeHandle != IntPtr.Zero;

    public WebGameWindow(WebGamePlatform platform, BrowserHostReadyInfo readyInfo, bool primaryWindow)
    {
        if (platform == null)
            throw new ArgumentNullException(nameof(platform));
        if (readyInfo == null)
            throw new ArgumentNullException(nameof(readyInfo));

        _platform = platform;
        _primaryWindow = primaryWindow;
        _clientBounds = new Rectangle(
            0,
            0,
            readyInfo.CanvasClientWidth,
            readyInfo.CanvasClientHeight);
        _screenDeviceName = string.Empty;
        Title = readyInfo.ApplicationName;

        Instance = this;
        CreateWindow();
    }

    internal static WebGameWindow Instance { get; private set; }

    public static WebGameWindow FromHandle(nint handle)
    {
        if (Instance == null)
            return null;

        return Instance.Handle == (IntPtr)handle ? Instance : null;
    }

    public override bool AllowUserResizing
    {
        get => _allowUserResizing;
        set => _allowUserResizing = value;
    }

    public override bool IsBorderless
    {
        get => _isBorderless;
        set => _isBorderless = value;
    }

    public override Rectangle ClientBounds => _clientBounds;

    public override Point Position
    {
        get => _position;
        set => _position = value;
    }

    public override DisplayOrientation CurrentOrientation => DisplayOrientation.Default;

    public override IntPtr Handle => _nativeHandle;

    public override string ScreenDeviceName => _screenDeviceName;

    internal unsafe void Destroy()
    {
        if (_handle != null)
        {
            MGP.Window_Destroy(_handle);
            _handle = null;
        }

        _nativeHandle = IntPtr.Zero;

        if (_primaryWindow)
            Mouse.WindowHandle = IntPtr.Zero;

        if (Instance == this)
            Instance = null;
    }

    internal unsafe void CreateWindow()
    {
        if (_handle != null)
            return;

        int width = Math.Max(_clientBounds.Width, 1);
        int height = Math.Max(_clientBounds.Height, 1);
        string title = Title ?? string.Empty;

        _handle = MGP.Window_Create(_platform.Handle, ref width, ref height, title);
        if (_handle == null)
            throw new NoSuitableGraphicsDeviceException("Failed to initialize browser window.");

        _clientBounds = new Rectangle(_position.X, _position.Y, width, height);
    }

    internal unsafe void CreateWindow(MGP_WindowCreateInfo windowCreateInfo)
    {
        if (_handle == null)
            CreateWindow();

        if (HasCreatedWindow)
        {
            if (!NeedsNativeWindowRecreation(windowCreateInfo))
                return;

            MGP.Window_DestroyNativeWindow(_handle);
            _nativeHandle = IntPtr.Zero;
        }

        int width = Math.Max(_clientBounds.Width, 1);
        int height = Math.Max(_clientBounds.Height, 1);
        string title = Title ?? string.Empty;

        if (MGP.Window_CreateNativeWindow(_handle, ref width, ref height, title, ref windowCreateInfo) == 0)
            throw new NoSuitableGraphicsDeviceException("Failed to initialize browser native window.");

        AttachNativeWindow(width, height);
        _windowCreateInfo = windowCreateInfo;
        _hasWindowCreateInfo = true;
        _hasPendingWindowCreateInfo = false;
    }

    internal void QueueNativeWindowRecreationIfNeeded(PresentationParameters pp, MGP_WindowCreateInfo windowCreateInfo)
    {
        if (!NeedsNativeWindowRecreation(windowCreateInfo))
            return;

        _clientBounds = new Rectangle(_position.X, _position.Y, pp.BackBufferWidth, pp.BackBufferHeight);
        _pendingWindowCreateInfo = windowCreateInfo;
        _hasPendingWindowCreateInfo = true;
    }

    internal unsafe void ApplyPendingNativeWindowChanges(PresentationParameters pp)
    {
        if (_hasPendingWindowCreateInfo)
        {
            CreateWindow(_pendingWindowCreateInfo);
            _hasPendingWindowCreateInfo = false;
        }

        pp.DeviceWindowHandle = Handle;
    }

    internal void FinalizePendingNativeWindowChanges()
    {
    }

    internal unsafe void Show(bool show)
    {
        _visible = show;

        if (HasCreatedWindow)
            MGP.Window_Show(_handle, (byte)(show ? 1 : 0));
    }

    public override void BeginScreenDeviceChange(bool willBeFullScreen)
    {
    }

    public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
    {
        _screenDeviceName = screenDeviceName ?? string.Empty;
        _clientBounds = new Rectangle(_position.X, _position.Y, clientWidth, clientHeight);
    }

    protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
    {
    }

    protected override unsafe void SetTitle(string title)
    {
        if (HasCreatedWindow)
            MGP.Window_SetTitle(_handle, title);
    }

    public unsafe void OnPresentationChanged(PresentationParameters pp)
    {
        _clientBounds = new Rectangle(_position.X, _position.Y, pp.BackBufferWidth, pp.BackBufferHeight);

        if (HasCreatedWindow)
            MGP.Window_SetClientSize(_handle, pp.BackBufferWidth, pp.BackBufferHeight);
    }

    public void ClientResize(int width, int height)
    {
        if (_clientBounds.Width == width && _clientBounds.Height == height)
            return;

        _clientBounds = new Rectangle(_position.X, _position.Y, width, height);
        _platform.Game.GraphicsDevice.PresentationParameters.BackBufferWidth = width;
        _platform.Game.GraphicsDevice.PresentationParameters.BackBufferHeight = height;
        _platform.Game.GraphicsDevice.Viewport = new Viewport(0, 0, width, height);

        OnClientSizeChanged();
    }

    private unsafe void AttachNativeWindow(int width, int height)
    {
        IntPtr nativeHandle = MGP.Window_GetNativeHandle(_handle);
        if (nativeHandle == IntPtr.Zero)
            throw new NoSuitableGraphicsDeviceException("Failed to initialize browser native window.");

        _nativeHandle = nativeHandle;
        _clientBounds = new Rectangle(_position.X, _position.Y, width, height);

        if (_allowUserResizing)
            MGP.Window_SetAllowUserResizing(_handle, 1);

        if (_isBorderless)
            MGP.Window_SetIsBorderless(_handle, 1);

        if (_visible)
            MGP.Window_Show(_handle, 1);

        if (_primaryWindow)
            Mouse.WindowHandle = Handle;
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
}
