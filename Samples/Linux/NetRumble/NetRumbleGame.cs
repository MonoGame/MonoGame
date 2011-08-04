#region File Description
//-----------------------------------------------------------------------------
// NetRumbleGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace NetRumble
{
    /// <summary>
    /// While this used to be an enum in the XNA Framework API, it was removed in version 4.0.
    /// However, it is still useful as a parameter to specify what kind of particles to draw,
    /// so it is revitalized here as a global enum.
    /// </summary>
    public enum SpriteBlendMode
    {
        Additive,
        AlphaBlend
    }

    /// <summary>
    /// The main type for this game.
    /// </summary>
    public class NetRumbleGame : Microsoft.Xna.Framework.Game
    {
        #region Graphics Data


        /// <summary>
        /// The graphics device manager used to render the game.
        /// </summary>
        GraphicsDeviceManager graphics;


        #endregion


        #region Game State Management Data


        /// <summary>
        /// The manager for all of the user-interface data.
        /// </summary>
        ScreenManager screenManager;


        #endregion


        #region Initialization Methods


        /// <summary>
        /// Constructs a new NetRumbleGame object.
        /// </summary>
        public NetRumbleGame()
        {
            // initialize the graphics device manager
            graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;

            // initialize the content manager
            Content.RootDirectory = "Content";

            // initialize the gamer-services component
            //   this component enables Live sign-in functionality
            //   and updates the Gamer.SignedInGamers collection.
            Components.Add(new GamerServicesComponent(this));

            // initialize the screen manager
            screenManager = new ScreenManager(this);
            Components.Add(screenManager);

            // initialize the audio system
            //AudioManager.Initialize(this, new DirectoryInfo(Content.RootDirectory + @"\audio\wav"));
            AudioManager.Initialize(this, new DirectoryInfo(Path.Combine(Content.RootDirectory,@"Audio/wav")));

        }


        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting.
        /// This is where it can query for any required services and load any
        /// non-graphic related content.  Calling base.Initialize will enumerate through
        /// any components and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // Useful to turn on SimulateTrialMode here if you want to test launching the game
            // from an invite and have it start in trial mode.
//#if DEBUG
//            Guide.SimulateTrialMode = true;
//#endif

            // load the initial screens
            screenManager.AddScreen(new BackgroundScreen());
            screenManager.AddScreen(new MainMenuScreen());

            // hookup the invite event-processing function
            NetworkSession.InviteAccepted += new EventHandler<InviteAcceptedEventArgs>(NetworkSession_InviteAccepted);
        }

        /// <summary>
        /// Begins the asynchronous process of joining a game from an invitation.
        /// </summary>
        void NetworkSession_InviteAccepted(object sender, InviteAcceptedEventArgs e)
        {
            if (Guide.IsTrialMode)
            {
                screenManager.invited = e.Gamer;

                string message = "Need to unlock full version before you can accept this invite.";
                MessageBoxScreen messageBox = new MessageBoxScreen(message);
                screenManager.AddScreen(messageBox);

                System.Console.WriteLine("Cannot accept invite yet because we're in trial mode");

                return;
            }

            // We will join the game from a method in this screen.
            MainMenuScreen mainMenu = null;

            // Keep the background screen and main menu screen but remove all others
            // to prepare for joining the game we were invited to.
            foreach (GameScreen screen in screenManager.GetScreens())
            {
                if (screen is BackgroundScreen)
                {
                    continue;
                }
                else if (screen is MainMenuScreen)
                {
                    mainMenu = screen as MainMenuScreen;
                }
                else
                {
                    // If there's an active network session, we'll need to end it
                    // before attempting to join a new one.
                    MethodInfo method = screen.GetType().GetMethod("EndSession");
                    if (method != null)
                    {
                        method.Invoke(screen, null);
                    }

                    // Now exit and remove this screen.
                    screen.ExitScreen();
                    screenManager.RemoveScreen(screen);
                }
            }

            // Now attempt to join the game to which we were invited!
            if (mainMenu != null)
                mainMenu.JoinInvitedGame();
        }


        #endregion


        #region Drawing Methods


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);
        }


        #endregion


        #region Entry Point


//        /// <summary>
//        /// The main entry point for the application.
//        /// </summary>
//        static class Program
//        {
//            static void Main()
//            {
//                using (NetRumbleGame game = new NetRumbleGame())
//                {
//                    game.Run();
//                }
//            }
//        }


        #endregion

    }
}
