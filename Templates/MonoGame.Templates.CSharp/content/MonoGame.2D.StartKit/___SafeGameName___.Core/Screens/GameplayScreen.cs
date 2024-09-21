#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Threading;
using ___SafeGameName___.Core;
using ___SafeGameName___.Core.Inputs;
using ___SafeGameName___.Core.Localization;
using ___SafeGameName___.Screens;
using GameStateManagement.Inputs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
#endregion

namespace ___SafeGameName___.Screens;

/// <summary>
/// This screen implements the actual game logic. It is just a
/// placeholder to get the idea across: you'll probably want to
/// put some more interesting gameplay in here!
/// </summary>
class GameplayScreen : GameScreen
{
    #region Fields

    ContentManager content;

    float pauseAlpha;

    private SpriteBatch spriteBatch;
    Vector2 baseScreenSize = new Vector2(800, 480);
    private Matrix globalTransformation;
    int backbufferWidth, backbufferHeight;

    // Global content.
    private SpriteFont hudFont;

    private Texture2D winOverlay;
    private Texture2D loseOverlay;
    private Texture2D diedOverlay;

    // Meta-level game state.
    private int levelIndex = 0;
    private Level level;
    private bool wasContinuePressed;

    // When the time remaining is less than the warning time, it blinks on the hud
    private static readonly TimeSpan WarningTime = TimeSpan.FromSeconds(30);

    // We store our input states so that we only poll once per frame, 
    // then we use the same input state wherever needed
    private GamePadState gamePadState;
    private KeyboardState keyboardState;
    private TouchCollection touchState;
    private AccelerometerState accelerometerState;

    private VirtualGamePad virtualGamePad;

    // The number of levels in the Levels directory of our content. We assume that
    // levels in our content are 0-based and that all numbers under this constant
    // have a level file present. This allows us to not need to check for the file
    // or handle exceptions, both of which can add unnecessary time to level loading.
    private const int numberOfLevels = 5;
    private const int textEdgeSpacing = 10;

    #endregion

    #region Initialization


    /// <summary>
    /// Constructor.
    /// </summary>
    public GameplayScreen()
    {
        TransitionOnTime = TimeSpan.FromSeconds(1.5);
        TransitionOffTime = TimeSpan.FromSeconds(0.5);
    }

    /// <summary>
    /// LoadContent will be called once per game and is the place to load
    /// all of your content for the game.
    /// </summary>
    public override void LoadContent()
    {
        if (content == null)
            content = new ContentManager(ScreenManager.Game.Services, "Content");

        spriteBatch = ScreenManager.SpriteBatch;

        // Load fonts
        hudFont = content.Load<SpriteFont>("Fonts/Hud");

        // Load overlay textures
        winOverlay = content.Load<Texture2D>("Overlays/you_win");
        loseOverlay = content.Load<Texture2D>("Overlays/you_lose");
        diedOverlay = content.Load<Texture2D>("Overlays/you_died");
        ScalePresentationArea();

        virtualGamePad = new VirtualGamePad(baseScreenSize, globalTransformation, content.Load<Texture2D>("Sprites/VirtualControlArrow"));

#if !__IOS__
        //Known issue that you get exceptions if you use Media PLayer while connected to your PC
        //See http://social.msdn.microsoft.com/Forums/en/windowsphone7series/thread/c8a243d2-d360-46b1-96bd-62b1ef268c66
        //Which means its impossible to test this from VS.
        //So we have to catch the exception and throw it away
        try
        {
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(content.Load<Song>("Sounds/Music"));
        }
        catch { }
#endif
        LoadNextLevel();

        // once the load has finished, we use ResetElapsedTime to tell the game's
        // timing mechanism that we have just finished a very long frame, and that
        // it should not try to catch up.
        ScreenManager.Game.ResetElapsedTime();
    }

