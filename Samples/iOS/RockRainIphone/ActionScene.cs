using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using RockRainIphone.Core;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;

namespace RockRainIphone
{
    /// <summary>
    /// This is a game component that implements the Action Scene.
    /// </summary>
    public class ActionScene : GameScene
    {
        // Basics
        protected Texture2D actionTexture;        
        protected SpriteBatch spriteBatch = null;
        protected AudioLibrary audio;

        // Game Elements
        protected Player player;
        protected MeteorsManager meteors;
        protected PowerSource powerSource;
        protected ImageComponent background;
        protected Score score;
		protected ExplosionManager _explosions;

        // Gui Stuff
        protected Vector2 pausePosition;
        protected Vector2 gameoverPosition;
        protected Rectangle pauseRect = new Rectangle(84, 65, 99, 30);
        protected Rectangle gameoverRect = new Rectangle(53, 108, 186, 30);

        // GameState elements
        protected bool paused;
        protected bool gameOver;
        protected TimeSpan elapsedTime = TimeSpan.Zero;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="game">The main game object</param>
        /// <param name="theTexture">Texture with the sprite elements</param>
        /// <param name="backgroundTexture">Texture for the background</param>
        /// <param name="font">Font used in the score</param>
        public ActionScene(Game game, Texture2D theTexture, 
            Texture2D backgroundTexture, SpriteFont font, ExplosionManager explosions) 
            : base(game)
        {
            // Get the current audiocomponent and play the background music
            audio = (AudioLibrary)Game.Services.GetService(typeof(AudioLibrary));

            background = new ImageComponent(game, backgroundTexture, 
                ImageComponent.DrawMode.Stretch);
            Components.Add(background);

            actionTexture = theTexture;

            spriteBatch = (SpriteBatch) 
                Game.Services.GetService(typeof (SpriteBatch));
            meteors = new MeteorsManager(Game, ref actionTexture);
            Components.Add(meteors);

            player = new Player(Game, ref actionTexture);
            player.Initialize();
            Components.Add(player);

            score = new Score(game, font, Color.LightGray);
            score.Position = new Vector2(1, 1);
            Components.Add(score);

            powerSource = new PowerSource(game, ref actionTexture);
            powerSource.Initialize();
            Components.Add(powerSource);
			
			_explosions = explosions;
        }

        /// <summary>
        /// Show the action scene
        /// </summary>
        public override void Show()
        {
			GamePad.Visible = true;
			
            MediaPlayer.Play(audio.BackMusic);

            meteors.Initialize();
            powerSource.PutinStartPosition();

            player.Reset();

            paused = false;
            pausePosition.X = (Game.Window.ClientBounds.Width - 
                pauseRect.Width)/2;
            pausePosition.Y = (Game.Window.ClientBounds.Height - 
                pauseRect.Height)/2;

            gameOver = false;
            gameoverPosition.X = (Game.Window.ClientBounds.Width - 
                gameoverRect.Width)/2;
            gameoverPosition.Y = (Game.Window.ClientBounds.Height - 
                gameoverRect.Height)/2;

            // Is a two-player game?
            player.Visible = true;

            base.Show();
        }

        /// <summary>
        /// Hide the scene
        /// </summary>
        public override void Hide()
        {
            // Stop the background music
            MediaPlayer.Stop();

            base.Hide();
        }

        /// <summary>
        /// True, if the game is in GameOver state
        /// </summary>
        public bool GameOver
        {
            get { return gameOver; }
        }

        /// <summary>
        /// Paused mode
        /// </summary>
        public bool Paused
        {
            get { return paused; }
            set
            {
                paused = value;
                if (paused)
                {
					//==================================
            		//AUDIO IS NOT SUPPORTED IN XNATOUCH
            		//==================================
                    //MediaPlayer.Pause();
                }
                else
                {
					//==================================
            		//AUDIO IS NOT SUPPORTED IN XNATOUCH
            		//==================================
                    //MediaPlayer.Resume();
                }
            }
        }

        /// <summary>
        /// Handle collisions with a meteor
        /// </summary>
        private void HandleDamages()
        {
            // Check Collision for player 1
            if (meteors.CheckForCollisions(player.GetBounds()))
            {
                // Player penalty
                player.Power -= 10;
				
				_explosions.AddExplosion(player.Position);
            }
        }

        /// <summary>
        /// Handle power-up stuff
        /// </summary>
        private void HandlePowerSourceSprite(GameTime gameTime)
        {
            // Player 1 get the power source
            if (powerSource.CheckCollision(player.GetBounds()))
            {
                audio.PowerGet.Play();
                elapsedTime = TimeSpan.Zero;
                powerSource.PutinStartPosition();
                player.Power += 50;
            }

            // Check for send a new Power source
            elapsedTime += gameTime.ElapsedGameTime;
            if (elapsedTime > TimeSpan.FromSeconds(15))
            {
                elapsedTime -= TimeSpan.FromSeconds(15);
                powerSource.Enabled = true;
                audio.PowerShow.Play();
            }
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
			// Update the explosions
            _explosions.Update(gameTime);
			
            if ((!paused) && (!gameOver))
            {
                // Check collisions with meteors
                HandleDamages();				 

                // Check if a player get a power boost
                HandlePowerSourceSprite(gameTime);

                // Update score
                score.Value = player.Score;
                score.Power = player.Power;

                // Check if player is dead
                gameOver = (player.Power <= 0);
                if (gameOver)
                {
                    player.Visible = (player.Power > 0);
                    
            		// Stop the music
					MediaPlayer.Stop();
                }

                // Update all other game components
                base.Update(gameTime);
            }

            // In game over state, keep the meteors animation
            if (gameOver)
            {
                meteors.Update(gameTime);
            }
        }

        /// <summary>
        /// Allows the game component to draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime)
        {
            // Draw all game components
            base.Draw(gameTime);
			
            if (paused)
            {
                // Draw the "pause" text
                spriteBatch.Draw(actionTexture, pausePosition, pauseRect, 
                    Color.White);
            }
            if (gameOver)
            {
                // Draw the "gameover" text
                spriteBatch.Draw(actionTexture, gameoverPosition, gameoverRect, 
                    Color.White);
            }
			
		}		
    }
}