#region File Description
//-----------------------------------------------------------------------------
// Party.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Xna.Framework.Content;
using RolePlayingGameData;
#endregion

namespace RolePlaying
{
    /// <summary>
    /// The group of players, under control of the user.
    /// </summary>
    public class Party
    {
        #region Party Members


        /// <summary>
        /// The ordered list of players in the party.
        /// </summary>
        /// <remarks>The first entry is the leader.</remarks>
        private List<Player> players = new List<Player>();

        /// <summary>
        /// The ordered list of players in the party.
        /// </summary>
        /// <remarks>The first entry is the leader.</remarks>
        [ContentSerializerIgnore]
        public List<Player> Players
        {
            get { return players; }
            set { players = value; }
        }

        

        /// <summary>
        /// Add a new player to the party.
        /// </summary>
        /// <param name="player">The new player.</param>
        /// <remarks>Instead of using the lists directly, this fixes all data.</remarks>
        public void JoinParty(Player player)
        {
            // check the parameter
            if (player == null)
            {
                throw new ArgumentNullException("player");
            }
            if (players.Contains(player))
            {
                throw new ArgumentException("The player was already in the party.");
            }

            // add the new player to the list
            players.Add(player);

            // add the initial gold
            partyGold += player.Gold;
            // only as NPCs are players allowed to have their own gold
            player.Gold = 0;

            // add the player's inventory items
            foreach (ContentEntry<Gear> contentEntry in player.Inventory)
            {
                AddToInventory(contentEntry.Content, contentEntry.Count);
            }
            // only as NPCs are players allowed to have their own non-equipped gear
            player.Inventory.Clear();
        }


        /// <summary>
        /// Gives the experience amount specified to all party members.
        /// </summary>
        public void GiveExperience(int experience)
        {
            // check the parameters
            if (experience < 0)
            {
                throw new ArgumentOutOfRangeException("experience");
            }
            else if (experience == 0)
            {
                return;
            }

            List<Player> leveledUpPlayers = null;
            foreach (Player player in players)
            {
                int oldLevel = player.CharacterLevel;
                player.Experience += experience;
                if (player.CharacterLevel > oldLevel)
                {
                    if (leveledUpPlayers == null)
                    {
                        leveledUpPlayers = new List<Player>();
                    }
                    leveledUpPlayers.Add(player);
                }
            }

            if ((leveledUpPlayers != null) && (leveledUpPlayers.Count > 0))
            {
                Session.ScreenManager.AddScreen(new LevelUpScreen(leveledUpPlayers));
            }
        }


        #endregion


        #region Inventory


        /// <summary>
        /// The items held by the party.
        /// </summary>
        private List<ContentEntry<Gear>> inventory = new List<ContentEntry<Gear>>();

        /// <summary>
        /// The items held by the party.
        /// </summary>
        [ContentSerializerIgnore]
        public ReadOnlyCollection<ContentEntry<Gear>> Inventory
        {
            get { return inventory.AsReadOnly(); }
        }


