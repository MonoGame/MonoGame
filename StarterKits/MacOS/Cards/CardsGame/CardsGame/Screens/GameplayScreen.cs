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
using System.Collections.Generic;
using System.Text;
using GameStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using CardsFramework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Input.Touch;
#endregion

namespace Blackjack
{
    class GameplayScreen : GameScreen
    {
        #region Fields and Properties
        BlackjackCardGame blackJackGame;

        InputHelper inputHelper;

        string theme;
        List<DrawableGameComponent> pauseEnabledComponents = new List<DrawableGameComponent>();
        List<DrawableGameComponent> pauseVisibleComponents = new List<DrawableGameComponent>();
        Rectangle safeArea;

        static Vector2[] playerCardOffset = new Vector2[] 
        { 
            new Vector2(100f * BlackjackGame.WidthScale, 190f * BlackjackGame.HeightScale),
            new Vector2(336f * BlackjackGame.WidthScale, 210f * BlackjackGame.HeightScale),
            new Vector2(570f * BlackjackGame.WidthScale, 190f * BlackjackGame.HeightScale) 
        };
        #endregion

        #region Initiaizations
        /// <summary>
        /// Initializes a new instance of the screen.
        /// </summary>
        public GameplayScreen(string theme)
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.0);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
#if WINDOWS_PHONE
            EnabledGestures = GestureType.Tap;
#endif
            this.theme = theme;
        }
        #endregion

        #region Loading
        /// <summary>
        /// Load content and initializes the actual game.
        /// </summary>
        public override void LoadContent()
        {
            safeArea = ScreenManager.SafeArea;

            // Initialize virtual cursor
            inputHelper = new InputHelper(ScreenManager.Game);
            inputHelper.DrawOrder = 1000;
            ScreenManager.Game.Components.Add(inputHelper);
            // Ignore the curser when not run in Xbox
#if !XBOX
            inputHelper.Visible = false;
            inputHelper.Enabled = false;
#endif

            blackJackGame = new BlackjackCardGame(ScreenManager.GraphicsDevice.Viewport.Bounds,
                new Vector2(safeArea.Left + safeArea.Width / 2 - 50, safeArea.Top + 20),
                GetPlayerCardPosition, ScreenManager, theme);


            InitializeGame();

            base.LoadContent();
        }

        /// <summary>
        /// Unload content loaded by the screen.
        /// </summary>
        public override void UnloadContent()
        {
            ScreenManager.Game.Components.Remove(inputHelper);

            base.UnloadContent();
        }
        #endregion

        #region Update and Render
        /// <summary>
        /// Handle user input.
        /// </summary>
        /// <param name="input">User input information.</param>
        public override void HandleInput(InputState input)
        {
            if (input.IsPauseGame(null))
            {
                PauseCurrentGame();
            }

            base.HandleInput(input);
        }

        /// <summary>
        /// Perform the screen's update logic.
        /// </summary>
        /// <param name="gameTime">The time that has passed since the last call to 
        /// this method.</param>
        /// <param name="otherScreenHasFocus">Whether or not another screen has
        /// the focus.</param>
        /// <param name="coveredByOtherScreen">Whether or not another screen covers
        /// this one.</param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
#if XBOX
            if (Guide.IsVisible)
            {
                PauseCurrentGame();
            }
#endif
            if (blackJackGame != null && !coveredByOtherScreen)
            {
                blackJackGame.Update(gameTime);
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        /// <summary>
        /// Draw the screen
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (blackJackGame != null)
            {
                blackJackGame.Draw(gameTime);
            }

        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Initializes the game component.
        /// </summary>
        private void InitializeGame()
        {
            blackJackGame.Initialize();
            // Add human player
            blackJackGame.AddPlayer(new BlackjackPlayer("Abe", blackJackGame));

            // Add AI players
            BlackjackAIPlayer player = new BlackjackAIPlayer("Benny", blackJackGame);
            blackJackGame.AddPlayer(player);
            player.Hit += player_Hit;
            player.Stand += player_Stand;

            player = new BlackjackAIPlayer("Chuck", blackJackGame);
            blackJackGame.AddPlayer(player);
            player.Hit += player_Hit;
            player.Stand += player_Stand;

            // Load UI assets
            string[] assets = { "blackjack", "bust", "lose", "push", "win", "pass", "shuffle_" + theme };

            for (int chipIndex = 0; chipIndex < assets.Length; chipIndex++)
            {
                blackJackGame.LoadUITexture("UI", assets[chipIndex]);
            }

            blackJackGame.StartRound();
        }

        /// <summary>
        /// Gets the player hand positions according to the player index.
        /// </summary>
        /// <param name="player">The player's index.</param>
        /// <returns>The position for the player's hand on the game table.</returns>
        private Vector2 GetPlayerCardPosition(int player)
        {
            switch (player)
            {
                case 0:
                case 1:
                case 2:
                    return new Vector2(ScreenManager.SafeArea.Left,
                        ScreenManager.SafeArea.Top + 200 * (BlackjackGame.HeightScale - 1)) +
                        playerCardOffset[player];
                default:
                    throw new ArgumentException(
                        "Player index should be between 0 and 2", "player");
            }
        }

        /// <summary>
        /// Pause the game.
        /// </summary>
        private void PauseCurrentGame()
        {
            // Move to the pause screen
            ScreenManager.AddScreen(new BackgroundScreen(), null);
            ScreenManager.AddScreen(new PauseScreen(), null);

            // Hide and disable all components which are related to the gameplay screen
            pauseEnabledComponents.Clear();
            pauseVisibleComponents.Clear();
            foreach (IGameComponent component in ScreenManager.Game.Components)
            {
                if (component is BetGameComponent ||
                    component is AnimatedGameComponent ||
                    component is GameTable ||
                    component is InputHelper)
                {
                    DrawableGameComponent pauseComponent = (DrawableGameComponent)component;
                    if (pauseComponent.Enabled)
                    {
                        pauseEnabledComponents.Add(pauseComponent);
                        pauseComponent.Enabled = false;
                    }
                    if (pauseComponent.Visible)
                    {
                        pauseVisibleComponents.Add(pauseComponent);
                        pauseComponent.Visible = false;
                    }
                }
            }
        }

        /// <summary>
        /// Returns from pause.
        /// </summary>
        public void ReturnFromPause()
        {
            // Reveal and enable all previously hidden components
            foreach (DrawableGameComponent component in pauseEnabledComponents)
            {
                component.Enabled = true;
            }
            foreach (DrawableGameComponent component in pauseVisibleComponents)
            {
                component.Visible = true;
            }
        }
        #endregion

        #region Event Handler
        /// <summary>
        /// Responds to the event sent when AI player's choose to "Stand".
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The 
        /// <see cref="System.EventArgs"/> instance containing the event data.</param>
        void player_Stand(object sender, EventArgs e)
        {
            blackJackGame.Stand();
        }

        /// <summary>
        /// Responds to the event sent when AI player's choose to "Split".
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The 
        /// <see cref="System.EventArgs"/> instance containing the event data.</param>
        void player_Split(object sender, EventArgs e)
        {
            blackJackGame.Split();
        }

        /// <summary>
        /// Responds to the event sent when AI player's choose to "Hit".
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void player_Hit(object sender, EventArgs e)
        {
            blackJackGame.Hit();
        }

        /// <summary>
        /// Responds to the event sent when AI player's choose to "Double".
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void player_Double(object sender, EventArgs e)
        {
            blackJackGame.Double();
        }
        #endregion
    }
}
