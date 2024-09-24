#region File Description
//-----------------------------------------------------------------------------
// OptionsMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using ___SafeGameName___.Core;
using ___SafeGameName___.Core.Localization;
using Microsoft.Xna.Framework;
using System;
#endregion

namespace ___SafeGameName___.Screens;

/// <summary>
/// The options screen is brought up over the top of the main menu
/// screen, and gives the user a chance to configure the game
/// in various hopefully useful ways.
/// </summary>
class AboutScreen : MenuScreen
{
    #region Fields

    MenuEntry builtWithMonoGameMenuEntry;
    MenuEntry monoGameWebsiteMenuEntry;

    #endregion

    #region Initialization


    /// <summary>
    /// Constructor.
    /// </summary>
    public AboutScreen()
        : base(Resources.About)
    {
        // Create our menu entries.
        builtWithMonoGameMenuEntry = new MenuEntry("#BuiltWithMonoGame", false);
        monoGameWebsiteMenuEntry = new MenuEntry("MonoGame Site");
        MenuEntry back = new MenuEntry(Resources.Back);

        // Hook up menu event handlers.
        monoGameWebsiteMenuEntry.Selected += MonoGameWebsiteMenuSelected;
        back.Selected += OnCancel;
        
        // Add entries to the menu.
        MenuEntries.Add(builtWithMonoGameMenuEntry);
        MenuEntries.Add(monoGameWebsiteMenuEntry);
        MenuEntries.Add(back);
    }

    #endregion

    #region Handle Input

    /// <summary>
    /// Event handler for when the MonoGame Website menu entry is selected.
    /// </summary>
    void MonoGameWebsiteMenuSelected(object sender, PlayerIndexEventArgs e)
    {
        // Launch defaut browser to the MonoGame website
        // More tweaks required to make this work on all platforms :)
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://www.monogame.net/") { UseShellExecute = true });
    }

    #endregion
}
