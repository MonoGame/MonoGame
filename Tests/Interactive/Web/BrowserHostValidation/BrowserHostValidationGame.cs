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
    private const int ValidationTextureSize = 2;
    private const int ValidationSpriteSize = 256;

    private static readonly Color[] ValidationTexturePixels =
    {
        Color.White,
        Color.Black,
        Color.Black,
        Color.White
    };

    private int _drawCount;
    private bool _reportedFirstUpdate;
    private bool _reportedFirstDraw;
    private bool _reportedFirstTexturedDraw;
    private SpriteBatch? _spriteBatch;
    private Texture2D? _validationTexture;

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
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _validationTexture = new Texture2D(GraphicsDevice, ValidationTextureSize, ValidationTextureSize);
        _validationTexture.SetData(ValidationTexturePixels);

        BrowserHostValidationReporter.ReportPhase(
            "loadContent",
            "LoadContent() reached after GraphicsDevice creation and validation texture upload.");
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

        if (_spriteBatch != null && _validationTexture != null)
        {
            Rectangle destinationRectangle = new Rectangle(
                (GraphicsDevice.Viewport.Width - ValidationSpriteSize) / 2,
                (GraphicsDevice.Viewport.Height - ValidationSpriteSize) / 2,
                ValidationSpriteSize,
                ValidationSpriteSize);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(_validationTexture, destinationRectangle, Color.White);
            _spriteBatch.End();

            if (!_reportedFirstTexturedDraw)
            {
                _reportedFirstTexturedDraw = true;
                BrowserHostValidationReporter.ReportPhase(
                    "firstTexturedDraw",
                    "First SpriteBatch textured draw reached with generated Texture2D.");
            }
        }

        _drawCount++;
        if (_drawCount == ExitAfterDrawCount)
        {
            BrowserHostValidationReporter.ReportPhase(
                "visualValidationComplete",
                "Rendered CornflowerBlue frames and generated Texture2D for visual validation. Requesting Exit().");
            Exit();
        }

        base.Draw(gameTime);
    }

    protected override void UnloadContent()
    {
        if (_validationTexture != null)
        {
            _validationTexture.Dispose();
            _validationTexture = null;
        }

        if (_spriteBatch != null)
        {
            _spriteBatch.Dispose();
            _spriteBatch = null;
        }

        base.UnloadContent();
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

