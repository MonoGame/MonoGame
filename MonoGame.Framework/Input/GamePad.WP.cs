// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Input
{
    static partial class GamePad
    {
        internal static bool back;

        internal static void GamePageWP8_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!e.Cancel)
            {
                back = true;
                e.Cancel = true;
            }
        }

        private static GamePadCapabilities PlatformGetCapabilities(int index)
        {
            GamePadCapabilities capabilities = new GamePadCapabilities();

            capabilities.IsConnected = (index == 0);
            capabilities.HasBackButton = true;

            return capabilities;
        }

        private static GamePadState PlatformGetState(int index, GamePadDeadZone deadZoneMode)
        {
            GamePadState state;
            if (index == 0 && back)
            {
                // Consume state
                back = false;
                state = new GamePadState(new GamePadThumbSticks(), new GamePadTriggers(), new GamePadButtons(Buttons.Back), new GamePadDPad());
            }
            else
                state = new GamePadState();

            return state;
        }

        private static bool PlatformSetVibration(int index, float leftMotor, float rightMotor)
        {
            return false;
        }
    }
}
