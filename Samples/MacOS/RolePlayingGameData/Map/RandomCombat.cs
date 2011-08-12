#region File Description
//-----------------------------------------------------------------------------
// RandomCombat.cs
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
    /// Description of possible random combats in a particular map.
    /// </summary>
    public class RandomCombat
    {
        /// <summary>
        /// The chance of a random combat starting with each step, from 1 to 100.
        /// </summary>
        private int combatProbability;

        /// <summary>
        /// The chance of a random combat starting with each step, from 1 to 100.
        /// </summary>
        public int CombatProbability
        {
            get { return combatProbability; }
            set { combatProbability = value; }
        }


        /// <summary>
        /// The chance of a successful escape from a random combat, from 1 to 100.
        /// </summary>
        private int fleeProbability;

        /// <summary>
        /// The chance of a successful escape from a random combat, from 1 to 100.
        /// </summary>
        public int FleeProbability
        {
            get { return fleeProbability; }
            set { fleeProbability = value; }
        }


        /// <summary>
        /// The range of possible quantities of monsters in the random encounter.
        /// </summary>
        private Int32Range monsterCountRange;

        /// <summary>
        /// The range of possible quantities of monsters in the random encounter.
        /// </summary>
        public Int32Range MonsterCountRange
        {
            get { return monsterCountRange; }
            set { monsterCountRange = value; }
        }


        /// <summary>
        /// The monsters that might be in the random encounter, 
        /// along with quantity and weight.
        /// </summary>
        private List<WeightedContentEntry<Monster>> entries =
            new List<WeightedContentEntry<Monster>>();

        /// <summary>
        /// The monsters that might be in the random encounter, 
        /// along with quantity and weight.
        /// </summary>
        public List<WeightedContentEntry<Monster>> Entries
        {
            get { return entries; }
            set { entries = value; }
        }


        #region Content Type Reader


        /// <summary>
        /// Reads a RandomCombat object from the content pipeline.
        /// </summary>
        public class RandomCombatReader : ContentTypeReader<RandomCombat>
        {
            protected override RandomCombat Read(ContentReader input,
                RandomCombat existingInstance)
            {
                RandomCombat randomCombat = existingInstance;
                if (randomCombat == null)
                {
                    randomCombat = new RandomCombat();
                }

                randomCombat.CombatProbability = input.ReadInt32();
                randomCombat.FleeProbability = input.ReadInt32();
                randomCombat.MonsterCountRange = input.ReadObject<Int32Range>();
                randomCombat.Entries.AddRange(
                    input.ReadObject<List<WeightedContentEntry<Monster>>>());
                foreach (ContentEntry<Monster> randomCombatEntry in randomCombat.Entries)
                {
                    randomCombatEntry.Content = input.ContentManager.Load<Monster>(
                        Path.Combine(@"Characters\Monsters",
                            randomCombatEntry.ContentName));
                }

                return randomCombat;
            }
        }


        #endregion
    }
}
