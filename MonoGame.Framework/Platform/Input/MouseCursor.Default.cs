// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Input
{
    public partial class MouseCursor
    {
        private static void PlatformInitalize()
        {
            Arrow = new MouseCursor(IntPtr.Zero);
            IBeam = new MouseCursor(IntPtr.Zero);
            Wait = new MouseCursor(IntPtr.Zero);
            Crosshair = new MouseCursor(IntPtr.Zero);
            WaitArrow = new MouseCursor(IntPtr.Zero);
            SizeNWSE = new MouseCursor(IntPtr.Zero);
            SizeNESW = new MouseCursor(IntPtr.Zero);
            SizeWE = new MouseCursor(IntPtr.Zero);
            SizeNS = new MouseCursor(IntPtr.Zero);
            SizeAll = new MouseCursor(IntPtr.Zero);
            No = new MouseCursor(IntPtr.Zero);
            Hand = new MouseCursor(IntPtr.Zero);
        }

        private static MouseCursor PlatformFromTexture2D(Texture2D texture, int originx, int originy)
        {
            return new MouseCursor(IntPtr.Zero);
        }

        private void PlatformDispose()
        {
        }
    }
}
