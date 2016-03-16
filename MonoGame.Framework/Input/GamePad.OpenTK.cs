// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

#if !(MONOMAC && !PLATFORM_MACOS_LEGACY)

namespace Microsoft.Xna.Framework.Input
{
    static partial class GamePad
    {
        #if DEBUG
        static bool prepDone = false;
        #endif

        static void PrepSettings()
        {
            #if DEBUG
            if (!prepDone)
            {
                try {
                    int count = 0;
                    for (int i = 0; i < 4; i++) 
                    {
                        if (OpenTK.Input.GamePad.GetState (i).IsConnected) 
                        {
                            count++;
                        }
                    }
                    Console.WriteLine("Number of joysticks: " + count);
                } catch (Exception ex) {
                    Console.WriteLine("Unable to determine number of joysticks.");
                    Console.WriteLine(ex.Message);
                }
            }

            prepDone = true;
            #endif
        }

        private static int PlatformGetMaxNumberOfGamePads()
        {
            return 16;
        }

        private static GamePadCapabilities PlatformGetCapabilities(int index)
        {
            PrepSettings ();

            var capabilitiesTK = OpenTK.Input.GamePad.GetCapabilities (index);
            if (capabilitiesTK.GamePadType == OpenTK.Input.GamePadType.Unknown) 
            {
                return new GamePadCapabilities ();
            }

            return new GamePadCapabilities 
            {
                IsConnected = true, // otherwise, GamePadType would have been Unknown
                HasAButton = capabilitiesTK.HasAButton,
                HasBButton = capabilitiesTK.HasBButton,
                HasXButton = capabilitiesTK.HasXButton,
                HasYButton = capabilitiesTK.HasYButton,
                HasBackButton = capabilitiesTK.HasBackButton,
                HasStartButton = capabilitiesTK.HasStartButton,
                HasDPadDownButton = capabilitiesTK.HasDPadDownButton,
                HasDPadLeftButton = capabilitiesTK.HasDPadLeftButton,
                HasDPadRightButton = capabilitiesTK.HasDPadRightButton,
                HasDPadUpButton = capabilitiesTK.HasDPadUpButton,
                HasLeftShoulderButton = capabilitiesTK.HasLeftShoulderButton,
                HasRightShoulderButton = capabilitiesTK.HasRightShoulderButton,
                HasLeftStickButton = capabilitiesTK.HasLeftStickButton,
                HasRightStickButton = capabilitiesTK.HasRightStickButton,
                HasLeftTrigger = capabilitiesTK.HasLeftTrigger,
                HasRightTrigger = capabilitiesTK.HasRightTrigger,
                HasLeftXThumbStick = capabilitiesTK.HasLeftXThumbStick,
                HasLeftYThumbStick = capabilitiesTK.HasLeftYThumbStick,
                HasRightXThumbStick = capabilitiesTK.HasRightXThumbStick,
                HasRightYThumbStick = capabilitiesTK.HasRightYThumbStick,
                HasLeftVibrationMotor = capabilitiesTK.HasLeftVibrationMotor,
                HasRightVibrationMotor = capabilitiesTK.HasRightVibrationMotor,
                HasVoiceSupport = capabilitiesTK.HasVoiceSupport,
                HasBigButton = capabilitiesTK.HasBigButton
            };
        }

        private static GamePadState PlatformGetState(int index, GamePadDeadZone deadZoneMode)
        {
            PrepSettings();

            var stateTK = OpenTK.Input.GamePad.GetState (index);

            if (!stateTK.IsConnected)
                return GamePadState.Default;

            var sticks = 
                new GamePadThumbSticks (
                    new Vector2(stateTK.ThumbSticks.Left.X, stateTK.ThumbSticks.Left.Y),
                    new Vector2(stateTK.ThumbSticks.Right.X, stateTK.ThumbSticks.Right.Y),
                    deadZoneMode
                );

            var triggers =
                new GamePadTriggers (
                    stateTK.Triggers.Left,
                    stateTK.Triggers.Right
                );

            Buttons buttonStates = 
                (stateTK.Buttons.A == OpenTK.Input.ButtonState.Pressed ? Buttons.A : 0) |
                (stateTK.Buttons.B == OpenTK.Input.ButtonState.Pressed ? Buttons.B : 0) |
                (stateTK.Buttons.Back == OpenTK.Input.ButtonState.Pressed ? Buttons.Back : 0) |
                (stateTK.Buttons.BigButton == OpenTK.Input.ButtonState.Pressed ? Buttons.BigButton : 0) |
                (stateTK.Buttons.LeftShoulder == OpenTK.Input.ButtonState.Pressed ? Buttons.LeftShoulder : 0) |
                (stateTK.Buttons.LeftStick == OpenTK.Input.ButtonState.Pressed ? Buttons.LeftStick : 0) |
                (stateTK.Buttons.RightShoulder == OpenTK.Input.ButtonState.Pressed ? Buttons.RightShoulder : 0) |
                (stateTK.Buttons.RightStick == OpenTK.Input.ButtonState.Pressed ? Buttons.RightStick : 0) |
                (stateTK.Buttons.Start == OpenTK.Input.ButtonState.Pressed ? Buttons.Start : 0) |
                (stateTK.Buttons.X == OpenTK.Input.ButtonState.Pressed ? Buttons.X : 0) |
                (stateTK.Buttons.Y == OpenTK.Input.ButtonState.Pressed ? Buttons.Y : 0) |
                0;
            var buttons = new GamePadButtons(buttonStates);

            var dpad = 
                new GamePadDPad(
                    stateTK.DPad.IsUp ? ButtonState.Pressed : ButtonState.Released,
                    stateTK.DPad.IsDown ? ButtonState.Pressed : ButtonState.Released,
                    stateTK.DPad.IsLeft ? ButtonState.Pressed : ButtonState.Released,
                    stateTK.DPad.IsRight ? ButtonState.Pressed : ButtonState.Released
                );

            var result = new GamePadState(sticks, triggers, buttons, dpad);
            result.PacketNumber = stateTK.PacketNumber;
            return result;
        }

        private static bool PlatformSetVibration(int index, float leftMotor, float rightMotor)
        {
            PrepSettings ();

            return OpenTK.Input.GamePad.SetVibration (index, leftMotor, rightMotor);
        }
    }
}

#endif
