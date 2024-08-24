// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Input;

static partial class GamePad
{
    private static int PlatformGetMaxNumberOfGamePads()
    {
        return 16;
    }

    private static GamePadCapabilities PlatformGetCapabilities(int index)
    {
        return new GamePadCapabilities();
    }

    private static GamePadState PlatformGetState(int index, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
    {
        return new GamePadState();
    }

    private static bool PlatformSetVibration(int index, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger)
    {
        return false;
    }
}
