#region File Description
//-----------------------------------------------------------------------------
// TraditionalCard.cs
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
#endregion

namespace CardsFramework
{
    /// <summary>
    /// Enum defining the various types of cards for a traditional-western card-set
    /// </summary>
    [Flags]
    public enum CardSuit
    {
        Heart = 0x01,
        Diamond = 0x02,
        Club = 0x04,
        Spade = 0x08,
        // Sets:
        AllSuits = Heart | Diamond | Club | Spade
    }

    /// <summary>
    /// Enum defining the various types of card values for a traditional-western card-set
    /// </summary>
    [Flags]
    public enum CardValue
    {
        Ace = 0x01,
        Two = 0x02,
        Three = 0x04,
        Four = 0x08,
        Five = 0x10,
        Six = 0x20,
        Seven = 0x40,
        Eight = 0x80,
        Nine = 0x100,
        Ten = 0x200,
        Jack = 0x400,
        Queen = 0x800,
        King = 0x1000,
        FirstJoker = 0x2000,
        SecondJoker = 0x4000,
        // Sets:
        AllNumbers = 0x3FF,
        NonJokers = 0x1FFF,
        Jokers = FirstJoker | SecondJoker,
        AllFigures = Jack | Queen | King,
    }

    /// <summary>
    /// Traditional-western card
    /// </summary>
    /// <remarks>
    /// Each card has a defined <see cref="CardSuit">Type</see> and <see cref="CardValue">Value</see>
    /// as well as the <see cref="CardPacket"/> in which it is being held.
    /// A card may not be held in more than one <see cref="CardPacket"/>. This is achived by enforcing any card transfer
    /// operation between <see cref="CarkPacket"/>s and <see cref="Hand"/>s to be performed only from within the card's 
    /// <see cref="MoveToHand"/> method only. This method accesses <c>internal</c> <see cref="Hand.Add"/> method and 
    /// <see cref="CardPacket.Remove"/> method accordingly to complete the card transfer operation.
    /// </remarks>
    public class TraditionalCard
    {
        #region Properties
        public CardSuit Type { get; set; }
        public CardValue Value { get; set; }
        public CardPacket HoldingCardCollection; 
        #endregion

        #region Initiaizations
        /// <summary>
        /// Initializes a new instance of the <see cref="TraditionalCard"/> class.
        /// </summary>
        /// <param name="type">The card suit. Supports only a single value.</param>
        /// <param name="value">The card's value. Only single values are 
        /// supported.</param>
        /// <param name="holdingCardCollection">The holding card collection.</param>
        internal TraditionalCard(CardSuit type, CardValue value,
            CardPacket holdingCardCollection)
        {
            // Check for single type
            switch (type)
            {
                case CardSuit.Club:
                case CardSuit.Diamond:
                case CardSuit.Heart:
                case CardSuit.Spade:
                    break;
                default:
                    {
                        throw new ArgumentException(
                            "type must be single value", "type");
                    }
            }

            // Check for single value
            switch (value)
            {
                case CardValue.Ace:
                case CardValue.Two:
                case CardValue.Three:
                case CardValue.Four:
                case CardValue.Five:
                case CardValue.Six:
                case CardValue.Seven:
                case CardValue.Eight:
                case CardValue.Nine:
                case CardValue.Ten:
                case CardValue.Jack:
                case CardValue.Queen:
                case CardValue.King:
                case CardValue.FirstJoker:
                case CardValue.SecondJoker:
                    break;
                default:
                    {
                        throw new ArgumentException(
                            "value must be single value", "value");
                    }
            }

            Type = type;
            Value = value;
            HoldingCardCollection = holdingCardCollection;
        } 
        #endregion

        /// <summary>
        /// Moves the card from its current <see cref="CardPacket"/> to the specified <paramref name="hand"/>. 
        /// This method of operation prevents any one card instance from being held by more than one
        /// <see cref="CardPacket"/> at the same time.
        /// </summary>
        /// <param name="hand">The receiving hand.</param>
        public void MoveToHand(Hand hand)
        {
            HoldingCardCollection.Remove(this);
            HoldingCardCollection = hand;
            hand.Add(this);
        } 
    }
}
