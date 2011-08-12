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
using System.IO;
using System.IO.IsolatedStorage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.GamerServices;

#if !WINDOWS_PHONE
using Microsoft.Xna.Framework.Storage;
#endif

#endregion

namespace HoneycombRush
{
    class HighScoreScreen : GameScreen
    {
        #region Fields/Properties

        static readonly string HighScoreFilename = "highscores.txt";

#if WINDOWS_PHONE
        const int highscorePlaces = 5;
#else
        const int highscorePlaces = 7;
#endif
        public static List<KeyValuePair<string, int>> highScore = new List<KeyValuePair<string, int>>(highscorePlaces)
        {
            new KeyValuePair<string,int>
                ("Jasper",55000),
            new KeyValuePair<string,int>
                ("Ellen",52750),
            new KeyValuePair<string,int>
                ("Terry",52200),
            new KeyValuePair<string,int>
                ("Lori",50200),
            new KeyValuePair<string,int>
                ("Michael",50750),
#if !WINDOWS_PHONE
            new KeyValuePair<string,int>
                ("Frodo",49550),
            new KeyValuePair<string,int>
                ("Chuck",46750),
#endif
        };

        SpriteFont highScoreFont;

        Dictionary<int, string> numberPlaceMapping;

        Rectangle safeArea;

        public static bool HighscoreLoaded { get; private set; }

        public static bool HighscoreSaved { get; private set; }

#if !WINDOWS_PHONE
        // These variables will be used to save/load the high score data asynchronously
        static bool shouldSaveHighScore;
        static bool savingHighscore;
        static bool loadingHighscore;
        static bool deviceSelectorLaunched;

        public static StorageDevice Storage { get; set; }
#endif


        #endregion

        #region Initialzations


        static HighScoreScreen()
        {
            HighscoreLoaded = false;
            HighscoreSaved = false;
        }

        /// <summary>
        /// Creates a new highscore screen instance.
        /// </summary>
        public HighScoreScreen()
        {
            EnabledGestures = GestureType.Tap;
            if (HighscoreLoaded == false)
            {
                throw new InvalidOperationException("Missing highscore data");
            }

            numberPlaceMapping = new Dictionary<int, string>();
            InitializeMapping();
        }

        /// <summary>
        /// Load screen resources
        /// </summary>
        public override void LoadContent()
        {
            highScoreFont = Load<SpriteFont>(@"Fonts\HighScoreFont");

            safeArea = SafeArea;

            base.LoadContent();
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Handles user input as a part of screen logic update.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        /// <param name="input">Input information.</param>
        public override void HandleInput(GameTime gameTime, InputState input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

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
            // Handle gamepad input
            PlayerIndex player;
            // Handle keyboard input
            if (input.IsNewKeyPress(Keys.Enter, ControllingPlayer, out player) ||
                input.IsNewKeyPress(Keys.Space, ControllingPlayer, out player))
            {
                Exit();
            }
        }

        /// <summary>
        /// Exit this screen.
        /// </summary>
        private void Exit()
        {
            this.ExitScreen();
            ScreenManager.AddScreen(new BackgroundScreen("titlescreen"), null);
            ScreenManager.AddScreen(new MainMenuScreen(), null);
        }


        #endregion

        #region Update


        /// <summary>
        /// Performs update logic.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        /// <param name="otherScreenHasFocus">Whether another screen has the focus.</param>
        /// <param name="coveredByOtherScreen">Whether this screen is covered by another.</param>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
#if !WINDOWS_PHONE
            if (shouldSaveHighScore)
            {
                SaveHighscore();
            }
#endif

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }


        #endregion

        #region Render


