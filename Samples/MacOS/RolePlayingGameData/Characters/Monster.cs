#region File Description
//-----------------------------------------------------------------------------
// Monster.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
#endregion

namespace RolePlayingGameData
{
    /// <summary>
    /// An enemy NPC that fights you in combat.
    /// </summary>
    /// <remarks>
    /// Any combat many have many of the same monster, and they don't exist beyond 
    /// combat.  Therefore, current statistics are tracked in the runtime combat engine.
    /// </remarks>
    public class Monster : FightingCharacter
    {
        #region Artificial Intelligence Parameters


        /// <summary>
        /// The chance that this monster will defend instead of attack.
        /// </summary>
        private int defendPercentage;

        /// <summary>
        /// The chance that this monster will defend instead of attack.
        /// </summary>
        public int DefendPercentage
        {
            get { return defendPercentage; }
            set { defendPercentage = (value > 100 ? 100 : (value < 0 ? 0 : value)); }
        }


        #endregion


        #region Combat Rewards


        /// <summary>
        /// The possible gear drops from this monster.
        /// </summary>
        private List<GearDrop> gearDrops = new List<GearDrop>();

        /// <summary>
        /// The possible gear drops from this monster.
        /// </summary>
        public List<GearDrop> GearDrops
        {
            get { return gearDrops; }
            set { gearDrops = value; }
        }


        public int CalculateGoldReward(Random random)
        {
            return CharacterClass.BaseGoldValue * CharacterLevel;
        }


        public int CalculateExperienceReward(Random random)
        {
            return CharacterClass.BaseExperienceValue * CharacterLevel;
        }


        public List<string> CalculateGearDrop(Random random)
        {
            List<string> gearRewards = new List<string>();

            Random useRandom = random;
            if (useRandom == null)
            {
                useRandom = new Random();
            }

            foreach (GearDrop gearDrop in GearDrops)
            {
                if (random.Next(100) < gearDrop.DropPercentage)
                {
                    gearRewards.Add(gearDrop.GearName);
                }
            }

            return gearRewards;
        }


        #endregion


        #region Content Type Reader


        /// <summary>
        /// Reads a Monster object from the content pipeline.
        /// </summary>
        public class MonsterReader : ContentTypeReader<Monster>
        {
            protected override Monster Read(ContentReader input, 
                Monster existingInstance)
            {
                Monster monster = existingInstance;
                if (monster == null)
                {
                    monster = new Monster();
                }

                input.ReadRawObject<FightingCharacter>(monster as FightingCharacter);

                monster.DefendPercentage = input.ReadInt32();
                monster.GearDrops.AddRange(input.ReadObject<List<GearDrop>>());

                return monster;
            }
        }


        #endregion
    }
}
