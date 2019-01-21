// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using JSIL;

namespace Microsoft.Xna.Framework.Input
{
    static partial class GamePad
    {
        private static readonly Dictionary<string, string> Configurations = new Dictionary<string, string>();
        private static Dictionary<string, GamepadTranslator> GamePadCache = new Dictionary<string, GamepadTranslator>();
        private static int platformlimit;

        static GamePad()
        {
            Configurations.Add("045e-028e-Microsoft X-Box 360 pad", "X360 Wireless Controller,a:b0,b:b1,x:b2,y:b3,dpup:a7,dpdown:a7,dpleft:a6,dpright:a6,leftshoulder:b4,leftstick:b9,lefttrigger:a2,rightshoulder:b5,rightstick:b10,righttrigger:a5,back:b6,start:b7,guide:b8,leftx:a0,lefty:a1,rightx:a3,righty:a4");
            Configurations.Add("Microsoft X-Box 360 pad (STANDARD GAMEPAD Vendor: 045e Product: 028e)", "X360 Wireless Controller,a:b0,b:b1,x:b2,y:b3,dpup:b12,dpdown:b13,dpleft:b14,dpright:b15,leftshoulder:b4,leftstick:b10,lefttrigger:b6,rightshoulder:b5,rightstick:b11,righttrigger:b7,back:b8,start:b9,guide:b16,leftx:a0,lefty:a1,rightx:a2,righty:a3");

            var navigator = Builtins.Global["navigator"];
            platformlimit = navigator.getGamepads ? 16 : 4; // Chrome has a limit of 4 gamepads
        }

        private static int PlatformGetMaxNumberOfGamePads()
        {
            return platformlimit;
        }

        private static GamePadCapabilities PlatformGetCapabilities(int index)
        {
            var jcap = Joystick.GetCapabilities(index);

            if (!GamePadCache.ContainsKey(jcap.Identifier))
                GamePadCache.Add(jcap.Identifier, Configurations.ContainsKey(jcap.Identifier) ? new GamepadTranslator(Configurations[jcap.Identifier]) : new GamepadTranslator(""));

            var gpc = GamePadCache[jcap.Identifier];

            return new GamePadCapabilities 
            {
                IsConnected = true,
                GamePadType = GamePadType.GamePad,
                HasAButton = (gpc.Read("a").index != -1),
                HasBButton = (gpc.Read("b").index != -1),
                HasXButton = (gpc.Read("x").index != -1),
                HasYButton = (gpc.Read("y").index != -1),
                HasBackButton = (gpc.Read("back").index != -1),
                HasStartButton = (gpc.Read("start").index != -1),
                HasDPadDownButton = (gpc.Read("dpdown").index != -1),
                HasDPadLeftButton = (gpc.Read("dpleft").index != -1),
                HasDPadRightButton = (gpc.Read("dpright").index != -1),
                HasDPadUpButton = (gpc.Read("dpup").index != -1),
                HasLeftShoulderButton = (gpc.Read("leftshoulder").index != -1),
                HasRightShoulderButton = (gpc.Read("rightshoulder").index != -1),
                HasLeftStickButton = (gpc.Read("leftstick").index != -1),
                HasRightStickButton = (gpc.Read("rightstick").index != -1),
                HasLeftTrigger = (gpc.Read("lefttrigger").index != -1),
                HasRightTrigger = (gpc.Read("righttrigger").index != -1),
                HasLeftXThumbStick = (gpc.Read("leftx").index != -1),
                HasLeftYThumbStick = (gpc.Read("lefty").index != -1),
                HasRightXThumbStick = (gpc.Read("rightx").index != -1),
                HasRightYThumbStick = (gpc.Read("righty").index != -1),
                HasLeftVibrationMotor = false,
                HasRightVibrationMotor = false,
                HasVoiceSupport = false,
                HasBigButton = (gpc.Read("guide").index != -1)
            };
        }

        private static GamePadState PlatformGetState(int index, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
        {
            var state = GamePadState.Default;
            var jcap = Joystick.GetCapabilities(index);

            if (jcap.IsConnected)
            {
                state.IsConnected = true;

                var jstate = Joystick.GetState(index);

                if (!GamePadCache.ContainsKey(jcap.Identifier))
                    GamePadCache.Add(jcap.Identifier, Configurations.ContainsKey(jcap.Identifier) ? new GamepadTranslator(Configurations[jcap.Identifier]) : new GamepadTranslator(""));

                var gpc = GamePadCache[jcap.Identifier];

                Buttons buttons = 
                    (gpc.ButtonPressed("a", jstate) ? Buttons.A : 0) |
                    (gpc.ButtonPressed("b", jstate) ? Buttons.B : 0) |
                    (gpc.ButtonPressed("back", jstate) ? Buttons.Back : 0) |
                    (gpc.ButtonPressed("guide", jstate) ? Buttons.BigButton : 0) |
                    (gpc.ButtonPressed("leftshoulder", jstate) ? Buttons.LeftShoulder : 0) |
                    (gpc.ButtonPressed("leftstick", jstate) ? Buttons.LeftStick : 0) |
                    (gpc.ButtonPressed("rightshoulder", jstate) ? Buttons.RightShoulder : 0) |
                    (gpc.ButtonPressed("rightstick", jstate) ? Buttons.RightStick : 0) |
                    (gpc.ButtonPressed("start", jstate) ? Buttons.Start : 0) |
                    (gpc.ButtonPressed("x", jstate) ? Buttons.X : 0) |
                    (gpc.ButtonPressed("y", jstate) ? Buttons.Y : 0) |
                    0;

                var sticks = 
                    new GamePadThumbSticks(
                        new Vector2(gpc.AxisPressed("leftx", jstate), gpc.AxisPressed("lefty", jstate)),
                        new Vector2(gpc.AxisPressed("rightx", jstate), gpc.AxisPressed("righty", jstate)),
                        leftDeadZoneMode,
						rightDeadZoneMode
                    );
                
                var dpad = 
                    new GamePadDPad(
                        gpc.DpadPressed("dpup", jstate) ? ButtonState.Pressed : ButtonState.Released,
                        gpc.DpadPressed("dpdown", jstate) ? ButtonState.Pressed : ButtonState.Released,
                        gpc.DpadPressed("dpleft", jstate) ? ButtonState.Pressed : ButtonState.Released,
                        gpc.DpadPressed("dpright", jstate) ? ButtonState.Pressed : ButtonState.Released
                    );

                var triggers = 
                    new GamePadTriggers(
                        gpc.TriggerPressed("lefttrigger", jstate),
                        gpc.TriggerPressed("righttrigger", jstate)
                    );

                
                state = new GamePadState(sticks, triggers, new GamePadButtons(buttons), dpad);
            }

            return state;
        }

        private static bool PlatformSetVibration(int index, float leftMotor, float rightMotor)
        {
            return false;
        }
    }
}
