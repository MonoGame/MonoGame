#region File Description

//-----------------------------------------------------------------------------
// MemoryMadness.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using GameStateManagement;
using System.IO.IsolatedStorage;
using System.IO;

#endregion

namespace MemoryMadness
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MemoryMadnessGame : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;
        ScreenManager screenManager;

        #endregion

        #region Initializations

        public MemoryMadnessGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            //Create a new instance of the Screen Manager
            screenManager = new ScreenManager(this);
            Components.Add(screenManager);

            // Switch to full screen for best game experience
            graphics.IsFullScreen = true;

            graphics.PreferredBackBufferHeight = 800;
            graphics.PreferredBackBufferWidth = 480;

            graphics.SupportedOrientations = DisplayOrientation.Portrait;

            // Initialize sound system
            AudioManager.Initialize(this);

            // Add two new screens
            screenManager.AddScreen(new BackgroundScreen(false), null);
			screenManager.AddScreen(new MainMenuScreen(), null);
/*            if (PhoneApplicationService.Current.StartupMode == StartupMode.Launch)
                screenManager.AddScreen(new MainMenuScreen(), null);
            else
                screenManager.AddScreen(new PauseScreen(true), null);

            // Subscribe to the application's lifecycle events
            PhoneApplicationService.Current.Activated += GameActivated;
            PhoneApplicationService.Current.Deactivated += GameDeactivated;
            PhoneApplicationService.Current.Closing += GameClosing;
            PhoneApplicationService.Current.Launching += GameLaunching;*/
        }

        #endregion

        #region Loading

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            AudioManager.LoadSounds();
            HighScoreScreen.LoadHighscores();
            base.LoadContent();

        }

        #endregion

        #region Tombstoning

        /// <summary>
        /// Saves the full state to the state object and the persistent state to 
        /// isolated storage.
        /// </summary>
 /*       void GameDeactivated(object sender, DeactivatedEventArgs e)
        {
            SaveToStateObject();

            if (PhoneApplicationService.Current.State.ContainsKey("CurrentLevel"))
            {
                SaveToIsolatedStorage((int)PhoneApplicationService.Current.State["CurrentLevel"]);
                PhoneApplicationService.Current.State.Remove("CurrentLevel");
            }
        }

        /// <summary>
        /// Loads the full state from the state object.
        /// </summary>
        void GameActivated(object sender, ActivatedEventArgs e)
        {
            LoadFromStateObject();
            LoadFromIsolatedStorage();
        }

        /// <summary>
        /// Saves persistent state to isolated storage.
        /// </summary>
        void GameClosing(object sender, ClosingEventArgs e)
        {
            if (PhoneApplicationService.Current.State.ContainsKey("CurrentLevel"))
            {
                SaveToIsolatedStorage((int)PhoneApplicationService.Current.State["CurrentLevel"]);
                PhoneApplicationService.Current.State.Remove("CurrentLevel");
            }
            else
            {
                CleanIsolatedStorage();
            }
        }*/


        /// <summary>
        /// Loads persistent state from isolated storage.
        /// </summary>
/*        void GameLaunching(object sender, LaunchingEventArgs e)
        {
            LoadFromIsolatedStorage();
        }*/

        #region Helpers functionality
        /// <summary>
        /// Saves current gameplay progress to the state object.
        /// </summary>
        private void SaveToStateObject()
        {
            // Get the gameplay screen object
            GameplayScreen gameplayScreen = GetGameplayScreen();

            if (null != gameplayScreen)
            {
                // If gameplay screen object found save current game progress to 
                // the state object
                PhoneApplicationService.Current.State["MovesPerformed"] =
                    gameplayScreen.currentLevel.MovesPerformed;

                PhoneApplicationService.Current.State["CurrentLevel"] =
                    gameplayScreen.currentLevel.levelNumber;
            }
        }

        /// <summary>
        /// Loads the game progress from the state object if such information exits.
        /// </summary>
        private void LoadFromStateObject()
        {
            int sequenceProgress = 0;

            // Check state object for sequence progress and load it if found
            if (!PhoneApplicationService.Current.State.ContainsKey("MovesPerformed"))
                PhoneApplicationService.Current.State["MovesPerformed"] = sequenceProgress;
        }

        /// <summary>
        /// Saves the level progress to isolated storage
        /// </summary>
        /// <param name="currentLevel">The level number to save</param>
        private void SaveToIsolatedStorage(int currentLevel)
        {
            // Get Isolated Storage for current application
            using (IsolatedStorageFile isolatedStorageFile
                = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // Create/overwrite file and save provided value
                /*using (IsolatedStorageFileStream fileStream
                    = isolatedStorageFile.CreateFile("MemoryMadness.dat"))
                {
                    using (StreamWriter streamWriter = new StreamWriter(fileStream))
                    {
                        streamWriter.WriteLine(currentLevel);
                    }
                }*/

            }
        }

        /// <summary>
        /// Clean isolated storage from previously saved information.
        /// </summary>
        private void CleanIsolatedStorage()
        {
            using (IsolatedStorageFile isolatedStorageFile
                = IsolatedStorageFile.GetUserStoreForApplication())
            {
                isolatedStorageFile.DeleteFile("MemoryMadness.dat");
            }

        }

        /// <summary>
        /// Loads game progress from isolated storage file if such a file exits.
        /// </summary>
        private void LoadFromIsolatedStorage()
        {
            int currentLevel = 1;

            // Get the isolated storage for the current application
            using (IsolatedStorageFile isolatedStorageFile
                = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // Check whether or not the data file exists
                /*if (isolatedStorageFile.FileExists("MemoryMadness.dat"))
                {
                    // If the file exits, open it and read its information
                    using (IsolatedStorageFileStream fileStream
                        = isolatedStorageFile.OpenFile("MemoryMadness.dat", FileMode.Open))
                    {
                        using (StreamReader streamReader = new StreamReader(fileStream))
                        {
                            if (!int.TryParse(streamReader.ReadLine(), out currentLevel))
                                currentLevel = 1;
                        }
                    }
                }*/
            }

            PhoneApplicationService.Current.State["CurrentLevel"] = currentLevel;
        }

        /// <summary>
        /// Finds a gameplay screen objects among all screens and returns it.
        /// </summary>
        /// <returns>A gameplay screen instance, or null if none 
        /// are available.</returns>
        private GameplayScreen GetGameplayScreen()
        {
            var screens = screenManager.GetScreens();

            foreach (var screen in screens)
            {
                if (screen is GameplayScreen)
                {
                    return screen as GameplayScreen;
                }
            }

            return null;            
        }

        #endregion

        #endregion
    }
}
