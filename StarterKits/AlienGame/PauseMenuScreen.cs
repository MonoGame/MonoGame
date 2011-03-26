//-----------------------------------------------------------------------------
// PauseMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;

namespace AlienGameSample
{
    /// <summary>
    /// Primarily for controlling what song is playing.
    /// </summary>
    class PauseMenuScreen : MenuScreen
    {
        MenuEntry songMenuEntry;
        MenuEntry playMenuEntry;

        static int selectedSongIndex;
        static int playingSongIndex = -1;

        /// <summary>
        /// Constructor.
        /// </summary>
        public PauseMenuScreen()
            : base("Pause")
        {

            IsPopup = true;

            MenuEntry resumeGameMenuEntry = new MenuEntry("RESUME");
            MenuEntry quitGameMenuEntry = new MenuEntry("QUIT");
            resumeGameMenuEntry.Selected += OnCancel;
            quitGameMenuEntry.Selected += QuitGameMenuEntrySelected;

            // Create our menu entries.
            songMenuEntry = new MenuEntry("SONG:");
            playMenuEntry = new MenuEntry("PLAY");

            MenuEntry backMenuEntry = new MenuEntry("BACK");

            // Hook up menu event handlers.
            songMenuEntry.Selected += SongMenuEntrySelected;
            playMenuEntry.Selected += PlayMenuEntrySelected;
            backMenuEntry.Selected += OnCancel;
            
            // Add entries to the menu.
            MenuEntries.Add(resumeGameMenuEntry);
            MenuEntries.Add(songMenuEntry);
            MenuEntries.Add(playMenuEntry);
            MenuEntries.Add(quitGameMenuEntry);

            if (playingSongIndex != -1)
            {
                selectedSongIndex = playingSongIndex;
            }

            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        public override void LoadContent()
        {
            // Set the initial menu content (requires ScreenManager to have been set).
            UpdateMenuText();

            base.LoadContent();
        }

        private void UpdateMenuText()
        {
            /*if (ScreenManager.MediaLibrary != null && ScreenManager.MediaLibrary.Songs.Count > 0)
            {
                songMenuEntry.Text = "SONG: " + ScreenManager.MediaLibrary.Songs[selectedSongIndex] + (ScreenManager.MediaLibrary.Songs[selectedSongIndex].IsProtected ? " (DRM)" : "");
                if (MediaPlayer.State == MediaState.Playing)
                    playMenuEntry.Text = "STOP";
                else
                {
                    if (ScreenManager.MediaLibrary.Songs[selectedSongIndex].IsProtected == true)
                    {
                        playMenuEntry.Text = "-";
                    }
                    else
                    {
                        playMenuEntry.Text = "PLAY";
                    }
                }
            }
            else
            {*/
                songMenuEntry.Text = "(NO SONGS)";
                playMenuEntry.Text = "-";
            //}
        }

        void AdvanceSong()
        {
            /*if (ScreenManager.MediaLibrary == null || ScreenManager.MediaLibrary.Songs.Count == 0)
                return;

            selectedSongIndex = (selectedSongIndex + 1) % ScreenManager.MediaLibrary.Songs.Count;

            UpdateMenuText();*/
        }

        /// <summary>
        /// Event handler for when the Language menu entry is selected.
        /// </summary>
        void SongMenuEntrySelected(object sender, EventArgs e)
        {
            AdvanceSong();
        }

        void PlayMenuEntrySelected(object sender, EventArgs e)
        {
            /*if (ScreenManager.MediaLibrary == null || ScreenManager.MediaLibrary.Songs.Count == 0)
                return;

            if (MediaPlayer.State == MediaState.Playing)
            {
                MediaPlayer.Stop();
                playingSongIndex = -1;
            }
            else
            {
                if (ScreenManager.MediaLibrary.Songs[selectedSongIndex].IsProtected == false)
                {
                    MediaPlayer.Play(ScreenManager.MediaLibrary.Songs[selectedSongIndex]);
                    playingSongIndex = selectedSongIndex;
                }
            }

            UpdateMenuText();*/
        }

        /// <summary>
        /// Event handler for when the Quit Game menu entry is selected.
        /// </summary>
        void QuitGameMenuEntrySelected(object sender, EventArgs e)
        {
            foreach (GameScreen screen in ScreenManager.GetScreens())
                screen.ExitScreen();

            ScreenManager.AddScreen(new BackgroundScreen());
            ScreenManager.AddScreen(new MainMenuScreen());
        }

        /// <summary>
        /// Draws the pause menu screen. This darkens down the gameplay screen
        /// that is underneath us, and then chains to the base MenuScreen.Draw.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);

            base.Draw(gameTime);
        }

    }
}
