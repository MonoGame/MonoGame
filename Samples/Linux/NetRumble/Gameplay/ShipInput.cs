#region File Description
//-----------------------------------------------------------------------------
// ShipInput.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
#endregion

namespace NetRumble
{
    /// <summary>
    /// Player input abstraction for ships.
    /// </summary>
    public struct ShipInput
    {
        #region Static Constants


        /// <summary>
        /// The empty version of this structure.
        /// </summary>
        private static ShipInput empty = 
            new ShipInput(Vector2.Zero, Vector2.Zero, false);
        public static ShipInput Empty
        {
            get { return empty; }
        }


        #endregion


        #region Input Data


        /// <summary>
        /// The left-stick value of the ship input, used for movement.
        /// </summary>
        public Vector2 LeftStick;

        /// <summary>
        /// The right-stick value of the ship input, used for firing.
        /// </summary>
        public Vector2 RightStick;

        /// <summary>
        /// If true, the player is trying to fire a mine.
        /// </summary>
        public bool MineFired;


        #endregion


        #region Initialization Methods


        /// <summary>
        /// Constructs a new ShipInput object.
        /// </summary>
        /// <param name="leftStick">The left-stick value of the ship input.</param>
        /// <param name="rightStick">The right-stick value of the ship input.</param>
        /// <param name="mineFired">If true, the player is firing a mine.</param>
        public ShipInput(Vector2 leftStick, Vector2 rightStick, bool mineFired)
        {
            this.LeftStick = leftStick;
            this.RightStick = rightStick;
            this.MineFired = mineFired;
        }


        /// <summary>
        /// Create a new ShipInput object based on the data in the packet.
        /// </summary>
        /// <param name="packetReader">The packet with the data.</param>
        public ShipInput(PacketReader packetReader)
        {
            // safety-check the parameters, as they must be valid
            if (packetReader == null)
            {
                throw new ArgumentNullException("packetReader");
            }

            // read the data
            LeftStick = packetReader.ReadVector2();
            RightStick = packetReader.ReadVector2();
            MineFired = packetReader.ReadBoolean();
        }


        /// <summary>
        /// Create a new ShipInput object based on local input state.
        /// </summary>
        /// <param name="gamePadState">The local gamepad state.</param>
        /// <param name="keyboardState">The local keyboard state</param>
        public ShipInput(GamePadState gamePadState, KeyboardState keyboardState)
        {
            // check for movement action
            bool keyHit = false;
            LeftStick = Vector2.Zero;
            if (keyboardState.IsKeyDown(Keys.W))
            {
                LeftStick += Vector2.UnitY;
                keyHit = true;
            }
            if (keyboardState.IsKeyDown(Keys.A))
            {
                LeftStick += Vector2.Multiply(Vector2.UnitX, -1f);
                keyHit = true;
            }
            if (keyboardState.IsKeyDown(Keys.S))
            {
                LeftStick += Vector2.Multiply(Vector2.UnitY, -1f);
                keyHit = true;
            }
            if (keyboardState.IsKeyDown(Keys.D))
            {
                LeftStick += Vector2.UnitX;
                keyHit = true;
            } 

            if (keyHit)
            {
                if (LeftStick.LengthSquared() > 0)
                {
                    LeftStick.Normalize();
                }
            }
            else
            {
                LeftStick = gamePadState.ThumbSticks.Left;
            }

            // check for firing action
            keyHit = false;
            RightStick = Vector2.Zero;
            if (keyboardState.IsKeyDown(Keys.Up))
            {
                RightStick += Vector2.UnitY;
                keyHit = true;
            }
            if (keyboardState.IsKeyDown(Keys.Left))
            {
                RightStick += Vector2.Multiply(Vector2.UnitX, -1f);
                keyHit = true;
            }
            if (keyboardState.IsKeyDown(Keys.Down))
            {
                RightStick += Vector2.Multiply(Vector2.UnitY, -1f);
                keyHit = true;
            }
            if (keyboardState.IsKeyDown(Keys.Right))
            {
                RightStick += Vector2.UnitX;
                keyHit = true;
            }

            if (keyHit)
            {
                if (RightStick.LengthSquared() > 0)
                {
                    RightStick.Normalize();
                }
            }
            else
            {
                RightStick = gamePadState.ThumbSticks.Right;
            }

            // check for firing a mine
            keyHit = false;
            MineFired = false;
            if (keyboardState.IsKeyDown(Keys.Space))
            {
                MineFired = true;
                keyHit = true;
            }

            if (!keyHit)
            {
                MineFired = gamePadState.Triggers.Right >= 0.9f;
            }
        }


        #endregion


        #region Networking Methods


        /// <summary>
        /// Serialize the object out to a packet.
        /// </summary>
        /// <param name="packetWriter">The packet to write to.</param>
        public void Serialize(PacketWriter packetWriter)
        {
            // safety-check the parameters, as they must be valid
            if (packetWriter == null)
            {
                throw new ArgumentNullException("packetWriter");
            }

            packetWriter.Write((int)World.PacketTypes.ShipInput); 
            packetWriter.Write(LeftStick);
            packetWriter.Write(RightStick);
            packetWriter.Write(MineFired);
        }


        #endregion
    }
}
