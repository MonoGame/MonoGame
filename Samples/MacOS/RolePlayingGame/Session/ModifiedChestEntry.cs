#region File Description
//-----------------------------------------------------------------------------
// ModifiedChestEntry.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using RolePlayingGameData;
#endregion

namespace RolePlaying
{
    /// <summary>
    /// A serializable description of the modified contents of a chest in the world.
    /// </summary>
    public class ModifiedChestEntry
    {
        /// <summary>
        /// The map and position of the modified chest.
        /// </summary>
        public WorldEntry<Chest> WorldEntry = new WorldEntry<Chest>();

        /// <summary>
        /// The modified chest contents, replacing the previous contents.
        /// </summary>
        public List<ContentEntry<Gear>> ChestEntries = new List<ContentEntry<Gear>>();

        /// <summary>
        /// The modified gold amount in the chest, replacing the previous amount.
        /// </summary>
        public int Gold = 0;
    }
}
