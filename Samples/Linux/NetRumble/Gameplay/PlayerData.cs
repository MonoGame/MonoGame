#region File Description
//-----------------------------------------------------------------------------
// PlayerData.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Net;
#endregion

namespace NetRumble
{
    /// <summary>
    /// Data for each player in a network session.
    /// </summary>
    public class PlayerData
    {
        #region Gameplay Data


        /// <summary>
        /// The color of the overlay portion of the ship.
        /// </summary>
        private byte shipColor = 0;
        public byte ShipColor
        {
            get { return shipColor; }
            set
            {
                if ((value < 0) || (value >= Ship.ShipColors.Length))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                shipColor = value;
                // apply the change to the ship immediately 
                if (ship != null)
                {
                    ship.Color = Ship.ShipColors[shipColor];
                }
            }
        }

        /// <summary>
        /// The ship model to use.
        /// </summary>
        private byte shipVariation = 0;
        public byte ShipVariation
        {
            get { return shipVariation; }
            set 
            {
                if ((value < 0) || (value >= Ship.Variations))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                shipVariation = value;
                // apply the change to the ship immediately 
                if (ship != null)
                {
                    ship.Variation = shipVariation;
                }
            }
        }


        /// <summary>
        /// The ship used by this player.
        /// </summary>
        private Ship ship = null;
        public Ship Ship
        {
            get { return ship; }
            set 
            { 
                ship = value;
                // apply the other values to the ship immediately 
                if (ship != null)
                {
                    ship.Variation = shipVariation;
                    ship.Color = Ship.ShipColors[shipColor];
                }
            }
        }


        #endregion


        #region Initialization Methods


        /// <summary>
        /// Constructs a PlayerData object.
        /// </summary>
        public PlayerData() 
        {
            Ship = new Ship();
        }


        #endregion


        #region Networking Methods


        /// <summary>
        /// Deserialize from the packet into the current object.
        /// </summary>
        /// <param name="packetReader">The packet reader that has the data.</param>
        public void Deserialize(PacketReader packetReader)
        {
            // safety-check the parameter, as it must be valid.
            if (packetReader == null)
            {
                throw new ArgumentNullException("packetReader");
            }

            ShipColor = packetReader.ReadByte();
            ShipVariation = packetReader.ReadByte();
        }


        /// <summary>
        /// Serialize the current object to a packet.
        /// </summary>
        /// <param name="packetWriter">The packet writer that receives the data.</param>
        public void Serialize(PacketWriter packetWriter)
        {
            // safety-check the parameter, as it must be valid.
            if (packetWriter == null)
            {
                throw new ArgumentNullException("packetWriter");
            }

            packetWriter.Write(ShipColor);
            packetWriter.Write(ShipVariation);
        }


        #endregion
    }
}
