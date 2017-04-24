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

        /// <summary>
        /// Serves as a hash function for a <see cref="T:Microsoft.Xna.Framework.Input.JoystickHat"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            var hash = 0;

            if (Left == ButtonState.Pressed)
                hash |= (1 << 3);
            if (Up == ButtonState.Pressed)
                hash |= (1 << 2);
            if (Right == ButtonState.Pressed)
                hash |= (1 << 1);
            if (Down == ButtonState.Pressed)
                hash |= (1 << 0);

            return hash;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.JoystickHat"/> in a format of 0000 where each number represents a boolean value of each respecting object property: Left, Up, Right, Down.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.JoystickHat"/>.</returns>
        public override string ToString()
        {
            return "" + (int)Left + (int)Up + (int)Right + (int)Down;
        }
    }
}

