#region File Description
//-----------------------------------------------------------------------------
// Quest.cs
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
    /// A quest that the party can embark on, with goals and rewards.
    /// </summary>
    public class Quest : ContentObject
#if WINDOWS
, ICloneable
#endif
    {
        #region Quest Stage


        /// <summary>
        /// The possible stages of a quest.
        /// </summary>
        public enum QuestStage
        {
            NotStarted,
            InProgress,
            RequirementsMet,
            Completed
        };

        /// <summary>
        /// The current stage of this quest.
        /// </summary>
        private QuestStage stage = QuestStage.NotStarted;

        /// <summary>
        /// The current stage of this quest.
        /// </summary>
        [ContentSerializerIgnore]
        public QuestStage Stage
        {
            get { return stage; }
            set { stage = value; }
        }
        

        #endregion


        #region Description


        /// <summary>
        /// The name of the quest.
        /// </summary>
        private string name;

        /// <summary>
        /// The name of the quest.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }


        /// <summary>
        /// A description of the quest.
        /// </summary>
        private string description;

        /// <summary>
        /// A description of the quest.
        /// </summary>
        public string Description
        {
            get { return description; }
            set { description = value; }
        }


        /// <summary>
        /// A message describing the objective of the quest, 
        /// presented when the player receives the quest.
        /// </summary>
        private string objectiveMessage;

        /// <summary>
        /// A message describing the objective of the quest, 
        /// presented when the player receives the quest.
        /// </summary>
        public string ObjectiveMessage
        {
            get { return objectiveMessage; }
            set { objectiveMessage = value; }
        }


        /// <summary>
        /// A message announcing the completion of the quest, 
        /// presented when the player reaches the goals of the quest.
        /// </summary>
        private string completionMessage;

        public string CompletionMessage
        {
            get { return completionMessage; }
            set { completionMessage = value; }
        }


        #endregion


        #region Requirements


        /// <summary>
        /// The gear that the player must have to finish the quest.
        /// </summary>
        private List<QuestRequirement<Gear>> gearRequirements =
            new List<QuestRequirement<Gear>>();

        /// <summary>
        /// The gear that the player must have to finish the quest.
        /// </summary>
        public List<QuestRequirement<Gear>> GearRequirements
        {
            get { return gearRequirements; }
            set { gearRequirements = value; }
        }


        /// <summary>
        /// The monsters that must be killed to finish the quest.
        /// </summary>
        private List<QuestRequirement<Monster>> monsterRequirements =
            new List<QuestRequirement<Monster>>();

        /// <summary>
        /// The monsters that must be killed to finish the quest.
        /// </summary>
        public List<QuestRequirement<Monster>> MonsterRequirements
        {
            get { return monsterRequirements; }
            set { monsterRequirements = value; }
        }


        /// <summary>
        /// Returns true if all requirements for this quest have been met.
        /// </summary>
        public bool AreRequirementsMet
        {
            get
            {
                foreach (QuestRequirement<Gear> gearRequirement in gearRequirements)
                {
                    if (gearRequirement.CompletedCount < gearRequirement.Count)
                    {
                        return false;
                    }
                }
                foreach (QuestRequirement<Monster> monsterRequirement 
                    in monsterRequirements)
                {
                    if (monsterRequirement.CompletedCount < monsterRequirement.Count)
                    {
                        return false;
                    }
                }
                return true;
            }
        }


        #endregion


        #region Quest Content


        /// <summary>
        /// The fixed combat encounters added to the world when this quest is active.
        /// </summary>
        private List<WorldEntry<FixedCombat>> fixedCombatEntries = 
            new List<WorldEntry<FixedCombat>>();

        /// <summary>
        /// The fixed combat encounters added to the world when this quest is active.
        /// </summary>
        public List<WorldEntry<FixedCombat>> FixedCombatEntries
        {
            get { return fixedCombatEntries; }
            set { fixedCombatEntries = value; }
        }


        /// <summary>
        /// The chests added to thew orld when this quest is active.
        /// </summary>
        private List<WorldEntry<Chest>> chestEntries = new List<WorldEntry<Chest>>();

        /// <summary>
        /// The chests added to thew orld when this quest is active.
        /// </summary>
        public List<WorldEntry<Chest>> ChestEntries
        {
            get { return chestEntries; }
            set { chestEntries = value; }
        }


        #endregion


        #region Destination


        /// <summary>
        /// The map with the destination Npc, if any.
        /// </summary>
        private string destinationMapContentName;

        /// <summary>
        /// The map with the destination Npc, if any.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public string DestinationMapContentName
        {
            get { return destinationMapContentName; }
            set { destinationMapContentName = value; }
        }


        /// <summary>
        /// The Npc that the party must visit to finish the quest, if any.
        /// </summary>
        private string destinationNpcContentName;

        /// <summary>
        /// The Npc that the party must visit to finish the quest, if any.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public string DestinationNpcContentName
        {
            get { return destinationNpcContentName; }
            set { destinationNpcContentName = value; }
        }


        /// <summary>
        /// The message shown when the party is eligible to complete the quest, if any.
        /// </summary>
        private string destinationObjectiveMessage;

        /// <summary>
        /// The message shown when the party is eligible to complete the quest, if any.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public string DestinationObjectiveMessage
        {
            get { return destinationObjectiveMessage; }
            set { destinationObjectiveMessage = value; }
        }


        #endregion


        #region Reward


        /// <summary>
        /// The number of experience points given to each party member as a reward.
        /// </summary>
        private int experienceReward;

        /// <summary>
        /// The number of experience points given to each party member as a reward.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public int ExperienceReward
        {
            get { return experienceReward; }
            set { experienceReward = value; }
        }


        /// <summary>
        /// The amount of gold given to the party as a reward.
        /// </summary>
        private int goldReward;

        /// <summary>
        /// The amount of gold given to the party as a reward.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public int GoldReward
        {
            get { return goldReward; }
            set { goldReward = value; }
        }


        /// <summary>
        /// The content names of the gear given to the party as a reward.
        /// </summary>
        private List<string> gearRewardContentNames = new List<string>();

        /// <summary>
        /// The content names of the gear given to the party as a reward.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public List<string> GearRewardContentNames
        {
            get { return gearRewardContentNames; }
            set { gearRewardContentNames = value; }
        }


        /// <summary>
        /// The gear given to the party as a reward.
        /// </summary>
        private List<Gear> gearRewards = new List<Gear>();

        /// <summary>
        /// The gear given to the party as a reward.
        /// </summary>
        [ContentSerializerIgnore]
        public List<Gear> GearRewards
        {
            get { return gearRewards; }
            set { gearRewards = value; }
        }


        #endregion


        #region Content Type Reader


        /// <summary>
        /// Reads a Quest object from the content pipeline.
        /// </summary>
        public class QuestReader : ContentTypeReader<Quest>
        {
            /// <summary>
            /// Reads a Quest object from the content pipeline.
            /// </summary>
            protected override Quest Read(ContentReader input, Quest existingInstance)
            {
                Quest quest = existingInstance;
                if (quest == null)
                {
                    quest = new Quest();
                }

                quest.AssetName = input.AssetName;

                quest.Name = input.ReadString();
                quest.Description = input.ReadString();
                quest.ObjectiveMessage = input.ReadString();
                quest.CompletionMessage = input.ReadString();

                quest.GearRequirements.AddRange(
                    input.ReadObject<List<QuestRequirement<Gear>>>());
                quest.MonsterRequirements.AddRange(
                    input.ReadObject<List<QuestRequirement<Monster>>>());

                // load the fixed combat entries
                Random random = new Random();
                quest.FixedCombatEntries.AddRange(
                    input.ReadObject<List<WorldEntry<FixedCombat>>>());
                foreach (WorldEntry<FixedCombat> fixedCombatEntry in
                    quest.FixedCombatEntries)
                {
                    fixedCombatEntry.Content =
                        input.ContentManager.Load<FixedCombat>(
                        System.IO.Path.Combine(@"Maps\FixedCombats",
                        fixedCombatEntry.ContentName));
                    // clone the map sprite in the entry, as there may be many entries
                    // per FixedCombat
                    fixedCombatEntry.MapSprite =
                        fixedCombatEntry.Content.Entries[0].Content.MapSprite.Clone()
                        as AnimatingSprite;
                    // play the idle animation
                    fixedCombatEntry.MapSprite.PlayAnimation("Idle",
                        fixedCombatEntry.Direction);
                    // advance in a random amount so the animations aren't synchronized
                    fixedCombatEntry.MapSprite.UpdateAnimation(
                        4f * (float)random.NextDouble());
                }

                quest.ChestEntries.AddRange(
                    input.ReadObject<List<WorldEntry<Chest>>>());
                foreach (WorldEntry<Chest> chestEntry in quest.ChestEntries)
                {
                    chestEntry.Content = input.ContentManager.Load<Chest>(
                        System.IO.Path.Combine(@"Maps\Chests",
                        chestEntry.ContentName)).Clone() as Chest;
                }

                quest.DestinationMapContentName = input.ReadString();
                quest.DestinationNpcContentName = input.ReadString();
                quest.DestinationObjectiveMessage = input.ReadString();

                quest.experienceReward = input.ReadInt32();
                quest.goldReward = input.ReadInt32();

                quest.GearRewardContentNames.AddRange(
                    input.ReadObject<List<string>>());
                foreach (string contentName in quest.GearRewardContentNames)
                {
                    quest.GearRewards.Add(input.ContentManager.Load<Gear>(
                        Path.Combine("Gear", contentName)));
                }

                return quest;
            }
        }


        #endregion


        #region ICloneable Members


        public object Clone()
        {
            Quest quest = new Quest();

            quest.AssetName = AssetName;
            foreach (WorldEntry<Chest> chestEntry in chestEntries)
            {
                WorldEntry<Chest> worldEntry = new WorldEntry<Chest>();
                worldEntry.Content = chestEntry.Content.Clone() as Chest;
                worldEntry.ContentName = chestEntry.ContentName;
                worldEntry.Count = chestEntry.Count;
                worldEntry.Direction = chestEntry.Direction;
                worldEntry.MapContentName = chestEntry.MapContentName;
                worldEntry.MapPosition = chestEntry.MapPosition;
                quest.chestEntries.Add(worldEntry);
            }
            quest.completionMessage = completionMessage;
            quest.description = description;
            quest.destinationMapContentName = destinationMapContentName;
            quest.destinationNpcContentName = destinationNpcContentName;
            quest.destinationObjectiveMessage = destinationObjectiveMessage;
            quest.experienceReward = experienceReward;
            quest.fixedCombatEntries.AddRange(fixedCombatEntries);
            quest.gearRequirements.AddRange(gearRequirements);
            quest.gearRewardContentNames.AddRange(gearRewardContentNames);
            quest.gearRewards.AddRange(gearRewards);
            quest.goldReward = goldReward;
            quest.monsterRequirements.AddRange(monsterRequirements);
            quest.name = name;
            quest.objectiveMessage = objectiveMessage;
            quest.stage = stage;

            return quest;
        }


        #endregion
    }
}
