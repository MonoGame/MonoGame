// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Input;

public static partial class Keyboard
{
    private static KeyboardState PlatformGetState()
    {
        return new KeyboardState();
    }
}
