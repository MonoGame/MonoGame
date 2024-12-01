using System;
using ___SafeGameName___.Core;
using ___SafeGameName___.Core.Effects;
using ___SafeGameName___.Core.Inputs;
using ___SafeGameName___.Core.Localization;
using ___SafeGameName___.Core.Settings;
using ___SafeGameName___.ScreenManagers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ___SafeGameName___.Screens;

/// <summary>
/// The main menu screen is the first thing displayed when the game starts up.
/// </summary>
class MainMenuScreen : MenuScreen
{
    private ContentManager content;
    private Level level;
    private bool readyToPlay;
    private PlayerIndex playerIndex;
    private ParticleManager particleManager;
    private SettingsManager<___SafeGameName___Settings> settingsManager;
    private MenuEntry playMenuEntry;
    private MenuEntry tutorialMenuEntry;
    private MenuEntry settingsMenuEntry;
    private MenuEntry aboutMenuEntry;
    private MenuEntry exitMenuEntry;
    private Texture2D gradientTexture;
    private bool showTutorial;
    private int tutorialStep = -1;
    private TimeSpan timeSinceLastMessage;

    /// <summary>
    /// Constructor fills in the menu contents.
    /// </summary>
    public MainMenuScreen()
        : base(Resources.MainMenu)
    {
        // Create our menu entries.
        playMenuEntry = new MenuEntry(Resources.Play);
        tutorialMenuEntry = new MenuEntry(Resources.Tutorial);
        settingsMenuEntry = new MenuEntry(Resources.Settings);
        aboutMenuEntry = new MenuEntry(Resources.About);
        exitMenuEntry = new MenuEntry(Resources.Exit);

        // Hook up menu event handlers.
        playMenuEntry.Selected += PlayMenuEntrySelected;
        tutorialMenuEntry.Selected += TutorialMenuEntrySelected;
        settingsMenuEntry.Selected += SettingsMenuEntrySelected;
        aboutMenuEntry.Selected += AboutMenuEntrySelected;
        exitMenuEntry.Selected += OnCancel;

        // Add entries to the menu.
        MenuEntries.Add(playMenuEntry);
        MenuEntries.Add(tutorialMenuEntry);
        MenuEntries.Add(settingsMenuEntry);
        MenuEntries.Add(aboutMenuEntry);
        MenuEntries.Add(exitMenuEntry);
    }

    private void SetLanguageText()
    {
        aboutMenuEntry.Text = Resources.About;
        playMenuEntry.Text = Resources.Play;
        settingsMenuEntry.Text = Resources.Settings;
        tutorialMenuEntry.Text = Resources.Tutorial;
        exitMenuEntry.Text = Resources.Exit;

        Title = "___SafeGameName___"; // TODO uncomment this if you want it to use Resources.MainMenu instead;
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

        particleManager ??= ScreenManager.Game.Services.GetService<ParticleManager>();

        settingsManager ??= ScreenManager.Game.Services.GetService<SettingsManager<___SafeGameName___Settings>>();
        settingsManager.Settings.PropertyChanged += (s, e) =>
        {
            SetLanguageText();
        };

        SetLanguageText();

        // Load the level.
        string levelPath = "Content/Levels/00.txt";
        level = new Level(ScreenManager.Game.Services, levelPath, 00);
        level.ParticleManager = particleManager;

        gradientTexture = content.Load<Texture2D>("Sprites/gradient");
    }

