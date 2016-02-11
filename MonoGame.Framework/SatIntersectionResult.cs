using System.Linq;
using System.Collections.Generic;
using System;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Represents an intersection result from two collision shapes with separating axis theorem.
    /// </summary>
    public struct SatIntersectionResult
    {
        /// <summary>
        /// Whether an intersection happens.
        /// </summary>
        public bool Intersects;

        /// <summary>
        /// Oriented distance to separte the colliding shapes.
        /// </summary>
        public Vector3 Separation;

        /// <summary>
        /// Default, when noiIntersection is present.
        /// </summary>
        public static SatIntersectionResult Empty = new SatIntersectionResult();
    }
}