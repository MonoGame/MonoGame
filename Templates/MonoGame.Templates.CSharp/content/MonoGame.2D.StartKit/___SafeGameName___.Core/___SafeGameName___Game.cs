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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Content;
using ___SafeGameName___.Core.ScreenManagers;
using ___SafeGameName___.Core.Screens;



#if !__IOS__
using Microsoft.Xna.Framework.Media;
#endif
#endregion

namespace ___SafeGameName___.Core;

/// <summary>
/// This is the main type for your game
/// </summary>
public class ___SafeGameName___Game : Microsoft.Xna.Framework.Game
{
    // Resources for drawing.
    private GraphicsDeviceManager graphics;
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
    private int levelIndex = -1;
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
    private const int numberOfLevels = 3;
    private const int textEdgeSpacing = 10;

    ScreenManager screenManager;

    public ___SafeGameName___Game()
    {
        graphics = new GraphicsDeviceManager(this);

        graphics.IsFullScreen = false;

        //graphics.PreferredBackBufferWidth = 800;
        //graphics.PreferredBackBufferHeight = 480;
        graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;

        Accelerometer.Initialize();

        // Create the screen manager component.
        screenManager = new ScreenManager(this);

        Components.Add(screenManager);

        // Activate the first screens.
        screenManager.AddScreen(new BackgroundScreen(), null);
        screenManager.AddScreen(new MainMenuScreen(), null);
    }

    /// <summary>
    /// LoadContent will be called once per game and is the place to load
    /// all of your content.
    /// </summary>
    protected override void LoadContent()
    {
        this.Content.RootDirectory = "Content";

        // Create a new SpriteBatch, which can be used to draw textures.
        spriteBatch = new SpriteBatch(GraphicsDevice);

        // Load fonts
        hudFont = Content.Load<SpriteFont>("Fonts/Hud");

        // Load overlay textures
        winOverlay = Content.Load<Texture2D>("Overlays/you_win");
        loseOverlay = Content.Load<Texture2D>("Overlays/you_lose");
        diedOverlay = Content.Load<Texture2D>("Overlays/you_died");

        ScalePresentationArea();

        virtualGamePad = new VirtualGamePad(baseScreenSize, globalTransformation, Content.Load<Texture2D>("Sprites/VirtualControlArrow"));

#if !__IOS__
        //Known issue that you get exceptions if you use Media PLayer while connected to your PC
        //See http://social.msdn.microsoft.com/Forums/en/windowsphone7series/thread/c8a243d2-d360-46b1-96bd-62b1ef268c66
        //Which means its impossible to test this from VS.
        //So we have to catch the exception and throw it away
        try
        {
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(Content.Load<Song>("Sounds/Music"));
        }
        catch { }
#endif
        LoadNextLevel();
    }

    public void ScalePresentationArea()
    {
        //Work out how much we need to scale our graphics to fill the screen
        backbufferWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
        backbufferHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
        float horScaling = backbufferWidth / baseScreenSize.X;
        float verScaling = backbufferHeight / baseScreenSize.Y;
        Vector3 screenScalingFactor = new Vector3(horScaling, verScaling, 1);
        globalTransformation = Matrix.CreateScale(screenScalingFactor);
        System.Diagnostics.Debug.WriteLine("Screen Size - Width[" + GraphicsDevice.PresentationParameters.BackBufferWidth + "] Height [" + GraphicsDevice.PresentationParameters.BackBufferHeight + "]");
    }

    
    /// <summary>
    /// Allows the game to run logic such as updating the world,
    /// checking for collisions, gathering input, and playing audio.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Update(GameTime gameTime)
    {
        //Confirm the screen has not been resized by the user
        if (backbufferHeight != GraphicsDevice.PresentationParameters.BackBufferHeight ||
            backbufferWidth != GraphicsDevice.PresentationParameters.BackBufferWidth)
        {
            ScalePresentationArea();
        }
        // Handle polling for our input and handling high-level input
        HandleInput(gameTime);

        // update our level, passing down the GameTime along with all of our input states
        level.Update(gameTime, keyboardState, gamePadState, 
                     accelerometerState, Window.CurrentOrientation);

        if (level.Player.Velocity != Vector2.Zero)
            virtualGamePad.NotifyPlayerIsMoving();

        base.Update(gameTime);
    }

    private void HandleInput(GameTime gameTime)
    {
        // get all of our input states
        keyboardState = Keyboard.GetState();
        touchState = TouchPanel.GetState();
        gamePadState = virtualGamePad.GetState(touchState, GamePad.GetState(PlayerIndex.One));
        accelerometerState = Accelerometer.GetState();

#if !NETFX_CORE && !__IOS__
        // Exit the game when back is pressed.
        if (gamePadState.Buttons.Back == ButtonState.Pressed)
            Exit();
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

    private void LoadNextLevel()
    {
        // move to the next level
        levelIndex = (levelIndex + 1) % numberOfLevels;

        // Unloads the content for the current level before loading the next one.
        if (level != null)
            level.Dispose();

        // Load the level.
        string levelPath = string.Format("Content/Levels/{0}.txt", levelIndex);
        using (Stream fileStream = TitleContainer.OpenStream(levelPath))
            level = new Level(Services, fileStream, levelIndex);
    }

    private void ReloadCurrentLevel()
    {
        --levelIndex;
        LoadNextLevel();
    }

    /// <summary>
    /// Draws the game from background to foreground.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Draw(GameTime gameTime)
    {
        graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

        spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null,null, globalTransformation);

        level.Draw(gameTime, spriteBatch);

        DrawHud();

        spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawHud()
    {
        Rectangle titleSafeArea = GraphicsDevice.Viewport.TitleSafeArea;
        Vector2 hudLocation = new Vector2(titleSafeArea.X, titleSafeArea.Y);
        //Vector2 center = new Vector2(titleSafeArea.X + titleSafeArea.Width / 2.0f,
        //                             titleSafeArea.Y + titleSafeArea.Height / 2.0f);

        Vector2 center = new Vector2(baseScreenSize.X / 2, baseScreenSize.Y / 2);

        // Draw time remaining. Uses modulo division to cause blinking when the
        // player is running out of time.
        string drawableString = "TIME: " + level.TimeRemaining.Minutes.ToString("00") + ":" + level.TimeRemaining.Seconds.ToString("00");
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
        drawableString = "SCORE: " + level.Score.ToString();
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
        spriteBatch.DrawString(font, value, position + new Vector2(1.0f, 1.0f), Color.Black);
        spriteBatch.DrawString(font, value, position, color);
    }
}
