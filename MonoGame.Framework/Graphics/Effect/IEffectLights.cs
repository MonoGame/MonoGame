// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
using System;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// The common effect light rendering parameters.
    /// </summary>
	public interface IEffectLights
	{
        /// <summary>
        /// The floating point ambient light color.
        /// </summary>
		Vector3 AmbientLightColor { get; set; }

        /// <summary>
        /// Returns the first directional light.
        /// </summary>
		DirectionalLight DirectionalLight0 { get; }

        /// <summary>
        /// Returns the second directional light.
        /// </summary>
		DirectionalLight DirectionalLight1 { get; }

        /// <summary>
        /// Returns the third directional light.
        /// </summary>
		DirectionalLight DirectionalLight2 { get; }

        /// <summary>
        /// Toggles the rendering of lighting.
        /// </summary>
		bool LightingEnabled { get; set; }

        /// <summary>
        /// Initializes the lights to the standard key/fill/back lighting rig.
        /// </summary>
		void EnableDefaultLighting ();
	}
}

