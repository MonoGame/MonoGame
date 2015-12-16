// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using WGI = Windows.Gaming.Input;

namespace Microsoft.Xna.Framework.Input
{
    static partial class GamePad
    {
        internal static bool Back;

        private static int PlatformGetMaxNumberOfGamePads()
        {
            return 16;
        }

        private static GamePadCapabilities PlatformGetCapabilities(int index)
        {
            if (index >= WGI.Gamepad.Gamepads.Count)
                return new GamePadCapabilities();

            var gamepad = WGI.Gamepad.Gamepads[index];

            // we can't check gamepad capabilities for most stuff with Windows.Gaming.Input.Gamepad
            return new GamePadCapabilities
            {
                IsConnected = true,
                HasAButton = true,
                HasBButton = true,
                HasXButton = true,
                HasYButton = true,
                HasBackButton = true,
                HasStartButton = true,
                HasDPadDownButton = true,
                HasDPadLeftButton = true,
                HasDPadRightButton = true,
                HasDPadUpButton = true,
                HasLeftShoulderButton = true,
                HasRightShoulderButton = true,
                HasLeftStickButton = true,
                HasRightStickButton = true,
                HasLeftTrigger = true,
                HasRightTrigger = true,
                HasLeftXThumbStick = true,
                HasLeftYThumbStick = true,
                HasRightXThumbStick = true,
                HasRightYThumbStick = true,
                HasLeftVibrationMotor = true,
                HasRightVibrationMotor = true,
                HasVoiceSupport = (gamepad.Headset != null && !string.IsNullOrEmpty(gamepad.Headset.CaptureDeviceId)),
                HasBigButton = false //we can't detect the big button from Windows.Gaming.Input.Gamepad, so it's always false
            };
        }

        private static GamePadState GetDefaultState()
        {
            var state = new GamePadState();
            state.Buttons = new GamePadButtons(Back ? Buttons.Back : 0);
            return state;
        }

        private static GamePadState PlatformGetState(int index, GamePadDeadZone deadZoneMode)
        {
            if (index >= WGI.Gamepad.Gamepads.Count)
                return (index == 0 ? GetDefaultState() : GamePadState.Default);

            var state = WGI.Gamepad.Gamepads[index].GetCurrentReading();

            var sticks = new GamePadThumbSticks(
                    new Vector2((float)state.LeftThumbstickX, (float)state.LeftThumbstickY),
                    new Vector2((float)state.RightThumbstickX, (float)state.RightThumbstickY),
                    deadZoneMode
                );

            var triggers = new GamePadTriggers(
                    (float)state.LeftTrigger,
                    (float)state.RightTrigger
                );

            Buttons buttonStates =
                (state.Buttons.HasFlag(WGI.GamepadButtons.A) ? Buttons.A : 0) |
                (state.Buttons.HasFlag(WGI.GamepadButtons.B) ? Buttons.B : 0) |
                ((state.Buttons.HasFlag(WGI.GamepadButtons.View) || Back) ? Buttons.Back : 0) |
                0 | //BigButton is unavailable by Windows.Gaming.Input.Gamepad
                (state.Buttons.HasFlag(WGI.GamepadButtons.LeftShoulder) ? Buttons.LeftShoulder : 0) |
                (state.Buttons.HasFlag(WGI.GamepadButtons.LeftThumbstick) ? Buttons.LeftStick : 0) |
                (state.Buttons.HasFlag(WGI.GamepadButtons.RightShoulder) ? Buttons.RightShoulder : 0) |
                (state.Buttons.HasFlag(WGI.GamepadButtons.RightThumbstick) ? Buttons.RightStick : 0) |
                (state.Buttons.HasFlag(WGI.GamepadButtons.Menu) ? Buttons.Start : 0) |
                (state.Buttons.HasFlag(WGI.GamepadButtons.X) ? Buttons.X : 0) |
                (state.Buttons.HasFlag(WGI.GamepadButtons.Y) ? Buttons.Y : 0) |
                0;
            var buttons = new GamePadButtons(buttonStates);

            var dpad = new GamePadDPad(
                    state.Buttons.HasFlag(WGI.GamepadButtons.DPadUp) ? ButtonState.Pressed : ButtonState.Released,
                    state.Buttons.HasFlag(WGI.GamepadButtons.DPadDown) ? ButtonState.Pressed : ButtonState.Released,
                    state.Buttons.HasFlag(WGI.GamepadButtons.DPadLeft) ? ButtonState.Pressed : ButtonState.Released,
                    state.Buttons.HasFlag(WGI.GamepadButtons.DPadRight) ? ButtonState.Pressed : ButtonState.Released
                );

            var result = new GamePadState(sticks, triggers, buttons, dpad);
            result.PacketNumber = (int)state.Timestamp;
            return result;
        }

        private static bool PlatformSetVibration(int index, float leftMotor, float rightMotor)
        {
            if (index >= WGI.Gamepad.Gamepads.Count)
                return false;

            var gamepad = WGI.Gamepad.Gamepads[index];

            gamepad.Vibration = new WGI.GamepadVibration
            {
                LeftMotor = leftMotor,
                LeftTrigger = leftMotor,
                RightMotor = rightMotor,
                RightTrigger = rightMotor
            };

            return true;
        }
    }
}
