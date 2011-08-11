#region File Description
//-----------------------------------------------------------------------------
// CombatAction.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using RolePlayingGameData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace RolePlaying
{
    /// <summary>
    /// An action taken by an individual character in combat.
    /// </summary>
    /// <remarks>
    /// Note that party actions, like Flee, are not represented here.  These are only 
    /// the actions that individual characters, on either side, can perform in combat.
    /// </remarks>
    abstract class CombatAction
    {
        #region State


        /// <summary>
        /// Returns true if the action is offensive, targeting the opponents.
        /// </summary>
        public abstract bool IsOffensive
        {
            get;
        }


        /// <summary>
        /// Returns true if this action requires a target.
        /// </summary>
        public abstract bool IsTargetNeeded
        {
            get;
        }


        #endregion


        #region Combat Stage


        /// <summary>
        /// Stages of the action as it is executed.
        /// </summary>
        public enum CombatActionStage
        {
            /// <summary>
            /// The initial state, with no action taken yet.
            /// </summary>
            NotStarted,

            /// <summary>
            /// The action is getting ready to start.
            /// </summary>
            /// <example>
            /// Spell actions stay in this stage while the casting animation plays.
            /// </example>
            Preparing,

            /// <summary>
            /// The effect is traveling to the target, if needed.
            /// </summary>
            /// <example>
            /// The character walks to melee targets while in this stage.  Spell effects
            /// also travel to the target while in this state.
            /// </example>
            Advancing,


            /// <summary>
            /// The action is being applied to the target(s).
            /// </summary>
            Executing,

            /// <summary>
            /// The effect is returning from the target, if needed.
            /// </summary>
            /// <example>
            /// The character walks back from the melee target while in this stage.
            /// </example>
            Returning,

            /// <summary>
            /// The action is performing any final actions.
            /// </summary>
            Finishing,

            /// <summary>
            /// The action is complete.
            /// </summary>
            Complete,
        };


        /// <summary>
        /// The current state of the action.
        /// </summary>
        protected CombatActionStage stage = CombatActionStage.NotStarted;

        /// <summary>
        /// The current state of the action.
        /// </summary>
        public CombatActionStage Stage
        {
            get { return stage; }
        }


        /// <summary>
        /// Starts a new combat stage.  Called right after the stage changes.
        /// </summary>
        /// <remarks>The stage never changes into NotStarted.</remarks>
        protected virtual void StartStage()
        {
            switch (stage)
            {
                case CombatActionStage.Preparing: // called from Start()
                    break;

                case CombatActionStage.Advancing:
                    break;

                case CombatActionStage.Executing:
                    break;

                case CombatActionStage.Returning:
                    break;

                case CombatActionStage.Finishing:
                    break;

                case CombatActionStage.Complete:
                    break;
            }
        }


        /// <summary>
        /// Update the action for the current stage.
        /// </summary>
        /// <remarks>
        /// This function is guaranteed to be called at least once per stage.
        /// </remarks>
        protected virtual void UpdateCurrentStage(GameTime gameTime)
        {
            switch (stage)
            {
                case CombatActionStage.NotStarted:
                    break;

                case CombatActionStage.Preparing:
                    break;

                case CombatActionStage.Advancing:
                    break;

                case CombatActionStage.Executing:
                    break;

                case CombatActionStage.Returning:
                    break;

                case CombatActionStage.Finishing:
                    break;

                case CombatActionStage.Complete:
                    break;
            }
        }


        /// <summary>
        /// Returns true if the combat action is ready to proceed to the next stage.
        /// </summary>
        protected virtual bool IsReadyForNextStage
        {
            get
            {
                switch (stage)
                {
                    case CombatActionStage.Preparing: // ready to advance?
                        break;

                    case CombatActionStage.Advancing: // ready to execute?
                        break;

                    case CombatActionStage.Executing: // ready to return?
                        break;

                    case CombatActionStage.Returning: // ready to finish?
                        break;

                    case CombatActionStage.Finishing: // ready to complete?
                        break;
                }

                // fall through - the action doesn't care about the state, so move on
                return true;
            }
        }


        #endregion


        #region Combatant


        /// <summary>
        /// The character performing this action.
        /// </summary>
        protected Combatant combatant;

        /// <summary>
        /// The character performing this action.
        /// </summary>
        public Combatant Combatant
        {
            get { return combatant; }
        }


        /// <summary>
        /// Returns true if the character can use this action.
        /// </summary>
        public virtual bool IsCharacterValidUser
        {
            get { return true; }
        }


        #endregion


        #region Target


        /// <summary>
        /// The target of the action.
        /// </summary>
        public Combatant Target = null;


        /// <summary>
        /// The number of adjacent targets in each direction that are affected.
        /// </summary>
        protected int adjacentTargets = 0;

        /// <summary>
        /// The number of adjacent targets in each direction that are affected.
        /// </summary>
        public int AdjacentTargets
        {
            get { return adjacentTargets; }
        }


        #endregion


        #region Heuristic


        /// <summary>
        /// The heuristic used to compare actions of this type to similar ones.
        /// </summary>
        public abstract int Heuristic
        {
            get;
        }


        /// <summary>
        /// Compares the combat actions by their heuristic, in descending order.
        /// </summary>
        public static int CompareCombatActionsByHeuristic(
            CombatAction a, CombatAction b)
        {
            return b.Heuristic.CompareTo(a.Heuristic);
        }


        #endregion


        #region Initialization


        /// <summary>
        /// Constructs a new CombatAction object.
        /// </summary>
        /// <param name="combatant">The combatant performing the action.</param>
        public CombatAction(Combatant combatant)
        {
            // check the parameter
            if (combatant == null)
            {
                throw new ArgumentNullException("combatant");
            }

            // assign the parameter
            this.combatant = combatant;

            Reset();
        }


        /// <summary>
        /// Reset the action so that it may be started again.
        /// </summary>
        public virtual void Reset()
        {
            // set the state to not-started
            stage = CombatActionStage.NotStarted;
        }


        /// <summary>
        /// Start executing the combat action.
        /// </summary>
        public virtual void Start()
        {
            // set the state to the first step
            stage = CombatActionStage.Preparing;
            StartStage();
        }


        #endregion


        #region Updating


        /// <summary>
        /// Updates the action over time.
        /// </summary>
        public virtual void Update(GameTime gameTime)
        {
            // update the current stage
            UpdateCurrentStage(gameTime);

            // if the action is ready for the next stage, then advance
            if ((stage != CombatActionStage.NotStarted) &&
                (stage != CombatActionStage.Complete) && IsReadyForNextStage)
            {
                switch (stage)
                {
                    case CombatActionStage.Preparing:
                        stage = CombatActionStage.Advancing;
                        break;

                    case CombatActionStage.Advancing:
                        stage = CombatActionStage.Executing;
                        break;

                    case CombatActionStage.Executing:
                        stage = CombatActionStage.Returning;
                        break;

                    case CombatActionStage.Returning:
                        stage = CombatActionStage.Finishing;
                        break;

                    case CombatActionStage.Finishing:
                        stage = CombatActionStage.Complete;
                        break;
                }
                StartStage();
            }
        }


        #endregion


        #region Drawing


        /// <summary>
        /// Draw any elements of the action that are independent of the character.
        /// </summary>
        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch) { }


        #endregion
    }
}
