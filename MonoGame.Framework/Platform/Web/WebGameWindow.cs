// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Interop;

namespace Microsoft.Xna.Framework;

internal sealed class WebGameWindow : GameWindow
{
    internal unsafe MGP_Window* _handle;

    private readonly WebGamePlatform _platform;
    private Rectangle _clientBounds;
    private string _screenDeviceName;
    private Point _position;
    private bool _allowUserResizing;
    private bool _isBorderless;

    public WebGameWindow(WebGamePlatform platform, bool primaryWindow)
    {
        if (platform == null)
            throw new ArgumentNullException(nameof(platform));

        _platform = platform;
        _clientBounds = new Rectangle(
            0,
            0,
            GraphicsDeviceManager.DefaultBackBufferWidth,
            GraphicsDeviceManager.DefaultBackBufferHeight);
        _screenDeviceName = string.Empty;

        Instance = this;
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

    public override IntPtr Handle => IntPtr.Zero;

    public override string ScreenDeviceName => _screenDeviceName;

    internal void CreateWindow()
    {
    }

    internal void CreateWindow(MGP_WindowCreateInfo windowCreateInfo)
    {
        throw new NotImplementedException();
    }

    internal void QueueNativeWindowRecreationIfNeeded(PresentationParameters pp, MGP_WindowCreateInfo windowCreateInfo)
    {
    }

    internal void ApplyPendingNativeWindowChanges(PresentationParameters pp)
    {
        pp.DeviceWindowHandle = Handle;
    }

    internal void FinalizePendingNativeWindowChanges()
    {
    }

    internal void Show(bool show)
    {
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

    protected override void SetTitle(string title)
    {
    }

    public void OnPresentationChanged(PresentationParameters pp)
    {
        _clientBounds = new Rectangle(_position.X, _position.Y, pp.BackBufferWidth, pp.BackBufferHeight);
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
}
