using ___SafeGameName___.Core.Localization;
using GameStateManagement.Inputs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ___SafeGameName___.Screens;

/// <summary>
/// A popup message box screen, used to display "are you sure?"
/// confirmation messages.
/// </summary>
class MessageBoxScreen : GameScreen
{
    string message;
    Texture2D gradientTexture;
    private readonly bool toastMessage;
    private readonly TimeSpan toastDuration;
    private TimeSpan toastTimer;


    public event EventHandler<PlayerIndexEventArgs> Accepted;
    public event EventHandler<PlayerIndexEventArgs> Cancelled;


    /// <summary>
    /// Constructor automatically includes the standard "A=ok, B=cancel"
    /// usage text prompt.
    /// </summary>
    public MessageBoxScreen(string message)
        : this(message, true, TimeSpan.Zero)
    { }


    /// <summary>
    /// Constructor lets the caller specify whether to include the standard
    /// "A=ok, B=cancel" usage text prompt.
    /// </summary>
    public MessageBoxScreen(string message, bool includeUsageText, TimeSpan toastDuration, bool toastMessage = false)
    {
        string usageText = $"{Environment.NewLine}{Environment.NewLine}{Resources.YesButtonHelp}{Environment.NewLine}{Resources.NoButtonHelp}";

        if (includeUsageText && !toastMessage)
            this.message = message + usageText;
        else
            this.message = message;

        this.toastMessage = toastMessage;
        this.toastDuration = toastDuration;
        this.toastTimer = TimeSpan.Zero;

        IsPopup = true;

        TransitionOnTime = TimeSpan.FromSeconds(0.2);
        TransitionOffTime = TimeSpan.FromSeconds(0.2);
    }


    /// <summary>
    /// Loads graphics content for this screen. This uses the shared ContentManager
    /// provided by the Game class, so the content will remain loaded forever.
    /// Whenever a subsequent MessageBoxScreen tries to load this same content,
    /// it will just get back another reference to the already loaded data.
    /// </summary>
    public override void LoadContent()
    {
        ContentManager content = ScreenManager.Game.Content;

        gradientTexture = content.Load<Texture2D>("Sprites/gradient");
    }


    /// <summary>
    /// Responds to user input, accepting or cancelling the message box.
    /// </summary>
    public override void HandleInput(GameTime gameTime, InputState inputState)
    {
        base.HandleInput(gameTime, inputState);

        // Ignore input if this is a ToastMessage
        if (toastMessage)
        {
            return;
        }

        PlayerIndex playerIndex;

        // We pass in our ControllingPlayer, which may either be null (to
        // accept input from any player) or a specific index. If we pass a null
        // controlling player, the InputState helper returns to us which player
        // actually provided the input. We pass that through to our Accepted and
        // Cancelled events, so they can tell which player triggered them.
        if (inputState.IsMenuSelect(ControllingPlayer, out playerIndex))
        {
            // Raise the accepted event, then exit the message box.
            if (Accepted != null)
                Accepted(this, new PlayerIndexEventArgs(playerIndex));

            ExitScreen();
        }
        else if (inputState.IsMenuCancel(ControllingPlayer, out playerIndex))
        {
            // Raise the cancelled event, then exit the message box.
            if (Cancelled != null)
                Cancelled(this, new PlayerIndexEventArgs(playerIndex));

            ExitScreen();
        }
    }


    /// <summary>
    /// Updates the screen, particularly for toast messages.
    /// </summary>
    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
    {
        base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

        // Handle toast duration countdown.
        if (toastMessage)
        {
            toastTimer += gameTime.ElapsedGameTime;
            if (toastTimer >= toastDuration)
            {
                // Raise the accepted event, then exit the message box.
                if (Accepted != null)
                    Accepted(this, new PlayerIndexEventArgs(PlayerIndex.One));

                // Exit the screen when the toast time has elapsed.
                ExitScreen();
            }
        }
    }

    /// <summary>
    /// Draws the message box.
    /// </summary>
    public override void Draw(GameTime gameTime)
    {
        SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
        SpriteFont font = ScreenManager.Font;

        // Darken down any other screens that were drawn beneath the popup.
        ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);

        // Center the message text in the BaseScreenSize. The GlobalTransformation will scale everything for us.
        Vector2 textSize = font.MeasureString(message);
        Vector2 textPosition = (ScreenManager.BaseScreenSize - textSize) / 2;

        // The background includes a border somewhat larger than the text itself.
        const int hPad = 32;
        const int vPad = 16;

        Rectangle backgroundRectangle = new Rectangle((int)textPosition.X - hPad,
                                                      (int)textPosition.Y - vPad,
                                                      (int)textSize.X + hPad * 2,
                                                      (int)textSize.Y + vPad * 2);

        // Fade the popup alpha during transitions.
        Color color = Color.White * TransitionAlpha;

        spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, ScreenManager.GlobalTransformation);

        // Draw the background rectangle.
        spriteBatch.Draw(gradientTexture, backgroundRectangle, color);

        // Draw the message box text.
        spriteBatch.DrawString(font, message, textPosition, color);

        spriteBatch.End();
    }
}
