// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework
{
    public enum TextInputEventType
    {
        /// <summary>
        /// When new text is entered
        /// </summary>
        Input,

        /// <summary>
        /// This the current text that has not yet been entered but is still being edited
        /// </summary>
        Composition,
    }

    /// <summary>
    /// This class is used for the game window's TextInput event as EventArgs.
    /// </summary>
    public class TextInputEventArgs : EventArgs
    {
        public string Text = string.Empty;
        public TextInputEventType Type;

        public TextInputEventArgs(char text, Keys key = Keys.None)
        {
            this.Text = text.ToString();
            this.Key = key;
        }

        public TextInputEventArgs() { }

        public Keys Key { get; set; } = Keys.None;
    }
}
