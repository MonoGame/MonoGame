#region File Description
//-----------------------------------------------------------------------------
// NpcScreen.cs
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
using Microsoft.Xna.Framework.Content;
using RolePlayingGameData;
#endregion

namespace RolePlaying
{
    /// <summary>
    /// Display of conversation dialog between the player and the npc
    /// </summary>
    abstract class NpcScreen<T> : DialogueScreen where T : Character
    {
        protected MapEntry<T> mapEntry = null;
        protected Character character = null;


        #region Initialization


        /// <summary>
        /// Create a new NpcScreen object.
        /// </summary>
        /// <param name="mapEntry"></param>
        public NpcScreen(MapEntry<T> mapEntry) : base()
        {
            if (mapEntry == null)
            {
                throw new ArgumentNullException("mapEntry");
            }
            this.mapEntry = mapEntry;
            this.character = mapEntry.Content as Character;
            if (this.character == null)
            {
                throw new ArgumentNullException(
                    "NpcScreen requires a MapEntry with a character.");
            }
            TitleText = character.Name;
        }


        #endregion
    }
}
