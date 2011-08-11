#region File Description
//-----------------------------------------------------------------------------
// PauseScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

#if ANDROID
using Android.App;
#endif

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using CardsFramework;
using GameStateManagement;
#endregion

namespace Blackjack
{
    /// <summary>
    /// This is the main game type.
    /// </summary>
    public class BlackjackGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        ScreenManager screenManager;

        public static float HeightScale = 1.0f;
        public static float WidthScale = 1.0f;

        /// <summary>
        /// Initializes a new instance of the game.
        /// </summary>
#if ANDROID 
		public BlackjackGame (Activity activity) : base (activity)
#else 
        public BlackjackGame ()  
#endif
        {
            graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";

            screenManager = new ScreenManager(this);

            screenManager.AddScreen(new BackgroundScreen(), null);
            screenManager.AddScreen(new MainMenuScreen(), null);

            Components.Add(screenManager);

#if WINDOWS || MACOS || LINUX
            IsMouseVisible = true;
#elif WINDOWS_PHONE || IOS || ANDROID
            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);
            graphics.IsFullScreen = true;
#else
            Components.Add(new GamerServicesComponent(this));
#endif

            // Initialize sound system
            AudioManager.Initialize(this);
        }

        protected override void Initialize()
        {
            base.Initialize();

#if XBOX
            graphics.PreferredBackBufferHeight = graphics.GraphicsDevice.DisplayMode.Height;
            graphics.PreferredBackBufferWidth = graphics.GraphicsDevice.DisplayMode.Width; 
#elif WINDOWS || MACOS || LINUX
            graphics.PreferredBackBufferHeight = 480;
            graphics.PreferredBackBufferWidth = 800; 
#endif         
            graphics.ApplyChanges();

            Rectangle bounds = graphics.GraphicsDevice.Viewport.TitleSafeArea;
            HeightScale = bounds.Height / 480f;
            WidthScale = bounds.Width / 800f;


        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            AudioManager.LoadSounds();

            base.LoadContent();
        }
    }
}
