#region File Description
//-----------------------------------------------------------------------------
// BlackjackGameEventArgs.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using CardsFramework;

#endregion



namespace Blackjack
{
    public class BlackjackGameEventArgs : EventArgs
    {
        public Player Player { get; set; }
        public HandTypes Hand { get; set; }
    }
}
