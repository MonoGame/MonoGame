#region File Description
//-----------------------------------------------------------------------------
// CollisionManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
#endregion

namespace NetRumble
{
    /// <summary>
    /// Manages collisions and collision events between all gameplay objects.
    /// </summary>
    public class CollisionManager : BatchRemovalCollection<GameplayObject>
    {
        #region Constants


        /// <summary>
        /// The ratio of speed to damage applied, for explosions.
        /// </summary>
        private const float speedDamageRatio = 0.5f;

        /// <summary>
        /// The number of times that the FindSpawnPoint method will try to find a point.
        /// </summary>
        private const int findSpawnPointAttempts = 25;


        #endregion


        #region Helper Types


        /// <summary>
        /// The result of a collision query.
        /// </summary>
        struct CollisionResult
        {
            /// <summary>
            /// How far away did the collision occur down the ray
            /// </summary>
            public float Distance;

            /// <summary>
            /// The collision "direction"
            /// </summary>
            public Vector2 Normal;

            /// <summary>
            /// What caused the collison (what the source ran into)
            /// </summary>
            public GameplayObject GameplayObject;


            public static int Compare(CollisionResult a, CollisionResult b)
            {
                return a.Distance.CompareTo(b.Distance);
            }
        }


        #endregion


        #region Singleton


        /// <summary>
        /// Singleton for collision management.
        /// </summary>
        private static CollisionManager collisionManager = new CollisionManager();
        public static BatchRemovalCollection<GameplayObject> Collection
        {
            get { return collisionManager as BatchRemovalCollection<GameplayObject>; }
        }


        #endregion


        #region Collision Data


        /// <summary>
        /// The dimensions of the space in which collision occurs.
        /// </summary>
        private Rectangle dimensions = new Rectangle(0, 0, 2048, 2048);
        public static Rectangle Dimensions
        {
            get { return (collisionManager == null ? Rectangle.Empty : 
                collisionManager.dimensions); }
            set
            {
                // safety-check the singleton
                if (collisionManager == null)
                {
                    throw new InvalidOperationException(
                        "The collision manager has not yet been initialized.");
                }
                collisionManager.dimensions = value;
            }
        }

        /// <summary>
        /// The list of barriers in the game world.
        /// </summary>
        /// <remarks>This list is not owned by this object.</remarks>
        private List<Rectangle> barriers = new List<Rectangle>();
        public static List<Rectangle> Barriers
        {
            get { return (collisionManager == null ? null : 
                collisionManager.barriers); }
        }


        /// <summary>
        /// Cached list of collision results, for more optimal collision detection.
        /// </summary>
        List<CollisionResult> collisionResults = new List<CollisionResult>();

        
        #endregion


        #region Initialization Methods


        /// <summary>
        /// Constructs a new collision manager.
        /// </summary>
        private CollisionManager() { }


        #endregion


        #region Updating Methods