        /// <summary>
        /// Renders the screen.
        /// </summary>
        /// <param name="gameTime">Game time information</param>
        public override void Draw(GameTime gameTime)
        {
            if (HighscoreLoaded == false)
            {
                base.Draw(gameTime);
                return;
            }

            ScreenManager.SpriteBatch.Begin();

            // Draw the high-scores table
#if WINDOWS_PHONE
            for (int i = 0; i < highScore.Count; i++)
            {
                if (!string.IsNullOrEmpty(highScore[i].Key))
                {
                    // Draw place number
                    ScreenManager.SpriteBatch.DrawString(highScoreFont, GetPlaceString(i),
                        new Vector2(safeArea.Left + UIConstants.HighScorePlaceLeftMargin,
                            safeArea.Top + i * UIConstants.HighScoreVerticalJump + UIConstants.HighScoreTopMargin),
                        Color.Black);

                    // Draw Name
                    ScreenManager.SpriteBatch.DrawString(highScoreFont, highScore[i].Key,
                        new Vector2(safeArea.Left + UIConstants.HighScoreNameLeftMargin,
                            safeArea.Top + i * UIConstants.HighScoreVerticalJump + UIConstants.HighScoreTopMargin),
                        Color.DarkRed);

                    // Draw score
                    ScreenManager.SpriteBatch.DrawString(highScoreFont, highScore[i].Value.ToString(),
                        new Vector2(safeArea.Left + UIConstants.HighScoreScoreLeftMargin,
                            safeArea.Top + i * UIConstants.HighScoreVerticalJump + UIConstants.HighScoreTopMargin),
                        Color.Yellow);
                }
            }
#else
            float verticalPosition = UIConstants.HighScoreTopMargin;

            for (int i = 0; i < highScore.Count; i++)
            {
                if (!string.IsNullOrEmpty(highScore[i].Key))
                {
                    // Draw place number
                    ScreenManager.SpriteBatch.DrawString( highScoreFont, GetPlaceString(i),
                        new Vector2(safeArea.Left + UIConstants.HighScorePlaceLeftMargin,
                            safeArea.Top + verticalPosition),
                        Color.Black);

                    // Draw Name
                    ScreenManager.SpriteBatch.DrawString(highScoreFont, highScore[i].Key, 
                        new Vector2(safeArea.Left + UIConstants.HighScoreNameLeftMargin,
                            safeArea.Top + verticalPosition),
                        Color.DarkRed);

                    // Draw score
                    ScreenManager.SpriteBatch.DrawString(highScoreFont, highScore[i].Value.ToString(),
                        new Vector2(safeArea.Left + UIConstants.HighScoreScoreLeftMargin,
                            safeArea.Top + verticalPosition),
                        Color.Yellow);
                }

                // Odd and even lines have different height. Remember that "i" is an index so even i's are actually
                // odd lines.
                if (i % 2 == 0)
                {
                    verticalPosition += UIConstants.HighScoreOddVerticalJump;
                }
                else
                {
                    verticalPosition += UIConstants.HighScoreEvenVerticalJump;
            }
            }
#endif

            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }


        #endregion

        #region Highscore loading/saving logic


        /// <summary>
        /// Check if a score belongs on the high score table.
        /// </summary>
        /// <returns>True if the score belongs on the highscore table, false otherwise.</returns>
        public static bool IsInHighscores(int score)
        {
            // If the score is better than the worst score in the table
            return score > highScore[highscorePlaces - 1].Value;
        }

        /// <summary>
        /// Put high score on highscores table.
        /// </summary>
        /// <param name="name">Player's name.</param>
        /// <param name="score">The player's score.</param>
        public static void PutHighScore(string playerName, int score)
        {
            if (IsInHighscores(score))
            {
                highScore[highscorePlaces - 1] = new KeyValuePair<string, int>(playerName, score);
                OrderGameScore();
                SaveHighscore();
            }
        }

        /// <summary>
        /// Call this method whenever the highscore data changes. This will mark the highscore data as not changed.
        /// </summary>
        public static void HighScoreChanged()
        {
            HighscoreSaved = false;
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

#if !WINDOWS_PHONE
        /// <summary>
        /// Initializes the screen's storage device.
        /// </summary>
        public static void InitializeStorageDevice()
        {
            if (Storage == null && Guide.IsVisible == false && deviceSelectorLaunched == false)
            {
                deviceSelectorLaunched = true;
                StorageDevice.BeginShowSelector(PlayerIndex.One, GetDevice, null);
            }
        }

        /// <summary>
        /// Handler for asynchronous storage device result.
        /// </summary>
        /// <param name="result">Result of the call for storage device selection/initialization.</param>
        static void GetDevice(IAsyncResult result)
        {
            Storage = StorageDevice.EndShowSelector(result);
            deviceSelectorLaunched = false;
        }
#endif

        /// <summary>
        /// Saves the current highscore to a text file. 
        /// </summary>
        public static void SaveHighscore()
        {
#if WINDOWS_PHONE
            // Get the place to store the data
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // Create the file to save the data
                using (IsolatedStorageFileStream isfs = isf.CreateFile(HighScoreScreen.HighScoreFilename))
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
                }
            }

