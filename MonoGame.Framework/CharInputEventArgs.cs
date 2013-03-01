using System;

namespace Microsoft.Xna.Framework
{
    public class CharInputEventArgs : EventArgs
    {
        private char data;
        public CharInputEventArgs(char data)
        {
            this.data = data;
        }
        public char Character
        {
            get
            {
                return data;
            }
        }
    }
}
