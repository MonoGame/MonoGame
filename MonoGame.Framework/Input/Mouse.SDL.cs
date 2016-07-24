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

        private static MouseState PlatformGetState(GameWindow window)
        {
            int x, y;

            var winFlags = Sdl.Window.GetWindowFlags(window.Handle);
            var state = (Sdl.Patch > 4) ? // SDL 2.0.4 has a bug with Global Mouse
                    Sdl.Mouse.GetGlobalState(out x, out y) :
                    Sdl.Mouse.GetState(out x, out y);
            var clientBounds = window.ClientBounds;

            if (clientBounds.Contains(x, y) && winFlags.HasFlag(Sdl.Window.State.MouseFocus))
            {
                window.MouseState.LeftButton = (state.HasFlag(Sdl.Mouse.Button.Left)) ? ButtonState.Pressed : ButtonState.Released;
                window.MouseState.MiddleButton = (state.HasFlag(Sdl.Mouse.Button.Middle)) ? ButtonState.Pressed : ButtonState.Released;
                window.MouseState.RightButton = (state.HasFlag(Sdl.Mouse.Button.Right)) ? ButtonState.Pressed : ButtonState.Released;
                window.MouseState.XButton1 = (state.HasFlag(Sdl.Mouse.Button.X1Mask)) ? ButtonState.Pressed : ButtonState.Released;
                window.MouseState.XButton2 = (state.HasFlag(Sdl.Mouse.Button.X2Mask)) ? ButtonState.Pressed : ButtonState.Released;

                window.MouseState.ScrollWheelValue = ScrollY;
            }

            window.MouseState.X = x - ((Sdl.Patch > 4) ? clientBounds.X : 0);
            window.MouseState.Y = y - ((Sdl.Patch > 4) ? clientBounds.Y : 0);

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
