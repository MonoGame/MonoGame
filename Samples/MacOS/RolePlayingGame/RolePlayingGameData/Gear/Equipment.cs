#region File Description
//-----------------------------------------------------------------------------
// Equipment.cs
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
    /// Gear that may be equipped onto a FightingCharacter.
    /// </summary>
    public class Equipment : Gear
    {
        #region Owner Buff


        /// <summary>
        /// The statistics buff applied by this equipment to its owner.
        /// </summary>
        /// <remarks>Buff values are positive, and will be added.</remarks>
        private StatisticsValue ownerBuffStatistics = new StatisticsValue();

        /// <summary>
        /// The statistics buff applied by this equipment to its owner.
        /// </summary>
        /// <remarks>Buff values are positive, and will be added.</remarks>
        [ContentSerializer(Optional=true)]
        public StatisticsValue OwnerBuffStatistics
        {
            get { return ownerBuffStatistics; }
            set { ownerBuffStatistics = value; }
        }


        #endregion


        #region Content Type Reader


        /// <summary>
        /// Read the Equipment type from the content pipeline.
        /// </summary>
        public class EquipmentReader : ContentTypeReader<Equipment>
        {
            /// <summary>
            /// Read the Equipment type from the content pipeline.
            /// </summary>
            protected override Equipment Read(ContentReader input, 
                Equipment existingInstance)
            {
                Equipment equipment = existingInstance;

                if (equipment == null)
                {
                    throw new ArgumentException(
                        "Unable to create new Equipment objects.");
                }

                // read the gear settings
                input.ReadRawObject<Gear>(equipment as Gear);

                // read the equipment settings
                equipment.OwnerBuffStatistics = input.ReadObject<StatisticsValue>();

                return equipment;
            }
        }


        #endregion
    }
}
