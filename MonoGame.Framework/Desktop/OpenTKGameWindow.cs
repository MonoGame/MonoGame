// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Utilities;

using OpenTK;
using OpenTK.Graphics;

namespace Microsoft.Xna.Framework
{
    class OpenTKGameWindow : GameWindow, IDisposable
    {
        public override bool AllowUserResizing
        {
            get { return _isResizable; }
            set
            {
                if (_isResizable != value)
                    _isResizable = value;
                else
                    return;
                
                if (_isBorderless)
                    return;
                
                Window.WindowBorder = _isResizable ? WindowBorder.Resizable : WindowBorder.Fixed;
            }
        }

        public override Rectangle ClientBounds 
        { 
            get 
            {
                var bounds = Window.ClientRectangle;
                return new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height);
            }
        }

        public override Point Position
        {
            get { return new Point(Window.Location.X,Window.Location.Y); }
            set { Window.Location = new System.Drawing.Point(value.X,value.Y); }
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
                return Window.WindowInfo.Handle;
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
            get { return _isBorderless; }
            set
            {
                if (_isBorderless == value)
                    return;
                    
                _isBorderless = value;
                
                if (_isBorderless)
                    Window.WindowBorder = WindowBorder.Hidden;
                else
                    Window.WindowBorder = _isResizable ? WindowBorder.Resizable : WindowBorder.Fixed;
            }
        }

#if DESKTOPGL
        public override Icon Icon
        {
            get
            {
                return Window.Icon;
            }
            set
            {
                Window.Icon = value;
            }
        }
#endif

        internal INativeWindow Window;

        private bool _willBeFullScreen, _isFullscreen;
        private bool _isResizable, _isBorderless;
        private string _screenDeviceName;

        private Game _game;
        private List<Keys> _keys;

        private bool disposed;

        public OpenTKGameWindow(Game game)
        {
            GraphicsContext.ShareContexts = true;

            _game = game;
            _screenDeviceName = "";

            Window = new NativeWindow();
            Window.WindowBorder = WindowBorder.Fixed;
            Window.Closing += OnClose;
            Window.Resize += OnResize;
            Window.KeyDown += OnKeyDown;
            Window.KeyUp += OnKeyUp;
            Window.KeyPress += OnKeyPress;

            var assembly = Assembly.GetEntryAssembly();
            var t = Type.GetType ("Mono.Runtime");

            Title = assembly != null ? AssemblyHelper.GetDefaultWindowTitle() : "MonoGame Application";

            // In case when DesktopGL dll is compiled using .Net, and you
            // try to load it using Mono, it will cause a crash because of this.
            try
            {
                if (t == null && assembly != null)
                    Window.Icon = Icon.ExtractAssociatedIcon(assembly.Location);
                else {
                    using (var stream = Assembly.GetEntryAssembly().GetManifestResourceStream(string.Format("{0}.Icon.ico", Assembly.GetEntryAssembly().EntryPoint.DeclaringType.Namespace)) ?? 
                        Assembly.GetExecutingAssembly().GetManifestResourceStream("Microsoft.Xna.Framework.monogame.ico")) {
                        if (stream != null)
                            Window.Icon = new Icon(stream);
                    }
                }
            }
            catch { }

            _keys = new List<Keys>();

            Mouse.setWindows(this);
            SetCursorVisible(false);
        }

        ~OpenTKGameWindow()
        {
            Dispose(false);
        }

        private void OnResize(object sender, EventArgs e)
        {
            if (_game == null || _game.GraphicsDevice == null || _isFullscreen)
                return;

            var winWidth = Window.Width;
            var winHeight = Window.Height;

            // If window size is zero, leave bounds unchanged
            // OpenTK appears to set the window client size to 1x1 when minimizing
            if (winWidth <= 1 || winHeight <= 1)
                return;

            _game.GraphicsDevice.PresentationParameters.BackBufferWidth = winWidth;
            _game.GraphicsDevice.PresentationParameters.BackBufferHeight = winHeight;

            _game.GraphicsDevice.Viewport = new Viewport(0, 0, winWidth, winHeight);

            OnClientSizeChanged();
        }

        private void OnClose(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            _game.Exit();
        }

        public void SetCursorVisible(bool visible)
        {
            Window.Cursor = visible ? MouseCursor.Default : MouseCursor.Empty;
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
            _willBeFullScreen = willBeFullScreen;
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
            _screenDeviceName = screenDeviceName;

            if (!Window.Visible)
            {
                Window.ClientSize = new Size(clientWidth, clientHeight);
                Window.Visible = true;
            }

            if (_willBeFullScreen != _isFullscreen)
            {
                if (_willBeFullScreen && _game.graphicsDeviceManager.HardwareModeSwitch)
                {
                    DisplayDevice.Default.ChangeResolution(clientWidth, clientHeight, 
                        DisplayDevice.Default.BitsPerPixel, DisplayDevice.Default.RefreshRate);
                }

                Window.WindowState = _willBeFullScreen ? WindowState.Fullscreen : WindowState.Normal;

                if (!_willBeFullScreen && _game.graphicsDeviceManager.HardwareModeSwitch)
                    DisplayDevice.Default.RestoreResolution();
            }

            if (Window.WindowState != WindowState.Fullscreen)
                Window.ClientSize = new Size(clientWidth, clientHeight);

            var context = GraphicsContext.CurrentContext;
            if (context != null)
                context.Update(Window.WindowInfo);

            _isFullscreen = _willBeFullScreen;
            OnClientSizeChanged();
        }

        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {
            // Do nothing. Desktop platforms don't do orientation.
        }

        protected override void SetTitle(string title)
        {
            Window.Title = title;            
        }

        internal void ProcessEvents()
        {
            Window.ProcessEvents();

            Keyboard.SetKeys(_keys);
        }

        private void OnKeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (_allowAltF4 && e.Key == OpenTK.Input.Key.F4 && _keys.Contains(Keys.LeftAlt))
            {
                Window.Close();
                return;
            }

            var xnaKey = KeyboardUtil.ToXna(e.Key);
            if (!_keys.Contains(xnaKey)) _keys.Add(xnaKey);
        }

        private void OnKeyUp(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            var xnaKey = KeyboardUtil.ToXna(e.Key);
            if (_keys.Contains(xnaKey)) _keys.Remove(xnaKey);
        }

        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            OnTextInput(sender, new TextInputEventArgs(e.KeyChar));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (_isFullscreen)
                    DisplayDevice.Default.RestoreResolution();

                if (disposing)
                {
                    Window.Dispose();
                    Window = null;
                }

                disposed = true;
            }
        }
    }
}
