// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using GBF = SharpDX.XInput.GamepadButtonFlags;

namespace Microsoft.Xna.Framework.Input
{
    static partial class GamePad
    {
        internal static bool Back;

        private static readonly SharpDX.XInput.Controller[] _controllers = new[]
        {
            new SharpDX.XInput.Controller(SharpDX.XInput.UserIndex.One),
            new SharpDX.XInput.Controller(SharpDX.XInput.UserIndex.Two),
            new SharpDX.XInput.Controller(SharpDX.XInput.UserIndex.Three),
            new SharpDX.XInput.Controller(SharpDX.XInput.UserIndex.Four),
        };

        private static readonly bool[] _connected = new bool[4];
        private static readonly long[] _timeout = new long[4];
        private static readonly long TimeoutTicks = TimeSpan.FromSeconds(1).Ticks;

        private static int PlatformGetMaxNumberOfGamePads()
        {
            return 4;
        }

        private static GamePadCapabilities PlatformGetCapabilities(int index)
        {
            // If the device was disconneced then wait for 
            // the timeout to elapsed before we test it again.
            if (!_connected[index] && _timeout[index] > DateTime.UtcNow.Ticks)
                return new GamePadCapabilities();
      
            // Check to see if the device is connected.
            var controller = _controllers[index];
            _connected[index] = controller.IsConnected;

            // If the device is disconnected retry it after the
            // timeout period has elapsed to avoid the overhead.
            if (!_connected[index])
            {
                _timeout[index] = DateTime.UtcNow.Ticks + TimeoutTicks;
                return new GamePadCapabilities();
            }

            var capabilities = controller.GetCapabilities(SharpDX.XInput.DeviceQueryType.Any);
            var ret = new GamePadCapabilities();
            switch (capabilities.SubType)
            {
#if DIRECTX11_1
                case SharpDX.XInput.DeviceSubType.ArcadePad:
                    Debug.WriteLine("XInput's DeviceSubType.ArcadePad is not supported in XNA");
                    ret.GamePadType = Input.GamePadType.Unknown; // TODO: Should this be BigButtonPad?
                    break;
                case SharpDX.XInput.DeviceSubType.FlightStick:
                    ret.GamePadType = Input.GamePadType.FlightStick;
                    break;
                case SharpDX.XInput.DeviceSubType.GuitarAlternate:
                    ret.GamePadType = Input.GamePadType.AlternateGuitar;
                    break;
                case SharpDX.XInput.DeviceSubType.GuitarBass:
                    // Note: XNA doesn't distinguish between Guitar and GuitarBass, but 
                    // GuitarBass is identical to Guitar in XInput, distinguished only
                    // to help setup for those controllers. 
                    ret.GamePadType = Input.GamePadType.Guitar;
                    break;
                case SharpDX.XInput.DeviceSubType.Unknown:
                    ret.GamePadType = Input.GamePadType.Unknown;
                    break;
#endif
                case SharpDX.XInput.DeviceSubType.ArcadeStick:
                    ret.GamePadType = GamePadType.ArcadeStick;
                    break;
                case SharpDX.XInput.DeviceSubType.DancePad:
                    ret.GamePadType = GamePadType.DancePad;
                    break;
                case SharpDX.XInput.DeviceSubType.DrumKit:
                    ret.GamePadType = GamePadType.DrumKit;
                    break;

                case SharpDX.XInput.DeviceSubType.Gamepad:
                    ret.GamePadType = GamePadType.GamePad;
                    break;
                case SharpDX.XInput.DeviceSubType.Guitar:
                    ret.GamePadType = GamePadType.Guitar;
                    break;
                case SharpDX.XInput.DeviceSubType.Wheel:
                    ret.GamePadType = GamePadType.Wheel;
                    break;
                default:
                    Debug.WriteLine("unexpected XInput DeviceSubType: {0}", capabilities.SubType.ToString());
                    ret.GamePadType = GamePadType.Unknown;
                    break;
            }

            var gamepad = capabilities.Gamepad;

            // digital buttons
            var buttons = gamepad.Buttons;
            ret.HasAButton = (buttons & GBF.A) == GBF.A;
            ret.HasBackButton = (buttons & GBF.Back) == GBF.Back;
            ret.HasBButton = (buttons & GBF.B) == GBF.B;
            ret.HasBigButton = false; // TODO: what IS this? Is it related to GamePadType.BigGamePad?
            ret.HasDPadDownButton = (buttons & GBF.DPadDown) == GBF.DPadDown;
            ret.HasDPadLeftButton = (buttons & GBF.DPadLeft) == GBF.DPadLeft;
            ret.HasDPadRightButton = (buttons & GBF.DPadRight) == GBF.DPadRight;
            ret.HasDPadUpButton = (buttons & GBF.DPadUp) == GBF.DPadUp;
            ret.HasLeftShoulderButton = (buttons & GBF.LeftShoulder) == GBF.LeftShoulder;
            ret.HasLeftStickButton = (buttons & GBF.LeftThumb) == GBF.LeftThumb;
            ret.HasRightShoulderButton = (buttons & GBF.RightShoulder) == GBF.RightShoulder;
            ret.HasRightStickButton = (buttons & GBF.RightThumb) == GBF.RightThumb;
            ret.HasStartButton = (buttons & GBF.Start) == GBF.Start;
            ret.HasXButton = (buttons & GBF.X) == GBF.X;
            ret.HasYButton = (buttons & GBF.Y) == GBF.Y;

            // analog controls
            ret.HasRightTrigger = gamepad.LeftTrigger > 0;
            ret.HasRightXThumbStick = gamepad.RightThumbX != 0;
            ret.HasRightYThumbStick = gamepad.RightThumbY != 0;
            ret.HasLeftTrigger = gamepad.LeftTrigger > 0;
            ret.HasLeftXThumbStick = gamepad.LeftThumbX != 0;
            ret.HasLeftYThumbStick = gamepad.LeftThumbY != 0;

            // vibration
#if DIRECTX11_1
            bool hasForceFeedback = (capabilities.Flags & SharpDX.XInput.CapabilityFlags.FfbSupported) == SharpDX.XInput.CapabilityFlags.FfbSupported;
            ret.HasLeftVibrationMotor = hasForceFeedback && capabilities.Vibration.LeftMotorSpeed > 0;
            ret.HasRightVibrationMotor = hasForceFeedback && capabilities.Vibration.RightMotorSpeed > 0;
#else
            ret.HasLeftVibrationMotor = (capabilities.Vibration.LeftMotorSpeed > 0);
            ret.HasRightVibrationMotor = (capabilities.Vibration.RightMotorSpeed > 0);
#endif

            // other
            ret.IsConnected = controller.IsConnected;
            ret.HasVoiceSupport = (capabilities.Flags & SharpDX.XInput.CapabilityFlags.VoiceSupported) == SharpDX.XInput.CapabilityFlags.VoiceSupported;

            return ret;
        }

