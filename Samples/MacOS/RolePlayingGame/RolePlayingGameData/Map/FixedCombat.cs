#region File Description
//-----------------------------------------------------------------------------
// FixedCombat.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;
#endregion

namespace RolePlayingGameData
{
    /// <summary>
    /// The description of a fixed combat encounter in the world.
    /// </summary>
    public class FixedCombat : WorldObject
    {
        /// <summary>
        /// The content name and quantities of the monsters in this encounter.
        /// </summary>
        private List<ContentEntry<Monster>> entries = new List<ContentEntry<Monster>>();

        /// <summary>
        /// The content name and quantities of the monsters in this encounter.
        /// </summary>
        public List<ContentEntry<Monster>> Entries
        {
            get { return entries; }
            set { entries = value; }
        }


        #region Content Type Reader


        /// <summary>
        /// Reads a FixedCombat object from the content pipeline.
        /// </summary>
        public class FixedCombatReader : ContentTypeReader<FixedCombat>
        {
            /// <summary>
            /// Reads a FixedCombat object from the content pipeline.
            /// </summary>
            protected override FixedCombat Read(ContentReader input,
                FixedCombat existingInstance)
            {
                FixedCombat fixedCombat = existingInstance;
                if (fixedCombat == null)
                {
                    fixedCombat = new FixedCombat();
                }

                input.ReadRawObject<WorldObject>(fixedCombat as WorldObject);

                fixedCombat.Entries.AddRange(
                    input.ReadObject<List<ContentEntry<Monster>>>());
                foreach (ContentEntry<Monster> fixedCombatEntry in fixedCombat.Entries)
                {
                    fixedCombatEntry.Content = input.ContentManager.Load<Monster>(
                        Path.Combine(@"Characters\Monsters", 
                            fixedCombatEntry.ContentName));
                }

                return fixedCombat;
            }
        }


        #endregion
    }
}
