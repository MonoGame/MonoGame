// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Input
{
    public static partial class Mouse
    {
        [DllImportAttribute("user32.dll", EntryPoint = "SetCursorPos")]
        [return: MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int X, int Y);

        private static Control _window;
        private static MouseInputWnd _mouseInputWnd = new MouseInputWnd();

        private static IntPtr PlatformGetWindowHandle()
        {
            return (_window == null) ? IntPtr.Zero : _window.Handle;
        }

        private static void PlatformSetWindowHandle(IntPtr windowHandle)
        {
            // Release old Handle
            if (_mouseInputWnd.Handle != IntPtr.Zero)
                _mouseInputWnd.ReleaseHandle();

            _window = Control.FromHandle(windowHandle);
            _mouseInputWnd.AssignHandle(windowHandle);
        }

        private static MouseState PlatformGetState(GameWindow window)
        {
            return window.MouseState;
        }

        private static MouseState PlatformGetState()
        {
            if (_window != null)
            {
                var screenPos = Control.MousePosition;
                var clientPos = _window.PointToClient(screenPos);
                var buttons = Control.MouseButtons;
                
                return new MouseState(
                    clientPos.X,
                    clientPos.Y,
                    _mouseInputWnd.ScrollWheelValue,
                    (buttons & MouseButtons.Left) == MouseButtons.Left ? ButtonState.Pressed : ButtonState.Released,
                    (buttons & MouseButtons.Middle) == MouseButtons.Middle ? ButtonState.Pressed : ButtonState.Released,
                    (buttons & MouseButtons.Right) == MouseButtons.Right ? ButtonState.Pressed : ButtonState.Released,                    
                    (buttons & MouseButtons.XButton1) == MouseButtons.XButton1 ? ButtonState.Pressed : ButtonState.Released,
                    (buttons & MouseButtons.XButton2) == MouseButtons.XButton2 ? ButtonState.Pressed : ButtonState.Released,
                    _mouseInputWnd.HorizontalScrollWheelValue
                    );
            }

            return _defaultState;
        }

        private static void PlatformSetPosition(int x, int y)
        {
            PrimaryWindow.MouseState.X = x;
            PrimaryWindow.MouseState.Y = y;
            
            var pt = _window.PointToScreen(new System.Drawing.Point(x, y));
            SetCursorPos(pt.X, pt.Y);
        }

        public static void PlatformSetCursor(MouseCursor cursor)
        {
            _window.Cursor = cursor.Cursor;
        }

        #region Nested class MouseInputWnd
        /// <remarks>
        /// Subclass WindowHandle to read WM_MOUSEWHEEL and WM_MOUSEHWHEEL messages
        /// </remarks>
        class MouseInputWnd : System.Windows.Forms.NativeWindow
        {
            const int WM_MOUSEWHEEL  = 0x020A;
            const int WM_MOUSEHWHEEL = 0x020E;
            
            public int ScrollWheelValue = 0;
            public int HorizontalScrollWheelValue = 0;

            protected override void WndProc(ref Message m)
            {
                switch (m.Msg)
                {
                    case WM_MOUSEWHEEL:
                        var delta = (short)(((ulong)m.WParam >> 16) & 0xffff);
                        ScrollWheelValue += delta;
                        break;
                    case WM_MOUSEHWHEEL:
                        var deltaH = (short)(((ulong)m.WParam >> 16) & 0xffff);
                        HorizontalScrollWheelValue += deltaH;
                        break;
                }

                base.WndProc(ref m);
            }
        }
        #endregion Nested class MouseInputWnd
    }
}
