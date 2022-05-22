﻿// MonoGame - Copyright (C) The MonoGame Team
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
            return GetCapabilities((int)playerIndex);
        }

        /// <summary>
        /// Returns the capabilites of the connected controller.
        /// </summary>
        /// <param name="index">Index for the controller you want to query.</param>
        /// <returns>The capabilites of the controller.</returns>
        public static GamePadCapabilities GetCapabilities(int index)
        {
            if (index < 0 || index >= PlatformGetMaxNumberOfGamePads())
                return new GamePadCapabilities();

            return PlatformGetCapabilities(index);
        }

        /// <summary>
        /// Gets the current state of a game pad controller with an independent axes dead zone.
        /// </summary>
        /// <param name="playerIndex">Player index for the controller you want to query.</param>
        /// <returns>The state of the controller.</returns>
        public static GamePadState GetState(PlayerIndex playerIndex)
        {
            return GetState((int)playerIndex, GamePadDeadZone.IndependentAxes);
        }

        /// <summary>
        /// Gets the current state of a game pad controller with an independent axes dead zone.
        /// </summary>
        /// <param name="index">Index for the controller you want to query.</param>
        /// <returns>The state of the controller.</returns>
        public static GamePadState GetState(int index)
        {
            return GetState(index, GamePadDeadZone.IndependentAxes);
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
            return GetState((int)playerIndex, deadZoneMode);
        }

        /// <summary>
        /// Gets the current state of a game pad controller, using a specified dead zone
        /// on analog stick positions.
        /// </summary>
        /// <param name="index">Index for the controller you want to query.</param>
        /// <param name="deadZoneMode">Enumerated value that specifies what dead zone type to use.</param>
        /// <returns>The state of the controller.</returns>
        public static GamePadState GetState(int index, GamePadDeadZone deadZoneMode)
        {           
            return GetState(index, deadZoneMode, deadZoneMode);
        }

        /// <summary>
        /// Gets the current state of a game pad controller, using a specified dead zone
        /// on analog stick positions.
        /// </summary>
        /// <param name="playerIndex">Player index for the controller you want to query.</param>
        /// <param name="leftDeadZoneMode">Enumerated value that specifies what dead zone type to use for the left stick.</param>
        /// <param name="rightDeadZoneMode">Enumerated value that specifies what dead zone type to use for the right stick.</param>
        /// <returns>The state of the controller.</returns>
        public static GamePadState GetState(PlayerIndex playerIndex, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
        {
            return GetState((int)playerIndex, leftDeadZoneMode, rightDeadZoneMode);
        }

        /// <summary>
        /// Gets the current state of a game pad controller, using a specified dead zone
        /// on analog stick positions.
        /// </summary>
        /// <param name="index">Index for the controller you want to query.</param>
        /// <param name="leftDeadZoneMode">Enumerated value that specifies what dead zone type to use for the left stick.</param>
        /// <param name="rightDeadZoneMode">Enumerated value that specifies what dead zone type to use for the right stick.</param>
        /// <returns>The state of the controller.</returns>
        public static GamePadState GetState(int index, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
        {
            if (index < 0 || index >= PlatformGetMaxNumberOfGamePads())
                return GamePadState.Default;

            return PlatformGetState(index, leftDeadZoneMode, rightDeadZoneMode);
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
            return SetVibration((int)playerIndex, leftMotor, rightMotor, 0.0f, 0.0f);
        }

        /// <summary>
        /// Sets the vibration motor speeds on the controller device if supported.
        /// </summary>
        /// <param name="playerIndex">Player index that identifies the controller to set.</param>
        /// <param name="leftMotor">The speed of the left motor, between 0.0 and 1.0. This motor is a low-frequency motor.</param>
        /// <param name="rightMotor">The speed of the right motor, between 0.0 and 1.0. This motor is a high-frequency motor.</param>
        /// <param name="leftTrigger">(Xbox One controller only) The speed of the left trigger motor, between 0.0 and 1.0. This motor is a high-frequency motor.</param>
        /// <param name="rightTrigger">(Xbox One controller only) The speed of the right trigger motor, between 0.0 and 1.0. This motor is a high-frequency motor.</param>
        /// <returns>Returns true if the vibration motors were set.</returns>
        public static bool SetVibration(PlayerIndex playerIndex, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger)
        {
            return SetVibration((int)playerIndex, leftMotor, rightMotor, leftTrigger, rightTrigger);
        }

        /// <summary>
        /// Sets the vibration motor speeds on the controller device if supported.
        /// </summary>
        /// <param name="index">Index for the controller you want to query.</param>
        /// <param name="leftMotor">The speed of the left motor, between 0.0 and 1.0. This motor is a low-frequency motor.</param>
        /// <param name="rightMotor">The speed of the right motor, between 0.0 and 1.0. This motor is a high-frequency motor.</param>
        /// <returns>Returns true if the vibration motors were set.</returns>
        public static bool SetVibration(int index, float leftMotor, float rightMotor)
        {           
            return SetVibration(index, leftMotor, rightMotor, 0.0f, 0.0f);
        }

        /// <summary>
        /// Sets the vibration motor speeds on the controller device if supported.
        /// </summary>
        /// <param name="index">Index for the controller you want to query.</param>
        /// <param name="leftMotor">The speed of the left motor, between 0.0 and 1.0. This motor is a low-frequency motor.</param>
        /// <param name="rightMotor">The speed of the right motor, between 0.0 and 1.0. This motor is a high-frequency motor.</param>
        /// <param name="leftTrigger">(Xbox One controller only) The speed of the left trigger motor, between 0.0 and 1.0. This motor is a high-frequency motor.</param>
        /// <param name="rightTrigger">(Xbox One controller only) The speed of the right trigger motor, between 0.0 and 1.0. This motor is a high-frequency motor.</param>
        /// <returns>Returns true if the vibration motors were set.</returns>
        public static bool SetVibration(int index, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger)
        {
            if (index < 0 || index >= PlatformGetMaxNumberOfGamePads())
                return false;

            return PlatformSetVibration(index, MathHelper.Clamp(leftMotor, 0.0f, 1.0f), MathHelper.Clamp(rightMotor, 0.0f, 1.0f), MathHelper.Clamp(leftTrigger, 0.0f, 1.0f), MathHelper.Clamp(rightTrigger, 0.0f, 1.0f));
        }

        /// <summary>
        /// The maximum number of game pads supported on this system.
        /// </summary>
        public static int MaximumGamePadCount
        {
            get { return PlatformGetMaxNumberOfGamePads(); }
        }
    }
}
