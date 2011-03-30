#region File Description
//-----------------------------------------------------------------------------
// Tank.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

#endregion

namespace Waypoint
{
    public enum BehaviorType
    {
        Linear,
        Steering,
    }

    /// <summary>
    /// A simple object that moves towards it's set destination
    /// </summary>
    public class Tank : DrawableGameComponent
    {
        #region Constants

        /// <summary>
        /// The "close enough" limit, if the tank is inside this many pixel 
        /// to it's destination it's considered at it's destination
        /// </summary>
        const float atDestinationLimit = 1f;

        /// <summary>
        /// This is how much the Tank can turn in one second in radians, since Pi 
        /// radians makes half a circle the tank can all the way around in one second
        /// </summary>
        public static float MaxAngularVelocity
        {
            get { return maxAngularVelocity; }
        }
        const float maxAngularVelocity = MathHelper.Pi;

        /// <summary>
        /// This is the Tanks’ best possible movement speed
        /// </summary>
        public static float MaxMoveSpeed
        {
            get { return maxMoveSpeed; }
        }
        const float maxMoveSpeed = 100f;

        /// <summary>
        /// This is most the tank can speed up or slow down in one second
        /// </summary>
        public static float MaxMoveSpeedDelta
        {
            get { return maxMoveSpeedDelta; }
        }
        const float maxMoveSpeedDelta = maxMoveSpeed / 2;

        #endregion

        #region Fields

        // Graphics data
        SpriteBatch spriteBatch;
        Texture2D tankTexture;
        Vector2 tankTextureCenter;

        /// <summary>
        /// The tanks’ current movement behavior, it’s responsible for updating the 
        /// tanks’ movement speed and direction
        /// </summary>
        Behavior currentBehavior;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current movement behavior
        /// </summary>
        public BehaviorType BehaviorType
        {
            get { return behaviorType; }
            set
            {
                if (behaviorType != value || currentBehavior == null)
                {
                    behaviorType = value;
                    switch (behaviorType)
                    {
                        case BehaviorType.Linear:
                            currentBehavior = new LinearBehavior(this);
                            break;
                        case BehaviorType.Steering:
                            currentBehavior = new SteeringBehavior(this);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        BehaviorType behaviorType;

        /// <summary>
        /// Length 1 vector that represents the tanks’ movement and facing direction
        /// </summary>
        public Vector2 Direction
        {
            get { return direction; }
            set { direction = value; }
        }
        protected Vector2 direction;

        /// <summary>
        /// The tank's current movement speed
        /// </summary>
        public float MoveSpeed
        {
            get { return moveSpeed; }
            set { moveSpeed = value; }
        }
        protected float moveSpeed;

        /// <summary>
        /// The tank's location on the map
        /// </summary>
        public Vector2 Location
        {
            get { return location; }
        }
        private Vector2 location;

        /// <summary>
        /// The list of points the tanks will move to in order from first to last
        /// </summary>
        public WaypointList Waypoints
        {
            get { return waypoints; }
        }
        private WaypointList waypoints;


        /// <summary>
        /// Linear distance to the Tank's current destination
        /// </summary>
        public float DistanceToDestination
        {
            get { return Vector2.Distance(location, waypoints.Peek()); }
        }

        /// <summary>
        /// True when the tank is "close enough" to it's destination
        /// </summary>
        public bool AtDestination
        {
            get { return DistanceToDestination < atDestinationLimit; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Tank constructor
        /// </summary>
        public Tank(Game game) : base(game)
        {
            location = Vector2.Zero;
            waypoints = new WaypointList();
            BehaviorType = BehaviorType.Linear;
        }

        /// <summary>
        /// Load the tank's texture resources
        /// </summary>
        /// <param name="content"></param>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            tankTexture = Game.Content.Load<Texture2D>("tank");

            tankTextureCenter =
                new Vector2(tankTexture.Width / 2, tankTexture.Height / 2);

            waypoints.LoadContent(Game.Content);

        }

        /// <summary>
        /// Reset the Tank's location on the map
        /// </summary>
        /// <param name="newLocation">new location on the map</param>
        public void Reset(Vector2 newLocation)
        {
            location = newLocation;
            waypoints.Clear();
        }

        #endregion

        #region Update Methods

        /// <summary>
        /// Update the Tank's position if it's not "close enough" to 
        /// it's destination
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // If we have any waypoints, the first one on the list is where 
            // we want to go
            if (waypoints.Count > 0)
            {
                if (AtDestination)
                {
                    // If we’re at the destination and there is at least one 
                    // waypoint in the list, get rid of the first one since we’re 
                    // there now
                    waypoints.Dequeue();
                }
                else
                {
                    // If we’re not at the destination, call Update on our 
                    // behavior and then move
                    if (currentBehavior != null)
                    {
                        currentBehavior.Update(gameTime);
                    }
                    location = location + (Direction *
                        MoveSpeed * elapsedTime);
                }
            }
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draw the Tank
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spriteBatch"></param>
        public override void Draw(GameTime gameTime)
        {
            waypoints.Draw(spriteBatch);

            float facingDirection = (float)Math.Atan2(
                Direction.Y, Direction.X);

            spriteBatch.Begin();
            spriteBatch.Draw(tankTexture, location, null, Color.White, facingDirection,
                tankTextureCenter, 1f, SpriteEffects.None, 0f);
            spriteBatch.End();
        }

        #endregion

        /// <summary>
        /// Change the tank movement Behavior
        /// </summary>
        public void CycleBehaviorType()
        {
            switch (behaviorType)
            {
                case BehaviorType.Linear:
                    BehaviorType = BehaviorType.Steering;
                    break;
                case BehaviorType.Steering:
                default:
                    BehaviorType = BehaviorType.Linear;
                    break;
            }
        }
    }
}
