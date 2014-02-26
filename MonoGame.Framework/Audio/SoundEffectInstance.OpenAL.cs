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
using System.IO;

#if MONOMAC
using MonoMac.OpenAL;
#else
using OpenTK.Audio.OpenAL;
#endif
#endregion Statements

namespace Microsoft.Xna.Framework.Audio
{
    public sealed partial class SoundEffectInstance : IDisposable
    {

#if (WINDOWS && OPENGL) || LINUX || MONOMAC

        private SoundState soundState = SoundState.Stopped;
        private OALSoundBuffer soundBuffer;
        private OpenALSoundController controller;

        private float _volume = 1.0f;
        private bool _looped = false;
        private float _pan = 0f;
        private float _pitch = 0f;

        bool hasSourceId = false;
        int sourceId;

#endif

        #region Initialization


#if (WINDOWS && OPENGL) || LINUX || MONOMAC

        /// <summary>
        /// Creates a standalone SoundEffectInstance from given wavedata.
        /// </summary>
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
        public SoundEffectInstance(SoundEffect parent)
        {
            InitializeSound();
            BindDataBuffer(parent._data, parent.Format, parent.Size, (int)parent.Rate);
        }

        /// <summary>
        /// Preserves the given data buffer by reference and binds its contents to the OALSoundBuffer
        /// that is created in the InitializeSound method.
        /// </summary>
        /// <param name="data">The sound data buffer</param>
        /// <param name="format">The sound buffer data format, e.g. Mono, Mono16 bit, Stereo, etc.</param>
        /// <param name="size">The size of the data buffer</param>
        /// <param name="rate">The sampling rate of the sound effect, e.g. 44 khz, 22 khz.</param>
        [CLSCompliant(false)]
        protected void BindDataBuffer(byte[] data, ALFormat format, int size, int rate)
        {
            soundBuffer.BindDataBuffer(data, format, size, rate);
        }

        /// <summary>
        /// Gets the OpenAL sound controller, constructs the sound buffer, and sets up the event delegates for
        /// the reserved and recycled events.
        /// </summary>
        private void InitializeSound()
        {
            controller = OpenALSoundController.GetInstance;
            soundBuffer = new OALSoundBuffer();
            soundBuffer.Reserved += HandleSoundBufferReserved;
            soundBuffer.Recycled += HandleSoundBufferRecycled;
        }

#endif

        #endregion


#if (WINDOWS && OPENGL) || LINUX || MONOMAC

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

        /// <summary>
        /// Converts the XNA [-1,1] pitch range to OpenAL (-1,+INF].
        /// <param name="xnaPitch">The pitch of the sound in the Microsoft XNA range.</param>
        /// </summary>
        private float XnaPitchToAlPitch(float xnaPitch)
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

#endif

        private void PlatformApply3D(AudioListener listener, AudioEmitter emitter)
        {

#if (WINDOWS && OPENGL) || LINUX || MONOMAC

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
#endif
        }

        private void PlatformPause()
        {

#if (WINDOWS && OPENGL) || LINUX || MONOMAC

            if (!hasSourceId || soundState != SoundState.Playing)
                return;

            controller.PauseSound(soundBuffer);
#else
            if (_sound == null)
                return;
#if ANDROID
			_sound.Pause(_streamId);
#else
            _sound.Pause();
#endif
#endif
            soundState = SoundState.Paused;
            
        }

        private void PlatformPlay()
        {
#if (WINDOWS && OPENGL) || LINUX || MONOMAC

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
#else

            if (_sound== null)
                return;
#if ANDROID
			if (soundState == SoundState.Paused)
				_sound.Resume(_streamId);
			else
				_streamId = _sound.Play();
#else
            if (soundState == SoundState.Paused)
                _sound.Resume();
            else
                _sound.Play();
#endif
#endif
            soundState = SoundState.Playing;
        }

        private void PlatformResume()
        {

#if (WINDOWS && OPENGL) || LINUX || MONOMAC

            if (!hasSourceId)
            {
                Play();
                return;
            }
            
            if (soundState == SoundState.Paused)
                controller.ResumeSound(soundBuffer);
#else
            if (_sound == null)
                return;
            
            if (soundState == SoundState.Paused)
            {
#if ANDROID
				_sound.Resume(_streamId);
#else
                _sound.Resume();
#endif
            }
#endif
            soundState = SoundState.Playing;
        }

