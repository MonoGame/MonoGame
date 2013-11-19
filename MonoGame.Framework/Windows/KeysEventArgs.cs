using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework
{
    // Amendment to OpenTK code to allow us to use our existing key processing logic (with a few slight alterations).
    public class KeysEventArgs : EventArgs
    {
        public Keys Keys { get; private set; }

        public KeysEventArgs(Microsoft.Xna.Framework.Input.Keys keys)
        {
            this.Keys = keys;
        }
    }
}
