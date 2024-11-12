using ___SafeGameName___.Core;
using ___SafeGameName___.Core.Effects;
using ___SafeGameName___.Core.Inputs;
using ___SafeGameName___.Core.Localization;
using ___SafeGameName___.Core.Settings;
using GameStateManagement.Inputs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using System;
using System.IO;

namespace ___SafeGameName___.Screens;

/// <summary>
/// This screen implements the actual game logic. It is just a
/// placeholder to get the idea across: you'll probably want to
/// put some more interesting gameplay in here!
/// </summary>
class GameplayScreen : GameScreen
{
    ContentManager content;

    float pauseAlpha;

    private SpriteBatch spriteBatch;

    // Global content.
    private SpriteFont hudFont;

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

    private Texture2D backpack;
    private ParticleManager particleManager;
    private SettingsManager<___SafeGameName___Leaderboard> leaderboardManager;
    private string endOfLevelMessage;
    private EndOfLevelMessageState endOfLevelMessgeState;
    private const int textEdgeSpacing = 10;

    enum EndOfLevelMessageState
    {
        NotShowing,
        Show,
        Showing,
    }

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
        base.LoadContent();

        if (content == null)
            content = new ContentManager(ScreenManager.Game.Services, "Content");

        spriteBatch = ScreenManager.SpriteBatch;

        // Load fonts
        hudFont = content.Load<SpriteFont>("Fonts/Hud");

        MediaPlayer.IsRepeating = true;
        MediaPlayer.Play(content.Load<Song>("Sounds/Music"));

        particleManager ??= ScreenManager.Game.Services.GetService<ParticleManager>();

        leaderboardManager ??= ScreenManager.Game.Services.GetService<SettingsManager<___SafeGameName___Leaderboard>>();

        LoadNextLevel();

        backpack = content.Load<Texture2D>("Sprites/backpack");

