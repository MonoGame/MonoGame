// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Interop;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Microsoft.Xna.Framework.Input;

public static partial class Keyboard
{
    // NOTE: Most keyboards can register 6 pressed keys at once
    // and some gaming keyboards can do up to 10 or 25 keys.
    //
    // For this reason a List<> is best here as in most of the time
    // it will perform faster than a HashSet or other collections.
    //
    internal static readonly List<Keys> Keys = new List<Keys>(32);

    private static unsafe KeyboardState PlatformGetState()
    {
        bool capsLock = Keys.Contains(Input.Keys.CapsLock);
        bool numLock = Keys.Contains(Input.Keys.NumLock);

        return new KeyboardState(Keys, capsLock, numLock);
    }
}
