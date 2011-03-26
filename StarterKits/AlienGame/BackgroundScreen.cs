//-----------------------------------------------------------------------------
// BackgroundScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AlienGameSample
{
    /// <summary>
    /// The background screen sits behind all the other menu screens.
    /// It draws a background image that remains fixed in place regardless
    /// of whatever transitions the screens on top of it may be doing.
    /// 
    /// A bit boring right now, but needed something in place...
    /// </summary>
    class BackgroundScreen : GameScreen
    {
        Texture2D title;
        Texture2D background;
		private Texture2D gamepadTexture;

        /// <summary>
        /// Constructor.
        /// </summary>
        public BackgroundScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.0);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        /// <summary>
        /// Loads graphics content for this screen. The background texture is quite
        /// big, so we use our own local ContentManager to load it. This allows us
        /// to unload before going from the menus into the game itself, wheras if we
        /// used the shared ContentManager provided by the Game class, the content
        /// would remain loaded forever.
        /// </summary>
        public override void LoadContent()
        {
            title = ScreenManager.Game.Content.Load<Texture2D>("title");
            background = ScreenManager.Game.Content.Load<Texture2D>("background");
			// Setup virtual gamepad
			gamepadTexture = ScreenManager.Game.Content.Load<Texture2D>("gamepad");  
			ButtonDefinition BButton = new ButtonDefinition();
			BButton.Texture = gamepadTexture;
			BButton.Position = new Vector2(270,240);
			BButton.Type = Buttons.Back;
			BButton.TextureRect = new Rectangle(72,77,36,36);
			
			ButtonDefinition AButton = new ButtonDefinition();
			AButton.Texture = gamepadTexture;
			AButton.Position = new Vector2(30,420);
			AButton.Type = Buttons.A;
			AButton.TextureRect = new Rectangle(73,114,36,36);
			
			GamePad.ButtonsDefinitions.Add(BButton);
			GamePad.ButtonsDefinitions.Add(AButton);
			
			ThumbStickDefinition thumbStick = new ThumbStickDefinition();
			thumbStick.Position = new Vector2(220,400);
			thumbStick.Texture = gamepadTexture;
			thumbStick.TextureRect = new Rectangle(2,2,68,68);
			
			GamePad.LeftThumbStickDefinition = thumbStick;
        }

        /// <summary>
        /// Updates the background screen. Unlike most screens, this should not
        /// transition off even if it has been covered by another screen: it is
        /// supposed to be covered, after all! This overload forces the
        /// coveredByOtherScreen parameter to false in order to stop the base
        /// Update method wanting to transition off.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);
        }

        /// <summary>
        /// Draws the background screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            // Make the menu slide into place during transitions, using a
            // power curve to make things look more interesting (this makes
            // the movement slow down as it nears the end).
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            spriteBatch.Begin();

            // Background
			spriteBatch.Draw(background,new Rectangle(0,0,320,480),new Color(255, 255, 255, TransitionAlpha));

            // Title
            spriteBatch.Draw(title, new Vector2((320-title.Width)/2, 60), new Color(255, 255, 255, TransitionAlpha));

			#if TARGET_IPHONE_SIMULATOR
			// Draw the GamePad
			GamePad.Draw(gameTime,spriteBatch);
			#endif
			
            spriteBatch.End();
        }
    }
}
