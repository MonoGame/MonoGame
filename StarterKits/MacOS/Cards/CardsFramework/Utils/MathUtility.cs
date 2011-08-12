#region File Description
//-----------------------------------------------------------------------------
// MathUtility.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
#endregion

namespace CardsFramework
{
    public static class MathUtility
    {
        /// <summary>
        /// Rotates a point around another specified point.
        /// </summary>
        /// <param name="point">The point to rotate.</param>
        /// <param name="origin">The rotation origin or "axis".</param>
        /// <param name="rotation">The rotation amount in radians.</param>
        /// <returns>The position of the point after rotating it.</returns>
        public static Vector2 RotateAboutOrigin(Vector2 point, Vector2 origin, float rotation)
        {
            // Point relative to origin   
            Vector2 u = point - origin; 

            if (u == Vector2.Zero)
                return point;

            // Angle relative to origin   
            float a = (float)Math.Atan2(u.Y, u.X);

            // Rotate   
            a += rotation; 

            // U is now the new point relative to origin   
            u = u.Length() * new Vector2((float)Math.Cos(a), (float)Math.Sin(a));
            return u + origin;
        }
    }
}
