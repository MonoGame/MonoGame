// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Framework.Utilities;
using MonoGame.Interop;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Microsoft.Xna.Framework;

internal class NativeGameWindow : GameWindow
{
    internal unsafe MGP_Window* _handle;

    private static readonly Dictionary<nint, NativeGameWindow> _windows = new Dictionary<nint, NativeGameWindow>();

    private NativeGamePlatform _platform;

    private bool _primaryWindow;

    private int _width;
    private int _height;
    
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
            return MGP.Window_GetAllowUserResizing(_handle);
        }

        set
        {
            MGP.Window_SetAllowUserResizing(_handle, value);
        }
    }
    public override unsafe bool IsBorderless
    {
        get
        {
            return MGP.Window_GetIsBorderless(_handle);
        }

        set
        {
            MGP.Window_SetIsBorderless(_handle, value);
        }
    }

    public bool IsFullScreen;

    public override DisplayOrientation CurrentOrientation { get; }
    public override IntPtr Handle { get; }
    public override string ScreenDeviceName { get; }

    public override unsafe Point Position
    {
        get
        {
            int x = 0, y = 0;

            if (!IsFullScreen)
                MGP.Window_GetPosition(_handle, out x, out y);

            return new Point(x, y);
        }
        set
        {
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

        // Assume the backbuffer size as the default client size.
        _width = GraphicsDeviceManager.DefaultBackBufferWidth;
        _height = GraphicsDeviceManager.DefaultBackBufferHeight;

        var title = Title == null ? AssemblyHelper.GetDefaultWindowTitle() : Title;

        _handle = MGP.Window_Create(
            platform.Handle,
            _width,
            _height,
            title);

        _windows[(nint)_handle] = this;

        Handle = MGP.Window_GetNativeHandle(_handle);
    }

    internal unsafe void Destroy()
    {
        if (_handle != null)
        {
            _windows.Remove((nint)_handle);
            MGP.Window_Destroy(_handle);
            _handle = null;
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

    public void OnPresentationChanged(PresentationParameters pp)
    {
        /*
        if (pp.IsFullScreen && pp.HardwareModeSwitch && IsFullScreen && HardwareModeSwitch)
        {
            if (_platform.IsActive)
            {
                // stay in hardware full screen, need to call ResizeTargets so the displaymode can be switched
                _platform.Game.GraphicsDevice.ResizeTargets();
            }
            else
            {
                // This needs to be called in case the user presses the Windows key while the focus is on the second monitor,
                //	which (sometimes) causes the window to exit fullscreen mode, but still keeps it visible
                MinimizeFullScreen();
            }
        }
        else if (pp.IsFullScreen && (!IsFullScreen || pp.HardwareModeSwitch != HardwareModeSwitch))
        */

        //if (pp.IsFullScreen && !IsFullScreen)
            //EnterFullScreen(pp);
        //else if (!pp.IsFullScreen && IsFullScreen)
            //ExitFullScreen();

        ClientResize(pp.BackBufferWidth, pp.BackBufferHeight);
    }

    public void ClientResize(int width, int height)
    {
        if (_width == width && _height == height)
            return;

        _width = width;
        _height = height;

        UpdateBackBufferSize();

        OnClientSizeChanged();
    }

    private void UpdateBackBufferSize()
    {
        // TODO: Implement sperate swap change logic for
        // non-primary windows.

        // TODO: We should expose a feature to allow
        // for either the swapchain to resize to match
        // the window size or remain fixed size and stretch.

        // Only the primary window will resize the
        // default back buffer.
        if (!_primaryWindow)
            return;

        var manager = _platform.Game.graphicsDeviceManager;
        if (manager.GraphicsDevice == null)
            return;

        if (_width == manager.PreferredBackBufferWidth &&
            _height == manager.PreferredBackBufferHeight)
            return;

        manager.PreferredBackBufferWidth = _width;
        manager.PreferredBackBufferHeight = _height;
        manager.ApplyChanges();
    }

    protected override unsafe void SetTitle(string title)
    {
        MGP.Window_SetTitle(_handle, title);
    }

    internal unsafe void Show(bool show)
    {
        MGP.Window_Show(_handle, show);
    }
}
