#region File Description
//-----------------------------------------------------------------------------
// Armor.cs
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
    /// Equipment that can be equipped on a FightingCharacter
    /// to improve their defense.
    /// </summary>
    public class Armor : Equipment
    {
        #region Slot


        /// <summary>
        /// Slots that a piece of armor may fill on a character.
        /// </summary>
        /// <remarks>Only one piece may fill a slot at the same time.</remarks>
        public enum ArmorSlot
        {
            Helmet,
            Shield,
            Torso,
            Boots,
        };


        /// <summary>
        /// The slot that this armor fills.
        /// </summary>
        private ArmorSlot slot;

        /// <summary>
        /// The slot that this armor fills.
        /// </summary>
        public ArmorSlot Slot
        {
            get { return slot; }
            set { slot = value; }
        }


        #endregion


        #region Description Data


        /// <summary>
        /// Builds and returns a string describing the power of this armor.
        /// </summary>
        public override string GetPowerText()
        {
            return "Weapon Defense: " + OwnerHealthDefenseRange.ToString() +
                "\nMagic Defense: " + OwnerMagicDefenseRange.ToString();
        }


        #endregion


        #region Owner Defense Data


        /// <summary>
        /// The range of health defense provided by this armor to its owner.
        /// </summary>
        private Int32Range ownerHealthDefenseRange;

        /// <summary>
        /// The range of health defense provided by this armor to its owner.
        /// </summary>
        public Int32Range OwnerHealthDefenseRange
        {
            get { return ownerHealthDefenseRange; }
            set { ownerHealthDefenseRange = value; }
        }


        /// <summary>
        /// The range of magic defense provided by this armor to its owner.
        /// </summary>
        private Int32Range ownerMagicDefenseRange;

        /// <summary>
        /// The range of magic defense provided by this armor to its owner.
        /// </summary>
        public Int32Range OwnerMagicDefenseRange
        {
            get { return ownerMagicDefenseRange; }
            set { ownerMagicDefenseRange = value; }
        }


        #endregion


        #region Content Type Reader


        /// <summary>
        /// Read the Weapon type from the content pipeline.
        /// </summary>
        public class ArmorReader : ContentTypeReader<Armor>
        {
            protected override Armor Read(ContentReader input, Armor existingInstance)
            {
                Armor armor = existingInstance;

                if (armor == null)
                {
                    armor = new Armor();
                }

                // read the gear settings
                input.ReadRawObject<Equipment>(armor as Equipment);

                // read armor settings
                armor.Slot = (ArmorSlot)input.ReadInt32();
                armor.OwnerHealthDefenseRange = input.ReadObject<Int32Range>();
                armor.OwnerMagicDefenseRange = input.ReadObject<Int32Range>();

                return armor;
            }
        }


        #endregion
    }
}
