#region File Description
//-----------------------------------------------------------------------------
// Spell.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace RolePlayingGameData
{
    public class Spell : ContentObject
#if WINDOWS
, ICloneable
#endif
    {
        #region Description Data


        /// <summary>
        /// The name of this spell.
        /// </summary>
        private string name;

        /// <summary>
        /// The name of this spell.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }


        /// <summary>
        /// The long description of this spell.
        /// </summary>
        private string description;

        /// <summary>
        /// The long description of this spell.
        /// </summary>
        public string Description
        {
            get { return description; }
            set { description = value; }
        }


        /// <summary>
        /// The cost, in magic points, to cast this spell.
        /// </summary>
        private int magicPointCost;

        /// <summary>
        /// The cost, in magic points, to cast this spell.
        /// </summary>
        public int MagicPointCost
        {
            get { return magicPointCost; }
            set { magicPointCost = value; }
        }


        /// <summary>
        /// Builds and returns a string describing the power of this spell.
        /// </summary>
        public virtual string GetPowerText()
        {
            return TargetEffectRange.GetModifierString();
        }


        #endregion


        #region Target Buff/Debuff Data


        /// <summary>
        /// If true, the statistics change are used as a debuff (subtracted).
        /// Otherwise, the statistics change is used as a buff (added).
        /// </summary>
        private bool isOffensive;

        /// <summary>
        /// If true, the statistics change are used as a debuff (subtracted).
        /// Otherwise, the statistics change is used as a buff (added).
        /// </summary>
        public bool IsOffensive
        {
            get { return isOffensive; }
            set { isOffensive = value; }
        }


        /// <summary>
        /// The duration of the effect of this spell on its target, in rounds.
        /// </summary>
        /// <remarks>
        /// If the duration is zero, then the effects last for the rest of the battle.
        /// </remarks>
        private int targetDuration;

        /// <summary>
        /// The duration of the effect of this spell on its target, in rounds.
        /// </summary>
        /// <remarks>
        /// If the duration is zero, then the effects last for the rest of the battle.
        /// </remarks>
        public int TargetDuration
        {
            get { return targetDuration; }
            set { targetDuration = value; }
        }


        /// <summary>
        /// The range of statistics effects of this spell on its target.
        /// </summary>
        /// <remarks>
        /// This is a debuff if IsOffensive is true, otherwise it's a buff.
        /// </remarks>
        private StatisticsRange targetEffectRange = new StatisticsRange();

        /// <summary>
        /// The range of statistics effects of this spell on its target.
        /// </summary>
        /// <remarks>
        /// This is a debuff if IsOffensive is true, otherwise it's a buff.
        /// </remarks>
        [ContentSerializerIgnore]
        public StatisticsRange TargetEffectRange
        {
            get { return targetEffectRange; }
        }


        /// <summary>
        /// The initial range of statistics effects of this spell on its target.
        /// </summary>
        /// <remarks>
        /// This is a debuff if IsOffensive is true, otherwise it's a buff.
        /// </remarks>
        private StatisticsRange initialTargetEffectRange = new StatisticsRange();

        /// <summary>
        /// The initial range of statistics effects of this spell on its target.
        /// </summary>
        /// <remarks>
        /// This is a debuff if IsOffensive is true, otherwise it's a buff.
        /// </remarks>
        public StatisticsRange InitialTargetEffectRange
        {
            get { return initialTargetEffectRange; }
            set { initialTargetEffectRange = value; }
        }


        /// <summary>
        /// The number of simultaneous, adjacent targets affected by this spell.
        /// </summary>
        private int adjacentTargets;

        /// <summary>
        /// The number of simultaneous, adjacent targets affected by this spell.
        /// </summary>
        public int AdjacentTargets
        {
            get { return adjacentTargets; }
            set { adjacentTargets = value; }
        }


        #endregion


        #region Spell Leveling


        /// <summary>
        /// The level of the spell.
        /// </summary>
        private int level = 1;

        /// <summary>
        /// The level of the spell.
        /// </summary>
        [ContentSerializerIgnore]
        public int Level
        {
            get { return level; }
            set 
            {
                level = value;
                targetEffectRange = initialTargetEffectRange;
                for (int i = 1; i < level; i++)
                {
                    targetEffectRange += LevelingProgression;
                }
            }
        }


        /// <summary>
        /// Defines how the spell improves as it levels up.
        /// </summary>
        private StatisticsValue levelingProgression = new StatisticsValue();

        /// <summary>
        /// Defines how the spell improves as it levels up.
        /// </summary>
        public StatisticsValue LevelingProgression
        {
            get { return levelingProgression; }
            set { levelingProgression = value; }
        }


        #endregion


        #region Sound Effects Data


        /// <summary>
        /// The name of the sound effect cue played when the spell is cast.
        /// </summary>
        private string creatingCueName;

        /// <summary>
        /// The name of the sound effect cue played when the spell is cast.
        /// </summary>
        public string CreatingCueName
        {
            get { return creatingCueName; }
            set { creatingCueName = value; }
        }


        /// <summary>
        /// The name of the sound effect cue played when the spell effect is traveling.
        /// </summary>
        private string travelingCueName;

        /// <summary>
        /// The name of the sound effect cue played when the spell effect is traveling.
        /// </summary>
        public string TravelingCueName
        {
            get { return travelingCueName; }
            set { travelingCueName = value; }
        }


        /// <summary>
        /// The name of the sound effect cue played when the spell affects its target.
        /// </summary>
        private string impactCueName;

        /// <summary>
        /// The name of the sound effect cue played when the spell affects its target.
        /// </summary>
        public string ImpactCueName
        {
            get { return impactCueName; }
            set { impactCueName = value; }
        }


        /// <summary>
        /// The name of the sound effect cue played when the spell effect is blocked.
        /// </summary>
        private string blockCueName;

        /// <summary>
        /// The name of the sound effect cue played when the spell effect is blocked.
        /// </summary>
        public string BlockCueName
        {
            get { return blockCueName; }
            set { blockCueName = value; }
        }


        #endregion


        #region Graphics Data


        /// <summary>
        /// The content path and name of the icon for this spell.
        /// </summary>
        private string iconTextureName;

        /// <summary>
        /// The content path and name of the icon for this spell.
        /// </summary>
        public string IconTextureName
        {
            get { return iconTextureName; }
            set { iconTextureName = value; }
        }


        /// <summary>
        /// The icon texture for this spell.
        /// </summary>
        private Texture2D iconTexture;

        /// <summary>
        /// The icon texture for this spell.
        /// </summary>
        [ContentSerializerIgnore]
        public Texture2D IconTexture
        {
            get { return iconTexture; }
        }
        
        
        /// <summary>
        /// The animating sprite used when this spell is in action.
        /// </summary>
        private AnimatingSprite spellSprite;

        /// <summary>
        /// The animating sprite used when this spell is in action.
        /// </summary>
        public AnimatingSprite SpellSprite
        {
            get { return spellSprite; }
            set { spellSprite = value; }
        }


        /// <summary>
        /// The overlay sprite for this spell.
        /// </summary>
        private AnimatingSprite overlay;

        /// <summary>
        /// The overlay sprite for this spell.
        /// </summary>
        public AnimatingSprite Overlay
        {
            get { return overlay; }
            set { overlay = value; }
        }


        #endregion


        #region Content Type Reader


        /// <summary>
        /// Read an Spell object from the content pipeline.
        /// </summary>
        public class SpellReader : ContentTypeReader<Spell>
        {
            /// <summary>
            /// Read an Spell object from the content pipeline.
            /// </summary>
            protected override Spell Read(ContentReader input, Spell existingInstance)
            {
                Spell spell = existingInstance;
                if (spell == null)
                {
                    spell = new Spell();
                }

                spell.AssetName = input.AssetName;

                spell.Name = input.ReadString();
                spell.Description = input.ReadString();
                spell.MagicPointCost = input.ReadInt32();
                spell.IconTextureName = input.ReadString();
                spell.iconTexture = input.ContentManager.Load<Texture2D>(
                    System.IO.Path.Combine(@"Textures\Spells", spell.IconTextureName));
                spell.IsOffensive = input.ReadBoolean();
                spell.TargetDuration = input.ReadInt32();
                spell.targetEffectRange = spell.InitialTargetEffectRange = 
                    input.ReadObject<StatisticsRange>();
                spell.AdjacentTargets = input.ReadInt32();
                spell.LevelingProgression = input.ReadObject<StatisticsValue>();
                spell.CreatingCueName = input.ReadString();
                spell.TravelingCueName = input.ReadString();
                spell.ImpactCueName = input.ReadString();
                spell.BlockCueName = input.ReadString();
                spell.SpellSprite = input.ReadObject<AnimatingSprite>();
                spell.SpellSprite.SourceOffset = new Vector2(
                    spell.SpellSprite.FrameDimensions.X / 2,
                    spell.SpellSprite.FrameDimensions.Y);
                spell.Overlay = input.ReadObject<AnimatingSprite>();
                spell.Overlay.SourceOffset = new Vector2(
                    spell.Overlay.FrameDimensions.X / 2, 
                    spell.Overlay.FrameDimensions.Y);

                spell.Level = 1;

                return spell;
            }
        }


        #endregion


        #region ICloneable Members


        public object Clone()
        {
            Spell spell = new Spell();

            spell.adjacentTargets = adjacentTargets;
            spell.AssetName = AssetName;
            spell.blockCueName = blockCueName;
            spell.creatingCueName = creatingCueName;
            spell.description = description;
            spell.iconTexture = iconTexture;
            spell.iconTextureName = iconTextureName;
            spell.impactCueName = impactCueName;
            spell.initialTargetEffectRange = initialTargetEffectRange;
            spell.isOffensive = isOffensive;
            spell.levelingProgression = levelingProgression;
            spell.magicPointCost = magicPointCost;
            spell.name = name;
            spell.overlay = overlay.Clone() as AnimatingSprite;
            spell.spellSprite = spellSprite.Clone() as AnimatingSprite;
            spell.targetDuration = targetDuration;
            spell.travelingCueName = travelingCueName;

            spell.Level = Level;

            return spell;
        }


        #endregion
    }
}
