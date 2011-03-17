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
	public class Sound
	{	
		private static AVAudioPlayer audioPlayer;
		
		private Sound()
		{
		}
		
		public double Duration
		{
			get 
			{ 
				return audioPlayer.Duration;
			}
		}
		
		public double CurrentPosition
		{
			get 
			{ 
				return audioPlayer.CurrentTime;
			}
		}
			
		public bool Looping
		{
			get 
			{ 
				//return this._Looping; 
				return (audioPlayer.NumberOfLoops == -1 );
			}
			set
			{
				if ( value )
				{
					audioPlayer.NumberOfLoops = -1;
				}
				else
				{
					audioPlayer.NumberOfLoops = 0;
				}
			}
		}
		
		public float Pan
		{
			get 
			{ 
				return audioPlayer.Pan;
			}
			set
			{
				audioPlayer.Pan = value;
			}
		}
		
		public bool Playing
		{
			get 
			{ 
				return audioPlayer.Playing;
			}
		}
		
		public void Pause()
		{		
			audioPlayer.Pause();
		}
		
		public void Play()
		{		
			audioPlayer.Play();
		}
		
		public void Stop()
		{			
			audioPlayer.Stop();
		}
		
		public float Volume
		{
			get 
			{ 
				return audioPlayer.Volume;
			}
			set
			{
				audioPlayer.Volume = value;
			}
		}
		
		public static Sound Create(string url, float volume, bool looping)
		{
			Sound sound = new Sound();
			
			var mediaFile = NSUrl.FromFilename(url);			
			audioPlayer =  AVAudioPlayer.FromUrl(mediaFile); 
			audioPlayer.Volume = volume;
			if ( looping )
			{
				audioPlayer.NumberOfLoops = -1;
			}
			else
			{
				audioPlayer.NumberOfLoops = 0;
			}
			
			return sound;
		}
		
		public static Sound CreateAndPlay(string url, float volume, bool looping)
		{
			Sound sound = Sound.Create(url, volume, looping);
			
			sound.Play();
			
			return sound;
		}
	}
}
