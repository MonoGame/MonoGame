#region File Description
//-----------------------------------------------------------------------------
// PlayerPosition.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using RolePlayingGameData;
#endregion

namespace RolePlaying
{
    /// <summary>
    /// The position of a player in the tile engine.
    /// </summary>
    /// <remarks>Players are the only objects that move between tiles.</remarks>
    public class PlayerPosition
    {
        #region Position 


        /// <summary>
        /// Position in map coordinates (tiles).
        /// </summary>
        public Point TilePosition = Point.Zero;


        /// <summary>
        /// The offset into the tile, in pixels.
        /// </summary>
        public Vector2 TileOffset = Vector2.Zero;


        /// <summary>
        /// The position in screen coordinates.
        /// </summary>
        public Vector2 ScreenPosition
        {
            get
            {
                return TileEngine.GetScreenPosition(TilePosition) + TileOffset;
            }
        }


        #endregion


        #region Direction


        /// <summary>
        /// The direction that the player is facing.
        /// </summary>
        public Direction Direction = Direction.South;


        #endregion


        #region Movement


        /// <summary>
        /// If true, the position moved on the last update.
        /// </summary>
        /// <remarks>Used to control animation.</remarks>
        private bool isMoving = false;

        /// <summary>
        /// If true, the position moved on the last update.
        /// </summary>
        /// <remarks>Used to control animation.</remarks>
        public bool IsMoving
        {
            get { return isMoving; }
        }


        /// <summary>
        /// Move the player by the given amount.
        /// </summary>
        public void Move(Vector2 movement)
        {
            isMoving = (movement != Vector2.Zero);

            CalculateMovement(movement, ref TilePosition, ref TileOffset);

            // if the position is moving, up the direction
            if (IsMoving)
            {
                Direction = CalculateDirection(movement);
            }
        }


        #endregion


        #region Calculation


        /// <summary>
        /// Calculates the effect of movement on the position.
        /// </summary>
        /// <param name="movement">
        /// The movement to be used to calculate the new tile position.
        /// </param>
        /// <param name="TileOffset">The map position (tiles).</param>
        /// <param name="TilePosition">The offset into the current tile.</param>
        public static void CalculateMovement(Vector2 movement, ref Point TilePosition,
            ref Vector2 TileOffset)
        {
            // add the movement
            TileOffset += movement;

            while (TileOffset.X > TileEngine.Map.TileSize.X / 2f)
            {
                TilePosition.X++;
                TileOffset.X -= TileEngine.Map.TileSize.X;
            }
            while (TileOffset.X < -TileEngine.Map.TileSize.X / 2f)
            {
                TilePosition.X--;
                TileOffset.X += TileEngine.Map.TileSize.X;
            }
            while (TileOffset.Y > TileEngine.Map.TileSize.Y / 2f)
            {
                TilePosition.Y++;
                TileOffset.Y -= TileEngine.Map.TileSize.Y;
            }
            while (TileOffset.Y < -TileEngine.Map.TileSize.Y / 2f)
            {
                TilePosition.Y--;
                TileOffset.Y += TileEngine.Map.TileSize.Y;
            }
        }


        /// <summary>
        /// Determine the direction based on the given movement vector.
        /// </summary>
        /// <param name="vector">The vector that the player is moving.</param>
        /// <returns>The calculated direction.</returns>
        public static Direction CalculateDirection(Vector2 vector)
        {
            if (vector.X > 0)
            {
                if (vector.Y > 0)
                {
                    return Direction.SouthEast;
                }
                else if (vector.Y < 0)
                {
                    return Direction.NorthEast;
                }
                else // y == 0
                {
                    return Direction.East;
                }
            }
            else if (vector.X < 0)
            {
                if (vector.Y > 0)
                {
                    return Direction.SouthWest;
                }
                else if (vector.Y < 0)
                {
                    return Direction.NorthWest;
                }
                else // y == 0
                {
                    return Direction.West;
                }
            }
            else // x == 0
            {
                if (vector.Y > 0)
                {
                    return Direction.South;
                }
                else if (vector.Y < 0)
                {
                    return Direction.North;
                }
            }
            // x == 0 && y == 0, so... south?
            return Direction.South;
        }


        #endregion
    }
}
