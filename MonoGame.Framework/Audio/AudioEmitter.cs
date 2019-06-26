// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>
    /// Represents a 3D audio emitter. Used to simulate 3D audio effects.
    /// </summary>
	public class AudioEmitter
	{
        /// <summary>Initializes a new AudioEmitter instance.</summary>
		public AudioEmitter ()
		{
            _dopplerScale = 1.0f;
			Forward = Vector3.Forward;
			Position = Vector3.Zero;
			Up = Vector3.Up;
			Velocity = Vector3.Zero;
		}

        private float _dopplerScale;

        /// <summary>Gets or sets a scale applied to the Doppler effect between the AudioEmitter and an AudioListener.</summary>
        /// <remarks>
        /// <para>Defaults to 1.0</para>
        /// <para>A value of 1.0 leaves the Doppler effect unmodified.</para>
        /// </remarks>
		public float DopplerScale 
        {
            get
            {
                return _dopplerScale;
            }

            set
            {
                if (value < 0.0f)
                    throw new ArgumentOutOfRangeException("AudioEmitter.DopplerScale must be greater than or equal to 0.0f");

                _dopplerScale = value;
            }
		}

        /// <summary>Gets or sets the emitter's forward vector.</summary>
        /// <remarks>
        /// <para>Defaults to Vector3.Forward. (new Vector3(0, 0, -1))</para>
        /// <para>Used with AudioListener.Velocity to calculate Doppler values.</para>
        /// <para>The Forward and Up values must be orthonormal.</para>
        /// </remarks>
		public Vector3 Forward {
			get;
			set;
		}

        /// <summary>Gets or sets the position of this emitter.</summary>
        public Vector3 Position {
			get;
			set;
		}

        /// <summary>Gets or sets the emitter's Up vector.</summary>
        /// <remarks>
        /// <para>Defaults to Vector3.Up. (new Vector3(0, -1, 1)).</para>
        /// <para>The Up and Forward vectors must be orthonormal.</para>
        /// </remarks>
		public Vector3 Up {
			get;
			set;
		}

        /// <summary>Gets or sets the emitter's velocity vector.</summary>
        /// <remarks>
        /// <para>Defaults to Vector3.Zero.</para>
        /// <para>This value is only used when calculating Doppler values.</para>
        /// </remarks>
		public Vector3 Velocity {
			get;
			set;
		}

	}
}
