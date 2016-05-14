// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

#if MONOMAC && PLATFORM_MACOS_LEGACY
using MonoMac.OpenAL;
#elif OPENAL
using OpenTK.Audio.OpenAL;
#endif

namespace Microsoft.Xna.Framework.Audio
{
    public partial class SoundEffectInstance : IDisposable
    {
		internal SoundState SoundState = SoundState.Stopped;
		private bool _looped = false;
		private float _alVolume = 1;

		internal int SourceId;
        int pauseCount;
        
        private OpenALSoundController controller;
        
        internal bool HasSourceId = false;

        #region Initialization

        /// <summary>
        /// Creates a standalone SoundEffectInstance from given wavedata.
        /// </summary>
        internal void PlatformInitialize(byte[] buffer, int sampleRate, int channels)
        {
            InitializeSound();
        }

        /// <summary>
        /// Gets the OpenAL sound controller, constructs the sound buffer, and sets up the event delegates for
        /// the reserved and recycled events.
        /// </summary>
        internal void InitializeSound()
        {
            controller = OpenALSoundController.GetInstance;
        }

        #endregion // Initialization

        /// <summary>
        /// Converts the XNA [-1, 1] pitch range to OpenAL pitch (0, INF) or Android SoundPool playback rate [0.5, 2].
        /// <param name="xnaPitch">The pitch of the sound in the Microsoft XNA range.</param>
        /// </summary>
        private static float XnaPitchToAlPitch(float xnaPitch)
        {
            return (float)Math.Pow(2, xnaPitch);
        }

        private void PlatformApply3D(AudioListener listener, AudioEmitter emitter)
        {
            // get AL's listener position
            float x, y, z;
            AL.GetListener(ALListener3f.Position, out x, out y, out z);
            ALHelper.CheckError("Failed to get source position.");

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
            AL.Source(SourceId, ALSource3f.Position, finalPos.X, finalPos.Y, finalPos.Z);
            ALHelper.CheckError("Failed to set source position.");
            AL.Source(SourceId, ALSource3f.Velocity, finalVel.X, finalVel.Y, finalVel.Z);
            ALHelper.CheckError("Failed to Set source velocity.");
        }

        private void PlatformPause()
        {
            if (!HasSourceId || SoundState != SoundState.Playing)
                return;

            if (!controller.CheckInitState())
            {
                return;
            }

            if (pauseCount == 0)
            {
                AL.SourcePause(SourceId);
                ALHelper.CheckError("Failed to pause source.");
            }
            ++pauseCount;
            SoundState = SoundState.Paused;
        }

        private void PlatformPlay()
        {

            SourceId = 0;
            HasSourceId = false;
            SourceId = controller.ReserveSource();
            HasSourceId = true;

            int bufferId = _effect.SoundBuffer.OpenALDataBuffer;
            AL.Source(SourceId, ALSourcei.Buffer, bufferId);
            ALHelper.CheckError("Failed to bind buffer to source.");

            // Send the position, gain, looping, pitch, and distance model to the OpenAL driver.
            if (!HasSourceId)
				return;

			// Distance Model
			AL.DistanceModel (ALDistanceModel.InverseDistanceClamped);
            ALHelper.CheckError("Failed set source distance.");
			// Pan
			AL.Source (SourceId, ALSource3f.Position, _pan, 0, 0.1f);
            ALHelper.CheckError("Failed to set source pan.");
			// Volume
			AL.Source (SourceId, ALSourcef.Gain, _alVolume);
            ALHelper.CheckError("Failed to set source volume.");
			// Looping
			AL.Source (SourceId, ALSourceb.Looping, IsLooped);
            ALHelper.CheckError("Failed to set source loop state.");
			// Pitch
			AL.Source (SourceId, ALSourcef.Pitch, XnaPitchToAlPitch(_pitch));
            ALHelper.CheckError("Failed to set source pitch.");

            AL.SourcePlay(SourceId);
            ALHelper.CheckError("Failed to play source.");

            SoundState = SoundState.Playing;
        }

        private void PlatformResume()
        {
            if (!HasSourceId)
            {
                Play();
                return;
            }

            if (SoundState == SoundState.Paused)
            {
                if (!controller.CheckInitState())
                {
                    return;
                }
                --pauseCount;
                if (pauseCount == 0)
                {
                    AL.SourcePlay(SourceId);
                    ALHelper.CheckError("Failed to play source.");
                }
            }
            SoundState = SoundState.Playing;
        }

        private void PlatformStop(bool immediate)
        {
            if (HasSourceId)
            {
                if (!controller.CheckInitState())
                {
                    return;
                }
                AL.SourceStop(SourceId);
                ALHelper.CheckError("Failed to stop source.");

                AL.Source(SourceId, ALSourcei.Buffer, 0);
                ALHelper.CheckError("Failed to free source from buffer.");

                controller.FreeSource(this);
            }
            SoundState = SoundState.Stopped;
        }

        private void PlatformSetIsLooped(bool value)
        {
            _looped = value;

            if (HasSourceId)
            {
                AL.Source(SourceId, ALSourceb.Looping, _looped);
                ALHelper.CheckError("Failed to set source loop state.");
            }
        }

        private bool PlatformGetIsLooped()
        {
            return _looped;
        }

        private void PlatformSetPan(float value)
        {
            if (HasSourceId)
            {
                AL.Source(SourceId, ALSource3f.Position, value, 0.0f, 0.1f);
                ALHelper.CheckError("Failed to set source pan.");
            }
        }

        private void PlatformSetPitch(float value)
        {
            if (HasSourceId)
            {
                AL.Source(SourceId, ALSourcef.Pitch, XnaPitchToAlPitch(value));
                ALHelper.CheckError("Failed to set source pitch.");
            }
        }

        private SoundState PlatformGetState()
        {
            if (!HasSourceId)
                return SoundState.Stopped;
            
            var alState = AL.GetSourceState(SourceId);
            ALHelper.CheckError("Failed to get source state.");

            switch (alState)
            {
                case ALSourceState.Initial:
                case ALSourceState.Stopped:
                    SoundState = SoundState.Stopped;
                    break;

                case ALSourceState.Paused:
                    SoundState = SoundState.Paused;
                    break;

                case ALSourceState.Playing:
                    SoundState = SoundState.Playing;
                    break;
            }

            return SoundState;
        }

        private void PlatformSetVolume(float value)
        {
            _alVolume = value;

            if (HasSourceId)
            {
                AL.Source(SourceId, ALSourcef.Gain, _alVolume);
                ALHelper.CheckError("Failed to set source volume.");
            }
        }

        private void PlatformDispose(bool disposing)
        {
            
        }
    }
}
