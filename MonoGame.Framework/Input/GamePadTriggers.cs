// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// A struct that countains information on the left and the right trigger buttons.
    /// </summary>
    public struct GamePadTriggers
    {
        /// <summary>
        /// Gets the position of the left trigger.
        /// </summary>
        /// <value>A value from 0.0f to 1.0f representing left trigger.</value>
        public float Left { get; private set; }

        /// <summary>
        /// Gets the position of the right trigger.
        /// </summary>
        /// <value>A value from 0.0f to 1.0f representing right trigger.</value>
        public float Right { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Xna.Framework.Input.GamePadTriggers"/> struct.
        /// </summary>
        /// <param name="leftTrigger">The position of the left trigger, the value will get clamped between 0.0f and 1.0f.</param>
        /// <param name="rightTrigger">The position of the right trigger, the value will get clamped between 0.0f and 1.0f.</param>
        public GamePadTriggers(float leftTrigger, float rightTrigger) : this()
        {
            Left = MathHelper.Clamp(leftTrigger, 0f, 1f);
            Right = MathHelper.Clamp(rightTrigger, 0f, 1f);
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="GamePadTriggers"/> are equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>true if <paramref name="left"/> and <paramref name="right"/> are equal; otherwise, false.</returns>
        public static bool operator ==(GamePadTriggers left, GamePadTriggers right)
        {
            return (left.Left == right.Left) && (left.Right == right.Right);
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="GamePadTriggers"/> are not equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>true if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, false.</returns>
        public static bool operator !=(GamePadTriggers left, GamePadTriggers right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare to this instance.</param>
        /// <returns>true if <paramref name="obj"/> is a <see cref="GamePadTriggers"/> and has the same value as this instance; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return (obj is GamePadTriggers) && (this == (GamePadTriggers)obj);
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="T:Microsoft.Xna.Framework.Input.GamePadTriggers"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Left.GetHashCode() * 397) ^ Right.GetHashCode();
            }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadTriggers"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadTriggers"/>.</returns>
        public override string ToString()
        {
            return "[GamePadTriggers: Left=" + Left + ", Right=" + Right + "]";
        }
    }
}
