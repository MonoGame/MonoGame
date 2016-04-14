#region License
/*
Microsoft Public License (Ms-PL)
XnaTouch - Copyright © 2009 The XnaTouch Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
using MonoGame.Utilities;


#endregion License

#region Using Statements
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenTK;
using OpenTK.Graphics;


#endregion Using Statements

namespace Microsoft.Xna.Framework
{
    class OpenTKGameWindow : GameWindow, IDisposable
    {
        private bool _isResizable;
        private bool _isBorderless;

		//private DisplayOrientation _currentOrientation;
        private IntPtr _windowHandle;
        private INativeWindow window;

        protected Game game;
        private List<Microsoft.Xna.Framework.Input.Keys> keys;
		//private OpenTK.Graphics.GraphicsContext backgroundContext;

        // we need this variables to make changes beetween threads
        private WindowState windowState;
        private Rectangle clientBounds;
        private Rectangle targetBounds;
        private bool updateClientBounds;
        private int updateborder = 0;
        bool disposed;

        #region Internal Properties

        internal Game Game 
        {
            get { return game; }
            set
            {
                if (game != value)
                {
                    game = value;                   
                }
            }
        }

        internal INativeWindow Window { get { return window; } }

        #endregion

        #region Public Properties

        public override IntPtr Handle { get { return _windowHandle; } }

        public override string ScreenDeviceName { get { return window.Title; } }

        public override Rectangle ClientBounds 
        { 
            get 
            {
                var pos = window.PointToScreen(new System.Drawing.Point(0));
                return new Rectangle(pos.X, pos.Y, clientBounds.Width, clientBounds.Height);
            }
        }

        // TODO: this is buggy on linux - report to opentk team
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
                window.WindowBorder = _isResizable ? WindowBorder.Resizable : WindowBorder.Fixed;
            }
        }

        public override DisplayOrientation CurrentOrientation
        {
            get { return DisplayOrientation.LandscapeLeft; }
        }
#if DESKTOPGL
        public override Microsoft.Xna.Framework.Point Position
        {
            get { return new Microsoft.Xna.Framework.Point(window.Location.X,window.Location.Y); }
            set { window.Location = new System.Drawing.Point(value.X,value.Y); }
        }

        public override System.Drawing.Icon Icon
        {
            get
            {
                return window.Icon;
            }
            set
            {
                window.Icon = value;
            }
        }
#endif
        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {
            // Do nothing.  Desktop platforms don't do orientation.
        }

        public override bool IsBorderless
        {
            get { return _isBorderless; }
            set
            {
                if (_isBorderless != value)
                    _isBorderless = value;
                else
                    return;
                if (_isBorderless)
                {
                    window.WindowBorder = WindowBorder.Hidden;
                }
                else
                    window.WindowBorder = _isResizable ? WindowBorder.Resizable : WindowBorder.Fixed;
            }
        }

        #endregion

        public OpenTKGameWindow(Game game)
        {
            Initialize(game);
        }

        ~OpenTKGameWindow()
        {
            Dispose(false);
        }

        #region Restricted Methods

        #region OpenTK GameWindow Methods

        #region Delegates

        private void OpenTkGameWindow_Closing(object sender, CancelEventArgs e)
        {
            //block the window from getting destroyed before we dispose of
            //the resources, than we will destroy it
            e.Cancel = true;

            Game.Exit();
        }

        private void Keyboard_KeyUp(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            Keys xnaKey = KeyboardUtil.ToXna(e.Key);
            if (keys.Contains(xnaKey)) keys.Remove(xnaKey);
        }
        
        private void Keyboard_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (_allowAltF4 && e.Key == OpenTK.Input.Key.F4 && keys.Contains(Keys.LeftAlt))
            {
                window.Close();
                return;
            }
            Keys xnaKey = KeyboardUtil.ToXna(e.Key);
            if (!keys.Contains(xnaKey)) keys.Add(xnaKey);
        }

        #endregion

        private void OnResize(object sender, EventArgs e)
        {
            // Ignore resize events until intialization is complete
            if (Game == null || Game.GraphicsDevice == null)
                return;

            lock (window)
            {
                var winWidth = window.ClientRectangle.Width;
                var winHeight = window.ClientRectangle.Height;
                var winRect = new Rectangle(0, 0, winWidth, winHeight);

                // If window size is zero, leave bounds unchanged
                // OpenTK appears to set the window client size to 1x1 when minimizing
                if (winWidth <= 1 || winHeight <= 1)
                    return;

                //If we've already got a pending change, do nothing
                if (updateClientBounds)
                    return;
            
                Game.GraphicsDevice.PresentationParameters.BackBufferWidth = winWidth;
                Game.GraphicsDevice.PresentationParameters.BackBufferHeight = winHeight;

                Game.GraphicsDevice.Viewport = new Viewport(0, 0, winWidth, winHeight);

                clientBounds = winRect;

                OnClientSizeChanged();
            }
        }

        internal void ProcessEvents()
        {
            lock (window)
            {
                if (CurrentPlatform.OS == OS.Linux)
                {
                    if (updateborder == 1)
                        UpdateBorder();

                    if(updateborder > 0)
                        updateborder--;
                }

                Window.ProcessEvents();
                UpdateWindowState();
                HandleInput();
            }
        }

        private void UpdateBorder()
        {
            WindowBorder desired;
            if (_isBorderless)
                desired = WindowBorder.Hidden;
            else
                desired = _isResizable ? WindowBorder.Resizable : WindowBorder.Fixed;
        
            if (desired != window.WindowBorder && window.WindowState != WindowState.Fullscreen)
                window.WindowBorder = desired;
        }

        private void UpdateWindowState()
        {
            // we should wait until window's not fullscreen to resize
            if (updateClientBounds)
            {
                var prevState = window.WindowState;

                if (CurrentPlatform.OS == OS.Linux)
                    window.WindowBorder = WindowBorder.Resizable;
                
                updateClientBounds = false;

                if (CurrentPlatform.OS == OS.MacOSX)
                {
                    // on Mac We need to do this calculation first.
                    int centerOffsetX = -(targetBounds.Width - window.ClientRectangle.Width) / 2;
                    int centerOffsetY = -(targetBounds.Height - window.ClientRectangle.Height) / 2;
                    window.X = Math.Max(0, centerOffsetX + window.X);
                    window.Y = Math.Max(0, centerOffsetY + window.Y);

                    window.ClientRectangle = new System.Drawing.Rectangle(targetBounds.X,
                                       targetBounds.Y, targetBounds.Width, targetBounds.Height);
                }
                
                // if the window-state is set from the outside (maximized button pressed) we have to update it here.
                // if it was set from the inside (.IsFullScreen changed), we have to change the window.
                // this code might not cover all corner cases
                // window was maximized
                if ((windowState == WindowState.Normal && window.WindowState == WindowState.Maximized) ||
                    (windowState == WindowState.Maximized && window.WindowState == WindowState.Normal))
                    windowState = window.WindowState; // maximize->normal and normal->maximize are usually set from the outside
                else
                    window.WindowState = windowState; // usually fullscreen-stuff is set from the code
                
                if (CurrentPlatform.OS != OS.MacOSX)
                {
                    if (!Configuration.RunningOnSdl2 && prevState != WindowState.Fullscreen)
                    {
                        int centerOffsetX = -(targetBounds.Width - window.ClientRectangle.Width) / 2;
                        int centerOffsetY = -(targetBounds.Height - window.ClientRectangle.Height) / 2;
                        window.X = Math.Max(0, centerOffsetX + window.X);
                        window.Y = Math.Max(0, centerOffsetY + window.Y);
                    }
                    window.ClientRectangle = new System.Drawing.Rectangle(targetBounds.X,
                        targetBounds.Y, targetBounds.Width, targetBounds.Height);
                }


                // we need to create a small delay between resizing the window
                // and changing the border to avoid OpenTK Linux bug
                if (CurrentPlatform.OS == OS.Linux)
                    updateborder = 2;
                else
                    UpdateBorder();

                var context = GraphicsContext.CurrentContext;
                if (context != null)
                    context.Update(window.WindowInfo);

                if (!Window.Visible)
                {
                    Window.Visible = true;

                    // Bug in OpenTK, it doesn't always set state if window is not visible
                    window.WindowState = windowState; 
                }
            }
        }

        private void HandleInput()
        {
            // mouse doesn't need to be treated here, Mouse class does it alone

            // keyboard
            Keyboard.SetKeys(keys);
        }

        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            OnTextInput(sender, new TextInputEventArgs(e.KeyChar));
        }
        
        #endregion

        private void Initialize(Game game)
        {
            Game = game;

            GraphicsContext.ShareContexts = true;

            window = new NativeWindow();
            window.WindowBorder = WindowBorder.Resizable;
            window.Closing += new EventHandler<CancelEventArgs>(OpenTkGameWindow_Closing);
            window.Resize += OnResize;
            window.KeyDown += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyDown);
            window.KeyUp += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyUp);

            window.KeyPress += OnKeyPress;

            var assembly = Assembly.GetEntryAssembly();
            var t = Type.GetType ("Mono.Runtime");

            Title = assembly != null ? AssemblyHelper.GetDefaultWindowTitle() : "MonoGame Application";

            // In case when DesktopGL dll is compiled using .Net, and you
            // try to load it using Mono, it will cause a crash because of this.
            try
            {
                if (t == null && assembly != null)
                    window.Icon = Icon.ExtractAssociatedIcon(assembly.Location);
                else {
                    using (var stream = Assembly.GetEntryAssembly().GetManifestResourceStream(string.Format("{0}.Icon.ico", Assembly.GetEntryAssembly().EntryPoint.DeclaringType.Namespace)) ?? 
                            Assembly.GetExecutingAssembly().GetManifestResourceStream("Microsoft.Xna.Framework.monogame.ico")) {
                        if (stream != null)
                           window.Icon = new Icon(stream);
                    }
                }
            }
            catch { }

            updateClientBounds = false;
            clientBounds = new Rectangle(window.ClientRectangle.X, window.ClientRectangle.Y,
                                         window.ClientRectangle.Width, window.ClientRectangle.Height);
            windowState = window.WindowState;            

            _windowHandle = window.WindowInfo.Handle;

            keys = new List<Keys>();

            // mouse
            // TODO review this when opentk 1.1 is released
#if DESKTOPGL || ANGLE
            Mouse.setWindows(this);
#else
            Mouse.UpdateMouseInfo(window.Mouse);
#endif

            // Default no resizing
            AllowUserResizing = false;

            // Default mouse cursor hidden 
            SetMouseVisible(false);
        }

        protected override void SetTitle(string title)
        {
            window.Title = title;            
        }

        internal void ToggleFullScreen()
        {
            if (windowState == WindowState.Fullscreen)
                windowState = WindowState.Normal;
            else
                windowState = WindowState.Fullscreen;
            updateClientBounds = true;
        }

        internal void ChangeClientBounds(Rectangle clientBounds)
        {
            if (this.clientBounds != clientBounds)
            {
                updateClientBounds = true;
                targetBounds = clientBounds;
            }
        }

        #endregion

        #region Public Methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Disposing of window will cause a crash on Linux
                    // tho it will get destroied anyway by not beeing updated
                    window.Close();
                }
                // The window handle no longer exists
                _windowHandle = IntPtr.Zero;

                disposed = true;
            }
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {

        }

        public void SetMouseVisible(bool visible)
        {
            window.Cursor = visible ? MouseCursor.Default : MouseCursor.Empty;
        }

        #endregion
    }
}

