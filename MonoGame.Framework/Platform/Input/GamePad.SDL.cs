// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Input
{
    static partial class GamePad
    {
        private class GamePadInfo
        {
            public IntPtr Device;
            public int PacketNumber;
        }

        private static readonly Dictionary<int, GamePadInfo> Gamepads = new Dictionary<int, GamePadInfo>();
        private static readonly Dictionary<int, int> _translationTable = new Dictionary<int, int>();

        /// <summary>
        /// Initialies the internal database of gamepad mappings for an SDL context
        /// </summary>
        public static void InitDatabase()
        {
            using (var stream = ReflectionHelpers.GetAssembly(typeof(GamePad)).GetManifestResourceStream("gamecontrollerdb.txt"))
            {
                if (stream != null)
                {
                    using (var reader = new BinaryReader(stream))
                    {
                        try
                        {
                            var src = Sdl.RwFromMem(reader.ReadBytes((int)stream.Length), (int)stream.Length);
                            Sdl.GameController.AddMappingFromRw(src, 1);
                        }
                        catch { }
                    }
                }
            }
        }

        internal static void AddDevice(int deviceId)
        {
            var gamepad = new GamePadInfo();
            gamepad.Device = Sdl.GameController.Open(deviceId);

            var id = 0;
            while (Gamepads.ContainsKey(id))
                id++;

            Gamepads.Add(id, gamepad);

            RefreshTranslationTable();
        }

        internal static void RemoveDevice(int instanceid)
        {
            foreach (KeyValuePair<int, GamePadInfo> entry in Gamepads)
            {
                if (Sdl.Joystick.InstanceID(Sdl.GameController.GetJoystick(entry.Value.Device)) == instanceid)
                {
                    Gamepads.Remove(entry.Key);
                    DisposeDevice(entry.Value);
                    break;
                }
            }

            RefreshTranslationTable();
        }

        internal static void RefreshTranslationTable()
        {
            _translationTable.Clear();
            foreach (var pair in Gamepads)
            {
                _translationTable[Sdl.Joystick.InstanceID(Sdl.GameController.GetJoystick(pair.Value.Device))] = pair.Key;
            }
        }

        internal static void UpdatePacketInfo(int instanceid, uint packetNumber)
        {
            int index;
            if (_translationTable.TryGetValue(instanceid, out index))
            {
                GamePadInfo info = null;
                if (Gamepads.TryGetValue(index, out info))
                {
                    info.PacketNumber = packetNumber < int.MaxValue ? (int)packetNumber : (int)(packetNumber - (uint)int.MaxValue);
                }
            }
        }

        private static void DisposeDevice(GamePadInfo info)
        {
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

            var gamecontroller = Gamepads[index].Device;
            var caps = new GamePadCapabilities();
            var mapping = Sdl.GameController.GetMapping(gamecontroller).Split(',');

            caps.IsConnected = true;
            caps.DisplayName = Sdl.GameController.GetName(gamecontroller);
            caps.Identifier = Sdl.Joystick.GetGUID(Sdl.GameController.GetJoystick(gamecontroller)).ToString();
            caps.HasLeftVibrationMotor = caps.HasRightVibrationMotor = Sdl.GameController.HasRumble(gamecontroller) != 0;
            caps.GamePadType = GamePadType.GamePad;

            foreach (var map in mapping)
            {
                var split = map.Split(':');
                if (split.Length != 2)
                    continue;

                switch (split[0])
                {
                    case "a":
                        caps.HasAButton = true;
                        break;
                    case "b":
                        caps.HasBButton = true;
                        break;
                    case "x":
                        caps.HasXButton = true;
                        break;
                    case "y":
                        caps.HasYButton = true;
                        break;
                    case "back":
                        caps.HasBackButton = true;
                        break;
                    case "guide":
                        caps.HasBigButton = true;
                        break;
                    case "start":
                        caps.HasStartButton = true;
                        break;
                    case "dpleft":
                        caps.HasDPadLeftButton = true;
                        break;
                    case "dpdown":
                        caps.HasDPadDownButton = true;
                        break;
                    case "dpright":
                        caps.HasDPadRightButton = true;
                        break;
                    case "dpup":
                        caps.HasDPadUpButton = true;
                        break;
                    case "leftshoulder":
                        caps.HasLeftShoulderButton = true;
                        break;
                    case "lefttrigger":
                        caps.HasLeftTrigger = true;
                        break;
                    case "rightshoulder":
                        caps.HasRightShoulderButton = true;
                        break;
                    case "righttrigger":
                        caps.HasRightTrigger = true;
                        break;
                    case "leftstick":
                        caps.HasLeftStickButton = true;
                        break;
                    case "rightstick":
                        caps.HasRightStickButton = true;
                        break;
                    case "leftx":
                        caps.HasLeftXThumbStick = true;
                        break;
                    case "lefty":
                        caps.HasLeftYThumbStick = true;
                        break;
                    case "rightx":
                        caps.HasRightXThumbStick = true;
                        break;
                    case "righty":
                        caps.HasRightYThumbStick = true;
                        break;
                }
            }

            return caps;
        }

        private static float GetFromSdlAxis(int axis)
        {
            // SDL Axis ranges from -32768 to 32767, so we need to divide with different numbers depending on if it's positive
            if (axis < 0)
                return axis / 32768f;

            return axis / 32767f;
        }

        private static GamePadState PlatformGetState(int index, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
        {
            if (!Gamepads.ContainsKey(index))
                return GamePadState.Default;

            var gamepadInfo = Gamepads[index];
            var gdevice = gamepadInfo.Device;

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
                    leftDeadZoneMode,
                    rightDeadZoneMode
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
                    ((triggers.Left > 0f) ? Buttons.LeftTrigger : 0) |
                    ((triggers.Right > 0f) ? Buttons.RightTrigger : 0)
                );

            var dPad =
                new GamePadDPad(
                    (Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.DpadUp) == 1) ? ButtonState.Pressed : ButtonState.Released,
                    (Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.DpadDown) == 1) ? ButtonState.Pressed : ButtonState.Released,
                    (Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.DpadLeft) == 1) ? ButtonState.Pressed : ButtonState.Released,
                    (Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.DpadRight) == 1) ? ButtonState.Pressed : ButtonState.Released
                );

            var ret = new GamePadState(thumbSticks, triggers, buttons, dPad);
            ret.PacketNumber = gamepadInfo.PacketNumber;
            return ret;
        }

        private static bool PlatformSetVibration(int index, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger)
        {
            if (!Gamepads.ContainsKey(index))
                return false;

            var gamepad = Gamepads[index];

            return Sdl.GameController.Rumble(gamepad.Device, (ushort)(65535f * leftMotor),
                       (ushort)(65535f * rightMotor), uint.MaxValue) == 0 &&
                   Sdl.GameController.RumbleTriggers(gamepad.Device, (ushort)(65535f * leftTrigger),
                       (ushort)(65535f * rightTrigger), uint.MaxValue) == 0;
        }
    }
}
