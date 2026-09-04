// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Runtime.Versioning;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;

namespace BrowserHostValidation;

[SupportedOSPlatform("browser")]
internal sealed class BrowserHostValidationGame : Game
{
    private const int ExitAfterDrawCount = 300;
    private const string ValidationEffectContentAssetName = "validation_effect";
    private const string ValidationEffectContentStreamAssetName = "Content/validation_effect.xnb";
    private const string ValidationFontContentAssetName = "arial";
    private const string ValidationFontContentStreamAssetName = "Content/arial.xnb";
    private const string ValidationFontSampleText = "SpriteFont content pipeline validation";
    private const string ValidationMissingAssetPath = "Content/missing-validation-raw.txt";
    private const string ValidationRawAssetContents = "MonoGame.Web raw-file validation.";
    private const string ValidationRawAssetPath = "Content/validation-raw.txt";
    private const string ValidationTextureContentAssetName = "monogame_logo";
    private const string ValidationTextureContentStreamAssetName = "Content/monogame_logo.xnb";
    private const int ValidationTextureMaxWidth = 384;
    private const int ValidationTextureMaxHeight = 224;

    private int _drawCount;
    private bool _reportedFirstDraw;
    private bool _reportedFirstValidationDraw;
    private Effect? _validationEffectFromContent;
    private SpriteBatch? _spriteBatch;
    private SpriteFont? _validationFontFromContent;
    private Texture2D? _validationTextureFromContent;

    public BrowserHostValidationGame()
    {
        GraphicsDeviceManager graphicsDeviceManager = new GraphicsDeviceManager(this);
        graphicsDeviceManager.PreferredBackBufferWidth = 1280;
        graphicsDeviceManager.PreferredBackBufferHeight = 720;

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.Title = "MonoGame.Web Browser Host Validation";
        Exiting += OnExiting;

        BrowserHostValidationReporter.ReportPhase(
            "constructor",
            "Game constructed and GraphicsDeviceManager registered.");
    }

    protected override void Initialize()
    {
        ValidatePlatformInfo();

        BrowserHostValidationReporter.ReportPhase(
            "initialize",
            "Initialize() reached.");
        base.Initialize();
    }

    protected override void LoadContent()
    {
        ValidateRawFile();
        ValidateRawFileReopen();
        ValidateMissingAsset();

        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _validationTextureFromContent = LoadValidatedContent<Texture2D>(
            ValidationTextureContentAssetName,
            ValidationTextureContentStreamAssetName);
        _validationFontFromContent = LoadValidatedContent<SpriteFont>(
            ValidationFontContentAssetName,
            ValidationFontContentStreamAssetName);
        _validationEffectFromContent = LoadValidatedContent<Effect>(
            ValidationEffectContentAssetName,
            ValidationEffectContentStreamAssetName);
        ValidateTextureDimensions();
        ValidateFontMeasurement();
        ValidateEffect();

        BrowserHostValidationReporter.ReportPhase(
            "contentValidated",
            "Validated Content.Load<Texture2D>, Content.Load<SpriteFont>, and Content.Load<Effect> through the browser content pipeline path.");
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

        if (IsValidationReady())
        {
            int startX = (GraphicsDevice.Viewport.Width - ValidationTextureMaxWidth) / 2;
            int topY = (GraphicsDevice.Viewport.Height - ValidationTextureMaxHeight) / 2;

            Rectangle fromContentRectangle = CreateDestinationRectangle(
                _validationTextureFromContent!,
                startX,
                topY);

            _spriteBatch!.Begin(samplerState: SamplerState.LinearClamp, effect: _validationEffectFromContent);
            _spriteBatch.Draw(_validationTextureFromContent!, fromContentRectangle, Color.White);
            Vector2 sampleTextSize = _validationFontFromContent!.MeasureString(ValidationFontSampleText);
            Vector2 sampleTextPosition = new Vector2(
                (GraphicsDevice.Viewport.Width - sampleTextSize.X) * 0.5f,
                topY + ValidationTextureMaxHeight + 32.0f);
            _spriteBatch.DrawString(
                _validationFontFromContent,
                ValidationFontSampleText,
                sampleTextPosition,
                Color.White);
            _spriteBatch.End();

            ReportFirstDrawValidation();
        }

        _drawCount++;
        if (_drawCount == ExitAfterDrawCount)
        {
            BrowserHostValidationReporter.ReportPhase(
                "visualValidationComplete",
                "Rendered the validation texture and text through Content.Load<Texture2D>, Content.Load<SpriteFont>, and Content.Load<Effect>. Requesting Exit().");
            Exit();
        }

        base.Draw(gameTime);
    }

