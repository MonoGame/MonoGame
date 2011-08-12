#region File Description
//-----------------------------------------------------------------------------
// SpellCombatAction.cs
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
using Microsoft.Xna.Framework.Audio;
#endregion

namespace RolePlaying
{
    /// <summary>
    /// A spell-casting combat action, including related data and calculations.
    /// </summary>
    class SpellCombatAction : CombatAction
    {
        #region State


        /// <summary>
        /// Returns true if the action is offensive, targeting the opponents.
        /// </summary>
        public override bool IsOffensive
        {
            get { return Spell.IsOffensive; }
        }

        
        /// <summary>
        /// Returns true if the character can use this action.
        /// </summary>
        public override bool IsCharacterValidUser
        {
            get
            {
                return (Spell.MagicPointCost <= Combatant.Statistics.MagicPoints);
            }
        }


        /// <summary>
        /// Returns true if this action requires a target.
        /// </summary>
        public override bool IsTargetNeeded
        {
            get { return true; }
        }


        #endregion


        #region Spell


        /// <summary>
        /// The spell used in this action.
        /// </summary>
        private Spell spell;

        /// <summary>
        /// The spell used in this action.
        /// </summary>
        public Spell Spell
        {
            get { return spell; }
        }


        /// <summary>
        /// The current position of the spell sprite.
        /// </summary>
        private Vector2 spellSpritePosition;


        /// <summary>
        /// Apply the action's spell to the given target.
        /// </summary>
        /// <returns>True if there was any effect on the target.</returns>
        private bool ApplySpell(Combatant spellTarget)
        {
            StatisticsValue effectStatistics = CalculateSpellDamage(combatant, spell);
            if (spell.IsOffensive)
            {
                // calculate the defense
                Int32Range defenseRange = spellTarget.Character.MagicDefenseRange + 
                    spellTarget.Statistics.MagicalDefense;
                Int32 defense = defenseRange.GenerateValue(Session.Random);
                // subtract the defense
                effectStatistics -= new StatisticsValue(defense,
                    defense, defense, defense, defense, defense);
                // make sure that this only contains damage
                effectStatistics.ApplyMinimum(new StatisticsValue());
                // damage the target
                spellTarget.Damage(effectStatistics, spell.TargetDuration);
            }
            else
            {
                // make sure that this only contains healing
                effectStatistics.ApplyMinimum(new StatisticsValue());
                // heal the target
                spellTarget.Heal(effectStatistics, spell.TargetDuration);
            }

            return !effectStatistics.IsZero;
        }


        #endregion


        #region Spell Projectile Data


        /// <summary>
        /// The speed at which the projectile moves, in units per second.
        /// </summary>
        private const float projectileSpeed = 600f;


        /// <summary>
        /// The direction of the projectile.
        /// </summary>
        private Vector2 projectileDirection;


        /// <summary>
        /// The distance covered so far by the projectile.
        /// </summary>
        private float projectileDistanceCovered = 0f;


        /// <summary>
        /// The total distance between the original combatant position and the target.
        /// </summary>
        private float totalProjectileDistance;


        /// <summary>
        /// The sprite effect on the projectile, if any.
        /// </summary>
        private SpriteEffects projectileSpriteEffect = SpriteEffects.None;


        /// <summary>
        /// The sound effect cue for the traveling projectile.
        /// </summary>
        private Cue projectileCue;


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
                        // play the animations
                        combatant.CombatSprite.PlayAnimation("SpellCast");
                        spellSpritePosition = Combatant.Position;
                        spell.SpellSprite.PlayAnimation("Creation");
                        // remove the magic points
                        Combatant.PayCostForSpell(spell);
                    }
                    break;

                case CombatActionStage.Advancing:
                    {
                        // play the animations
                        spell.SpellSprite.PlayAnimation("Traveling");
                        // calculate the projectile destination
                        projectileDirection = Target.Position -
                            Combatant.OriginalPosition;
                        totalProjectileDistance = projectileDirection.Length();
                        projectileDirection.Normalize();
                        projectileDistanceCovered = 0f;
                        // determine if the projectile is flipped
                        if (Target.Position.X > Combatant.Position.X)
                        {
                            projectileSpriteEffect = SpriteEffects.FlipHorizontally;
                        }
                        else
                        {
                            projectileSpriteEffect = SpriteEffects.None;
                        }
                        // get the projectile's cue and play it
                        projectileCue = AudioManager.GetCue(spell.TravelingCueName);
                        if (projectileCue != null)
                        {
                            projectileCue.Play();
                        }
                    }
                    break;

                case CombatActionStage.Executing:
                    {
                        // play the animation
                        spell.SpellSprite.PlayAnimation("Impact");
                        // stop the projectile sound effect
                        if (projectileCue != null)
                        {
                            projectileCue.Stop(AudioStopOptions.Immediate);
                        }
                        // apply the spell effect to the primary target
                        bool damagedAnyone = ApplySpell(Target);
                        // apply the spell to the secondary targets
                        foreach (Combatant targetCombatant in
                            CombatEngine.SecondaryTargetedCombatants)
                        {
                            // skip dead or dying targets
                            if (targetCombatant.IsDeadOrDying)
                            {
                                continue;
                            }
                            // apply the spell
                            damagedAnyone |= ApplySpell(targetCombatant);
                        }
                        // play the impact sound effect
                        if (damagedAnyone)
                        {
                            AudioManager.PlayCue(spell.ImpactCueName);
                            if (spell.Overlay != null)
                            {
                                spell.Overlay.PlayAnimation(0);
                                spell.Overlay.ResetAnimation();
                            }
                        }
                    }
                    break;

