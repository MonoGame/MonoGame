// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Input
{
    public static partial class Mouse
    {
        internal static int ScrollX;
        internal static int ScrollY;

        private static IntPtr PlatformGetHandle()
        {
            return PrimaryWindow.Handle;
        }

        private static MouseState PlatformGetState(GameWindow window)
        {
            int x, y;

            var winFlags = Sdl.Window.GetWindowFlags(window.Handle);
            var state = (Sdl.Patch > 4) ? // SDL 2.0.4 has a bug with Global Mouse
                    Sdl.Mouse.GetGlobalState(out x, out y) :
                    Sdl.Mouse.GetState(out x, out y);
            
            if ((winFlags & Sdl.Window.State.MouseFocus) != 0)
            {
                window.MouseState.LeftButton = (state & Sdl.Mouse.Button.Left) != 0 ? ButtonState.Pressed : ButtonState.Released;
                window.MouseState.MiddleButton = (state & Sdl.Mouse.Button.Middle) != 0 ? ButtonState.Pressed : ButtonState.Released;
                window.MouseState.RightButton = (state & Sdl.Mouse.Button.Right) != 0 ? ButtonState.Pressed : ButtonState.Released;
                window.MouseState.XButton1 = (state & Sdl.Mouse.Button.X1Mask) != 0 ? ButtonState.Pressed : ButtonState.Released;
                window.MouseState.XButton2 = (state & Sdl.Mouse.Button.X2Mask) != 0 ? ButtonState.Pressed : ButtonState.Released;

                window.MouseState.HorizontalScrollWheelValue = ScrollX;
                window.MouseState.ScrollWheelValue = ScrollY;
            }

            if (Sdl.Patch > 4)
            {
                var clientBounds = window.ClientBounds;
                window.MouseState.X = x - clientBounds.X;
                window.MouseState.Y = y - clientBounds.Y;
            }
            else
            {
                window.MouseState.X = x;
                window.MouseState.Y = y;
            }

            return window.MouseState;
        }

        private static void PlatformSetPosition(int x, int y)
        {
            PrimaryWindow.MouseState.X = x;
            PrimaryWindow.MouseState.Y = y;
            
            Sdl.Mouse.WarpInWindow(PrimaryWindow.Handle, x, y);
        }

        private static void PlatformSetCursor(MouseCursor cursor)
        {
            Sdl.Mouse.SetCursor(cursor.Handle);
        }
    }
}
