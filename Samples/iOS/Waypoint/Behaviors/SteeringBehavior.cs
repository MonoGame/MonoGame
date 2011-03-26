#region File Description
//-----------------------------------------------------------------------------
// SteeringBehavior.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using System;
#if IPHONE
using Microsoft.Xna.Framework;
#else
using Microsoft.Xna.Framework;
#endif

#endregion

namespace Waypoint
{
    /// <summary>
    /// This Behavior causes the tank to steer  and change direction smoothly 
    /// to its current destination. This Behavior uses logic from the Aiming sample
    /// </summary>
    class SteeringBehavior : Behavior
    {
        #region Initialization

        public SteeringBehavior(Tank tank)
            : base(tank)
        {
        }

        #endregion

        #region Update

        /// <summary>
        /// Update adjusts the tanks’ movement speed as necessary to ensure that 
        /// the current waypoint is inside its’ turning radius and steers the tank 
        /// towards the waypoint based on its’ maximum angular velocity.
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // This code causes the tank to change its speed gradually while it 
            // moves toward the waypoint previousMoveSpeed tracks how fast the 
            // tank was going, desiredMoveSpeed finds how fast the tank want to 
            // go and Math.Clamp keeps the tank from accelerating or decelerating 
            // too fast.
            float previousMoveSpeed = tank.MoveSpeed;
            float desiredMoveSpeed = FindMaxMoveSpeed(tank.Waypoints.Peek());
            tank.MoveSpeed = MathHelper.Clamp(desiredMoveSpeed,
                previousMoveSpeed - Tank.MaxMoveSpeedDelta * elapsedTime,
                previousMoveSpeed + Tank.MaxMoveSpeedDelta * elapsedTime);

            // This code causes the tank to turn towards the waypoint.  First we 
            // take the vector that represents the tanks’ current heading, 
            // Tank.Direction, and convert it into an angle in radians. Then we 
            // use TurnToFace to make the tank turn towards it’s waypoint based 
            // on it’s turning speed, Tank,MaxAngualrVelocity. After we have the 
            // new direction in radian we convert it back into a vector.
            float facingDirection = (float)Math.Atan2(
                tank.Direction.Y, tank.Direction.X);
            facingDirection = TurnToFace(tank.Location, tank.Waypoints.Peek(),
                    facingDirection, Tank.MaxAngularVelocity * elapsedTime);
            tank.Direction = new Vector2(
                (float)Math.Cos(facingDirection),
                (float)Math.Sin(facingDirection));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Estimate the Tank's best possible movement speed to it's destination
        /// </summary>
        /// <param name="newDestination">The Tank's target location</param>
        /// <returns>Maximum estimated movement speed for the Tank 
        /// up to Tank.MaxMoveSpeed</returns>
        private float FindMaxMoveSpeed(Vector2 waypoint)
        {
            float finalSpeed = Tank.MaxMoveSpeed;
            // Given a velocity v (Tank.MaxMoveSpeed) and an angular velocity 
            // w(Tank.MaxAngularVelocity), the smallest turning radius 
            // r(turningRadius) ofthe tank is the velocity divided by the turning
            // speed: r = v/w 
            float turningRadius = Tank.MaxMoveSpeed / Tank.MaxAngularVelocity;


            // This code figures out if the tank can move to its waypoint from its 
            // current location based on its turning circle(turningRadius) when its 
            // moving as fast as possible(Tank.MaxMoveSpeed). For any given turning 
            // circle there is an area to either side of the tank that it cannot 
            // move into that can be represented by 2 circles of radius turningRadius 
            // on either side of the tank. If the waypoint is inside one of these 
            // 2 circles the tank will have to slow down before it can move to it

            // This creates a vector that’s orthogonal to the tank in the direction 
            // it's facing. This means that the vector is at a right angle to the 
            // direction the tank is pointing in.
            Vector2 orth = new Vector2(tank.Direction.Y, -tank.Direction.X);

            // In this code we can combine the tanks’ location, the orthogonal 
            // vector and the tanks’ turning radius to find the 2 points that 
            // describe the centers of the circles the tanks cannot move into. 
            // Then we use Vector2.Distance to find the distances from each circle 
            // center to the waypoint. Afterwards Math.Min return the distance from 
            // the waypoint to whichever circle was closest.
            float closestDistance = Math.Min(
                Vector2.Distance(waypoint, tank.Location + (orth * turningRadius)),
                Vector2.Distance(waypoint, tank.Location - (orth * turningRadius)));


            // If closestDistance is less than turningRadius, then the waypoint is 
            // inside one of the 2 circles the Tank cannot turn into when moving at 
            // Tank.MaxMoveSpeed, instead we need to estimate a speed that the tank 
            // can move at.
            if (closestDistance < turningRadius)
            {
                // This finds the radius of a circle where the Tank's location and 
                // the waypoint are 2 points on opposite sides of the circle.
                float radius = Vector2.Distance(tank.Location, waypoint) / 2;
                // Now we use the radius from above to and Tank.MaxAngularVelocity 
                // to find out how fast we can move towards the waypoint by taking 
                // r = v/w and turning it into v = r*w
                finalSpeed = Tank.MaxAngularVelocity * radius;
            }

            return finalSpeed;
        }