                case CombatActionStage.Returning:
                    // play the animation
                    combatant.CombatSprite.PlayAnimation("Idle");
                    break;

                case CombatActionStage.Finishing:
                    // play the animation
                    combatant.CombatSprite.PlayAnimation("Idle");
                    break;

                case CombatActionStage.Complete:
                    // play the animation
                    combatant.CombatSprite.PlayAnimation("Idle");
                    // make sure that the overlay has stopped
                    spell.Overlay.StopAnimation();
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
            switch (stage)
            {
                case CombatActionStage.Advancing:
                    if (projectileDistanceCovered < totalProjectileDistance)
                    {
                        projectileDistanceCovered += projectileSpeed *
                            (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                    spellSpritePosition = combatant.OriginalPosition +
                        projectileDirection * projectileDistanceCovered;
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
                        return (combatant.CombatSprite.IsPlaybackComplete &&
                            spell.SpellSprite.IsPlaybackComplete);

                    case CombatActionStage.Advancing: // ready to execute?
                        if (spell.SpellSprite.IsPlaybackComplete ||
                            (projectileDistanceCovered >= totalProjectileDistance))
                        {
                            projectileDistanceCovered = totalProjectileDistance;
                            return true;
                        }
                        return false;

                    case CombatActionStage.Executing: // ready to return?
                        return spell.SpellSprite.IsPlaybackComplete;
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
                return (Combatant.Statistics.MagicalOffense + 
                    Spell.TargetEffectRange.HealthPointsRange.Average);
            }
        }


        #endregion


        #region Initialization


        /// <summary>
        /// Constructs a new SpellCombatAction object.
        /// </summary>
        /// <param name="character">The combatant performing the action.</param>
        public SpellCombatAction(Combatant combatant, Spell spell)
            : base(combatant)
        {
            // check the parameter
            if (spell == null)
            {
                throw new ArgumentNullException("spell");
            }

            // assign the parameter
            this.spell = spell;
            this.adjacentTargets = this.spell.AdjacentTargets;
        }


        /// <summary>
        /// Start executing the combat action.
        /// </summary>
        public override void Start()
        {
            // play the creation sound effect
            AudioManager.PlayCue(spell.CreatingCueName);

            base.Start();
        }


        #endregion


        #region Updating


        /// <summary>
        /// Updates the action over time.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // update the animations
            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            spell.SpellSprite.UpdateAnimation(elapsedSeconds);
            if (spell.Overlay != null)
            {
                spell.Overlay.UpdateAnimation(elapsedSeconds);
                if (!spell.Overlay.IsPlaybackComplete &&
                    Target.CombatSprite.IsPlaybackComplete)
                {
                    spell.Overlay.StopAnimation();
                }
            }

            base.Update(gameTime);
        }


        #endregion


        #region Drawing


        /// <summary>
        /// Draw any elements of the action that are independent of the character.
        /// </summary>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // draw the spell projectile
            if (!spell.SpellSprite.IsPlaybackComplete)
            {
                if (stage == CombatActionStage.Advancing)
                {
                    spell.SpellSprite.Draw(spriteBatch, spellSpritePosition, 0f,
                        projectileSpriteEffect);
                }
                else
                {
                    spell.SpellSprite.Draw(spriteBatch, spellSpritePosition, 0f);
                }
            }

            // draw the spell overlay
            if (!spell.Overlay.IsPlaybackComplete)
            {
                spell.Overlay.Draw(spriteBatch, Target.Position, 0f);
            }

            base.Draw(gameTime, spriteBatch);
        }


        #endregion


        #region Static Calculation Methods


        /// <summary>
        /// Calculate the spell damage done by the given combatant and spell.
        /// </summary>
        public static StatisticsValue CalculateSpellDamage(Combatant combatant, 
            Spell spell)
        {
            // check the parameters
            if (combatant == null)
            {
                throw new ArgumentNullException("combatant");
            }
            if (spell == null)
            {
                throw new ArgumentNullException("spell");
            }

            // get the magical offense from the character's class, gear, and bonuses
            // -- note that this includes stat buffs
            int magicalOffense = combatant.Statistics.MagicalOffense;

            // add the magical offense to the spell
            StatisticsValue damage = 
                spell.TargetEffectRange.GenerateValue(Session.Random);
            damage.HealthPoints += (damage.HealthPoints != 0) ? magicalOffense : 0;
            damage.MagicPoints += (damage.MagicPoints != 0) ? magicalOffense : 0;
            damage.PhysicalOffense += (damage.PhysicalOffense != 0) ? magicalOffense : 0;
            damage.PhysicalDefense += (damage.PhysicalDefense != 0) ? magicalOffense : 0;
            damage.MagicalOffense += (damage.MagicalOffense != 0) ? magicalOffense : 0;
            damage.MagicalDefense += (damage.MagicalDefense != 0) ? magicalOffense : 0;

            // add in the spell damage
            return damage;
        }


        #endregion
    }
}
