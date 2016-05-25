// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework
{
    public class KeyInputEventArgs : EventArgs
    {
        public KeyInputEventArgs (Keys key)
        {
            this.Key = key;
        }

        public Keys Key { get; private set; }
    }
}

