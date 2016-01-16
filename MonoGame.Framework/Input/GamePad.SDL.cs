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

        private static SDL.SDL_HapticEffect HapticLeftRightEffect = new SDL.SDL_HapticEffect
        {
            type = SDL.SDL_HAPTIC_LEFTRIGHT,
            leftright = new SDL.SDL_HapticLeftRight
            {
                type = SDL.SDL_HAPTIC_LEFTRIGHT,
                length = SDL.SDL_HAPTIC_INFINITY,
                large_magnitude = ushort.MaxValue,
                small_magnitude = ushort.MaxValue
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
                        SDL.SDL_GameControllerAddMapping(line);
            }
        }

        internal static void AddDevice(int device_id, IntPtr jdevice)
        {
            if (SDL.SDL_IsGameController(device_id) == 0)
            {
                var guide = "";
                foreach (var b in SDL.SDL_JoystickGetGUID(jdevice).ToByteArray())
                    guide += ((int)b).ToString("X2");
                
                SDL.SDL_GameControllerAddMapping(guide + ",Unknown Gamepad,a:b0,b:b1,back:b8,dpdown:h0.4,dpleft:h0.8,dpright:h0.2,dpup:h0.1,guide:,leftshoulder:b4,leftstick:b10,lefttrigger:b6,leftx:a0,lefty:a1,rightshoulder:b5,rightstick:b11,righttrigger:b7,rightx:a2,righty:a3,start:b9,x:b2,y:b3,");
            }

            var gamepad = new GamePadInfo();
            gamepad.Device = SDL.SDL_GameControllerOpen(device_id);

            gamepads.Add(device_id, gamepad);

            if (SDL.SDL_JoystickIsHaptic(jdevice) != 0)
            {
                gamepad.HapticDevice = SDL.SDL_HapticOpenFromJoystick(jdevice);

                if (SDL.SDL_HapticEffectSupported(gamepad.HapticDevice, ref HapticLeftRightEffect) == 1)
                {
                    SDL.SDL_HapticNewEffect(gamepad.HapticDevice, ref HapticLeftRightEffect);
                    gamepad.HapticType = 1;
                }
                else if (SDL.SDL_HapticRumbleSupported(gamepad.HapticDevice) == 1)
                {
                    SDL.SDL_HapticRumbleInit(gamepad.HapticDevice);
                    gamepad.HapticType = 2;
                }
                else
                    SDL.SDL_HapticClose(gamepad.HapticDevice);
            }
        }

        internal static void RemoveDevice(int device_id)
        {
            DisposeDevice(gamepads[device_id]);
            gamepads.Remove(device_id);
        }

        private static void DisposeDevice(GamePadInfo info)
        {
            SDL.SDL_HapticClose(info.HapticDevice);
            SDL.SDL_GameControllerClose(info.Device);
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

            if (SDL.SDL_GameControllerName(gamepads[index].Device) == "Unknown Gamepad")
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
                        GetFromSDLAxis(SDL.SDL_GameControllerGetAxis(gdevice, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX)),
                        GetFromSDLAxis(SDL.SDL_GameControllerGetAxis(gdevice, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTY))
                    ),
                    new Vector2(
                        GetFromSDLAxis(SDL.SDL_GameControllerGetAxis(gdevice, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTX)),
                        GetFromSDLAxis(SDL.SDL_GameControllerGetAxis(gdevice, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTY))
                    ),
                    deadZoneMode
                );

            var triggers = new GamePadTriggers(
                GetFromSDLAxis(SDL.SDL_GameControllerGetAxis(gdevice, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERLEFT)),
                GetFromSDLAxis(SDL.SDL_GameControllerGetAxis(gdevice, SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT))
            );
            
            var buttons = 
                new GamePadButtons(
                    ((SDL.SDL_GameControllerGetButton(gdevice, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A) == 1) ? Buttons.A : 0) |
                    ((SDL.SDL_GameControllerGetButton(gdevice, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_B) == 1) ? Buttons.B : 0) |
                    ((SDL.SDL_GameControllerGetButton(gdevice, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_BACK) == 1) ? Buttons.Back : 0) |
                    ((SDL.SDL_GameControllerGetButton(gdevice, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_GUIDE) == 1) ? Buttons.BigButton : 0) |
                    ((SDL.SDL_GameControllerGetButton(gdevice, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSHOULDER) == 1) ? Buttons.LeftShoulder : 0) |
                    ((SDL.SDL_GameControllerGetButton(gdevice, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSHOULDER) == 1) ? Buttons.RightShoulder : 0) |
                    ((SDL.SDL_GameControllerGetButton(gdevice, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSTICK) == 1) ? Buttons.LeftStick : 0) |
                    ((SDL.SDL_GameControllerGetButton(gdevice, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSTICK) == 1) ? Buttons.RightStick : 0) |
                    ((SDL.SDL_GameControllerGetButton(gdevice, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_START) == 1) ? Buttons.Start : 0) |
                    ((SDL.SDL_GameControllerGetButton(gdevice, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_X) == 1) ? Buttons.X : 0) |
                    ((SDL.SDL_GameControllerGetButton(gdevice, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_Y) == 1) ? Buttons.Y : 0) |
                    0
                );

            var dPad = 
                new GamePadDPad(
                    (SDL.SDL_GameControllerGetButton(gdevice, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_UP) == 1) ? ButtonState.Pressed : ButtonState.Released,
                    (SDL.SDL_GameControllerGetButton(gdevice, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_DOWN) == 1) ? ButtonState.Pressed : ButtonState.Released,
                    (SDL.SDL_GameControllerGetButton(gdevice, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT) == 1) ? ButtonState.Pressed : ButtonState.Released,
                    (SDL.SDL_GameControllerGetButton(gdevice, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT) == 1) ? ButtonState.Pressed : ButtonState.Released
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
                SDL.SDL_HapticStopAll(gamepad.HapticDevice);
            else if (gamepad.HapticType == 1)
            {
                HapticLeftRightEffect.leftright.large_magnitude = (ushort)(65535f * leftMotor);
                HapticLeftRightEffect.leftright.small_magnitude = (ushort)(65535f * rightMotor);

                SDL.SDL_HapticUpdateEffect(gamepad.HapticDevice, 0, ref HapticLeftRightEffect);
                SDL.SDL_HapticRunEffect(gamepad.HapticDevice, 0, 1);
            }
            else if(gamepad.HapticType == 2)
                SDL.SDL_HapticRumblePlay(gamepad.HapticDevice, Math.Max(leftMotor, rightMotor), SDL.SDL_HAPTIC_INFINITY);

            return true;
        }
    }
}
