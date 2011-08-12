#region File Description
//-----------------------------------------------------------------------------
// GearDrop.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework.Content;
#endregion

namespace RolePlayingGameData
{
    /// <summary>
    /// Description of how often a particular gear drops, typically from a Monster.
    /// </summary>
    public class GearDrop
    {
        /// <summary>
        /// The content name of the gear.
        /// </summary>
        private string gearName;

        /// <summary>
        /// The content name of the gear.
        /// </summary>
        public string GearName
        {
            get { return gearName; }
            set { gearName = value; }
        }


        /// <summary>
        /// The percentage chance that the gear will drop, from 0 to 100.
        /// </summary>
        private int dropPercentage;

        /// <summary>
        /// The percentage chance that the gear will drop, from 0 to 100.
        /// </summary>
        public int DropPercentage
        {
            get { return dropPercentage; }
            set { dropPercentage = (value > 100 ? 100 : (value < 0 ? 0 : value)); }
        }


        #region Content Type Reader


        /// <summary>
        /// Read a GearDrop object from the content pipeline.
        /// </summary>
        public class GearDropReader : ContentTypeReader<GearDrop>
        {
            protected override GearDrop Read(ContentReader input, 
                GearDrop existingInstance)
            {
                GearDrop gearDrop = existingInstance;
                if (gearDrop == null)
                {
                    gearDrop = new GearDrop();
                }

                gearDrop.GearName = input.ReadString();
                gearDrop.DropPercentage = input.ReadInt32();

                return gearDrop;
            }
        }


        #endregion
    }
}
