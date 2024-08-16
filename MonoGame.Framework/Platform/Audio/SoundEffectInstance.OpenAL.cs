// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using MonoGame.OpenAL;

namespace Microsoft.Xna.Framework.Audio
{
    public partial class SoundEffectInstance : IDisposable
    {
		internal SoundState SoundState = SoundState.Stopped;
		private bool _looped = false;
		private float _alVolume = 1f;

		internal int SourceId;
        private float reverb = 0f;
        bool applyFilter = false;
        EfxFilterType filterType;
        float filterQ;
        float frequency;
        int pauseCount;

        internal readonly object sourceMutex = new object();
        
        internal OpenALSoundController controller;
        
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
            controller = OpenALSoundController.Instance;
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
            if (!HasSourceId)
                return;
            // get AL's listener position
            float x, y, z;
            AL.GetListener(ALListener3f.Position, out x, out y, out z);
            ALHelper.CheckError("Failed to get source position.");

            // get the emitter offset from origin
            Vector3 posOffset = emitter.Position - listener.Position;
            // set up matrix to transform world space coordinates to listener space coordinates
            Matrix worldSpaceToListenerSpace = Matrix.Transpose(Matrix.CreateWorld(Vector3.Zero, listener.Forward, listener.Up));
            // set up our final position and velocity according to orientation of listener
            Vector3 finalPos = new Vector3(x + posOffset.X, y + posOffset.Y, z + posOffset.Z);
            finalPos = Vector3.Transform(finalPos, worldSpaceToListenerSpace);
            Vector3 finalVel = emitter.Velocity - listener.Velocity;
            finalVel = Vector3.Transform(finalVel, worldSpaceToListenerSpace);

            // set the position based on relative positon
            AL.Source(SourceId, ALSource3f.Position, finalPos.X, finalPos.Y, finalPos.Z);
            ALHelper.CheckError("Failed to set source position.");
            AL.Source(SourceId, ALSource3f.Velocity, finalVel.X, finalVel.Y, finalVel.Z);
            ALHelper.CheckError("Failed to set source velocity.");

            AL.Source(SourceId, ALSourcef.ReferenceDistance, SoundEffect.DistanceScale);
            ALHelper.CheckError("Failed to set source distance scale.");
            AL.DopplerFactor(SoundEffect.DopplerScale);
            ALHelper.CheckError("Failed to set Doppler scale.");
        }

        private void PlatformPause()
        {
            if (!HasSourceId || SoundState != SoundState.Playing)
                return;

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
            if (HasSourceId)
                PlatformStop(true);
            SourceId = controller.ReserveSource();
            HasSourceId = true;

            int bufferId = _effect.SoundBuffer.OpenALDataBuffer;
            AL.Source(SourceId, ALSourcei.Buffer, bufferId);
            ALHelper.CheckError("Failed to bind buffer to source.");

            // Send the position, gain, looping, pitch, and distance model to the OpenAL driver.
            if (!HasSourceId)
				return;

            AL.Source(SourceId, ALSourcei.SourceRelative, 1);
            ALHelper.CheckError("Failed set source relative.");
            // Distance Model
			AL.DistanceModel (ALDistanceModel.InverseDistanceClamped);
            ALHelper.CheckError("Failed set source distance.");
			// Pan
			AL.Source (SourceId, ALSource3f.Position, _pan, 0f, 0f);
            ALHelper.CheckError("Failed to set source pan.");
            // Velocity
			AL.Source (SourceId, ALSource3f.Velocity, 0f, 0f, 0f);
            ALHelper.CheckError("Failed to set source pan.");
			// Volume
            AL.Source(SourceId, ALSourcef.Gain, _alVolume);
            ALHelper.CheckError("Failed to set source volume.");
			// Looping
			AL.Source (SourceId, ALSourceb.Looping, IsLooped);
            ALHelper.CheckError("Failed to set source loop state.");
			// Pitch
			AL.Source (SourceId, ALSourcef.Pitch, XnaPitchToAlPitch(_pitch));
            ALHelper.CheckError("Failed to set source pitch.");

            ApplyReverb ();
            ApplyFilter ();

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
            FreeSource();
            if (pauseCount > 0) pauseCount = 0;
            SoundState = SoundState.Stopped;
        }

