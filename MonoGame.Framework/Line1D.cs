using System.Linq;
using System.Collections.Generic;
using System;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Represents a one dimensional line segment.
    /// </summary>
    public struct Line1D
    {
        /// <summary>
        /// Start of line.
        /// </summary>
        public float Min;

        /// <summary>
        /// End of line.
        /// </summary>
        public float Max;

        /// <summary>
        /// Creates a one dimenensional line instance.
        /// </summary>
        /// <param name="min">Start of line.</param>
        /// <param name="max">End of line.</param>
        public Line1D(float min, float max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Calculates the overlapping part to another line.
        /// </summary>
        /// <param name="other">Line to get overlapping part of.</param>
        /// <returns>Overlapping size.</returns>
        public float OverlapSize(Line1D other)
        {
            return Math.Max(0, Math.Min(Max, other.Max) - Math.Max(Min, other.Min));
        }
    }
}