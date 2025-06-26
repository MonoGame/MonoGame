// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input
{
    public static partial class Keyboard
    {
        static List<Keys> _keys;

        private static KeyboardState PlatformGetState()
        {
            return new KeyboardState(_keys);
        }

        internal static void SetKeys(List<Keys> keys)
        {
            _keys = keys;
        }
    }
}
