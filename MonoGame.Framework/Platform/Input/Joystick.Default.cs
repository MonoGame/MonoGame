// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Input
{
    static partial class Joystick
    {
        private const bool PlatformIsSupported = false;

        private static JoystickCapabilities PlatformGetCapabilities(int index)
        {
            return new JoystickCapabilities()
            {
                IsConnected = false,
                DisplayName = string.Empty,
                IsGamepad = false,
                AxisCount = 0,
                ButtonCount = 0,
                HatCount = 0
            };
        }

        private static JoystickState PlatformGetState(int index)
        {
            return _defaultJoystickState;
        }

        private static int PlatformLastConnectedIndex
        {
            get
            {
                return -1;
            }
        }

        private static void PlatformGetState(ref JoystickState joystickState, int index)
        {

        }
    }
}