        private static GamePadState GetDefaultState()
        {
            var state = new GamePadState();
            state.Buttons = new GamePadButtons(Back ? Buttons.Back : 0);
            return state;
        }

        private static GamePadState PlatformGetState(int index, GamePadDeadZone deadZoneMode)
        {
            // If the device was disconneced then wait for 
            // the timeout to elapsed before we test it again.
            if (!_connected[index] && _timeout[index] > DateTime.UtcNow.Ticks)
                return GetDefaultState();

            int packetNumber = 0;

            // Try to get the controller state.
            var gamepad = new SharpDX.XInput.Gamepad();
            try
            {
                SharpDX.XInput.State xistate;
                var controller = _controllers[index];
                _connected[index] = controller.GetState(out xistate);
                packetNumber = xistate.PacketNumber;
                gamepad = xistate.Gamepad;
            }
            catch (Exception ex)
            {
            }

            // If the device is disconnected retry it after the
            // timeout period has elapsed to avoid the overhead.
            if (!_connected[index])
            {
                _timeout[index] = DateTime.UtcNow.Ticks + TimeoutTicks;
                return GetDefaultState();
            }

            var thumbSticks = new GamePadThumbSticks(
                leftPosition: new Vector2(gamepad.LeftThumbX, gamepad.LeftThumbY) / (float)short.MaxValue,
                rightPosition: new Vector2(gamepad.RightThumbX, gamepad.RightThumbY) / (float)short.MaxValue,
                    deadZoneMode: deadZoneMode);

            var triggers = new GamePadTriggers(
                    leftTrigger: gamepad.LeftTrigger / (float)byte.MaxValue,
                    rightTrigger: gamepad.RightTrigger / (float)byte.MaxValue);

            var dpadState = new GamePadDPad(
                upValue: ConvertToButtonState(gamepad.Buttons, SharpDX.XInput.GamepadButtonFlags.DPadUp),
                downValue: ConvertToButtonState(gamepad.Buttons, SharpDX.XInput.GamepadButtonFlags.DPadDown),
                leftValue: ConvertToButtonState(gamepad.Buttons, SharpDX.XInput.GamepadButtonFlags.DPadLeft),
                rightValue: ConvertToButtonState(gamepad.Buttons, SharpDX.XInput.GamepadButtonFlags.DPadRight));

            var buttons = ConvertToButtons(
                buttonFlags: gamepad.Buttons,
                leftThumbX: gamepad.LeftThumbX,
                leftThumbY: gamepad.LeftThumbY,
                rightThumbX: gamepad.RightThumbX,
                rightThumbY: gamepad.RightThumbY,
                leftTrigger: gamepad.LeftTrigger,
                rightTrigger: gamepad.RightTrigger);

            var state = new GamePadState(
                thumbSticks: thumbSticks,
                triggers: triggers,
                buttons: buttons,
                dPad: dpadState);

            state.PacketNumber = packetNumber;

            return state;
        }

