// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Arguments for the <see cref="ImmService.TextComposition" /> event.
    /// </summary>
    public struct TextCompositionEventArgs
    {
        /// <summary>
        /// Construct a text composition event.
        /// </summary>
        public TextCompositionEventArgs(IMEString compositionText,
                                        int cursorPosition,
                                        IMEString[] candidateList = null,
                                        int candidatePageStart = 0,
                                        int candidatePageSize = 0,
                                        int candidateSelection = 0)
        {
            CompositionText = compositionText;
            CursorPosition = cursorPosition;

            CandidateList = candidateList;
            CandidatePageStart = candidatePageStart;
            CandidatePageSize = candidatePageSize;
            CandidateSelection = candidateSelection;
        }

        /// <summary>
        /// The full string as it's composed by the IMM.
        /// </summary>    
        public readonly IMEString CompositionText;

        /// <summary>
        /// The position of the cursor inside the composed string.
        /// </summary>    
        public readonly int CursorPosition;

        /// <summary>
        /// The candidate text list for the composition.
        /// This property is only supported on WindowsDX and WindowsUniversal.
        /// If the composition string does not generate candidates this array is empty.
        /// </summary>    
        public readonly IMEString[] CandidateList;

        /// <summary>
        /// First candidate index of current page.
        /// </summary>
        public readonly int CandidatePageStart;

        /// <summary>
        /// How many candidates should display per page.
        /// </summary>
        public readonly int CandidatePageSize;

        /// <summary>
        /// The selected candidate index.
        /// </summary>
        public readonly int CandidateSelection;
    }
}
