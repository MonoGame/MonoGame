#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License
using System;
using OpenTK;

namespace Microsoft.Xna.Framework.Input
{
    static partial class GamePad
    {
        static bool prepDone = false;

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
            #endif

            prepDone = true;
        }

        private static GamePadCapabilities PlatformGetCapabilities(int index)
        {
            PrepSettings ();

            var capabilitiesTK = OpenTK.Input.GamePad.GetCapabilities (index);
            if (capabilitiesTK.GamePadType == OpenTK.Input.GamePadType.Unknown) 
            {
                return new GamePadCapabilities ();
            }

            return new GamePadCapabilities()
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
            {
                return GamePadState.Default;
            }

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