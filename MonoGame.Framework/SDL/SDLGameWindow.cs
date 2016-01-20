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

                SDL.SDL_GetWindowSize(Handle, out w, out h);

                if (!_isFullScreen)
                {
                    SDL.SDL_GetWindowPosition(Handle, out x, out y);

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
                    SDL.SDL_GetWindowPosition(Handle, out x, out y);
                
                return new Point(x, y);
            }
            set
            {
                SDL.SDL_SetWindowPosition(Handle, value.X, value.Y);
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
                SDL.SDL_SetWindowBordered(this._handle, value ? 1 : 0);
                _borderless = value;
            }
        }

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
        }

        internal void CreateWindow()
        {
            var title = MonoGame.Utilities.AssemblyHelper.GetDefaultWindowTitle();

            var initflags = 
                SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL |
                SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN |
                SDL.SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS |
                SDL.SDL_WindowFlags.SDL_WINDOW_MOUSE_FOCUS;

            if (_resizable)
                initflags |= SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;

            this._handle = SDL.SDL_CreateWindow(title, 
                SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED, 
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
            var err = SDL.SDL_ShowCursor(visible ? 1 : 0);

            if (err < 0)
                Console.WriteLine("Failed to set cursor! SDL Error: " + SDL.SDL_GetError());
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
            _willBeFullScreen = willBeFullScreen;
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
            this._screenDeviceName = screenDeviceName;

            var prevBounds = ClientBounds;

            SDL.SDL_SetWindowSize(Handle, clientWidth, clientHeight);

            if (_willBeFullScreen != _isFullScreen)
            {
                var fullscreenFlag = _game.graphicsDeviceManager.HardwareModeSwitch ? SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN : SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP;
                SDL.SDL_SetWindowFullscreen(Handle, (_willBeFullScreen) ? fullscreenFlag : 0);
            }

            if (!_willBeFullScreen && _isFullScreen)
                SDL.SDL_SetWindowPosition(_handle, SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED);
            else if (!_willBeFullScreen)
            {
                SDL.SDL_SetWindowPosition(Handle,
                    Math.Max(prevBounds.X - ((IsBorderless || _isFullScreen) ? 0 : BorderX) + ((prevBounds.Width - clientWidth) / 2), 0),
                    Math.Max(prevBounds.Y - ((IsBorderless || _isFullScreen) ? 0 : BorderY)  + ((prevBounds.Height - clientHeight) / 2), 0)
                );
            }

            _isFullScreen = _willBeFullScreen;
            OnClientSizeChanged();
        }

        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {
            // Nothing to do here
        }

        protected override void SetTitle(string title)
        {
            SDL.SDL_SetWindowTitle(this._handle, title);
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
                SDL.SDL_DestroyWindow(_handle);
                _handle = IntPtr.Zero;

                _disposed = true;
            }
        }
    }
}

