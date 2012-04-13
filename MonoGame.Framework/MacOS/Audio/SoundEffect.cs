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
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Microsoft.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

#if IPHONE
using MonoTouch.AudioToolbox;
using MonoTouch.AudioUnit;

using OpenTK.Audio.OpenAL;
#elif MONOMAC
using MonoMac.AudioToolbox;
using MonoMac.AudioUnit;

using MonoMac.OpenAL;
#endif

using System.Diagnostics;

namespace Microsoft.Xna.Framework.Audio
{
	public sealed partial class SoundEffect : IDisposable
	{
		private string _name = "";
		private string _filename = "";
		internal byte[] _data;
		List<SoundEffectInstance> playing = null;
		List<SoundEffectInstance> available = null;
		List<SoundEffectInstance> toBeRecycled = null;

		internal float Rate { get; set; }

		internal ALFormat Format { get; set; }

		internal int Size { get; set; }

		internal SoundEffect (string fileName)
		{
			_filename = fileName;

			if (_filename == string.Empty) {
				throw new FileNotFoundException ("Supported Sound Effect formats are wav, mp3, acc, aiff");
			}

			int size;
			ALFormat format;
			double rate;
			double duration;

			_data = OpenALSupport.LoadFromFile (_filename,
			                                    out size, out format, out rate, out duration);

			_name = Path.GetFileNameWithoutExtension (fileName);

			Rate = (float)rate;
			Size = size;
			Format = format;
			_duration = TimeSpan.FromSeconds (duration);
			//Console.WriteLine ("From File: " + _name + " - " + Format + " = " + Rate + " / " + Size + " -- "  + Duration);

		}

		//SoundEffect from playable audio data
		internal SoundEffect (string name, byte[] data)
		{
			_data = data;
			_name = name;
			LoadAudioStream (_data);

		}

		public SoundEffect (byte[] buffer, int sampleRate, AudioChannels channels)
		{
			//buffer should contain 16-bit PCM wave data
			short bitsPerSample = 16;

			MemoryStream mStream = new MemoryStream (44 + buffer.Length);
			BinaryWriter writer = new BinaryWriter (mStream);

			writer.Write ("RIFF".ToCharArray ()); //chunk id
			writer.Write ((int)(36 + buffer.Length)); //chunk size
			writer.Write ("WAVE".ToCharArray ()); //RIFF type

			writer.Write ("fmt ".ToCharArray ()); //chunk id
			writer.Write ((int)16); //format header size
			writer.Write ((short)1); //format (PCM)
			writer.Write ((short)channels);
			writer.Write ((int)sampleRate);
			short blockAlign = (short)((bitsPerSample / 8) * (int)channels);
			writer.Write ((int)(sampleRate * blockAlign)); //byte rate
			writer.Write ((short)blockAlign);
			writer.Write ((short)bitsPerSample);

			writer.Write ("data".ToCharArray ()); //chunk id
			writer.Write ((int)buffer.Length); //data size
			writer.Write (buffer);

			writer.Close ();
			mStream.Close ();

			_data = mStream.ToArray ();
			_name = "";

			LoadAudioStream (_data);

		}

		void LoadAudioStream (byte[] audiodata)
		{
			AudioFileStream afs = new AudioFileStream (AudioFileType.WAVE);
			//long pac = afs.DataPacketCount;
			afs.PacketDecoded += HandlePacketDecoded;

			afs.ParseBytes (audiodata, false);
			afs.Close ();
		}

