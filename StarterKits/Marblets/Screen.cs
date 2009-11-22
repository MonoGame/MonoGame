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
using XnaTouch.Framework;
using XnaTouch.Framework.Graphics;
using XnaTouch.Framework.Audio;
#endregion

namespace Marblets
{
    /// <summary>
    /// Screen represents a unit of rendering for the game, generally transitional point
    /// such as splash screens, selection screens and the actual game levels.
    /// </summary>
    public class Screen : DrawableGameComponent
    {
        private Texture2D backgroundTexture;
        private string backgroundImage;
        private SpriteBatch batch;

        /// <summary>
        /// Gets the sprite batch used for this screen
        /// </summary>
        /// <value>The sprite batch for this screen</value>
        public SpriteBatch SpriteBatch
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
        public Screen(Game game, string backgroundImage)
            : base(game)
        {
            this.backgroundImage = backgroundImage;
        }

        /// <summary>
        /// Initializes the component.  Override to load any non-graphics resources and
        /// query for any required services.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
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
        }

        /// <summary>
        /// Tidies up the scene.
        /// </summary>
        public virtual void Shutdown()
        {
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

            batch = new SpriteBatch(graphicsService.GraphicsDevice);

            //Load content for any sub components
            if (!String.IsNullOrEmpty(backgroundImage))
            {
                backgroundTexture =
                    MarbletsGame.Content.Load<Texture2D>(backgroundImage);
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

                Vector2 drawPosition = Vector2.Zero;

                // Added functionality to account for the screen orientation.
                switch (MarbletsGame.screenOrientation)
                {
                    case MarbletsGame.ScreenOrientation.LandscapeRight:
                        drawPosition.X = 320;
                        drawPosition.Y = 160;
                        break;

                    case MarbletsGame.ScreenOrientation.LandscapeLeft:
                        drawPosition.X = 272;
                        drawPosition.Y = 0;
                        break;

                    default:
                        break;
                }

				SpriteBatch.Draw(backgroundTexture, new Rectangle((int)drawPosition.X,(int)drawPosition.Y,480,320),null,
				                 Color.White,MarbletsGame.screenRotation,Vector2.Zero,SpriteEffects.None,0.0f);

                SpriteBatch.End();
            }
        }
    }
}
