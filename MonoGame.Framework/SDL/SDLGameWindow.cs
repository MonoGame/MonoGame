// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework
{
    internal class SdlGameWindow : GameWindow, IDisposable
    {
        public override bool AllowUserResizing
        {
            get { return !IsBorderless && _resizable; }
            set
            {
                if (_init)
                {
                    if (Sdl.Patch > 4)
                        Sdl.Window.SetResizable(_handle, value);
                    else
                        throw new Exception("SDL does not support changing resizable parameter of the window after it's already been created.");
                }

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
                int x = 0, y = 0;

                if (!IsFullScreen)
                    Sdl.Window.GetPosition(Handle, out x, out y);

                return new Point(x, y);
            }
            set { Sdl.Window.SetPosition(Handle, value.X, value.Y); }
        }

        public override DisplayOrientation CurrentOrientation
        {
            get { return DisplayOrientation.LandscapeLeft; }
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
                Sdl.Window.SetBordered(_handle, value ? 1 : 0);
                _borderless = value;
            }
        }

        public static GameWindow Instance;
        public bool IsFullScreen;

        internal readonly Game _game;
        private IntPtr _handle;
        private bool _init, _disposed;
        private bool _resizable, _borderless, _willBeFullScreen, _mouseVisible;
        private string _screenDeviceName;
        private int _winx, _winy, _width, _height;

        public SdlGameWindow(Game game)
        {
            _game = game;
            _screenDeviceName = "";

            Instance = this;

            _winx = Sdl.Window.PosUndefined;
            _winy = Sdl.Window.PosUndefined;

            if (Sdl.Patch >= 4)
            {
                var display = GetMouseDisplay();
                _winx = display.X + display.Width / 2;
                _winy = display.Y + display.Height / 2;
            }

            // We need a dummy handle for GraphicDevice until our window gets created
            _handle = Sdl.Window.Create("", _winx, _winy,
                GraphicsDeviceManager.DefaultBackBufferWidth, GraphicsDeviceManager.DefaultBackBufferHeight,
                Sdl.Window.State.Hidden);
        }

        internal void CreateWindow()
        {
            _width = GraphicsDeviceManager.DefaultBackBufferWidth;
            _height = GraphicsDeviceManager.DefaultBackBufferHeight;
            var title = MonoGame.Utilities.AssemblyHelper.GetDefaultWindowTitle();

            var initflags =
                Sdl.Window.State.OpenGL |
                Sdl.Window.State.Hidden |
                Sdl.Window.State.InputFocus |
                Sdl.Window.State.MouseFocus;

            if (_resizable)
                initflags |= Sdl.Window.State.Resizable;

            if (_borderless)
                initflags |= Sdl.Window.State.Boderless;

            Sdl.Window.Destroy(_handle);

            var surfaceFormat = _game.graphicsDeviceManager.PreferredBackBufferFormat.GetColorFormat ();
            var depthStencilFormat = _game.graphicsDeviceManager.PreferredDepthStencilFormat;
            // TODO Need to get this data from the Presentation Parameters
            Sdl.GL.SetAttribute (Sdl.GL.Attribute.RedSize, surfaceFormat.R);
            Sdl.GL.SetAttribute (Sdl.GL.Attribute.GreenSize, surfaceFormat.G);
            Sdl.GL.SetAttribute (Sdl.GL.Attribute.BlueSize, surfaceFormat.B);
            Sdl.GL.SetAttribute (Sdl.GL.Attribute.AlphaSize, surfaceFormat.A);
            switch (depthStencilFormat)
            {
                case DepthFormat.None:
                    Sdl.GL.SetAttribute (Sdl.GL.Attribute.DepthSize, 0);
                    Sdl.GL.SetAttribute (Sdl.GL.Attribute.StencilSize, 0);
                    break;
                case DepthFormat.Depth16:
                    Sdl.GL.SetAttribute (Sdl.GL.Attribute.DepthSize, 16);
                    Sdl.GL.SetAttribute (Sdl.GL.Attribute.StencilSize, 0);
                    break;
                case DepthFormat.Depth24:
                    Sdl.GL.SetAttribute (Sdl.GL.Attribute.DepthSize, 24);
                    Sdl.GL.SetAttribute (Sdl.GL.Attribute.StencilSize, 0);
                    break;
                case DepthFormat.Depth24Stencil8:
                    Sdl.GL.SetAttribute (Sdl.GL.Attribute.DepthSize, 24);
                    Sdl.GL.SetAttribute (Sdl.GL.Attribute.StencilSize, 8);
                    break;
            }
            Sdl.GL.SetAttribute (Sdl.GL.Attribute.DoubleBuffer, 1);
            Sdl.GL.SetAttribute (Sdl.GL.Attribute.ContextMajorVersion, 2);
            Sdl.GL.SetAttribute (Sdl.GL.Attribute.ContextMinorVersion, 1);
            Sdl.GL.SetAttribute (Sdl.GL.Attribute.ShareWithCurrentContext, 1);
            
            _handle = Sdl.Window.Create (title,
                _winx - _width / 2, _winy - _height / 2,
                _width, _height, initflags);

            Sdl.SetHint("SDL_VIDEO_MINIMIZE_ON_FOCUS_LOSS", "0");
            Sdl.SetHint("SDL_JOYSTICK_ALLOW_BACKGROUND_EVENTS", "1");

            Sdl.Window.SetTitle(Handle, title);

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
                            var icon = Sdl.LoadBMP_RW(src, 1);
                            Sdl.Window.SetIcon(_handle, icon);
                            Sdl.FreeSurface(icon);
                        }
                        catch { }
                    }
            }

            SetCursorVisible(_mouseVisible);

            _init = true;
        }

        ~SdlGameWindow()
        {
            Dispose(false);
        }

        private static Sdl.Rectangle GetMouseDisplay()
        {
            var rect = new Sdl.Rectangle();

            int x, y;
            Sdl.Mouse.GetGlobalState(out x, out y);

            var displayCount = Sdl.Display.GetNumVideoDisplays();
            for (var i = 0; i < displayCount; i++)
            {
                Sdl.Display.GetBounds(i, out rect);

                if (x >= rect.X && x < rect.X + rect.Width &&
                    y >= rect.Y && y < rect.Y + rect.Height)
                {
                    return rect;
                }
            }

            return rect;
        }

        public void SetCursorVisible(bool visible)
        {
            _mouseVisible = visible;
            Sdl.Mouse.ShowCursor(visible ? 1 : 0);
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
            _willBeFullScreen = willBeFullScreen;
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
            _screenDeviceName = screenDeviceName;

            var prevBounds = ClientBounds;
            var displayIndex = Sdl.Window.GetDisplayIndex(Handle);

            Sdl.Rectangle displayRect;
            Sdl.Display.GetBounds(displayIndex, out displayRect);

            if (_willBeFullScreen != IsFullScreen)
            {
                var fullscreenFlag = _game.graphicsDeviceManager.HardwareModeSwitch ? Sdl.Window.State.Fullscreen : Sdl.Window.State.FullscreenDesktop;
                Sdl.Window.SetFullscreen(Handle, (_willBeFullScreen) ? fullscreenFlag : 0);
            }

            if (!_willBeFullScreen || _game.graphicsDeviceManager.HardwareModeSwitch)
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

            var centerX = Math.Max(prevBounds.X + ((prevBounds.Width - clientWidth) / 2), 0);
            var centerY = Math.Max(prevBounds.Y + ((prevBounds.Height - clientHeight) / 2), 0);

            if (IsFullScreen && !_willBeFullScreen)
            {
                // We need to get the display information again in case
                // the resolution of it was changed.
                Sdl.Display.GetBounds (displayIndex, out displayRect);

                // This centering only occurs when exiting fullscreen
                // so it should center the window on the current display.
                centerX = displayRect.X + displayRect.Width / 2 - clientWidth / 2;
                centerY = displayRect.Y + displayRect.Height / 2 - clientHeight / 2;
            }

            // If this window is resizable, there is a bug in SDL 2.0.4 where
            // after the window gets resized, window position information
            // becomes wrong (for me it always returned 10 8). Solution is
            // to not try and set the window position because it will be wrong.
            if (Sdl.Patch > 4 || !AllowUserResizing)
                Sdl.Window.SetPosition(Handle, centerX, centerY);

            IsFullScreen = _willBeFullScreen;
            OnClientSizeChanged();
        }

        public void ClientResize(int width, int height)
        {
            // SDL reports many resize events even if the Size didn't change.
            // Only call the code below if it actually changed.
            if (_game.GraphicsDevice.PresentationParameters.BackBufferWidth == width &&
                _game.GraphicsDevice.PresentationParameters.BackBufferHeight == height) {
                return;
            }
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

            _disposed = true;
        }
    }
}
