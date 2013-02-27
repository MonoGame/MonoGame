using System;

namespace Microsoft.Xna.Framework.Input
{
    public static class GamePad
    {
        internal static bool back;

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
    }
}
