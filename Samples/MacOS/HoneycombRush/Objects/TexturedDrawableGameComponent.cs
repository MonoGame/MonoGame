#region File Description
//-----------------------------------------------------------------------------
// TexturedDrawableGameComponent.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements


using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


#endregion


namespace HoneycombRush
{
    /// <summary>
    /// This abstract class represent a component which has a texture that represents it visually.
    /// </summary>
    public abstract class TexturedDrawableGameComponent : DrawableGameComponent
    {
        #region Fields/Properties


        protected ScaledSpriteBatch scaledSpriteBatch;
        protected Texture2D texture;
        protected Vector2 position;
        protected GameplayScreen gamePlayScreen;

        /// <summary>
        /// Represents the bounds of the component.
        /// </summary>
        public virtual Rectangle Bounds
        {
            get
            {
                if (texture == null)
                {
                    return default(Rectangle);
                }
                else
                {
                    return new Rectangle((int)position.X, (int)position.Y, 
                        (int)(texture.Width * scaledSpriteBatch.ScaleVector.X), 
                        (int)(texture.Height * scaledSpriteBatch.ScaleVector.Y));
                }
            }
        }

        /// <summary>
        /// Represents an area used for collision calculations.
        /// </summary>
        public virtual Rectangle CentralCollisionArea
        {
            get
            {
                return default(Rectangle);
            }
        }

        public Dictionary<string, ScaledAnimation> AnimationDefinitions { get; set; }


        #endregion


        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="game">Associated game object.</param>
        /// <param name="gamePlayScreen">Gameplay screen where the component will be presented.</param>
        public TexturedDrawableGameComponent(Game game, GameplayScreen gamePlayScreen)
            : base(game)
        {
            this.gamePlayScreen = gamePlayScreen;

            scaledSpriteBatch = (ScaledSpriteBatch)game.Services.GetService(typeof(ScaledSpriteBatch));
        }
    }
}
