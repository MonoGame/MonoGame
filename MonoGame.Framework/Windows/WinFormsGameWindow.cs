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
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using XnaKey = Microsoft.Xna.Framework.Input.Keys;

namespace MonoGame.Framework
{
    public class WinFormsGameWindow : GameWindow
    {
        private Form _form;

        private List<XnaKey> _keyState = new List<XnaKey>();

        private WinFormsGamePlatform _platform;

        private bool _isResizable;

        private bool _isBorderless;

        private bool _isMouseHidden;

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
            get { return _isResizable; }
            set
            {
                if (_isResizable != value)
                {
                    _isResizable = value;
                    _form.MaximizeBox = _isResizable;
                }
                else
                    return;
                if (_isBorderless)
                    return;
                _form.FormBorderStyle = _isResizable ? FormBorderStyle.Sizable : FormBorderStyle.FixedSingle;
            }
        }

        public override DisplayOrientation CurrentOrientation
        {
            get { return DisplayOrientation.Default; }
        }

        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {
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
                    _form.FormBorderStyle = FormBorderStyle.None;
                else
                    _form.FormBorderStyle = _isResizable ? FormBorderStyle.Sizable : FormBorderStyle.FixedSingle;
            }
        }

        #endregion

        internal WinFormsGameWindow(WinFormsGamePlatform platform)
        {
            _platform = platform;
            Game = platform.Game;

            _form = new Form();
            _form.Icon = Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly().Location);
            _form.MaximizeBox = false;
            _form.FormBorderStyle = FormBorderStyle.FixedSingle;
            _form.StartPosition = FormStartPosition.CenterScreen;

            Mouse.SetWindows(_form);

            // Capture mouse and keyboard events.
            _form.MouseDown += OnMouseState;
            _form.MouseMove += OnMouseState;
            _form.MouseUp += OnMouseState;
            _form.MouseWheel += OnMouseState;
            _form.KeyDown += OnKeyDown;
            _form.KeyUp += OnKeyUp;
            _form.MouseEnter += OnMouseEnter;
            _form.MouseLeave += OnMouseLeave;
            Keyboard.SetKeys(_keyState);

            _form.Activated += OnActivated;
            _form.Deactivate += OnDeactivate;
            _form.ClientSizeChanged += OnClientSizeChanged;
        }

        private void OnActivated(object sender, EventArgs eventArgs)
        {
            _platform.IsActive = true;
        }

        private void OnDeactivate(object sender, EventArgs eventArgs)
        {
            _platform.IsActive = false;
            _keyState.Clear();
        }

        private void OnMouseState(object sender, MouseEventArgs mouseEventArgs)
        {
            var previousState = Mouse.State.LeftButton;
            
            Mouse.State.X = mouseEventArgs.X;
            Mouse.State.Y = mouseEventArgs.Y;
            Mouse.State.LeftButton = (mouseEventArgs.Button & MouseButtons.Left) == MouseButtons.Left ? ButtonState.Pressed : ButtonState.Released;
            Mouse.State.MiddleButton = (mouseEventArgs.Button & MouseButtons.Middle) == MouseButtons.Middle ? ButtonState.Pressed : ButtonState.Released;
            Mouse.State.RightButton = (mouseEventArgs.Button & MouseButtons.Right) == MouseButtons.Right ? ButtonState.Pressed : ButtonState.Released;
            Mouse.State.ScrollWheelValue = mouseEventArgs.Delta;
            
            TouchLocationState? touchState = null;
            if (Mouse.State.LeftButton == ButtonState.Pressed)
                if (previousState == ButtonState.Released)
                    touchState = TouchLocationState.Pressed;
                else
                    touchState = TouchLocationState.Moved;
            else if (previousState == ButtonState.Pressed)
                touchState = TouchLocationState.Released;

            if (touchState.HasValue)
                TouchPanel.AddEvent(0, touchState.Value, new Vector2(Mouse.State.X, Mouse.State.Y), true);
        }

        private void OnKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            var key = (XnaKey)keyEventArgs.KeyCode;
            if (!_keyState.Contains(key))
                _keyState.Add(key);
        }

        private void OnKeyUp(object sender, KeyEventArgs keyEventArgs)
        {
            var key = (XnaKey)keyEventArgs.KeyCode;
            _keyState.Remove(key);
        }

        private void OnMouseEnter(object sender, EventArgs e)
        {
            if (!_platform.IsMouseVisible && !_isMouseHidden)
            {
                _isMouseHidden = true;
                Cursor.Hide();
            }
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            if (_isMouseHidden)
            {
                _isMouseHidden = false;
                Cursor.Show();
            }
        }

        internal void Initialize()
        {
            var manager = Game.graphicsDeviceManager;
            _form.ClientSize = new Size(manager.PreferredBackBufferWidth, manager.PreferredBackBufferHeight);
            _form.Show();
        }

        private void OnClientSizeChanged(object sender, EventArgs eventArgs)
        {
            var manager = Game.graphicsDeviceManager;

            // Set the default new back buffer size and viewport, but this
            // can be overloaded by the two events below.
            
            var newWidth = _form.ClientRectangle.Width;
            var newHeight = _form.ClientRectangle.Height;
            manager.PreferredBackBufferWidth = newWidth;
            manager.PreferredBackBufferHeight = newHeight;
            if (manager.GraphicsDevice == null)
                return;

            // Set the new view state which will trigger the 
            // Game.ApplicationViewChanged event and signal
            // the client size changed event.
            OnClientSizeChanged();
        }

        protected override void SetTitle(string title)
        {
            _form.Text = title;
        }

        internal void RunLoop()
        {
            Application.Idle += OnIdle;
            Application.Run(_form);
            Application.Idle -= OnIdle;
        }

        private void OnIdle(object sender, EventArgs eventArgs)
        {
            // While there are no pending messages 
            // to be processed tick the game.
            NativeMessage msg;
            while (!PeekMessage(out msg, IntPtr.Zero, 0, 0, 0))
                Game.Tick();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NativeMessage
        {
            public IntPtr handle;
            public uint msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public System.Drawing.Point p;
        }

        internal void ChangeClientSize(Size clientBounds)
        {
            this._form.ClientSize = clientBounds;
        }

        [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern bool PeekMessage(out NativeMessage msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, uint flags);

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

