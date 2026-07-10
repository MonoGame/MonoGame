// MonoGame - Copyright (C) MonoGame Foundation, Inc
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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Windows;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Keys = Microsoft.Xna.Framework.Input.Keys;
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

        private Point _locationBeforeFullScreen;
        // flag to indicate that we're switching to/from full screen and should ignore resize events
        private bool _switchingFullScreen;

        // true if window position was moved either through code or by dragging/resizing the form
        private bool _wasMoved;

        private bool _isResizeTickEnabled;
        private readonly System.Timers.Timer _resizeTickTimer;

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
                RefreshAdapter();
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

        public bool IsFullScreen { get; private set; }
        public bool HardwareModeSwitch { get; private set; }

        #endregion

        internal WinFormsGameWindow(WinFormsGamePlatform platform)
        {
            _platform = platform;
            Game = platform.Game;

            Form = new WinFormsGameForm(this);
            ChangeClientSize(new Size(GraphicsDeviceManager.DefaultBackBufferWidth, GraphicsDeviceManager.DefaultBackBufferHeight));

            SetIcon();
            Title = MonoGame.Framework.Utilities.AssemblyHelper.GetDefaultWindowTitle();

            Form.MaximizeBox = false;
            Form.FormBorderStyle = FormBorderStyle.FixedSingle;
            Form.StartPosition = FormStartPosition.Manual;

            // Capture mouse events.
            Mouse.WindowHandle = Form.Handle;
            Form.MouseWheel += OnMouseScroll;
            Form.MouseHorizontalWheel += OnMouseHorizontalScroll;
            Form.MouseEnter += OnMouseEnter;
            Form.MouseLeave += OnMouseLeave;            

            _resizeTickTimer = new System.Timers.Timer(1) { SynchronizingObject = Form, AutoReset = false };
            _resizeTickTimer.Elapsed += OnResizeTick;

            Form.FormClosing += OnFormClosing;
            Form.Activated += OnActivated;
            Form.Deactivate += OnDeactivate;
            Form.Resize += OnResize;
            Form.ResizeBegin += OnResizeBegin;
            Form.ResizeEnd += OnResizeEnd;

            Form.KeyPress += OnKeyPress;

            RegisterToAllWindows();
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct POINTSTRUCT
        {
            public int X;
            public int Y;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
        private static extern IntPtr ExtractIcon(IntPtr hInst, string exeFileName, int iconIndex);
        
        [DllImport("user32.dll", ExactSpelling=true, CharSet=CharSet.Auto)]
        [return: MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(out POINTSTRUCT pt);
        
        [DllImport("user32.dll", ExactSpelling=true, CharSet=CharSet.Auto)]
        internal static extern int MapWindowPoints(HandleRef hWndFrom, HandleRef hWndTo, out POINTSTRUCT pt, int cPoints);

        [DllImport("shell32.dll")]
        private static extern void DragAcceptFiles(IntPtr hwnd, bool fAccept);

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

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            _platform.Game.Exit();
        }

        private void OnActivated(object sender, EventArgs eventArgs)
        {
            _platform.IsActive = true;
            Keyboard.SetActive(true);

            DragAcceptFiles(Handle, true); //allows drag and dropping
        }

        private void OnDeactivate(object sender, EventArgs eventArgs)
        {
            // If in exclusive mode full-screen, force it out of exclusive mode and minimize the window
			if( IsFullScreen && _platform.Game.GraphicsDevice.PresentationParameters.HardwareModeSwitch ) {			
				// This is true when the user presses the Windows key while game window has focus
				if( Form.WindowState == FormWindowState.Minimized )
					MinimizeFullScreen();				
			}
            _platform.IsActive = false;
            Keyboard.SetActive(false);
        }

        private void OnMouseScroll(object sender, MouseEventArgs mouseEventArgs)
        {
            MouseState.ScrollWheelValue += mouseEventArgs.Delta;
        }

        private void OnMouseHorizontalScroll(object sender, HorizontalMouseWheelEventArgs mouseEventArgs)
        {
            MouseState.HorizontalScrollWheelValue += mouseEventArgs.Delta;
        }

        private void UpdateMouseState()
        {
            // If we call the form client functions before the form has
            // been made visible it will cause the wrong window size to
            // be applied at startup.
            if (!Form.Visible)
                return;

            POINTSTRUCT pos;
            GetCursorPos(out pos);
            MapWindowPoints(new HandleRef(null, IntPtr.Zero), new HandleRef(Form, Form.Handle), out pos, 1);
            var clientPos = new System.Drawing.Point(pos.X, pos.Y);
            var withinClient = Form.ClientRectangle.Contains(clientPos);
            var buttons = Control.MouseButtons;

            var previousState = MouseState.LeftButton;

            MouseState.X = clientPos.X;
            MouseState.Y = clientPos.Y;
            MouseState.LeftButton = (buttons & MouseButtons.Left) == MouseButtons.Left ? ButtonState.Pressed : ButtonState.Released;
            MouseState.MiddleButton = (buttons & MouseButtons.Middle) == MouseButtons.Middle ? ButtonState.Pressed : ButtonState.Released;
            MouseState.RightButton = (buttons & MouseButtons.Right) == MouseButtons.Right ? ButtonState.Pressed : ButtonState.Released;
            MouseState.XButton1 = (buttons & MouseButtons.XButton1) == MouseButtons.XButton1 ? ButtonState.Pressed : ButtonState.Released;
            MouseState.XButton2 = (buttons & MouseButtons.XButton2) == MouseButtons.XButton2 ? ButtonState.Pressed : ButtonState.Released;

            // Don't process touch state if we're not active 
            // and the mouse is within the client area.
            if (!_platform.IsActive || !withinClient)
            {                
                if (MouseState.LeftButton == ButtonState.Pressed)
                {
                    // Release mouse TouchLocation
                    var touchX = MathHelper.Clamp(MouseState.X, 0, Form.ClientRectangle.Width-1);
                    var touchY = MathHelper.Clamp(MouseState.Y, 0, Form.ClientRectangle.Height-1);
                    TouchPanelState.AddEvent(0, TouchLocationState.Released, new Vector2(touchX, touchY), true);
                }
                return;
            }
            
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

        [DllImport("user32.dll")]
        private static extern short VkKeyScanEx(char ch, IntPtr dwhkl);

        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            var key = (Keys) (VkKeyScanEx(e.KeyChar, InputLanguage.CurrentInputLanguage.Handle) & 0xff);
            OnTextInput(new TextInputEventArgs(e.KeyChar, key));
        }

        internal void Initialize(int width, int height)
        {
            ChangeClientSize(new Size(width, height));
        }

        internal void Initialize(PresentationParameters pp)
        {
            ChangeClientSize(new Size(pp.BackBufferWidth, pp.BackBufferHeight));

            if (pp.IsFullScreen)
            {
                EnterFullScreen(pp);
                if (!pp.HardwareModeSwitch)
                    _platform.Game.GraphicsDevice.OnPresentationChanged();
            }
        }

        private FormWindowState _lastFormState;

        private void OnResize(object sender, EventArgs eventArgs)
        {
            if (_switchingFullScreen || Form.IsResizing)
                return;

            // this event can be triggered when moving the window through Windows hotkeys
            // in that case we should no longer center the window after resize
            if (_lastFormState == Form.WindowState)
                _wasMoved = true;

            if (Game.Window == this && Form.WindowState != FormWindowState.Minimized) {
                // we may need to restore full screen when coming back from a minimized window
                if (_lastFormState == FormWindowState.Minimized)
                    _platform.Game.GraphicsDevice.SetHardwareFullscreen();
                UpdateBackBufferSize();
            }

            _lastFormState = Form.WindowState;
            OnClientSizeChanged();
        }

        private void OnResizeBegin(object sender, EventArgs e)
        {
            _isResizeTickEnabled = true;
            _resizeTickTimer.Enabled = true;
        }

        private void OnResizeTick(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!_isResizeTickEnabled)
                return;
            UpdateWindows();
            Game.Tick();
            _resizeTickTimer.Enabled = true;
        }

        private void OnResizeEnd(object sender, EventArgs eventArgs)
        {
            _isResizeTickEnabled = false;
            _resizeTickTimer.Enabled = false;

            _wasMoved = true;
            if (Game.Window == this)
            {
                UpdateBackBufferSize();
                RefreshAdapter();
            }

            OnClientSizeChanged();
        }

        private void RefreshAdapter()
        {
            // the display that the window is on might have changed, so we need to
            // check and possibly update the Adapter of the GraphicsDevice
            if (Game.GraphicsDevice != null)
                Game.GraphicsDevice.RefreshAdapter();
        }

        private void UpdateBackBufferSize()
        {
            var manager = Game.graphicsDeviceManager;
            if (manager.GraphicsDevice == null)
                return;

            var newSize = Form.ClientSize;
            if (newSize.Width == manager.PreferredBackBufferWidth
                && newSize.Height == manager.PreferredBackBufferHeight)
                return;

            // Set the default new back buffer size
            manager.PreferredBackBufferWidth = newSize.Width;
            manager.PreferredBackBufferHeight = newSize.Height;
            manager.ApplyChanges();
        }

        protected override void SetTitle(string title)
        {
            Form.Text = title;
        }

        internal void RunLoop()
        {
            Application.Idle += TickOnIdle;
            Application.Run(Form);
            Application.Idle -= TickOnIdle;

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

        // Run game loop when the app becomes Idle.
        private void TickOnIdle(object sender, EventArgs e)
        {
            var nativeMsg = new NativeMessage();
            do
            {
                UpdateWindows();
                Game.Tick();
            }
            while (!PeekMessage(out nativeMsg, IntPtr.Zero, 0, 0, 0) && Form != null && Form.IsDisposed == false);
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
            var prevIsResizing = Form.IsResizing;
            // make sure we don't see the events from this as a user resize
            Form.IsResizing = true;

            if(this.Form.ClientSize != clientBounds)
                this.Form.ClientSize = clientBounds;

            // if the window wasn't moved manually and it's resized, it should be centered
            if (!_wasMoved)
                Form.CenterOnPrimaryMonitor();

            Form.IsResizing = prevIsResizing;
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
            Mouse.WindowHandle = IntPtr.Zero;
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

        internal void OnPresentationChanged(PresentationParameters pp)
        {
            var raiseClientSizeChanged = false;
            if (pp.IsFullScreen && pp.HardwareModeSwitch && IsFullScreen && HardwareModeSwitch)
            {
                if( _platform.IsActive ) {
					// stay in hardware full screen, need to call ResizeTargets so the displaymode can be switched
					_platform.Game.GraphicsDevice.ResizeTargets();
				} else {
					// This needs to be called in case the user presses the Windows key while the focus is on the second monitor,
					//	which (sometimes) causes the window to exit fullscreen mode, but still keeps it visible
					MinimizeFullScreen();
				}
            }
            else if (pp.IsFullScreen && (!IsFullScreen || pp.HardwareModeSwitch != HardwareModeSwitch))
            {
                EnterFullScreen(pp);
                raiseClientSizeChanged = true;
            }
            else if (!pp.IsFullScreen && IsFullScreen)
            {
                ExitFullScreen();
                raiseClientSizeChanged = true;
            }

            ChangeClientSize(new Size(pp.BackBufferWidth, pp.BackBufferHeight));

            if (raiseClientSizeChanged)
                OnClientSizeChanged();
        }

        #endregion

        private void EnterFullScreen(PresentationParameters pp)
        {
            _switchingFullScreen = true;

            // store the location of the window so we can restore it later
            if (!IsFullScreen)
                _locationBeforeFullScreen = Form.Location;

            _platform.Game.GraphicsDevice.SetHardwareFullscreen();

            if (!pp.HardwareModeSwitch)
            {
                // FIXME: setting the WindowState to Maximized when the form is not shown will not update the ClientBounds
                // this causes the back buffer to be the wrong size when initializing in soft full screen
                // we show the form to bypass the issue
                Form.Show();
                IsBorderless = true;
                Form.WindowState = FormWindowState.Maximized;
                _lastFormState = FormWindowState.Maximized;
            }

            IsFullScreen = true;
            HardwareModeSwitch = pp.HardwareModeSwitch;

            _switchingFullScreen = false;
        }


        [DllImport("user32.dll")]
        static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, uint flags);

        private void ExitFullScreen()
        {
            _switchingFullScreen = true;

            _platform.Game.GraphicsDevice.ClearHardwareFullscreen();

            IsBorderless = false;
            Form.WindowState = FormWindowState.Normal;
            _lastFormState = FormWindowState.Normal;
            Form.Location = _locationBeforeFullScreen;
            IsFullScreen = false;

            // Windows does not always correctly redraw the desktop when exiting soft full screen, so force a redraw
            if (!HardwareModeSwitch)
                RedrawWindow(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 1);

            _switchingFullScreen = false;
        }

        private void MinimizeFullScreen()
        {
            _switchingFullScreen = true;

            _platform.Game.GraphicsDevice.ClearHardwareFullscreen();

            IsBorderless = false;
            Form.WindowState = FormWindowState.Minimized;
            _lastFormState = FormWindowState.Minimized;
            Form.Location = _locationBeforeFullScreen;
            IsFullScreen = false;

            // Windows does not always correctly redraw the desktop when exiting soft full screen, so force a redraw
            if (!HardwareModeSwitch)
                RedrawWindow(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 1);

            _switchingFullScreen = false;
        }
    }
}

