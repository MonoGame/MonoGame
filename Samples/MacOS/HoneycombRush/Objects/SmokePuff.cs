#region File Description
//-----------------------------------------------------------------------------
// SmokePuff.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


#endregion

namespace HoneycombRush
{
    /// <summary>
    /// Represents a puff of smoke fired from the beekeeper's smoke gun.
    /// </summary>
    /// <remarks>Smoke puffs add and remove themselves from the list of game components as appropriate.</remarks>
    public class SmokePuff : TexturedDrawableGameComponent
    {
        #region Fields/Properties


        readonly TimeSpan growthTimeInterval = TimeSpan.FromMilliseconds(50);
        const float growthStep = 0.05f;

        TimeSpan lifeTime;
        TimeSpan growthTimeTrack;

        /// <summary>
        /// Used to scale the smoke puff
        /// </summary>
        float spreadFactor;
        Vector2 initialVelocity;
        Vector2 velocity;
        Vector2 acceleration;

        Vector2 drawOrigin;

        Random random = new Random();

        public bool IsGone
        {
            get
            {
                return lifeTime <= TimeSpan.Zero;
            }
        }

        bool isInGameComponents;

        /// <summary>
        /// Represents an area used for collision calculations.
        /// </summary>
        public override Rectangle CentralCollisionArea
        {
            get
            {
                int boundsWidth = (int)(texture.Width * spreadFactor * 1.5f * scaledSpriteBatch.ScaleVector.X);
                int boundsHeight = (int)(texture.Height * spreadFactor * 1.5f * scaledSpriteBatch.ScaleVector.Y);

                return new Rectangle((int)position.X - boundsWidth / 4, (int)position.Y - boundsHeight / 4,
                    boundsWidth, boundsHeight);
            }
        }


        #endregion

        #region Initialization


        /// <summary>
        /// Creates a new puff of smoke.
        /// </summary>
        /// <param name="game">Associated game object.</param>
        /// <param name="gameplayScreen">The gameplay screen where the smoke puff will be displayed.</param>
        /// <param name="texture">The texture which represents the smoke puff.</param>        
        public SmokePuff(Game game, GameplayScreen gameplayScreen, Texture2D texture) 
            : base(game, gameplayScreen)
        {
            this.texture = texture;

            drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);

            DrawOrder = Int32.MaxValue - 15;
        }

        /// <summary>
        /// Fires the smoke puff from a specified position and at a specified velocity. This also adds the smoke puff
        /// to the game component collection.
        /// </summary>
        /// <param name="origin">The position where the smoke puff should first appear.</param>
        /// <param name="initialVelocity">A vector indicating the initial velocity for this new smoke puff.</param>
        /// <remarks>The smoke puff's acceleration is internaly derived from 
        /// <paramref name="initialVelocity"/>.
        /// This method is not thread safe and calling it from another thread while the smoke puff expires (via
        /// its <see cref="Update"/> method) might have undesired effects.</remarks>
        public void Fire(Vector2 origin, Vector2 initialVelocity)
        {
            spreadFactor = 0.05f;

            lifeTime = TimeSpan.FromSeconds(5);
            growthTimeTrack = TimeSpan.Zero;

            position = origin;
            velocity = initialVelocity;
            this.initialVelocity = initialVelocity;
            initialVelocity.Normalize();

            acceleration = -(initialVelocity) * 6;

            if (!isInGameComponents)
            {
                Game.Components.Add(this);
                isInGameComponents = true;
            }
        }


        #endregion

        #region Update


        /// <summary>
        /// Performs update logic for the smoke puff. The smoke puff slows down while growing and eventually 
        /// evaporates.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        public override void Update(GameTime gameTime)
        {
            if (!gamePlayScreen.IsActive)
            {
                base.Update(gameTime);
                return;
            }

            lifeTime -= gameTime.ElapsedGameTime;

            // The smoke puff needs to vanish
            if (lifeTime <= TimeSpan.Zero)
            {
                Game.Components.Remove(this);
                isInGameComponents = false;
                base.Update(gameTime);
                return;
            }

            growthTimeTrack += gameTime.ElapsedGameTime;

            // See if it is time for the smoke puff to grow
            if ((spreadFactor < 1) && (growthTimeTrack >= growthTimeInterval))
            {
                growthTimeTrack = TimeSpan.Zero;
                spreadFactor += growthStep;
            }

            // Stop the smoke once it starts moving in the other direction
            if (Vector2.Dot(initialVelocity, velocity) > 0)
            {
                position += velocity;
                velocity += acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            base.Update(gameTime);
        }


        #endregion

        #region Render


        /// <summary>
        /// Draws the smoke puff.
        /// </summary>
        /// <param name="gameTime">Game time information.</param>
        public override void Draw(GameTime gameTime)
        {
            if (!gamePlayScreen.IsActive)
            {
                base.Draw(gameTime);
                return;
            }

            scaledSpriteBatch.Begin();

            Vector2 offset = GetRandomOffset();
            
            scaledSpriteBatch.Draw(texture, position + offset, null, Color.White, 0, drawOrigin, spreadFactor, 
                SpriteEffects.None, 0);

            scaledSpriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Used to make the smoke puff shift randomly.
        /// </summary>
        /// <returns>An offset which should be added to the smoke puff's position.</returns>
        private Vector2 GetRandomOffset()
        {
            return new Vector2(random.Next(2) - 4, random.Next(2) - 4);
        }


        #endregion
    }
}
