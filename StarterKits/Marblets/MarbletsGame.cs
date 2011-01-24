#region File Description
//-----------------------------------------------------------------------------
// MarbletsGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
#if IPHONE
using XnaTouch.Framework;
using XnaTouch.Framework.Audio;
using XnaTouch.Framework.Graphics;
using XnaTouch.Framework.Input;
using XnaTouch.Framework.Storage;
using XnaTouch.Framework.Content;
using XnaTouch.Framework.GamerServices;
using XnaTouch.Framework.Input.Touch;
#else
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
#endif
#endregion

namespace Marblets
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    partial class MarbletsGame : Game
    {
        // Added functionality to account for the screen orientation.
        public enum ScreenOrientation
        {
            Normal,
            UpsideDown,
            LandscapeRight,
            LandscapeLeft
        }

        public static ScreenOrientation screenOrientation =
            ScreenOrientation.LandscapeRight;
        public static ScreenOrientation priorScreenOrientation =
            ScreenOrientation.LandscapeRight;
        public static float screenRotation = 4.72f;

        public static TouchCollection touchCollection;
        public static AccelerometerState accelerometerState;

        /// <summary>
        /// A cache of content used by the game
        /// </summary>
        public new static ContentManager Content;

        /// <summary>
        /// The game settings from settings.xml
        /// </summary>
        public static Settings Settings = new Settings();

        /// <summary>
        /// The current game state
        /// </summary>
        public static GameState GameState = GameState.Started;

        /// <summary>
        /// The storage device that the game is saving high-scores to.
        /// </summary>
        public static StorageDevice StorageDevice = null;

        /// <summary>
        /// The new game state
        /// </summary>
        public static GameState NextGameState = GameState.None;

        public static int Score; //= 0;

        public static List<int> HighScores;

        private GraphicsDeviceManager graphics;
        private GameScreen mainGame;
        private TitleScreen splashScreen;

        public MarbletsGame()
        {
            Settings.Load();

            //Create the content pipeline manager.
            base.Content.RootDirectory = "Content";
            MarbletsGame.Content = base.Content;

            //Set up the device to be HD res.
            graphics = new GraphicsDeviceManager(this);
			
			// Zune use 30 frames per second
			this.TargetElapsedTime = TimeSpan.FromSeconds(1/30.0f);
			
			// The assets need to be stretched...put some quality
			graphics.PreferMultiSampling = true;

            mainGame = new GameScreen(this, "play_frame_zunehd");
            mainGame.Enabled = false;
            mainGame.Visible = false;
            this.Components.Add(mainGame);

            splashScreen = new TitleScreen(this, "title_frame_zunehd");
            splashScreen.Enabled = true;
            splashScreen.Visible = true;
            this.Components.Add(splashScreen);

            this.Components.Add(new GamerServicesComponent(this));
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to
        /// run. This is where it can query for any required services and load any 
        /// non-graphic related content.  Calling base.Initialize will enumerate through
        /// any components and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //This will call initialize on all the game components
            base.Initialize();

            // create initial high scores
            HighScores = new List<int>();
            for (int i = 0; i < 5; i++)
            {
                HighScores.Add(500 - i * 100);
            }

            // try to load the real ones...
            // now that the GamerServicesComponent has initialize,
            // we can show the Guide to get the storage device for the high scores
            LoadHighScores();
        }

        /// <summary>
        /// Load the high scores from the drive.
        /// </summary>
        private static void LoadHighScores()
        {
            if ((MarbletsGame.StorageDevice != null) &&
                MarbletsGame.StorageDevice.IsConnected)
            {
                LoadHighScoresCallback(null);
            }
            else
            {
                /* TODO Guide.BeginShowStorageDeviceSelector(
                    new AsyncCallback(LoadHighScoresCallback), null); */
            }
        }

        /// <summary>
        /// Callback method for loading the high scores from the drive.
        /// </summary>
        private static void LoadHighScoresCallback(IAsyncResult result)
        {
            if ((result != null) && result.IsCompleted)
            {
                // TODO MarbletsGame.StorageDevice = Guide.EndShowStorageDeviceSelector(result);
            }

            if ((MarbletsGame.StorageDevice != null) &&
                MarbletsGame.StorageDevice.IsConnected)
            {
                using (StorageContainer storageContainer =
                    MarbletsGame.StorageDevice.OpenContainer("Marblets"))
                {
                    string highscoresPath = Path.Combine(storageContainer.Path,
                        "highscores.xml");

                    if (File.Exists(highscoresPath))
                    {
                        using (FileStream file =
                            File.Open(highscoresPath, FileMode.Open))
                        {
                            XmlSerializer serializer =
                                new XmlSerializer(typeof(List<int>));
                            HighScores = (List<int>)serializer.Deserialize(file);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            IGraphicsDeviceService graphicsService =
                (IGraphicsDeviceService)Services.GetService(
                typeof(IGraphicsDeviceService));

            //Ask static helper objects to reload too
            Font.LoadContent();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            touchCollection = TouchPanel.GetState();
            accelerometerState = Accelerometer.GetState();

            // Use X axis to determine if the Zune is being viewed from the
            // left or right side.
            // Leave fixed for now, need to rethink recalculating marble
            // positions when screen is rotated.
            
            if (accelerometerState.Acceleration.X >= 0.0 &&
                accelerometerState.Acceleration.X < 1.0)
            {
                screenOrientation = ScreenOrientation.LandscapeRight;
                screenRotation = 4.72f;
            }
            else
            {
                screenOrientation = ScreenOrientation.LandscapeLeft;
                screenRotation = 1.57f;
            }

            if (screenOrientation != priorScreenOrientation)
            {
                if (GameState == GameState.Play2D)
                    mainGame.RecalculateMarblePositions();
    
                priorScreenOrientation = screenOrientation;
            }

            if (NextGameState != GameState.None)
            {
                ChangeGameState();
            }
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);
            
            //Nothing to draw the components will handle it
            base.Draw(gameTime);
        }

        private void ChangeGameState()
        {
            //Marblets only has 3 game states - the splash screen and the game screen
            //in 2d and 3d Since they are both game components of type screen just 
            //making the right one visible will cause the correct background to be shown
            //and the right music to be played
            if ((GameState == GameState.Started) && (NextGameState == GameState.Play2D))
            {
                Score = 0;
                splashScreen.Enabled = false;
                splashScreen.Visible = false;
                mainGame.Enabled = true;
                mainGame.Visible = true;
                //Start a new game - reset score and board etc
                mainGame.NewGame();
                GameState = NextGameState;
            }
            else if (NextGameState == GameState.Started)
            {
                splashScreen.Enabled = true;
                splashScreen.Visible = true;
                mainGame.Enabled = false;
                mainGame.Visible = false;

                GameState = NextGameState;
            }
        }

        /* TODO protected override void OnExiting(object sender, EventArgs args)
        {
            splashScreen.Shutdown();
            mainGame.Shutdown();

            base.OnExiting(sender, args);
             }*/
    }

    /// <summary>
    /// This enum is for the state transitions.
    /// </summary>
    public enum GameState
    {
        /// <summary>
        /// Default value - means no state is set
        /// </summary>
        None,

        /// <summary>
        /// Nothing visible, game has just been run and nothing is initialized
        /// </summary>
        Started,

        /// <summary>
        /// Logo Screen is being displayed
        /// </summary>
        LogoSplash,

        /// <summary>
        /// Currently playing the 2d version
        /// </summary>
        Play2D,
    }
}
