// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Input
{
    public static partial class Keyboard
    {
        private static readonly byte[] DefinedKeyCodes;

        private static readonly byte[] _keyState = new byte[256];
        private static readonly List<Keys> _keys = new List<Keys>(10);

        private static bool _isActive;

        [DllImport("user32.dll")]
        private static extern bool GetKeyboardState(byte[] lpKeyState);

        private static readonly Predicate<Keys> IsKeyReleasedPredicate = key => IsKeyReleased((byte)key);

        static Keyboard()
        {
            var definedKeys = Enum.GetValues(typeof(Keys));
            var keyCodes = new List<byte>(Math.Min(definedKeys.Length, 255));
            foreach (var key in definedKeys)
            {
                var keyCode = (int)key;
                if ((keyCode >= 1) && (keyCode <= 255))
                    keyCodes.Add((byte)keyCode);
            }
            DefinedKeyCodes = keyCodes.ToArray();
        }

        private static KeyboardState PlatformGetState()
        {
            if (_isActive && GetKeyboardState(_keyState))
            {
                _keys.RemoveAll(IsKeyReleasedPredicate);

                foreach (var keyCode in DefinedKeyCodes)
                {
                    if (IsKeyReleased(keyCode))
                        continue;
                    var key = (Keys)keyCode;
                    if (!_keys.Contains(key))
                        _keys.Add(key);
                }
            }

            return new KeyboardState(_keys, Console.CapsLock, Console.NumberLock);
        }

        private static bool IsKeyReleased(byte keyCode)
        {
            return ((_keyState[keyCode] & 0x80) == 0);
        }

        internal static void SetActive(bool isActive)
        {
            _isActive = isActive;
            if (!_isActive)
                _keys.Clear();
        }
    }
}
