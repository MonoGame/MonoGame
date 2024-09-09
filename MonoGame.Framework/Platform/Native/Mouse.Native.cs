// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Interop;
using System;

namespace Microsoft.Xna.Framework.Input;

public static partial class Mouse
{
    private static IntPtr PlatformGetWindowHandle()
    {
        // TODO: Multiple window support.

        return PrimaryWindow.Handle;
    }

    private static void PlatformSetWindowHandle(IntPtr windowHandle)
    {
        // TODO: Multiple window support.
    }

    private static unsafe MouseState PlatformGetState(GameWindow window)
    {
        // Mouse events keep this updated for each window.
        return window.MouseState;
    }

    private static unsafe void PlatformSetPosition(int x, int y)
    {
        // TODO: Multiple window support.

        PrimaryWindow.MouseState.X = x;
        PrimaryWindow.MouseState.Y = y;

        var window = PrimaryWindow as NativeGameWindow;

        MGP.Mouse_WarpPosition(window._handle, x, y);
    }

    private static unsafe void PlatformSetCursor(MouseCursor cursor)
    {
        // TODO: Multiple window support?

        var window = PrimaryWindow as NativeGameWindow;
        MGP.Window_SetCursor(window._handle, (MGP_Cursor*)cursor.Handle);
    }
}
