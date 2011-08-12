#region File Description
//-----------------------------------------------------------------------------
// GameTable.cs
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

namespace CardsFramework
{
    /// <summary>
    /// The UI representation of the table where the game is played.
    /// </summary>
    public class GameTable : DrawableGameComponent
    {
        #region Fields and Properties and Indexer
        public string Theme { get; private set; }
        public Texture2D TableTexture { get; private set; }
        public Vector2 DealerPosition { get; private set; }
        public SpriteBatch SpriteBatch { get; private set; }
        public Func<int, Vector2> PlaceOrder { get; private set; }
        public Rectangle TableBounds { get; private set; }
        public int Places { get; private set; }

        /// <summary>
        /// Returns the player position on the table according to the player index.
        /// </summary>
        /// <param name="index">Player's index.</param>
        /// <returns>The position of the player corrsponding to the 
        /// supplied index.</returns>
        /// <remarks>The location's are relative to the entire game area, even
        /// if the table only occupies part of it.</remarks>
        public Vector2 this[int index]
        {
            get
            {
                return new Vector2(TableBounds.Left, TableBounds.Top) +
                    PlaceOrder(index);
            }
        }
        #endregion

        #region Initiaizations
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="tableBounds">The table bounds.</param>
        /// <param name="dealerPosition">The dealer's position.</param>
        /// <param name="places">Amount of places on the table</param>
        /// <param name="placeOrder">A method to convert player indices to their
        /// respective location on the table.</param>
        /// <param name="theme">The theme used to display UI elements.</param>
        /// <param name="game">The associated game object.</param>
        public GameTable(Rectangle tableBounds, Vector2 dealerPosition, int places,
            Func<int, Vector2> placeOrder, string theme, Game game)
            : base(game)
        {
            TableBounds = tableBounds;
            DealerPosition = dealerPosition +
                new Vector2(tableBounds.Left, tableBounds.Top);
            Places = places;
            PlaceOrder = placeOrder;
            Theme = theme;
            SpriteBatch = new SpriteBatch(game.GraphicsDevice);
        }

        /// <summary>
        /// Load the table texture.
        /// </summary>
        protected override void LoadContent()
        {
            string assetName = string.Format(@"Images\UI\table");
            TableTexture = Game.Content.Load<Texture2D>(assetName);

            base.LoadContent();
        }
        #endregion

        #region Render
        /// <summary>
        /// Render the table.
        /// </summary>
        /// <param name="gameTime">Time passed since the last call to 
        /// this method.</param>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch.Begin();

            // Draw the table texture
            SpriteBatch.Draw(TableTexture, TableBounds, Color.White);

            SpriteBatch.End();

            base.Draw(gameTime);
        }
        #endregion
    }
}
