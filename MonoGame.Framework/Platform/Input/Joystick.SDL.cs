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

        internal static void AddDevice(int deviceId)
        {
            var jdevice = Sdl.Joystick.Open(deviceId);
            var id = 0;

            while (Joysticks.ContainsKey(id))
                id++;

            Joysticks.Add(id, jdevice);

            if (Sdl.GameController.IsGameController(deviceId) == 1)
                GamePad.AddDevice(deviceId);
        }

        internal static void RemoveDevice(int instanceid)
        {
            foreach (KeyValuePair<int, IntPtr> entry in Joysticks)
            {
                if (Sdl.Joystick.InstanceID(entry.Value) == instanceid)
                {
                    Sdl.Joystick.Close(Joysticks[entry.Key]);
                    Joysticks.Remove(entry.Key);
                    break;
                }
            }
        }

        internal static void CloseDevices()
        {
            GamePad.CloseDevices();

            foreach (var entry in Joysticks)
                Sdl.Joystick.Close(entry.Value);

            Joysticks.Clear ();
        }

        private const bool PlatformIsSupported = true;

        private static JoystickCapabilities PlatformGetCapabilities(int index)
        {
            if (!Joysticks.ContainsKey(index))
                return new JoystickCapabilities
                {
                    IsConnected = false,
                    Identifier = "",
                    IsGamepad = false,
                    AxisCount = 0,
                    ButtonCount = 0,
                    HatCount = 0
                };

            var jdevice = Joysticks[index];
            return new JoystickCapabilities
            {
                IsConnected = true,
                Identifier = Sdl.Joystick.GetGUID(jdevice).ToString(),
                IsGamepad = (Sdl.GameController.IsGameController(index) == 1),
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
                    Axes = new int[0],
                    Buttons = new ButtonState[0],
                    Hats = new JoystickHat[0]
                };

            var jcap = PlatformGetCapabilities(index);
            var jdevice = Joysticks[index];

            var axes = new int[jcap.AxisCount];
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
