// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Input;

public static partial class Mouse
{
    private static IntPtr PlatformGetWindowHandle()
    {
        return IntPtr.Zero;
    }

    private static void PlatformSetWindowHandle(IntPtr windowHandle)
    {

    }

    private static MouseState PlatformGetState(GameWindow window)
    {
        return window.MouseState;
    }

    private static void PlatformSetPosition(int x, int y)
    {

    }

    /// <inheritdoc cref="Mouse.SetCursor"/>
    public static void PlatformSetCursor(MouseCursor cursor)
    {

    }
}
