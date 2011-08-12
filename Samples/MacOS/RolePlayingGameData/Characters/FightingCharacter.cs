#region File Description
//-----------------------------------------------------------------------------
// FightingCharacter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
#endregion

namespace RolePlayingGameData
{
    /// <summary>
    /// A character that engages in combat.
    /// </summary>
    public abstract class FightingCharacter : Character
    {
        #region Class Data


        /// <summary>
        /// The name of the character class.
        /// </summary>
        private string characterClassContentName;

        /// <summary>
        /// The name of the character class.
        /// </summary>
        public string CharacterClassContentName
        {
            get { return characterClassContentName; }
            set { characterClassContentName = value; }
        }


        /// <summary>
        /// The character class itself.
        /// </summary>
        private CharacterClass characterClass;

        /// <summary>
        /// The character class itself.
        /// </summary>
        [ContentSerializerIgnore]
        public CharacterClass CharacterClass
        {
            get { return characterClass; }
            set 
            {
                characterClass = value;
                ResetBaseStatistics();
            }
        }


        /// <summary>
        /// The level of the character.
        /// </summary>
        private int characterLevel = 1;

        /// <summary>
        /// The level of the character.
        /// </summary>
        public int CharacterLevel
        {
            get { return characterLevel; }
            set 
            { 
                characterLevel = value;
                ResetBaseStatistics();
                spells = null;
            }
        }


        /// <summary>
        /// Returns true if the character is at the maximum level allowed by their class.
        /// </summary>
        public bool IsMaximumCharacterLevel
        {
            get
            {
                return characterLevel >= characterClass.LevelEntries.Count;
            }
        }
        

        /// <summary>
        /// The cached list of spells for this level.
        /// </summary>
        private List<Spell> spells = null;

        /// <summary>
        /// The cached list of spells for this level.
        /// </summary>
        [ContentSerializerIgnore]
        public List<Spell> Spells
        {
            get 
            { 
                if ((spells == null) && (characterClass != null))
                {
                    spells = characterClass.GetAllSpellsForLevel(characterLevel);
                }
                return spells;
            }
        }



        #endregion


        #region Experience


        /// <summary>
        /// The amount of experience points that this character has.
        /// </summary>
        private int experience;

        /// <summary>
        /// The amount of experience points that this character has.
        /// </summary>
        [ContentSerializerIgnore]
        public int Experience
        {
            get { return experience; }
            set 
            { 
                experience = value;
                while (experience >= ExperienceForNextLevel)
                {
                    if (IsMaximumCharacterLevel)
                    {
                        break;
                    }
                    experience -= ExperienceForNextLevel;
                    CharacterLevel++;
                }
            }
        }


        /// <summary>
        /// Returns the amount of experience necessary to reach the next character level.
        /// </summary>
        public int ExperienceForNextLevel
        {
            get
            {
                int checkIndex = Math.Min(characterLevel,
                    characterClass.LevelEntries.Count) - 1;
                return characterClass.LevelEntries[checkIndex].ExperiencePoints;
            }
        }


        #endregion


        #region Statistics
        

        /// <summary>
        /// The base statistics of this character, from the character class and level.
        /// </summary>
        private StatisticsValue baseStatistics = new StatisticsValue();

        /// <summary>
        /// The base statistics of this character, from the character class and level.
        /// </summary>
        [ContentSerializerIgnore]
        public StatisticsValue BaseStatistics
        {
            get { return baseStatistics; }
            set { baseStatistics = value; }
        }


        /// <summary>
        /// Reset the character's base statistics.
        /// </summary>
        public void ResetBaseStatistics()
        {
            if (characterClass == null)
            {
                baseStatistics = new StatisticsValue();
            }
            else
            {
                baseStatistics = characterClass.GetStatisticsForLevel(characterLevel);
            }
        }


        /// <summary>
        /// The total statistics for this character.
        /// </summary>
        [ContentSerializerIgnore]
        public StatisticsValue CharacterStatistics
        {
            get { return baseStatistics + equipmentBuffStatistics; }
        }


        #endregion


        #region Equipment


        /// <summary>
        /// The equipment currently equipped on this character.
        /// </summary>
        private List<Equipment> equippedEquipment = new List<Equipment>();

        /// <summary>
        /// The equipment currently equipped on this character.
        /// </summary>
        [ContentSerializerIgnore]
        public List<Equipment> EquippedEquipment
        {
            get { return equippedEquipment; }
        }


