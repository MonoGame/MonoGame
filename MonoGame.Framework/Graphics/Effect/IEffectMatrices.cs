// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
using System;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Gets or sets transformation matrix parameters for the current effect.
    /// </summary>
	public interface IEffectMatrices
    {
        /// <summary>
        /// Gets or sets the projection matrix in the current effect.
        /// </summary>
		Matrix Projection { get; set; }
        /// <summary>
        /// Gets or sets the view matrix in the current effect.
        /// </summary>
		Matrix View { get; set; }

        /// <summary>
        /// Gets or sets the world matrix in the current effect.
        /// </summary>
		Matrix World { get; set; }
    }
}