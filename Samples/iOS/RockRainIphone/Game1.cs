
using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using RockRainIphone;
using RockRainIphone.Core;

namespace RockRainIphone
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
		private ExplosionManager explosions;

        // Textures
		protected Texture2D gamepadTexture,helpBackgroundTexture;
        protected Texture2D startBackgroundTexture, startElementsTexture;
        protected Texture2D actionElementsTexture, actionBackgroundTexture;

        // Game Scenes
        protected HelpScene helpScene;
        protected StartScene startScene;
        protected ActionScene actionScene;
        protected GameScene activeScene;

        // Audio Stuff
        private AudioLibrary audio;

        // Fonts
        private SpriteFont smallFont, largeFont, scoreFont;

		// Used for handle input
        protected GamePadState oldGamePadState;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
			
            // Used for input handling
            oldGamePadState = GamePad.GetState(PlayerIndex.One);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            Services.AddService(typeof(SpriteBatch), spriteBatch);

			// Setup virtual gamepad
			gamepadTexture = Content.Load<Texture2D>("gamepad.png");  
			ButtonDefinition BButton = new ButtonDefinition();
			BButton.Texture = gamepadTexture;
			BButton.Position = new Vector2(100,420);
			BButton.Type = Buttons.Back;
			BButton.TextureRect = new Rectangle(72,77,36,36);
			
			ButtonDefinition AButton = new ButtonDefinition();
			AButton.Texture = gamepadTexture;
			AButton.Position = new Vector2(150,420);
			AButton.Type = Buttons.A;
			AButton.TextureRect = new Rectangle(73,114,36,36);
			
			GamePad.ButtonsDefinitions.Add(BButton);
			GamePad.ButtonsDefinitions.Add(AButton);
			
			ThumbStickDefinition thumbStick = new ThumbStickDefinition();
			thumbStick.Position = new Vector2(220,400);
			thumbStick.Texture = gamepadTexture;
			thumbStick.TextureRect = new Rectangle(2,2,68,68);	
			GamePad.LeftThumbStickDefinition = thumbStick;
			
            // Create the audio bank
            audio = new AudioLibrary(Content);
            Services.AddService(typeof(AudioLibrary), audio);
			
            // Create the Start Scene
            smallFont = Content.Load<SpriteFont>("menuSmall");
			largeFont = Content.Load<SpriteFont>("menuLarge");
			startElementsTexture = Content.Load<Texture2D>("startsceneelements.png");            
            startBackgroundTexture = Content.Load<Texture2D>("startbackground");
            startScene = new StartScene(this, smallFont, largeFont,startBackgroundTexture, startElementsTexture);
            Components.Add(startScene);
			
            // Start the game in the start Scene :)
            startScene.Show();
            activeScene = startScene;
        }

		private void CreateActionScene()
		{
			explosions = new ExplosionManager(this);
            actionElementsTexture = Content.Load<Texture2D>("rockrainenhanced.png");
            actionBackgroundTexture = Content.Load<Texture2D>("spacebackground.jpg");
            scoreFont = Content.Load<SpriteFont>("score");
            actionScene = new ActionScene(this, actionElementsTexture,actionBackgroundTexture, scoreFont, explosions);
            Components.Add(actionScene);
		}
		
		private void CreateHelpScene()
		{
            helpBackgroundTexture = Content.Load<Texture2D>("helpscreen.png");
            helpScene = new HelpScene(this, helpBackgroundTexture);
            Components.Add(helpScene);
		}
		
        /// <summary>
        /// Open a new scene
        /// </summary>
        /// <param name="scene">Scene to be opened</param>
        protected void ShowScene(GameScene scene)
        {
            activeScene.Hide();
            activeScene = scene;
            scene.Show();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Handle Game Inputs
            HandleScenesInput();

            base.Update(gameTime);
        }

        /// <summary>
        /// Handle input of all game scenes
        /// </summary>
        private void HandleScenesInput()
        {
            // Handle Start Scene Input
            if (activeScene == startScene)
            {
                HandleStartSceneInput();
            }
            // Handle Help Scene input
            else if (activeScene == helpScene)
            {
                if ((Mouse.GetState().X != 0) || (Mouse.GetState().Y != 0))
                {
                    ShowScene(startScene);
                }
            }
            // Handle Action Scene Input
            else if (activeScene == actionScene)
            {
                HandleActionInput();
            }
        }

        /// <summary>
        /// Check if the Enter Key ou 'A' button was pressed
        /// </summary>
        /// <returns>true, if enter key ou 'A' button was pressed</returns>
        private void HandleActionInput()
        {
            // Get the Keyboard and GamePad state
            GamePadState gamepadState = GamePad.GetState(PlayerIndex.One);

            bool backKey = (oldGamePadState.Buttons.Back == ButtonState.Pressed) &&
                       (gamepadState.Buttons.Back == ButtonState.Released);

            bool enterKey = (oldGamePadState.Buttons.A == ButtonState.Pressed) &&
                        (gamepadState.Buttons.A == ButtonState.Released);

            oldGamePadState = gamepadState;

            if (enterKey)
            {
                if (actionScene.GameOver)
                {
                    ShowScene(startScene);
                }
                else
                {
                    audio.MenuBack.Play();
                    actionScene.Paused = !actionScene.Paused;
                }
            }

            if (backKey)
            {
                ShowScene(startScene);
            }
        }

        /// <summary>
        /// Handle buttons and keyboard in StartScene
        /// </summary>
        private void HandleStartSceneInput()
        {
            if (startScene.MenuSelected)
            {
				Mouse.SetPosition(0,0);
                audio.MenuSelect.Play();
                switch (startScene.SelectedMenuIndex)
                {
                    case 0:
						if (actionScene == null)
							CreateActionScene();
                        ShowScene(actionScene);
                        break;
                    case 1:
						if (helpScene == null)
							CreateHelpScene();
                        ShowScene(helpScene);
                        break;
                    case 2:
                        Exit();
                        break;
                }
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
			graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
			
            // Begin..
            spriteBatch.Begin();

            // Draw all Game Components..
            base.Draw(gameTime);

			GamePad.Draw(gameTime,spriteBatch);
            // End.
            spriteBatch.End();
						
			// Draw particles
			if (activeScene == actionScene)
				explosions.Draw(gameTime);
		}
    }
}
