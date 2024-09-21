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
using ___SafeGameName___.Core.Localization;
using ___SafeGameName___.ScreenManagers;
using Microsoft.Xna.Framework;
#endregion

namespace ___SafeGameName___.Screens;

/// <summary>
/// The main menu screen is the first thing displayed when the game starts up.
/// </summary>
class MainMenuScreen : MenuScreen
{
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


    #endregion

    #region Handle Input


    /// <summary>
    /// Event handler for when the Play Game menu entry is selected.
    /// </summary>
    void PlayMenuEntrySelected(object sender, PlayerIndexEventArgs e)
    {
        LoadingScreen.Load(ScreenManager, true, e.PlayerIndex,
                           new GameplayScreen());
    }


    /// <summary>
    /// Event handler for when the Options menu entry is selected.
    /// </summary>
    void OptionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
    {
        ScreenManager.AddScreen(new OptionsMenuScreen(), e.PlayerIndex);
    }

    /// <summary>
    /// Event handler for when the Options menu entry is selected.
    /// </summary>
    void AboutMenuEntrySelected(object sender, PlayerIndexEventArgs e)
    {
        //ScreenManager.AddScreen(new AboutMenuScreen(), e.PlayerIndex);
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
