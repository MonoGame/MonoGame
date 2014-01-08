using Microsoft.Devices;
using System;

namespace Microsoft.Xna.Framework.Input
{
    public static class GamePad
    {
        internal static bool back;

        /// <summary>
        /// Specifies the total timespan that the vibration motor will be active for.
        /// </summary>
        public static TimeSpan VibrationTime = TimeSpan.FromMilliseconds(500);

        internal static void GamePageWP8_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            back = true;
            e.Cancel = true;
        }

        public static Microsoft.Xna.Framework.Input.GamePadCapabilities GetCapabilities(PlayerIndex playerIndex)
        {
            GamePadCapabilities capabilities = new GamePadCapabilities();

            capabilities.IsConnected = (playerIndex == PlayerIndex.One);
            capabilities.HasBackButton = true;

            return capabilities;
        }

        public static Microsoft.Xna.Framework.Input.GamePadState GetState(PlayerIndex playerIndex)
        {
            GamePadState state;
            if (playerIndex == PlayerIndex.One && back)
            {
                // Consume state
                back = false;
                state = new GamePadState(new GamePadThumbSticks(), new GamePadTriggers(), new GamePadButtons(Buttons.Back), new GamePadDPad());
            }
            else
                state = new GamePadState();

            return state;
        }

        public static bool SetVibration(PlayerIndex playerIndex, float leftMotor, float rightMotor)
        {
            VibrateController controller = VibrateController.Default;
            controller.Start(VibrationTime);
            return true;
        }
    }
}
