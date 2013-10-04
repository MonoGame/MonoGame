#region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
#endregion License

#region Using Statements
using System;

#if MONOMAC
using MonoMac.OpenAL;
#else
using OpenTK.Audio.OpenAL;
#endif

using Microsoft.Xna.Framework;

#endregion Statements

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>
    /// Implements the SoundEffectInstance, which is used to access high level features of a SoundEffect. This class uses the OpenAL
    /// sound system to play and control the sound effects. Please refer to the OpenAL 1.x specification from Creative Labs to better
    /// understand the features provides by SoundEffectInstance. 
    /// </summary>
	public class SoundEffectInstance : IDisposable
	{
		private bool isDisposed = false;
		private SoundState soundState = SoundState.Stopped;
		private OALSoundBuffer soundBuffer;
		private OpenALSoundController controller;

        private float _volume = 1.0f;
        private bool _looped = false;
        private float _pan = 0f;
        private float _pitch = 0f;

		bool hasSourceId = false;
		int sourceId;

        /// <summary>
        /// Creates an instance and initializes it.
        /// </summary>
        public SoundEffectInstance()
        {
            InitializeSound();
        }

        ~SoundEffectInstance()
        {
            Dispose();
        }

        /* Creates a standalone SoundEffectInstance from given wavedata. */
        internal SoundEffectInstance(byte[] buffer, int sampleRate, int channels)
        {
            InitializeSound();
            BindDataBuffer(
                buffer,
                (channels == 2) ? ALFormat.Stereo16 : ALFormat.Mono16,
                buffer.Length,
                sampleRate
            );
        }

        /// <summary>
        /// Construct the instance from the given SoundEffect. The data buffer from the SoundEffect is 
        /// preserved in this instance as a reference. This constructor will bind the buffer in OpenAL.
        /// </summary>
        /// <param name="parent"></param>
		public SoundEffectInstance (SoundEffect parent)
		{
			InitializeSound ();
            BindDataBuffer(parent._data, parent.Format, parent.Size, (int)parent.Rate);
		}

        /// <summary>
        /// Gets the OpenAL sound controller, constructs the sound buffer, and sets up the event delegates for
        /// the reserved and recycled events.
        /// </summary>
		private void InitializeSound ()
		{
			controller = OpenALSoundController.GetInstance;
			soundBuffer = new OALSoundBuffer ();			
			soundBuffer.Reserved += HandleSoundBufferReserved;
			soundBuffer.Recycled += HandleSoundBufferRecycled;                        
		}

        /// <summary>
        /// Preserves the given data buffer by reference and binds its contents to the OALSoundBuffer
        /// that is created in the InitializeSound method.
        /// </summary>
        /// <param name="data">The sound data buffer</param>
        /// <param name="format">The sound buffer data format, e.g. Mono, Mono16 bit, Stereo, etc.</param>
        /// <param name="size">The size of the data buffer</param>
        /// <param name="rate">The sampling rate of the sound effect, e.g. 44 khz, 22 khz.</param>
        protected void BindDataBuffer(byte[] data, ALFormat format, int size, int rate)
        {
            soundBuffer.BindDataBuffer(data, format, size, rate);
        }

        /// <summary>
        /// Event handler that resets internal state of this instance. The sound state will report
        /// SoundState.Stopped after this event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void HandleSoundBufferRecycled (object sender, EventArgs e)
		{
			sourceId = 0;
			hasSourceId = false;
			soundState = SoundState.Stopped;
			//Console.WriteLine ("recycled: " + soundEffect.Name);
		}

        /// <summary>
        /// Called after the hardware has allocated a sound buffer, this event handler will
        /// maintain the numberical ID of the source ID.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void HandleSoundBufferReserved (object sender, EventArgs e)
		{
			sourceId = soundBuffer.SourceId;
			hasSourceId = true;
		}

        /// <summary>
        /// Stops the current running sound effect, if relevant, removes its event handlers, and disposes
        /// of the sound buffer.
        /// </summary>
		public void Dispose ()
        {
            if (!isDisposed)
            {
                this.Stop(true);
                soundBuffer.Reserved -= HandleSoundBufferReserved;
                soundBuffer.Recycled -= HandleSoundBufferRecycled;
                soundBuffer.Dispose();
                soundBuffer = null;
                isDisposed = true;
            }
		}
		
        /// <summary>
        /// Wrapper for Apply3D(AudioListener[], AudioEmitter)
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="emitter"></param>
		public void Apply3D (AudioListener listener, AudioEmitter emitter)
		{
			Apply3D ( new AudioListener[] { listener }, emitter);
		}
		
        /// <summary>
        /// Applies a 3D transform on the emitter and the listeners to account for head-up
        /// listening orientation in a 3D surround-sound pseudo-environment. The actual 3D
        /// sound production is handled by OpenAL. This method computes the listener positions
        /// and orientation and hands off the calculations to OpenAL.
        /// </summary>
        /// <param name="listeners"></param>
        /// <param name="emitter"></param>
		public void Apply3D (AudioListener[] listeners, AudioEmitter emitter)
		{
			// get AL's listener position
			float x, y, z;
			AL.GetListener (ALListener3f.Position, out x, out y, out z);

			for (int i = 0; i < listeners.Length; i++)
			{
				AudioListener listener = listeners[i];
				
				// get the emitter offset from origin
				Vector3 posOffset = emitter.Position - listener.Position;
				// set up orientation matrix
				Matrix orientation = Matrix.CreateWorld(Vector3.Zero, listener.Forward, listener.Up);
				// set up our final position and velocity according to orientation of listener
				Vector3 finalPos = new Vector3(x + posOffset.X, y + posOffset.Y, z + posOffset.Z);
				finalPos = Vector3.Transform(finalPos, orientation);
				Vector3 finalVel = emitter.Velocity;
				finalVel = Vector3.Transform(finalVel, orientation);
				
				// set the position based on relative positon
				AL.Source(sourceId, ALSource3f.Position, finalPos.X, finalPos.Y, finalPos.Z);
				AL.Source(sourceId, ALSource3f.Velocity, finalVel.X, finalVel.Y, finalVel.Z);
			}
		}

        /// <summary>
        /// When the sound state is playing and the source is created, this method will pause
        /// the sound playback and set the state to SoundState.Paused. Otherwise, no change is
        /// made to the state of this instance.
        /// </summary>
		public void Pause ()
		{
			if (hasSourceId && soundState == SoundState.Playing)
            {
				controller.PauseSound (soundBuffer);
				soundState = SoundState.Paused;
			}
		}

		/// <summary>
		/// Converts the XNA [-1,1] pitch range to OpenAL (-1,+INF].
        /// <param name="xnaPitch">The pitch of the sound in the Microsoft XNA range.</param>
		/// </summary>
        private float XnaPitchToAlPitch(float xnaPitch)
        {
            /* 
            XNA sets pitch bounds to [-1.0f, 1.0f], each end being one octave.
             •OpenAL's AL_PITCH boundaries are (0.0f, INF). *
             •Consider the function f(x) = 2 ^ x
             •The domain is (-INF, INF) and the range is (0, INF). *
             •0.0f is the original pitch for XNA, 1.0f is the original pitch for OpenAL.
             •Note that f(0) = 1, f(1) = 2, f(-1) = 0.5, and so on.
             •XNA's pitch values are on the domain, OpenAL's are on the range.
             •Remember: the XNA limit is arbitrarily between two octaves on the domain. *
             •To convert, we just plug XNA pitch into f(x). 
                    */
            if (xnaPitch < -1.0f || xnaPitch > 1.0f)
            {
                throw new Exception("XNA PITCH MUST BE WITHIN [-1.0f, 1.0f]!");
            }
            return (float)Math.Pow(2, xnaPitch);
        }

        /// <summary>
        /// Sends the position, gain, looping, pitch, and distance model to the OpenAL driver.
        /// </summary>
		private void ApplyState ()
		{
			if (!hasSourceId)
				return;
			// Distance Model
			AL.DistanceModel (ALDistanceModel.InverseDistanceClamped);
			// Listener
			// Pan
			AL.Source (sourceId, ALSource3f.Position, _pan, 0, 0.1f);
			// Volume
			AL.Source (sourceId, ALSourcef.Gain, _volume * SoundEffect.MasterVolume);
			// Looping
			AL.Source (sourceId, ALSourceb.Looping, IsLooped);
			// Pitch
			AL.Source (sourceId, ALSourcef.Pitch, XnaPitchToAlPitch(_pitch));
		}

        /// <summary>
        /// If no source is ready, then this method does not change the current state of the instance. Otherwise,
        /// if the controller can not reserve the source then InstancePLayLimitException is thrown. Finally, the sound
        /// buffer is sourced to OpenAL, then ApplyState is called and then the sound is set to play. Upon success,
        /// the sound state is set to SoundState.Playing.
        /// </summary>
		public virtual void Play ()
		{
            if (!TryPlay())
                throw new InstancePlayLimitException();
		}

        /// <summary>
        /// Internal implementation of Play that returns false on failure. See comments on Play for workings.
        /// </summary>
        internal bool TryPlay()
        {
            if (hasSourceId) {
                return true;
            }
            bool isSourceAvailable = controller.ReserveSource (soundBuffer);
            if (!isSourceAvailable)
                return false;

            int bufferId = soundBuffer.OpenALDataBuffer;
            AL.Source(soundBuffer.SourceId, ALSourcei.Buffer, bufferId);
            ApplyState ();

            controller.PlaySound (soundBuffer);            
            //Console.WriteLine ("playing: " + sourceId + " : " + soundEffect.Name);
            soundState = SoundState.Playing;

            return true;
        }
        /// <summary>
        /// When the sound state is paused, and the source is available, then the sound
        /// is played using the ResumeSound method from the OpenALSoundController. Otherwise,
        /// the sound is played using the Play() method. Upon success, the sound state should
        /// be SoundState.Playing.
        /// </summary>
		public void Resume ()
		{
            if (hasSourceId)
            {
                if (soundState == SoundState.Paused)
                {
                    controller.ResumeSound(soundBuffer);
                    soundState = SoundState.Playing;
                }
            }
            else
            {
                Play();
            }
		}

        /// <summary>
        /// When the source is available, the sound buffer playback is stopped. Either way,
        /// the state of the instance will always be SoundState.Stopped after this method is
        /// called.
        /// </summary>
		public void Stop ()
		{
			if (hasSourceId) {
				//Console.WriteLine ("stop " + sourceId + " : " + soundEffect.Name);
				controller.StopSound (soundBuffer);
			}
			soundState = SoundState.Stopped;
		}

        /// <summary>
        /// Wrapper for Stop()
        /// </summary>
        /// <param name="immediate">Is not used.</param>
		public void Stop (bool immediate)
		{
			Stop ();
		}

        /// <summary>
        /// returns true if this object has been disposed.
        /// </summary>
		public bool IsDisposed {
			get {
				return isDisposed;
			}
		}

        /// <summary>
        /// Set/get if this sound is looped. When set, and the source is already active, then
        /// the looping setting is applied immediately.
        /// </summary>
		public virtual bool IsLooped {
			get {
				return _looped;
			}

			set {
				_looped = value;
				if (hasSourceId) {
					// Looping
					AL.Source (sourceId, ALSourceb.Looping, _looped);
				}
			}
		}

        /// <summary>
        /// Set/get for sound panning. Sound panning controls the location of the listener in the coordinate space
        /// defined by your world. This method only affects the 'x' coordinate of the listener. The final position of
        /// the listener is (pan, 0, 0.1). 
        /// </summary>
		public float Pan {
			get {
				return _pan;
			}

			set {
				_pan = value;
				if (hasSourceId) {
					// Listener
					// Pan
					AL.Source (sourceId, ALSource3f.Position, _pan, 0.0f, 0.1f);
				}
			}
		}

        /// <summary>
        /// Set/get the pitch (Octave adjustment) of the sound effect. This attribute assumes you are setting
        /// the pitch using the [-1,1] Microsoft XNA pitch range. The pitch will be automatically adjusted
        /// using the XnaPitchToAlPitch method. If the source is active, then the pitch change will
        /// be applied immediately.
        /// </summary>
		public float Pitch {
			get {
				return _pitch;
			}
			set {
				_pitch = value;
				if (hasSourceId) {
					// Pitch
					AL.Source (sourceId, ALSourcef.Pitch, XnaPitchToAlPitch(_pitch));
				}

			}
		}

        /// <summary>
        /// Returns the current state of the SoundEffect.
        /// </summary>
		public SoundState State {
			get {
				return soundState;
			}
		}

        /// <summary>
        /// Get/set the relative volume of this sound effect. The volume is relative to the master
        /// volume (SoundEffect.MasterVolume). The values in this attribute should be [0,1]. If the source
        /// is active, then volume changes will be applied immediately.
        /// </summary>
		public float Volume {
			get {
				return _volume;
			}
			
			set {
				_volume = value;
				if (hasSourceId) {
					// Volume
					AL.Source (sourceId, ALSourcef.Gain, _volume * SoundEffect.MasterVolume);
				}

			}
		}	
		
		
	}
}
