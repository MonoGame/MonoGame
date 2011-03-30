#region File Description
//-----------------------------------------------------------------------------
// Screen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace Marblets
{
    /// <summary>
    /// Screen represents a unit of rendering for the game, generally transitional point
    /// such as splash screens, selection screens and the actual game levels.
    /// </summary>
    public class Screen : DrawableGameComponent
    {
        private bool isMusicPlaying;
        // TODO private Cue cue;
        // TODO private SoundEntry backgroundMusic;
        private Texture2D backgroundTexture;
        private string backgroundImage;
        private RelativeSpriteBatch batch;

        /// <summary>
        /// Gets the sprite batch used for this screen
        /// </summary>
        /// <value>The sprite batch for this screen</value>
        public RelativeSpriteBatch SpriteBatch
        {
            get
            {
                return batch;
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="game">The game object</param>
        /// <param name="backgroundImage">The background image to use when this is 
        /// visible</param>
        /// <param name="backgroundMusic">The background music to play when this is 
        /// visible</param>
        public Screen(Game game, string backgroundImage/* TODO, SoundEntry backgroundMusic*/)
            : base(game)
        {
            this.backgroundImage = backgroundImage;
            // TODO this.backgroundMusic = backgroundMusic;
        }

        /// <summary>
        /// Initializes the component.  Override to load any non-graphics resources and
        /// query for any required services.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            StartMusic();
        }

        /// <summary>
        /// Called when the DrawableGameComponent.Visible property changes.  Raises the
        /// DrawableGameComponent.VisibleChanged event.
        /// </summary>
        /// <param name="sender">The DrawableGameComponent.</param>
        /// <param name="args">Arguments to the DrawableGameComponent.VisibleChanged 
        /// event.</param>
        protected override void OnVisibleChanged(object sender, EventArgs args)
        {
            base.OnVisibleChanged(sender, args);
            if (!Visible)
            {
                ShutdownMusic();
            }
            else
            {
                StartMusic();
            }

        }

        private void ShutdownMusic()
        {
            if (isMusicPlaying)
            {
                // TODO Sound.Stop(cue);
                isMusicPlaying = false;
            }
        }

        private void StartMusic()
        {
            // TODO cue = Sound.Play(backgroundMusic);
            isMusicPlaying = true;
        }

        /// <summary>
        /// Tidies up the scene.
        /// </summary>
        public virtual void Shutdown()
        {
            ShutdownMusic();

            if (batch != null)
            {
                batch.Dispose();
                batch = null;
            }
        }

        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            //Re-Create the Sprite Batch!
            IGraphicsDeviceService graphicsService =
                Game.Services.GetService(typeof(IGraphicsDeviceService))
                as IGraphicsDeviceService;

            batch = new RelativeSpriteBatch(graphicsService.GraphicsDevice);

            //Load content for any sub components

			if (!String.IsNullOrEmpty(backgroundImage))
            {
                backgroundTexture = MarbletsGame.Content.Load<Texture2D>(backgroundImage);
            }
        }


        /// <summary>
        /// Renders the screen. 
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (!String.IsNullOrEmpty(backgroundImage))
            {
                SpriteBatch.Begin();
                SpriteBatch.Draw(backgroundTexture, Vector2.Zero, Color.White);
                SpriteBatch.End();

            }
        }
    }
}
