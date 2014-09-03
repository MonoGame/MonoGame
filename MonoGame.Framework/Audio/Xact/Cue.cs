// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>Manages the playback of a sound or set of sounds.</summary>
    /// <remarks>
    /// <para>Cues are comprised of one or more sounds.</para>
    /// <para>Cues also define specific properties such as pitch or volume.</para>
    /// <para>Cues are referenced through SoundBank objects.</para>
    /// </remarks>
	public class Cue : IDisposable
	{
		AudioEngine engine;
		string name;
		XactSound[] sounds;
		float[] probs;
		XactSound curSound;
		
		float volume = 1.0f;

        /// <summary>Indicates whether or not the cue is currently paused.</summary>
        /// <remarks>IsPlaying and IsPaused both return true if a cue is paused while playing.</remarks>
		public bool IsPaused
		{
			get 
            {
				if (curSound != null)
					return curSound.IsPaused;

				return false;
			}
		}

        /// <summary>Indicates whether or not the cue is currently playing.</summary>
        /// <remarks>IsPlaying and IsPaused both return true if a cue is paused while playing.</remarks>
		public bool IsPlaying
		{
			get 
            {
				if (curSound != null)
					return curSound.Playing;

				return false;
			}
		}

        /// <summary>Indicates whether or not the cue is currently stopped.</summary>
		public bool IsStopped
		{
			get 
            {
				if (curSound != null)
                    return curSound.Stopped;

				return true;
			}
		}

        public bool IsStopping
        {
            get
            {
                // TODO: Implement me!
                return false;
            }
        }

        public bool IsPreparing
        {
            get
            {
                // TODO: Implement me!
                return false;
            }
        }

        public bool IsPrepared
        {
            get
            {
                // TODO: Implement me!
                return false;
            }
        }

        /// <summary>Gets the friendly name of the cue.</summary>
        /// <remarks>The friendly name is a value set from the designer.</remarks>
		public string Name
		{
			get { return name; }
		}
		
		internal Cue (AudioEngine engine, string cuename, XactSound sound)
		{
			this.engine = engine;
			name = cuename;
			sounds = new XactSound[1];
			sounds[0] = sound;
			
			probs = new float[1];
			probs[0] = 1.0f;
		}
		
		internal Cue(AudioEngine engine, string cuename, XactSound[] _sounds, float[] _probs)
		{
            this.engine = engine;
			name = cuename;
			sounds = _sounds;
			probs = _probs;
		}

        /// <summary>Pauses playback.</summary>
		public void Pause()
		{
			if (curSound != null)
				curSound.Pause();
		}

        /// <summary>Requests playback of a prepared or preparing Cue.</summary>
        /// <remarks>Calling Play when the Cue already is playing can result in an InvalidOperationException.</remarks>
		public void Play()
		{
            if (!engine._activeCues.Contains(this))
                engine._activeCues.Add(this);
			
			//TODO: Probabilities
            var index = XactHelpers.Random.Next(sounds.Length);
            curSound = sounds[index];
			
			curSound.Volume = volume;
			curSound.Play();
		}

        /// <summary>Resumes playback of a paused Cue.</summary>
		public void Resume()
		{
			if (curSound != null)
				curSound.Resume();
		}

        /// <summary>Stops playback of a Cue.</summary>
        /// <param name="options">Specifies if the sound should play any pending release phases or transitions before stopping.</param>
		public void Stop(AudioStopOptions options)
		{
            engine._activeCues.Remove(this);
			
			if (curSound != null)
                curSound.Stop(options);
		}
		
        /// <summary>
        /// Sets the value of a cue-instance variable based on its friendly name.
        /// </summary>
        /// <param name="name">Friendly name of the variable to set.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <remarks>The friendly name is a value set from the designer.</remarks>
		public void SetVariable (string name, float value)
		{
			if (name == "Volume") 
            {
				volume = value;
				if (curSound != null)
					curSound.Volume = value;
			} 
            else
            {
				engine.SetGlobalVariable (name, value);
			}
		}

        /// <summary>Gets a cue-instance variable value based on its friendly name.</summary>
        /// <param name="name">Friendly name of the variable.</param>
        /// <returns>Value of the variable.</returns>
        /// <remarks>
        /// <para>Cue-instance variables are useful when multiple instantiations of a single cue (and its associated sounds) are required (for example, a "car" cue where there may be more than one car at any given time). While a global variable allows multiple audio elements to be controlled in unison, a cue instance variable grants discrete control of each instance of a cue, even for each copy of the same cue.</para>
        /// <para>The friendly name is a value set from the designer.</para>
        /// </remarks>
		public float GetVariable (string name)
		{
			if (name == "Volume")
				return volume;

            return engine.GetGlobalVariable (name);
		}

        /// <summary>Updates the simulated 3D Audio settings calculated between an AudioEmitter and AudioListener.</summary>
        /// <param name="listener">The listener to calculate.</param>
        /// <param name="emitter">The emitter to calculate.</param>
        /// <remarks>
        /// <para>This must be called before Play().</para>
        /// <para>Calling this method automatically converts the sound to monoaural and sets the speaker mix for any sound played by this cue to a value calculated with the listener's and emitter's positions. Any stereo information in the sound will be discarded.</para>
        /// </remarks>
		public void Apply3D(AudioListener listener, AudioEmitter emitter) 
        {
            if (curSound != null)
                curSound.Apply3D(listener, emitter);			
        }

        internal void Update(float dt)
        {
            if (curSound != null)
                curSound.Update(dt);
        }
		
		
		/// <summary>Indicateds whether or not the object has been disposed.</summary>
		public bool IsDisposed { get { return false; } }
		
		#region IDisposable implementation
        /// <summary>Immediately releases any unmanaged resources used by this object.</summary>
		public void Dispose ()
		{
		}
		#endregion
	}
}

