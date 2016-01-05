// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

#if !PLATFORM_MACOS_LEGACY

namespace Microsoft.Xna.Framework.Input
{
    static partial class Joystick
    {
        private static JoystickCapabilities PlatformGetCapabilities(int index)
        {
            return new JoystickCapabilities()
            {
                IsConnected = false,
                AxisCount = 0,
                ButtonCount = 0,
                HatCount = 0
            };
        }

        private static JoystickState PlatformGetState(int index)
        {
            return new JoystickState()
            {
                IsConnected = false,
                Axes = new float[0],
                Buttons = new ButtonState[0],
                Hats = new JoystickHat[0]
            };
        }
    }
}

#endif