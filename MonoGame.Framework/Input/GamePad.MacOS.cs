// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

#if !PLATFORM_MACOS_LEGACY

namespace Microsoft.Xna.Framework.Input
{
    /// <summary> 
    /// Supports querying the game controllers and setting the vibration motors.
    /// </summary>
    public static partial class GamePad
    {
        private static int PlatformGetMaxIndex()
        {
            return 0;
        }

        private static GamePadCapabilities PlatformGetCapabilities(int index)
        {
            throw new NotSupportedException("We don't support game pads under the new Xamarin.Mac API yet");
        }

        private static GamePadState PlatformGetState(int index, GamePadDeadZone deadZoneMode)
        {
            throw new NotSupportedException("We don't support game pads under the new Xamarin.Mac API yet");
        }

        private static bool PlatformSetVibration(int index, float leftMotor, float rightMotor)
        {
            throw new NotSupportedException("We don't support game pads under the new Xamarin.Mac API yet");
        }
    }
}

#endif