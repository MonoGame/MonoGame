#region File Description
//-----------------------------------------------------------------------------
// DefendCombatAction.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using RolePlayingGameData;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace RolePlaying
{
    /// <summary>
    /// A melee-attack combat action, including related data and calculations.
    /// </summary>
    class DefendCombatAction : CombatAction
    {
        #region State


        /// <summary>
        /// Returns true if the action is offensive, targeting the opponents.
        /// </summary>
        public override bool IsOffensive
        {
            get { return false; }
        }


        /// <summary>
        /// Returns true if this action requires a target.
        /// </summary>
        public override bool  IsTargetNeeded
        {
            get { return false; }
        }


        #endregion


        #region Combat Stage


        /// <summary>
        /// Starts a new combat stage.  Called right after the stage changes.
        /// </summary>
        /// <remarks>The stage never changes into NotStarted.</remarks>
        protected override void StartStage()
        {
            switch (stage)
            {
                case CombatActionStage.Preparing: // called from Start()
                    Combatant.CombatSprite.PlayAnimation("Defend");
                    break;

                case CombatActionStage.Executing:
                    Combatant.CombatEffects.AddStatistics(new StatisticsValue(
                        0, 0, 0, Combatant.Character.CharacterStatistics.PhysicalDefense,
                        0, Combatant.Character.CharacterStatistics.MagicalDefense), 1);
                    break;
            }
        }


        #endregion


        #region Heuristic


        /// <summary>
        /// The heuristic used to compare actions of this type to similar ones.
        /// </summary>
        public override int Heuristic
        {
            get 
            {
                return 0;
            }
        }


        #endregion


        #region Initialization


        /// <summary>
        /// Constructs a new DefendCombatAction object.
        /// </summary>
        /// <param name="character">The character performing the action.</param>
        public DefendCombatAction(Combatant combatant)
            : base(combatant) { }

        
        #endregion
    }
}