        // once the load has finished, we use ResetElapsedTime to tell the game's
        // timing mechanism that we have just finished a very long frame, and that
        // it should not try to catch up.
        ScreenManager.Game.ResetElapsedTime();
    }

    private void LoadNextLevel()
    {
        // move to the next level
        levelIndex = (levelIndex + 1) % Level.NUMBER_OF_LEVELS;

        // Unloads the content for the current level before loading the next one.
        if (level != null)
            level.Dispose();

        // Load the level.
        var levelPath = string.Format("Content/Levels/{0:00}.txt", levelIndex);
        level = new Level(ScreenManager.Game.Services, levelPath, levelIndex);
        level.ParticleManager = particleManager;

        var levelFileName = Path.GetFileName(levelPath);
        var leaderboardFileName = Path.ChangeExtension(levelFileName, ".json");
        leaderboardManager.Storage.SettingsFileName = leaderboardFileName;
        level.LeaderboardManager = leaderboardManager;

        endOfLevelMessgeState = EndOfLevelMessageState.NotShowing;
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

        level.Paused = !IsActive;

        // update our level, passing down the GameTime along with all of our input states
        level.Update(gameTime,
            keyboardState,
            gamePadState,
            accelerometerState,
            ScreenManager.Game.Window.CurrentOrientation);

        if (IsActive)
        {
            switch (endOfLevelMessgeState)
            {
                case EndOfLevelMessageState.NotShowing:
                    if (level.TimeTaken == level.MaximumTimeToCompleteLevel)
                    {
                        if (level.ReachedExit)
                        {
                            endOfLevelMessage = GetLevelStats(Resources.LevelCompleted);
                        }
                        else
                        {
                            endOfLevelMessage = GetLevelStats(Resources.TimeRanOut);
                        }

                        endOfLevelMessgeState = EndOfLevelMessageState.Show;
                    }
                    else if (!level.Player.IsAlive)
                    {
                        endOfLevelMessage = GetLevelStats(Resources.YouDied);
                        endOfLevelMessgeState = EndOfLevelMessageState.Show;
                    }
                    break;
                case EndOfLevelMessageState.Showing:
                    break;
            }
        }
    }

    private string GetLevelStats(string messageTitle)
    {
        var message = messageTitle + Environment.NewLine + Environment.NewLine;

        if (level.NewHighScore)
            message += Resources.NewHighScore + Environment.NewLine + Environment.NewLine;

        message +=
            Resources.Score + ": " + level.Score + Environment.NewLine +
            Resources.Time + ": " + level.TimeTaken + Environment.NewLine +
            Resources.GemsCollected + $": {level.GemsCollected:D2}/ {level.GemsCount:D2}";

        return message;
    }

    /// <summary>
    /// Lets the game respond to player input. Unlike the Update method,
    /// this will only be called when the gameplay screen is active.
    /// </summary>
    public override void HandleInput(GameTime gameTime, InputState inputState)
    {
        ArgumentNullException.ThrowIfNull(inputState);

        // Get all of our input states for the active player profile.
        int playerIndex = (int)ControllingPlayer.Value;

        // The game pauses either if the user presses the pause button, or if
        // they unplug the active gamepad. This requires us to keep track of
        // whether a gamepad was ever plugged in, because we don't want to pause
        // on PC if they are playing with a keyboard and have no gamepad at all!
        bool gamePadDisconnected = !gamePadState.IsConnected &&
                                   inputState.GamePadWasConnected[playerIndex];

        if (inputState.IsPauseGame(ControllingPlayer) || gamePadDisconnected)
        {
            ScreenManager.AddScreen(new PauseScreen(), ControllingPlayer);
        }
        else
        {
            keyboardState = inputState.CurrentKeyboardStates[playerIndex];
            gamePadState = inputState.CurrentGamePadStates[playerIndex];

            touchState = inputState.CurrentTouchState;

            accelerometerState = inputState.CurrentAccelerometerState;

            // Exit the game when back is pressed.
            if (gamePadState.Buttons.Back == ButtonState.Pressed)
                ScreenManager.Game.Exit();

            if (endOfLevelMessgeState == EndOfLevelMessageState.Show && IsActive)
            {
                var toastMessageBox = new MessageBoxScreen(endOfLevelMessage, false, new TimeSpan(0, 0, 5), true);
                toastMessageBox.Accepted += (sender, e) =>
                {
                    wasContinuePressed = true;
                };
                endOfLevelMessgeState = EndOfLevelMessageState.Showing;
                ScreenManager.AddScreen(toastMessageBox, ControllingPlayer);
            }

            // Perform the appropriate action to advance the game and
            // to get the player back to playing.
            if (wasContinuePressed)
            {
                if (!level.Player.IsAlive)
                {
                    level.StartNewLife();
                }
                else if (level.TimeTaken == level.MaximumTimeToCompleteLevel)
                {
                    if (level.ReachedExit)
                    {
                        LoadNextLevel();
                    }
                    else
                    {
                        ReloadCurrentLevel();
                    }
                }

                wasContinuePressed = false;
            }
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

        spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, ScreenManager.GlobalTransformation);

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

        // Draw time taken. Uses modulo division to cause blinking when the
        // player is running out of time.
        string drawableString = Resources.Time + level.TimeTaken.Minutes.ToString("00") + ":" + level.TimeTaken.Seconds.ToString("00");
        var timeDimensions = hudFont.MeasureString(drawableString);
        Color timeColor;
        if (level.TimeTaken < level.MaximumTimeToCompleteLevel - WarningTime ||
            level.ReachedExit ||
            (int)level.TimeTaken.TotalSeconds % 2 == 0)
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
        DrawShadowedString(hudFont, drawableString, hudLocation + new Vector2(hudLocation.X + ScreenManager.BaseScreenSize.X - scoreDimensions.X - textEdgeSpacing, textEdgeSpacing), Color.Yellow);

        spriteBatch.Draw(backpack, new Vector2((ScreenManager.BaseScreenSize.X - backpack.Width) / 2, textEdgeSpacing), Color.White);
    }

    private void DrawShadowedString(SpriteFont font, string value, Vector2 position, Color color)
    {
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

        spriteBatch.DrawString(font, value, position + new Vector2(1.0f, 1.0f), Color.Black);
        spriteBatch.DrawString(font, value, position, color);
    }
}
