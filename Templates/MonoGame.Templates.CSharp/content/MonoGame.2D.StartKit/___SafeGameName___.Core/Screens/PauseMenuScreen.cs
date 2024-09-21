#region File Description
//-----------------------------------------------------------------------------
// PauseMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using ___SafeGameName___.Core;
using ___SafeGameName___.Core.Localization;
using Microsoft.Xna.Framework;
#endregion

namespace ___SafeGameName___.Screens;

/// <summary>
/// The pause menu comes up over the top of the game,
/// giving the player options to resume or quit.
/// </summary>
class PauseMenuScreen : MenuScreen
{
    #region Initialization


    /// <summary>
    /// Constructor.
    /// </summary>
    public PauseMenuScreen()
        : base(Resources.Paused)
    {
        // Create our menu entries.
        MenuEntry resumeGameMenuEntry = new MenuEntry(Resources.Resume);
        MenuEntry quitGameMenuEntry = new MenuEntry(Resources.Quit);
        
        // Hook up menu event handlers.
        resumeGameMenuEntry.Selected += OnCancel;
        quitGameMenuEntry.Selected += QuitGameMenuEntrySelected;

        // Add entries to the menu.
        MenuEntries.Add(resumeGameMenuEntry);
        MenuEntries.Add(quitGameMenuEntry);
    }


    #endregion

    #region Handle Input


    /// <summary>
    /// Event handler for when the Quit Game menu entry is selected.
    /// </summary>
    void QuitGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
    {
        string message = Resources.QuitQuestion;

        MessageBoxScreen confirmQuitMessageBox = new MessageBoxScreen(message);

        confirmQuitMessageBox.Accepted += ConfirmQuitMessageBoxAccepted;

        ScreenManager.AddScreen(confirmQuitMessageBox, ControllingPlayer);
    }


    /// <summary>
    /// Event handler for when the user selects ok on the "are you sure
    /// you want to quit" message box. This uses the loading screen to
    /// transition from the game back to the main menu screen.
    /// </summary>
    void ConfirmQuitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
    {
        LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(),
                                                       new MainMenuScreen());
    }


    #endregion
}
