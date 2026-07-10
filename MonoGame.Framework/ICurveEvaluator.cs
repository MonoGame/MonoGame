// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Interface for curve evaluation. Implemented by <see cref="Curve"/>
    /// </summary>    
	public interface ICurveEvaluator<T>
    {
        /// <summary>
        /// Evaluate the value at a position of this <see cref="ICurveEvaluator{T}"/>.
        /// </summary>
        /// <param name="position">The position on this <see cref="ICurveEvaluator{T}"/>.</param>
        /// <returns>Value at the position on this <see cref="ICurveEvaluator{T}"/>.</returns>
        T Evaluate(float position);
    }
}
