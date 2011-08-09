#region File Description
//-----------------------------------------------------------------------------
// AnimatedHandGameComponent.cs
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
#endregion

namespace CardsFramework
{
    public class AnimatedHandGameComponent : AnimatedGameComponent
    {
        #region Fields and Properties
        public int Place { get; private set; }
        public readonly Hand Hand;

        List<AnimatedCardsGameComponent> heldAnimatedCards = new List<AnimatedCardsGameComponent>();

        public override bool IsAnimating 
        { 
            get 
            {
                for (int animationIndex = 0; animationIndex < heldAnimatedCards.Count; animationIndex++)
                {
                    if (heldAnimatedCards[animationIndex].IsAnimating)
                    {
                        return true;
                    }
                }
                return false;
            } 
        }

        /// <summary>
        /// Returns the animated cards contained in the hand.
        /// </summary>
        public IEnumerable<AnimatedCardsGameComponent> AnimatedCards
        {
            get
            {
                return heldAnimatedCards.AsReadOnly();
            }
        } 
        #endregion

        #region Initiaizations
        /// <summary>
        /// Initializes a new instance of the animated hand component. This means
        /// setting the hand's position and initializing all animated cards and their
        /// respective positions. Also, registrations are performed to the associated
        /// <paramref name="hand"/> events to update the animated hand as cards are
        /// added or removed.
        /// </summary>
        /// <param name="place">The player's place index (-1 for the dealer).</param>
        /// <param name="hand">The hand represented by this instance.</param>
        /// <param name="cardGame">The associated card game.</param>
        public AnimatedHandGameComponent(int place, Hand hand, CardsGame cardGame)
            : base(cardGame, null)
        {
            Place = place;
            Hand = hand;
            hand.ReceivedCard += Hand_ReceivedCard;
            hand.LostCard += Hand_LostCard;

            // Set the component's position
            if (place == -1)
            {
                CurrentPosition = CardGame.GameTable.DealerPosition;
            }
            else
            {
                CurrentPosition = CardGame.GameTable[place];
            }

            // Create and initialize animated cards according to the cards in the 
            // associated hand
            for (int cardIndex = 0; cardIndex < hand.Count; cardIndex++)
            {
                AnimatedCardsGameComponent animatedCardGameComponent =
                    new AnimatedCardsGameComponent(hand[cardIndex], CardGame)
                    {
                        CurrentPosition = CurrentPosition + new Vector2(30 * cardIndex, 0)
                    };

                heldAnimatedCards.Add(animatedCardGameComponent);
                Game.Components.Add(animatedCardGameComponent);
            }

            Game.Components.ComponentRemoved += Components_ComponentRemoved;
        } 
        #endregion

        #region Update
        /// <summary>
        /// Updates the component.
        /// </summary>
        /// <param name="gameTime">The time which elapsed since this method was last
        /// called.</param>
        public override void Update(GameTime gameTime)
        {
            // Arrange the hand's animated cards' positions
            for (int animationIndex = 0; animationIndex < heldAnimatedCards.Count; animationIndex++)
            {
                if (!heldAnimatedCards[animationIndex].IsAnimating)
                {
                    heldAnimatedCards[animationIndex].CurrentPosition = CurrentPosition + 
                        GetCardRelativePosition(animationIndex);
                }
            }
            base.Update(gameTime);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets the card's offset from the hand position according to its index
        /// in the hand.
        /// </summary>
        /// <param name="cardLocationInHand">The card index in the hand.</param>
        /// <returns></returns>
        public virtual Vector2 GetCardRelativePosition(int cardLocationInHand)
        {
            return default(Vector2);
        }

        /// <summary>
        /// Finds the index of a specified card in the hand.
        /// </summary>
        /// <param name="card">The card to locate.</param>
        /// <returns>The card's index inside the hand, or -1 if it cannot be
        /// found.</returns>
        public int GetCardLocationInHand(TraditionalCard card)
        {
            for (int animationIndex = 0; animationIndex < heldAnimatedCards.Count; animationIndex++)
            {
                if (heldAnimatedCards[animationIndex].Card == card)
                {
                    return animationIndex;
                }
            }
            return -1;
        }

        /// <summary>
        /// Gets the animated game component associated with a specified card.
        /// </summary>
        /// <param name="card">The card for which to get the animation 
        /// component.</param>
        /// <returns>The card's animation component, or null if such a card cannot
        /// be found in the hand.</returns>
        public AnimatedCardsGameComponent GetCardGameComponent(TraditionalCard card)
        {
            int location = GetCardLocationInHand(card);
            if (location == -1)
                return null;

            return heldAnimatedCards[location];
        }

        /// <summary>
        /// Gets the animated game component associated with a specified card.
        /// </summary>
        /// <param name="location">The location where the desired card is 
        /// in the hand.</param>
        /// <returns>The card's animation component.</return>s 
        public AnimatedCardsGameComponent GetCardGameComponent(int location)
        {
            if (location == -1 || location >= heldAnimatedCards.Count)
                return null;

            return heldAnimatedCards[location];
        } 
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the ComponentRemoved event of the Components control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The 
        /// <see cref="Microsoft.Xna.Framework.GameComponentCollectionEventArgs"/> 
        /// instance containing the event data.</param>
        void Components_ComponentRemoved(object sender, GameComponentCollectionEventArgs e)
        {
            if (e.GameComponent == this)
            {
                Dispose();
            }
        }

        /// <summary>
        /// Handles the hand's LostCard event be removing the corresponding animated
        /// card.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The 
        /// <see cref="CardsFramework.CardEventArgs"/> 
        /// instance containing the event data.</param>
        void Hand_LostCard(object sender, CardEventArgs e)
        {
            // Remove the card from screen
            for (int animationIndex = 0; animationIndex < heldAnimatedCards.Count; animationIndex++)
            {
                if (heldAnimatedCards[animationIndex].Card == e.Card)
                {
                    Game.Components.Remove(heldAnimatedCards[animationIndex]);
                    heldAnimatedCards.RemoveAt(animationIndex);
                    return;
                }
            }
        }

        /// <summary>
        /// Handles the hand's ReceivedCard event be adding a corresponding 
        /// animated card.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The 
        /// <see cref="CardsFramework.CardEventArgs"/> 
        /// instance containing the event data.</param>
        void Hand_ReceivedCard(object sender, CardEventArgs e)
        {
            // Add the card to the screen
            AnimatedCardsGameComponent animatedCardGameComponent =
                new AnimatedCardsGameComponent(e.Card, CardGame) { Visible = false };

            heldAnimatedCards.Add(animatedCardGameComponent);
            Game.Components.Add(animatedCardGameComponent);
        }
        #endregion

        /// <summary>
        /// Calculate the estimated time at which the longest lasting animation currently managed 
        /// will complete.
        /// </summary>
        /// <returns>The estimated time for animation complete </returns>
        public override TimeSpan EstimatedTimeForAnimationsCompletion()
        {
            TimeSpan result = TimeSpan.Zero;

            if (IsAnimating)
            {
                for (int animationIndex = 0; animationIndex < heldAnimatedCards.Count; animationIndex++)
                {
                    if (heldAnimatedCards[animationIndex].EstimatedTimeForAnimationsCompletion() > result)
                    {
                        result = heldAnimatedCards[animationIndex].EstimatedTimeForAnimationsCompletion();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Properly disposes of the component when it is removed.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            // Remove the registrations to the event to make this 
            // instance collectable by gc
            Hand.ReceivedCard -= Hand_ReceivedCard;
            Hand.LostCard -= Hand_LostCard;

            base.Dispose(disposing);
        }
    }
}
