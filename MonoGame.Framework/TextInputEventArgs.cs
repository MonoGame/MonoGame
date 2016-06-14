// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// An event containing keyboard text input data.  See
    /// <see cref="GameWindow.TextEvent"/> for details.
    /// </summary>
    public class TextInputEventArgs : EventArgs
    {
        /// <summary>
        /// Construct the event.
        /// </summary>
        /// <param name="character">The character value.</param>
        /// <param name="key">The key code.</param>
        public TextInputEventArgs(char character, Keys key)
        {
            Character = character;
            Key = key;
        }

        /// <summary>
        /// The character value for this event.
        /// </summary>
        public char Character { get; private set; }

        /// <summary>
        /// The key code for this event.
        /// </summary>
        public Keys Key { get; private set; }
    }
}
