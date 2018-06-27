using System;

namespace Microsoft.Xna.Framework
{
    public class TextCompositionEventArgs : EventArgs
    {
        public string CompositedText { get; }
        public Point CursorPosition { get; }

        public TextCompositionEventArgs(string compositedText, Point cursorPosition)
        {
            CompositedText = compositedText;
            CursorPosition = cursorPosition;
        }
    }
}
