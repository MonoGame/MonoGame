using System;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework
{
    public class InputKeyEventArgs : EventArgs
    {
        public Keys Key { get; private set; }

        public InputKeyEventArgs(Keys key)
        {
            Key = key;
        }
    }
}
