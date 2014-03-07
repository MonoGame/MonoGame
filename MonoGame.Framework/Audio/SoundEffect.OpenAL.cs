// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
﻿
using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

#if WINDOWS || LINUX
using OpenTK.Audio.OpenAL;
#elif ANDROID
using Android.Content;
using Android.Content.Res;
using Android.Media;
using Android.Util;
using Stream = System.IO.Stream;
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

		internal float Rate { get; set; }

#if WINDOWS || LINUX || IOS || MONOMAC

        internal int Size { get; set; }

        internal ALFormat Format { get; set; }
#endif

#if ANDROID
		private SoundEffectInstance _instance;
		private int _soundID = -1;
#endif

        #region Public Constructors

        private void PlatformLoadAudioStream(Stream s)
        {
#if WINDOWS || LINUX
            
            ALFormat format;
            int size;
            int freq;

            _data = AudioLoader.Load(s, out format, out size, out freq);

            Format = format;
            Size = size;
            Rate = freq;

            return;
#endif

#if MONOMAC || IOS

            AudioFileStream afs = new AudioFileStream (AudioFileType.WAVE);

			var audiodata = new byte[s.Length];
			s.Read(audiodata, 0, (int)s.Length);
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

#endif

#if ANDROID

			// Creating a soundeffect from a stream
			// doesn't seem to be supported in Android
#endif
        }

        private void PlatformInitialize(byte[] buffer, int sampleRate, AudioChannels channels)
        {
			Rate = (float)sampleRate;

#if WINDOWS || LINUX

            _data = buffer;
            Size = buffer.Length;
            Format = (channels == AudioChannels.Stereo) ? ALFormat.Stereo16 : ALFormat.Mono16;

            return;

#endif

#if MONOMAC || IOS

            //buffer should contain 16-bit PCM wave data
            short bitsPerSample = 16;

            Size = (int)buffer.Length;

            if ((int)channels <= 1)
                Format = bitsPerSample == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
            else
                Format = bitsPerSample == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;

            var _dblDuration = (Size / ((bitsPerSample / 8) * (((int)channels == 0) ? 1 : (int)channels))) / Rate;
            _duration = TimeSpan.FromSeconds(_dblDuration);

            _name = "";
            _data = buffer;

#endif

#if ANDROID
            _name = "";

            _data = AudioUtil.FormatWavData(buffer, sampleRate, (int)channels);
#endif
        }

#if ANDROID

		internal SoundEffect(string fileName)
		{
			using (AssetFileDescriptor fd = Game.Activity.Assets.OpenFd(fileName))
				_soundID = SoundEffectInstance.SoundPool.Load(fd.FileDescriptor, fd.StartOffset, fd.Length, 1);
		}

#endif

        private void PlatformInitialize(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int loopStart, int loopLength)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Additional SoundEffect/SoundEffectInstance Creation Methods

        private SoundEffectInstance PlatformCreateInstance()
        {
#if WINDOWS || LINUX || MONOMAC || IOS
            return new SoundEffectInstance(this);
#endif

#if ANDROID
			return new SoundEffectInstance()
            {
                _soundId = _soundID,
                _sampleRate = Rate
            };
#endif
        }

        #endregion

        #region Play

        private bool PlatformPlay()
        {
#if WINDOWS || LINUX || MONOMAC || IOS
            return PlatformPlay(MasterVolume, 0.0f, 0.0f);
#endif

#if ANDROID
            return PlatformPlay(1.0f, 0.0f, 0.0f);
#endif
        }

        private bool PlatformPlay(float volume, float pitch, float pan)
        {
            // TODO: While merging the SoundEffect classes together
            // I noticed that the return values seem to widly differ
            // between platforms. It also doesn't seem to match
            // what's written in the XNA docs.

            if (MasterVolume <= 0.0f)
                return false;

#if WINDOWS || LINUX

            SoundEffectInstance instance = PlatformCreateInstance();
            instance.Volume = volume;
            instance.Pitch = pitch;
            instance.Pan = pan;
            instance.Play();

            // Why is this always returning false?
            return false;

#endif

#if MONOMAC || IOS
            
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
            instance.Play();
            return true;
#endif

#if ANDROID

			if(_instance == null)
				_instance = CreateInstance();

			_instance.Volume = volume;
			_instance.Pitch = pitch;
			_instance.Pan = pan;
			_instance.Play();
			return true;            
#endif
        }

        #endregion

        #region Public Properties

        private TimeSpan PlatformGetDuration()
        {
             return _duration;
        }

        #endregion

        #region Static Members

        private static void PlatformSetMasterVolume()
        {
			// Appears to be a no-op outside of DirectX Platforms.
        }

        #endregion

        #region IDisposable Members

        private void PlatformDispose()
        {
            // A No-op for WINDOWS and LINUX. Note that isDisposed remains false!

#if ANDROID
            if (!isDisposed)
            {
				if (_soundID != -1)
					SoundEffectInstance.SoundPool.Unload(_soundID);

				_soundID = -1;

                isDisposed = true;
            }
#endif
        }

        #endregion
    }
}

