#region File Description
//-----------------------------------------------------------------------------
// ItemCombatAction.cs
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
    /// A item-casting combat action, including related data and calculations.
    /// </summary>
    class ItemCombatAction : CombatAction
    {
        #region State


        /// <summary>
        /// Returns true if the action is offensive, targeting the opponents.
        /// </summary>
        public override bool IsOffensive
        {
            get { return Item.IsOffensive; }
        }


        /// <summary>
        /// Returns true if the character can use this action.
        /// </summary>
        public override bool IsCharacterValidUser
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


        #region Item


        /// <summary>
        /// The item used in this action.
        /// </summary>
        private Item item;

        /// <summary>
        /// The item used in this action.
        /// </summary>
        public Item Item
        {
            get { return item; }
        }


        /// <summary>
        /// The current position of the item sprite.
        /// </summary>
        private Vector2 itemSpritePosition;


        /// <summary>
        /// Apply the action's item to the given target.
        /// </summary>
        /// <returns>True if there was any effect on the target.</returns>
        private bool ApplyItem(Combatant itemTarget)
        {
            StatisticsValue effectStatistics = CalculateItemDamage(combatant, item);
            if (item.IsOffensive)
            {
                // calculate the defense
                Int32Range defenseRange = itemTarget.Character.MagicDefenseRange +
                    itemTarget.Statistics.MagicalDefense;
                Int32 defense = defenseRange.GenerateValue(Session.Random);
                // subtract the defense
                effectStatistics -= new StatisticsValue(defense,
                    defense, defense, defense, defense, defense);
                // make sure that this only contains damage
                effectStatistics.ApplyMinimum(new StatisticsValue());
                // damage the target
                itemTarget.Damage(effectStatistics, item.TargetDuration);
            }
            else
            {
                // make sure taht this only contains healing
                effectStatistics.ApplyMinimum(new StatisticsValue());
                // heal the target
                itemTarget.Heal(effectStatistics, item.TargetDuration);
            }
            return !effectStatistics.IsZero;
        }
        

        #endregion


        #region Item Projectile Data


        /// <summary>
        /// The speed at which the projectile moves, in units per second.
        /// </summary>
        private const float projectileSpeed = 300f;


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
                        combatant.CombatSprite.PlayAnimation("ItemCast");
                        itemSpritePosition = Combatant.Position;
                        item.SpellSprite.PlayAnimation("Creation");
                        Session.Party.RemoveFromInventory(item, 1);
                    }
                    break;

                case CombatActionStage.Advancing:
                    {
                        // play the animations
                        item.SpellSprite.PlayAnimation("Traveling");
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
                        projectileCue = AudioManager.GetCue(item.TravelingCueName);
                        if (projectileCue != null)
                        {
                            projectileCue.Play();
                        }
                    }
                    break;

                case CombatActionStage.Executing:
                    // play the animation
                    item.SpellSprite.PlayAnimation("Impact");
                    // stop the projectile sound effect
                    if (projectileCue != null)
                    {
                        projectileCue.Stop(AudioStopOptions.Immediate);
                    }
                    // apply the item effect to the primary target
                    bool damagedAnyone = ApplyItem(Target);
                    // apply the item effect to the secondary targets
                    foreach (Combatant targetCombatant in
                        CombatEngine.SecondaryTargetedCombatants)
                    {
                        // skip any dead or dying combatants
                        if (targetCombatant.IsDeadOrDying)
                        {
                            continue;
                        }
                        // apply the effect
                        damagedAnyone |= ApplyItem(targetCombatant);
                    }
                    // play the impact sound effect
                    if (damagedAnyone)
                    {
                        AudioManager.PlayCue(item.ImpactCueName);
                        if (item.Overlay != null)
                        {
                            item.Overlay.PlayAnimation(0);
                            item.Overlay.ResetAnimation();
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
                    itemSpritePosition = combatant.OriginalPosition +
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
                            item.SpellSprite.IsPlaybackComplete);

                    case CombatActionStage.Advancing: // ready to execute?
                        if (item.SpellSprite.IsPlaybackComplete ||
                            (projectileDistanceCovered >= totalProjectileDistance))
                        {
                            projectileDistanceCovered = totalProjectileDistance;
                            return true;
                        }
                        return false;

                    case CombatActionStage.Executing: // ready to return?
                        return item.SpellSprite.IsPlaybackComplete;
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
                return Item.TargetEffectRange.HealthPointsRange.Average;
            }
        }


        #endregion


        #region Initialization


        /// <summary>
        /// Constructs a new ItemCombatAction object.
        /// </summary>
        /// <param name="character">The combatant performing the action.</param>
        public ItemCombatAction(Combatant combatant, Item item)
            : base(combatant)
        {
            // check the parameter
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            if ((item.Usage & Item.ItemUsage.Combat) == 0)
            {
                throw new ArgumentException("Combat items must have Combat usage.");
            }

            // assign the parameter
            this.item = item;
            this.adjacentTargets = this.item.AdjacentTargets;
        }


        /// <summary>
        /// Start executing the combat action.
        /// </summary>
        public override void Start()
        {
            // play the creation sound effect
            AudioManager.PlayCue(item.UsingCueName);

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
            item.SpellSprite.UpdateAnimation(elapsedSeconds);
            if (item.Overlay != null)
            {
                item.Overlay.UpdateAnimation(elapsedSeconds);
                if (!item.Overlay.IsPlaybackComplete &&
                    Target.CombatSprite.IsPlaybackComplete)
                {
                    item.Overlay.StopAnimation();
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
            // draw the item projectile
            if (!item.SpellSprite.IsPlaybackComplete)
            {
                if (stage == CombatActionStage.Advancing)
                {
                    item.SpellSprite.Draw(spriteBatch, itemSpritePosition, 0f,
                        projectileSpriteEffect);
                }
                else
                {
                    item.SpellSprite.Draw(spriteBatch, itemSpritePosition, 0f);
                }
            }

            // draw the item overlay
            if (!item.Overlay.IsPlaybackComplete)
            {
                item.Overlay.Draw(spriteBatch, Target.Position, 0f);
            }

            base.Draw(gameTime, spriteBatch);
        }


        #endregion


        #region Static Calculation Methods


        /// <summary>
        /// Calculate the item damage done by the given combatant and item.
        /// </summary>
        public static StatisticsValue CalculateItemDamage(Combatant combatant,
            Item item)
        {
            // check the parameters
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            // generate a new effect value - no stats are involved for items
            return item.TargetEffectRange.GenerateValue(Session.Random);
        }


        #endregion
    }
}
