// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

#if !(MONOMAC && !PLATFORM_MACOS_LEGACY)

namespace Microsoft.Xna.Framework.Input
{
    static partial class Joystick
    {
        private static JoystickCapabilities PlatformGetCapabilities(int index)
        {
            var cap = OpenTK.Input.Joystick.GetCapabilities(index);

            if (!cap.IsConnected)
                return new JoystickCapabilities()
                {
                    IsConnected = false,
                    AxisCount = 0,
                    ButtonCount = 0,
                    HatCount = 0
                };

            return new JoystickCapabilities 
            {
                IsConnected = true,
                Id = OpenTK.Input.Joystick.GetGuid(index).ToString(),
                AxisCount = cap.AxisCount,
                ButtonCount = cap.ButtonCount,
                HatCount = cap.HatCount
            };
        }

        private static JoystickState PlatformGetState(int index)
        {
            var state = OpenTK.Input.Joystick.GetState(index);

            if (!state.IsConnected)
                return new JoystickState()
                {
                    IsConnected = false,
                    Axes = new float[0],
                    Buttons = new ButtonState[0],
                    Hats = new JoystickHat[0]
                };

            int noa = Enum.GetValues(typeof(OpenTK.Input.JoystickButton)).Length;
            float[] axes = new float[noa];

            for (int i = 0; i < noa; i++)
                axes[i] = state.GetAxis((OpenTK.Input.JoystickAxis)i);

            int nob = Enum.GetValues(typeof(OpenTK.Input.JoystickButton)).Length;
            ButtonState[] buttons = new ButtonState[nob];

            for (int i = 0; i < nob; i++)
                buttons[i] = state.IsButtonDown((OpenTK.Input.JoystickButton)i) ? ButtonState.Pressed : ButtonState.Released;

            int noh = Enum.GetValues(typeof(OpenTK.Input.JoystickHat)).Length;
            JoystickHat[] hats = new JoystickHat[noh];

            for (int i = 0; i < noh; i++)
            {
                var hat = state.GetHat((OpenTK.Input.JoystickHat)i);
                hats[i] = new JoystickHat 
                {
                    Up = hat.IsUp ? ButtonState.Pressed : ButtonState.Released,
                    Down = hat.IsDown ? ButtonState.Pressed : ButtonState.Released,
                    Left = hat.IsLeft ? ButtonState.Pressed : ButtonState.Released,
                    Right = hat.IsRight ? ButtonState.Pressed : ButtonState.Released
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

#endif