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
#endregion License

using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Rectangle = Microsoft.Xna.Framework.Rectangle;


namespace MonoGame.Framework
{
    public class WinFormsGameWindow : GameWindow
    {
        private Form _form;

        #region Internal Properties

        internal Game Game { get; private set; }

        #endregion

        #region Public Properties

        public override IntPtr Handle { get { return _form.Handle; } }

        public override string ScreenDeviceName { get { return String.Empty; } }

        public override Rectangle ClientBounds
        {
            get
            {
                var clientRect = _form.ClientRectangle;
                return new Rectangle(clientRect.X, clientRect.Y, clientRect.Width, clientRect.Height);
            }
        }

        public override bool AllowUserResizing
        {
            get { return _form.FormBorderStyle == FormBorderStyle.Sizable; }
            set
            {
                _form.FormBorderStyle = value ? FormBorderStyle.Sizable : FormBorderStyle.Fixed3D;
            }
        }

        public override DisplayOrientation CurrentOrientation
        {
            get { return DisplayOrientation.Default; }
        }

        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {
        }

        #endregion

        public WinFormsGameWindow(Game game)
        {
            Game = game;

            _form = new Form();
            _form.Icon = Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly().Location);

            // Capture mouse events.
            _form.MouseDown += OnMouseState;
            _form.MouseMove += OnMouseState;
            _form.MouseUp += OnMouseState;
            _form.MouseWheel += OnMouseState;

            //_coreWindow.SizeChanged += Window_SizeChanged;
            //_coreWindow.Closed += Window_Closed;
            //_coreWindow.Activated += Window_FocusChanged;
            //_clientBounds = bounds;
        }

        private void OnMouseState(object sender, MouseEventArgs mouseEventArgs)
        {
            Mouse.State.X = mouseEventArgs.X;
            Mouse.State.Y = mouseEventArgs.Y;
            Mouse.State.LeftButton = (mouseEventArgs.Button & MouseButtons.Left) == MouseButtons.Left ? ButtonState.Pressed : ButtonState.Released;
            Mouse.State.MiddleButton = (mouseEventArgs.Button & MouseButtons.Middle) == MouseButtons.Middle ? ButtonState.Pressed : ButtonState.Released;
            Mouse.State.RightButton = (mouseEventArgs.Button & MouseButtons.Right) == MouseButtons.Right ? ButtonState.Pressed : ButtonState.Released;
            Mouse.State.ScrollWheelValue = mouseEventArgs.Delta;
        }

        internal void Initialize()
        {
            var manager = Game.graphicsDeviceManager;
            _form.ClientSize = new Size(manager.PreferredBackBufferWidth, manager.PreferredBackBufferHeight);
            _form.Show();
        }

        /*
        private void Window_FocusChanged(CoreWindow sender, WindowActivatedEventArgs args)
        {
            if (args.WindowActivationState == CoreWindowActivationState.Deactivated)
                Platform.IsActive = false;
            else
                Platform.IsActive = true;
        }

        private void Window_Closed(CoreWindow sender, CoreWindowEventArgs args)
        {
            Game.Exit();
        }

        private void Window_SizeChanged(CoreWindow sender, WindowSizeChangedEventArgs args)
        {
            var manager = Game.graphicsDeviceManager;

            // If we haven't calculated the back buffer scale then do it now.
            if (_backBufferScale == Vector2.Zero)
            {
                _backBufferScale = new Vector2( manager.PreferredBackBufferWidth/(float)_clientBounds.Width, 
                                                manager.PreferredBackBufferHeight/(float)_clientBounds.Height);
            }

            // Set the new client bounds.
            SetClientBounds(args.Size.Width, args.Size.Height);

            // Set the default new back buffer size and viewport, but this
            // can be overloaded by the two events below.
            
            var newWidth = (int)((_backBufferScale.X * _clientBounds.Width) + 0.5f);
            var newHeight = (int)((_backBufferScale.Y * _clientBounds.Height) + 0.5f);
            manager.PreferredBackBufferWidth = newWidth;
            manager.PreferredBackBufferHeight = newHeight;

            manager.GraphicsDevice.Viewport = new Viewport(0, 0, newWidth, newHeight);            

            // Set the new view state which will trigger the 
            // Game.ApplicationViewChanged event and signal
            // the client size changed event.
            Platform.ViewState = ApplicationView.Value;
            OnClientSizeChanged();

            // If we have a valid client bounds then 
            // update the graphics device.
            if (_clientBounds.Width > 0 && _clientBounds.Height > 0)
                manager.ApplyChanges();
        }
        */

        protected override void SetTitle(string title)
        {
            _form.Text = title;
        }

        internal void RunLoop()
        {
            //_form.Activate();
            Application.Idle += OnIdle;
            Application.Run(_form);
            Application.Idle -= OnIdle;
        }

        private void OnIdle(object sender, EventArgs eventArgs)
        {
            while (StillIdle)
                Game.Tick();
        }

        private bool StillIdle
        {
            get
            {
                Message msg;
                return !PeekMessage(out msg, IntPtr.Zero, 0, 0, 0);
            }
        }

        [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern bool PeekMessage(out Message msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, uint flags);

        #region Public Methods

        public void Dispose()
        {
            if (_form != null)
            {
                _form.Dispose();
                _form = null;
            }
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
        }

        #endregion
    }
}