        private static ButtonState ConvertToButtonState(
            SharpDX.XInput.GamepadButtonFlags buttonFlags,
            SharpDX.XInput.GamepadButtonFlags desiredButton)
        {
            return ((buttonFlags & desiredButton) == desiredButton) ? ButtonState.Pressed : ButtonState.Released;
        }

        private static Buttons AddButtonIfPressed(
            SharpDX.XInput.GamepadButtonFlags buttonFlags,
            SharpDX.XInput.GamepadButtonFlags xInputButton,
            Buttons xnaButton)
        {
            var buttonState = ((buttonFlags & xInputButton) == xInputButton) ? ButtonState.Pressed : ButtonState.Released;
            return buttonState == ButtonState.Pressed ? xnaButton : 0;
        }

        private static Buttons AddThumbstickButtons(
            short thumbX, short thumbY, short deadZone, 
            Buttons thumbstickLeft, 
            Buttons thumbStickRight, 
            Buttons thumbStickUp, 
            Buttons thumbStickDown)
        {
            // TODO: this needs adjustment. Very naive implementation. Doesn't match XNA yet
            var result = (Buttons)0;
            if (thumbX < -deadZone)
                result |= thumbstickLeft;
            if (thumbX > deadZone)
                result |= thumbStickRight;
            if (thumbY < -deadZone)
                result |= thumbStickDown;
            else if (thumbY > deadZone)
                result |= thumbStickUp;
            return result;
        }

        private static GamePadButtons ConvertToButtons(SharpDX.XInput.GamepadButtonFlags buttonFlags,
            short leftThumbX, short leftThumbY,
            short rightThumbX, short rightThumbY,
            byte leftTrigger,
            byte rightTrigger)
        {
            var ret = (Buttons)0;
            ret |= AddButtonIfPressed(buttonFlags, GBF.A, Buttons.A);
            ret |= AddButtonIfPressed(buttonFlags, GBF.B, Buttons.B);
            ret |= AddButtonIfPressed(buttonFlags, GBF.Back, Buttons.Back);
            ret |= AddButtonIfPressed(buttonFlags, GBF.DPadDown, Buttons.DPadDown);
            ret |= AddButtonIfPressed(buttonFlags, GBF.DPadLeft, Buttons.DPadLeft);
            ret |= AddButtonIfPressed(buttonFlags, GBF.DPadRight, Buttons.DPadRight);
            ret |= AddButtonIfPressed(buttonFlags, GBF.DPadUp, Buttons.DPadUp);
            ret |= AddButtonIfPressed(buttonFlags, GBF.LeftShoulder, Buttons.LeftShoulder);
            ret |= AddButtonIfPressed(buttonFlags, GBF.RightShoulder, Buttons.RightShoulder);
            ret |= AddButtonIfPressed(buttonFlags, GBF.LeftThumb, Buttons.LeftStick);
            ret |= AddButtonIfPressed(buttonFlags, GBF.RightThumb, Buttons.RightStick);
            ret |= AddButtonIfPressed(buttonFlags, GBF.Start, Buttons.Start);
            ret |= AddButtonIfPressed(buttonFlags, GBF.X, Buttons.X);
            ret |= AddButtonIfPressed(buttonFlags, GBF.Y, Buttons.Y);

            ret |= AddThumbstickButtons(leftThumbX, leftThumbY,
                SharpDX.XInput.Gamepad.LeftThumbDeadZone,
                Buttons.LeftThumbstickLeft, 
                Buttons.LeftThumbstickRight, 
                Buttons.LeftThumbstickUp, 
                Buttons.LeftThumbstickDown);

            ret |= AddThumbstickButtons(rightThumbX, rightThumbY,
                SharpDX.XInput.Gamepad.RightThumbDeadZone,
                Buttons.RightThumbstickLeft, 
                Buttons.RightThumbstickRight, 
                Buttons.RightThumbstickUp, 
                Buttons.RightThumbstickDown);

            if (leftTrigger >= SharpDX.XInput.Gamepad.TriggerThreshold)
                ret |= Buttons.LeftTrigger;

            if (rightTrigger >= SharpDX.XInput.Gamepad.TriggerThreshold)
                ret |= Buttons.RightTrigger;

            // Check for the hardware back button.
            if (Back)
                ret |= Buttons.Back;

            return new GamePadButtons(ret);
        }

        private static bool PlatformSetVibration(int index, float leftMotor, float rightMotor)
        {
            if (!_connected[index])
                return false;

            var controller = _controllers[index];
            var result = controller.SetVibration(new SharpDX.XInput.Vibration
            {
                LeftMotorSpeed = (ushort)(leftMotor * ushort.MaxValue),
                RightMotorSpeed = (ushort)(rightMotor * ushort.MaxValue),
            });

            return result == SharpDX.Result.Ok;
        }
    }
}
