// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Input
{
    /// <summary> 
    /// Allows interaction with joysticks. Unlike <see cref="GamePad"/> the number of Buttons/Axes/DPads is not limited.
    /// </summary>
    public static partial class Joystick
    {
        /// <summary>
        /// A default <see cref="JoystickState"/>.
        /// </summary>
        private static JoystickState _defaultJoystickState = new JoystickState
        {
            IsConnected = false,
            Axes = new int[0],
            Buttons = new ButtonState[0],
            Hats = new JoystickHat[0]
        };

        /// <summary>
        /// Gets a value indicating whether the current platform supports reading raw joystick data.
        /// </summary>
        /// <value><c>true</c> if the current platform supports reading raw joystick data; otherwise, <c>false</c>.</value>
        public static bool IsSupported
        {
            get { return PlatformIsSupported; }
        }

        /// <summary>
        /// Gets a value indicating the last joystick index connected to the system. If this value is less than 0, no joysticks are connected.
        /// <para>The order joysticks are connected and disconnected determines their index.
        /// As such, this value may be larger than 0 even if only one joystick is connected.
        /// </para>
        /// </summary>
        public static int LastConnectedIndex
        {
            get { return PlatformLastConnectedIndex; }
        }

        /// <summary>
        /// Gets the capabilities of the joystick.
        /// </summary>
        /// <param name="index">Index of the joystick you want to access.</param>
        /// <returns>The capabilities of the joystick.</returns>
        public static JoystickCapabilities GetCapabilities(int index)
        {
            return PlatformGetCapabilities(index);
        }

        /// <summary>
        /// Gets the current state of the joystick.
        /// </summary>
        /// <param name="index">Index of the joystick you want to access.</param>
        /// <returns>The state of the joystick.</returns>
        public static JoystickState GetState(int index)
        {
            return PlatformGetState(index);
        }

        /// <summary>
        /// Gets the current state of the joystick by updating an existing <see cref="JoystickState"/>.
        /// </summary>
        /// <param name="joystickState">The <see cref="JoystickState"/> to update.</param>
        /// <param name="index">Index of the joystick you want to access.</param>
        public static void GetState(ref JoystickState joystickState, int index)
        {
            PlatformGetState(ref joystickState, index);
        }
    }
}
