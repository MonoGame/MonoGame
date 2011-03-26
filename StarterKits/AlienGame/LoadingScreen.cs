//-----------------------------------------------------------------------------
// LoadingScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;
using System.Threading;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;

namespace AlienGameSample
{
    /// <summary>
    /// This just loads all content we're going to use on a background thread.  It doesn't
    /// draw anything because it's supposed to be invoked in front of the background screen and right
    /// before the main menu (i.e. noone will notice).  This saves us from spinning up the drive later
    /// and stalling between menu and gameplay.
    /// </summary>
    class LoadingScreen : GameScreen
    {
        private Thread backgroundThread;
 
        /// <summary>
        /// The constructor is private: loading screens should
        /// be activated via the static Load method instead.
        /// </summary>
        public LoadingScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.0);
            TransitionOffTime = TimeSpan.FromSeconds(0.0);
       }

        public override void LoadContent()
        {
            // Create the thread here (instead of the ctor), so we can guarantee that loading content will work
            if (backgroundThread == null)
            {
                backgroundThread = new Thread(BackgroundLoadContent);
                backgroundThread.Start();
            }

            base.LoadContent();
        }

        void BackgroundLoadContent()
        {
            // First thing we need to do is get a valid storage device for loading and saving
            // high scores.  Since this is a background thread, we can block on it; rendering the guide
            // for Xbox 360 will happen on the main thread.
            StorageDevice device = StorageDevice.ShowStorageDeviceGuide();
            ScreenManager.Game.Services.AddService(typeof(StorageDevice), device);

            /*ScreenManager.Game.Content.Load<object>("Alien_Hit");
            ScreenManager.Game.Content.Load<object>("alien1");
            ScreenManager.Game.Content.Load<object>("background");
            ScreenManager.Game.Content.Load<object>("badguy_blue");
            ScreenManager.Game.Content.Load<object>("badguy_green");
            ScreenManager.Game.Content.Load<object>("badguy_orange");
            ScreenManager.Game.Content.Load<object>("badguy_red");
            ScreenManager.Game.Content.Load<object>("bullet");
            ScreenManager.Game.Content.Load<object>("cloud1");
            ScreenManager.Game.Content.Load<object>("cloud2");
            ScreenManager.Game.Content.Load<object>("fire");
            ScreenManager.Game.Content.Load<object>("gamefont");
            ScreenManager.Game.Content.Load<object>("ground");
            ScreenManager.Game.Content.Load<object>("hills");
            ScreenManager.Game.Content.Load<object>("laser");
            ScreenManager.Game.Content.Load<object>("menufont");
            ScreenManager.Game.Content.Load<object>("moon");
            ScreenManager.Game.Content.Load<object>("mountains_blurred");
            ScreenManager.Game.Content.Load<object>("Player_Hit");
            ScreenManager.Game.Content.Load<object>("scorefont");
            ScreenManager.Game.Content.Load<object>("smoke");
            ScreenManager.Game.Content.Load<object>("sun");
            ScreenManager.Game.Content.Load<object>("tank");
            ScreenManager.Game.Content.Load<object>("tank_fire");
            ScreenManager.Game.Content.Load<object>("tank_tire");
            ScreenManager.Game.Content.Load<object>("tank_top");            
            ScreenManager.Game.Content.Load<object>("title");
            ScreenManager.Game.Content.Load<object>("titlefont");*/
        }

        /// <summary>
        /// Updates the loading screen.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            // See if the loading has completed...
            if (backgroundThread != null && backgroundThread.Join(10))
            {
                backgroundThread = null;
                this.ExitScreen();
                ScreenManager.AddScreen(new MainMenuScreen());
                ScreenManager.Game.ResetElapsedTime();
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }
    }
}
