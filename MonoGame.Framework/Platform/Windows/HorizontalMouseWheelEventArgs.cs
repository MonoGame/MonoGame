using System;

namespace Microsoft.Xna.Framework.Windows
{
    internal class HorizontalMouseWheelEventArgs : EventArgs
    {
        internal int Delta { get; private set; }

        internal HorizontalMouseWheelEventArgs(int delta)
        {
            Delta = delta;
        }
    }
}
