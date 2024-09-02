// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Input;

public partial class MouseCursor
{
    private static void PlatformInitalize()
    {

    }

    private static MouseCursor PlatformFromTexture2D(Texture2D texture, int originx, int originy)
    {
        return new MouseCursor(IntPtr.Zero);
    }

    private void PlatformDispose()
    {

    }
}