    protected override void UnloadContent()
    {
        if (_validationTextureFromContent != null)
        {
            Content.Unload();
            _validationTextureFromContent = null;
        }

        _validationEffectFromContent = null;
        _validationFontFromContent = null;

        if (_spriteBatch != null)
        {
            _spriteBatch.Dispose();
            _spriteBatch = null;
        }

        base.UnloadContent();
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

    private T LoadValidatedContent<T>(string assetName, string assetPath)
    {
        ValidateContentPipelineAssetHeader(assetPath);
        return Content.Load<T>(assetName);
    }

    private bool IsValidationReady()
    {
        return _spriteBatch != null
            && _validationEffectFromContent != null
            && _validationFontFromContent != null
            && _validationTextureFromContent != null;
    }

    private void ReportFirstDrawValidation()
    {
        if (_reportedFirstValidationDraw)
            return;

        _reportedFirstValidationDraw = true;
        BrowserHostValidationReporter.ReportPhase(
            "firstValidationDraw",
            "First validation draw reached with Content.Load<Texture2D>, Content.Load<SpriteFont>, and Content.Load<Effect>.");
    }

    private static void ValidateContentPipelineAssetHeader(string assetName)
    {
        using Stream contentStream = TitleContainer.OpenStream(assetName);

        int firstByte = contentStream.ReadByte();
        int secondByte = contentStream.ReadByte();
        int thirdByte = contentStream.ReadByte();

        if (firstByte != 'X' || secondByte != 'N' || thirdByte != 'B')
        {
            throw new InvalidOperationException(
                $"The content pipeline asset '{assetName}' did not begin with the expected XNB header.");
        }
    }

    private static void ValidateRawFile()
    {
        OpenValidatedRawFile(ValidationRawAssetPath, ValidationRawAssetContents);

        BrowserHostValidationReporter.ReportPhase(
            "rawFileValidated",
            "Validated TitleContainer.OpenStream for a raw file staged in the named asset pack.");
    }

    private static void ValidateRawFileReopen()
    {
        OpenValidatedRawFile(ValidationRawAssetPath, ValidationRawAssetContents);

        BrowserHostValidationReporter.ReportPhase(
            "rawFileReopenValidated",
            "Validated reopening a raw file through TitleContainer.OpenStream after its first stream was disposed.");
    }

    private static void OpenValidatedRawFile(string assetPath, string expectedContents)
    {
        using Stream rawFileStream = TitleContainer.OpenStream(assetPath);
        using StreamReader rawFileReader = new StreamReader(rawFileStream, Encoding.UTF8, false);
        string rawFileContents = rawFileReader.ReadLine() ?? string.Empty;

        if (!string.Equals(rawFileContents, expectedContents, StringComparison.Ordinal)
            || rawFileReader.ReadLine() != null)
        {
            throw new InvalidOperationException(
                $"The raw file '{assetPath}' did not contain the expected validation content.");
        }
    }

    private static void ValidateMissingAsset()
    {
        try
        {
            using Stream missingAssetStream = TitleContainer.OpenStream(ValidationMissingAssetPath);
        }
        catch (FileNotFoundException exception)
        {
            if (!exception.Message.Contains(ValidationMissingAssetPath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"The missing asset error did not identify '{ValidationMissingAssetPath}'.",
                    exception);
            }

            BrowserHostValidationReporter.ReportPhase(
                "missingAssetValidated",
                "Validated the path-specific FileNotFoundException for an unstaged raw file.");
            return;
        }

        throw new InvalidOperationException(
            $"TitleContainer.OpenStream unexpectedly opened missing asset '{ValidationMissingAssetPath}'.");
    }

    private void ValidateTextureDimensions()
    {
        if (_validationTextureFromContent == null)
        {
            throw new InvalidOperationException("Validation texture was not initialized before dimension validation.");
        }

        if (_validationTextureFromContent.Width <= 0
            || _validationTextureFromContent.Height <= 0)
        {
            throw new InvalidOperationException(
                $"Texture dimensions were invalid. Content={_validationTextureFromContent.Width}x{_validationTextureFromContent.Height}.");
        }
    }

    private void ValidateFontMeasurement()
    {
        if (_validationFontFromContent == null)
        {
            throw new InvalidOperationException("Validation SpriteFont was not initialized before measurement.");
        }

        Vector2 sampleTextSize = _validationFontFromContent.MeasureString(ValidationFontSampleText);
        if (sampleTextSize.X <= 0.0f || sampleTextSize.Y <= 0.0f)
        {
            throw new InvalidOperationException(
                $"SpriteFont measurement was invalid. Measured size={sampleTextSize.X}x{sampleTextSize.Y}.");
        }
    }

    private void ValidateEffect()
    {
        if (_validationEffectFromContent == null)
        {
            throw new InvalidOperationException("Validation Effect was not initialized before effect validation.");
        }

        if (_validationEffectFromContent.CurrentTechnique == null
            || _validationEffectFromContent.CurrentTechnique.Passes.Count == 0)
        {
            throw new InvalidOperationException("Validation Effect did not expose a usable technique and pass.");
        }

        EffectParameter? validationTintParameter = _validationEffectFromContent.Parameters["ValidationTint"];
        if (validationTintParameter == null)
        {
            throw new InvalidOperationException("Validation Effect did not expose the expected ValidationTint parameter.");
        }

        validationTintParameter.SetValue(new Vector4(0.75f, 1.0f, 0.75f, 1.0f));
    }

    private static void ValidatePlatformInfo()
    {
        if (PlatformInfo.MonoGamePlatform != MonoGamePlatform.WebGL)
        {
            throw new InvalidOperationException(
                $"Expected PlatformInfo.MonoGamePlatform to be {MonoGamePlatform.WebGL}, but it was {PlatformInfo.MonoGamePlatform}.");
        }

        if (PlatformInfo.GraphicsBackend != GraphicsBackend.OpenGL)
        {
            throw new InvalidOperationException(
                $"Expected PlatformInfo.GraphicsBackend to be {GraphicsBackend.OpenGL}, but it was {PlatformInfo.GraphicsBackend}.");
        }

        BrowserHostValidationReporter.ReportPhase(
            "platformInfoValidated",
            "Validated the shared native PlatformInfo values WebGL and OpenGL.");
    }
}
