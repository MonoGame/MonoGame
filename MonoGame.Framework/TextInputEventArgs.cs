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
        /// The text or character from the text event.
        /// </summary>
        public readonly string Text;
        /// <summary>
        /// Creates an instance of <see cref="TextInputEventArgs"/>.
        /// </summary>
        /// <param name="text">The text or character from the text event</param>
        public TextInputEventArgs(string text)
        {
            Text = text;
        }
    }
}
