// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

#if MONOMAC
using MonoMac.OpenAL;
#elif OPENAL
using OpenTK.Audio.OpenAL;
#endif

namespace Microsoft.Xna.Framework.Audio
{
    public partial class SoundEffectInstance : IDisposable
    {
		private SoundState soundState = SoundState.Stopped;
		private bool _looped = false;
		int sourceId;

        private OALSoundBuffer soundBuffer;
        private OpenALSoundController controller;
        
        bool hasSourceId = false;

        #region Initialization

        /// <summary>
        /// Creates a standalone SoundEffectInstance from given wavedata.
        /// </summary>
        internal void PlatformInitialize(byte[] buffer, int sampleRate, int channels)
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
        /// Preserves the given data buffer by reference and binds its contents to the OALSoundBuffer
        /// that is created in the InitializeSound method.
        /// </summary>
        /// <param name="data">The sound data buffer</param>
        /// <param name="format">The sound buffer data format, e.g. Mono, Mono16 bit, Stereo, etc.</param>
        /// <param name="size">The size of the data buffer</param>
        /// <param name="rate">The sampling rate of the sound effect, e.g. 44 khz, 22 khz.</param>
        internal void BindDataBuffer(byte[] data, ALFormat format, int size, int rate)
        {
            soundBuffer.BindDataBuffer(data, format, size, rate);
        }

        /// <summary>
        /// Gets the OpenAL sound controller, constructs the sound buffer, and sets up the event delegates for
        /// the reserved and recycled events.
        /// </summary>
        internal void InitializeSound()
        {
            controller = OpenALSoundController.GetInstance;
            soundBuffer = new OALSoundBuffer();
            soundBuffer.Reserved += HandleSoundBufferReserved;
            soundBuffer.Recycled += HandleSoundBufferRecycled;
        }

        /// <summary>
        /// Event handler that resets internal state of this instance. The sound state will report
        /// SoundState.Stopped after this event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleSoundBufferRecycled(object sender, EventArgs e)
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
        private void HandleSoundBufferReserved(object sender, EventArgs e)
        {
            sourceId = soundBuffer.SourceId;
            hasSourceId = true;
        }

        #endregion // Initialization

        /// <summary>
        /// Converts the XNA [-1, 1] pitch range to OpenAL pitch (0, INF) or Android SoundPool playback rate [0.5, 2].
        /// <param name="xnaPitch">The pitch of the sound in the Microsoft XNA range.</param>
        /// </summary>
        private static float XnaPitchToAlPitch(float xnaPitch)
        {
            /*XNA sets pitch bounds to [-1.0f, 1.0f], each end being one octave.
            •OpenAL's AL_PITCH boundaries are (0.0f, INF). *
            •Consider the function f(x) = 2 ^ x
            •The domain is (-INF, INF) and the range is (0, INF). *
            •0.0f is the original pitch for XNA, 1.0f is the original pitch for OpenAL.
            •Note that f(0) = 1, f(1) = 2, f(-1) = 0.5, and so on.
            •XNA's pitch values are on the domain, OpenAL's are on the range.
            •Remember: the XNA limit is arbitrarily between two octaves on the domain. *
            •To convert, we just plug XNA pitch into f(x).*/

            if (xnaPitch < -1.0f || xnaPitch > 1.0f)
                throw new ArgumentOutOfRangeException("XNA PITCH MUST BE WITHIN [-1.0f, 1.0f]!");

            return (float)Math.Pow(2, xnaPitch);
        }

        private void PlatformApply3D(AudioListener listener, AudioEmitter emitter)
        {
            // get AL's listener position
            float x, y, z;
            AL.GetListener(ALListener3f.Position, out x, out y, out z);

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

        private void PlatformPause()
        {
            if (!hasSourceId || soundState != SoundState.Playing)
                return;

            controller.PauseSound(soundBuffer);
            soundState = SoundState.Paused;
        }

        private void PlatformPlay()
        {
            if (hasSourceId)
                return;
            
            bool isSourceAvailable = controller.ReserveSource (soundBuffer);
            if (!isSourceAvailable)
                throw new InstancePlayLimitException();

            int bufferId = soundBuffer.OpenALDataBuffer;
            AL.Source(soundBuffer.SourceId, ALSourcei.Buffer, bufferId);

            // Send the position, gain, looping, pitch, and distance model to the OpenAL driver.
            if (!hasSourceId)
				return;

			// Distance Model
			AL.DistanceModel (ALDistanceModel.InverseDistanceClamped);
			// Pan
			AL.Source (sourceId, ALSource3f.Position, _pan, 0, 0.1f);
			// Volume
			AL.Source (sourceId, ALSourcef.Gain, _volume * SoundEffect.MasterVolume);
			// Looping
			AL.Source (sourceId, ALSourceb.Looping, IsLooped);
			// Pitch
			AL.Source (sourceId, ALSourcef.Pitch, XnaPitchToAlPitch(_pitch));

            controller.PlaySound (soundBuffer);
            //Console.WriteLine ("playing: " + sourceId + " : " + soundEffect.Name);
            soundState = SoundState.Playing;
        }

        private void PlatformResume()
        {
            if (!hasSourceId)
            {
                Play();
                return;
            }
            
            if (soundState == SoundState.Paused)
                controller.ResumeSound(soundBuffer);
            soundState = SoundState.Playing;
        }

        private void PlatformStop(bool immediate)
        {
            if (hasSourceId)
            {
                //Console.WriteLine ("stop " + sourceId + " : " + soundEffect.Name);
                controller.StopSound(soundBuffer);
            }
            soundState = SoundState.Stopped;
        }

        private void PlatformSetIsLooped(bool value)
        {
            _looped = value;
            
            if (hasSourceId)
                AL.Source(sourceId, ALSourceb.Looping, _looped);
        }

        private bool PlatformGetIsLooped()
        {
            return _looped;
        }

        private void PlatformSetPan(float value)
        {
            if (hasSourceId)
                AL.Source(sourceId, ALSource3f.Position, value, 0.0f, 0.1f);
        }

        private void PlatformSetPitch(float value)
        {
            if (hasSourceId)
                AL.Source (sourceId, ALSourcef.Pitch, XnaPitchToAlPitch(value));
        }

        private SoundState PlatformGetState()
        {
            if (!hasSourceId)
                return SoundState.Stopped;
            
            var alState = AL.GetSourceState(sourceId);

            switch (alState)
            {
                case ALSourceState.Initial:
                case ALSourceState.Stopped:
                    soundState = SoundState.Stopped;
                    break;

                case ALSourceState.Paused:
                    soundState = SoundState.Paused;
                    break;

                case ALSourceState.Playing:
                    soundState = SoundState.Playing;
                    break;
            }

            return soundState;
        }

        private void PlatformSetVolume(float value)
        {
            if (hasSourceId)
                AL.Source(sourceId, ALSourcef.Gain, value * SoundEffect.MasterVolume);
        }

        private void PlatformDispose(bool disposing)
        {
            if (disposing)
            {
                if (soundBuffer != null)
                {
                    this.Stop(true);
                    soundBuffer.Reserved -= HandleSoundBufferReserved;
                    soundBuffer.Recycled -= HandleSoundBufferRecycled;
                    soundBuffer.Dispose();
                    soundBuffer = null;
                }
            }
        }
    }
}
