// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>
    /// Represents a 3D audio listener. Used when simulating 3D Audio.
    /// </summary>
	public class AudioListener
	{
		public AudioListener ()
		{
			Forward = Vector3.Forward;
			Position = Vector3.Zero;
			Up = Vector3.Up;
			Velocity = Vector3.Zero;
		}

        /// <summary>Gets or sets the listener's forward vector.</summary>
        /// <remarks>
        /// <para>Defaults to Vector3.Forward. (new Vector3(0, 0, -1))</para>
        /// <para>Used with AudioListener.Velocity and AudioEmitter.Velocity to calculate Doppler values.</para>
        /// <para>The Forward and Up vectors must be orthonormal.</para>
        /// </remarks>
		public Vector3 Forward {
			get;
			set;
		}

        /// <summary>Gets or sets the listener's position.</summary>
        /// <remarks>
        /// Defaults to Vector3.Zero.
        /// </remarks>
		public Vector3 Position {
			get;
			set;
		}

        /// <summary>
        /// Gets or sets the listener's up vector..
        /// </summary>
        /// <remarks>
        /// <para>Defaults to Vector3.Up (New Vector3(0, -1, 0)).</para>
        /// <para>Used with AudioListener.Velocity and AudioEmitter.Velocity to calculate Doppler values.</para>
        /// <para>The values of the Forward and Up vectors must be orthonormal.</para>
        /// </remarks>
		public Vector3 Up {
			get;
			set;
		}

        /// <summary>Gets or sets the listener's velocity vector.</summary>
        /// <remarks>
        /// <para>Defaults to Vector3.Zero.</para>
        /// <para>Scaled by DopplerScale to calculate the Doppler effect value applied to a Cue.</para>
        /// <para>This value is only used to calculate Doppler values.</para>
        /// </remarks>
		public Vector3 Velocity {
			get;
			set;
		}
	}
}

