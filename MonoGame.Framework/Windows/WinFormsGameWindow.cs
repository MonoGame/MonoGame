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
using Microsoft.Xna.Framework.Windows;
using SharpDX.Multimedia;
using SharpDX.RawInput;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Point = System.Drawing.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using XnaKey = Microsoft.Xna.Framework.Input.Keys;
using XnaPoint = Microsoft.Xna.Framework.Point;

namespace MonoGame.Framework
{
    class WinFormsGameWindow : GameWindow
    {
        internal WinFormsGameForm _form;

        private readonly WinFormsGamePlatform _platform;

        private bool _isResizable;

        private bool _isBorderless;

        private bool _isMouseHidden;

        private bool _isMouseInBounds;

        private MouseButtons _mouseDownButtonsState;

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

        public override bool AllowAltF4
        {
             get { return base.AllowAltF4; }
             set
             {
                 _form.AllowAltF4 = value;
                 base.AllowAltF4 = value;
             }
        }

        public override DisplayOrientation CurrentOrientation
        {
            get { return DisplayOrientation.Default; }
        }

        public override XnaPoint Position
        {
            get { return new XnaPoint(_form.DesktopLocation.X, _form.DesktopLocation.Y); }
            set { _form.DesktopLocation = new Point(value.X, value.Y); }
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

        #region Non-Public Properties

        internal List<XnaKey> KeyState { get; set; }

        #endregion

        internal WinFormsGameWindow(WinFormsGamePlatform platform)
        {
            _platform = platform;
            Game = platform.Game;

            _form = new WinFormsGameForm(this);
            
            // When running unit tests this can return null.
            var assembly = Assembly.GetEntryAssembly();
            if (assembly != null)
                _form.Icon = Icon.ExtractAssociatedIcon(assembly.Location);
            Title = Utilities.AssemblyHelper.GetDefaultWindowTitle();

            _form.MaximizeBox = false;
            _form.FormBorderStyle = FormBorderStyle.FixedSingle;
            _form.StartPosition = FormStartPosition.CenterScreen;           

            // Capture mouse events.
            _form.MouseDown += OnMouseDown;
            _form.MouseMove += OnMouseState;
            _form.MouseUp += OnMouseUp;
            _form.MouseWheel += OnMouseState;
            _form.MouseEnter += OnMouseEnter;
            _form.MouseLeave += OnMouseLeave;            

            // Use RawInput to capture key events.
            Device.RegisterDevice(UsagePage.Generic, UsageId.GenericKeyboard, DeviceFlags.None);
            Device.KeyboardInput += OnRawKeyEvent;

            _form.Activated += OnActivated;
            _form.Deactivate += OnDeactivate;
            _form.ClientSizeChanged += OnClientSizeChanged;

            _form.KeyPress += OnKeyPress;
        }

        private void OnActivated(object sender, EventArgs eventArgs)
        {
            var buttons = Control.MouseButtons;
            var position = Control.MousePosition;
            _mouseDownButtonsState = buttons;
            OnMouseState(null, new MouseEventArgs(buttons, 0, position.X, position.Y, 0));

            _platform.IsActive = true;
        }

        private void OnDeactivate(object sender, EventArgs eventArgs)
        {
            _platform.IsActive = false;

            if (KeyState != null)
                KeyState.Clear();
        }

        private void OnMouseDown(object sender, MouseEventArgs mouseEventArgs)
        {
            _mouseDownButtonsState |= mouseEventArgs.Button;
            OnMouseState(sender, mouseEventArgs);
        }

        private void OnMouseUp(object sender, MouseEventArgs mouseEventArgs)
        {
            _mouseDownButtonsState &= ~mouseEventArgs.Button;
            OnMouseState(sender, mouseEventArgs);
        }

        private void OnMouseState(object sender, MouseEventArgs mouseEventArgs)
        {
            var previousState = MouseState.LeftButton;

            MouseState.X = mouseEventArgs.X;
            MouseState.Y = mouseEventArgs.Y;
            MouseState.LeftButton = (_mouseDownButtonsState & MouseButtons.Left) == MouseButtons.Left ? ButtonState.Pressed : ButtonState.Released;
            MouseState.MiddleButton = (_mouseDownButtonsState & MouseButtons.Middle) == MouseButtons.Middle ? ButtonState.Pressed : ButtonState.Released;
            MouseState.RightButton = (_mouseDownButtonsState & MouseButtons.Right) == MouseButtons.Right ? ButtonState.Pressed : ButtonState.Released;
            MouseState.ScrollWheelValue += mouseEventArgs.Delta;
            
            TouchLocationState? touchState = null;
            if (MouseState.LeftButton == ButtonState.Pressed)
                if (previousState == ButtonState.Released)
                    touchState = TouchLocationState.Pressed;
                else
                    touchState = TouchLocationState.Moved;
            else if (previousState == ButtonState.Pressed)
                touchState = TouchLocationState.Released;

            if (touchState.HasValue)
                TouchPanelState.AddEvent(0, touchState.Value, new Vector2(MouseState.X, MouseState.Y), true);
        } 

        private void OnMouseEnter(object sender, EventArgs e)
        {
            _isMouseInBounds = true;
            if (!_platform.IsMouseVisible && !_isMouseHidden)
            {
                _isMouseHidden = true;
                Cursor.Hide();
            }
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            _isMouseInBounds = false;
            if (_isMouseHidden)
            {
                _isMouseHidden = false;
                Cursor.Show();
            }
        }

        private void OnRawKeyEvent(object sender, KeyboardInputEventArgs args)
        {
            if (KeyState == null)
                return;

            if ((int)args.Key == 0xff)
            {
                // dead key, e.g. a "shift" automatically happens when using Up/Down/Left/Right
                return;
            }

            XnaKey xnaKey;

            switch (args.MakeCode)
            {
                case 0x2a: // LShift
                    xnaKey = XnaKey.LeftShift;
                    break;

                case 0x36: // RShift
                    xnaKey = XnaKey.RightShift;
                    break;

                case 0x1d: // Ctrl
                    xnaKey = (args.ScanCodeFlags & ScanCodeFlags.E0) != 0 ? XnaKey.RightControl : XnaKey.LeftControl;
                    break;

                case 0x38: // Alt
                    xnaKey = (args.ScanCodeFlags & ScanCodeFlags.E0) != 0 ? XnaKey.RightAlt : XnaKey.LeftAlt;
                    break;

                default:
                    xnaKey = (XnaKey)args.Key;
                    break;
            }

            if ((args.State == SharpDX.RawInput.KeyState.KeyDown || args.State == SharpDX.RawInput.KeyState.SystemKeyDown) && !KeyState.Contains(xnaKey))
                KeyState.Add(xnaKey);
            else if (args.State == SharpDX.RawInput.KeyState.KeyUp || args.State == SharpDX.RawInput.KeyState.SystemKeyUp)
                KeyState.Remove(xnaKey);
        }

        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            OnTextInput(sender, new TextInputEventArgs(e.KeyChar));
        }

        internal void Initialize(int width, int height)
        {            
            _form.ClientSize = new Size(width, height);
            _form.Show();
        }

        private void OnClientSizeChanged(object sender, EventArgs eventArgs)
        {
            if (Game.Window == this)
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
            }

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

        public void MouseVisibleToggled()
        {
            if (_platform.IsMouseVisible)
            {
                if (_isMouseHidden)
                {
                    Cursor.Show();
                    _isMouseHidden = false;
                }
            }
            else if (!_isMouseHidden && _isMouseInBounds)
            {
                Cursor.Hide();
                _isMouseHidden = true;
            }
        }

        #endregion
    }
}

