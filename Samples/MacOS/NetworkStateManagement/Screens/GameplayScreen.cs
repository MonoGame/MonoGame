#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
#endregion

namespace NetworkStateManagement
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    class GameplayScreen : GameScreen
    {
        #region Fields

        NetworkSession networkSession;

        ContentManager content;
        SpriteFont gameFont;

        Vector2 playerPosition = new Vector2(100, 100);
        Vector2 enemyPosition = new Vector2(100, 100);

        Random random = new Random();

        float pauseAlpha;

        #endregion

        #region Properties


        /// <summary>
        /// The logic for deciding whether the game is paused depends on whether
        /// this is a networked or single player game. If we are in a network session,
        /// we should go on updating the game even when the user tabs away from us or
        /// brings up the pause menu, because even though the local player is not
        /// responding to input, other remote players may not be paused. In single
        /// player modes, however, we want everything to pause if the game loses focus.
        /// </summary>
        new bool IsActive
        {
            get
            {
                if (networkSession == null)
                {
                    // Pause behavior for single player games.
                    return base.IsActive;
                }
                else
                {
                    // Pause behavior for networked games.
                    return !IsExiting;
                }
            }
        }


        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen(NetworkSession networkSession)
        {
            this.networkSession = networkSession;

            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");

            gameFont = content.Load<SpriteFont>("gamefont");

            // A real game would probably have more content than this sample, so
            // it would take longer to load. We simulate that by delaying for a
            // while, giving you a chance to admire the beautiful loading screen.
            Thread.Sleep(1000);

            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the state of the game.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);

            // Gradually fade in or out depending on whether we are covered by the pause screen.
            if (coveredByOtherScreen)
                pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
            else
                pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);

            if (IsActive)
            {
                // Apply some random jitter to make the enemy move around.
                const float randomization = 10;

                enemyPosition.X += (float)(random.NextDouble() - 0.5) * randomization;
                enemyPosition.Y += (float)(random.NextDouble() - 0.5) * randomization;

                // Apply a stabilizing force to stop the enemy moving off the screen.
                Vector2 targetPosition = new Vector2(200, 200);

                enemyPosition = Vector2.Lerp(enemyPosition, targetPosition, 0.05f);

                // TODO: this game isn't very fun! You could probably improve
                // it by inserting something more interesting in this space :-)
            }

            // If we are in a network game, check if we should return to the lobby.
            if ((networkSession != null) && !IsExiting)
            {
                if (networkSession.SessionState == NetworkSessionState.Lobby)
                {
                    LoadingScreen.Load(ScreenManager, true, null,
                                       new BackgroundScreen(),
                                       new LobbyScreen(networkSession));
                }
            }
        }


        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            if (ControllingPlayer.HasValue)
            {
                // In single player games, handle input for the controlling player.
                HandlePlayerInput(input, ControllingPlayer.Value);
            }
            else if (networkSession != null)
            {
                // In network game modes, handle input for all the
                // local players who are participating in the session.
                foreach (LocalNetworkGamer gamer in networkSession.LocalGamers)
                {
                    if (!HandlePlayerInput(input, gamer.SignedInGamer.PlayerIndex))
                        break;
                }
            }
        }


        /// <summary>
        /// Handles input for the specified player. In local game modes, this is called
        /// just once for the controlling player. In network modes, it can be called
        /// more than once if there are multiple profiles playing on the local machine.
        /// Returns true if we should continue to handle input for subsequent players,
        /// or false if this player has paused the game.
        /// </summary>
        bool HandlePlayerInput(InputState input, PlayerIndex playerIndex)
        {
            // Look up inputs for the specified player profile.
            KeyboardState keyboardState = input.CurrentKeyboardStates[(int)playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[(int)playerIndex];

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            bool gamePadDisconnected = !gamePadState.IsConnected &&
                                       input.GamePadWasConnected[(int)playerIndex];

            if (input.IsPauseGame(playerIndex) || gamePadDisconnected)
            {
                ScreenManager.AddScreen(new PauseMenuScreen(networkSession), playerIndex);
                return false;
            }

            // Otherwise move the player position.
            Vector2 movement = Vector2.Zero;

            if (keyboardState.IsKeyDown(Keys.Left))
                movement.X--;

            if (keyboardState.IsKeyDown(Keys.Right))
                movement.X++;

            if (keyboardState.IsKeyDown(Keys.Up))
                movement.Y--;

            if (keyboardState.IsKeyDown(Keys.Down))
                movement.Y++;

            Vector2 thumbstick = gamePadState.ThumbSticks.Left;

            movement.X += thumbstick.X;
            movement.Y -= thumbstick.Y;

            if (movement.Length() > 1)
                movement.Normalize();

            playerPosition += movement * 2;

            return true;
        }


        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.CornflowerBlue, 0, 0);

            // Our player and enemy are both actually just text strings.
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            spriteBatch.DrawString(gameFont, "// TODO", playerPosition, Color.Green);

            spriteBatch.DrawString(gameFont, "Insert Gameplay Here",
                                   enemyPosition, Color.DarkRed);

            if (networkSession != null)
            {
                string message = "Players: " + networkSession.AllGamers.Count;
                Vector2 messagePosition = new Vector2(100, 480);
                spriteBatch.DrawString(gameFont, message, messagePosition, Color.White);
            }

            spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 || pauseAlpha > 0)
            {
                float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

                ScreenManager.FadeBackBufferToBlack(alpha);
            }
        }


        #endregion
    }
}
