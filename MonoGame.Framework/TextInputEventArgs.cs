using System;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// This class is used for the game window's TextInput event as EventArgs.
    /// </summary>
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
