// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.Versioning;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BrowserHostValidation;

[SupportedOSPlatform("browser")]
internal sealed class BrowserHostValidationGame : Game
{
    private const int ExitAfterDrawCount = 300;

    private int _drawCount;
    private bool _reportedFirstUpdate;
    private bool _reportedFirstDraw;

    public BrowserHostValidationGame()
    {
        GraphicsDeviceManager graphicsDeviceManager = new GraphicsDeviceManager(this);
        graphicsDeviceManager.PreferredBackBufferWidth = 1280;
        graphicsDeviceManager.PreferredBackBufferHeight = 720;

        IsMouseVisible = true;
        Window.Title = "MonoGame.Web Browser Host Validation";
        Activated += OnActivated;
        Deactivated += OnDeactivated;
        Exiting += OnExiting;

        BrowserHostValidationReporter.ReportPhase(
            "constructor",
            "Game constructed and GraphicsDeviceManager registered.");
    }

    protected override void Initialize()
    {
        BrowserHostValidationReporter.ReportPhase(
            "initialize",
            "Initialize() reached before LoadContent().");
        base.Initialize();
    }

    protected override void LoadContent()
    {
        BrowserHostValidationReporter.ReportPhase(
            "loadContent",
            "LoadContent() reached after GraphicsDevice creation.");
    }

    protected override void Update(GameTime gameTime)
    {
        if (!_reportedFirstUpdate)
        {
            _reportedFirstUpdate = true;
            BrowserHostValidationReporter.ReportPhase(
                "firstUpdate",
                "First Update() reached.");
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        if (!_reportedFirstDraw)
        {
            _reportedFirstDraw = true;
            BrowserHostValidationReporter.ReportPhase(
                "firstDraw",
                "First Draw() reached. Clearing CornflowerBlue for visual validation.");
        }

        GraphicsDevice.Clear(Color.CornflowerBlue);

        _drawCount++;
        if (_drawCount == ExitAfterDrawCount)
        {
            BrowserHostValidationReporter.ReportPhase(
                "visualValidationComplete",
                "Rendered CornflowerBlue frames for visual validation. Requesting Exit().");
            Exit();
        }

        base.Draw(gameTime);
    }

    private void OnActivated(object sender, EventArgs eventArgs)
    {
        BrowserHostValidationReporter.ReportPhase(
            "activated",
            "The MonoGame browser platform reported focus gained.");
    }

    private void OnDeactivated(object sender, EventArgs eventArgs)
    {
        BrowserHostValidationReporter.ReportPhase(
            "deactivated",
            "The MonoGame browser platform reported focus lost.");
    }

    private void OnExiting(object sender, EventArgs eventArgs)
    {
        BrowserHostValidationReporter.ReportPhase(
            "exiting",
            "Exit was observed.");
    }
}

