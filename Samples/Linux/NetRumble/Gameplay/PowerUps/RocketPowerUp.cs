#region File Description
//-----------------------------------------------------------------------------
// RocketPowerUp.cs
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
    /// A power-up that gives a player a rocket-launching weapon.
    /// </summary>
    public class RocketPowerUp : PowerUp
    {
        #region Static Graphics Data


        /// <summary>
        /// Texture for the rocket power-up.
        /// </summary>
        private static Texture2D texture;


        #endregion


        #region Initialization Methods


        /// <summary>
        /// Constructs a new rocket-launcher power-up.
        /// </summary>
        public RocketPowerUp()
            : base() { }


        #endregion


        #region Drawing Methods


        /// <summary>
        /// Draw the rocket power-up.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        /// <param name="spriteBatch">The SpriteBatch object used to draw.</param>
        public override void Draw(float elapsedTime, SpriteBatch spriteBatch)
        {
            // ignore the parameter color if we have an owner
            base.Draw(elapsedTime, spriteBatch, texture, null, Color.White);
        }


        #endregion


        #region Interaction Methods


        /// <summary>
        /// Defines the interaction between this power-up and a target GameplayObject
        /// when they touch.
        /// </summary>
        /// <param name="target">The GameplayObject that is touching this one.</param>
        /// <returns>True if the objects meaningfully interacted.</returns>
        public override bool Touch(GameplayObject target)
        {
            // if we hit a ship, give it the weapon
            Ship ship = target as Ship;
            if (ship != null)
            {
                ship.Weapon = new RocketWeapon(ship);
            }

            return base.Touch(target);
        }


        #endregion


        #region Static Graphics Methods


        /// <summary>
        /// Load all of the static graphics content for this class.
        /// </summary>
        /// <param name="contentManager">The content manager to load with.</param>
        public static void LoadContent(ContentManager contentManager)
        {
            // safety-check the parameters
            if (contentManager == null)
            {
                throw new ArgumentNullException("contentManager");
            }

            // load the texture
            texture = contentManager.Load<Texture2D>("Textures/powerupRocket");
        }


        /// <summary>
        /// Unload all of the static graphics content for this class.
        /// </summary>
        public static void UnloadContent()
        {
            texture = null;
        }


        #endregion
    }
}
