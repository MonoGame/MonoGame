#region File Description
//-----------------------------------------------------------------------------
// CombatantMonster.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using RolePlayingGameData;
using Microsoft.Xna.Framework;
#endregion

namespace RolePlaying
{
    /// <summary>
    /// Encapsulates all of the combat-runtime data for a particular monster combatant.
    /// </summary>
    /// <remarks>
    /// There may be many of a particular Monster in combat.  This class adds the 
    /// statistics and AI data that are particular to this particular combatant.
    /// </remarks>
    class CombatantMonster : Combatant
    {
        /// <summary>
        /// The monster content object that this combatant uses.
        /// </summary>
        private Monster monster;

        /// <summary>
        /// The monster content object that this combatant uses.
        /// </summary>
        public Monster Monster
        {
            get { return monster; }
        }

        /// <summary>
        /// The character encapsulated by this combatant.
        /// </summary>
        public override FightingCharacter Character
        {
            get { return monster as FightingCharacter; }
        }
        
        
        #region State

        /// <summary>
        /// The current state of this combatant.
        /// </summary>
        private Character.CharacterState state;


        /// <summary>
        /// The current state of this combatant.
        /// </summary>
        public override Character.CharacterState State
        {
            get { return state; }
            set
            {
                if (value == state)
                {
                    return;
                }
                state = value;
                switch (state)
                {
                    case RolePlayingGameData.Character.CharacterState.Idle:
                        CombatSprite.PlayAnimation("Idle");
                        break;

                    case RolePlayingGameData.Character.CharacterState.Hit:
                        CombatSprite.PlayAnimation("Hit");
                        break;

                    case RolePlayingGameData.Character.CharacterState.Dying:
                        statistics.HealthPoints = 0;
                        CombatSprite.PlayAnimation("Die");
                        break;
                }
            }
        }


        #endregion


        #region Graphics Data


        /// <summary>
        /// The combat sprite for this combatant, copied from the monster.
        /// </summary>
        private AnimatingSprite combatSprite;

        /// <summary>
        /// Accessor for the combat sprite for this combatant.
        /// </summary>
        public override AnimatingSprite CombatSprite
        {
            get { return combatSprite; }
        }


        #endregion


        #region Current Statistics


        /// <summary>
        /// The statistics for this particular combatant.
        /// </summary>
        private StatisticsValue statistics = new StatisticsValue();

        /// <summary>
        /// The current statistics of this combatant.
        /// </summary>
        public override StatisticsValue Statistics
        {
            get { return statistics + CombatEffects.TotalStatistics; }
        }


        /// <summary>
        /// Heals the combatant by the given amount.
        /// </summary>
        public override void Heal(StatisticsValue healingStatistics, int duration)
        {
            if (duration > 0)
            {
                CombatEffects.AddStatistics(healingStatistics, duration);
            }
            else
            {
                statistics += healingStatistics;
                statistics.ApplyMaximum(monster.CharacterStatistics);
            }
            base.Heal(healingStatistics, duration);
        }


        /// <summary>
        /// Damages the combatant by the given amount.
        /// </summary>
        public override void Damage(StatisticsValue damageStatistics, int duration)
        {
            if (duration > 0)
            {
                CombatEffects.AddStatistics(new StatisticsValue() - damageStatistics, 
                    duration);
            }
            else
            {
                statistics -= damageStatistics;
                statistics.ApplyMaximum(monster.CharacterStatistics);
            }
            base.Damage(damageStatistics, duration);
        }

        
        /// <summary>
        /// Pay the cost for the given spell.
        /// </summary>
        /// <returns>True if the cost could be paid (and therefore was paid).</returns>
        public override bool PayCostForSpell(Spell spell)
        {
            // check the parameter.
            if (spell == null)
            {
                throw new ArgumentNullException("spell");
            }

            // check the requirements
            if (Statistics.MagicPoints < spell.MagicPointCost)
            {
                return false;
            }

            // reduce the player's magic points by the spell's cost
            statistics.MagicPoints -= spell.MagicPointCost;

            return true;
        }


        #endregion


        #region Artificial Intelligence


        /// <summary>
        /// The artificial intelligence data for this particular combatant.
        /// </summary>
        private ArtificialIntelligence artificialIntelligence;

        /// <summary>
        /// The artificial intelligence data for this particular combatant.
        /// </summary>
        public ArtificialIntelligence ArtificialIntelligence
        {
            get { return artificialIntelligence; }
        }


        #endregion


        #region Initialization


        /// <summary>
        /// Create a new CombatMonster object containing the given monster.
        /// </summary>
        /// <param name="monster"></param>
        public CombatantMonster(Monster monster) : base()
        {
            // check the parameter
            if (monster == null)
            {
                throw new ArgumentNullException("monster");
            }

            // assign the parameters
            this.monster = monster;
            this.statistics += monster.CharacterStatistics;
            this.combatSprite = monster.CombatSprite.Clone() as AnimatingSprite;
            this.State = RolePlayingGameData.Character.CharacterState.Idle;
            this.CombatSprite.PlayAnimation("Idle");

            // create the AI data
            this.artificialIntelligence = new ArtificialIntelligence(this);
        }


        #endregion


        #region Updating


        /// <summary>
        /// Update the monster for this frame.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // start any waiting action immediately
            if ((CombatAction != null) &&
                (CombatAction.Stage == CombatAction.CombatActionStage.NotStarted))
            {
                CombatAction.Start();
            }

            base.Update(gameTime);
        }


        #endregion
    }
}
