using System;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Invoked when the IMM service is enabled and a character composition is changed.
    /// </summary>
    public class TextCompositionEventArgs : EventArgs
    {
        /// <summary>
        /// The full string as it's composited by the IMM.
        /// </summary>    
        public string CompositedText { get; }

        /// <summary>
        /// The position of the cursor inside the composited string.
        /// </summary>    
        public Point CursorPosition { get; }

        /// <summary>
        /// The suggested alternative texts for the composition.
        /// </summary>    
        public CandidateList CandidateList { get; }

        public TextCompositionEventArgs(string compositedText, Point cursorPosition, CandidateList candidateList)
        {
            CompositedText = compositedText;
            CursorPosition = cursorPosition;
            CandidateList = candidateList;
        }
    }
}
