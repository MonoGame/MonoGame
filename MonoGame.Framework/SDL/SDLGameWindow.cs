// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenGL;

namespace Microsoft.Xna.Framework
{
    internal class SdlGameWindow : GameWindow, IDisposable
    {
        public override bool AllowUserResizing
        {
            get { return !IsBorderless && _resizable; }
            set
            {
                if (Sdl.Patch > 4)
                    Sdl.Window.SetResizable(_handle, value);
                else
                    throw new Exception("SDL 2.0.4 does not support changing resizable parameter of the window after it's already been created, please use a newer version of it.");

                _resizable = value;
            }
        }

        public override Rectangle ClientBounds
        {
            get
            {
                int x = 0, y = 0;
                Sdl.Window.GetPosition(Handle, out x, out y);
                return new Rectangle(x, y, _width, _height);
            }
        }

        public override Point Position
        {
            get
            {
                if (Handle == IntPtr.Zero)
                    return _position;

                int x = 0, y = 0;

                if (!IsFullScreen)
                    Sdl.Window.GetPosition(Handle, out x, out y);

                return new Point(x, y);
            }
            set
            {
                _position = value;
                Sdl.Window.SetPosition(Handle, value.X, value.Y);
                _wasMoved = true;
            }
        }

        public override DisplayOrientation CurrentOrientation
        {
            get { return DisplayOrientation.Default; }
        }

        public override IntPtr Handle
        {
            get { return _handle; }
        }

        public override string ScreenDeviceName
        {
            get { return _screenDeviceName; }
        }

        public override bool IsBorderless
        {
            get { return _borderless; }
            set
            {
                Sdl.Window.SetBordered(_handle, value ? 0 : 1);
                _borderless = value;
            }
        }

        public static GameWindow Instance;
        public bool IsFullScreen { get; private set; }

        private readonly Game _game;
        private IntPtr _handle, _icon;
        private bool _disposed;
        private bool _resizable, _borderless, _mouseVisible, _hardwareSwitch;
        private string _screenDeviceName;
        private Point _position;
        private int _width, _height;
        private bool _wasMoved, _supressMoved;

        private ColorFormat _surfaceFormat;
        private DepthFormat _depthStencilFormat;
        private int _multisampleCount;

        public SdlGameWindow(Game game)
        {
            _game = game;
            Instance = this;

            Sdl.Rectangle bounds;
            var display = GetMouseDisplay(out bounds);
            if (display != -1)
            {
                _screenDeviceName = Sdl.Display.GetDisplayName(display);

                var x = bounds.X + (bounds.Width - GraphicsDeviceManager.DefaultBackBufferWidth) / 2;
                var y = bounds.Y + (bounds.Height - GraphicsDeviceManager.DefaultBackBufferHeight) / 2;
                _position = new Point(x, y);
            }
            
            Sdl.SetHint("SDL_VIDEO_MINIMIZE_ON_FOCUS_LOSS", "0");
            Sdl.SetHint("SDL_JOYSTICK_ALLOW_BACKGROUND_EVENTS", "1");

            using (
                var stream =
                    Assembly.GetEntryAssembly().GetManifestResourceStream(Assembly.GetEntryAssembly().EntryPoint.DeclaringType.Namespace + ".Icon.bmp") ??
                    Assembly.GetEntryAssembly().GetManifestResourceStream("Icon.bmp") ??
                    Assembly.GetExecutingAssembly().GetManifestResourceStream("MonoGame.bmp"))
            {
                if (stream != null)
                    using (var br = new BinaryReader(stream))
                    {
                        try
                        {
                            var src = Sdl.RwFromMem(br.ReadBytes((int)stream.Length), (int)stream.Length);
                            _icon = Sdl.LoadBMP_RW(src, 1);
                        }
                        catch { }
                    }
            }
        }

        internal void CreateWindow(PresentationParameters pp)
        {
            if (Handle != IntPtr.Zero)
                Sdl.Window.Destroy(Handle);

            _width = pp.BackBufferWidth;
            _height = pp.BackBufferHeight;

            _surfaceFormat = pp.BackBufferFormat.GetColorFormat();
            _depthStencilFormat = pp.DepthStencilFormat;
            _multisampleCount = pp.MultiSampleCount;

            var initflags =
                Sdl.Window.State.OpenGL |
                Sdl.Window.State.InputFocus |
                Sdl.Window.State.MouseFocus;

            if (pp.IsFullScreen)
                initflags |= pp.HardwareModeSwitch ? Sdl.Window.State.Fullscreen : Sdl.Window.State.FullscreenDesktop;

            IsFullScreen = pp.IsFullScreen;
            _hardwareSwitch = pp.HardwareModeSwitch;

            _handle = Sdl.Window.Create(Title,
                _position.X, _position.Y,
                _width, _height, initflags);

            if (_icon != IntPtr.Zero)
                Sdl.Window.SetIcon(_handle, _icon);

            Sdl.Window.SetBordered(_handle, _borderless ? 0 : 1);
            Sdl.Window.SetResizable(_handle, _resizable);

            SetCursorVisible(_mouseVisible);

            _supressMoved = true;
        }