    public void ScalePresentationArea()
    {
        //Work out how much we need to scale our graphics to fill the screen
        backbufferWidth = ScreenManager.GraphicsDevice.PresentationParameters.BackBufferWidth;
        backbufferHeight = ScreenManager.GraphicsDevice.PresentationParameters.BackBufferHeight;
        float horScaling = backbufferWidth / baseScreenSize.X;
        float verScaling = backbufferHeight / baseScreenSize.Y;
        Vector3 screenScalingFactor = new Vector3(horScaling, verScaling, 1);
        globalTransformation = Matrix.CreateScale(screenScalingFactor);
        System.Diagnostics.Debug.WriteLine("Screen Size - Width[" + ScreenManager.GraphicsDevice.PresentationParameters.BackBufferWidth + "] Height [" + ScreenManager.GraphicsDevice.PresentationParameters.BackBufferHeight + "]");
    }

    private void LoadNextLevel()
    {
        // move to the next level
        levelIndex = (levelIndex + 1) % numberOfLevels;

        // Unloads the content for the current level before loading the next one.
        if (level != null)
            level.Dispose();

        // Load the level.
        string levelPath = string.Format("Content/Levels/{0:00}.txt", levelIndex);
        using (Stream fileStream = TitleContainer.OpenStream(levelPath))
            level = new Level(ScreenManager.Game.Services, fileStream, levelIndex);
    }

    private void ReloadCurrentLevel()
    {
        --levelIndex;
        LoadNextLevel();
    }

    /// <summary>
    /// Unload graphics content used by the game.
    /// </summary>
    public override void UnloadContent()
    {
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
    public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                   bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, false);

        // Gradually fade in or out depending on whether we are covered by the pause screen.
        if (coveredByOtherScreen)
            pauseAlpha = Math.Min(pauseAlpha + 1f / 32, 1);
        else
            pauseAlpha = Math.Max(pauseAlpha - 1f / 32, 0);

