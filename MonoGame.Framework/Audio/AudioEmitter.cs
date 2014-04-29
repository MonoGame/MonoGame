// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>
    /// Represents a 3D audio emitter. This object, used in combination with an AudioListener,
    /// can simulate 3D audio effects for a given Cue or SoundEffectInstance.
    /// </summary>
	public class AudioEmitter
	{
        /// <summary>Initializes a new instance of this class.</summary>
		public AudioEmitter ()
		{
            _dopplerScale = 1.0f;
			Forward = Vector3.Forward;
			Position = Vector3.Zero;
			Up = Vector3.Up;
			Velocity = Vector3.Zero;
		}

        private float _dopplerScale;

        /// <summary>Gets or sets a scalar applied to the level of Doppler effect calculated between this and any AudioListener.</summary>
        /// <remarks>
        /// <para>By default, this value is 1.0.</para>
        /// <para>This value determines how much to modify the calculated Doppler effect between this object and a AudioListener. Values below 1.0 scale down the Doppler effect to make it less apparent. Values above 1.0 exaggerate the Doppler effect. A value of 1.0 leaves the effect unmodified.</para>
        /// <para>Note that this value modifies only the calculated Doppler between this object and a AudioListener. The calculated Doppler is a product of the relationship between AudioEmitter.Velocity and AudioListener.Velocity. If the calculation yields a result of no Doppler effect, this value has no effect.</para>
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

        /// <summary>Gets or sets the forward orientation vector for this emitter.</summary>
        /// <remarks><para>By default, this value is Vector3.Forward. In most cases, it is not necessary to modify this value.</para>
        /// <para>Doppler values between an AudioEmitter and an AudioListener are calculated by the relationship between AudioListener.Velocity and AudioEmitter.Velocity with respect to the axes defined by the Forward and Up vectors of each.</para>
        /// <para>The values of the Forward and Up vectors must be orthonormal (at right angles to one another). Behavior is undefined if these vectors are not orthonormal.</para></remarks>
		public Vector3 Forward {
			get;
			set;
		}

        /// <summary>Gets or sets the position of this emitter.</summary>
        /// <remarks>
        /// <para>By default, the value of this property is Vector3.Zero.</para>
        /// <para>The relative positions of an AudioEmitter and an AudioListener are used to determine speaker positioning of a sound.</para>
        /// </remarks>
        public Vector3 Position {
			get;
			set;
		}

        /// <summary>Gets or sets the upward orientation vector for this emitter.</summary>
        /// <remarks>
        /// <para>By default, this value is Vector3.Up. In most cases, it is not necessary to modify this value.</para>
        /// <para>Doppler values between an AudioEmitter and an AudioListener are calculated by the relationship between AudioListener.Velocity and AudioEmitter.Velocity with respect to the axes defined by the Forward and Up vectors of each.</para>
        /// <para>The values of the Forward and Up vectors must be orthonormal (at right angles to one another). Behavior is undefined if these vectors are not orthonormal.</para>
        /// </remarks>
		public Vector3 Up {
			get;
			set;
		}

        /// <summary>Gets or sets the velocity vector of this emitter.</summary>
        /// <remarks><para>By default, the value of this property is Vector3.Zero.</para>
        /// <para>The Doppler effect value applied to a Cue is based on the relative Velocity values of the AudioEmitter and AudioListener, scaled by the DopplerScale value.</para>
        /// <para>The Velocity property is used only to calculate Doppler values. It is not applied to the Position vector or otherwise used to set game state. You must set Velocity and Position each frame to maintain accurate 3D audio values in your game.</para></remarks>
		public Vector3 Velocity {
			get;
			set;
		}

	}
}
