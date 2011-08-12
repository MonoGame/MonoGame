#region File Description
//-----------------------------------------------------------------------------
// BlackjackRule.cs
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
    /// Represents a rule which checks if one of the player has achieved "blackjack".
    /// </summary>
    public class BlackJackRule : GameRule
    {
        List<BlackjackPlayer> players;

        /// <summary>
        /// Creates a new instance of the <see cref="BlackJackRule"/> class.
        /// </summary>
        /// <param name="players">A list of players participating in the game.</param>
        public BlackJackRule(List<Player> players)
        {
            this.players = new List<BlackjackPlayer>();
            for (int playerIndex = 0; playerIndex < players.Count; playerIndex++)
            {
                this.players.Add((BlackjackPlayer)players[playerIndex]);
            }
        }

        /// <summary>
        /// Check if any of the players has a hand value of 21 in any of their hands.
        /// </summary>
        public override void Check()
        {
            for (int playerIndex = 0; playerIndex < players.Count; playerIndex++)
            {
                players[playerIndex].CalculateValues();

                if (!players[playerIndex].BlackJack)
                {
                    // Check to see if the hand is eligible for a Black Jack
                    if (((players[playerIndex].FirstValue == 21) ||
                        (players[playerIndex].FirstValueConsiderAce &&
                        players[playerIndex].FirstValue + 10 == 21)) &&
                        players[playerIndex].Hand.Count == 2)
                    {
                        FireRuleMatch(new BlackjackGameEventArgs()
                        {
                            Player = players[playerIndex],
                            Hand = HandTypes.First
                        });
                    }
                }
                if (!players[playerIndex].SecondBlackJack)
                {
                    // Check to see if the hand is eligible for a Black Jack
                    // A Black Jack is only eligible with 2 cards in a hand                   
                    if ((players[playerIndex].IsSplit) && ((players[playerIndex].SecondValue == 21) ||
                        (players[playerIndex].SecondValueConsiderAce &&
                         players[playerIndex].SecondValue + 10 == 21)) &&
                         players[playerIndex].SecondHand.Count == 2)
                    {
                        FireRuleMatch(new BlackjackGameEventArgs()
                        {
                            Player = players[playerIndex],
                            Hand = HandTypes.Second
                        });
                    }
                }
            }
        }
    }
}
