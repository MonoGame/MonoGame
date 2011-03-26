#region File Description
//-----------------------------------------------------------------------------
// GameScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
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

        public static TouchCollection touchCollection;

        public GameScreen(Game game, string backgroundImage)
            : base(game, backgroundImage)
        {
            board = new GameBoard(this.Game);
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

            // Check if the user wants to restart the game.
            if (board.GameOver)
            {
                touchCollection = TouchPanel.GetState();

                if (touchCollection.Count > 0)
                {
                    if (touchCollection[0].Position.X > 0 &&
                        touchCollection[0].Position.X < 272 &&
                        touchCollection[0].Position.Y > 0 &&
                        touchCollection[0].Position.Y < 480)
                    {
                        MarbletsGame.NextGameState = GameState.Started;
                    }
                }
            }
        }

        protected override void LoadContent()
        {
            gameOver = 
                MarbletsGame.Content.Load<Texture2D>("game_over_frame_zunehd");

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            SpriteBatch.Begin(SpriteSortMode.Deferred,SpriteBlendMode.AlphaBlend, 
                              SaveStateMode.None);

            board.Draw(SpriteBatch);

            //Draw Score

            // Added functionality to account for the screen orientation.
            switch (MarbletsGame.screenOrientation)
            {
                case MarbletsGame.ScreenOrientation.LandscapeRight:
                    Font.Draw(SpriteBatch, FontStyle.Large, 83, 100,
                              String.Format("{0:###,##0}", MarbletsGame.Score));

                    if (board.GameOver)
                    {
                        SpriteBatch.Draw(gameOver, new Vector2(200, 265), null, 
                            Color.White, MarbletsGame.screenRotation, Vector2.Zero, 1.0f,SpriteEffects.None, 0.0f);
                    }
                    break;

                case MarbletsGame.ScreenOrientation.LandscapeLeft:
                    Font.Draw(SpriteBatch, FontStyle.Large, (480 - 48 + 3), (320 - 125),
                              String.Format("{0:###,##0}", MarbletsGame.Score));

                    if (board.GameOver)
                    {
                        SpriteBatch.Draw(gameOver, new Vector2((320 - 100), (480 - 355)), 
                            null, Color.White, MarbletsGame.screenRotation, Vector2.Zero,
                            1.0f, SpriteEffects.None, 0.0f);
                    }
                    break;

                default:
                    break;
            }

            SpriteBatch.End();
        }

        public void NewGame()
        {
            board.NewGame();
        }

        public void RecalculateMarblePositions()
        {
            board.RecalculateMarblePositions();
        }
    }
}
