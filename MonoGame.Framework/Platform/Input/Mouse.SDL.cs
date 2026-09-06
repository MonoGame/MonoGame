// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Input
{
    public static partial class Mouse
    {
        internal static int ScrollX;
        internal static int ScrollY;

        private static IntPtr PlatformGetWindowHandle()
        {
            return PrimaryWindow.Handle;
        }
        
        private static void PlatformSetWindowHandle(IntPtr windowHandle)
        {
        }

        private static MouseState PlatformGetState(GameWindow window)
        {
            int x, y;
            var winFlags = Sdl.Window.GetWindowFlags(window.Handle);
            var state = Sdl.Mouse.GetGlobalState(out x, out y);
            var clientBounds = window.ClientBounds;

            window.MouseState.LeftButton = (state & Sdl.Mouse.Button.Left) != 0 ? ButtonState.Pressed : ButtonState.Released;
            window.MouseState.MiddleButton = (state & Sdl.Mouse.Button.Middle) != 0 ? ButtonState.Pressed : ButtonState.Released;
            window.MouseState.RightButton = (state & Sdl.Mouse.Button.Right) != 0 ? ButtonState.Pressed : ButtonState.Released;
            window.MouseState.XButton1 = (state & Sdl.Mouse.Button.X1Mask) != 0 ? ButtonState.Pressed : ButtonState.Released;
            window.MouseState.XButton2 = (state & Sdl.Mouse.Button.X2Mask) != 0 ? ButtonState.Pressed : ButtonState.Released;

            window.MouseState.HorizontalScrollWheelValue = ScrollX;
            window.MouseState.ScrollWheelValue = ScrollY;

            var localX = x - clientBounds.X;
            var localY = y - clientBounds.Y;

            if (window is SdlGameWindow sdlGameWindow && sdlGameWindow.UseHighDpi)
            {
                localX = (int)Math.Round(localX * sdlGameWindow.DpiScaleX);
                localY = (int)Math.Round(localY * sdlGameWindow.DpiScaleY);
            }

            window.MouseState.X = localX;
            window.MouseState.Y = localY;

            return window.MouseState;
        }

        private static void PlatformSetPosition(int x, int y)
        {
            PrimaryWindow.MouseState.X = x;
            PrimaryWindow.MouseState.Y = y;

            var localX = x;
            var localY = y;
            if (PrimaryWindow is SdlGameWindow sdlGameWindow && sdlGameWindow.UseHighDpi)
            {
                localX = (int)Math.Round(x / sdlGameWindow.DpiScaleX);
                localY = (int)Math.Round(y / sdlGameWindow.DpiScaleY);
            }
            
            Sdl.Mouse.WarpInWindow(PrimaryWindow.Handle, localX, localY);
        }

        private static void PlatformSetCursor(MouseCursor cursor)
        {
            Sdl.Mouse.SetCursor(cursor.Handle);
        }
    }
}
