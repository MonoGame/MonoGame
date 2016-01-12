// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

using MonoGame.Utilities;

namespace Microsoft.Xna.Framework.Input
{
    static partial class Joystick
    {
        private static Dictionary<int, IntPtr> joysticks = new Dictionary<int, IntPtr>();

        internal static void AddDevice(int device_id)
        {
            joysticks.Add(device_id, SDL.SDL_JoystickOpen(device_id));
        }

        internal static void RemoveDevice(int device_id)
        {
            joysticks.Remove(device_id);
        }

        internal static void CloseDevices()
        {
            foreach (KeyValuePair<int, IntPtr> entry in joysticks)
                SDL.SDL_JoystickClose(entry.Value);
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
                Id = SDL.SDL_JoystickGetGUID(jdevice).ToString(),
                AxisCount = SDL.SDL_JoystickNumAxes(jdevice),
                ButtonCount = SDL.SDL_JoystickNumButtons(jdevice),
                HatCount = SDL.SDL_JoystickNumHats(jdevice)
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
                axes[i] = SDL.SDL_JoystickGetAxis(jdevice, i);
            
            var buttons = new ButtonState[jcap.ButtonCount];
            for (int i = 0; i < buttons.Length; i++)
                buttons[i] = (SDL.SDL_JoystickGetButton(jdevice, i) == 0) ? ButtonState.Released : ButtonState.Pressed;

            var hats = new JoystickHat[jcap.HatCount];
            for (int i = 0; i < hats.Length; i++)
            {
                hats[i] = new JoystickHat();
                var hatstate = SDL.SDL_JoystickGetHat(jdevice, i);

                switch (hatstate)
                {
                    case SDL.SDL_HAT.SDL_HAT_UP:
                    case SDL.SDL_HAT.SDL_HAT_LEFTUP:
                    case SDL.SDL_HAT.SDL_HAT_RIGHTUP:
                        hats[i].Up = ButtonState.Pressed;
                        break;
                    case SDL.SDL_HAT.SDL_HAT_DOWN:
                    case SDL.SDL_HAT.SDL_HAT_LEFTDOWN:
                    case SDL.SDL_HAT.SDL_HAT_RIGHTDOWN:
                        hats[i].Down = ButtonState.Pressed;
                        break;
                }

                switch (hatstate)
                {
                    case SDL.SDL_HAT.SDL_HAT_LEFT:
                    case SDL.SDL_HAT.SDL_HAT_LEFTDOWN:
                    case SDL.SDL_HAT.SDL_HAT_LEFTUP:
                        hats[i].Left = ButtonState.Pressed;
                        break;
                    case SDL.SDL_HAT.SDL_HAT_RIGHT:
                    case SDL.SDL_HAT.SDL_HAT_RIGHTDOWN:
                    case SDL.SDL_HAT.SDL_HAT_RIGHTUP:
                        hats[i].Right = ButtonState.Pressed;
                        break;
                }
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