            HighscoreSaved = true;
#else
            if (Storage == null || Storage.IsConnected == false)
            {
                shouldSaveHighScore = true;
                // We do not have a storage device, initialize it                
                InitializeStorageDevice();
            }
            else if (!savingHighscore)
            {
                shouldSaveHighScore = false;
                savingHighscore = true;
                SaveHighscoreToStorage();
            }
#endif
        }

#if !WINDOWS_PHONE
        /// <summary>
        /// Saves the high score data to the initialized storage device.
        /// </summary>
        private static void SaveHighscoreToStorage()
        {
            Storage.BeginOpenContainer(HoneycombRush.GameName, SaveContainerOpened, null);
        }

        /// <summary>
        /// Handler for initializing the storage container.
        /// </summary>
        /// <param name="result">Asynchronous result of the storage container initialization.</param>
        private static void SaveContainerOpened(IAsyncResult result)
        {
            StorageContainer container = Storage.EndOpenContainer(result);

            if (container.FileExists(HighScoreFilename))
            {
                container.DeleteFile(HighScoreFilename);
            }

            Stream stream = container.CreateFile(HighScoreFilename);

            using (StreamWriter writer = new StreamWriter(stream))
            {
                for (int i = 0; i < highScore.Count; i++)
                {
                    // Write the scores
                    writer.WriteLine(highScore[i].Key);
                    writer.WriteLine(highScore[i].Value.ToString());
                }
            }

            HighscoreSaved = true;
            savingHighscore = false;
        }
#endif

        /// <summary>
        /// Loads the high score from a text file.  
        /// </summary>
        public static void LoadHighscores()
        {
#if WINDOWS_PHONE
    // Get the place the data stored
    using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
    {
        // Try to open the file
        if (isf.FileExists(HighScoreScreen.HighScoreFilename))
        {
            using (IsolatedStorageFileStream isfs =
                isf.OpenFile(HighScoreScreen.HighScoreFilename, FileMode.Open))
            {
                // Get the stream to read the data
                using (StreamReader reader = new StreamReader(isfs))
                {
                    // Read the highscores
                    int i = 0;
                    while (!reader.EndOfStream)
                    {
                        string name = reader.ReadLine();
                        string score = reader.ReadLine();
                        highScore[i++] = new KeyValuePair<string, int>(name, int.Parse(score));
                    }
                }
            }
        }
    }

    OrderGameScore();

    HighscoreLoaded = true;
#else
            if (Storage == null || Storage.IsConnected == false)
            {
                // We do not have a storage device, initialize it                
                InitializeStorageDevice();
            }
            else if (!loadingHighscore)
            {
                loadingHighscore = true;
                LoadHighscoreFromStorage();
            }
#endif
        }

#if !WINDOWS_PHONE
        /// <summary>
        /// Handler for initializing the storage container.
        /// </summary>
        /// <param name="result">Asynchronous result of the storage container initialization.</param>
        public static void LoadHighscoreFromStorage()
        {
            Storage.BeginOpenContainer(HoneycombRush.GameName, LoadContainerOpened, null);
        }

        /// <summary>
        /// Handler for initializing the storage container.
        /// </summary>
        /// <param name="result">Asynchronous result of the storage container initialization.</param>
        public static void LoadContainerOpened(IAsyncResult result)
        {
            StorageContainer container = Storage.EndOpenContainer(result);

            if (container.FileExists(HighScoreFilename))
            {
                Stream stream = container.OpenFile(HighScoreFilename, FileMode.Open);

                using (StreamReader reader = new StreamReader(stream))
                {
                    // Read the highscores
                    int i = 0;
                    while (!reader.EndOfStream)
                    {
                        string name = reader.ReadLine();
                        string score = reader.ReadLine();
                        highScore[i++] = new KeyValuePair<string, int>(name, int.Parse(score));
                    }
                }
            }

            OrderGameScore();

            HighscoreLoaded = true;
            loadingHighscore = false;
        }
#endif

        /// <summary>
        /// Gets a string describing an index's position in the highscore.
        /// </summary>
        /// <param name="number">Score's index.</param>
        /// <returns>A string describing the score's index.</returns>
        private string GetPlaceString(int number)
        {
            return numberPlaceMapping[number];
        }

        /// <summary>
        /// Initializes the mapping between score indices and position strings.
        /// </summary>
        private void InitializeMapping()
        {
            numberPlaceMapping.Add(0, "1ST");
            numberPlaceMapping.Add(1, "2ND");
            numberPlaceMapping.Add(2, "3RD");
            numberPlaceMapping.Add(3, "4TH");
            numberPlaceMapping.Add(4, "5TH");
            numberPlaceMapping.Add(5, "6TH");
            numberPlaceMapping.Add(6, "7TH");
        }


        #endregion
    }
}
