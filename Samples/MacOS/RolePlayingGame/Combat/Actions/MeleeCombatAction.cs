#region File Description
//-----------------------------------------------------------------------------
// MeleeCombatAction.cs
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
    class MeleeCombatAction : CombatAction
    {
        #region State


        /// <summary>
        /// Returns true if the action is offensive, targeting the opponents.
        /// </summary>
        public override bool IsOffensive
        {
            get { return true; }
        }


        /// <summary>
        /// Returns true if this action requires a target.
        /// </summary>
        public override bool IsTargetNeeded
        {
            get { return true; }
        }


        #endregion


        #region Advancing/Returning Data


        /// <summary>
        /// The speed at which the advancing character moves, in units per second.
        /// </summary>
        private const float advanceSpeed = 300f;


        /// <summary>
        /// The offset from the advance destination to the target position
        /// </summary>
        private static readonly Vector2 advanceOffset = new Vector2(85f, 0f);


        /// <summary>
        /// The direction of the advancement.
        /// </summary>
        private Vector2 advanceDirection;


        /// <summary>
        /// The distance covered so far by the advance/return action
        /// </summary>
        private float advanceDistanceCovered = 0f;


        /// <summary>
        /// The total distance between the original combatant position and the target.
        /// </summary>
        private float totalAdvanceDistance;


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
                    {
                        // play the animation
                        combatant.CombatSprite.PlayAnimation("Idle");
                    }
                    break;

                case CombatActionStage.Advancing:
                    {
                        // play the animation
                        combatant.CombatSprite.PlayAnimation("Walk");
                        // calculate the advancing destination
                        if (Target.Position.X > Combatant.Position.X)
                        {
                            advanceDirection = Target.Position -
                                Combatant.OriginalPosition - advanceOffset;
                        }
                        else
                        {
                            advanceDirection = Target.Position -
                                Combatant.OriginalPosition + advanceOffset;
                        }
                        totalAdvanceDistance = advanceDirection.Length();
                        advanceDirection.Normalize();
                        advanceDistanceCovered = 0f;
                    }
                    break;

                case CombatActionStage.Executing:
                    {
                        // play the animation
                        combatant.CombatSprite.PlayAnimation("Attack");
                        // play the audio
                        Weapon weapon = combatant.Character.GetEquippedWeapon();
                        if (weapon != null)
                        {
                            AudioManager.PlayCue(weapon.SwingCueName);
                        }
                        else
                        {
                            AudioManager.PlayCue("StaffSwing");
                        }
                    }
                    break;

                case CombatActionStage.Returning:
                    {
                        // play the animation
                        combatant.CombatSprite.PlayAnimation("Walk");
                        // calculate the damage
                        Int32Range damageRange = combatant.Character.TargetDamageRange +
                            combatant.Statistics.PhysicalOffense;
                        Int32Range defenseRange = Target.Character.HealthDefenseRange + 
                            Target.Statistics.PhysicalDefense;
                        int damage = Math.Max(0, 
                            damageRange.GenerateValue(Session.Random) -
                            defenseRange.GenerateValue(Session.Random));
                        // apply the damage
                        if (damage > 0)
                        {
                            // play the audio
                            Weapon weapon = combatant.Character.GetEquippedWeapon();
                            if (weapon != null)
                            {
                                AudioManager.PlayCue(weapon.HitCueName);
                            }
                            else
                            {
                                AudioManager.PlayCue("StaffHit");
                            }
                            // damage the target
                            Target.DamageHealth(damage, 0);
                            if ((weapon != null) && (weapon.Overlay != null))
                            {
                                weapon.Overlay.PlayAnimation(0);
                                weapon.Overlay.ResetAnimation();
                            }
                        }
                    }
                    break;

                case CombatActionStage.Finishing:
                    {
                        // play the animation
                        combatant.CombatSprite.PlayAnimation("Idle");
                    }
                    break;

                case CombatActionStage.Complete:
                    {
                        // play the animation
                        combatant.CombatSprite.PlayAnimation("Idle");
                    }
                    break;
            }
        }


        /// <summary>
        /// Update the action for the current stage.
        /// </summary>
        /// <remarks>
        /// This function is guaranteed to be called at least once per stage.
        /// </remarks>
        protected override void UpdateCurrentStage(GameTime gameTime)
        {
            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            switch (stage)
            {
                case CombatActionStage.Advancing:
                    {
                        // move to the destination
                        if (advanceDistanceCovered < totalAdvanceDistance)
                        {
                            advanceDistanceCovered = Math.Min(advanceDistanceCovered +
                                advanceSpeed * elapsedSeconds, totalAdvanceDistance);
                        }
                        // update the combatant's position
                        combatant.Position = combatant.OriginalPosition +
                            advanceDirection * advanceDistanceCovered;
                    }
                    break;

                case CombatActionStage.Returning:
                    {
                        // move to the destination
                        if (advanceDistanceCovered > 0f)
                        {
                            advanceDistanceCovered -= advanceSpeed * elapsedSeconds;
                        }
                        combatant.Position = combatant.OriginalPosition +
                            advanceDirection * advanceDistanceCovered;
                    }
                    break;
            }
        }


        /// <summary>
        /// Returns true if the combat action is ready to proceed to the next stage.
        /// </summary>
        protected override bool IsReadyForNextStage
        {
            get
            {
                switch (stage)
                {
                    case CombatActionStage.Preparing: // ready to advance?
                        return true;

                    case CombatActionStage.Advancing: // ready to execute?
                        if (advanceDistanceCovered >= totalAdvanceDistance)
                        {
                            advanceDistanceCovered = totalAdvanceDistance;
                            combatant.Position = combatant.OriginalPosition +
                                advanceDirection * totalAdvanceDistance;
                            return true;
                        }
                        else
                        {
                            return false;
                        }

                    case CombatActionStage.Executing: // ready to return?
                        return combatant.CombatSprite.IsPlaybackComplete;

                    case CombatActionStage.Returning: // ready to finish?
                        if (advanceDistanceCovered <= 0f)
                        {
                            advanceDistanceCovered = 0f;
                            combatant.Position = combatant.OriginalPosition;
                            return true;
                        }
                        else
                        {
                            return false;
                        }

                    case CombatActionStage.Finishing: // ready to complete?
                        return true;
                }

                // fall through to the base behavior
                return base.IsReadyForNextStage;
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
                return combatant.Character.TargetDamageRange.Average;
            }
        }


        #endregion


        #region Initialization


        /// <summary>
        /// Constructs a new MeleeCombatAction object.
        /// </summary>
        /// <param name="character">The character performing the action.</param>
        public MeleeCombatAction(Combatant combatant)
            : base(combatant) { }

        
        #endregion


        #region Updating


        /// <summary>
        /// Updates the action over time.
        /// </summary>
        public override void Update(GameTime gameTime) 
        {
            // update the weapon animation
            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Weapon weapon = Combatant.Character.GetEquippedWeapon();
            if ((weapon != null) && (weapon.Overlay != null))
            {
                weapon.Overlay.UpdateAnimation(elapsedSeconds);
            }

            // update the action
            base.Update(gameTime);
        }


        #endregion


        #region Drawing


        /// <summary>
        /// Draw any elements of the action that are independent of the character.
        /// </summary>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // draw the weapon overlay (typically blood)
            Weapon weapon = Combatant.Character.GetEquippedWeapon();
            if ((weapon != null) && (weapon.Overlay != null) && 
                !weapon.Overlay.IsPlaybackComplete)
            {
                weapon.Overlay.Draw(spriteBatch, Target.Position, 0f);
            }

            base.Draw(gameTime, spriteBatch);
        }


        #endregion
    }
}
