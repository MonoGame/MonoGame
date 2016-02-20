// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework
{
    class SDLGameWindow : GameWindow, IDisposable
    {
        public override bool AllowUserResizing
        {
            get
            {
                return !this.IsBorderless && _resizable;
            }
            set
            {
                if (!_handle.Equals(IntPtr.Zero))
                    throw new Exception("SDL does not support changing resizable parameter of the window after it's already been created.");
                
                _resizable = value;
            }
        }

        public override Rectangle ClientBounds
        {
            get
            {
                int x = 0, y = 0, w, h;

                SDL.Window.GetSize(Handle, out w, out h);

                if (!_isFullScreen)
                {
                    SDL.Window.GetPosition(Handle, out x, out y);

                    if (!IsBorderless)
                    {
                        x += BorderX;
                        y += BorderY;
                    }
                }

                return new Rectangle(x, y, w, h);
            }
        }

        public override Point Position
        {
            get
            {
                int x = 0, y = 0;

                if (!_isFullScreen)
                    SDL.Window.GetPosition(Handle, out x, out y);
                
                return new Point(x, y);
            }
            set
            {
                SDL.Window.SetPosition(Handle, value.X, value.Y);
            }
        }

        public override DisplayOrientation CurrentOrientation
        {
            get
            {
                return DisplayOrientation.LandscapeLeft;
            }
        }

        public override IntPtr Handle
        {
            get
            {
                return _handle;
            }
        }

        public override string ScreenDeviceName
        {
            get
            {
                return _screenDeviceName;
            }
        }

        public override bool IsBorderless
        {
            get
            {
                return _borderless;
            }
            set
            {
                SDL.Window.SetBordered(this._handle, value ? 1 : 0);
                _borderless = value;
            }
        }

        internal static GameWindow Instance;

        internal int BorderX, BorderY;
        internal bool _isFullScreen;

        private Game _game;
        private IntPtr _handle;
        private bool _disposed, _resizable, _borderless, _willBeFullScreen, _mouseVisible;
        private string _screenDeviceName;

        public SDLGameWindow(Game game)
        {
            this._game = game;
            this._screenDeviceName = "";

            Instance = this;
        }

        internal void CreateWindow()
        {
            var title = MonoGame.Utilities.AssemblyHelper.GetDefaultWindowTitle();

            var initflags = 
                SDL.Window.State.OpenGL |
                SDL.Window.State.Hidden |
                SDL.Window.State.InputFocus |
                SDL.Window.State.MouseFocus;

            if (_resizable)
                initflags |= SDL.Window.State.Resizable;

            this._handle = SDL.Window.Create(title, 
                SDL.Window.PosCentered, SDL.Window.PosCentered, 
                GraphicsDeviceManager.DefaultBackBufferWidth, GraphicsDeviceManager.DefaultBackBufferHeight, 
                initflags);

            SetCursorVisible(_mouseVisible);

            // TODO, per platform border size detection
        }

        ~SDLGameWindow()
        {
            Dispose(false);
        }

        public void SetCursorVisible(bool visible)
        {
            _mouseVisible = visible;
            var err = SDL.Mouse.ShowCursor(visible ? 1 : 0);

            if (err < 0)
                Console.WriteLine("Failed to set cursor! SDL Error: " + SDL.GetError());
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
            _willBeFullScreen = willBeFullScreen;
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
            this._screenDeviceName = screenDeviceName;

            var prevBounds = ClientBounds;

            var displayIndex = SDL.Window.GetDisplayIndex(Handle);

            SDL.Rectangle displayRect;
            SDL.Display.GetBounds(displayIndex, out displayRect);
            
            if (_willBeFullScreen != _isFullScreen)
            {
                var fullscreenFlag = _game.graphicsDeviceManager.HardwareModeSwitch ? SDL.Window.State.Fullscreen : SDL.Window.State.FullscreenDesktop;
                SDL.Window.SetFullscreen(Handle, (_willBeFullScreen) ? fullscreenFlag : 0);
            }

            SDL.Window.SetSize(Handle, clientWidth, clientHeight);

            var centerX = Math.Max(prevBounds.X - ((IsBorderless || _isFullScreen) ? 0 : BorderX) + ((prevBounds.Width - clientWidth) / 2), 0);
            var centerY = Math.Max(prevBounds.Y - ((IsBorderless || _isFullScreen) ? 0 : BorderY) + ((prevBounds.Height - clientHeight) / 2), 0);

            if (_isFullScreen && !_willBeFullScreen)
            {
                centerX += displayRect.X;
                centerY += displayRect.Y;
            }
            
            SDL.Window.SetPosition(Handle, centerX, centerY);

            _isFullScreen = _willBeFullScreen;
            OnClientSizeChanged();
        }

        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {
            // Nothing to do here
        }

        protected override void SetTitle(string title)
        {
            SDL.Window.SetTitle(this._handle, title);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                SDL.Window.Destroy(_handle);
                _handle = IntPtr.Zero;

                _disposed = true;
            }
        }
    }
}

