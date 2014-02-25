#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License
﻿
using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

#if (WINDOWS && OPENGL) || LINUX
using OpenTK.Audio.OpenAL;
#elif IOS
using MonoTouch.AudioToolbox;
using MonoTouch.AudioUnit;
using OpenTK.Audio.OpenAL;
#elif MONOMAC
using MonoMac.AudioToolbox;
using MonoMac.AudioUnit;

using MonoMac.OpenAL;
#endif

namespace Microsoft.Xna.Framework.Audio
{
    public sealed partial class SoundEffect : IDisposable
    {
        internal byte[] _data;


#if IOS || MONOMAC

        private List<SoundEffectInstance> playing = null;
        private List<SoundEffectInstance> available = null;
        private List<SoundEffectInstance> toBeRecycled = null;

#endif

#if (WINDOWS && OPENGL) || LINUX || IOS || MONOMAC

        private TimeSpan _duration = TimeSpan.Zero;

        internal int Size { get; set; }

        internal float Rate { get; set; }

        internal ALFormat Format { get; set; }
#else
        private Sound _sound;
        private SoundEffectInstance _instance;
#endif

        #region Public Constructors

        private void PlatformLoadAudioStream(Stream s)
        {

#if (WINDOWS && OPENGL) || LINUX
            
            ALFormat format;
            int size;
            int freq;

            _data = AudioLoader.Load(s, out format, out size, out freq);

            Format = format;
            Size = size;
            Rate = freq;

#elif (MONOMAC || IOS)

            AudioFileStream afs = new AudioFileStream (AudioFileType.WAVE);
            afs.ParseBytes (audiodata, false); // AudioFileStreamStatus status
            AudioStreamBasicDescription asbd = afs.StreamBasicDescription;
            
            Rate = (float)asbd.SampleRate;
            Size = (int)afs.DataByteCount;
            
            if (asbd.ChannelsPerFrame == 1)
                Format = asbd.BitsPerChannel == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
            else
                Format = asbd.BitsPerChannel == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;

            byte []d = new byte[afs.DataByteCount];
            Array.Copy (audiodata, afs.DataOffset, d, 0, afs.DataByteCount);

            _data = d;

            var _dblDuration = (Size / ((asbd.BitsPerChannel / 8) * ((asbd.ChannelsPerFrame == 0) ? 1 : asbd.ChannelsPerFrame))) / asbd.SampleRate;
            _duration = TimeSpan.FromSeconds(_dblDuration);

			afs.Close ();
#else

            _data = new byte[s.Length];
            s.Read(_data, 0, (int)s.Length);
            _sound = new Sound(_data, 1.0f, false);
#endif
        }

        private void PlatformInitialize(byte[] buffer, int sampleRate, AudioChannels channels)
        {

#if (WINDOWS && OPENGL) || LINUX

            _data = buffer;
            Size = buffer.Length;
            Format = (channels == AudioChannels.Stereo) ? ALFormat.Stereo16 : ALFormat.Mono16;
            Rate = sampleRate;

#elif MONOMAC || IOS

            //buffer should contain 16-bit PCM wave data
            short bitsPerSample = 16;

            Rate = (float)sampleRate;
            Size = (int)buffer.Length;

            if ((int)channels <= 1)
                Format = bitsPerSample == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
            else
                Format = bitsPerSample == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;

            var _dblDuration = (Size / ((bitsPerSample / 8) * (((int)channels == 0) ? 1 : (int)channels))) / Rate;
            _duration = TimeSpan.FromSeconds(_dblDuration);

            _name = "";
            _data = buffer;

#else
            //buffer should contain 16-bit PCM wave data
            short bitsPerSample = 16;

            _name = "";

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
                writer.Write((int)buffer.Length); //data size   MonoGame.Framework.Windows8.DLL!Microsoft.Xna.Framework.Audio.Sound.Sound(byte[] audiodata, float volume, bool looping) Line 199    C#

                writer.Write(buffer);

                _data = mStream.ToArray();
            }

            _sound = new Sound(_data, 1.0f, false);
#endif
        }

        private void PlatformInitialize(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int loopStart, int loopLength)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Additional SoundEffect/SoundEffectInstance Creation Methods

        private SoundEffectInstance PlatformCreateInstance()
        {
#if (WINDOWS && OPENGL) || LINUX
            return new SoundEffectInstance(this);
#else
            var instance = new SoundEffectInstance();
            instance.Sound = _sound;
            return instance;
#endif
        }

        #endregion

        #region Play

        private bool PlatformPlay()
        {
#if (WINDOWS && OPENGL) || LINUX || MONOMAC || IOS
            return PlatformPlay(MasterVolume, 0.0f, 0.0f);
#else
            return PlatformPlay(1.0f, 0.0f, 0.0f);
#endif
        }

        private bool PlatformPlay(float volume, float pitch, float pan)
        {
            if (MasterVolume <= 0.0f)
                return false;

#if (WINDOWS && OPENGL) || LINUX

            SoundEffectInstance instance = PlatformCreateInstance();
            instance.Volume = volume;
            instance.Pitch = pitch;
            instance.Pan = pan;
            instance.Play();

#elif MONOMAC || IOS
            
			if (playing == null)
            {
				playing = new List<SoundEffectInstance>();
				available = new List<SoundEffectInstance>();
				toBeRecycled = new List<SoundEffectInstance>();
			}
			else
            {
				// Lets cycle through our playing list and see if any are stopped
				// so that we can recycle them.
				if (playing.Count > 0)
                {
					foreach(var instance2 in playing)
                    {
						if (instance2.State == SoundState.Stopped)
							toBeRecycled.Add(instance2);
					}
				}
			}

			SoundEffectInstance instance = null;
			if (toBeRecycled.Count > 0)
            {
				foreach(var recycle in toBeRecycled)
                {
					available.Add(recycle);
					playing.Remove(recycle);
				}

				toBeRecycled.Clear();
			}

			if (available.Count > 0)
            {
				instance = available[0];
				playing.Add(instance);
				available.Remove(instance);
				//System.Console.WriteLine("from pool = " + playing.Count);
			}
			else
            {
				instance = CreateInstance ();
				playing.Add (instance);
				//System.Console.WriteLine("pooled = "  + playing.Count);
			}

			instance.Volume = volume;
			instance.Pitch = pitch;
			instance.Pan = pan;
			return instance.TryPlay();

#else
            if(_instance == null)
                _instance = CreateInstance();

            _instance.Volume = volume;
            _instance.Pitch = pitch;
            _instance.Pan = pan;
            _instance.Play();
            return _instance.Sound.Playing;
            
#endif
            return false;
        }

        #endregion

        #region Public Properties

        private TimeSpan PlatformGetDuration()
        {

#if (WINDOWS && OPENGL) || LINUX

             return _duration;
#else
            if ( _sound != null ) 
                return new TimeSpan(0, 0, (int)_sound.Duration); 
            else 
                return new TimeSpan(0);
#endif
        }

        #endregion

        #region Static Members

        private static void PlatformSetMasterVolume() { }

        #endregion

        #region IDisposable Members

        private void PlatformDispose()
        {
#if (WINDOWS && OPENGL) || LINUX
            // No-op. Note that isDisposed remains false!
#else

            _sound.Dispose();
            isDisposed = true;
#endif
        }

        #endregion
    }
}