        /// <summary>
        /// Calculates the angle that an object should face, given its position, its
        /// target's position, its current angle, and its maximum turning speed.
        /// </summary>
        private static float TurnToFace(Vector2 position, Vector2 faceThis,
            float currentAngle, float turnSpeed)
        {
            // consider this diagram:
            //         C 
            //        /|
            //      /  |
            //    /    | y
            //  / o    |
            // S--------
            //     x
            // 
            // where S is the position of the spot light, C is the position of the cat,
            // and "o" is the angle that the spot light should be facing in order to 
            // point at the cat. we need to know what o is. using trig, we know that
            //      tan(theta)       = opposite / adjacent
            //      tan(o)           = y / x
            // if we take the arctan of both sides of this equation...
            //      arctan( tan(o) ) = arctan( y / x )
            //      o                = arctan( y / x )
            // so, we can use x and y to find o, our "desiredAngle."
            // x and y are just the differences in position between the two objects.
            float x = faceThis.X - position.X;
            float y = faceThis.Y - position.Y;

            // we'll use the Atan2 function. Atan will calculates the arc tangent of 
            // y / x for us, and has the added benefit that it will use the signs of x
            // and y to determine what cartesian quadrant to put the result in.
            // http://msdn2.microsoft.com/en-us/library/system.math.atan2.aspx
            float desiredAngle = (float)Math.Atan2(y, x);

            // so now we know where we WANT to be facing, and where we ARE facing...
            // if we weren't constrained by turnSpeed, this would be easy: we'd just 
            // return desiredAngle.
            // instead, we have to calculate how much we WANT to turn, and then make
            // sure that's not more than turnSpeed.

            // first, figure out how much we want to turn, using WrapAngle to get our
            // result from -Pi to Pi ( -180 degrees to 180 degrees )
            float difference = WrapAngle(desiredAngle - currentAngle);

            // clamp that between -turnSpeed and turnSpeed.
            difference = MathHelper.Clamp(difference, -turnSpeed, turnSpeed);

            // so, the closest we can get to our target is currentAngle + difference.
            // return that, using WrapAngle again.
            return WrapAngle(currentAngle + difference);
        }

        /// <summary>
        /// Returns the angle expressed in radians between -Pi and Pi.
        /// </summary>
        private static float WrapAngle(float radians)
        {
            while (radians < -MathHelper.Pi)
            {
                radians += MathHelper.TwoPi;
            }
            while (radians > MathHelper.Pi)
            {
                radians -= MathHelper.TwoPi;
            }
            return radians;
        }

        #endregion
    }
}
