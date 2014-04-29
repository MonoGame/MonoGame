// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;


namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>Defines methods for managing the playback of sounds.</summary>
    /// <remarks>
    /// <para>Cues are what programmers use to play sounds. Cues are typically played when certain game events occur, such as footsteps or gunshots. A cue is composed of one or more sounds, so when the cue is triggered, the set of associated sounds is heard.</para>
    /// <para>A sound specifies how one or more waves should be played. A sound also has specific properties such as volume and pitch. The sound designer can adjust these properties.</para>
    /// <para>The advantage to using the Audio API to reference cues rather than specific sounds is that an audio designer can reassign sounds to a cue in the sound bank without programmer intervention. For example, a sound designer can try various gunshot waves associated with a particular game event without requiring the programmer to change code or rename sounds.</para>
    /// <para>Cues and sounds are referenced through SoundBank objects. The waves that compose a sound are referenced through WaveBank objects.</para>
    /// </remarks>
	public class Cue : IDisposable
	{
		AudioEngine engine;
		string name;
		XactSound[] sounds;
		float[] probs;
		XactSound curSound;
		Random variationRand;
		
		float volume = 1.0f;

        /// <summary>Returns whether the cue is currently paused.</summary>
        /// <remarks>IsPlaying and IsPaused both return true if a cue is paused while playing.</remarks>
		public bool IsPaused
		{
			get {
				if (curSound != null)
					return curSound.IsPaused;
				return true;
			}
		}

        /// <summary>Returns whether the cue is playing.</summary>
        /// <remarks>IsPlaying and IsPaused both return true if a cue is paused while playing.</remarks>
		public bool IsPlaying
		{
			get {
				if (curSound != null) {
					return curSound.Playing;
				}
				return false;
			}
		}

        /// <summary>Returns whether the cue is currently stopped.</summary>
		public bool IsStopped
		{
			get {
				if (curSound != null) {
					return !curSound.Playing;
				}
				return true;
			}
		}

        /// <summary>Returns the friendly name of the cue.</summary>
        /// <remarks>The friendly name is a text string that the designer can associate with the cue when it is built.</remarks>
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
			
			variationRand = new Random();
		}
		
		internal Cue(string cuename, XactSound[] _sounds, float[] _probs)
		{
			name = cuename;
			sounds = _sounds;
			probs = _probs;
			
			variationRand = new Random();
		}

        /// <summary>Pauses playback.</summary>
		public void Pause()
		{
			if (curSound != null) {
				curSound.Pause();
			}
		}

        /// <summary>Requests playback of a prepared or preparing Cue.</summary>
        /// <remarks>Calling Play when the Cue already is playing can result in an InvalidOperationException.</remarks>
		public void Play()
		{
			//TODO: Probabilities
			curSound = sounds[variationRand.Next (sounds.Length)];
			
			curSound.Volume = volume;
			curSound.Play ();
		}

        /// <summary>Resumes playback of a paused Cue.</summary>
		public void Resume()
		{
			if (curSound != null) {
				curSound.Resume ();
			}
		}

        /// <summary>Stops playback of a Cue.</summary>
        /// <param name="options">Enumerated value specifying how the sound should stop. If set to None, the sound will play any release phase or transition specified in the audio designer. If set to Immediate, the sound will stop immediately, ignoring any release phases or transitions.</param>
		public void Stop(AudioStopOptions options)
		{
			if (curSound != null) {
				curSound.Stop();
			}
		}
		
        /// <summary>
        /// Sets the value of a cue-instance variable based on its friendly name.
        /// </summary>
        /// <param name="name">Friendly name of the variable to set.</param>
        /// <param name="value">Value to assign to the variable.</param>
        /// <remarks>The friendly name is a text string that the designer can associate with the cue when it is built.</remarks>
		public void SetVariable (string name, float value)
		{
			if (name == "Volume") {
				volume = value;
				if (curSound != null) {
					curSound.Volume = value;
				}
			} else {
				engine.SetGlobalVariable (name, value);
			}
		}

        /// <summary>Gets a cue-instance variable value based on its friendly name.</summary>
        /// <param name="name">Friendly name of the variable.</param>
        /// <returns>Value of the variable.</returns>
        /// <remarks>
        /// <para>Cue-instance variables are useful when multiple instantiations of a single cue (and its associated sounds) are required (for example, a "car" cue where there may be more than one car at any given time). While a global variable allows multiple audio elements to be controlled in unison, a cue instance variable grants discrete control of each instance of a cue, even for each copy of the same cue.</para>
        /// <para>The friendly name is a text string that the designer can associate with the cue when it is built.</para>
        /// </remarks>
		public float GetVariable (string name, float value)
		{
			if (name == "Volume") {
				return volume;
			} else {
				return engine.GetGlobalVariable (name);
			}
		}

        /// <summary>Calculates the 3D audio values between an AudioEmitter and an AudioListener object, and applies the resulting values to this Cue.</summary>
        /// <param name="listener">The listener to calculate.</param>
        /// <param name="emitter">The emitter to calculate.</param>
        /// <remarks>
        /// <para>If you want to apply 3D effects to a Cue, you must call this method before you call the Play method. Not doing so will throw an exception the next time Apply3D is called.</para>
        /// <para>Calling this method automatically sets the speaker mix for any sound played by this cue to a value calculated by the difference in Position values between listener and emitter. In preparation for the mix, the sound is converted to monoaural. Any stereo information in the sound is discarded.</para>
        /// <para>Calling this method sets the cue-instance variables Distance, DopplerPitchScalar, and OrientationAngle to the resulting values of the 3D calculation between listener and emitter. These values do not modify sound attenuation over distance, or pitch shifting using Doppler values, on their own. You must set up a Runtime Parameter Curve (RPC) that defines how to map the cue-instance variable values to pitch and volume shifts, and associate sounds to these curves in the Microsoft Cross-Platform Audio Creation Tool (XACT). For information on doing so, see Applying 3D Audio Effects (XACT).</para>
        /// </remarks>
		public void Apply3D(AudioListener listener, AudioEmitter emitter) {
			
		}

        /// <summary>Gets a value indicating whether the object has been disposed.</summary>
		public bool IsDisposed { get { return false; } }
		
		
		
		#region IDisposable implementation
        /// <summary>Immediately releases the unmanaged resources used by this object.</summary>
		public void Dispose ()
		{
			//_sound.Dispose();
		}
		#endregion
	}
}

