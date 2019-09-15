// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input
{
    public static partial class Keyboard
    {   
        static KeyboardState _keyboardState;
        static KeyboardState _nextKeyboardState;

        private static KeyboardState PlatformGetState()
        {
            return _keyboardState;
        }

        internal static void UpdateState()
        {
            _keyboardState = _nextKeyboardState;
        }

        internal static void SetKey(Keys key)
        {
            _nextKeyboardState.InternalSetKey(key);
        }

        internal static void ClearKey(Keys key)
        {
            _nextKeyboardState.InternalClearKey(key);
        }
                
        internal static void Clear()
        {
            _nextKeyboardState.InternalClearAllKeys();
        }

    }
}
