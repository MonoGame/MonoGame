// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// Describes current joystick state.
    /// </summary>
    public struct JoystickState
    {
        /// <summary>
        /// Gets a value indicating whether the joystick is connected.
        /// </summary>
        /// <value><c>true</c> if the joystick is connected; otherwise, <c>false</c>.</value>
        public bool IsConnected { get; internal set; }

        /// <summary>
        /// Gets the joystick axis values.
        /// </summary>
        /// <value>An array list of floats that indicate axis values.</value>
        public float[] Axes { get; internal set; }

        /// <summary>
        /// Gets the joystick button values.
        /// </summary>
        /// <value>An array list of ButtonState that indicate button values.</value>
        public ButtonState[] Buttons { get; internal set; }

        /// <summary>
        /// Gets the joystick hat values.
        /// </summary>
        /// <value>An array list of <see cref="JoystickHat"/> that indicate hat values.</value>
        public JoystickHat[] Hats{ get; internal set; }
    }
}

