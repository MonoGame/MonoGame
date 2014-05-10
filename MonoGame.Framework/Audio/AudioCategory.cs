// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>
    /// Provides functionality for manipulating multiple sounds at a time.
    /// </summary>
	public struct AudioCategory : IEquatable<AudioCategory>
	{
		string name;
		AudioEngine engine;

		internal float volume;
		internal bool isBackgroundMusic;
		internal bool isPublic;

		internal bool instanceLimit;
		internal int maxInstances;

		List<XactSound> sounds;

		//insatnce limiting behaviour
		internal enum MaxInstanceBehaviour {
			FailToPlay,
			Queue,
			ReplaceOldest,
			ReplaceQuietest,
			ReplaceLowestPriority,
		}
		internal MaxInstanceBehaviour instanceBehaviour;

		internal enum CrossfadeType {
			Linear,
			Logarithmic,
			EqualPower,
		}
		internal CrossfadeType fadeType;
		internal float fadeIn;
		internal float fadeOut;

		
		internal AudioCategory (AudioEngine audioengine, string name, BinaryReader reader)
		{
		    Debug.Assert(audioengine != null);
            Debug.Assert(!string.IsNullOrEmpty(name));

			this.sounds = new List<XactSound>();
			this.name = name;
			engine = audioengine;

			maxInstances = reader.ReadByte ();
			instanceLimit = maxInstances != 0xff;

			fadeIn = (reader.ReadUInt16 () / 1000f);
			fadeOut = (reader.ReadUInt16 () / 1000f);

			byte instanceFlags = reader.ReadByte ();
			fadeType = (CrossfadeType)(instanceFlags & 0x7);
			instanceBehaviour = (MaxInstanceBehaviour)(instanceFlags >> 3);

			reader.ReadUInt16 (); //unkn

			byte vol = reader.ReadByte (); //volume in unknown format
			//lazy 4-param fitting:
			//0xff 6.0
			//0xca 2.0
			//0xbf 1.0
			//0xb4 0.0
			//0x8f -4.0
			//0x5a -12.0
			//0x14 -38.0
			//0x00 -96.0
			var a = -96.0;
			var b = 0.432254984608615;
			var c = 80.1748600297963;
			var d = 67.7385212334047;
			volume = (float)(((a-d)/(1+(Math.Pow(vol/c, b)))) + d);

			byte visibilityFlags = reader.ReadByte ();
			isBackgroundMusic = (visibilityFlags & 0x1) != 0;
			isPublic = (visibilityFlags & 0x2) != 0;
		}

		internal void AddSound(XactSound sound)
		{
			sounds.Add(sound);
		}

        /// <summary>
        /// Gets the category's friendly name.
        /// </summary>
		public string Name { get { return name; } }

        /// <summary>
        /// Pauses all associated sounds.
        /// </summary>
		public void Pause ()
		{
			foreach (var sound in sounds)
				sound.Pause();
		}

        /// <summary>
        /// Resumes all associated paused sounds.
        /// </summary>
		public void Resume ()
		{
			foreach (var sound in sounds)
				sound.Resume();
		}

        /// <summary>
        /// Stops all associated sounds.
        /// </summary>
		public void Stop ()
		{
			foreach (var sound in sounds)
				sound.Stop();
		}

        /// <summary>
        /// Sets the volume of all sounds associated with this category.
        /// </summary>
        /// <param name="volume">Volume scale applied to all sounds. A value of 1.0 is full volume.</param>
		public void SetVolume(float volume) {
			foreach (var sound in sounds)
				sound.Volume = volume;
		}

        /// <summary>
        /// Determines whether two AudioCategory instances are equal.
        /// </summary>
        /// <param name="first">First AudioCategory instance to compare.</param>
        /// <param name="second">Second AudioCategory instance to compare.</param>
        /// <returns>true if the objects are equal or false if they aren't.</returns>
        public static bool operator ==(AudioCategory first, AudioCategory second)
        {
            return first.engine == second.engine && first.name.Equals(second.name, StringComparison.Ordinal);
        }

        /// <summary>
        /// Determines whether two AudioCategory instances are not equal.
        /// </summary>
        /// <param name="first">First AudioCategory instance to compare.</param>
        /// <param name="second">Second AudioCategory instance to compare.</param>
        /// <returns>true if the objects are not equal or false if they are.</returns>
        public static bool operator !=(AudioCategory first, AudioCategory second)
	    {
            return first.engine != second.engine || !first.name.Equals(second.name, StringComparison.Ordinal);
	    }

        /// <summary>
        /// Determines whether two AudioCategory instances are equal.
        /// </summary>
        /// <param name="other">AudioCategory to compare with this instance.</param>
        /// <returns>true if the objects are equal or false if they aren't</returns>
	    public bool Equals(AudioCategory other)
		{
            return engine == other.engine && name.Equals(other.name, StringComparison.Ordinal);
		}

        /// <summary>
        /// Determines whether two AudioCategory instances are equal.
        /// </summary>
        /// <param name="obj">Object to compare with this instance.</param>
        /// <returns>true if the objects are equal or false if they aren't.</returns>
        public override bool Equals(object obj)
        {
            if (obj is AudioCategory)
            {
                var other = (AudioCategory)obj;
                return engine == other.engine && name.Equals(other.name, StringComparison.Ordinal);
            }

            return false;
        }

        /// <summary>
        /// Gets the hash code for this instance.
        /// </summary>
        /// <returns>Hash code for this object.</returns>
        public override int GetHashCode()
        {
            return name.GetHashCode() ^ engine.GetHashCode();
        }

        /// <summary>
        /// Returns the name of this AudioCategory
        /// </summary>
        /// <returns>Friendly name of the AudioCategory</returns>
        public override string ToString()
        {
            return name;
        }
	}
}

