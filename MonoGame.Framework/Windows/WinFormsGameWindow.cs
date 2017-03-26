﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Windows;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Point = System.Drawing.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using XnaPoint = Microsoft.Xna.Framework.Point;

namespace MonoGame.Framework
{
    class WinFormsGameWindow : GameWindow, IDisposable
    {
        internal WinFormsGameForm Form;

        static private ReaderWriterLockSlim _allWindowsReaderWriterLockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        static private List<WinFormsGameWindow> _allWindows = new List<WinFormsGameWindow>();

        private WinFormsGamePlatform _platform;

        private bool _isResizable;

        private bool _isBorderless;

        private bool _isMouseHidden;

        private bool _isMouseInBounds;

        // true if window position was moved either through code or by dragging/resizing the form
        private bool _wasMoved;

        #region Internal Properties

        internal Game Game { get; private set; }

        #endregion

        #region Public Properties

        public override IntPtr Handle { get { return Form.Handle; } }

        public override string ScreenDeviceName { get { return String.Empty; } }

        public override Rectangle ClientBounds
        {
            get
            {
                var position = Form.PointToScreen(Point.Empty);
                var size = Form.ClientSize;
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
                    Form.MaximizeBox = _isResizable;
                }
                else
                    return;
                if (_isBorderless)
                    return;
                Form.FormBorderStyle = _isResizable ? FormBorderStyle.Sizable : FormBorderStyle.FixedSingle;
            }
        }

        public override bool AllowAltF4
        {
             get { return base.AllowAltF4; }
             set
             {
                 Form.AllowAltF4 = value;
                 base.AllowAltF4 = value;
             }
        }

        public override DisplayOrientation CurrentOrientation
        {
            get { return DisplayOrientation.Default; }
        }

        public override XnaPoint Position
        {
            get { return new XnaPoint(Form.Location.X, Form.Location.Y); }
            set
            {
                _wasMoved = true;
                Form.Location = new Point(value.X, value.Y);
            }
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
                    Form.FormBorderStyle = FormBorderStyle.None;
                else
                    Form.FormBorderStyle = _isResizable ? FormBorderStyle.Sizable : FormBorderStyle.FixedSingle;
            }
        }

        #endregion

        internal WinFormsGameWindow(WinFormsGamePlatform platform)
        {
            _platform = platform;
            Game = platform.Game;

            Form = new WinFormsGameForm(this);
            Form.ClientSize = new Size(GraphicsDeviceManager.DefaultBackBufferWidth, GraphicsDeviceManager.DefaultBackBufferHeight);

            SetIcon();
            Title = Utilities.AssemblyHelper.GetDefaultWindowTitle();

            Form.MaximizeBox = false;
            Form.FormBorderStyle = FormBorderStyle.FixedSingle;
            Form.StartPosition = FormStartPosition.Manual;

            // Capture mouse events.
            Form.MouseWheel += OnMouseScroll;
            Form.MouseEnter += OnMouseEnter;
            Form.MouseLeave += OnMouseLeave;            

            Form.Activated += OnActivated;
            Form.Deactivate += OnDeactivate;
            Form.ResizeEnd += OnResizeEnd;

            Form.KeyPress += OnKeyPress;

            RegisterToAllWindows();

            Form.CenterOnPrimaryMonitor();
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
        private static extern IntPtr ExtractIcon(IntPtr hInst, string exeFileName, int iconIndex);

        private void SetIcon()
        {
            // When running unit tests this can return null.
            var assembly = Assembly.GetEntryAssembly();
            if (assembly == null)
                return;
            var handle = ExtractIcon(IntPtr.Zero, assembly.Location, 0);
            if (handle != IntPtr.Zero)
                Form.Icon = Icon.FromHandle(handle);
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
            _platform.IsActive = true;
            Keyboard.SetActive(true);
        }

        private void OnDeactivate(object sender, EventArgs eventArgs)
        {
            _platform.IsActive = false;
            Keyboard.SetActive(false);
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
            if (!Form.Visible)
                return;

            var clientPos = Form.PointToClient(Control.MousePosition);
            var withinClient = Form.ClientRectangle.Contains(clientPos);
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

        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            OnTextInput(sender, new TextInputEventArgs(e.KeyChar));
        }

        internal void Initialize(int width, int height)
        {
            Form.ClientSize = new Size(width, height);
            if (!_wasMoved)
                Form.CenterOnPrimaryMonitor();
        }

        private void OnResizeEnd(object sender, EventArgs eventArgs)
        {
            _wasMoved = true;
            if (Game.Window == this)
            {
                var manager = Game.graphicsDeviceManager;
                if (manager.GraphicsDevice == null)
                    return;

                var newSize = Form.ClientSize;
                if (newSize.Width == manager.PreferredBackBufferWidth
                    && newSize.Height == manager.PreferredBackBufferHeight)
                    return;

                // Set the default new back buffer size and viewport, but this
                // can be overloaded by the two events below.
                manager.PreferredBackBufferWidth = newSize.Width;
                manager.PreferredBackBufferHeight = newSize.Height;
                manager.ApplyChanges();
            }

            // Set the new view state which will trigger the 
            // Game.ApplicationViewChanged event and signal
            // the client size changed event.
            OnClientSizeChanged();
        }

        protected override void SetTitle(string title)
        {
            Form.Text = title;
        }

        internal void RunLoop()
        {
            // https://bugzilla.novell.com/show_bug.cgi?id=487896
            // Since there's existing bug from implementation with mono WinForms since 09'
            // Application.Idle is not working as intended
            // So we're just going to emulate Application.Run just like Microsoft implementation
            Form.Show();

            var nativeMsg = new NativeMessage();
            while (Form != null && Form.IsDisposed == false)
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
            while (PeekMessage(out msg, IntPtr.Zero, 0, 1 << 5, 1));
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
            if(this.Form.ClientSize != clientBounds)
                this.Form.ClientSize = clientBounds;

            // if the window wasn't moved manually and it's resized, it should be centered
            if (!_wasMoved)
                Form.CenterOnPrimaryMonitor();
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
                if (Form != null)
                {
                    UnregisterFromAllWindows(); 
                    Form.Dispose();
                    Form = null;
                }
            }
            _platform = null;
            Game = null;
            Mouse.Window = null;
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

