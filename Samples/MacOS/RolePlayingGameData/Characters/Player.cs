#region File Description
//-----------------------------------------------------------------------------
// Player.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace RolePlayingGameData
{
    /// <summary>
    /// A member of the player's party, also represented in the world before joining.
    /// </summary>
    /// <remarks>
    /// There is only one of a given Player in the game world at a time, and their 
    /// current statistics persist after combat.  Thererefore, current statistics 
    /// are tracked here.
    /// </remarks>
    public class Player : FightingCharacter
#if WINDOWS
, ICloneable
#endif
    {
        #region Current Statistics


        /// <summary>
        /// The current set of persistent statistics modifiers - damage, etc.
        /// </summary>
        [ContentSerializerIgnore]
        public StatisticsValue StatisticsModifiers = new StatisticsValue();


        /// <summary>
        /// The current set of statistics, including damage, etc.
        /// </summary>
        [ContentSerializerIgnore]
        public StatisticsValue CurrentStatistics
        {
            get { return CharacterStatistics + StatisticsModifiers; }
        }


        #endregion


        #region Initial Data


        /// <summary>
        /// The amount of gold that the player has when it joins the party.
        /// </summary>
        private int gold;

        /// <summary>
        /// The amount of gold that the player has when it joins the party.
        /// </summary>
        public int Gold
        {
            get { return gold; }
            set { gold = value; }
        }


        #endregion


        #region Dialogue Data


        /// <summary>
        /// The dialogue that the player says when it is greeted as an Npc in the world.
        /// </summary>
        private string introductionDialogue;

        /// <summary>
        /// The dialogue that the player says when it is greeted as an Npc in the world.
        /// </summary>
        public string IntroductionDialogue
        {
            get { return introductionDialogue; }
            set { introductionDialogue = value; }
        }


        /// <summary>
        /// The dialogue that the player says when its offer to join is accepted.
        /// </summary>
        private string joinAcceptedDialogue;

        /// <summary>
        /// The dialogue that the player says when its offer to join is accepted.
        /// </summary>
        public string JoinAcceptedDialogue
        {
            get { return joinAcceptedDialogue; }
            set { joinAcceptedDialogue = value; }
        }


        /// <summary>
        /// The dialogue that the player says when its offer to join is rejected.
        /// </summary>
        private string joinRejectedDialogue;

        /// <summary>
        /// The dialogue that the player says when its offer to join is rejected.
        /// </summary>
        public string JoinRejectedDialogue
        {
            get { return joinRejectedDialogue; }
            set { joinRejectedDialogue = value; }
        }


        #endregion


        #region Portrait Data


        /// <summary>
        /// The name of the active portrait texture.
        /// </summary>
        private string activePortraitTextureName;

        /// <summary>
        /// The name of the active portrait texture.
        /// </summary>
        public string ActivePortraitTextureName
        {
            get { return activePortraitTextureName; }
            set { activePortraitTextureName = value; }
        }


        /// <summary>
        /// The active portrait texture.
        /// </summary>
        private Texture2D activePortraitTexture;

        /// <summary>
        /// The active portrait texture.
        /// </summary>
        [ContentSerializerIgnore]
        public Texture2D ActivePortraitTexture
        {
            get { return activePortraitTexture; }
        }


        /// <summary>
        /// The name of the inactive portrait texture.
        /// </summary>
        private string inactivePortraitTextureName;

        /// <summary>
        /// The name of the inactive portrait texture.
        /// </summary>
        public string InactivePortraitTextureName
        {
            get { return inactivePortraitTextureName; }
            set { inactivePortraitTextureName = value; }
        }


        /// <summary>
        /// The inactive portrait texture.
        /// </summary>
        private Texture2D inactivePortraitTexture;

        /// <summary>
        /// The inactive portrait texture.
        /// </summary>
        [ContentSerializerIgnore]
        public Texture2D InactivePortraitTexture
        {
            get { return inactivePortraitTexture; }
        }


        /// <summary>
        /// The name of the unselectable portrait texture.
        /// </summary>
        private string unselectablePortraitTextureName;

        /// <summary>
        /// The name of the unselectable portrait texture.
        /// </summary>
        public string UnselectablePortraitTextureName
        {
            get { return unselectablePortraitTextureName; }
            set { unselectablePortraitTextureName = value; }
        }


        /// <summary>
        /// The unselectable portrait texture.
        /// </summary>
        private Texture2D unselectablePortraitTexture;

        /// <summary>
        /// The unselectable portrait texture.
        /// </summary>
        [ContentSerializerIgnore]
        public Texture2D UnselectablePortraitTexture
        {
            get { return unselectablePortraitTexture; }
        }


        #endregion


        #region Content Type Reader


        /// <summary>
        /// Read a Player object from the content pipeline.
        /// </summary>
        public class PlayerReader : ContentTypeReader<Player>
        {
            protected override Player Read(ContentReader input, Player existingInstance)
            {
                Player player = existingInstance;
                if (player == null)
                {
                    player = new Player();
                }

                input.ReadRawObject<FightingCharacter>(player as FightingCharacter);

                player.Gold = input.ReadInt32();
                player.IntroductionDialogue = input.ReadString();
                player.JoinAcceptedDialogue = input.ReadString();
                player.JoinRejectedDialogue = input.ReadString();
                player.ActivePortraitTextureName = input.ReadString();
                player.activePortraitTexture = 
                    input.ContentManager.Load<Texture2D>(
                        System.IO.Path.Combine(@"Textures\Characters\Portraits",
                        player.ActivePortraitTextureName));
                player.InactivePortraitTextureName = input.ReadString();
                player.inactivePortraitTexture = 
                    input.ContentManager.Load<Texture2D>(
                        System.IO.Path.Combine(@"Textures\Characters\Portraits",
                        player.InactivePortraitTextureName));
                player.UnselectablePortraitTextureName = input.ReadString();
                player.unselectablePortraitTexture = 
                    input.ContentManager.Load<Texture2D>(
                        System.IO.Path.Combine(@"Textures\Characters\Portraits",
                        player.UnselectablePortraitTextureName));

                return player;
            }
        }


        #endregion


        #region ICloneable Members


        public object Clone()
        {
            Player player = new Player();

            player.activePortraitTexture = activePortraitTexture;
            player.activePortraitTextureName = activePortraitTextureName;
            player.AssetName = AssetName;
            player.CharacterClass = CharacterClass;
            player.CharacterClassContentName = CharacterClassContentName;
            player.CharacterLevel = CharacterLevel;
            player.CombatAnimationInterval = CombatAnimationInterval;
            player.CombatSprite = CombatSprite.Clone() as AnimatingSprite;
            player.Direction = Direction;
            player.EquippedEquipment.AddRange(EquippedEquipment);
            player.Experience = Experience;
            player.gold = gold;
            player.inactivePortraitTexture = inactivePortraitTexture;
            player.inactivePortraitTextureName = inactivePortraitTextureName;
            player.InitialEquipmentContentNames.AddRange(InitialEquipmentContentNames);
            player.introductionDialogue = introductionDialogue;
            player.Inventory.AddRange(Inventory);
            player.joinAcceptedDialogue = joinAcceptedDialogue;
            player.joinRejectedDialogue = joinRejectedDialogue;
            player.MapIdleAnimationInterval = MapIdleAnimationInterval;
            player.MapPosition = MapPosition;
            player.MapSprite = MapSprite.Clone() as AnimatingSprite;
            player.MapWalkingAnimationInterval = MapWalkingAnimationInterval;
            player.Name = Name;
            player.ShadowTexture = ShadowTexture;
            player.State = State;
            player.unselectablePortraitTexture = unselectablePortraitTexture;
            player.unselectablePortraitTextureName = unselectablePortraitTextureName;
            player.WalkingSprite = WalkingSprite.Clone() as AnimatingSprite;

            player.RecalculateEquipmentStatistics();
            player.RecalculateTotalDefenseRanges();
            player.RecalculateTotalTargetDamageRange();
            player.ResetAnimation(false);
            player.ResetBaseStatistics();

            return player;
        }


        #endregion
    }
}
