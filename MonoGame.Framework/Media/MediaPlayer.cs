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
using Microsoft.Xna.Framework.Audio;

﻿namespace Microsoft.Xna.Framework.Media
{
    public static class MediaPlayer
    {
		private static Song _song = null;
		private static MediaState _mediaState = MediaState.Stopped;
		private static float _volume = 1.0f;
		private static bool _looping = true;
		
        public static void Pause()
        {
			if (_song != null)
			{
				_song.Pause();
				_mediaState = MediaState.Paused;
			}			
        }

        public static void Play(Song song)
        {
			if ( song != null )
			{
				_song = song;
				_song.Volume = _volume;
				_song.Loop = _looping;
				_song.Play();
				_mediaState = MediaState.Playing;
			}
        }

        public static void Resume()
        {
			if (_song != null)
			{
				_song.Resume();
				_mediaState = MediaState.Playing;
			}					
        }

        public static void Stop()
        {
			if (_song != null)
			{
				_song.Stop();
				_mediaState = MediaState.Stopped;
			}
        }

        public static bool IsMuted
        {
            get
            {
				if (_song != null)
				{
					return _song.Volume == 0.0f;
				}
				else
				{
					return false;
				}
            }
            set
            {
				if (_song != null) 
				{
					if (value)
					{
						_song.Volume = 0.0f;
					}
					else 
					{
						_song.Volume = _volume;
					}
				}
            }
        }

        public static bool IsRepeating
        {
            get
            {
				if (_song != null)
				{
					return _song.Loop;
				}
				else
				{
					return false;
				}
            }
            set
            {
				_looping = value;
				if(_song != null) _song.Loop = value;
            }
        }

        public static bool IsShuffled
        {
            get
            {
				return false;
            }
        }

        public static bool IsVisualizationEnabled
        {
            get
            {
				return false;
            }
        }

        public static TimeSpan PlayPosition
        {
            get
            {
				if (_song != null)
				{
					return _song.Position;
				}
				else
				{
					return new TimeSpan(0);
				}
            }
        }

        public static MediaState State
        {
            get
            {
				return _mediaState;
            }
        }
		
		public static bool GameHasControl
        {
            get
            {
            	return true;
			}
		}

        public static float Volume
        {
            get
            {
            	return _volume;
			}
            set
            {         
				if (_song != null)
				{
					_volume = value;
					_song.Volume = value;
				}
			}
        }
    }
}

