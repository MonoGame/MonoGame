// MonoGame - Copyright (C) MonoGame Foundation, Inc
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
        readonly string _name;
        readonly AudioEngine _engine;
        readonly List<XactSound> _sounds;

        // This is a bit gross, but we use an array here
        // instead of a field since AudioCategory is a struct
        // This allows us to save _volume when the user
        // holds onto a reference of AudioCategory, or when a cue
        // is created/loaded after the volume's already been set.
        internal float[] _volume;

        internal bool isBackgroundMusic;
        internal bool isPublic;

        internal bool instanceLimit;
        internal int maxInstances;
        internal MaxInstanceBehavior InstanceBehavior;

        internal CrossfadeType fadeType;
        internal float fadeIn;
        internal float fadeOut;

        
        internal AudioCategory (AudioEngine audioengine, string name, BinaryReader reader)
        {
            Debug.Assert(audioengine != null);
            Debug.Assert(!string.IsNullOrEmpty(name));

            _sounds = new List<XactSound>();
            _name = name;
            _engine = audioengine;

            maxInstances = reader.ReadByte ();
            instanceLimit = maxInstances != 0xff;

            fadeIn = (reader.ReadUInt16 () / 1000f);
            fadeOut = (reader.ReadUInt16 () / 1000f);

            byte instanceFlags = reader.ReadByte ();
            fadeType = (CrossfadeType)(instanceFlags & 0x7);
            InstanceBehavior = (MaxInstanceBehavior)(instanceFlags >> 3);

            reader.ReadUInt16 (); //unkn

            var volume = XactHelpers.ParseVolumeFromDecibels(reader.ReadByte());
            _volume = new float[1] { volume };

            byte visibilityFlags = reader.ReadByte ();
            isBackgroundMusic = (visibilityFlags & 0x1) != 0;
            isPublic = (visibilityFlags & 0x2) != 0;
        }

        internal void AddSound(XactSound sound)
        {
            _sounds.Add(sound);
        }

        internal int GetPlayingInstanceCount()
        {
            var sum = 0;
            for (var i = 0; i < _sounds.Count; i++)
            {
                if (_sounds[i].Playing)
                    sum++;
            }
            return sum;
        }

        internal XactSound GetOldestInstance()
        {
            for (var i = 0; i < _sounds.Count; i++)
            {
                if (_sounds[i].Playing)
                    return _sounds[i];
            }
            return null;
        }

        /// <summary>
        /// Gets the category's friendly name.
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Pauses all associated sounds.
        /// </summary>
        public void Pause ()
        {
            foreach (var sound in _sounds)
                sound.Pause();
        }

        /// <summary>
        /// Resumes all associated paused sounds.
        /// </summary>
        public void Resume ()
        {
            foreach (var sound in _sounds)
                sound.Resume();
        }

        /// <summary>
        /// Stops all associated sounds.
        /// </summary>
        public void Stop(AudioStopOptions options)
        {
            foreach (var sound in _sounds)
                sound.Stop(options);
        }

        /// <summary>
        /// Set the volume for this <see cref="AudioCategory"/>.
        /// </summary>
        /// <param name="volume">The new volume of the category.</param>
        /// <exception cref="ArgumentException">If the volume is less than zero.</exception>
        public void SetVolume(float volume)
        {
            if (volume < 0)
                throw new ArgumentException("The volume must be positive.");

            // Updating all the sounds in a category can be
            // very expensive... so avoid it if we can.
            if (_volume[0] == volume)
                return;

            _volume[0] = volume;

            foreach (var sound in _sounds)
                sound.UpdateCategoryVolume(volume);
        }

        /// <summary>
        /// Determines whether two AudioCategory instances are equal.
        /// </summary>
        /// <param name="first">First AudioCategory instance to compare.</param>
        /// <param name="second">Second AudioCategory instance to compare.</param>
        /// <returns>true if the objects are equal or false if they aren't.</returns>
        public static bool operator ==(AudioCategory first, AudioCategory second)
        {
            return first._engine == second._engine && first._name.Equals(second._name, StringComparison.Ordinal);
        }

        /// <summary>
        /// Determines whether two AudioCategory instances are not equal.
        /// </summary>
        /// <param name="first">First AudioCategory instance to compare.</param>
        /// <param name="second">Second AudioCategory instance to compare.</param>
        /// <returns>true if the objects are not equal or false if they are.</returns>
        public static bool operator !=(AudioCategory first, AudioCategory second)
        {
            return first._engine != second._engine || !first._name.Equals(second._name, StringComparison.Ordinal);
        }

        /// <summary>
        /// Determines whether two AudioCategory instances are equal.
        /// </summary>
        /// <param name="other">AudioCategory to compare with this instance.</param>
        /// <returns>true if the objects are equal or false if they aren't</returns>
        public bool Equals(AudioCategory other)
        {
            return _engine == other._engine && _name.Equals(other._name, StringComparison.Ordinal);
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
                return _engine == other._engine && _name.Equals(other._name, StringComparison.Ordinal);
            }

            return false;
        }

        /// <summary>
        /// Gets the hash code for this instance.
        /// </summary>
        /// <returns>Hash code for this object.</returns>
        public override int GetHashCode()
        {
            return _name.GetHashCode() ^ _engine.GetHashCode();
        }

        /// <summary>
        /// Returns the name of this AudioCategory
        /// </summary>
        /// <returns>Friendly name of the AudioCategory</returns>
        public override string ToString()
        {
            return _name;
        }
    }
}