        if (IsActive)
        {
            //Confirm the screen has not been resized by the user
            if (backbufferHeight != ScreenManager.GraphicsDevice.PresentationParameters.BackBufferHeight ||
            backbufferWidth != ScreenManager.GraphicsDevice.PresentationParameters.BackBufferWidth)
            {
                ScalePresentationArea();
            }

            // update our level, passing down the GameTime along with all of our input states
            level.Update(gameTime, keyboardState, gamePadState,
                         accelerometerState, ScreenManager.Game.Window.CurrentOrientation);

            if (level.Player.Velocity != Vector2.Zero)
                virtualGamePad.NotifyPlayerIsMoving();
        }
    }

    /// <summary>
    /// Lets the game respond to player input. Unlike the Update method,
    /// this will only be called when the gameplay screen is active.
    /// </summary>
    public override void HandleInput(InputState input, GameTime gameTime)
    {
        ArgumentNullException.ThrowIfNull(input);

        // Get all of our input states for the active player profile.
        int playerIndex = (int)ControllingPlayer.Value;

        // The game pauses either if the user presses the pause button, or if
        // they unplug the active gamepad. This requires us to keep track of
        // whether a gamepad was ever plugged in, because we don't want to pause
        // on PC if they are playing with a keyboard and have no gamepad at all!
        bool gamePadDisconnected = !gamePadState.IsConnected &&
                                   input.GamePadWasConnected[playerIndex];

        if (input.IsPauseGame(ControllingPlayer) || gamePadDisconnected)
        {
            ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
        }
        else
        {
            keyboardState = input.CurrentKeyboardStates[playerIndex];
            gamePadState = virtualGamePad.GetState(touchState, input.CurrentGamePadStates[playerIndex]);

            touchState = input.TouchState;

            accelerometerState = input.AccelerometerState;

#if !NETFX_CORE && !__IOS__
            // Exit the game when back is pressed.
            if (gamePadState.Buttons.Back == ButtonState.Pressed)
                ScreenManager.Game.Exit();
#endif
            bool continuePressed =
                keyboardState.IsKeyDown(Keys.Space) ||
                keyboardState.IsKeyDown(Keys.Up) ||
                keyboardState.IsKeyDown(Keys.W) ||
                gamePadState.IsButtonDown(Buttons.A) ||
                touchState.AnyTouch();

            // Perform the appropriate action to advance the game and
            // to get the player back to playing.
            if (!wasContinuePressed && continuePressed)
            {
                if (!level.Player.IsAlive)
                {
                    level.StartNewLife();
                }
                else if (level.TimeRemaining == TimeSpan.Zero)
                {
                    if (level.ReachedExit)
                        LoadNextLevel();
                    else
                        ReloadCurrentLevel();
                }
            }

            wasContinuePressed = continuePressed;

            virtualGamePad.Update(gameTime);



        }
    }

    /// <summary>
    /// Draws the gameplay screen.
    /// </summary>
    public override void Draw(GameTime gameTime)
    {
        // This game has a blue background. Why? Because!
        ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                           Color.CornflowerBlue, 0, 0);

        // Our player and enemy are both actually just text strings.
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

        spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, globalTransformation);

        level.Draw(gameTime, spriteBatch);

        DrawHud();

        spriteBatch.End();

        base.Draw(gameTime);

        // If the game is transitioning on or off, fade it out to black.
        if (TransitionPosition > 0 || pauseAlpha > 0)
        {
            float alpha = MathHelper.Lerp(1f - TransitionAlpha, 1f, pauseAlpha / 2);

            ScreenManager.FadeBackBufferToBlack(alpha);
        }
    }

    private void DrawHud()
    {
        Rectangle titleSafeArea = ScreenManager.GraphicsDevice.Viewport.TitleSafeArea;
        Vector2 hudLocation = new Vector2(titleSafeArea.X, titleSafeArea.Y);
        //Vector2 center = new Vector2(titleSafeArea.X + titleSafeArea.Width / 2.0f,
        //                             titleSafeArea.Y + titleSafeArea.Height / 2.0f);

        Vector2 center = new Vector2(baseScreenSize.X / 2, baseScreenSize.Y / 2);

        // Draw time remaining. Uses modulo division to cause blinking when the
        // player is running out of time.
        string drawableString = Resources.Time + level.TimeRemaining.Minutes.ToString("00") + ":" + level.TimeRemaining.Seconds.ToString("00");
        var timeDimensions = hudFont.MeasureString(drawableString);
        Color timeColor;
        if (level.TimeRemaining > WarningTime ||
            level.ReachedExit ||
            (int)level.TimeRemaining.TotalSeconds % 2 == 0)
        {
            timeColor = Color.Yellow;
        }
        else
        {
            timeColor = Color.Red;
        }
        DrawShadowedString(hudFont, drawableString, hudLocation + new Vector2(textEdgeSpacing, textEdgeSpacing), timeColor);

        // Draw score
        drawableString = Resources.Score + level.Score.ToString();
        var scoreDimensions = hudFont.MeasureString(drawableString);
        DrawShadowedString(hudFont, drawableString, hudLocation + new Vector2(hudLocation.X + backbufferWidth - scoreDimensions.X - textEdgeSpacing, textEdgeSpacing), Color.Yellow);

        // Determine the status overlay message to show.
        Texture2D status = null;
        if (level.TimeRemaining == TimeSpan.Zero)
        {
            if (level.ReachedExit)
            {
                status = winOverlay;
            }
            else
            {
                status = loseOverlay;
            }
        }
        else if (!level.Player.IsAlive)
        {
            status = diedOverlay;
        }

        if (status != null)
        {
            // Draw status message.
            Vector2 statusSize = new Vector2(status.Width, status.Height);
            spriteBatch.Draw(status, center - statusSize / 2, Color.White);
        }

        if (touchState.IsConnected)
            virtualGamePad.Draw(spriteBatch);
    }

    private void DrawShadowedString(SpriteFont font, string value, Vector2 position, Color color)
    {
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

        spriteBatch.DrawString(font, value, position + new Vector2(1.0f, 1.0f), Color.Black);
        spriteBatch.DrawString(font, value, position, color);
    }
    #endregion
}
