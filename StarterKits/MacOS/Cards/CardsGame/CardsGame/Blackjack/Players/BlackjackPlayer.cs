#region File Description
//-----------------------------------------------------------------------------
// BlackjackPlayer.cs
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

#endregion



namespace Blackjack
{
    /// <summary>
    /// Depicts hands the player can interact with.
    /// </summary>
    public enum HandTypes
    {
        First,
        Second
    }

    public class BlackjackPlayer : Player
    {
        #region Fields/Properties
        // Various fields which depict the state of the players two hands
        private int firstValue;
        private bool firstValueConsiderAce;

        private int secondValue;
        private bool secondValueConsiderAce;

        public bool Bust { get; set; }
        public bool SecondBust { get; set; }
        public bool BlackJack { get; set; }
        public bool SecondBlackJack { get; set; }
        public bool Double { get; set; }
        public bool SecondDouble { get; set; }

        public bool IsSplit { get; set; }
        public Hand SecondHand { get; private set; }

        /// <summary>
        /// The type of hand that the player is currently interacting with.
        /// </summary>
        public HandTypes CurrentHandType { get; set; }

        /// <summary>
        /// Returns the hand that the player is currently interacting with.
        /// </summary>
        public Hand CurrentHand
        {
            get
            {
                switch (CurrentHandType)
                {
                    case HandTypes.First:
                        return Hand;
                    case HandTypes.Second:
                        return SecondHand;
                    default:
                        throw new Exception("No hand to return");
                }
            }
        }

        public int FirstValue
        {
            get { return firstValue; }
        }
        public bool FirstValueConsiderAce
        {
            get { return firstValueConsiderAce; }
        }

        public int SecondValue
        {
            get { return secondValue; }
        }
        public bool SecondValueConsiderAce
        {
            get { return secondValueConsiderAce; }
        }

        public bool MadeBet { get { return BetAmount > 0; } }
        public bool IsDoneBetting { get; set; }
        public float Balance { get; set; }
        public float BetAmount { get; private set; }
        public bool IsInsurance { get; set; }
        #endregion

        /// <summary>
        /// Creates a new blackjack player instance.
        /// </summary>
        /// <param name="name">The player's name.</param>
        /// <param name="game">The game associated with the player.</param>
        public BlackjackPlayer(string name, CardsFramework.CardsGame game)
            : base(name, game)
        {
            Balance = 500;
            CurrentHandType = HandTypes.First;
        }

        /// <summary>
        /// Calculates the value represented by a specified hand.
        /// </summary>
        /// <param name="hand">The hand for which to calculate the value.</param>
        /// <param name="game">The associated game.</param>
        /// <param name="value">Will contain the hand's value. If the hand has two
        /// possible values due to it containing an ace, this will be the lower
        /// value.</param>
        /// <param name="considerAce">Whether or not an ace can be considered to
        /// make the hand have an alternative value.</param>
        private static void CalulateValue(Hand hand, CardsFramework.CardsGame game,
            out int value, out bool considerAce)
        {
            value = 0;
            considerAce = false;

            for (int cardIndex = 0; cardIndex < hand.Count; cardIndex++)
            {
                value += game.CardValue(hand[cardIndex]);

                if (hand[cardIndex].Value == CardValue.Ace)
                {
                    considerAce = true;
                }
            }

            if (considerAce && value + 10 > 21)
            {
                considerAce = false;
            }
        }

        #region Public Methods
        /// <summary>
        /// Bets a specified amount of money, if the player's balance permits it.
        /// </summary>
        /// <param name="amount">The amount to bet.</param>
        /// <returns>True if the player has enough money to perform the bet, false
        /// otherwise.</returns>
        /// <remarks>The player's bet amount and balance are only updated if this
        /// method returns true.</remarks>
        public bool Bet(float amount)
        {
            if (amount > Balance)
            {
                return false;
            }
            BetAmount += amount;
            Balance -= amount;
            return true;
        }

        /// <summary>
        /// Resets the player's bet to 0, returning the current bet amount to 
        /// the player's balance.
        /// </summary>
        public void ClearBet()
        {
            Balance += BetAmount;
            BetAmount = 0;
        }

        /// <summary>
        /// Calculates the values of the player's two hands.
        /// </summary>
        public void CalculateValues()
        {
            CalulateValue(Hand, Game, out firstValue, out firstValueConsiderAce);

            if (SecondHand != null)
            {
                CalulateValue(SecondHand, Game, out secondValue,
                    out secondValueConsiderAce);
            }
        }

        /// <summary>
        /// Reset's the player's various state fields.
        /// </summary>
        public void ResetValues()
        {
            BlackJack = false;
            SecondBlackJack = false;
            Bust = false;
            SecondBust = false;
            Double = false;
            SecondDouble = false;
            firstValue = 0;
            firstValueConsiderAce = false;
            IsSplit = false;
            secondValue = 0;
            secondValueConsiderAce = false;
            BetAmount = 0;
            IsDoneBetting = false;
            IsInsurance = false;
            CurrentHandType = HandTypes.First;
        }

        /// <summary>
        /// Initializes the player's second hand.
        /// </summary>
        public void InitializeSecondHand()
        {
            SecondHand = new Hand();
        }

        /// <summary>
        /// Splits the player's current hand into two hands as per the blackjack rules.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if performing a split
        /// is not legal for the current player status.</exception>
        public void SplitHand()
        {
            if (SecondHand == null)
            {
                throw new InvalidOperationException("Second hand is not initialized.");
            }

            if (IsSplit == true)
            {
                throw new InvalidOperationException(
                    "A hand cannot be split more than once.");
            }

            if (Hand.Count != 2)
            {
                throw new InvalidOperationException(
                    "You must have two cards to perform a split.");
            }

            if (Hand[0].Value != Hand[1].Value)
            {
                throw new InvalidOperationException(
                    "You can only split when both cards are of identical value.");
            }

            IsSplit = true;

            // Move the top card in the first hand to the second hand
            Hand[1].MoveToHand(SecondHand);
        } 
        #endregion
    }
}
