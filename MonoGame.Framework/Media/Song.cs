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
﻿
namespace Microsoft.Xna.Framework.Media
{
    public class Song : IEquatable<Song>, IDisposable
    {
		private Sound _sound;
		private string _name;
		private int _playCount;
		
		internal Song(string fileName)
		{			
			_name = fileName;
			_sound = new Sound(_name, 1.0f, true);
		}
		
		public void Dispose()
        {
			_sound.Dispose();
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
			if ( _sound != null )
			{
				_sound.Play();
				_playCount++;
			}
        }
		
		internal void Pause()
		{			
			if ( _sound != null )
			{
				_sound.Pause();
			}
        }
		
		internal void Stop()
		{
			if ( _sound != null )
			{
				_sound.Stop();
			}
		}
		
		internal bool Loop
		{
			get
			{
				if ( _sound != null )
				{
					return _sound.Looping;
				}
				else
				{
				 	return false;	
				}
			}
			set 
			{
				if ( _sound != null )
				{
					if ( _sound.Looping != value )
					{
						_sound.Looping = value;
					}
				}
			}
		}
		
		internal float Volume
		{
			get
			{
				if (_sound != null)
				{
					return _sound.Volume;
				}
				else
				{
					return 0.0f;
				}
			}
			
			set
			{
				if ( _sound != null )
				{
					if ( _sound.Volume != value )
					{
						_sound.Volume = value;
					}
				}
			}			
		}
		
        public TimeSpan Duration
        {
            get
            {
				if ( _sound != null )
				{
					return new TimeSpan(0,0,(int)_sound.Duration);
				}
				else
				{
					return new TimeSpan(0);
				}
            }
        }
		
		public TimeSpan Position
        {
            get
            {
				if ( _sound != null )
				{
					return new TimeSpan(0,0,(int)_sound.CurrentPosition);
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

