#region File Description
//-----------------------------------------------------------------------------
// Starfield.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace NetRumble
{
    /// <summary>
    /// The starfield that renders behind the game, including a parallax effect.
    /// </summary>
    public class Starfield : IDisposable
    {
        #region Constants


        /// <summary>
        /// The number of stars in the starfield.
        /// </summary>
        const int numberOfStars = 256;

        /// <summary>
        /// The number of layers in the starfield.
        /// </summary>
        const int numberOfLayers = 8;

        /// <summary>
        /// The colors for each layer of stars.
        /// </summary>
        static readonly Color[] layerColors = new Color[numberOfLayers] 
            { 
                new Color(255, 255, 255, 255), 
                new Color(255, 255, 255, 216), 
                new Color(255, 255, 255, 192), 
                new Color(255, 255, 255, 160), 
                new Color(255, 255, 255, 128), 
                new Color(255, 255, 255, 96), 
                new Color(255, 255, 255, 64), 
                new Color(255, 255, 255, 32) 
            };

        /// <summary>
        /// The movement factor for each layer of stars, used in the parallax effect.
        /// </summary>
        static readonly float[] movementFactors = new float[numberOfLayers]
            {
                0.9f, 0.8f, 0.7f, 0.6f, 0.5f, 0.4f, 0.3f, 0.2f
            };

        /// <summary>
        /// The maximum amount of movement allowed per update.
        /// </summary>
        /// <remarks>
        /// Any per-update movement values that exceed this will trigger a 
        /// starfield reset.
        /// </remarks>
        const float maximumMovementPerUpdate = 128f;

        /// <summary>
        /// The background color of the starfield.
        /// </summary>
        static readonly Color backgroundColor = new Color(0, 0, 32);

        /// <summary>
        /// The size of each star, in pixels.
        /// </summary>
        const int starSize = 2;


        #endregion


        #region Gameplay Data


        /// <summary>
        /// The last position, used for the parallax effect.
        /// </summary>
        private Vector2 lastPosition;

        /// <summary>
        /// The current position, used for the parallax effect.
        /// </summary>
        private Vector2 position;

        /// <summary>
        /// The stars in the starfield.
        /// </summary>
        private Vector2[] stars;


        #endregion


        #region Graphics Data


        /// <summary>
        /// The graphics device used to render the starfield.
        /// </summary>
        private GraphicsDevice graphicsDevice;

        /// <summary>
        /// The content manager used to manage the textures in the starfield.
        /// </summary>
        private ContentManager contentManager;

        /// <summary>
        /// The SpriteBatch used to render the starfield.
        /// </summary>
        private SpriteBatch spriteBatch;

        /// <summary>
        /// The texture used for each star, typically one white pixel.
        /// </summary>
        private Texture2D starTexture;

        /// <summary>
        /// The cloud texture that appears behind the stars.
        /// </summary>
        private Texture2D cloudTexture;

        /// <summary>
        /// The effect used to draw the clouds.
        /// </summary>
        private Effect cloudEffect;

        /// <summary>
        /// The parameter on the cloud effect that receives the current position
        /// </summary>
        private EffectParameter cloudEffectPosition;


        #endregion


        #region Initialization Methods


        /// <summary>
        /// Create a new Starfield object.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="graphicsDevice">The graphics device used to render.</param>
        /// <param name="contentManager">The content manager for this object.</param>
        public Starfield(Vector2 position, GraphicsDevice graphicsDevice, 
            ContentManager contentManager)
        {
            // safety-check the parameters, as they must be valid
            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("graphicsDevice");
            }
            if (contentManager == null)
            {
                throw new ArgumentNullException("contentManager");
            }

            // assign the parameters
            this.graphicsDevice = graphicsDevice;
            this.contentManager = contentManager;

            // initialize the stars
            stars = new Vector2[numberOfStars];
            Reset(position);
        }


        /// <summary>
        /// Load graphics data from the system.
        /// </summary>
        public void LoadContent()
        {
            // load the cloud texture
            cloudTexture = contentManager.Load<Texture2D>("Textures/clouds");

            // load the cloud effect
            cloudEffect = contentManager.Load<Effect>("Effects/Clouds");
			//cloudEffect = new CloudsEffect(graphicsDevice); 
            cloudEffectPosition = cloudEffect.Parameters["Position"];
      
            // create the star texture
            starTexture = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
            starTexture.SetData<Color>(new Color[] { Color.White });

            // create the SpriteBatch object
            spriteBatch = new SpriteBatch(graphicsDevice);
        }


        /// <summary>
        /// Release graphics data.
        /// </summary>
        public void UnloadContent()
        {
            cloudTexture = null;
            cloudEffect = null;
            cloudEffectPosition = null;

            if (starTexture != null)
            {
                starTexture.Dispose();
                starTexture = null;
            }

            if (spriteBatch != null)
            {
                spriteBatch.Dispose();
                spriteBatch = null;
            }
        }


        /// <summary>
        /// Reset the stars and the parallax effect.
        /// </summary>
        /// <param name="position">The new origin point for the parallax effect.</param>
        public void Reset(Vector2 position)
        {
            // recreate the stars
            int viewportWidth = graphicsDevice.Viewport.Width;
            int viewportHeight = graphicsDevice.Viewport.Height;
            for (int i = 0; i < stars.Length; ++i)
            {
                stars[i] = new Vector2(RandomMath.Random.Next(0, viewportWidth), 
                    RandomMath.Random.Next(0, viewportHeight));
            }

            // reset the position
            this.lastPosition = this.position = position;
        }


        #endregion


        #region Draw Methods


        /// <summary>
        /// Update and draw the starfield.
        /// </summary>
        /// <remarks>
        /// This function updates and draws the starfield, 
        /// so that the per-star loop is only run once per frame.
        /// </remarks>
        /// <param name="position">The new position for the parallax effect.</param>
        public void Draw(Vector2 position)
        {
            // update the current position
            this.lastPosition = this.position;
            this.position = position;

            // determine the movement vector of the stars
            // -- for the purposes of the parallax effect, 
            //    this is the opposite direction as the position movement.
            Vector2 movement = -1.0f * (position - lastPosition);

            // create a rectangle representing the screen dimensions of the starfield
            Rectangle starfieldRectangle = new Rectangle(0, 0, 
                graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);

            // draw a background color for the starfield
            spriteBatch.Begin();
            spriteBatch.Draw(starTexture, starfieldRectangle, backgroundColor);
            spriteBatch.End();

            // draw the cloud texture
            cloudEffectPosition.SetValue(this.position);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, 
                null, null, null, cloudEffect);
            spriteBatch.Draw(cloudTexture, starfieldRectangle, null, Color.White, 0.0f,
                Vector2.Zero, SpriteEffects.None, 1.0f);
            spriteBatch.End();

            // if we've moved too far, then reset, as the stars will be moving too fast
            if (movement.Length() > maximumMovementPerUpdate)
            {
                Reset(position);
                return;
            }

            // draw all of the stars
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
            for (int i = 0; i < stars.Length; i++)
            {
                // move the star based on the depth
                int depth = i % movementFactors.Length;
                stars[i] += movement * movementFactors[depth];

                // wrap the stars around
                if (stars[i].X < starfieldRectangle.X)
                {
                    stars[i].X = starfieldRectangle.X + starfieldRectangle.Width;
                    stars[i].Y = starfieldRectangle.Y + 
                        RandomMath.Random.Next(starfieldRectangle.Height);
                }
                if (stars[i].X > (starfieldRectangle.X + starfieldRectangle.Width))
                {
                    stars[i].X = starfieldRectangle.X;
                    stars[i].Y = starfieldRectangle.Y + 
                        RandomMath.Random.Next(starfieldRectangle.Height);
                }
                if (stars[i].Y < starfieldRectangle.Y)
                {
                    stars[i].X = starfieldRectangle.X + 
                        RandomMath.Random.Next(starfieldRectangle.Width);
                    stars[i].Y = starfieldRectangle.Y + starfieldRectangle.Height;
                }
                if (stars[i].Y > 
                    (starfieldRectangle.Y + graphicsDevice.Viewport.Height))
                {
                    stars[i].X = starfieldRectangle.X + 
                        RandomMath.Random.Next(starfieldRectangle.Width);
                    stars[i].Y = starfieldRectangle.Y;
                }

                // draw the star
                spriteBatch.Draw(starTexture, 
                    new Rectangle((int)stars[i].X, (int)stars[i].Y, starSize, starSize),
                    null, layerColors[depth]);
            }
            spriteBatch.End();
        }


        #endregion

    
        #region IDisposable Implementation


        /// <summary>
        /// Finalizes the Starfield object, calls Dispose(false)
        /// </summary>
        ~Starfield()
        {
            Dispose(false);
        }


        /// <summary>
        /// Disposes the Starfield object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        
        /// <summary>
        /// Disposes this object.
        /// </summary>
        /// <param name="disposing">
        /// True if this method was called as part of the Dispose method.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (this)
                {
                    if (starTexture != null)
                    {
                        starTexture.Dispose();
                        starTexture = null;
                    }
                    if (spriteBatch != null)
                    {
                        spriteBatch.Dispose();
                        spriteBatch = null;
                    }
                }
            }
        }


        #endregion
    }
}

