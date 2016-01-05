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
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
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
    class WinFormsGameWindow : GameWindow, IDisposable
    {
        internal WinFormsGameForm _form;

        static private ReaderWriterLockSlim _allWindowsReaderWriterLockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        static private List<WinFormsGameWindow> _allWindows = new List<WinFormsGameWindow>();

        private WinFormsGamePlatform _platform;

        private bool _isResizable;

        private bool _isBorderless;

        private bool _isMouseHidden;

        private bool _isMouseInBounds;

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
                var position = _form.PointToScreen(Point.Empty);
                var size = _form.ClientSize;
                return new Rectangle(position.X, position.Y, size.Width, size.Height);
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
            _form.MouseWheel += OnMouseScroll;
            _form.MouseEnter += OnMouseEnter;
            _form.MouseLeave += OnMouseLeave;            

            // Use RawInput to capture key events.
            Device.RegisterDevice(UsagePage.Generic, UsageId.GenericKeyboard, DeviceFlags.None);
            Device.KeyboardInput += OnRawKeyEvent;

            _form.Activated += OnActivated;
            _form.Deactivate += OnDeactivate;
            _form.ClientSizeChanged += OnClientSizeChanged;

            _form.KeyPress += OnKeyPress;

            RegisterToAllWindows();
        }

        ~WinFormsGameWindow()
        {
            Dispose(false);
        }

        private void RegisterToAllWindows()
        {
            _allWindowsReaderWriterLockSlim.EnterWriteLock();

            try
            {
                _allWindows.Add(this);
            }
            finally
            {
                _allWindowsReaderWriterLockSlim.ExitWriteLock();
            }
        }

        private void UnregisterFromAllWindows()
        {
            _allWindowsReaderWriterLockSlim.EnterWriteLock();

            try
            {
                _allWindows.Remove(this);
            }
            finally
            {
                _allWindowsReaderWriterLockSlim.ExitWriteLock();
            }
        }

        private void OnActivated(object sender, EventArgs eventArgs)
        {
#if (WINDOWS && DIRECTX)
            if (Game.GraphicsDevice != null)
            {
                if (Game.graphicsDeviceManager.HardwareModeSwitch)
                {
                    if (!_platform.IsActive && Game.GraphicsDevice.PresentationParameters.IsFullScreen)
                   {
                       Game.GraphicsDevice.PresentationParameters.IsFullScreen = true;
                       Game.GraphicsDevice.CreateSizeDependentResources(true);
                        Game.GraphicsDevice.ApplyRenderTargets(null);
                   }
                }
          }
#endif
            _platform.IsActive = true;
        }

        private void OnDeactivate(object sender, EventArgs eventArgs)
        {
            _platform.IsActive = false;

            if (KeyState != null)
                KeyState.Clear();
        }

        private void OnMouseScroll(object sender, MouseEventArgs mouseEventArgs)
        {
            MouseState.ScrollWheelValue += mouseEventArgs.Delta;
        }

        private void UpdateMouseState()
        {
            // If we call the form client functions before the form has
            // been made visible it will cause the wrong window size to
            // be applied at startup.
            if (!_form.Visible)
                return;

            var clientPos = _form.PointToClient(Control.MousePosition);
            var withinClient = _form.ClientRectangle.Contains(clientPos);
            var buttons = Control.MouseButtons;

            var previousState = MouseState.LeftButton;

            MouseState.X = clientPos.X;
            MouseState.Y = clientPos.Y;
            MouseState.LeftButton = (buttons & MouseButtons.Left) == MouseButtons.Left ? ButtonState.Pressed : ButtonState.Released;
            MouseState.MiddleButton = (buttons & MouseButtons.Middle) == MouseButtons.Middle ? ButtonState.Pressed : ButtonState.Released;
            MouseState.RightButton = (buttons & MouseButtons.Right) == MouseButtons.Right ? ButtonState.Pressed : ButtonState.Released;

            // Don't process touch state if we're not active 
            // and the mouse is within the client area.
            if (!_platform.IsActive || !withinClient)
                return;
            
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

#if !(WINDOWS && DIRECTX)
                manager.PreferredBackBufferWidth = newWidth;
                manager.PreferredBackBufferHeight = newHeight;
#endif
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
            // https://bugzilla.novell.com/show_bug.cgi?id=487896
            // Since there's existing bug from implementation with mono WinForms since 09'
            // Application.Idle is not working as intended
            // So we're just going to emulate Application.Run just like Microsoft implementation
            _form.Show();

            var nativeMsg = new NativeMessage();
            while (_form != null && _form.IsDisposed == false)
            {
                if (PeekMessage(out nativeMsg, IntPtr.Zero, 0, 0, 0))
                {
                    Application.DoEvents();

                    if (nativeMsg.msg == WM_QUIT)
                        break;

                    continue;
                }

                UpdateWindows();
                Game.Tick();
            }

            // We need to remove the WM_QUIT message in the message 
            // pump as it will keep us from restarting on this 
            // same thread.
            //
            // This is critical for some NUnit runners which
            // typically will run all the tests on the same
            // process/thread.

            var msg = new NativeMessage();
            do
            {
                if (msg.msg == WM_QUIT)
                    break;

                Thread.Sleep(100);
            } 
            while (PeekMessage(out msg, IntPtr.Zero, 0, 0, 1));
        }

        internal void UpdateWindows()
        {
            _allWindowsReaderWriterLockSlim.EnterReadLock();

            try
            {
                // Update the mouse state for each window.
                foreach (var window in _allWindows)
                    if (window.Game == Game)
                        window.UpdateMouseState();
            }
            finally
            {
                _allWindowsReaderWriterLockSlim.ExitReadLock();
            }
        }

        private const uint WM_QUIT = 0x12;

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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_form != null)
                {
                    UnregisterFromAllWindows(); 
                    _form.Dispose();
                    _form = null;
                }
            }
            _platform = null;
            Game = null;
            Mouse.SetWindows(null);
            Device.KeyboardInput -= OnRawKeyEvent;
            Device.RegisterDevice(UsagePage.Generic, UsageId.GenericKeyboard, DeviceFlags.Remove);
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