        /// <summary>
        /// Update the collision system.
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        public static void Update(float elapsedTime)
        {
            // safety-check the singleton
            if (collisionManager == null)
            {
                throw new InvalidOperationException(
                    "The collision manager has not yet been initialized.");
            }


            // move each object
            for (int i = 0; i < collisionManager.Count; ++i)
            {
                if (collisionManager[i].Active)
                {
                    // determine how far they are going to move
                    Vector2 movement = collisionManager[i].Velocity * elapsedTime;
                    // only allow collisionManager that have not collided yet 
                    // collisionManager frame to collide
                    // -- otherwise, objects can "double-hit" and trade their momentum
                    if (collisionManager[i].CollidedThisFrame == false)
                    {
                        movement = MoveAndCollide(collisionManager[i], movement);
                    }
                    // determine the new position
                    collisionManager[i].Position += movement;

                    // collide with the barriers
                    for (int b = 0; b < collisionManager.barriers.Count; ++b)
                    {
                        CollisionMath.CircleLineCollisionResult result = 
                            new CollisionMath.CircleLineCollisionResult();
                        if (collisionManager[i] is Projectile)
                        {
                            CollisionMath.CircleRectangleCollide(
                                collisionManager[i].Position - movement, 
                                collisionManager[i].Radius, 
                                collisionManager.barriers[b], ref result);
                            if (result.Collision)
                            {
                                collisionManager[i].Position -= movement;
                                collisionManager[i].Die(null, false);
                            }
                        }
                        else
                        {
                            CollisionMath.CircleRectangleCollide(
                                collisionManager[i].Position, 
                                collisionManager[i].Radius, 
                                collisionManager.barriers[b], ref result);
                            if (result.Collision)
                            {
                                // if a non-projectile hits a barrier, bounce slightly
                                float vn = Vector2.Dot(collisionManager[i].Velocity, 
                                    result.Normal);
                                collisionManager[i].Velocity -= (2.0f * vn) * 
                                    result.Normal;
                                collisionManager[i].Position += result.Normal * 
                                    result.Distance;
                            }
                        }
                    }
                }
            }

            CollisionManager.Collection.ApplyPendingRemovals();
        }


        /// <summary>
        /// Move the given gameplayObject by the given movement, colliding and adjusting
        /// as necessary.
        /// </summary>
        /// <param name="gameplayObject">The gameplayObject who is moving.</param>
        /// <param name="movement">The desired movement vector for this update.</param>
        /// <returns>The movement vector after considering all collisions.</returns>
        private static Vector2 MoveAndCollide(GameplayObject gameplayObject, 
            Vector2 movement)
        {
            // safety-check the singleton
            if (collisionManager == null)
            {
                throw new InvalidOperationException(
                    "The collision manager has not yet been initialized.");
            }

            if (gameplayObject == null)
            {
                throw new ArgumentNullException("gameplayObject");
            }
            // make sure we care about where this gameplayObject goes
            if (!gameplayObject.Active)
            {
                return movement;
            }
            // make sure the movement is significant
            if (movement.LengthSquared() <= 0f)
            {
                return movement;
            }

            // generate the list of collisions
            Collide(gameplayObject, movement);

            // determine if we had any collisions
            if (collisionManager.collisionResults.Count > 0)
            {
                collisionManager.collisionResults.Sort(CollisionResult.Compare);
                foreach (CollisionResult collision in collisionManager.collisionResults)
                {
                    // let the two objects touch each other, and see what happens
                    if (gameplayObject.Touch(collision.GameplayObject) && 
                        collision.GameplayObject.Touch(gameplayObject))
                    {
                        gameplayObject.CollidedThisFrame =
                            collision.GameplayObject.CollidedThisFrame = true;
                        // they should react to the other, even if they just died
                        AdjustVelocities(gameplayObject, collision.GameplayObject);
                        return Vector2.Zero;
                    }
                }
            }

            return movement;
        }


