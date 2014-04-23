// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Input
{
    /// <summary> 
    /// Supports querying the game controllers and setting the vibration motors.
    /// </summary>
    public static partial class GamePad
    {
        /// <summary>
        /// Returns the capabilites of the connected controller.
        /// </summary>
        /// <param name="playerIndex">Player index for the controller you want to query.</param>
        /// <returns>The capabilites of the controller.</returns>
        public static GamePadCapabilities GetCapabilities(PlayerIndex playerIndex)
        {
            // Make sure the player index is in range.
            var index = (int)playerIndex;
            if (index < (int)PlayerIndex.One || index > (int)PlayerIndex.Four)
                throw new InvalidOperationException();

            return PlatformGetCapabilities(index);
        }

        
        /// <summary>
        /// Gets the current state of a game pad controller with an independent axes dead zone.
        /// </summary>
        /// <param name="playerIndex">Player index for the controller you want to query.</param>
        /// <returns>The state of the controller.</returns>
        public static GamePadState GetState(PlayerIndex playerIndex)
        {
            return GetState(playerIndex, GamePadDeadZone.IndependentAxes);
        }


        /// <summary>
        /// Gets the current state of a game pad controller, using a specified dead zone
        /// on analog stick positions.
        /// </summary>
        /// <param name="playerIndex">Player index for the controller you want to query.</param>
        /// <param name="deadZoneMode">Enumerated value that specifies what dead zone type to use.</param>
        /// <returns>The state of the controller.</returns>
        public static GamePadState GetState(PlayerIndex playerIndex, GamePadDeadZone deadZoneMode)
        {
            // Make sure the player index is in range.
            var index = (int)playerIndex;
            if (index < (int)PlayerIndex.One || index > (int)PlayerIndex.Four)
                throw new InvalidOperationException();

            return PlatformGetState(index, deadZoneMode);
        }


        /// <summary>
        /// Sets the vibration motor speeds on the controller device if supported.
        /// </summary>
        /// <param name="playerIndex">Player index that identifies the controller to set.</param>
        /// <param name="leftMotor">The speed of the left motor, between 0.0 and 1.0. This motor is a low-frequency motor.</param>
        /// <param name="rightMotor">The speed of the right motor, between 0.0 and 1.0. This motor is a high-frequency motor.</param>
        /// <returns>Returns true if the vibration motors were set.</returns>
        public static bool SetVibration(PlayerIndex playerIndex, float leftMotor, float rightMotor)
        {
            // Make sure the player index is in range.
            var index = (int)playerIndex;
            if (index < (int)PlayerIndex.One || index > (int)PlayerIndex.Four)
                throw new InvalidOperationException();

            return PlatformSetVibration(index, MathHelper.Clamp(leftMotor, 0.0f, 1.0f), MathHelper.Clamp(rightMotor, 0.0f, 1.0f));
        }
    }
}
