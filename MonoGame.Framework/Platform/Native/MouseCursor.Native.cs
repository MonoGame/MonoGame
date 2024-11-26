// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Interop;

namespace Microsoft.Xna.Framework.Input;

public partial class MouseCursor
{
    private unsafe MouseCursor(SystemCursor cursor)
    {
        Handle = (nint)MGP.Cursor_Create(cursor);
    }

    private static void PlatformInitalize()
    {
        Arrow = new MouseCursor(SystemCursor.Arrow);
        IBeam = new MouseCursor(SystemCursor.IBeam);
        Wait = new MouseCursor(SystemCursor.Wait);
        Crosshair = new MouseCursor(SystemCursor.Crosshair);
        WaitArrow = new MouseCursor(SystemCursor.WaitArrow);
        SizeNWSE = new MouseCursor(SystemCursor.SizeNWSE);
        SizeNESW = new MouseCursor(SystemCursor.SizeNESW);
        SizeWE = new MouseCursor(SystemCursor.SizeWE);
        SizeNS = new MouseCursor(SystemCursor.SizeNS);
        SizeAll = new MouseCursor(SystemCursor.SizeAll);
        No = new MouseCursor(SystemCursor.No);
        Hand = new MouseCursor(SystemCursor.Hand);
    }

    private unsafe static MouseCursor PlatformFromTexture2D(Texture2D texture, int originx, int originy)
    {
        var bytes = new byte[texture.Width * texture.Height * 4];
        texture.GetData(bytes);

        var handle = MGP.Cursor_CreateCustom(bytes, texture.Width, texture.Height, originx, originy);

        return new MouseCursor((nint)handle);
    }

    private unsafe void PlatformDispose()
    {
        if (Handle == IntPtr.Zero)
            return;

        MGP.Cursor_Destroy((MGP_Cursor*)Handle);
        Handle = IntPtr.Zero;
    }
}
