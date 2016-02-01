// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input
{
    static partial class Joystick
    {
        internal static Dictionary<int, IntPtr> joysticks = new Dictionary<int, IntPtr>();

        internal static void AddDevice(int device_id)
        {
            var jdevice = SDL.Joystick.Open(device_id);
            joysticks.Add(device_id, jdevice);
            GamePad.AddDevice(device_id, jdevice);
        }

        internal static void RemoveDevice(int device_id)
        {
            SDL.Joystick.Close(joysticks[device_id]);
            joysticks.Remove(device_id);
            GamePad.RemoveDevice(device_id);
        }

        internal static void CloseDevices()
        {
            GamePad.CloseDevices();

            foreach (KeyValuePair<int, IntPtr> entry in joysticks)
                SDL.Joystick.Close(entry.Value);
        }

        private static JoystickCapabilities PlatformGetCapabilities(int index)
        {
            if (!joysticks.ContainsKey(index))
                return new JoystickCapabilities
                {
                    IsConnected = false,
                    Id = "",
                    AxisCount = 0,
                    ButtonCount = 0,
                    HatCount = 0
                };

            var jdevice = joysticks[index];
            return new JoystickCapabilities
            {
                IsConnected = true,
                Id = SDL.Joystick.GetGUID(jdevice).ToString(),
                AxisCount = SDL.Joystick.NumAxes(jdevice),
                ButtonCount = SDL.Joystick.NumButtons(jdevice),
                HatCount = SDL.Joystick.NumHats(jdevice)
            };
        }

        private static JoystickState PlatformGetState(int index)
        {
            if (!joysticks.ContainsKey(index))
                return new JoystickState
                {
                    IsConnected = false,
                    Axes = new float[0],
                    Buttons = new ButtonState[0],
                    Hats = new JoystickHat[0]
                };

            var jcap = PlatformGetCapabilities(index);
            var jdevice = joysticks[index];

            var axes = new float[jcap.AxisCount];
            for (int i = 0; i < axes.Length; i++)
                axes[i] = SDL.Joystick.GetAxis(jdevice, i);
            
            var buttons = new ButtonState[jcap.ButtonCount];
            for (int i = 0; i < buttons.Length; i++)
                buttons[i] = (SDL.Joystick.GetButton(jdevice, i) == 0) ? ButtonState.Released : ButtonState.Pressed;

            var hats = new JoystickHat[jcap.HatCount];
            for (int i = 0; i < hats.Length; i++)
            {
                var hatstate = SDL.Joystick.GetHat(jdevice, i);

                hats[i] = new JoystickHat
                {
                    Up = hatstate.HasFlag(SDL.Joystick.Hat.Up) ? ButtonState.Pressed : ButtonState.Released,
                    Down = hatstate.HasFlag(SDL.Joystick.Hat.Down) ? ButtonState.Pressed : ButtonState.Released,
                    Left = hatstate.HasFlag(SDL.Joystick.Hat.Left) ? ButtonState.Pressed : ButtonState.Released,
                    Right = hatstate.HasFlag(SDL.Joystick.Hat.Right) ? ButtonState.Pressed : ButtonState.Released
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
