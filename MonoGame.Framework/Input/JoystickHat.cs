// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// Describes joystick hat state.
    /// </summary>
    public struct JoystickHat
    {
        /// <summary>
        /// Gets if joysticks hat "down" is pressed.
        /// </summary>
        /// <value><see cref="ButtonState.Pressed"/> if the button is pressed otherwise, <see cref="ButtonState.Released"/>.</value>
        public ButtonState Down
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets if joysticks hat "left" is pressed.
        /// </summary>
        /// <value><see cref="ButtonState.Pressed"/> if the button is pressed otherwise, <see cref="ButtonState.Released"/>.</value>
        public ButtonState Left
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets if joysticks hat "right" is pressed.
        /// </summary>
        /// <value><see cref="ButtonState.Pressed"/> if the button is pressed otherwise, <see cref="ButtonState.Released"/>.</value>
        public ButtonState Right
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets if joysticks hat "up" is pressed.
        /// </summary>
        /// <value><see cref="ButtonState.Pressed"/> if the button is pressed otherwise, <see cref="ButtonState.Released"/>.</value>
        public ButtonState Up
        {
            get;
            internal set;
        }
    }
}

