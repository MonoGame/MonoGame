// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;

using System.Reflection;

namespace Microsoft.Xna.Framework.Input
{
    static partial class GamePad
    {
        private class GamePadInfo
        {
            public IntPtr Device;
            public IntPtr HapticDevice;
            public int HapticType;
        }

        private static Dictionary<int, GamePadInfo> gamepads = new Dictionary<int, GamePadInfo>();

        private static SDL.Haptic.Effect HapticLeftRightEffect = new SDL.Haptic.Effect
        {
            type = SDL.Haptic.EffectID.LeftRight,
            leftright = new SDL.Haptic.LeftRight
            {
                Type = SDL.Haptic.EffectID.LeftRight,
                Length = SDL.Haptic.Infinity,
                LargeMagnitude = ushort.MaxValue,
                SmallMagnitude = ushort.MaxValue
            }
        };

        static GamePad()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("gamecontrollerdb.txt"))
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                    if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                        SDL.GameController.AddMapping(line);
            }
        }

        internal static void AddDevice(int device_id, IntPtr jdevice)
        {
            if (SDL.GameController.IsGameController(device_id) == 0)
            {
                var guide = "";
                foreach (var b in SDL.Joystick.GetGUID(jdevice).ToByteArray())
                    guide += ((int)b).ToString("X2");
                
                SDL.GameController.AddMapping(guide + ",Unknown Gamepad,a:b0,b:b1,back:b8,dpdown:h0.4,dpleft:h0.8,dpright:h0.2,dpup:h0.1,guide:,leftshoulder:b4,leftstick:b10,lefttrigger:b6,leftx:a0,lefty:a1,rightshoulder:b5,rightstick:b11,righttrigger:b7,rightx:a2,righty:a3,start:b9,x:b2,y:b3,");
            }

            var gamepad = new GamePadInfo();
            gamepad.Device = SDL.GameController.Open(device_id);

            gamepads.Add(device_id, gamepad);

            if (SDL.Haptic.IsHaptic(jdevice) != 0)
            {
                gamepad.HapticDevice = SDL.Haptic.OpenFromJoystick(jdevice);

                if (SDL.Haptic.EffectSupported(gamepad.HapticDevice, ref HapticLeftRightEffect) == 1)
                {
                    SDL.Haptic.NewEffect(gamepad.HapticDevice, ref HapticLeftRightEffect);
                    gamepad.HapticType = 1;
                }
                else if (SDL.Haptic.RumbleSupported(gamepad.HapticDevice) == 1)
                {
                    SDL.Haptic.RumbleInit(gamepad.HapticDevice);
                    gamepad.HapticType = 2;
                }
                else
                    SDL.Haptic.Close(gamepad.HapticDevice);
            }
        }

        internal static void RemoveDevice(int device_id)
        {
            DisposeDevice(gamepads[device_id]);
            gamepads.Remove(device_id);
        }

        private static void DisposeDevice(GamePadInfo info)
        {
            SDL.Haptic.Close(info.HapticDevice);
            SDL.GameController.Close(info.Device);
        }

        internal static void CloseDevices()
        {
            foreach (KeyValuePair<int, GamePadInfo> entry in gamepads)
                DisposeDevice(entry.Value);
        }

        private static int PlatformGetMaxNumberOfGamePads()
        {
            return 16;
        }

        private static GamePadCapabilities PlatformGetCapabilities(int index)
        {
            if (!gamepads.ContainsKey(index))
                return new GamePadCapabilities();

            if (SDL.GameController.GetName(gamepads[index].Device) == "Unknown Gamepad")
                return new GamePadCapabilities
                {
                    IsConnected = true
                };

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
                HasVoiceSupport = true,
                HasBigButton = true
            };
        }

        private static float GetFromSDLAxis(int axis)
        {
            // SDL Axis ranges from -32768 to 32767, so we need to divide with different numbers depending on if it's positive
            if (axis < 0)
                return axis / 32768f;

            return axis / 32767f;
        }   

        private static GamePadState PlatformGetState(int index, GamePadDeadZone deadZoneMode)
        {
            if (!gamepads.ContainsKey(index))
                return GamePadState.Default;

            var gdevice = gamepads[index].Device;

            var thumbSticks = 
                new GamePadThumbSticks(
                    new Vector2(
                        GetFromSDLAxis(SDL.GameController.GetAxis(gdevice, SDL.GameController.Axis.LeftX)),
                        GetFromSDLAxis(SDL.GameController.GetAxis(gdevice, SDL.GameController.Axis.LeftY))
                    ),
                    new Vector2(
                        GetFromSDLAxis(SDL.GameController.GetAxis(gdevice, SDL.GameController.Axis.RightX)),
                        GetFromSDLAxis(SDL.GameController.GetAxis(gdevice, SDL.GameController.Axis.RightY))
                    ),
                    deadZoneMode
                );

            var triggers = new GamePadTriggers(
                GetFromSDLAxis(SDL.GameController.GetAxis(gdevice, SDL.GameController.Axis.TriggerLeft)),
                GetFromSDLAxis(SDL.GameController.GetAxis(gdevice, SDL.GameController.Axis.TriggerRight))
            );
            
            var buttons = 
                new GamePadButtons(
                    ((SDL.GameController.GetButton(gdevice, SDL.GameController.Button.A) == 1) ? Buttons.A : 0) |
                    ((SDL.GameController.GetButton(gdevice, SDL.GameController.Button.B) == 1) ? Buttons.B : 0) |
                    ((SDL.GameController.GetButton(gdevice, SDL.GameController.Button.Back) == 1) ? Buttons.Back : 0) |
                    ((SDL.GameController.GetButton(gdevice, SDL.GameController.Button.Guide) == 1) ? Buttons.BigButton : 0) |
                    ((SDL.GameController.GetButton(gdevice, SDL.GameController.Button.LeftShoulder) == 1) ? Buttons.LeftShoulder : 0) |
                    ((SDL.GameController.GetButton(gdevice, SDL.GameController.Button.RightShoulder) == 1) ? Buttons.RightShoulder : 0) |
                    ((SDL.GameController.GetButton(gdevice, SDL.GameController.Button.LeftStick) == 1) ? Buttons.LeftStick : 0) |
                    ((SDL.GameController.GetButton(gdevice, SDL.GameController.Button.RightStick) == 1) ? Buttons.RightStick : 0) |
                    ((SDL.GameController.GetButton(gdevice, SDL.GameController.Button.Start) == 1) ? Buttons.Start : 0) |
                    ((SDL.GameController.GetButton(gdevice, SDL.GameController.Button.X) == 1) ? Buttons.X : 0) |
                    ((SDL.GameController.GetButton(gdevice, SDL.GameController.Button.Y) == 1) ? Buttons.Y : 0) |
                    0
                );

            var dPad = 
                new GamePadDPad(
                    (SDL.GameController.GetButton(gdevice, SDL.GameController.Button.DpadUp) == 1) ? ButtonState.Pressed : ButtonState.Released,
                    (SDL.GameController.GetButton(gdevice, SDL.GameController.Button.DpadDown) == 1) ? ButtonState.Pressed : ButtonState.Released,
                    (SDL.GameController.GetButton(gdevice, SDL.GameController.Button.DpadLeft) == 1) ? ButtonState.Pressed : ButtonState.Released,
                    (SDL.GameController.GetButton(gdevice, SDL.GameController.Button.DpadRight) == 1) ? ButtonState.Pressed : ButtonState.Released
                );

            return new GamePadState(thumbSticks, triggers, buttons, dPad);
        }

        private static bool PlatformSetVibration(int index, float leftMotor, float rightMotor)
        {
            if (!gamepads.ContainsKey(index))
                return false;

            var gamepad = gamepads[index];

            if (gamepad.HapticType == 0)
                return false;

            if (leftMotor <= 0.0f && rightMotor <= 0.0f)
                SDL.Haptic.StopAll(gamepad.HapticDevice);
            else if (gamepad.HapticType == 1)
            {
                HapticLeftRightEffect.leftright.LargeMagnitude = (ushort)(65535f * leftMotor);
                HapticLeftRightEffect.leftright.SmallMagnitude = (ushort)(65535f * rightMotor);

                SDL.Haptic.UpdateEffect(gamepad.HapticDevice, 0, ref HapticLeftRightEffect);
                SDL.Haptic.RunEffect(gamepad.HapticDevice, 0, 1);
            }
            else if(gamepad.HapticType == 2)
                SDL.Haptic.RumblePlay(gamepad.HapticDevice, Math.Max(leftMotor, rightMotor), SDL.Haptic.Infinity);

            return true;
        }
    }
}
