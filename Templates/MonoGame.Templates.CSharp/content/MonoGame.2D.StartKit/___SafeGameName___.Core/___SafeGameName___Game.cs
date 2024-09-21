#region File Description
//-----------------------------------------------------------------------------
// ___SafeGameName___Game.cs
//
// MonoGame Foundation Game Platform
// Copyright (C) MonoGame Foundation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;

using Microsoft.Xna.Framework;
using ___SafeGameName___.ScreenManagers;
using ___SafeGameName___.Screens;
using System.IO.Pipes;
using System.Reflection.Emit;



#if !__IOS__
using Microsoft.Xna.Framework.Media;
#endif
#endregion

namespace ___SafeGameName___.Core;

/// <summary>
/// This is the main type for your game
/// </summary>
public class ___SafeGameName___Game : Game
{
    // Resources for drawing.
    private GraphicsDeviceManager graphics;

    ScreenManager screenManager;

    public ___SafeGameName___Game()
    {
        Content.RootDirectory = "Content";

        graphics = new GraphicsDeviceManager(this);

        graphics.IsFullScreen = false;

        //graphics.PreferredBackBufferWidth = 800;
        //graphics.PreferredBackBufferHeight = 480;
        graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;

        // Create the screen manager component.
        screenManager = new ScreenManager(this);

        Components.Add(screenManager);

        // Activate the first screens.
        screenManager.AddScreen(new BackgroundScreen(), null);
        screenManager.AddScreen(new MainMenuScreen(), null);
    }
}
