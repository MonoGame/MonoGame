// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
using System;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// The common effect fog rendering parameters.
    /// </summary>
	public interface IEffectFog
	{
        /// <summary>
        /// The floating point fog color.
        /// </summary>
		Vector3 FogColor { get; set; }

        /// <summary>
        /// Used to toggle the rendering of fog.
        /// </summary>
		bool FogEnabled { get; set; }

        /// <summary>
        /// The world space distance from the camera at which fogging is fully applied.
        /// </summary>
        /// <remarks>
        /// FogEnd should be greater than FogStart.  If FogEnd and FogStart 
        /// are the same value everything is fully fogged.
        /// </remarks>
		float FogEnd { get; set; }

        /// <summary>
        /// The world space distance from the camera at which fogging begins.
        /// </summary>
        /// <remarks>
        /// FogStart should be less than FogEnd.  If FogEnd and FogStart are the
        /// same value everything is fully fogged.
        /// </remarks>
		float FogStart { get; set; }
	}
}