        private void FreeSource()
        {
            if (!HasSourceId)
                return;

            lock (sourceMutex)
            {
                if (HasSourceId && AL.IsSource(SourceId))
                {
                    AL.SourceStop(SourceId);
                    ALHelper.CheckError("Failed to stop source.");

                    // Reset the SendFilter to 0 if we are NOT using reverb since
                    // sources are recycled
                    if (OpenALSoundController.Instance.SupportsEfx)
                    {
                        OpenALSoundController.Efx.BindSourceToAuxiliarySlot(SourceId, 0, 0, 0);
                        ALHelper.CheckError("Failed to unset reverb.");
                        AL.Source(SourceId, ALSourcei.EfxDirectFilter, 0);
                        ALHelper.CheckError("Failed to unset filter.");
                    }

                    controller.FreeSource(this);
                }
            }
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

        internal void PlatformSetReverbMix(float mix)
        {
            if (!OpenALSoundController.Efx.IsInitialized)
                return;
            reverb = mix;
            if (State == SoundState.Playing)
            {
                ApplyReverb();
                reverb = 0f;
            }
        }

        void ApplyReverb()
        {
            if (reverb > 0f && SoundEffect.ReverbSlot != 0)
            {
                OpenALSoundController.Efx.BindSourceToAuxiliarySlot(SourceId, (int)SoundEffect.ReverbSlot, 0, 0);
                ALHelper.CheckError("Failed to set reverb.");
            }
        }

        void ApplyFilter()
        {
            if (applyFilter && controller.Filter > 0)
            {
                var freq = frequency / 20000f;
                var lf = 1.0f - freq;
                var efx = OpenALSoundController.Efx;
                efx.Filter(controller.Filter, EfxFilteri.FilterType, (int)filterType);
                ALHelper.CheckError("Failed to set filter.");
                switch (filterType)
                {
                case EfxFilterType.Lowpass:
                    efx.Filter(controller.Filter, EfxFilterf.LowpassGainHF, freq);
                    ALHelper.CheckError("Failed to set LowpassGainHF.");
                    break;
                case EfxFilterType.Highpass:
                    efx.Filter(controller.Filter, EfxFilterf.HighpassGainLF, freq);
                    ALHelper.CheckError("Failed to set HighpassGainLF.");
                    break;
                case EfxFilterType.Bandpass:
                    efx.Filter(controller.Filter, EfxFilterf.BandpassGainHF, freq);
                    ALHelper.CheckError("Failed to set BandpassGainHF.");
                    efx.Filter(controller.Filter, EfxFilterf.BandpassGainLF, lf);
                    ALHelper.CheckError("Failed to set BandpassGainLF.");
                    break;
                }
                AL.Source(SourceId, ALSourcei.EfxDirectFilter, controller.Filter);
                ALHelper.CheckError("Failed to set DirectFilter.");
            }
        }

        internal void PlatformSetFilter(FilterMode mode, float filterQ, float frequency)
        {
            if (!OpenALSoundController.Efx.IsInitialized)
                return;

            applyFilter = true;
            switch (mode)
            {
            case FilterMode.BandPass:
                filterType = EfxFilterType.Bandpass;
                break;
                case FilterMode.LowPass:
                filterType = EfxFilterType.Lowpass;
                break;
                case FilterMode.HighPass:
                filterType = EfxFilterType.Highpass;
                break;
            }
            this.filterQ = filterQ;
            this.frequency = frequency;
            if (State == SoundState.Playing)
            {
                ApplyFilter();
                applyFilter = false;
            }
        }

        internal void PlatformClearFilter()
        {
            if (!OpenALSoundController.Efx.IsInitialized)
                return;

            applyFilter = false;
        }

        private void PlatformDispose(bool disposing)
        {
            FreeSource();
        }
    }
}