        private void PlatformStop(bool immediate)
        {

#if (WINDOWS && OPENGL) || LINUX || MONOMAC

            if (hasSourceId)
            {
                //Console.WriteLine ("stop " + sourceId + " : " + soundEffect.Name);
                controller.StopSound(soundBuffer);
            }
#else
            if (_sound == null)
                return;
            
#if ANDROID
            _sound.Stop(_streamId);
            _streamId = -1;
#else
            _sound.Stop();
#endif
#endif
            soundState = SoundState.Stopped;
        }

        private void PlatformSetIsLooped(bool value)
        {

#if (WINDOWS && OPENGL) || LINUX || MONOMAC
            
            _looped = value;
            
            if (hasSourceId)
                AL.Source(sourceId, ALSourceb.Looping, _looped);
            
#else
            if (_sound != null && _sound.Looping != value)
                _sound.Looping = value;
#endif
        }

        private bool PlatformGetIsLooped()
        {
#if (WINDOWS && OPENGL) || LINUX || MONOMAC
            
            return _looped;
#else

            if (_sound != null)
                return _sound.Looping;
            
            return false;
#endif
        }

        private void PlatformSetPan(float value)
        {

#if (WINDOWS && OPENGL) || LINUX || MONOMAC

            _pan = value;
            if (hasSourceId)
                AL.Source(sourceId, ALSource3f.Position, _pan, 0.0f, 0.1f);

#else

            if (_sound != null && _sound.Pan != value)
                _sound.Pan = value;
#endif
        }

        private float PlatformGetPan()
        {

#if (WINDOWS && OPENGL) || LINUX || MONOMAC

            return _pan;
#else
            if (_sound == null)
                return 0.0f;
            
            return _sound.Pan;
#endif
        }

        private void PlatformSetPitch(float value)
        {
#if (WINDOWS && OPENGL) || LINUX || MONOMAC
            _pitch = value;

			if (hasSourceId)
				AL.Source (sourceId, ALSourcef.Pitch, XnaPitchToAlPitch(_pitch));
#else
            if (_sound != null && _sound.Rate != value)
                _sound.Rate = value;
#endif
        }

        private float PlatformGetPitch()
        {
#if (WINDOWS && OPENGL) || LINUX || MONOMAC

            return _pitch;
#else
            if (_sound == null)
                return 0.0f;
            
            return _sound.Rate
#endif
        }

        private SoundState PlatformGetState()
        {

#if (WINDOWS && OPENGL) || LINUX || MONOMAC

            return soundState;

#elif ANDROID
            // Android SoundPool can't tell us when a sound is finished playing.
            // TODO: Remove this code when OpenAL for Android is implemented
            if (_sound != null && IsLooped)
            {
                // Looping sounds use our stored state
                return soundState;
            }
            else
            {
                // Non looping sounds always return Stopped
                return SoundState.Stopped;
            }
#else
            if (_sound != null && soundState == SoundState.Playing && !_sound.Playing)
                soundState = SoundState.Stopped;

            return soundState;
#endif
        }

        private void PlatformSetVolume(float value)
        {

#if (WINDOWS && OPENGL) || LINUX || MONOMAC

            _volume = value;
			if (hasSourceId)
				AL.Source (sourceId, ALSourcef.Gain, _volume * SoundEffect.MasterVolume);

#else
            if (_sound != null && _sound.Volume != value)
                _sound.Volume = value;
#endif
        }

        private float PlatformGetVolume()
        {

#if (WINDOWS && OPENGL) || LINUX || MONOMAC

            return _volume;
#else
            if (_sound == null)
                return 0.0f;
            
            return _sound.Volume;

#endif
        }

        private void PlatformDispose()
        {
#if (WINDOWS && OPENGL) || LINUX || MONOMAC

            this.Stop(true);
            soundBuffer.Reserved -= HandleSoundBufferReserved;
            soundBuffer.Recycled -= HandleSoundBufferRecycled;
            soundBuffer.Dispose();
            soundBuffer = null;

#else
            // When disposing a SoundEffectInstance, the Sound should
            // just be stopped as it will likely be reused later
            _sound.Stop();
#endif
        }
    }
}
