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
            var modifiers = Sdl.Keyboard.GetModState();
            return new KeyboardState(_keys,
                                     (modifiers & Sdl.Keyboard.Keymod.CapsLock) == Sdl.Keyboard.Keymod.CapsLock,
                                     (modifiers & Sdl.Keyboard.Keymod.NumLock) == Sdl.Keyboard.Keymod.NumLock);
        }

        internal static void SetKeys(List<Keys> keys)
        {
            _keys = keys;
        }
    }
}