		void HandlePacketDecoded (object sender, PacketReceivedEventArgs e)
		{
			AudioFileStream afs = (AudioFileStream)sender;
			byte[] audioData = new byte[e.Bytes];
			Marshal.Copy (e.InputData, audioData, 0, e.Bytes);
			//Console.WriteLine ("Packet decoded ");
			AudioStreamBasicDescription asbd = afs.StreamBasicDescription;

			Rate = (float)asbd.SampleRate;
			Size = e.Bytes;

			if (asbd.ChannelsPerFrame == 1) {
				if (asbd.BitsPerChannel == 8) {
					Format = ALFormat.Mono8;
				}
				else if (asbd.BitsPerChannel == 0) // This shouldn't happen. hackking around bad data for now.
				{
				//TODO: Remove this when sound's been fixed on iOS and other devices.
					Format = ALFormat.Mono16;
					Debug.WriteLine("Warning, bad decoded audio packet in SoundEffect.HandlePacketDecoded. Squelching sound.");
					_duration = TimeSpan.Zero;
					_data = audioData;
					return;
				}
				else 
				{
					Format = ALFormat.Mono16;
				}
			} else {
				if (asbd.BitsPerChannel == 8) {
					Format = ALFormat.Stereo8;
				} else {
					Format = ALFormat.Stereo16;
				}
			}
			_data = audioData;


			var _dblDuration = (e.Bytes / ((asbd.BitsPerChannel / 8) * asbd.ChannelsPerFrame)) / asbd.SampleRate;
			_duration = TimeSpan.FromSeconds (_dblDuration);
//			Console.WriteLine ("From Data: " + _name + " - " + Format + " = " + Rate + " / " + Size + " -- "  + Duration);
//			Console.WriteLine("Duration: " + _dblDuration
//			                  		+ " / size: " + e.Bytes
//			                  		+ " bits: " + asbd.BitsPerChannel
//			                  		+ " channels: " + asbd.ChannelsPerFrame
//			                  		+ " rate: " + asbd.SampleRate);
		}

		//double _dblDuration = 0;
		TimeSpan _duration = TimeSpan.Zero;

		public bool Play ()
		{
			return Play (MasterVolume, 1.0f, 0.0f);
		}

		public bool Play (float volume, float pitch, float pan)
		{
			if (MasterVolume > 0.0f) {
				if (playing == null) {
					playing = new List<SoundEffectInstance>();
					available = new List<SoundEffectInstance>();
					toBeRecycled = new List<SoundEffectInstance>();
				}
				else {
					// Lets cycle through our playing list and see if any are stopped
					// so that we can recycle them.
					if (playing.Count > 0) {

						foreach(var instance2 in playing) {
							if (instance2.State == SoundState.Stopped) {
								toBeRecycled.Add(instance2);
							}
						}
					}
				}

				SoundEffectInstance instance = null;
				if (toBeRecycled.Count > 0) {
					foreach(var recycle in toBeRecycled) {
						available.Add(recycle);
						playing.Remove(recycle);
					}
					toBeRecycled.Clear();
				}
				if (available.Count > 0) {
					instance = available[0];
					playing.Add(instance);
					available.Remove(instance);
					//System.Console.WriteLine("from pool = " + playing.Count);
				}
				else {
					instance = CreateInstance ();
					playing.Add (instance);
					//System.Console.WriteLine("pooled = "  + playing.Count);
				}
				instance.Volume = volume;
				instance.Pitch = pitch;
				instance.Pan = pan;
				instance.Play ();
			}
			return false;
		}

		public TimeSpan Duration {
			get {

				return _duration;
			}
		}

		public string Name {
			get {
				return _name;
			}
			set {
				_name = value;
			}
		}
		
		public SoundEffectInstance CreateInstance ()
		{
			var instance = new SoundEffectInstance (this);
			return instance;
		}
		
		#region IDisposable Members

		public void Dispose ()
		{
			//_sound.Dispose ();
		}

		#endregion
		
		static float _masterVolume = 1.0f;

		public static float MasterVolume { 
			get {
				return _masterVolume;
			}
			set {
				_masterVolume = value;	
			}
		}

		static float _distanceScale = 1f;

		public static float DistanceScale {
			get {
				return _distanceScale;
			}
			set {
				if (value <= 0f) {
					throw new ArgumentOutOfRangeException ("value of DistanceScale");
				}
				_distanceScale = value;
			}
		}

		static float _dopplerScale = 1f;

		public static float DopplerScale {
			get {
				return _dopplerScale;
			}
			set {
				// As per documenation it does not look like the value can be less than 0
				//   although the documentation does not say it throws an error we will anyway
				//   just so it is like the DistanceScale
				if (value < 0f) {
					throw new ArgumentOutOfRangeException ("value of DopplerScale");
				}
				_dopplerScale = value;
			}
		}

		static float speedOfSound = 343.5f;

		public static float SpeedOfSound {
			get {
				return speedOfSound;
			}
			set {
				speedOfSound = value;
			}
		}		
	}
}

