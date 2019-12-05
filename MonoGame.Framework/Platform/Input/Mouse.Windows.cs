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
        private static Control _window;

        private static IntPtr PlatformGetWindowHandle()
        {
            return _window.Handle;
        }

        private static void PlatformSetWindowHandle(IntPtr windowHandle)
        {
            _window = Control.FromHandle(windowHandle);
        }

        private static MouseState PlatformGetState(GameWindow window)
        {
            return window.MouseState;
        }

        private static void SetPosition(GameWindow window, int x, int y)
        {
            window.MouseState.X = x;
            window.MouseState.Y = y;
            window.MouseStateSetPositionRequested = true;
        }

        public static void PlatformSetCursor(MouseCursor cursor)
        {
            _window.Cursor = cursor.Cursor;
        }
    }
}
