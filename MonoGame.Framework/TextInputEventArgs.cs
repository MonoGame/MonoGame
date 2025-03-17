// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// This class is used in the <see cref="GameWindow.TextInput"/> event as <see cref="EventArgs"/>.
    /// </summary>
    public struct TextInputEventArgs
    {

        /// <summary>
        /// The character from the text event.
        /// </summary>
        public readonly uint CharacterCodePoint;

        /// <summary>
        /// The character position of string.
        /// </summary>
        public readonly uint CharacterIndex;
        /// <summary>
        /// Creates an instance of <see cref="TextInputEventArgs"/>.
        /// </summary>
        /// <param name="characterCodePoint">The character from the text event</param>
        /// <param name="characterIndex">The character position of string.</param>
        public TextInputEventArgs(uint characterCodePoint, uint characterIndex = 0)
        {
            CharacterCodePoint = characterCodePoint;
            CharacterIndex = characterIndex;
        }

        /// <summary>
        /// the formatted character from the text event.
        /// </summary>
        public string Character => char.ConvertFromUtf32((int)CharacterCodePoint);
    }
}
