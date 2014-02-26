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
#if !DIRECTX
using System.IO;
#endif
#endregion Statements

namespace Microsoft.Xna.Framework.Audio
{
    public sealed partial class SoundEffectInstance : IDisposable
    {
        private bool isDisposed = false;
#if !DIRECTX
        //private SoundState soundState = SoundState.Stopped;
#endif

#if ANDROID
        private int _streamId = -1;
#endif

#if !DIRECTX
        /*private Sound _sound;
		internal Sound Sound 
		{ 
			get
			{
				return _sound;
			} 
			
			set
			{
				_sound = value;
			} 
		}*/
#endif

        public bool IsLooped
        { 
            get { return PlatformGetIsLooped(); }
            set { PlatformSetIsLooped(value); }
        }

        public float Pan
        {
            get { return PlatformGetPan(); } 
            set { PlatformSetPan(value); }
        }

        public float Pitch
        {
            get { return PlatformGetPitch(); }
            set { PlatformSetPitch(value); }
        }

        public SoundState State { get { return PlatformGetState(); } }

        public bool IsDisposed { get { return isDisposed; } }

        public float Volume
        {
            get { return PlatformGetVolume(); } 
            set { PlatformSetVolume(value); }
        }

#if !DIRECTX && !(WINDOWS && OPENGL) && !LINUX && !MONOMAC

        internal SoundEffectInstance(){}

        /* Creates a standalone SoundEffectInstance from given wavedata. */
        internal SoundEffectInstance(byte[] buffer, int sampleRate, int channels)
        {
            // buffer should contain 16-bit PCM wave data
            short bitsPerSample = 16;

            using (var mStream = new MemoryStream(44+buffer.Length))
            using (var writer = new BinaryWriter(mStream))
            {
                writer.Write("RIFF".ToCharArray()); //chunk id
                writer.Write((int)(36 + buffer.Length)); //chunk size
                writer.Write("WAVE".ToCharArray()); //RIFF type

                writer.Write("fmt ".ToCharArray()); //chunk id
                writer.Write((int)16); //format header size
                writer.Write((short)1); //format (PCM)
                writer.Write((short)channels);
                writer.Write((int)sampleRate);
                short blockAlign = (short)((bitsPerSample / 8) * (int)channels);
                writer.Write((int)(sampleRate * blockAlign)); //byte rate
                writer.Write((short)blockAlign);
                writer.Write((short)bitsPerSample);

                writer.Write("data".ToCharArray()); //chunk id
                writer.Write((int)buffer.Length); //data size

                writer.Write(buffer);

                _sound = new Sound(mStream.ToArray(), 1.0f, false);
                _sound.Rate = sampleRate;
            }
        }
#endif

        public void Apply3D(AudioListener listener, AudioEmitter emitter)
        {
            PlatformApply3D(listener, emitter);
        }

        public void Apply3D(AudioListener[] listeners, AudioEmitter emitter)
        {
            foreach (var l in listeners)
                Apply3D(l, emitter);
        }

        public void Pause()
        {
            PlatformPause();
        }

        public void Play()
        {
            if (State == SoundState.Playing)
                return;

            PlatformPlay();
        }

        /// <summary>
        /// Tries to play the sound, returns true if successful
        /// </summary>
        /// <returns></returns>
        internal bool TryPlay()
        {
            Play();
#if ANDROID
			return _streamId != 0;
#else
            return true;
#endif
        }

        public void Resume()
        {
            PlatformResume();
        }

        public void Stop()
        {
            PlatformStop(true);
        }

        public void Stop(bool immediate)
        {
            PlatformStop(immediate);
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            PlatformDispose();

            isDisposed = true;
        }
    }
}
