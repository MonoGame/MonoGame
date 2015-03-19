// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// The common effect fog rendering parameters.
    /// </summary>
	public interface IEffectFog
	{
        /// <summary>
        /// Gets or set the fog color in floating point format.
        /// </summary>
		Vector3 FogColor { get; set; }

        /// <summary>
        /// Toggles the rendering of fog.
        /// </summary>
		bool FogEnabled { get; set; }

        /// <summary>
        /// Gets or sets the world space distance from the camera at which fogging is fully applied.
        /// </summary>
        /// <remarks>
        /// FogEnd should be greater than FogStart. If FogEnd and FogStart 
        /// are the same value everything is fully fogged.
        /// </remarks>
		float FogEnd { get; set; }

        /// <summary>
        /// Gets or sets the world space distance from the camera at which fogging begins.
        /// </summary>
        /// <remarks>
        /// FogStart should be less than FogEnd. If FogEnd and FogStart are the
        /// same value everything is fully fogged.
        /// </remarks>
		float FogStart { get; set; }
	}
}