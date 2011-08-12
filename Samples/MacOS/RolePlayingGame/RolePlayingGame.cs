#region File Description
//-----------------------------------------------------------------------------
// RolePlayingGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using RolePlayingGameData;
using Microsoft.Xna.Framework.GamerServices;
#endregion

namespace RolePlaying
{
    /// <summary>
    /// The Game object for the Role-Playing Game starter kit.
    /// </summary>
    public class RolePlayingGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        ScreenManager screenManager;

        /// <summary>
        /// Create a new RolePlayingGame object.
        /// </summary>
        public RolePlayingGame()
        {
            // initialize the graphics system
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            
            // configure the content manager
            Content.RootDirectory = "Content";

            // add a gamer-services component, which is required for the storage APIs
            Components.Add(new GamerServicesComponent(this));

            // add the audio manager
            AudioManager.Initialize(this, @"Content\Audio\RpgAudio.xgs", 
                @"Content\Audio\Wave Bank.xwb", @"Content\Audio\Sound Bank.xsb");

            // add the screen manager
            screenManager = new ScreenManager(this);
            Components.Add(screenManager);
        }


        /// <summary>
        /// Allows the game to perform any initialization it needs to 
        /// before starting to run.  This is where it can query for any required 
        /// services and load any non-graphic related content.  Calling base.Initialize 
        /// will enumerate through any components and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            InputManager.Initialize();

            base.Initialize();

            TileEngine.Viewport = graphics.GraphicsDevice.Viewport;

            screenManager.AddScreen(new MainMenuScreen());
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Fonts.LoadContent(Content);

            base.LoadContent();
        }


        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            Fonts.UnloadContent();

            base.UnloadContent();
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            InputManager.Update();

            base.Update(gameTime);
        }


        #region Drawing


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Transparent);

            base.Draw(gameTime);
        }


        #endregion


        #region Entry Point


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
//        static void Main(string[] args)
//        {
//            using (RolePlayingGame game = new RolePlayingGame())
//            {
//                game.Run();
//            }
//        }


        #endregion
    }
}