        ~SdlGameWindow()
        {
            Dispose(false);
        }

        private static int GetMouseDisplay(out Sdl.Rectangle bounds)
        {
            int x, y;
            Sdl.Mouse.GetGlobalState(out x, out y);

            var displayCount = Sdl.Display.GetNumVideoDisplays();
            for (var i = 0; i < displayCount; i++)
            {
                Sdl.Rectangle rect;
                Sdl.Display.GetBounds(i, out rect);

                if (x >= rect.X && x < rect.X + rect.Width &&
                    y >= rect.Y && y < rect.Y + rect.Height)
                {
                    bounds = rect;
                    return i;
                }
            }

            bounds = default(Sdl.Rectangle);
            return -1;
        }

        public void SetCursorVisible(bool visible)
        {
            _mouseVisible = visible;
            Sdl.Mouse.ShowCursor(visible ? 1 : 0);
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
            var pp = _game.GraphicsDevice.PresentationParameters;

            // TODO recreate the window if depth format, back buffer format or multisample count changed

            _screenDeviceName = screenDeviceName;

            var displayIndex = Sdl.Window.GetDisplayIndex(Handle);

            Sdl.Rectangle displayRect;
            Sdl.Display.GetBounds(displayIndex, out displayRect);

            if ((!IsFullScreen && pp.IsFullScreen) ||
                (IsFullScreen && pp.IsFullScreen && _hardwareSwitch != pp.HardwareModeSwitch))
            {
                IsFullScreen = true;
                var fullscreenFlag = pp.HardwareModeSwitch ? Sdl.Window.State.Fullscreen : Sdl.Window.State.FullscreenDesktop;
                Sdl.Window.SetFullscreen(Handle, fullscreenFlag);
                _hardwareSwitch = pp.HardwareModeSwitch;

                OnClientSizeChanged();
            }
            else if (IsFullScreen && !pp.IsFullScreen)
            {
                IsFullScreen = false;
                Sdl.Window.SetFullscreen(Handle, 0);
                Sdl.Window.SetPosition(Handle, _position.X, _position.Y);

                OnClientSizeChanged();
            }

            if (!IsFullScreen || _hardwareSwitch)
            {
                Sdl.Window.SetSize(Handle, clientWidth, clientHeight);
                _width = clientWidth;
                _height = clientHeight;
            }
            else
            {
                _width = displayRect.Width;
                _height = displayRect.Height;
            }

            if (!_wasMoved && !IsFullScreen)
                CenterWindow();

            _supressMoved = true;
            }

        private void CenterWindow()
        {
            // If this window is resizable, there is a bug in SDL 2.0.4 where
            // after the window gets resized, window position information
            // becomes wrong (for me it always returned 10 8). Solution is
            // to not try and set the window position because it will be wrong.
            if (Sdl.Patch < 4 && AllowUserResizing)
                return;

            int x, y;
            Sdl.Window.GetPosition(Handle, out x, out y);

            var di = Sdl.Window.GetDisplayIndex(Handle);
            Sdl.Rectangle displayBounds;
            Sdl.Display.GetBounds(di, out displayBounds);

            x = displayBounds.X + (displayBounds.Width - _width) / 2;
            y = displayBounds.Y + (displayBounds.Height - _height) / 2;

            Sdl.Window.SetPosition(Handle, x, y);
        }

        internal void Moved()
        {
            if (_supressMoved)
            {
                _supressMoved = false;
                return;
            }

            int x, y;
            Sdl.Window.GetPosition(Handle, out x, out y);
            _position = new Point(x, y);

            _wasMoved = true;
        }

        public void ClientResize(int width, int height)
        {
            // SDL reports many resize events even if the Size didn't change.
            // Only call the code below if it actually changed.
            if (_game.GraphicsDevice.PresentationParameters.BackBufferWidth == width &&
                _game.GraphicsDevice.PresentationParameters.BackBufferHeight == height) {
                return;
            }

            // TODO This should not even be working... We should change preferredBackBufferWidth/Height on the
            // GraphicsDeviceManager, but that currently causes issues because this event is raised when
            // switching to full screen with width and height the size of the display. So it messes up
            // the back buffer size if we set it with those values...

            // The issues with the current implementation are very noticeable when
            // both letting the user resize and resizing the backbuffer through code.
            _game.GraphicsDevice.PresentationParameters.BackBufferWidth = width;
            _game.GraphicsDevice.PresentationParameters.BackBufferHeight = height;
            _game.GraphicsDevice.Viewport = new Viewport(0, 0, width, height);

            Sdl.Window.GetSize(Handle, out _width, out _height);

            OnClientSizeChanged();
        }

        public void CallTextInput(char c, Keys key = Keys.None)
        {
            OnTextInput(this, new TextInputEventArgs(c, key));
        }

        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {
            // Nothing to do here
        }

        protected override void SetTitle(string title)
        {
            Sdl.Window.SetTitle(_handle, title);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            Sdl.Window.Destroy(_handle);
            _handle = IntPtr.Zero;

            if (_icon != IntPtr.Zero)
                Sdl.FreeSurface(_icon);

            _disposed = true;
        }
    }
}