        /// <summary>
        /// The content names of the equipment initially equipped on the character.
        /// </summary>
        private List<string> initialEquipmentContentNames = new List<string>();

        /// <summary>
        /// The content names of the equipment initially equipped on the character.
        /// </summary>
        public List<string> InitialEquipmentContentNames
        {
            get { return initialEquipmentContentNames; }
            set { initialEquipmentContentNames = value; }
        }


        /// <summary>
        /// Retrieve the currently equipped weapon.
        /// </summary>
        /// <remarks>There can only be one weapon equipped at the same time.</remarks>
        public Weapon GetEquippedWeapon()
        {
            return equippedEquipment.Find(delegate(Equipment equipment)
                { return equipment is Weapon; }) as Weapon;
        }


        /// <summary>
        /// Equip a new weapon.
        /// </summary>
        /// <returns>True if the weapon was equipped.</returns>
        public bool EquipWeapon(Weapon weapon, out Equipment oldEquipment)
        {
            // check the parameter
            if (weapon == null)
            {
                throw new ArgumentNullException("weapon");
            }

            // check equipment restrictions
            if (!weapon.CheckRestrictions(this))
            {
                oldEquipment = null;
                return false;
            }

            // unequip any existing weapon
            Weapon existingWeapon = GetEquippedWeapon();
            if (existingWeapon != null)
            {
                oldEquipment = existingWeapon;
                equippedEquipment.Remove(existingWeapon);
            }
            else
            {
                oldEquipment = null;
            }

            // add the weapon
            equippedEquipment.Add(weapon);

            // recalculate the statistic changes from equipment
            RecalculateEquipmentStatistics();
            RecalculateTotalTargetDamageRange();

            return true;
        }


        /// <summary>
        /// Remove any equipped weapons.
        /// </summary>
        public void UnequipWeapon()
        {
            equippedEquipment.RemoveAll(delegate(Equipment equipment)
                { return equipment is Weapon; });
            RecalculateEquipmentStatistics();
        }


        /// <summary>
        /// Retrieve the armor equipped in the given slot.
        /// </summary>
        public Armor GetEquippedArmor(Armor.ArmorSlot slot)
        {
            return equippedEquipment.Find(delegate(Equipment equipment)
                {
                    Armor armor = equipment as Armor;
                    return ((armor != null) && (armor.Slot == slot));
                }) as Armor;
        }


        /// <summary>
        /// Equip a new piece of armor.
        /// </summary>
        /// <returns>True if the armor could be equipped.</returns>
        public bool EquipArmor(Armor armor, out Equipment oldEquipment)
        {
            // check the parameter
            if (armor == null)
            {
                throw new ArgumentNullException("armor");
            }

            // check equipment requirements
            if (!armor.CheckRestrictions(this))
            {
                oldEquipment = null;
                return false;
            }

            // remove any armor equipped in this slot
            Armor equippedArmor = GetEquippedArmor(armor.Slot);
            if (equippedArmor != null)
            {
                oldEquipment = equippedArmor;
                equippedEquipment.Remove(equippedArmor);
            }
            else
            {
                oldEquipment = null;
            }

            // add the armor
            equippedEquipment.Add(armor);

            // recalcuate the total armor defense values
            RecalculateTotalDefenseRanges();

            // recalculate the statistics buffs from equipment
            RecalculateEquipmentStatistics();

            return true;        
        }


        /// <summary>
        /// Unequip any armor in the given slot.
        /// </summary>
        public void UnequipArmor(Armor.ArmorSlot slot)
        {
            equippedEquipment.RemoveAll(delegate(Equipment equipment)
                {
                    Armor armor = equipment as Armor;
                    return ((armor != null) && (armor.Slot == slot));
                });
            RecalculateEquipmentStatistics();
            RecalculateTotalDefenseRanges();
        }



        /// <summary>
        /// Equip a new piece of equipment.
        /// </summary>
        /// <returns>True if the equipment could be equipped.</returns>
        public virtual bool Equip(Equipment equipment)
        {
            Equipment oldEquipment;

            return Equip(equipment, out oldEquipment);
        }

            
        /// <summary>
        /// Equip a new piece of equipment, specifying any equipment auto-unequipped.
        /// </summary>
        /// <returns>True if the equipment could be equipped.</returns>
        public virtual bool Equip(Equipment equipment, out Equipment oldEquipment)
        {
            if (equipment == null)
            {
                throw new ArgumentNullException("equipment");
            }

            if (equipment is Weapon)
            {
                return EquipWeapon(equipment as Weapon, out oldEquipment);
            }
            else if (equipment is Armor)
            {
                return EquipArmor(equipment as Armor, out oldEquipment);
            }
            else
            {
                oldEquipment = null;
            }

            return false;
        }