        /// <summary>
        /// Add the given gear, in the given quantity, to the party's inventory.
        /// </summary>
        public void AddToInventory(Gear gear, int count)
        {
            // check the parameters
            if ((gear == null) || (count <= 0))
            {
                return;
            }

            // search for an existing entry
            ContentEntry<Gear> existingEntry = inventory.Find(
                delegate(ContentEntry<Gear> entry)
                {
                    return (entry.Content == gear);
                });
            // increment the existing entry, if any
            if (existingEntry != null)
            {
                existingEntry.Count += count;
                return;
            }

            // no existing entry - create a new entry
            ContentEntry<Gear> newEntry = new ContentEntry<Gear>();
            newEntry.Content = gear;
            newEntry.Count = count;
            newEntry.ContentName = gear.AssetName;
            if (newEntry.ContentName.StartsWith(@"Gear\"))
            {
                newEntry.ContentName = newEntry.ContentName.Substring(5);
            }
            inventory.Add(newEntry);
        }


        /// <summary>
        /// Remove the given quantity of the given gear from the party's inventory.
        /// </summary>
        /// <returns>True if the quantity specified could be removed.</returns>
        public bool RemoveFromInventory(Gear gear, int count)
        {
            // check the parameters
            if (gear == null)
            {
                throw new ArgumentNullException("gear");
            }
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            // search for an existing entry
            ContentEntry<Gear> existingEntry = inventory.Find(
                delegate(ContentEntry<Gear> entry)
                {
                    return (entry.Content == gear);
                });
            // no existing entry, so this is moot
            if (existingEntry == null)
            {
                return false;
            }

            // decrement the existing entry
            existingEntry.Count -= count;
            bool fullRemoval = (existingEntry.Count >= 0);

            // if the entry is empty, then remove it
            if (existingEntry.Count <= 0)
            {
                inventory.Remove(existingEntry);
            }

            return fullRemoval;
        }


        /// <summary>
        /// The gold held by the party.
        /// </summary>
        private int partyGold;

        /// <summary>
        /// The gold held by the party.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public int PartyGold
        {
            get { return partyGold; }
            set { partyGold = value; }
        }


        #endregion


        #region Quest Data

        
        /// <summary>
        /// The name and kill-count of monsters killed in the active quest.
        /// </summary>
        /// <remarks>
        /// Used to determine if the requirements for the active quest have been met.
        /// </remarks>
        private Dictionary<string, int> monsterKills = new Dictionary<string, int>();

        /// <summary>
        /// The name and kill-count of monsters killed in the active quest.
        /// </summary>
        /// <remarks>
        /// Used to determine if the requirements for the active quest have been met.
        /// </remarks>
        public Dictionary<string, int> MonsterKills
        {
            get { return monsterKills; }
        }


        /// <summary>
        /// Add a new monster-kill to the party's records.
        /// </summary>
        public void AddMonsterKill(Monster monster)
        {
            if (monsterKills.ContainsKey(monster.AssetName))
            {
                monsterKills[monster.AssetName]++;
            }
            else
            {
                monsterKills.Add(monster.AssetName, 1);
            }
        }


        #endregion


        #region Initialization


        /// <summary>
        /// Creates a new Party object from the game-start description.
        /// </summary>
        public Party(GameStartDescription gameStartDescription, 
            ContentManager contentManager)
        {
            // check the parameters
            if (gameStartDescription == null)
            {
                throw new ArgumentNullException("gameStartDescription");
            }
            if (contentManager == null)
            {
                throw new ArgumentNullException("contentManager");
            }

            // load the players
            foreach (string contentName in gameStartDescription.PlayerContentNames)
            {
                JoinParty(contentManager.Load<Player>(
                    Path.Combine(@"Characters\Players", contentName)).Clone()
                    as Player);
            }
        }


        /// <summary>
        /// Create a new Party object from serialized party data.
        /// </summary>
        public Party(PartySaveData partyData, ContentManager contentManager)
        {
            // check the parameters
            if (partyData == null)
            {
                throw new ArgumentNullException("partyData");
            }
            if (contentManager == null)
            {
                throw new ArgumentNullException("contentManager");
            }

            // load the players
            foreach (PlayerSaveData playerData in partyData.players)
            {
                Player player = 
                    contentManager.Load<Player>(playerData.assetName).Clone() as Player;
                player.CharacterLevel = playerData.characterLevel;
                player.Experience = playerData.experience;
                player.EquippedEquipment.Clear();
                foreach (string equipmentAssetName in playerData.equipmentAssetNames)
                {
                    player.Equip(contentManager.Load<Equipment>(equipmentAssetName));
                }
                player.StatisticsModifiers = playerData.statisticsModifiers;
                JoinParty(player);
            }

            // load the party inventory
            inventory.Clear();
            inventory.AddRange(partyData.inventory);
            foreach (ContentEntry<Gear> entry in inventory)
            {
                entry.Content = contentManager.Load<Gear>(
                    Path.Combine(@"Gear", entry.ContentName));
            }

            // set the party gold
            partyGold = partyData.partyGold;

            // load the monster kills
            for (int i = 0; i < partyData.monsterKillNames.Count; i++)
            {
                monsterKills.Add(partyData.monsterKillNames[i],
                    partyData.monsterKillCounts[i]);
            }            
        }


        #endregion
    }
}
