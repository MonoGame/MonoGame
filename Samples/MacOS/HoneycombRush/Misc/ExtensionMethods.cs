#region File Description
//-----------------------------------------------------------------------------
// ExtensionMethods.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
#endregion

namespace HoneycombRush
{
    /// <summary>
    /// A class containing extension methods.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Returns a vector pointing to the rectangle's top left corner.
        /// </summary>
        /// <param name="rect">The rectangle for which to produce the vector.</param>
        /// <returns>A vector pointing to the rectangle's top left corner.</returns>
        public static Vector2 GetVector(this Rectangle rect)
        {
            return new Vector2(rect.X, rect.Y);
        }

        /// <summary>
        /// Returns a vector pointing to the specified point.
        /// </summary>
        /// <param name="point">The point for which to produce the vector.</param>
        /// <returns>A vector pointing to the specified point.</returns>
        public static Vector2 GetVector(this Point point)
        {
            return new Vector2(point.X, point.Y);
        }

        /// <summary>
        /// Check for collision between two rectangle.
        /// </summary>
        /// <param name="first">The first rectangle.</param>
        /// <param name="second">The second rectangle.</param>
        /// <returns>Returns true if the two rectangles collide, 
        /// false otherwise.</returns>
        public static bool HasCollision(this Rectangle first, Rectangle second)
        {
            return first.Intersects(second);
        }
    }
}