        /// <summary>
        /// Unequip a piece of equipment.
        /// </summary>
        /// <returns>True if the equipment could be unequipped.</returns>
        public virtual bool Unequip(Equipment equipment)
        {
            if (equipment == null)
            {
                throw new ArgumentNullException("equipment");
            }

            if (equippedEquipment.Remove(equipment))
            {
                RecalculateEquipmentStatistics();
                RecalculateTotalTargetDamageRange();
                RecalculateTotalDefenseRanges();
                return true;
            }

            return false;
        }


        #endregion


        #region Combined Equipment Values


        /// <summary>
        /// The total statistics changes (buffs) from all equipped equipment.
        /// </summary>
        private StatisticsValue equipmentBuffStatistics = new StatisticsValue();

        /// <summary>
        /// The total statistics changes (buffs) from all equipped equipment.
        /// </summary>
        [ContentSerializerIgnore]
        public StatisticsValue EquipmentBuffStatistics
        {
            get { return equipmentBuffStatistics; }
            set { equipmentBuffStatistics = value; }
        }


        /// <summary>
        /// Recalculate the character's equipment-buff statistics.
        /// </summary>
        public void RecalculateEquipmentStatistics()
        {
            // start from scratch
            equipmentBuffStatistics = new StatisticsValue();

            // add the statistics for each piece of equipped equipment
            foreach (Equipment equipment in equippedEquipment)
            {
                equipmentBuffStatistics += equipment.OwnerBuffStatistics;
            }
        }


        /// <summary>
        /// The target damage range for this character, aggregated from all weapons.
        /// </summary>
        private Int32Range targetDamageRange;

        /// <summary>
        /// The health damage range for this character, aggregated from all weapons.
        /// </summary>
        public Int32Range TargetDamageRange
        {
            get { return targetDamageRange; }
        }

        /// <summary>
        /// Recalculate the character's defense ranges from all of their armor.
        /// </summary>
        public void RecalculateTotalTargetDamageRange()
        {
            // set the initial damage range to the physical offense statistic
            targetDamageRange = new Int32Range();

            // add each weapon's target damage range
            foreach (Equipment equipment in equippedEquipment)
            {
                Weapon weapon = equipment as Weapon;
                if (weapon != null)
                {
                    targetDamageRange += weapon.TargetDamageRange;
                }
            }
        }


        /// <summary>
        /// The health defense range for this character, aggregated from all armor.
        /// </summary>
        private Int32Range healthDefenseRange;

        /// <summary>
        /// The health defense range for this character, aggregated from all armor.
        /// </summary>
        public Int32Range HealthDefenseRange
        {
            get { return healthDefenseRange; }
        }


        /// <summary>
        /// The magic defense range for this character, aggregated from all armor.
        /// </summary>
        private Int32Range magicDefenseRange;

        /// <summary>
        /// The magic defense range for this character, aggregated from all armor.
        /// </summary>
        public Int32Range MagicDefenseRange
        {
            get { return magicDefenseRange; }
        }


        /// <summary>
        /// Recalculate the character's defense ranges from all of their armor.
        /// </summary>
        public void RecalculateTotalDefenseRanges()
        {
            // set the initial damage ranges based on character statistics
            healthDefenseRange = new Int32Range();
            magicDefenseRange = new Int32Range();

            // add the defense ranges for each piece of equipped armor
            foreach (Equipment equipment in equippedEquipment)
            {
                Armor armor = equipment as Armor;
                if (armor != null)
                {
                    healthDefenseRange += armor.OwnerHealthDefenseRange;
                    magicDefenseRange += armor.OwnerMagicDefenseRange;
                }
            }
        }


        #endregion


        #region Inventory


        /// <summary>
        /// The gear in this character's inventory (and not equipped).
        /// </summary>
        private List<ContentEntry<Gear>> inventory = new List<ContentEntry<Gear>>();

        /// <summary>
        /// The gear in this character's inventory (and not equipped).
        /// </summary>
        public List<ContentEntry<Gear>> Inventory
        {
            get { return inventory; }
        }


        #endregion


        #region Graphics Data


        /// <summary>
        /// The animating sprite for the combat view of this character.
        /// </summary>
        private AnimatingSprite combatSprite;

