// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Input
{
    public static partial class Mouse
    {
        internal static int ScrollY;

        private static IntPtr PlatformGetHandle()
        {
            return PrimaryWindow.Handle;
        }

        private static bool PlatformCaptureMouse()
        {
            if (_isMouseCaptured)
                return true;

            int x, y;
            var state = Sdl.Mouse.GetState(out x, out y);

            if ((state & Sdl.Mouse.Button.Left) == 0)
                return false;

            _isMouseCaptured = (Sdl.Mouse.CaptureMouse(true) == 0);
            return _isMouseCaptured;
        }

        private static void PlatformFreeMouse()
        {
            if (!_isMouseCaptured)
                return;
            
            Sdl.Mouse.CaptureMouse(false);
            _isMouseCaptured = false;
        }

        private static MouseState PlatformGetState(GameWindow window)
        {
            int x, y;

            var winFlags = Sdl.Window.GetWindowFlags(window.Handle);
            var state = (Sdl.Patch > 4) ? // SDL 2.0.4 has a bug with Global Mouse
                    Sdl.Mouse.GetGlobalState(out x, out y) :
                    Sdl.Mouse.GetState(out x, out y);
            
            if (winFlags.HasFlag(Sdl.Window.State.MouseFocus))
            {
                window.MouseState.ScrollWheelValue = ScrollY;

                window.MouseState.LeftButton = ((state & Sdl.Mouse.Button.Left) != 0) ? ButtonState.Pressed : ButtonState.Released;
                window.MouseState.MiddleButton = ((state & Sdl.Mouse.Button.Middle) != 0) ? ButtonState.Pressed : ButtonState.Released;
                window.MouseState.RightButton = ((state & Sdl.Mouse.Button.Right) != 0) ? ButtonState.Pressed : ButtonState.Released;
                window.MouseState.XButton1 = ((state & Sdl.Mouse.Button.X1Mask) != 0) ? ButtonState.Pressed : ButtonState.Released;
                window.MouseState.XButton2 = ((state & Sdl.Mouse.Button.X2Mask) != 0) ? ButtonState.Pressed : ButtonState.Released;

                if (!_isMouseCaptured)
                {
                    window.MouseState.X = x;
                    window.MouseState.Y = y;

                    if (Sdl.Patch > 4)
                    {
                        var clientBounds = window.ClientBounds;
                        window.MouseState.X -= clientBounds.X;
                        window.MouseState.Y -= clientBounds.Y;
                    }
                }
            }

            return window.MouseState;
        }

        private static void PlatformSetPosition(int x, int y)
        {
            PrimaryWindow.MouseState.X = x;
            PrimaryWindow.MouseState.Y = y;
            
            Sdl.Mouse.WarpInWindow(PrimaryWindow.Handle, x, y);
        }

        public static void PlatformSetCursor(MouseCursor cursor)
        {
            Sdl.Mouse.SetCursor(cursor.Handle);
        }
    }
}
