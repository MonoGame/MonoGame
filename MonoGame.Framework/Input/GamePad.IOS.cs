// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Input
{
    static partial class GamePad
    {
        private static int PlatformGetMaxNumberOfGamePads()
        {
            return 0;
        }

        private static GamePadCapabilities PlatformGetCapabilities(int index)
        {
            return new GamePadCapabilities { IsConnected = false };
        }
               
        private static GamePadState PlatformGetState(int index, GamePadDeadZone deadZoneMode)
        {
            return new GamePadState() { IsConnected = false };
        }

        private static bool PlatformSetVibration(int index, float leftMotor, float rightMotor)
        {
            return false;
        }
    }
}
