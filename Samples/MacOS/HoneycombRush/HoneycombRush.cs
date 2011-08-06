#region File Description
//-----------------------------------------------------------------------------
// HoneycombRush.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using Microsoft.Xna.Framework;
using HoneycombRush.GameDebugTools;
using Microsoft.Xna.Framework.GamerServices;


#endregion

namespace HoneycombRush
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class HoneycombRush : Game
    {
        #region Fields


        GraphicsDeviceManager graphics;
        ScreenManager screenManager;

        public static string GameName = "Honeycomb Rush";

        DebugSystem debugSystem;


        #endregion

        #region Initialization


        public HoneycombRush()
        {
            // Initialize sound system
            AudioManager.Initialize(this);

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

#if WINDOWS_PHONE
            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            graphics.IsFullScreen = true;

            screenManager = new ScreenManager(this, Vector2.One);
#elif WINDOWS || MAC
            graphics.PreferredBackBufferHeight = 480;
            graphics.PreferredBackBufferWidth = 800;            

            // Make the game windowed
            graphics.IsFullScreen = false;
            IsMouseVisible = true;

            Components.Add(new GamerServicesComponent(this));

            Vector2 scaleVector = new Vector2(graphics.PreferredBackBufferWidth / 1280f, 
                graphics.PreferredBackBufferHeight / 720f);

            UIConstants.SetScale(scaleVector);

            // Create a new instance of the Screen Manager. Have all drawing scaled from 720p to the PC's resolution
            screenManager = new ScreenManager(this, scaleVector);
#endif

            screenManager.AddScreen(new BackgroundScreen("titleScreen"), null);
            screenManager.AddScreen(new MainMenuScreen(), PlayerIndex.One);
            Components.Add(screenManager);
        }

        protected override void Initialize()
        {
            // Initialize the debug system with the game and the name of the font 
            // we want to use for the debugging
            debugSystem = DebugSystem.Initialize(this, @"Fonts\GameScreenFont16px");

            base.Initialize();
        }


        #endregion

        #region Update and Draw


        protected override void Update(GameTime gameTime)
        {
            // Tell the TimeRuler that we're starting a new frame. you always want
            // to call this at the start of Update
            debugSystem.TimeRuler.StartFrame();

            // Start measuring time for "Update".
            debugSystem.TimeRuler.BeginMark("Update", Color.Blue);

            base.Update(gameTime);

            // Stop measuring time for "Update".
            debugSystem.TimeRuler.EndMark("Update");
        }

        protected override void Draw(GameTime gameTime)
        {
            // Start measuring time for "Draw".
            debugSystem.TimeRuler.BeginMark("Draw", Color.Yellow);

            base.Draw(gameTime);

            // Stop measuring time for "Draw".
            debugSystem.TimeRuler.EndMark("Draw");
        }


        #endregion
    }
}
