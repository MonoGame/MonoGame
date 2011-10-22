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

using MonoTouch;
using MonoTouch.Foundation;	
using MonoTouch.AVFoundation;

namespace Microsoft.Xna.Framework.Audio
{	
	internal class Sound : IDisposable
	{	
		private AVAudioPlayer _audioPlayer;
		
		public Sound()
		{
		}
		
		public Sound(string url, float volume, bool looping)
		{
			var mediaFile = NSUrl.FromFilename(url);			
			_audioPlayer =  AVAudioPlayer.FromUrl(mediaFile); 
			_audioPlayer.Volume = volume;
			if ( looping )
			{
				_audioPlayer.NumberOfLoops = -1;
			}
			else
			{
				_audioPlayer.NumberOfLoops = 0;
			}
			
			if (!_audioPlayer.PrepareToPlay())
			{
				throw new Exception("Unable to Prepare sound for playback!");
			}
		}
		
		public Sound(byte[] audiodata, float volume, bool looping)
		{
			var data = NSData.FromArray(audiodata);
			_audioPlayer = AVAudioPlayer.FromData(data);
			_audioPlayer.Volume = volume;
			if ( looping )
			{
				_audioPlayer.NumberOfLoops = -1;
			}
			else
			{
				_audioPlayer.NumberOfLoops = 0;
			}
			
			if (!_audioPlayer.PrepareToPlay())
			{
				throw new Exception("Unable to Prepare sound for playback!");
			}
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
				return _audioPlayer.Duration;
			}
		}
		
		public double CurrentPosition
		{
			get 
			{ 
				return _audioPlayer.CurrentTime;
			}
		}
			
		public bool Looping
		{
			get 
			{ 
				//return this._Looping; 
				return (_audioPlayer.NumberOfLoops == -1 );
			}
			set
			{
				if ( value )
				{
					_audioPlayer.NumberOfLoops = -1;
				}
				else
				{
					_audioPlayer.NumberOfLoops = 0;
				}
			}
		}
		
		public float Pan
		{
			get 
			{ 
				return _audioPlayer.Pan;
			}
			set
			{
				_audioPlayer.Pan = value;
			}
		}
		
		public bool Playing
		{
			get 
			{ 
				return _audioPlayer.Playing;
			}
		}
		
		public void Pause()
		{		
			_audioPlayer.Pause();
		}
		
		public void Play()
		{
            ThreadPool.QueueUserWorkItem(delegate
            {
                _audioPlayer.Play();
            });
		}
		
		public void Stop()
		{			
			_audioPlayer.Stop();
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
