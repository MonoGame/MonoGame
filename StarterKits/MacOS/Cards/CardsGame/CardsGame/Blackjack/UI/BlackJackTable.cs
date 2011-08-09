#region File Description
//-----------------------------------------------------------------------------
// BlackJackTable.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using CardsFramework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Blackjack
{
    class BlackJackTable : GameTable
    {
        public Texture2D RingTexture { get; private set; }
        public Vector2 RingOffset { get; private set; }


        public BlackJackTable(Vector2 ringOffset, Rectangle tableBounds, Vector2 dealerPosition, int places,
            Func<int, Vector2> placeOrder, string theme, Game game)
            : base(tableBounds, dealerPosition, places, placeOrder, theme, game)
        {
            RingOffset = ringOffset;
        }

        /// <summary>
        /// Load the component assets
        /// </summary>
        protected override void LoadContent()
        {
            string assetName = string.Format(@"Images\UI\ring");
            RingTexture = Game.Content.Load<Texture2D>(assetName);

            base.LoadContent();
        }

        /// <summary>
        /// Draw the rings of the chip on the table
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            SpriteBatch.Begin();

            for (int placeIndex = 0; placeIndex < Places; placeIndex++)
            {
                SpriteBatch.Draw(RingTexture, PlaceOrder(placeIndex) + RingOffset, Color.White);
            }

            SpriteBatch.End();
        }
    }
}
