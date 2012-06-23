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

using System;
using System.IO;

using Microsoft.Xna.Framework.Audio;

#if IPHONE
using MonoTouch.Foundation;
using MonoTouch.AVFoundation;
#endif
﻿
namespace Microsoft.Xna.Framework.Media
{
    public sealed class Song : IEquatable<Song>, IDisposable
    {
#if IPHONE
		private AVAudioPlayer _sound;
#else
		private SoundEffectInstance _sound;
#endif
		
		private string _name;
		private int _playCount;
    
		internal delegate void FinishedPlayingHandler(object sender, EventArgs args);
		event FinishedPlayingHandler DonePlaying;
		
		internal Song(string fileName)
		{			
			_name = fileName;
			
#if IPHONE
			_sound = AVAudioPlayer.FromUrl(NSUrl.FromFilename(fileName));
			_sound.NumberOfLoops = 0;
            _sound.FinishedPlaying += OnFinishedPlaying;
#elif !WINRT
            _sound = new SoundEffect(_name).CreateInstance();
#endif
		}
		
		internal void OnFinishedPlaying (object sender, EventArgs args)
		{
			if (DonePlaying == null)
				return;
			
			DonePlaying(sender, args);
		}
		
		/// <summary>
		/// Set the event handler for "Finished Playing". Done this way to prevent multiple bindings.
		/// </summary>
		internal void SetEventHandler(FinishedPlayingHandler handler)
		{
			if (DonePlaying != null)
				return;
			
			DonePlaying += handler;
		}
		
		public string FilePath
		{
			get { return _name; }
		}
		
		public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_sound != null)
                {
#if IPHONE
                    _sound.FinishedPlaying -= OnFinishedPlaying;
#endif
                    _sound.Dispose();
                    _sound = null;
                }
            }
        }
        
		public bool Equals(Song song) 
		{
			return ((object)song != null) && (Name == song.Name);
		}
		
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
		
		public override bool Equals(Object obj)
		{
			if(obj == null)
			{
				return false;
			}
			
			return Equals(obj as Song);  
		}
		
		public static bool operator ==(Song song1, Song song2)
		{
			if((object)song1 == null)
			{
				return (object)song2 == null;
			}

			return song1.Equals(song2);
		}
		
		public static bool operator !=(Song song1, Song song2)
		{
		  return ! (song1 == song2);
		}
		
		internal void Play()
		{			
			if ( _sound == null )
				return;
			
			_sound.Play();
			_playCount++;
        }

		internal void Resume()
		{
			if (_sound == null)
				return;
			
#if IPHONE
			_sound.Play();
#else
			_sound.Resume();
#endif

		}
		
		internal void Pause()
		{			
			if ( _sound == null )
				return;
			
			_sound.Pause();
        }
		
		internal void Stop()
		{
			if ( _sound == null )
				return;
			
			_sound.Stop();
			
			_playCount = 0;
		}
		
		internal float Volume
		{
			get
			{
				if (_sound != null)
					return _sound.Volume;
				else
					return 0.0f;
			}
			
			set
			{
				if ( _sound != null && _sound.Volume != value )
					_sound.Volume = value;
			}			
		}
		
		// TODO: Implement
        public TimeSpan Duration
        {
            get
            {
				if ( _sound != null )
				{
					//return new TimeSpan(0,0,(int)_sound.Duration);
					return new TimeSpan(0);
				}
				else
				{
					return new TimeSpan(0);
				}
				
            }
        }
		
		// TODO: Implement
		public TimeSpan Position
        {
            get
            {
				if ( _sound != null )
				{
					//return new TimeSpan(0,0,(int)_sound.CurrentPosition);
					return new TimeSpan(0);
				}
				else
				{
					return new TimeSpan(0);
				}
            }
        }

        public bool IsProtected
        {
            get
            {
				return false;
            }
        }

        public bool IsRated
        {
            get
            {
				return false;
            }
        }

        public string Name
        {
            get
            {
				return Path.GetFileNameWithoutExtension(_name);
            }
        }

        public int PlayCount
        {
            get
            {
				return _playCount;
            }
        }

        public int Rating
        {
            get
            {
				return 0;
            }
        }

        public int TrackNumber
        {
            get
            {
				return 0;
            }
        }
    }
}

