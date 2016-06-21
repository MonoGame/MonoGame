// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private static readonly Dictionary<int, GamePadInfo> Gamepads = new Dictionary<int, GamePadInfo>();

        // we have to maintain this mapping because instance IDs are not ordered by player index (i.e. player lights on Xbox gamepads), but DeviceID are
        private static readonly Dictionary<int, int> _deviceInstaceToId = new Dictionary<int, int>();

        private static Sdl.Haptic.Effect _hapticLeftRightEffect = new Sdl.Haptic.Effect
        {
            type = Sdl.Haptic.EffectId.LeftRight,
            leftright = new Sdl.Haptic.LeftRight
            {
                Type = Sdl.Haptic.EffectId.LeftRight,
                Length = Sdl.Haptic.Infinity,
                LargeMagnitude = ushort.MaxValue,
                SmallMagnitude = ushort.MaxValue
            }
        };

        static GamePad()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("gamecontrollerdb.txt"))
                if (stream != null)
                    using (var reader = new StreamReader(stream))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                            if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                                Sdl.GameController.AddMapping(line);
                    }
        }

        internal static void AddDevice(int deviceId)
        {
            var jdevice = Sdl.Joystick.Open(deviceId);
            var instanceid = Sdl.Joystick.InstanceID(jdevice);

            var gamepad = new GamePadInfo();
            gamepad.Device = Sdl.GameController.Open(deviceId);

            _deviceInstaceToId.Add(instanceid, deviceId);

            Gamepads.Add(deviceId, gamepad);

            if (Sdl.Haptic.IsHaptic(jdevice) == 1)
            {

                gamepad.HapticDevice = Sdl.Haptic.OpenFromJoystick(jdevice);

                if (gamepad.HapticDevice != IntPtr.Zero && Sdl.Haptic.EffectSupported(gamepad.HapticDevice, ref _hapticLeftRightEffect) == 1)
                {
                    try // for some reason, even if a GamePad "supports" the haptic effect, it may still fail on some gamepads (Logitech F710, F310 for instance)
                    {
                        Sdl.Haptic.NewEffect(gamepad.HapticDevice, ref _hapticLeftRightEffect);
                        gamepad.HapticType = 1;
                    }
                    catch (Exception)
                    {
                        Sdl.Haptic.Close(gamepad.HapticDevice);
                    }
                }
                else if (gamepad.HapticDevice != IntPtr.Zero && Sdl.Haptic.RumbleSupported(gamepad.HapticDevice) == 1)
                {
                    try // for some reason, even if a GamePad "supports" the haptic effect, it may still fail on some gamepads (Logitech F710, F310 for instance)
                    {
                        Sdl.Haptic.RumbleInit(gamepad.HapticDevice);
                        gamepad.HapticType = 2;
                    }
                    catch (Exception)
                    {
                        Sdl.Haptic.Close(gamepad.HapticDevice);
                    }
                }
                else
                {
                    Sdl.Haptic.Close(gamepad.HapticDevice);
                }
            }

            Sdl.ClearError(); // in case anything wrong happened with the haptic init
        }

        internal static void RemoveDevice(int instanceid)
        {
            int deviceId = _deviceInstaceToId[instanceid];
            DisposeDevice(Gamepads[deviceId]);
            Gamepads.Remove(deviceId);
            _deviceInstaceToId.Remove(instanceid);
        }

        private static void DisposeDevice(GamePadInfo info)
        {
            if (info.HapticType > 0)
                Sdl.Haptic.Close(info.HapticDevice);
            Sdl.GameController.Close(info.Device);
        }

        internal static void CloseDevices()
        {
            foreach (var entry in Gamepads)
                DisposeDevice(entry.Value);

            Gamepads.Clear();
        }

        private static int PlatformGetMaxNumberOfGamePads()
        {
            return 16;
        }

        private static GamePadCapabilities PlatformGetCapabilities(int index)
        {
            if (!Gamepads.ContainsKey(index))
                return new GamePadCapabilities();

            if (Sdl.GameController.GetName(Gamepads[index].Device) == "Unknown Gamepad")
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

        private static float GetFromSdlAxis(int axis)
        {
            // SDL Axis ranges from -32768 to 32767, so we need to divide with different numbers depending on if it's positive
            if (axis < 0)
                return axis / 32768f;

            return axis / 32767f;
        }

        private static GamePadState PlatformGetState(int index, GamePadDeadZone deadZoneMode)
        {
            if (!Gamepads.ContainsKey(index))
                return GamePadState.Default;

            var gdevice = Gamepads[index].Device;

            // Y gamepad axis is rotate between SDL and XNA
            var thumbSticks =
                new GamePadThumbSticks(
                    new Vector2(
                        GetFromSdlAxis(Sdl.GameController.GetAxis(gdevice, Sdl.GameController.Axis.LeftX)),
                        GetFromSdlAxis(Sdl.GameController.GetAxis(gdevice, Sdl.GameController.Axis.LeftY)) * -1f
                    ),
                    new Vector2(
                        GetFromSdlAxis(Sdl.GameController.GetAxis(gdevice, Sdl.GameController.Axis.RightX)),
                        GetFromSdlAxis(Sdl.GameController.GetAxis(gdevice, Sdl.GameController.Axis.RightY)) * -1f
                    ),
                    deadZoneMode
                );

            var triggers = new GamePadTriggers(
                GetFromSdlAxis(Sdl.GameController.GetAxis(gdevice, Sdl.GameController.Axis.TriggerLeft)),
                GetFromSdlAxis(Sdl.GameController.GetAxis(gdevice, Sdl.GameController.Axis.TriggerRight))
            );

            var buttons =
                new GamePadButtons(
                    ((Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.A) == 1) ? Buttons.A : 0) |
                    ((Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.B) == 1) ? Buttons.B : 0) |
                    ((Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.Back) == 1) ? Buttons.Back : 0) |
                    ((Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.Guide) == 1) ? Buttons.BigButton : 0) |
                    ((Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.LeftShoulder) == 1) ? Buttons.LeftShoulder : 0) |
                    ((Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.RightShoulder) == 1) ? Buttons.RightShoulder : 0) |
                    ((Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.LeftStick) == 1) ? Buttons.LeftStick : 0) |
                    ((Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.RightStick) == 1) ? Buttons.RightStick : 0) |
                    ((Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.Start) == 1) ? Buttons.Start : 0) |
                    ((Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.X) == 1) ? Buttons.X : 0) |
                    ((Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.Y) == 1) ? Buttons.Y : 0) |
                    0
                );

            var dPad =
                new GamePadDPad(
                    (Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.DpadUp) == 1) ? ButtonState.Pressed : ButtonState.Released,
                    (Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.DpadDown) == 1) ? ButtonState.Pressed : ButtonState.Released,
                    (Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.DpadLeft) == 1) ? ButtonState.Pressed : ButtonState.Released,
                    (Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.DpadRight) == 1) ? ButtonState.Pressed : ButtonState.Released
                );

            return new GamePadState(thumbSticks, triggers, buttons, dPad);
        }

        private static bool PlatformSetVibration(int index, float leftMotor, float rightMotor)
        {
            if (!Gamepads.ContainsKey(index))
                return false;

            var gamepad = Gamepads[index];

            if (gamepad.HapticType == 0)
                return false;

            if (leftMotor <= 0.0f && rightMotor <= 0.0f)
                Sdl.Haptic.StopAll(gamepad.HapticDevice);
            else if (gamepad.HapticType == 1)
            {
                _hapticLeftRightEffect.leftright.LargeMagnitude = (ushort)(65535f * leftMotor);
                _hapticLeftRightEffect.leftright.SmallMagnitude = (ushort)(65535f * rightMotor);

                Sdl.Haptic.UpdateEffect(gamepad.HapticDevice, 0, ref _hapticLeftRightEffect);
                Sdl.Haptic.RunEffect(gamepad.HapticDevice, 0, 1);
            }
            else if (gamepad.HapticType == 2)
                Sdl.Haptic.RumblePlay(gamepad.HapticDevice, Math.Max(leftMotor, rightMotor), Sdl.Haptic.Infinity);

            return true;
        }
    }
}