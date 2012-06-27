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
	public sealed class SoundEffectInstance : IDisposable
	{
		private bool isDisposed = false;
		private SoundState soundState = SoundState.Stopped;
		private OALSoundBuffer soundBuffer;
		private OpenALSoundController controller;
		private SoundEffect soundEffect;

		float _volume = 1.0f;
		bool _looped = false;
		float _pan = 0;
		float _pitch = 0f;

		bool hasSourceId = false;
		int sourceId;

		public SoundEffectInstance (SoundEffect parent)
		{
			this.soundEffect = parent;
			InitializeSound ();
		}

		private void InitializeSound ()
		{
			controller = OpenALSoundController.GetInstance;
			soundBuffer = new OALSoundBuffer ();
			soundBuffer.BindDataBuffer (soundEffect._data, soundEffect.Format, soundEffect.Size, (int)soundEffect.Rate);
			soundBuffer.Reserved += HandleSoundBufferReserved;
			soundBuffer.Recycled += HandleSoundBufferRecycled;

		}

		void HandleSoundBufferRecycled (object sender, EventArgs e)
		{
			sourceId = 0;
			hasSourceId = false;
			//Console.WriteLine ("recycled: " + soundEffect.Name);
		}

		void HandleSoundBufferReserved (object sender, EventArgs e)
		{
			sourceId = soundBuffer.SourceId;
			hasSourceId = true;
		}

		public void Dispose ()
		{
			soundBuffer.Reserved -= HandleSoundBufferReserved;
			soundBuffer.Recycled -= HandleSoundBufferRecycled;
			soundBuffer.Dispose ();
			soundBuffer = null;
			isDisposed = true;
		}
		
		public void Apply3D (AudioListener listener, AudioEmitter emitter)
		{
			Apply3D ( new AudioListener[] { listener }, emitter);
		}
		
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

		public void Pause ()
		{
			if (hasSourceId) {
				controller.PauseSound (soundBuffer);
				soundState = SoundState.Paused;
			}
		}

        private float XnaPitchToAlPitch(float pitch)
        {
            // pitch is different in XNA and OpenAL. XNA has a pitch between -1 and 1 for one octave down/up.
            // openAL uses 0.5 to 2 for one octave down/up, while 1 is the default. The default value of 0 would make it completely silent.
            float alPitch = 1;
            if (pitch < 0)
                alPitch = 1 + 0.5f * pitch;
            else if (pitch > 0)
                alPitch = 1 + pitch;
            return alPitch;
        }
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
			AL.Source (sourceId, ALSourcef.Gain, _volume);
			// Looping
			AL.Source (sourceId, ALSourceb.Looping, IsLooped);
			// Pitch
			AL.Source (sourceId, ALSourcef.Pitch, XnaPitchToAlPitch(_pitch));
		}

		public void Play ()
		{
			int bufferId = soundBuffer.OpenALDataBuffer;
			if (hasSourceId) {
				return;
			}
			bool isSourceAvailable = controller.ReserveSource (soundBuffer);
			if (!isSourceAvailable)
				return;

			AL.Source (soundBuffer.SourceId, ALSourcei.Buffer, bufferId);
			ApplyState ();

			controller.PlaySound (soundBuffer);
			//Console.WriteLine ("playing: " + sourceId + " : " + soundEffect.Name);
			soundState = SoundState.Playing;
		}

		public void Resume ()
		{
			Play ();
		}

		public void Stop ()
		{
			if (hasSourceId) {
				//Console.WriteLine ("stop " + sourceId + " : " + soundEffect.Name);
				controller.StopSound (soundBuffer);
			}
			soundState = SoundState.Stopped;
		}

		public void Stop (bool immediate)
		{
			Stop ();
		}

		public bool IsDisposed {
			get {
				return isDisposed;
			}
		}

		public bool IsLooped {
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

		private byte[] audioData;

		internal byte[] EffectData {
			get {
				return audioData;
			}

			set {
				audioData = value;
			}
		}

		public SoundState State {
			get {
				return soundState;
			}
		}

		public float Volume {
			get {
				return _volume;
			}
			
			set {
				_volume = value;
				if (hasSourceId) {
					// Volume
					AL.Source (sourceId, ALSourcef.Gain, _volume);
				}

			}
		}	
		
		
	}
}
