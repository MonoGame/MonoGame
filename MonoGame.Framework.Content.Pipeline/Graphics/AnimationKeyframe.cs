// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides methods and properties for managing a keyframe. A keyframe describes the position of an animation channel at a single point in time.
    /// </summary>
    /// <param name="time">Time offset of the keyframe.</param>
    /// <param name="transform">Position of the keyframe.</param>
    public sealed class AnimationKeyframe(TimeSpan time, Matrix transform) : IComparable<AnimationKeyframe>
    {
        /// <summary>
        /// Gets the time offset from the start of the animation to the position described by this keyframe.
        /// </summary>
        public TimeSpan Time { get; } = time;

        /// <summary>
        /// Gets or sets the position described by this keyframe.
        /// </summary>
        public Matrix Transform { get; set; } = transform;

        /// <summary>
        /// Compares this instance of a keyframe to another.
        /// </summary>
        /// <param name="other">Keyframe being compared to.</param>
        /// <returns>Indication of their relative values.</returns>
        public int CompareTo(AnimationKeyframe? other)
        {
            // No sense in comparing the transform, so compare the time.
            // This would be used for sorting keyframes in time order.
            return other != null ? Time.CompareTo(other.Time) : 1;
        }
    }
}
