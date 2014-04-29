// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Media
{
    public static partial class MediaPlayer
    {
		// Need to hold onto this to keep track of how many songs
		// have played when in shuffle mode
		private static int _numSongsInQueuePlayed = 0;
		private static MediaState _state = MediaState.Stopped;
		private static float _volume = 1.0f;
		private static bool _isMuted = false;
		private static readonly MediaQueue _queue = new MediaQueue();

		public static event EventHandler<EventArgs> ActiveSongChanged;

        static MediaPlayer()
        {
            PlatformInitialize();
        }

        #region Properties

        public static MediaQueue Queue { get { return _queue; } }
		
		public static bool IsMuted
        {
            get { return _isMuted; }
            set
            {
				_isMuted = value;

                PlatformSetIsMuted();
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

                // RAYB: At the moment, is a no-op on most platforms
#if WINDOWS_MEDIA_ENGINE
                PlatformSetIsRepeating();
#endif
            }
        }

        public static bool IsShuffled { get; set; }

        public static bool IsVisualizationEnabled { get { return false; } }

        public static TimeSpan PlayPosition
        {
            get
            {
                return PlatformGetPlayPosition();
            }
        }

        public static MediaState State
        {
            get { return _state; }
            private set
            {
                if (_state != value)
                {
                    _state = value;
                    if (MediaStateChanged != null)
                        MediaStateChanged (null, EventArgs.Empty);
                }
            }
        }
        public static event EventHandler<EventArgs> MediaStateChanged;

        public static bool GameHasControl
        {
            get
            {
                return PlatformGetGameHasControl();
            }
        }
		

        public static float Volume
        {
            get { return _volume; }
            set
            {
                _volume = MathHelper.Clamp(value, 0, 1);

                PlatformSetVolume();
            }
        }

		#endregion
		
        public static void Pause()
        {
            if (State != MediaState.Playing || _queue.ActiveSong == null)
                return;

            PlatformPause();

            State = MediaState.Paused;
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
			_queue.ActiveSongIndex = 0;
            
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
            PlatformPlaySong(song);
            State = MediaState.Playing;
        }

        internal static void OnSongFinishedPlaying(object sender, EventArgs args)
		{
			// TODO: Check args to see if song sucessfully played
			_numSongsInQueuePlayed++;
			
			if (_numSongsInQueuePlayed >= _queue.Count)
			{
				_numSongsInQueuePlayed = 0;
				if (!IsRepeating)
				{
					Stop();

					if (ActiveSongChanged != null)
					{
						ActiveSongChanged.Invoke(null, null);
					}

					return;
				}
			}

#if WINDOWS_PHONE
            if (IsRepeating)
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    _mediaElement.Position = TimeSpan.Zero;
                    _mediaElement.Play();
                });
            }
#endif
			
			MoveNext();
		}

        public static void Resume()
        {
            if (State != MediaState.Paused)
                return;

            PlatformResume();
			State = MediaState.Playing;
        }

        public static void Stop()
        {
            if (State == MediaState.Stopped)
                return;

            PlatformStop();
			State = MediaState.Stopped;
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
            if (IsRepeating && _queue.ActiveSongIndex >= _queue.Count - 1)
            {
                _queue.ActiveSongIndex = 0;
                
                // Setting direction to 0 will force the first song
                // in the queue to be played.
                // if we're on "shuffle", then it'll pick a random one
                // anyway, regardless of the "direction".
                direction = 0;
            }

			var nextSong = _queue.GetNextSong(direction, IsShuffled);

            if (nextSong == null)
                Stop();
            else
                PlaySong(nextSong);

            if (ActiveSongChanged != null)
            {
                ActiveSongChanged.Invoke(null, null);
            }
		}
    }
}

