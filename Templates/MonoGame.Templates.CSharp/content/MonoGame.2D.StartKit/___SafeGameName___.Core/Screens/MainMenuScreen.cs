#region File Description
//-----------------------------------------------------------------------------
// MainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using ___SafeGameName___.Core;
using ___SafeGameName___.Core.Inputs;
using ___SafeGameName___.Core.Localization;
using ___SafeGameName___.ScreenManagers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;
using System.Reflection.Emit;
#endregion

namespace ___SafeGameName___.Screens;

/// <summary>
/// The main menu screen is the first thing displayed when the game starts up.
/// </summary>
class MainMenuScreen : MenuScreen
{
    #region Fields
    private ContentManager content;
    private Level level;
    private bool readyToPlay;
    private PlayerIndex playerIndex;
    #endregion

    #region Initialization


    /// <summary>
    /// Constructor fills in the menu contents.
    /// </summary>
    public MainMenuScreen()
        : base(Resources.MainMenu)
    {
        // Create our menu entries.
        MenuEntry aboutMenuEntry = new MenuEntry(Resources.About);
        MenuEntry playMenuEntry = new MenuEntry(Resources.Play);
        MenuEntry optionsMenuEntry = new MenuEntry(Resources.Options);
        MenuEntry exitMenuEntry = new MenuEntry(Resources.Exit);

        // Hook up menu event handlers.
        playMenuEntry.Selected += PlayMenuEntrySelected;
        optionsMenuEntry.Selected += OptionsMenuEntrySelected;
        aboutMenuEntry.Selected += AboutMenuEntrySelected;
        exitMenuEntry.Selected += OnCancel;

        // Add entries to the menu.
        MenuEntries.Add(playMenuEntry);
        MenuEntries.Add(optionsMenuEntry);
        MenuEntries.Add(aboutMenuEntry);
        MenuEntries.Add(exitMenuEntry);
    }

    /// <summary>
    /// LoadContent will be called once per game and is the place to load
    /// all of your content for the game.
    /// </summary>
    public override void LoadContent()
    {
        if (content == null)
            content = new ContentManager(ScreenManager.Game.Services, "Content");

        // Load the level.
        string levelPath = "Content/Levels/00.txt";
        using (Stream fileStream = TitleContainer.OpenStream(levelPath))
            level = new Level(ScreenManager.Game.Services, fileStream, 00);
    }

    /// <summary>
    /// Unload graphics content used by the game.
    /// </summary>
    public override void UnloadContent()
    {
        if (level != null)
        {
            level.Dispose();
        }

        content.Unload();
    }
    #endregion

    #region Update and Draw
    /// <summary>
    /// Allows the game to run logic such as updating the world,
    /// checking for collisions, gathering input, and playing audio.
    ///
    /// This method checks the GameScreen.IsActive
    /// property, so the game will stop updating when the pause menu is active,
    /// or if you tab away to a different application.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    /// <param name="otherScreenHasFocus">If another screen has focus</param>
    /// <param name="coveredByOtherScreen">If currently covered by another screen</param>
    public override void Update(GameTime gameTime,
        bool otherScreenHasFocus,
        bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        if (readyToPlay)
        {
            if (!level.ReachedExit)
            {
                level.Player.Movement = 1.0f;

                // Maybe get it to jump after moving??
                level.Player.Move(gameTime);
            }
            else
            {
                LoadingScreen.Load(ScreenManager,
                    true,
                    playerIndex,
                    new GameplayScreen());
            }
        }

        // update our level, passing down the GameTime along with all of our input states
        level.Update(gameTime,
            Keyboard.GetState(),
            GamePad.GetState(PlayerIndex.One),
            Accelerometer.GetState(),
            ScreenManager.Game.Window.CurrentOrientation,
            readyToPlay);
    }

    /// <summary>
    /// Draws the gameplay screen.
    /// </summary>
    public override void Draw(GameTime gameTime)
    {
        // Our player and enemy are both actually just text strings.
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

        spriteBatch.Begin();

        level.Draw(gameTime, spriteBatch);

        spriteBatch.End();

        base.Draw(gameTime);
    }
    #endregion

    #region Handle Input

    /// <summary>
    /// Event handler for when the Play menu entry is selected.
    /// </summary>
    void PlayMenuEntrySelected(object sender, PlayerIndexEventArgs e)
    {
        playerIndex = e.PlayerIndex;
        readyToPlay = true;
    }

    /// <summary>
    /// Event handler for when the Options menu entry is selected.
    /// </summary>
    void OptionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
    {
        ScreenManager.AddScreen(new SettingsScreen(), e.PlayerIndex);
    }

    /// <summary>
    /// Event handler for when the Options menu entry is selected.
    /// </summary>
    void AboutMenuEntrySelected(object sender, PlayerIndexEventArgs e)
    {
        ScreenManager.AddScreen(new AboutScreen(), e.PlayerIndex);
    }

    /// <summary>
    /// When the user cancels the main menu, ask if they want to exit the sample.
    /// </summary>
    protected override void OnCancel(PlayerIndex playerIndex)
    {
        string message = Resources.ExitQuestion;

        MessageBoxScreen confirmExitMessageBox = new MessageBoxScreen(message);

        confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;

        ScreenManager.AddScreen(confirmExitMessageBox, playerIndex);
    }


    /// <summary>
    /// Event handler for when the user selects ok on the "are you sure
    /// you want to exit" message box.
    /// </summary>
    void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
    {
        ScreenManager.Game.Exit();
    }

    #endregion
}
