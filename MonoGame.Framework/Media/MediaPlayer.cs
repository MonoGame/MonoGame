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

#if IPHONE
using MonoTouch.AudioToolbox;
#endif

using Microsoft.Xna.Framework.Audio;

#if IPHONE
using MonoTouch.AudioToolbox;
using MonoTouch.AVFoundation;
using MonoTouch.Foundation;
using MonoTouch.MediaPlayer;
#endif

#if WINRT
using SharpDX.MediaFoundation;
using SharpDX;
using Windows.Storage;
#endif

using System.Linq;

namespace Microsoft.Xna.Framework.Media
{
    public static class MediaPlayer
    {
		// Need to hold onto this to keep track of how many songs
		// have played when in shuffle mode
		private static int _numSongsInQueuePlayed = 0;
		private static MediaState _mediaState = MediaState.Stopped;
		private static float _volume = 1.0f;
		private static bool _isMuted = false;
		private static MediaQueue _queue = new MediaQueue();

#if WINRT
        private static MediaEngine _mediaEngineEx;

        static MediaPlayer()
        {            
            MediaManager.Startup(true);

            using (var factory = new MediaEngineClassFactory())
            {
                var mediaEngine = new MediaEngine(factory, null, MediaEngineCreateflags.Audioonly);
                _mediaEngineEx = mediaEngine.QueryInterface<MediaEngineEx>();
            }
            
            _mediaEngineEx.PlaybackEvent += MediaEngineExOnPlaybackEvent;            
        }

        private static void MediaEngineExOnPlaybackEvent(MediaEngineEvent mediaEvent, long param1, int param2)
        {
            if (mediaEvent == MediaEngineEvent.Ended)
            {
                OnSongFinishedPlaying(null, null);
            }
        }
#endif

		#region Properties
		
		public static MediaQueue Queue { get { return _queue; } }
		
		public static bool IsMuted
        {
            get { return _isMuted; }
            set
            {
				_isMuted = value;

#if WINRT
                _mediaEngineEx.Muted = value;
#else
				if (_queue.Count == 0)
					return;
				
				var newVolume = value ? 0.0f : _volume;
                _queue.SetVolume(newVolume);
#endif
            }
        }

        private static bool _isRepeating;

        public static bool IsRepeating 
        {
            get
            {
                return _isRepeating;
            }

            set
            {
                _isRepeating = value;

#if WINRT
                _mediaEngineEx.Loop = value;
#endif
            }
        }

        public static bool IsShuffled { get; set; }

        public static bool IsVisualizationEnabled { get { return false; } }
#if !WINRT
        public static TimeSpan PlayPosition
        {
            get
            {
				if (_queue.ActiveSong == null)
					return TimeSpan.Zero;
				
				return _queue.ActiveSong.Position;
            }
        }
#endif
		
        public static MediaState State { get { return _mediaState; } }
        public static event EventHandler<EventArgs> MediaStateChanged;
        
		
#if IPHONE
		public static bool GameHasControl 
		{ 
			get 
			{ 
				var musicPlayer = MPMusicPlayerController.iPodMusicPlayer;
				
				if (musicPlayer == null)
					return true;
				
				// TODO: Research the Interrupted state and see if it's valid to
				// have control at that time.
				
				// Note: This will throw a bunch of warnings/output to the console
				// if running in the simulator. This is a known issue:
				// http://forums.macrumors.com/showthread.php?t=689102
				if (musicPlayer.PlaybackState == MPMusicPlaybackState.Playing || 
				 	musicPlayer.PlaybackState == MPMusicPlaybackState.SeekingForward ||
				    musicPlayer.PlaybackState == MPMusicPlaybackState.SeekingBackward)
				return false;
				
				return true;
			} 
		}
#else
		public static bool GameHasControl { get { return true; } }
#endif
		

        public static float Volume
        {
            get { return _volume; }
			set 
			{       
				_volume = value;
				
#if WINRT
                _mediaEngineEx.Volume = value;       
#else
				if (_queue.ActiveSong == null)
					return;

                _queue.SetVolume(_isMuted ? 0.0f : value);
#endif
            }
        }
		
		#endregion
		
        public static void Pause()
        {
#if WINRT
            _mediaEngineEx.Pause();
#else
			if (_queue.ActiveSong == null)
				return;
		
			_queue.ActiveSong.Pause();
#endif
			_mediaState = MediaState.Paused;
            if (MediaStateChanged != null) MediaStateChanged(null, EventArgs.Empty);
        }
		
		/// <summary>
		/// Play clears the current playback queue, and then queues up the specified song for playback. 
		/// Playback starts immediately at the beginning of the song.
		/// </summary>
        public static void Play(Song song)
        {                        
            _queue.Clear();
            _numSongsInQueuePlayed = 0;
            _queue.Add(song);
            
            PlaySong(song);
        }
		
		public static void Play(SongCollection collection, int index = 0)
		{
            _queue.Clear();
            _numSongsInQueuePlayed = 0;

			foreach(var song in collection)
				_queue.Add(song);
			
			_queue.ActiveSongIndex = index;
			
			PlaySong(_queue.ActiveSong);
		}
		
		private static void PlaySong(Song song)
		{
#if WINRT
            var folder = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
            var path = folder + "\\" + song.FilePath;
            var uri = new Uri(path);
            var converted = uri.AbsoluteUri;

            _mediaEngineEx.Source = converted;            
            _mediaEngineEx.Load();
            _mediaEngineEx.Play();

#else
			song.SetEventHandler(OnSongFinishedPlaying);			
			song.Volume = _isMuted ? 0.0f : _volume;
			song.Play();
#endif
			_mediaState = MediaState.Playing;
            if (MediaStateChanged != null) MediaStateChanged(null, EventArgs.Empty);
		}
		
		internal static void OnSongFinishedPlaying (object sender, EventArgs args)
		{
			// TODO: Check args to see if song sucessfully played
			_numSongsInQueuePlayed++;
			
			if (_numSongsInQueuePlayed >= _queue.Count)
			{
				_numSongsInQueuePlayed = 0;
				if (!IsRepeating)
				{
					_mediaState = MediaState.Stopped;
                    if (MediaStateChanged != null) MediaStateChanged(null, EventArgs.Empty);
					return;
				}
			}
			
			MoveNext();
		}

        public static void Resume()
        {
#if WINRT
            _mediaEngineEx.Play();            
#else
			if (_queue.ActiveSong == null)
				return;
			
			_queue.ActiveSong.Resume();
#endif
			_mediaState = MediaState.Playing;
            if (MediaStateChanged != null) MediaStateChanged(null, EventArgs.Empty);
        }

        public static void Stop()
        {
#if WINRT
            _mediaEngineEx.Source = null;
#else
			if (_queue.ActiveSong == null)
				return;
			
			// Loop through so that we reset the PlayCount as well
			foreach(var song in Queue.Songs)
				_queue.ActiveSong.Stop();
#endif
			_mediaState = MediaState.Stopped;
            if (MediaStateChanged != null) MediaStateChanged(null, EventArgs.Empty);
		}
		
		public static void MoveNext()
		{
			NextSong(1);
		}
		
		public static void MovePrevious()
		{
			NextSong(-1);
		}
		
		private static void NextSong(int direction)
		{
			var nextSong = _queue.GetNextSong(direction, IsShuffled);

            if (nextSong == null)
                Stop();
            else            
                Play(nextSong);                            
		}
    }
}

