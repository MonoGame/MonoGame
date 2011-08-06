#region File Description
//-----------------------------------------------------------------------------
// Bee.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


#endregion

namespace HoneycombRush
{
    /// <summary>
    /// Repesents the base bee component.
    /// </summary>
    public abstract class Bee : TexturedDrawableGameComponent
    {
        #region Fields/Properties


        protected static Random random = new Random();

        protected Beehive relatedBeehive;
        protected Vector2 velocity;

        protected float rotation;
        protected bool isHitBySmoke;
        protected bool isGotHit;

        protected string AnimationKey { get; set; }

        TimeSpan velocityChangeTimer = TimeSpan.Zero;

        /// <summary>
        /// Timespan used to regenerate the be after it is chased away by smoke
        /// </summary>
        TimeSpan timeToRegenerate;

        /// <summary>
        /// Time at which the bee was hit by smoke
        /// </summary>
        TimeSpan timeHit;

        public bool IsBeeHit
        {
            get
            {
                return isHitBySmoke;
            }
        }

        public Beehive Beehive
        {
            get
            {
                return relatedBeehive;
            }
        }

        protected virtual TimeSpan VelocityChangeInterval
        {
            get
            {
                return TimeSpan.FromMilliseconds(500);
            }
        }

        public override Rectangle Bounds
        {
            get
            {
                if (texture == null)
                {
                    return default(Rectangle);
                }
                else
                {
                    // The bee's texture is an animation strip, so we must devide the texture's width by three 
                    // to get the bee's actual width
                    return new Rectangle((int)position.X, (int)position.Y, 
                        (int)(texture.Width / 3 * scaledSpriteBatch.ScaleVector.X),
                        (int)(texture.Height * scaledSpriteBatch.ScaleVector.Y));
                }
            }
        }


        #endregion

        #region Abstract Properties


        abstract protected int MaxVelocity { get; }
        abstract protected float AccelerationFactor { get; }


        #endregion

        #region Initialization


        /// <summary>
        /// Creates a new bee instance.
        /// </summary>
        /// <param name="game">The game object.</param>
        /// <param name="gamePlayScreen">The gameplay screen.</param>
        /// <param name="beehive">The related beehive.</param>
        public Bee(Game game, GameplayScreen gamePlayScreen, Beehive beehive)
            : base(game, gamePlayScreen)
        {
            this.relatedBeehive = beehive;
            DrawOrder = Int32.MaxValue - 20;
        }

        /// <summary>
        /// Initialize the bee's location and animation.
        /// </summary>
        public override void Initialize()
        {
            // Start up position
            SetStartupPosition();
            if (!string.IsNullOrEmpty(AnimationKey))
            {
                AnimationDefinitions[AnimationKey].PlayFromFrameIndex(0);
            }
            base.Initialize();
        }


        #endregion

        #region Update


        /// <summary>
        /// Updates the bee's status.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        public override void Update(GameTime gameTime)
        {
            if (!(gamePlayScreen.IsActive && gamePlayScreen.IsStarted))
            {
                base.Update(gameTime);
                return;
            }

            // This method will handle the regeneration of bees that were hit by  
            // smoke
            if (!HandleRegeneration(gameTime))
            {
                return;
            }

            if (!string.IsNullOrEmpty(AnimationKey))
            {
                AnimationDefinitions[AnimationKey].Update(gameTime, true);
            }

            // If a bee is hit by smoke, it doesn't have random movement until  
            //  regeneration
            if (!isHitBySmoke)
            {
                SetRandomMovement(gameTime);
            }

            // Moving the bee according to its velocity
            position += velocity * scaledSpriteBatch.ScaleVector;

            // If the bee is hit by smoke make it bee move faster
            if (isHitBySmoke)
            {
                position += velocity * scaledSpriteBatch.ScaleVector;
            }

            // If the bee is out of screen
            if (position.X < 0 || position.X > Game.GraphicsDevice.Viewport.Width - Bounds.Width ||
                position.Y < 0 || position.Y > Game.GraphicsDevice.Viewport.Height - Bounds.Height)
            {
                if (isHitBySmoke)
                {
                    // Reset the bee's position
                    SetStartupPositionWithTimer();
                }
                else
                {
                    // When hit by the screen bounds, we want the bee to move
                    // longer than usual before picking a new direction
                    velocityChangeTimer = TimeSpan.FromMilliseconds(-160);
                    if (position.X < Bounds.Width || position.X > Game.GraphicsDevice.Viewport.Width - Bounds.Width)
                    {
                        velocity = new Vector2(velocity.X *= -1, velocity.Y);
                    }
                    else
                    {
                        velocity = new Vector2(velocity.X, velocity.Y *= -1);
                    }
                }
            }

            base.Update(gameTime);
        }


