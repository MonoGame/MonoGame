// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input
{
    static partial class Joystick
    {
        internal static Dictionary<int, IntPtr> Joysticks = new Dictionary<int, IntPtr>();

        // we have to maintain this mapping because instance IDs are not ordered by player index (i.e. player lights on Xbox gamepads), but DeviceID are
        private static readonly Dictionary<int, int> _deviceInstaceToId = new Dictionary<int, int>();

        internal static void AddDevice(int deviceId)
        {
            var jdevice = Sdl.Joystick.Open(deviceId);
            var instanceid = Sdl.Joystick.InstanceID(jdevice);

            _deviceInstaceToId.Add(instanceid, deviceId);

            Joysticks.Add(deviceId, jdevice);
        }

        internal static void RemoveDevice(int instanceid)
        {
            int deviceId = _deviceInstaceToId[instanceid];
            Sdl.Joystick.Close(Joysticks[deviceId]);
            Joysticks.Remove(deviceId);
            _deviceInstaceToId.Remove(instanceid);
        }

        internal static void CloseDevices()
        {
            GamePad.CloseDevices();

            foreach (var entry in Joysticks)
                Sdl.Joystick.Close(entry.Value);

            Joysticks.Clear ();
        }

        private static JoystickCapabilities PlatformGetCapabilities(int index)
        {
            if (!Joysticks.ContainsKey(index))
                return new JoystickCapabilities
                {
                    IsConnected = false,
                    Id = "",
                    AxisCount = 0,
                    ButtonCount = 0,
                    HatCount = 0
                };

            var jdevice = Joysticks[index];
            return new JoystickCapabilities
            {
                IsConnected = true,
                Id = Sdl.Joystick.GetGUID(jdevice).ToString(),
                AxisCount = Sdl.Joystick.NumAxes(jdevice),
                ButtonCount = Sdl.Joystick.NumButtons(jdevice),
                HatCount = Sdl.Joystick.NumHats(jdevice)
            };
        }

        private static JoystickState PlatformGetState(int index)
        {
            if (!Joysticks.ContainsKey(index))
                return new JoystickState
                {
                    IsConnected = false,
                    Axes = new float[0],
                    Buttons = new ButtonState[0],
                    Hats = new JoystickHat[0]
                };

            var jcap = PlatformGetCapabilities(index);
            var jdevice = Joysticks[index];

            var axes = new float[jcap.AxisCount];
            for (var i = 0; i < axes.Length; i++)
                axes[i] = Sdl.Joystick.GetAxis(jdevice, i);

            var buttons = new ButtonState[jcap.ButtonCount];
            for (var i = 0; i < buttons.Length; i++)
                buttons[i] = (Sdl.Joystick.GetButton(jdevice, i) == 0) ? ButtonState.Released : ButtonState.Pressed;

            var hats = new JoystickHat[jcap.HatCount];
            for (var i = 0; i < hats.Length; i++)
            {
                var hatstate = Sdl.Joystick.GetHat(jdevice, i);

                hats[i] = new JoystickHat
                {
                    Up = hatstate.HasFlag(Sdl.Joystick.Hat.Up) ? ButtonState.Pressed : ButtonState.Released,
                    Down = hatstate.HasFlag(Sdl.Joystick.Hat.Down) ? ButtonState.Pressed : ButtonState.Released,
                    Left = hatstate.HasFlag(Sdl.Joystick.Hat.Left) ? ButtonState.Pressed : ButtonState.Released,
                    Right = hatstate.HasFlag(Sdl.Joystick.Hat.Right) ? ButtonState.Pressed : ButtonState.Released
                };
            }

            return new JoystickState
            {
                IsConnected = true,
                Axes = axes,
                Buttons = buttons,
                Hats = hats
            };
        }
    }
}