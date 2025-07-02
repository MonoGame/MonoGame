// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;
using MonoGame.Interop;
using System;
using System.Collections.Generic;
using System.Text;

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

    public override unsafe bool IsUsingTextInput
    {
        get => MGP.Window_GetIsUsingTextInput(_handle);
        set => MGP.Window_SetIsUsingTextInput(_handle, value);
    }

    public override unsafe Rectangle IMEPosition
    {
        get
        {
            Rectangle rectangle = new Rectangle();
            MGP.Window_GetIMEPosition(_handle, out rectangle.X, out rectangle.Y, out rectangle.Width, out rectangle.Height);
            return rectangle;
        }
        set => MGP.Window_SetIMEPosition(_handle, value.X, value.Y, value.Width, value.Height);
    }

    public override unsafe string ClipboardText
    {
        get
        {
            int length = MGP.Window_GetClipboardText(_handle, null, 0);
            byte[] buffer = new byte[length];
            MGP.Window_GetClipboardText(_handle, buffer, length);
            return UTF8Encoding.UTF8.GetString(buffer);
        }
        set
        {
            byte[] buffer = Encoding.UTF8.GetBytes(value + '\0');
            MGP.Window_SetClipboardText(_handle, buffer);
        }
    }

    public override unsafe bool AllowUserResizing
    {
        get
        {
            return MGP.Window_GetAllowUserResizing(_handle) == 0 ? false : true;
        }

        set
        {
            MGP.Window_SetAllowUserResizing(_handle, (byte)(value ? 1 : 0));
        }
    }

    public override unsafe bool IsBorderless
    {
        get
        {
            return MGP.Window_GetIsBorderless(_handle) == 0 ? false : true;
        }

        set
        {
            MGP.Window_SetIsBorderless(_handle, (byte)(value ? 1 : 0));
        }
    }

    public bool IsFullScreen { get; private set; }

    public bool HardwareModeSwitch { get; private set; }

    public override DisplayOrientation CurrentOrientation { get; }

    public override IntPtr Handle { get; }

    public override string ScreenDeviceName { get; }

    public override unsafe Point Position
    {
        get
        {
            int x = 0, y = 0;

            if (!IsFullScreen)
            {
                MGP.Window_GetPosition(_handle, &x, &y);
            }

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

        // Create the window which size may be changed by the platform.
        fixed (int* w = &_width)
        fixed (int* h = &_height)
        fixed (byte* t = System.Text.Encoding.UTF8.GetBytes(title + '\0'))
            _handle = MGP.Window_Create(platform.Handle, w, h, t);

        _windows[(nint)_handle] = this;

        var icon = AssemblyHelper.GetDefaultWindowIcon();
        if (icon != null)
        {
            fixed(byte* i = icon)
                MGP.Window_SetIconBitmap(_handle, i, icon.Length);
        }

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

            MGP.Window_EnterFullScreen(_handle, (byte)(HardwareModeSwitch ? 1 : 0));
        }
        else if (!pp.IsFullScreen && IsFullScreen)
        {
            IsFullScreen = pp.IsFullScreen;

            MGP.Window_ExitFullScreen(_handle);
        }

        ClientResize(pp.BackBufferWidth, pp.BackBufferHeight);
    }

    public unsafe void ClientResize(int width, int height)
    {
        if (_width == width && _height == height)
            return;

        if (!IsFullScreen)
            MGP.Window_SetClientSize(_handle, width, height);

        UpdateBackBufferSize(width, height);

        OnClientSizeChanged();
    }

    private void UpdateBackBufferSize(int width, int height)
    {
        _width = width;
        _height = height;

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

        manager.PreferredBackBufferWidth = _width;
        manager.PreferredBackBufferHeight = _height;
        manager.ApplyChanges();
    }

    protected override unsafe void SetTitle(string title)
    {
        fixed (byte* t = System.Text.Encoding.UTF8.GetBytes(title + '\0'))
            MGP.Window_SetTitle(_handle, t);
    }

    internal unsafe void Show(bool show)
    {
        MGP.Window_Show(_handle, (byte)(show ? 1 : 0));
    }
}
