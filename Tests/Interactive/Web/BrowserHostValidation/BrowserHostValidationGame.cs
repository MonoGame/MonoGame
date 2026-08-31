// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Runtime.Versioning;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BrowserHostValidation;

[SupportedOSPlatform("browser")]
internal sealed class BrowserHostValidationGame : Game
{
    private const int ExitAfterDrawCount = 300;
    private const string ValidationTextureAssetName = "Content/monogame_logo.png";
    private const int ValidationTextureMaxWidth = 384;
    private const int ValidationTextureMaxHeight = 224;
    private const int ValidationTextureSpacing = 48;

    private int _drawCount;
    private bool _reportedFirstUpdate;
    private bool _reportedFirstDraw;
    private bool _reportedFirstTexturedDraw;
    private SpriteBatch? _spriteBatch;
    private Texture2D? _validationTextureFromFile;
    private Texture2D? _validationTextureFromStream;

    public BrowserHostValidationGame()
    {
        GraphicsDeviceManager graphicsDeviceManager = new GraphicsDeviceManager(this);
        graphicsDeviceManager.PreferredBackBufferWidth = 1280;
        graphicsDeviceManager.PreferredBackBufferHeight = 720;

        Content.RootDirectory = "Content";
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
        _validationTextureFromFile = Texture2D.FromFile(GraphicsDevice, ValidationTextureAssetName, DefaultColorProcessors.PremultiplyAlpha);

        using (Stream textureStream = TitleContainer.OpenStream(ValidationTextureAssetName))
        {
            _validationTextureFromStream = Texture2D.FromStream(GraphicsDevice, textureStream, DefaultColorProcessors.PremultiplyAlpha);
        }

        ValidateTextureEquality(ValidationTextureAssetName);

        BrowserHostValidationReporter.ReportPhase(
            "fromFileLoaded",
            "Texture2D.FromFile loaded Content/monogame_logo.png through the browser title container path.");

        BrowserHostValidationReporter.ReportPhase(
            "fromStreamLoaded",
            "Texture2D.FromStream loaded Content/monogame_logo.png from a TitleContainer stream.");

        BrowserHostValidationReporter.ReportPhase(
            "textureDataVerified",
            "Texture2D.FromFile and Texture2D.FromStream produced identical texture data.");

        BrowserHostValidationReporter.ReportPhase(
            "loadContent",
            "LoadContent() reached after GraphicsDevice creation and validation texture loads.");
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

        if (_spriteBatch != null
            && _validationTextureFromFile != null
            && _validationTextureFromStream != null)
        {
            int totalWidth = (ValidationTextureMaxWidth * 2) + ValidationTextureSpacing;
            int startX = (GraphicsDevice.Viewport.Width - totalWidth) / 2;
            int topY = (GraphicsDevice.Viewport.Height - ValidationTextureMaxHeight) / 2;

            Rectangle fromFileRectangle = CreateDestinationRectangle(
                _validationTextureFromFile,
                startX,
                topY);
            Rectangle fromStreamRectangle = CreateDestinationRectangle(
                _validationTextureFromStream,
                startX + ValidationTextureMaxWidth + ValidationTextureSpacing,
                topY);

            _spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
            _spriteBatch.Draw(_validationTextureFromFile, fromFileRectangle, Color.White);
            _spriteBatch.Draw(_validationTextureFromStream, fromStreamRectangle, Color.White);
            _spriteBatch.End();

            if (!_reportedFirstTexturedDraw)
            {
                _reportedFirstTexturedDraw = true;
                BrowserHostValidationReporter.ReportPhase(
                    "firstTexturedDraw",
                    "First SpriteBatch textured draw reached with Texture2D.FromFile and Texture2D.FromStream assets.");
            }
        }

        _drawCount++;
        if (_drawCount == ExitAfterDrawCount)
        {
            BrowserHostValidationReporter.ReportPhase(
                "visualValidationComplete",
                "Rendered CornflowerBlue frames and drew Texture2D.FromFile and Texture2D.FromStream assets for visual validation. Requesting Exit().");
            Exit();
        }

        base.Draw(gameTime);
    }

    protected override void UnloadContent()
    {
        if (_validationTextureFromFile != null)
        {
            _validationTextureFromFile.Dispose();
            _validationTextureFromFile = null;
        }

        if (_validationTextureFromStream != null)
        {
            _validationTextureFromStream.Dispose();
            _validationTextureFromStream = null;
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

    private static Rectangle CreateDestinationRectangle(Texture2D texture, int left, int top)
    {
        float widthScale = (float)ValidationTextureMaxWidth / texture.Width;
        float heightScale = (float)ValidationTextureMaxHeight / texture.Height;
        float scale = MathF.Min(widthScale, heightScale);

        int scaledWidth = Math.Max(1, (int)MathF.Round(texture.Width * scale));
        int scaledHeight = Math.Max(1, (int)MathF.Round(texture.Height * scale));
        int centeredLeft = left + ((ValidationTextureMaxWidth - scaledWidth) / 2);
        int centeredTop = top + ((ValidationTextureMaxHeight - scaledHeight) / 2);

        return new Rectangle(centeredLeft, centeredTop, scaledWidth, scaledHeight);
    }

    private static void ValidateTextureEquality(string assetName)
    {
        using Stream fromFileStream = TitleContainer.OpenStream(assetName);
        using Stream fromStreamStream = TitleContainer.OpenStream(assetName);
        DecodedImage fromFileImage = BrowserHostValidationImageDecoder.DecodeRgba(
            fromFileStream,
            DefaultColorProcessors.PremultiplyAlpha);
        DecodedImage fromStreamImage = BrowserHostValidationImageDecoder.DecodeRgba(
            fromStreamStream,
            DefaultColorProcessors.PremultiplyAlpha);

        if (fromFileImage.Width != fromStreamImage.Width
            || fromFileImage.Height != fromStreamImage.Height)
        {
            throw new InvalidOperationException(
                $"Decoded image dimensions differ. FromFile={fromFileImage.Width}x{fromFileImage.Height}, "
                + $"FromStream={fromStreamImage.Width}x{fromStreamImage.Height}.");
        }

        for (int index = 0; index < fromFileImage.RgbaBytes.Length; index++)
        {
            if (fromFileImage.RgbaBytes[index] != fromStreamImage.RgbaBytes[index])
            {
                throw new InvalidOperationException(
                    $"Decoded image data differs at byte index {index}. "
                    + $"FromFile={fromFileImage.RgbaBytes[index]}, FromStream={fromStreamImage.RgbaBytes[index]}.");
            }
        }
    }
}
