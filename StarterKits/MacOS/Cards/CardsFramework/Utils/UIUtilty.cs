#region File Description
//-----------------------------------------------------------------------------
// UIUtilty.cs
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
    public static class UIUtilty
    {
        /// <summary>
        /// Gets the name of a card asset.
        /// </summary>
        /// <param name="card">The card type for which to get the asset name.</param>
        /// <returns>The card's asset name.</returns>
        public static string GetCardAssetName(TraditionalCard card)
        {
            return string.Format("{0}{1}",
                ((card.Value | CardValue.FirstJoker) == 
                    CardValue.FirstJoker ||
                (card.Value | CardValue.SecondJoker) == 
                CardValue.SecondJoker) ?
                    "" : card.Type.ToString(), card.Value);
        }
    }
}