    /// <summary>
    /// Unload graphics content used by the game.
    /// </summary>
    public override void UnloadContent()
    {
        if (level != null)
        {
            level.Dispose();
        }

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
    public override void Update(GameTime gameTime,
        bool otherScreenHasFocus,
        bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        if (readyToPlay)
        {
            if (!level.ReachedExit)
            {
                level.Player.Movement = 1.0f;

                // Maybe get it to jump after moving??
                level.Player.Move(gameTime);
            }
            else
            {
                LoadingScreen.Load(ScreenManager,
                    true,
                    playerIndex,
                    new GameplayScreen());
            }
        }

        if (showTutorial)
        {
            UpdateTutorialSteps(gameTime);
        }
    }

    /// <summary>
    /// Responds to user input.
    /// </summary>
    public override void HandleInput(GameTime gameTime, InputState inputState)
    {
        base.HandleInput(gameTime, inputState);

        // update our level, passing down the GameTime along with all of our input states
        level.Update(gameTime,
            inputState,
            ScreenManager.Game.Window.CurrentOrientation,
            readyToPlay);
    }

    private void UpdateTutorialSteps(GameTime gameTime)
    {
        if (gameTime.TotalGameTime - timeSinceLastMessage > TimeSpan.FromSeconds(3)) // Should the showtime be in settings?
        {
            tutorialStep++;
            timeSinceLastMessage = gameTime.TotalGameTime;

            if (tutorialStep > 2)
            {
                tutorialStep = -1;
                showTutorial = false;
            }
        }
    }

    /// <summary>
    /// Draws the gameplay screen.
    /// </summary>
    public override void Draw(GameTime gameTime)
    {
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

        spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, ScreenManager.GlobalTransformation);

        level.Draw(gameTime, spriteBatch);

        if (showTutorial)
        {
            DrawTutorialSteps(spriteBatch);
        }

        spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawTutorialSteps(SpriteBatch spriteBatch)
    {
        SpriteFont font = ScreenManager.Font;

        // The background includes a border somewhat larger than the text itself.
        const int hPad = 32;
        const int vPad = 16;

        Rectangle backgroundRectangle = Rectangle.Empty;
        Vector2 textSize;
        string message = string.Empty;

        switch (tutorialStep)
        {
            case 0:
                message = Resources.CollectThese;
                textSize = font.MeasureString(message);
                backgroundRectangle = new Rectangle((int)level.Gems[0].Position.X - 50 - hPad,
                                                      (int)level.Gems[0].Position.Y - vPad - 60,
                                                      (int)textSize.X + hPad * 2,
                                                      (int)textSize.Y + vPad * 2);
                break;

            case 1:
                message = Resources.GetToHere;
                textSize = font.MeasureString(message);
                backgroundRectangle = new Rectangle((int)level.Exit.X - 50 - hPad,
                                                      (int)level.Exit.Y - vPad - 60,
                                                      (int)textSize.X + hPad * 2,
                                                      (int)textSize.Y + vPad * 2);
                break;

            case 2:
                message = Resources.DontDie;
                textSize = font.MeasureString(message);
                backgroundRectangle = new Rectangle((int)level.Player.Position.X - 50 - hPad,
                                                      (int)level.Player.Position.Y - vPad - 100,
                                                      (int)textSize.X + hPad * 2,
                                                      (int)textSize.Y + vPad * 2);
                break;
        }

        Vector2 textPosition = new Vector2(backgroundRectangle.X + hPad, backgroundRectangle.Y + vPad);

        // Draw the background rectangle.
        spriteBatch.Draw(gradientTexture, backgroundRectangle, Color.White);

        // Draw the tutorial text.
        spriteBatch.DrawString(font, message, textPosition, Color.White);
    }

    /// <summary>
    /// Event handler for when the Play menu entry is selected.
    /// </summary>
    void PlayMenuEntrySelected(object sender, PlayerIndexEventArgs e)
    {
        var toastMessageBox = new MessageBoxScreen(Resources.LetsGo, false, new TimeSpan(0, 0, 1), true);
        toastMessageBox.Accepted += (sender, e) =>
        {
            playerIndex = e.PlayerIndex;
            readyToPlay = true;
        };
        ScreenManager.AddScreen(toastMessageBox, e.PlayerIndex);
    }

    /// <summary>
    /// Event handler for when the Tutorial menu entry is selected.
    /// </summary>
    private void TutorialMenuEntrySelected(object sender, PlayerIndexEventArgs e)
    {
        // You could create another screen and show the tutorial there.
        // But modern games, have a more dynamic main menu
        showTutorial = true;
    }

    /// <summary>
    /// Event handler for when the Options menu entry is selected.
    /// </summary>
    void SettingsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
    {
        ScreenManager.AddScreen(new SettingsScreen(), e.PlayerIndex);
    }

    /// <summary>
    /// Event handler for when the Options menu entry is selected.
    /// </summary>
    void AboutMenuEntrySelected(object sender, PlayerIndexEventArgs e)
    {
        ScreenManager.AddScreen(new AboutScreen(), e.PlayerIndex);
    }

    /// <summary>
    /// When the user cancels the main menu, ask if they want to exit the sample.
    /// </summary>
    protected override void OnCancel(PlayerIndex playerIndex)
    {
        string message = Resources.ExitQuestion;

        MessageBoxScreen confirmExitMessageBox = new MessageBoxScreen(message);

        confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;

        ScreenManager.AddScreen(confirmExitMessageBox, playerIndex);
    }


    /// <summary>
    /// Event handler for when the user selects ok on the "are you sure
    /// you want to exit" message box.
    /// </summary>
    void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
    {
        ScreenManager.Game.Exit();
    }
}