        /// <summary>
        /// The animating sprite for the combat view of this character.
        /// </summary>
        public AnimatingSprite CombatSprite
        {
            get { return combatSprite; }
            set { combatSprite = value; }
        }


        /// <summary>
        /// Reset the animations for this character.
        /// </summary>
        public override void ResetAnimation(bool isWalking)
        {
            base.ResetAnimation(isWalking);
            if (combatSprite != null)
            {
                combatSprite.PlayAnimation("Idle");
            }
        }


        #endregion


        #region Static Animation Data


        /// <summary>
        /// The default animation interval for the combat map sprite.
        /// </summary>
        private int combatAnimationInterval = 100;

        /// <summary>
        /// The default animation interval for the combat map sprite.
        /// </summary>
        [ContentSerializer(Optional=true)]
        public int CombatAnimationInterval
        {
            get { return combatAnimationInterval; }
            set { combatAnimationInterval = value; }
        }


        /// <summary>
        /// Add the standard character walk animations to this character.
        /// </summary>
        private void AddStandardCharacterCombatAnimations()
        {
            if (combatSprite != null)
            {
                combatSprite.AddAnimation(new Animation("Idle", 37, 42, 
                    CombatAnimationInterval, true));
                combatSprite.AddAnimation(new Animation("Walk", 25, 30,
                    CombatAnimationInterval, true));
                combatSprite.AddAnimation(new Animation("Attack", 1, 6, 
                    CombatAnimationInterval, false));
                combatSprite.AddAnimation(new Animation("SpellCast", 31, 36, 
                    CombatAnimationInterval, false));
                combatSprite.AddAnimation(new Animation("Defend", 13, 18, 
                    CombatAnimationInterval, false));
                combatSprite.AddAnimation(new Animation("Dodge", 13, 18, 
                    CombatAnimationInterval, false));
                combatSprite.AddAnimation(new Animation("Hit", 19, 24, 
                    CombatAnimationInterval, false));
                combatSprite.AddAnimation(new Animation("Die", 7, 12, 
                    CombatAnimationInterval, false));
            }
        }


        #endregion


        #region Content Type Reader


        /// <summary>
        /// Reads a FightingCharacter object from the content pipeline.
        /// </summary>
        public class FightingCharacterReader : ContentTypeReader<FightingCharacter>
        {
            /// <summary>
            /// Reads a FightingCharacter object from the content pipeline.
            /// </summary>
            protected override FightingCharacter Read(ContentReader input, 
                FightingCharacter existingInstance)
            {
                FightingCharacter fightingCharacter = existingInstance;
                if (fightingCharacter == null)
                {
                    throw new ArgumentNullException("existingInstance");
                }

                input.ReadRawObject<Character>(fightingCharacter as Character);

                fightingCharacter.CharacterClassContentName = input.ReadString();
                fightingCharacter.CharacterLevel = input.ReadInt32();
                fightingCharacter.InitialEquipmentContentNames.AddRange(
                    input.ReadObject<List<string>>());
                fightingCharacter.Inventory.AddRange(
                    input.ReadObject<List<ContentEntry<Gear>>>());
                fightingCharacter.CombatAnimationInterval = input.ReadInt32();
                fightingCharacter.CombatSprite = input.ReadObject<AnimatingSprite>();
                fightingCharacter.AddStandardCharacterCombatAnimations();
                fightingCharacter.ResetAnimation(false);

                // load the character class
                fightingCharacter.CharacterClass =
                    input.ContentManager.Load<CharacterClass>(
                        System.IO.Path.Combine("CharacterClasses",
                            fightingCharacter.CharacterClassContentName));

                // populate the equipment list
                foreach (string gearName in 
                    fightingCharacter.InitialEquipmentContentNames)
                {
                    fightingCharacter.Equip(input.ContentManager.Load<Equipment>(
                        System.IO.Path.Combine("Gear", gearName)));
                }
                fightingCharacter.RecalculateEquipmentStatistics();
                fightingCharacter.RecalculateTotalTargetDamageRange();
                fightingCharacter.RecalculateTotalDefenseRanges();

                // populate the inventory based on the content names
                foreach (ContentEntry<Gear> inventoryEntry in 
                    fightingCharacter.Inventory)
                {
                    inventoryEntry.Content = input.ContentManager.Load<Gear>(
                        System.IO.Path.Combine("Gear", inventoryEntry.ContentName));
                }

                return fightingCharacter;
            }
        }


        #endregion
    }
}
