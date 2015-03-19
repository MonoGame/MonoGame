// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// The common effect matrix parameters.
    /// </summary>
	public interface IEffectMatrices
    { 
        /// <summary>
        /// Gets or sets the projection matrix.
        /// </summary>
		Matrix Projection { get; set; }

        /// <summary>
        /// Gets or sets the view matrix.
        /// </summary>
		Matrix View { get; set; }

        /// <summary>
        /// Gets or sets the world matrix.
        /// </summary>
		Matrix World { get; set; }
	}
}