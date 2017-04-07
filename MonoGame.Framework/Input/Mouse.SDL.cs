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
            
            var newMouseState = window.MouseState;

            if ((winFlags & Sdl.Window.State.MouseFocus) != 0)
            {
                newMouseState.LeftButton = (state & Sdl.Mouse.Button.Left) != 0 ? ButtonState.Pressed : ButtonState.Released;
                newMouseState.MiddleButton = (state & Sdl.Mouse.Button.Middle) != 0 ? ButtonState.Pressed : ButtonState.Released;
                newMouseState.RightButton = (state & Sdl.Mouse.Button.Right) != 0 ? ButtonState.Pressed : ButtonState.Released;
                newMouseState.XButton1 = (state & Sdl.Mouse.Button.X1Mask) != 0 ? ButtonState.Pressed : ButtonState.Released;
                newMouseState.XButton2 = (state & Sdl.Mouse.Button.X2Mask) != 0 ? ButtonState.Pressed : ButtonState.Released;

                newMouseState.HorizontalScrollWheelValue = ScrollX;
                newMouseState.ScrollWheelValue = ScrollY;
            }

            if (Sdl.Patch > 4)
            {
                var clientBounds = window.ClientBounds;
                newMouseState.X = x - clientBounds.X;
                newMouseState.Y = y - clientBounds.Y;
            }
            else
            {
                newMouseState.X = x;
                newMouseState.Y = y;
            }

            window.MouseState = newMouseState;

            return window.MouseState;
        }

        private static void PlatformSetPosition(int x, int y)
        {
            var newMouseState = PrimaryWindow.MouseState;
            newMouseState.X = x;
            newMouseState.Y = y;
            PrimaryWindow.MouseState = newMouseState;
            
            Sdl.Mouse.WarpInWindow(PrimaryWindow.Handle, x, y);
        }

        public static void PlatformSetCursor(MouseCursor cursor)
        {
            Sdl.Mouse.SetCursor(cursor.Handle);
        }
    }
}
