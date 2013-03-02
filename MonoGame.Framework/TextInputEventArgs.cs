using System;

namespace Microsoft.Xna.Framework
{
    public class TextInputEventArgs : EventArgs
    {
        char character;
        public TextInputEventArgs(char character)
        {
            this.character = character;
        }
        public char Character
        {
            get
            {
                return character;
            }
        }
    }
}
