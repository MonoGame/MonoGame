// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Input;

static partial class GamePad
{
    private const int MaxSupported = 0;

    private static int PlatformGetMaxNumberOfGamePads()
    {
        return MaxSupported;
    }

    private static GamePadCapabilities PlatformGetCapabilities(int index)
    {
        return new GamePadCapabilities();
    }

    private static GamePadState PlatformGetState(int index, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
    {
        return GamePadState.Default;
    }

    private static bool PlatformSetVibration(int index, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger)
    {
        return false;
    }
}
