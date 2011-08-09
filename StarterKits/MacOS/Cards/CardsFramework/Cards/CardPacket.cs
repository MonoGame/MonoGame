#region File Description
//-----------------------------------------------------------------------------
// CardsCollection.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;

using System.Text;
#endregion

namespace CardsFramework
{
    /// <summary>
    /// Card related <see cref="EventArgs"/> holding event information of a <see cref="TraditionalCard"/> 
    /// </summary>
    public class CardEventArgs : EventArgs
    {
        public TraditionalCard Card { get; set; }
    }

    /// <summary>
    /// A packet of cards
    /// </summary>
    /// <remarks>
    /// A card packet may be initialized with a collection of cards. 
    /// It may lose cards or deal them to <see cref="Hand"/>, but may
    /// not receive new cards unless derived and overridden.
    /// </remarks>
    public class CardPacket
    {
        #region Field Property Indexer
        protected List<TraditionalCard> cards { get; set; }

        /// <summary>
        /// An event which triggers when a card is removed from the collection.
        /// </summary>
        public event EventHandler<CardEventArgs> LostCard;

        public int Count { get { return cards.Count; } }

        /// <summary>
        /// Initializes a card collection by simply allocating a new card list.
        /// </summary>
        protected CardPacket()
        {
            cards = new List<TraditionalCard>();
        }

        /// <summary>
        /// Returns a card at a specified index in the collection.
        /// </summary>
        /// <param name="index">The card's index.</param>
        /// <returns>The card at the specified index.</returns>
        public TraditionalCard this[int index]
        {
            get
            {
                return cards[index];
            }
        }
        #endregion

