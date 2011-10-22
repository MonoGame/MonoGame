/*
	Sound.cs
	 
	Author:
	      Christian Beaumont chris@foundation42.org (http://www.foundation42.com)
	
	Copyright (c) 2009 Foundation42 LLC
	
	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:
	
	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.
	
	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	THE SOFTWARE.
*/

using System;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using MonoMac;
using MonoMac.AppKit;
using MonoMac.Foundation;	

namespace Microsoft.Xna.Framework.Audio
{	
	internal class Sound : IDisposable
	{	
		private NSSound _audioPlayer;
		
		public Sound()
		{
		}
		
		public Sound(string url, float volume, bool looping)
		{			
			var data = NSData.FromUrl(NSUrl.FromFilename(url));
			_audioPlayer = new NSSound(data);
			_audioPlayer.Volume = volume;
			_audioPlayer.Loops = looping;
		}
		
		public Sound(byte[] audiodata, float volume, bool looping) {
			var data = NSData.FromArray(audiodata);
			_audioPlayer = new NSSound(data);
			_audioPlayer.Volume = volume;
			_audioPlayer.Loops = looping;
		}
		
		~Sound()
		{
			Dispose();	
		}
		
		public void Dispose()
		{
			_audioPlayer.Dispose();
		}
		
		public double Duration
		{
			get
			{
				return _audioPlayer.Duration();
			}
		}
		
		public double CurrentPosition
		{
			get
			{
				return _audioPlayer.CurrentTime;
			}
			set
			{
				_audioPlayer.CurrentTime = value;
			}
		}
			
		public bool Looping
		{
			get
			{
				return _audioPlayer.Loops;
			}
			set
			{
				_audioPlayer.Loops = value;
			}
		}
		
		public float Pan
		{
			get;
			set;
		}
		
		public bool Playing
		{
			get
			{
				return _audioPlayer.IsPlaying();
			}
			set
			{
				if ( value )
				{
					if (!_audioPlayer.IsPlaying())
					{
						Play();
					}
				}
				else
				{
					if (_audioPlayer.IsPlaying())
					{
						Stop();
					}
				}
			}
		}
		
		public void Pause()
		{		
			//HACK: Stopping or pausing NSSound is really slow (~200ms), don't if the sample is short :/
			if (Duration > 2) {
				_audioPlayer.Pause();
			}
		}
		
		public void Play()
		{		
			_audioPlayer.Play();
		}
		
		public void Stop()
		{			
			if (Duration > 2) { 
				_audioPlayer.Stop();
			}
		}
		
		public float Volume
		{
			get
			{
				return _audioPlayer.Volume;
			}
			set
			{
				_audioPlayer.Volume = value;
			}
		}
		
		public static Sound CreateAndPlay(string url, float volume, bool looping)
		{
			Sound sound = new Sound(url, volume, looping);
			
			sound.Play();
			
			return sound;
		}
	}
}
