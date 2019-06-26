// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Defines the continuity of keys on a <see cref="Curve"/>.
    /// </summary>
    public enum CurveContinuity
    {
        /// <summary>
        /// Interpolation can be used between this key and the next.
        /// </summary>
        Smooth,
        /// <summary>
        /// Interpolation cannot be used. A position between the two points returns this point.
        /// </summary>
        Step
    }
}