        #region Initializations
        /// <summary>
        /// Initializes a new instance of the <see cref="CardPacket"/> class.
        /// </summary>
        /// <param name="numberOfDecks">The number of decks to add to 
        /// the collection.</param>
        /// <param name="jokersInDeck">The amount of jokers in each deck.</param>
        /// <param name="suits">The suits to add to each decks. Suits are specified 
        /// as flags and several can be added.</param>
        /// <param name="cardValues">The card values which will appear in each deck.
        /// values are specified as flags and several can be added.</param>
        public CardPacket(int numberOfDecks, int jokersInDeck,
            CardSuit suits, CardValue cardValues)
        {
            cards = new List<TraditionalCard>();

            for (int deckIndex = 0; deckIndex < numberOfDecks; deckIndex++)
            {
                AddSuit(suits, cardValues);

                for (int j = 0; j < jokersInDeck / 2; j++)
                {
                    cards.Add(new TraditionalCard(CardSuit.Club,
                        CardValue.FirstJoker, this));
                    cards.Add(new TraditionalCard(CardSuit.Club,
                        CardValue.SecondJoker, this));
                }

                if (jokersInDeck % 2 == 1)
                {
                    cards.Add(new TraditionalCard(CardSuit.Club,
                        CardValue.FirstJoker, this));
                }
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Adds suits of cards to the collection.
        /// </summary>
        /// <param name="suits">The suits to add to each decks. Suits are specified 
        /// as flags and several can be added.</param>
        /// <param name="cardValues">The card values which will appear in each deck.
        /// values are specified as flags and several can be added.</param>
        private void AddSuit(CardSuit suits, CardValue cardValues)
        {
            if ((suits & CardSuit.Club) == CardSuit.Club)
            {
                AddCards(CardSuit.Club, cardValues);
            }

            if ((suits & CardSuit.Diamond) == CardSuit.Diamond)
            {
                AddCards(CardSuit.Diamond, cardValues);
            }

            if ((suits & CardSuit.Heart) == CardSuit.Heart)
            {
                AddCards(CardSuit.Heart, cardValues);
            }

            if ((suits & CardSuit.Spade) == CardSuit.Spade)
            {
                AddCards(CardSuit.Spade, cardValues);
            }
        }

        /// <summary>
        /// Adds cards to the collection.
        /// </summary>
        /// <param name="suit">The suit of the added cards.</param>
        /// <param name="cardValues">The card values which will appear in each deck.
        /// values are specified as flags and several can be added.</param>
        private void AddCards(CardSuit suit,
            CardValue cardValues)
        {
            if ((cardValues & CardValue.Ace) == CardValue.Ace)
            {
                cards.Add(new TraditionalCard(suit, CardValue.Ace, this));
            }

            if ((cardValues & CardValue.Two) == CardValue.Two)
            {
                cards.Add(new TraditionalCard(suit, CardValue.Two, this));
            }

            if ((cardValues & CardValue.Three) == CardValue.Three)
            {
                cards.Add(new TraditionalCard(suit, CardValue.Three, this));
            }

            if ((cardValues & CardValue.Four) == CardValue.Four)
            {
                cards.Add(new TraditionalCard(suit, CardValue.Four, this));
            }

            if ((cardValues & CardValue.Five) == CardValue.Five)
            {
                cards.Add(new TraditionalCard(suit, CardValue.Five, this));
            }

            if ((cardValues & CardValue.Six) == CardValue.Six)
            {
                cards.Add(new TraditionalCard(suit, CardValue.Six, this));
            }

            if ((cardValues & CardValue.Seven) == CardValue.Seven)
            {
                cards.Add(new TraditionalCard(suit, CardValue.Seven, this));
            }

            if ((cardValues & CardValue.Eight) == CardValue.Eight)
            {
                cards.Add(new TraditionalCard(suit, CardValue.Eight, this));
            }

            if ((cardValues & CardValue.Nine) == CardValue.Nine)
            {
                cards.Add(new TraditionalCard(suit, CardValue.Nine, this));
            }

            if ((cardValues & CardValue.Ten) == CardValue.Ten)
            {
                cards.Add(new TraditionalCard(suit, CardValue.Ten, this));
            }

            if ((cardValues & CardValue.Jack) == CardValue.Jack)
            {
                cards.Add(new TraditionalCard(suit, CardValue.Jack, this));
            }

            if ((cardValues & CardValue.Queen) == CardValue.Queen)
            {
                cards.Add(new TraditionalCard(suit, CardValue.Queen, this));
            }

            if ((cardValues & CardValue.King) == CardValue.King)
            {
                cards.Add(new TraditionalCard(suit, CardValue.King, this));
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Shuffles the cards in the packet by randomly changing card placement.
        /// </summary>
        public void Shuffle()
        {
            Random random = new Random();
            List<TraditionalCard> shuffledDeck = new List<TraditionalCard>();

            while (cards.Count > 0)
            {
                TraditionalCard card = cards[random.Next(0, cards.Count)];
                cards.Remove(card);
                shuffledDeck.Add(card);
            }

            cards = shuffledDeck;
        }

        /// <summary>
        /// Removes the specified card from the packet. The first matching card
        /// will be removed.
        /// </summary>
        /// <param name="card">The card to remove.</param>
        /// <returns>The card that was removed from the collection.</returns>
        /// <remarks>
        /// Please note that removing a card from a packet may only be performed internally by
        /// other card-framework classes to maintain the principle that a card may only be held
        /// by one <see cref="CardPacket"/> only at any given time.
        /// </remarks>
        internal TraditionalCard Remove(TraditionalCard card)
        {
            if (cards.Contains(card))
            {
                cards.Remove(card);

                if (LostCard != null)
                {
                    LostCard(this, new CardEventArgs() { Card = card });
                }

                return card;
            }
            return null;
        }


        /// <summary>
        /// Removes all the cards from the collection.
        /// </summary>
        /// <returns>A list of all the cards that were removed.</returns>
        internal List<TraditionalCard> Remove()
        {
            List<TraditionalCard> cards = this.cards;
            this.cards = new List<TraditionalCard>();
            return cards;
        }

        /// <summary>
        /// Deals the first card from the collection to a specified hand.
        /// </summary>
        /// <param name="destinationHand">The destination hand.</param>
        /// <returns>The card that was moved to the hand.</returns>
        public TraditionalCard DealCardToHand(Hand destinationHand)
        {
            TraditionalCard firstCard = cards[0];

            firstCard.MoveToHand(destinationHand);

            return firstCard;
        }

        /// <summary>
        /// Deals several cards to a specified hand.
        /// </summary>
        /// <param name="destinationHand">The destination hand.</param>
        /// <param name="count">The amount of cards to deal.</param>
        /// <returns>A list of the cards that were moved to the hand.</returns>
        public List<TraditionalCard> DealCardsToHand(Hand destinationHand, int count)
        {
            List<TraditionalCard> dealtCards = new List<TraditionalCard>();

            for (int cardIndex = 0; cardIndex < count; cardIndex++)
            {
                dealtCards.Add(DealCardToHand(destinationHand));
            }

            return dealtCards;
        }
        #endregion
    }
}