        /// <summary>
        /// Determine all collisions that will happen as the given gameplayObject moves.
        /// </summary>
        /// <param name="gameplayObject">The gameplayObject that is moving.</param>
        /// <param name="movement">The gameplayObject's movement vector.</param>
        /// <remarks>The results are stored in the cached list.</remarks>
        public static void Collide(GameplayObject gameplayObject, Vector2 movement)
        {
            // safety-check the singleton
            if (collisionManager == null)
            {
                throw new InvalidOperationException(
                    "The collision manager has not yet been initialized.");
            }

            collisionManager.collisionResults.Clear();

            if (gameplayObject == null)
            {
                throw new ArgumentNullException("gameplayObject");
            }
            if (!gameplayObject.Active)
            {
                return;
            }

            // determine the movement direction and scalar
            float movementLength = movement.Length();
            if (movementLength <= 0f)
            {
                return;
            }

            // check each gameplayObject
            foreach (GameplayObject checkActor in collisionManager)
            {
                if ((gameplayObject == checkActor) || !checkActor.Active)
                {
                    continue;
                }

                // calculate the target vector
                float combinedRadius = checkActor.Radius + gameplayObject.Radius;
                Vector2 checkVector = checkActor.Position - gameplayObject.Position;
                float checkVectorLength = checkVector.Length();
                if (checkVectorLength <= 0f)
                {
                    continue;
                }

                float distanceBetween = MathHelper.Max(checkVectorLength - 
                    (checkActor.Radius + gameplayObject.Radius), 0);

                // check if they could possibly touch no matter the direction
                if (movementLength < distanceBetween)
                {
                    continue;
                }

                // determine how much of the movement is bringing the two together
                float movementTowards = Vector2.Dot(movement, checkVector);

                // check to see if the movement is away from each other
                if (movementTowards < 0f)
                {
                    continue;
                }

                if (movementTowards < distanceBetween)
                {
                    continue;
                }

                CollisionResult result = new CollisionResult();
                result.Distance = distanceBetween;
                result.Normal = Vector2.Normalize(checkVector);
                result.GameplayObject = checkActor;

                collisionManager.collisionResults.Add(result);
            }
        }


        /// <summary>
        /// Adjust the velocities of the two collisionManager as if they have collided,
        /// distributing their velocities according to their masses.
        /// </summary>
        /// <param name="actor1">The first gameplayObject.</param>
        /// <param name="actor2">The second gameplayObject.</param>
        private static void AdjustVelocities(GameplayObject actor1, 
            GameplayObject actor2)
        {
            // don't adjust velocities if at least one has negative mass
            if ((actor1.Mass <= 0f) || (actor2.Mass <= 0f))
            {
                return;
            }

            // determine the vectors normal and tangent to the collision
            Vector2 collisionNormal = actor2.Position - actor1.Position;
            if (collisionNormal.LengthSquared() > 0f)
            {
                collisionNormal.Normalize();
            }
            else
            {
                return;
            }

            Vector2 collisionTangent = new Vector2(
                -collisionNormal.Y, collisionNormal.X);

            // determine the velocity components along the normal and tangent vectors
            float velocityNormal1 = Vector2.Dot(actor1.Velocity, collisionNormal);
            float velocityTangent1 = Vector2.Dot(actor1.Velocity, collisionTangent);
            float velocityNormal2 = Vector2.Dot(actor2.Velocity, collisionNormal);
            float velocityTangent2 = Vector2.Dot(actor2.Velocity, collisionTangent);

            // determine the new velocities along the normal
            float velocityNormal1New = ((velocityNormal1 * (actor1.Mass - actor2.Mass))
                + (2f * actor2.Mass * velocityNormal2)) / (actor1.Mass + actor2.Mass);
            float velocityNormal2New = ((velocityNormal2 * (actor2.Mass - actor1.Mass))
                + (2f * actor1.Mass * velocityNormal1)) / (actor1.Mass + actor2.Mass);

            // determine the new total velocities
            actor1.Velocity = (velocityNormal1New * collisionNormal) +
                (velocityTangent1 * collisionTangent);
            actor2.Velocity = (velocityNormal2New * collisionNormal) +
                (velocityTangent2 * collisionTangent);
        }


        #endregion


        #region Interaction Methods


