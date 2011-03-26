//-----------------------------------------------------------------------------
// AlienGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlienGameSample
{
    /// <summary>
    /// This is the main type for your game.  All of the logic for the game is
    /// handled inside of a GameScreen, so Game1 is just used to setup the 
    /// starting screens.
    /// </summary>
    public class AlienGame : Game
    {
        GraphicsDeviceManager graphics;
        ScreenManager screenManager;

        public AlienGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
           
            screenManager = new ScreenManager(this);
            Components.Add(screenManager);

			// Zune use 30 frames per second
			this.TargetElapsedTime = TimeSpan.FromSeconds(1/30.0f);
			
			// The assets need to be stretched...put some quality
			graphics.PreferMultiSampling = true;
			
            // Add the background screen
            screenManager.AddScreen(new BackgroundScreen());

            // This loading screen pre-loads all content the game needs.  It
            // doesn't draw anything, so the user sees the background screen
            // then the title and menus pop up.
            screenManager.AddScreen(new LoadingScreen());
        }
    }
}

