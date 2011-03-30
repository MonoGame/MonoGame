#region File Description

//-----------------------------------------------------------------------------
// LoadingScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using GameStateManagement;
using Microsoft.Xna.Framework.Input.Touch;

#endregion

namespace MemoryMadness
{
    class HighScoreScreen : GameScreen
    {
        #region Fields

        // define default highscore table
        const int highscorePlaces = 10;
        public static List<KeyValuePair<string, int>> highScore = 
            new List<KeyValuePair<string, int>>(highscorePlaces)
        {
            new KeyValuePair<string, int>("Jasper",10),
            new KeyValuePair<string, int>("Ellen",9),
            new KeyValuePair<string, int>("Terry",8),
            new KeyValuePair<string, int>("Lori",7),
            new KeyValuePair<string, int>("Michael",6),
            new KeyValuePair<string, int>("Carol",5),
            new KeyValuePair<string, int>("Toni",4),
            new KeyValuePair<string, int>("Cassie",3),
            new KeyValuePair<string, int>("Luca",2),
            new KeyValuePair<string, int>("Brian",1)
        };

        SpriteFont highScoreFont;

        #endregion

        #region Initialzations

        public HighScoreScreen()
        {
            EnabledGestures = GestureType.Tap;
        }

        #endregion

        #region Loading

        /// <summary>
        /// Load screen resources.
        /// </summary>
        public override void LoadContent()
        {
            highScoreFont = Load<SpriteFont>(@"Fonts\HighScoresFont");

            base.LoadContent();
        }

        #endregion

        #region Handle Input

        /// <summary>
        /// Handles user input as a part of screen logic update
        /// </summary>
        /// <param name="input"></param>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            if (input.IsPauseGame(null))
            {
                Exit();
            }

            // Return to the main menu when a tap gesture is recognized
            if (input.Gestures.Count > 0)
            {
                GestureSample sample = input.Gestures[0];
                if (sample.GestureType == GestureType.Tap)
                {
                    Exit();

                    input.Gestures.Clear();
                }
            }
        }

        /// <summary>
        /// Exit this screen
        /// </summary>
        private void Exit()
        {
            this.ExitScreen();
            ScreenManager.AddScreen(new BackgroundScreen(false), null);
            ScreenManager.AddScreen(new MainMenuScreen(), null);
        }

        #endregion

        #region Render

        /// <summary>
        /// Renders the screen
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            ScreenManager.SpriteBatch.Begin();

            // Draw the title
            string text = "High Scores";
            var textSize = highScoreFont.MeasureString(text);
            var position = new Vector2(
                ScreenManager.GraphicsDevice.Viewport.Width / 2 - textSize.X / 2,
                340);

            ScreenManager.SpriteBatch.DrawString(highScoreFont, text, 
                position, Color.Red);

            // Draw the highscores table
            for (int i = 0; i < highScore.Count; i++)
            {
                ScreenManager.SpriteBatch.DrawString(highScoreFont, 
                    String.Format("{0,2}. {1}", i + 1, highScore[i].Key),
                    new Vector2(50, i * 40 + position.Y + 40), Color.YellowGreen);
                ScreenManager.SpriteBatch.DrawString(highScoreFont, 
                    highScore[i].Value.ToString(),
                    new Vector2(370, i * 40 + position.Y + 40), 
                    Color.YellowGreen);
            }

            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }

        #endregion

        #region Highscore loading/saving logic

        /// <summary>
        /// Check if a score belongs on the high score table
        /// </summary>
        /// <returns></returns>
        public static bool IsInHighscores(int level)
        {
            // If the score is higher than the worst score in the table
            return level > highScore[highscorePlaces - 1].Value;
        }

        /// <summary>
        /// Put high score on highscores table
        /// </summary>
        /// <param name="name">Name of the player who achieved the highscore.</param>
        /// <param name="level">The level the player reached.</param>
        public static void PutHighScore(string playerName, int level)
        {
            if (IsInHighscores(level))
            {
                highScore[highscorePlaces - 1] = 
                    new KeyValuePair<string, int>(playerName, level);
                OrderGameScore();
            }
        }

        /// <summary>
        /// Order the high scores table.
        /// </summary>
        private static void OrderGameScore()
        {
            highScore.Sort(CompareScores);
        }

        /// <summary>
        /// Comparison method used to compare two highscore entries.
        /// </summary>
        /// <param name="score1">First highscore entry.</param>
        /// <param name="score2">Second highscore entry.</param>
        /// <returns>1 if the first highscore is smaller than the second, 0 if both
        /// are equal and -1 otherwise.</returns>
        private static int CompareScores(KeyValuePair<string, int> score1, 
            KeyValuePair<string, int> score2)
        {
            if (score1.Value < score2.Value)
            {
                return 1;
            }

            if (score1.Value == score2.Value)
            {
                return 0;
            }

            return -1;
        }

        /// <summary>
        /// Saves the current highscore to a text file. 
        /// </summary>
        public static void SaveHighscore()
        {
            // Get the place to store data
            using (IsolatedStorageFile isf = 
                IsolatedStorageFile.GetUserStoreForApplication())
            {
                // Create a file to save the highscore data
                
                /*using (IsolatedStorageFileStream isfs = 
                    isf.CreateFile("highscores.txt"))
                {
                    using (StreamWriter writer = new StreamWriter(isfs))
                    {
                        for (int i = 0; i < highScore.Count; i++)
                        {
                            // Write the scores
                            writer.WriteLine(highScore[i].Key);
                            writer.WriteLine(highScore[i].Value.ToString());
                        }
                    }
                }*/
            }
        }

        /// <summary>
        /// Loads the high scores from a text file.  
        /// </summary>
        public static void LoadHighscores()
        {
            // Get the place where data is stored
            /*using (IsolatedStorageFile isf = 
                IsolatedStorageFile.GetUserStoreForApplication())
            {
                // Try to open the file
                if (isf.FileExists("highscores.txt"))
                {
                    using (IsolatedStorageFileStream isfs = 
                        isf.OpenFile("highscores.txt", FileMode.Open))                        
                    {
                        // Get the stream to read the data
                        using (StreamReader reader = new StreamReader(isfs))
                        {
                            // Read the highscores
                            int i = 0;
                            while (!reader.EndOfStream)
                            {
                                string name = reader.ReadLine();
                                int score = int.Parse(reader.ReadLine());

                                highScore[i++] =  new KeyValuePair<string, int>(
                                    name, score);
                            }
                        }
                    }
                }
            }

            OrderGameScore();
            */
        }

        #endregion
    }
}
