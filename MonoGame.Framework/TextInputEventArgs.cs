// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// This class is used for the game window's TextInput event as EventArgs.
    /// </summary>
    public struct TextInputEventArgs
    {
        private char _character;
        private Keys _key;
        
        public TextInputEventArgs(char character, Keys key = Keys.None)
        {
            _character = character;
            _key = key;
        }

        public char Character {
            get { return _character; }
            internal set
            {
                _character = value;
            }
        }

        public Keys Key
        {
            get { return _key; }
            internal set
            {
                _key = value;
            }
        }
    }
}