        /// <summary>
        /// Find a valid spawn point in the world.
        /// </summary>
        /// <param name="radius">The radius of the object to be spawned.</param>
        /// <param name="random">A persistent Random object.</param>
        /// <returns>The spawn point.</returns>
        public static Vector2 FindSpawnPoint(GameplayObject spawnedObject, float radius)
        {
            // safety-check the singleton
            if (collisionManager == null)
            {
                throw new InvalidOperationException(
                    "The collision manager has not yet been initialized.");
            }

            // safety-check the parameters
            if ((radius < 0f) || (radius > Dimensions.Width / 2))
            {
                throw new ArgumentOutOfRangeException("radius");
            }

            // keep trying to find a valid point
            Vector2 spawnPoint = new Vector2(
                radius + Dimensions.X + 
                   RandomMath.Random.Next((int)Math.Floor(Dimensions.Width - radius)),
                radius + Dimensions.Y + 
                   RandomMath.Random.Next((int)Math.Floor(Dimensions.Height - radius)));
            for (int i = 0; i < findSpawnPointAttempts; i++)
            {
                bool valid = true;

                // check the barriers
                if (Barriers != null)
                {
                    CollisionMath.CircleLineCollisionResult result = 
                        new CollisionMath.CircleLineCollisionResult();
                    foreach (Rectangle rectangle in Barriers)
                    {
                        if (CollisionMath.CircleRectangleCollide(spawnPoint, radius, 
                            rectangle, ref result))
                        {
                            valid = false;
                            break;
                        }
                    }
                }

                // check the other objects
                if (valid)
                {
                    foreach (GameplayObject gameplayObject in collisionManager)
                    {
                        if (!gameplayObject.Active || (gameplayObject == spawnedObject))
                        {
                            continue;
                        }
                        if (CollisionMath.CircleCircleIntersect(spawnPoint, radius, 
                            gameplayObject.Position, gameplayObject.Radius))
                        {
                            valid = false;
                            break;
                        }
                    }
                }
                if (valid)
                {
                    break;
                }
                spawnPoint = new Vector2(
                    radius + Dimensions.X + RandomMath.Random.Next(
                       (int)Math.Floor(Dimensions.Width - radius)),
                    radius + Dimensions.Y + RandomMath.Random.Next(
                       (int)Math.Floor(Dimensions.Height - radius)));
            }

            return spawnPoint;
        }


        /// <summary>
        /// Process an explosion in the world against the objects in it.
        /// </summary>
        /// <param name="source">The source of the explosion.</param>
        /// <param name="target">The target of the attack.</param>
        /// <param name="damageAmount">The amount of explosive damage.</param>
        /// <param name="position">The position of the explosion.</param>
        /// <param name="damageRadius">The radius of the explosion.</param>
        /// <param name="damageOwner">If true, it will hit the source.</param>
        public static void Explode(GameplayObject source, GameplayObject target, 
            float damageAmount, Vector2 position, float damageRadius, bool damageOwner)
        {
            // safety-check the singleton
            if (collisionManager == null)
                {
                    throw new InvalidOperationException(
                        "The collision manager has not yet been initialized.");
            }

            if (damageRadius <= 0f)
            {
                return;
            }
            float damageRadiusSquared = damageRadius * damageRadius;

            foreach (GameplayObject gameplayObject in collisionManager)
            {
                // don't bother if it's already dead
                if (!gameplayObject.Active)
                {
                    continue;
                }
                // don't hurt the GameplayObject that the projectile hit, it's hurt
                if (gameplayObject == target)
                {
                    continue;
                }
                // don't hit the owner if the damageOwner flag is off
                if ((gameplayObject == source) && !damageOwner)
                {
                    continue;
                }
                // measure the distance to the GameplayObject and see if it's in range
                Vector2 direction = gameplayObject.Position - position;
                float distanceSquared = direction.LengthSquared();
                if ((distanceSquared > 0f) && (distanceSquared <= damageRadiusSquared))
                {
                    float distance = (float)Math.Sqrt((float)distanceSquared);
                    // adjust the amount of damage based on the distance
                    // -- note that damageRadius <= 0 is accounted for earlier
                    float adjustedDamage = damageAmount *
                        (damageRadius - distance) / damageRadius;
                    // if we're still damaging the GameplayObject, then apply it
                    if (adjustedDamage > 0f)
                    {
                        gameplayObject.Damage(source, adjustedDamage);
                    }
                    // move those affected by the blast
                    if (gameplayObject != source)
                    {
                        direction.Normalize();
                        gameplayObject.Velocity += direction * adjustedDamage * 
                            speedDamageRatio;
                    }
                }
            }
        }


        #endregion
    }
}
