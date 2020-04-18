// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Interface to handle text composition.
    /// </summary>
    public interface IImeService
    {
        /// <summary>
        /// Enable the system IMM service to support composited character input.
        /// This should be called when you expect text input from a user and you support languages
        /// that require an IME (Input Method Editor).
        /// </summary>
        void StartTextInput();

        /// <summary>
        /// Stop the system IMM service.
        /// </summary>
        void StopTextInput();

        /// <summary>
        /// Show the Ime Candidate window rendered by the OS.
        /// Set to <c>false</c> if you want to render the IME candidate list yourself.
        /// On DesktopGL the candidate list is not exposed and this will always return <c>true</c>.
        /// </summary>
        bool ShowOSImeWindow { get; set; }

        /// <summary>
        /// Position Y of virtual keyboard, for mobile platforms has virtual keyboard.
        /// </summary>
        int VirtualKeyboardHeight { get; }

        /// <summary>
        /// Update the Ime service. For instance, stop the text input when clicking on the other
        /// screen area than the virtual keyboard poped up.
        /// </summary>
        void Update(GameTime gameTime);

        /// <summary>
        /// Returns true if text input is enabled, else returns false.
        /// </summary>
        bool IsTextInputActive { get; }

        /// <summary>
        /// Set the position of the candidate window rendered by the OS.
        /// Let the OS render the candidate window by setting <see cref="ShowOSImeWindow"/> to <c>true</c>.
        ///
        /// This API is supported on DesktopGL and WindowsDX.
        /// </summary>
        void SetTextInputRect(Rectangle rect);

        /// <summary>
        /// Invoked when the IMM service is enabled and a character composition is changed.
        /// </summary>
        event EventHandler<TextCompositionEventArgs> TextComposition;

        /// <summary>
        /// Invoked when the IMM service generates a composition result.
        /// </summary>
        event EventHandler<TextInputEventArgs> TextInput;
    }
}