        #endregion

        #region Render


        /// <summary>
        /// Renders the bee.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        public override void Draw(GameTime gameTime)
        {
            if (gamePlayScreen.IsActive && gamePlayScreen.IsStarted)
            {
                scaledSpriteBatch.Begin();

                // If the bee has an animation, draw it
                if (!string.IsNullOrEmpty(AnimationKey))
                {
                    AnimationDefinitions[AnimationKey].Draw(scaledSpriteBatch, position, SpriteEffects.None);
                }
                else
                {
                    scaledSpriteBatch.Draw(texture, position, null, Color.White, 0, Vector2.Zero, 1f, 
                        SpriteEffects.None, 0);
                }

                scaledSpriteBatch.End();
            }

            base.Draw(gameTime);
        }


        #endregion

        #region Public Methods


        /// <summary>
        /// Denotes that the bee has been hit by smoke.
        /// </summary>
        /// <param name="smokePuff">The smoke puff which the be was hit by.</param>
        public void HitBySmoke(SmokePuff smokePuff)
        {
            if (!isHitBySmoke)
            {
                // Causes the bee to fly away from the smoke puff
                Vector2 escapeVector = Bounds.Center.GetVector() - smokePuff.Bounds.Center.GetVector();
                escapeVector.Normalize();
                escapeVector *= random.Next(3, 6);

                velocity = escapeVector;
                
                isHitBySmoke = true;
            }
        }

        /// <summary>
        /// Sets the startup position for the bee.
        /// </summary>
        public virtual void SetStartupPosition()
        {
            if (relatedBeehive.AllowBeesToGenerate)
            {
                Rectangle rect = relatedBeehive.Bounds;
                position = new Vector2(rect.Center.X, rect.Center.Y);
                velocity = new Vector2(random.Next(-MaxVelocity * 100, MaxVelocity * 100) / 100,
                                       random.Next(-MaxVelocity * 100, MaxVelocity * 100) / 100);
                isHitBySmoke = false;
                timeToRegenerate = TimeSpan.Zero;
                timeHit = TimeSpan.Zero;
            }
        }

        /// <summary>
        /// Checks collision with a specified rectangle.
        /// </summary>
        /// <param name="bounds">Rectabgke with which to check for collisions.</param>
        public void Collide(Rectangle bounds)
        {
            // Check if this collision is new
            if (!isGotHit)
            {
                // Moves to new dircetion calculted by the "wall" that the bee collided 
                // with.
                velocityChangeTimer = TimeSpan.FromMilliseconds(-300);
                if (position.X < bounds.X || position.X > bounds.X + bounds.Width)
                {
                    velocity = new Vector2(velocity.X *= -1, velocity.Y);
                }
                else
                {
                    velocity = new Vector2(velocity.X, velocity.Y *= -1);
                }

                isGotHit = true;
            }
        }


        #endregion

        #region Private Methods


        /// <summary>
        /// Set a timer which will cause the be to regenerate when it expires.
        /// </summary>
        private void SetStartupPositionWithTimer()
        {
            timeToRegenerate = TimeSpan.FromMilliseconds(random.Next(3000, 5000));
        }

        /// <summary>
        /// This method handles a bee's regeneration.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        /// <returns>True if the bee has regenerated or no regeneration was necessary,
        /// false otherwise.</returns>
        private bool HandleRegeneration(GameTime gameTime)
        {
            // Checks if regeneration is needed
            if (timeToRegenerate != TimeSpan.Zero)
            {
                // Saves the time the bee was hit
                if (timeHit == TimeSpan.Zero)
                {
                    timeHit = gameTime.TotalGameTime;
                }

                // If enough time has pass, regenerate the bee
                if (timeToRegenerate + timeHit < gameTime.TotalGameTime)
                {
                    SetStartupPosition();
                }
                else
                {
                    position = new Vector2(-texture.Width, -texture.Height);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Alter the bee's movement randomly.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        private void SetRandomMovement(GameTime gameTime)
        {
            velocityChangeTimer += gameTime.ElapsedGameTime;
            if (velocityChangeTimer >= VelocityChangeInterval)
            {
                velocity = new Vector2(random.Next(-MaxVelocity * 100, MaxVelocity * 100) / 100,
                                        random.Next(-MaxVelocity * 100, MaxVelocity * 100) / 100);

                velocityChangeTimer = TimeSpan.Zero;

                if (isGotHit)
                {
                    isGotHit = false;
                }
            }
        }


        #endregion
    }
}
