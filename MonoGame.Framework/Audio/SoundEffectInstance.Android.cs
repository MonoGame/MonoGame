// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Android.Media;

namespace Microsoft.Xna.Framework.Audio
{
    public partial class SoundEffectInstance : IDisposable
    {
		private SoundState soundState = SoundState.Stopped;
		private bool _looped = false;
        int streamId = 0;
        internal int _soundId = -1;

        private static SoundPool s_soundPool = new SoundPool(SoundEffectInstancePool.MAX_PLAYING_INSTANCES, Android.Media.Stream.Music, 0);
        internal static SoundPool SoundPool { get { return s_soundPool; } }

        #region Initialization

        /// <summary>
        /// Creates a standalone SoundEffectInstance from given wavedata.
        /// </summary>
        internal void PlatformInitialize(byte[] buffer, int sampleRate, int channels)
        {
            // No-op on Android
        }

        #endregion // Initialization

        /// <summary>
        /// Converts the XNA volume [0, 1] and pan [-1, 1] to Android SoundPool left and right volume [0, 1].
        /// <param name="xnaVolume">The volume of the sound in the Microsoft XNA range.</param>
        /// <param name="xnaPan">The pan of the sound in the Microsoft XNA range.</param>
        /// <param name="leftVolume">Android SoundPool left volume.</param>
        /// <param name="rightVolume">Android SoundPool right volume.</param>
        /// </summary>
        private static void XnaVolumeAndPanToAndroidVolume(float xnaVolume, float xnaPan, out float leftVolume, out float rightVolume)
        {
            float panRatio = (xnaPan + 1.0f) / 2.0f;
            float volumeTotal = SoundEffect.MasterVolume * xnaVolume;
            leftVolume = volumeTotal * (1.0f - panRatio);
            rightVolume = volumeTotal * panRatio;
        }

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
			// Appears to be a no-op on Android?
        }

        private void PlatformPause()
        {
			if (streamId == 0)
				return;

			s_soundPool.Pause(streamId);
            soundState = SoundState.Paused;
        }

        private void PlatformPlay()
        {
			if (soundState == SoundState.Paused)
				s_soundPool.Resume(streamId);
			else
			{
				if (streamId != 0)
				{
					s_soundPool.Stop(streamId);
					streamId = 0;
				}

                float volumeLeft, volumeRight;
                XnaVolumeAndPanToAndroidVolume(_volume, _pan, out volumeLeft, out volumeRight);

                float playbackRate = XnaPitchToAlPitch(_pitch);

                streamId = s_soundPool.Play(_soundId, volumeLeft, volumeRight, 1, _looped ? -1 : 0, playbackRate);
			}
            soundState = SoundState.Playing;
        }

        private void PlatformResume()
        {
            if (soundState == SoundState.Paused)
            {
				if (streamId == 0)
					return;

				s_soundPool.Resume(streamId);
            }
            soundState = SoundState.Playing;
        }

        private void PlatformStop(bool immediate)
        {
            if (streamId == 0)
                return;

			s_soundPool.Stop(streamId);
			streamId = 0;
            soundState = SoundState.Stopped;
        }

        private void PlatformSetIsLooped(bool value)
        {
            if (streamId != 0)
                s_soundPool.SetLoop(streamId, value ? -1 : 0);
            _looped = value;
        }

        private bool PlatformGetIsLooped()
        {
            return _looped;
        }

        private void PlatformSetPan(float value)
        {
            if (streamId != 0)
            {
                float leftVolume, rightVolume;
                XnaVolumeAndPanToAndroidVolume(_volume, value, out leftVolume, out rightVolume);
                s_soundPool.SetVolume(streamId, leftVolume, rightVolume);
            }
        }

        private void PlatformSetPitch(float value)
        {
            if (streamId != 0)
                s_soundPool.SetRate(streamId, XnaPitchToAlPitch(value));
        }

        private SoundState PlatformGetState()
        {
            // Android SoundPool can't tell us when a sound is finished playing.
            // TODO: Remove this code when OpenAL for Android is implemented
			if (streamId != 0 && IsLooped)
            {
                // Looping sounds use our stored state
                return soundState;
            }
            else
            {
                // Non looping sounds always return Stopped
                return SoundState.Stopped;
            }
        }

        private void PlatformSetVolume(float value)
        {
            if (streamId != 0)
            {
                float leftVolume, rightVolume;
                XnaVolumeAndPanToAndroidVolume(value, _pan, out leftVolume, out rightVolume);
                s_soundPool.SetVolume(streamId, leftVolume, rightVolume);
            }
        }

        private void PlatformDispose(bool disposing)
        {
            // When disposing a SoundEffectInstance, the Sound should
            // just be stopped as it will likely be reused later
            if (streamId != 0)
            {
                s_soundPool.Stop(streamId);
                streamId = 0;
            }
        }
    }
}
