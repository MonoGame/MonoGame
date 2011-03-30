#region File Description
//-----------------------------------------------------------------------------
// GameScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

// #define TutorialVersion //Uncomment this line to play the tutorial version

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Marblets
{
    /// <summary>
    /// GameScreen is the screen that is loaded to play Marblets
    /// </summary>
    class GameScreen : Screen
    {
        private GameBoard board;
        private Texture2D gameOver;

        public GameScreen(Game game, string backgroundImage/*TODO , SoundEntry backgroundMusic*/)
            : base(game, backgroundImage/* TODO , backgroundMusic*/)
        {

            //Remove the define at the top of the file to play the tutorial version
            bool tutorialVersion = false;
#if TutorialVersion
            tutorialVersion = true;
#endif
            if (tutorialVersion)
            {
                board = new TutorialGameBoard(this.Game);
            }
            else
            {
                board = new GameBoard(this.Game);
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            board.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            board.Update(gameTime);

            if (board.GameOver && InputHelper.GamePads[PlayerIndex.One].APressed)
            {
                MarbletsGame.NextGameState = GameState.Started;
            }
        }

        protected override void LoadContent()
        {
            gameOver = MarbletsGame.Content.Load<Texture2D>("Textures/game_over_frame");
            base.LoadContent();
        }


        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            SpriteBatch.Begin(SpriteSortMode.Deferred, SpriteBlendMode.AlphaBlend, 
                              SaveStateMode.None);

            board.Draw(SpriteBatch);

            //Draw Score
            Font.Draw(SpriteBatch, FontStyle.Small, 946, 140,
                      String.Format("{0:###,##0}", MarbletsGame.Score));

            if (board.GameOver)
            {
                SpriteBatch.Draw(gameOver, new Vector2(329, 264), Color.White);
            }

            SpriteBatch.End();

        }

        public void NewGame()
        {
            board.NewGame();
        }
    }
}
