// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Input
{
    public static partial class Keyboard
    {
        private static readonly Keys[] DefinedKeys;

        private static readonly byte[] _keyState = new byte[256];
        private static readonly List<Keys> _keys = new List<Keys>(10);

        private static bool _isActive;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetKeyboardState(byte[] lpKeyState);

        private static readonly Predicate<Keys> IsKeyReleasedPredicate = key => !IsKeyPressed(key);

        static Keyboard()
        {
            DefinedKeys = Enum.GetValues(typeof(Keys)).Cast<Keys>()
                .Where(k => ((int)k >= 1) && ((int)k <= 255))
                .ToArray();
        }

        private static KeyboardState PlatformGetState()
        {
            if (_isActive && GetKeyboardState(_keyState))
            {
                _keys.RemoveAll(IsKeyReleasedPredicate);

                foreach (var key in DefinedKeys)
                    if (IsKeyPressed(key) && !_keys.Contains(key))
                        _keys.Add(key);
            }

            return new KeyboardState(_keys, Console.CapsLock, Console.NumberLock);
        }

        private static bool IsKeyPressed(Keys key)
        {
            return ((_keyState[(int)key] & 0x80) != 0);
        }

        internal static void SetActive(bool isActive)
        {
            _isActive = isActive;
            if (!_isActive)
                _keys.Clear();
        }
    }
}
