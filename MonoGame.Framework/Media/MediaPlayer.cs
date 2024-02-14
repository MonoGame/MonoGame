// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Media
{
    /// <summary>
    /// Provides methods and properties to play, pause, resume, and stop songs.
    /// <see cref="MediaPlayer"/> also exposes shuffle, repeat, volume, play position, and visualization capabilities.
    /// </summary>
    public static partial class MediaPlayer
    {
		// Need to hold onto this to keep track of how many songs
		// have played when in shuffle mode
		private static int _numSongsInQueuePlayed = 0;
		private static MediaState _state = MediaState.Stopped;
		private static float _volume = 1.0f;
		private static bool _isMuted;
        private static bool _isRepeating;
        private static bool _isShuffled;
		private static readonly MediaQueue _queue = new MediaQueue();

        /// <summary>
        /// Raised when the active song changes due to active playback or due to explicit calls to the <see cref="MoveNext()"/> or <see cref="MovePrevious()"/> methods.
        /// </summary>
		public static event EventHandler<EventArgs> ActiveSongChanged;
        /// <summary>
        /// Raised when the media player play state changes.
        /// </summary>
        public static event EventHandler<EventArgs> MediaStateChanged;

        static MediaPlayer()
        {
            PlatformInitialize();
        }

        #region Properties

        /// <summary>
        /// Gets the media playback queue, <see cref="MediaQueue"/>.
        /// </summary>
        public static MediaQueue Queue { get { return _queue; } }

        /// <summary>
        /// Gets or set the muted setting for the media player.
        /// </summary>
		public static bool IsMuted
        {
            get { return PlatformGetIsMuted(); }
            set { PlatformSetIsMuted(value); }
        }

        /// <summary>
        /// Gets or sets the repeat setting for the media player.
        /// </summary>
        /// <remarks>
        /// When set to <see langword="true"/>, the playback queue will begin playing again after all songs in the queue have been played.
        /// </remarks>
        public static bool IsRepeating 
        {
            get { return PlatformGetIsRepeating(); }
            set { PlatformSetIsRepeating(value); }
        }

        /// <summary>
        /// Gets or sets the shuffle setting for the media player.
        /// </summary>
        /// <remarks>
        /// When set to <see langword="true"/>, songs in the playback queue are played in random order rather than from first to last.
        /// </remarks>
        public static bool IsShuffled
        {
            get { return PlatformGetIsShuffled(); }
            set { PlatformSetIsShuffled(value); }
        }

        /// <summary>
        /// Gets or sets the visualization enabled setting for the media player.
        /// </summary>
        /// <remarks>
        /// Always returns <see langword="false"/>
        /// </remarks>
        public static bool IsVisualizationEnabled { get { return false; } }

        /// <summary>
        /// Gets the play position within the currently playing song.
        /// </summary>
        public static TimeSpan PlayPosition
        {
            get { return PlatformGetPlayPosition(); }
#if (IOS && !TVOS) || ANDROID
            set { PlatformSetPlayPosition(value); }
#endif
        }

        /// <summary>
        /// Gets the media playback state, <see cref="MediaState"/>
        /// </summary>
        public static MediaState State
        {
            get { return PlatformGetState(); }
            private set
            {
                if (_state != value)
                {
                    _state = value;
                    EventHelpers.Raise(null, MediaStateChanged, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Determines whether the game has control of the background music.
        /// </summary>
        /// <remarks>
        /// A gamer can play their own music as the background to your game by using the Xbox 360 dashboard.
        /// If the game is currently playing custom background music (specified by the gamer using the Xbox 360 dashboard),
        /// calls to <see cref="Play(Song)"/>, <see cref="Stop()"/>, <see cref="Pause()"/>,
        /// <see cref="Resume()"/>, <see cref="MoveNext()"/>, and <see cref="MovePrevious"/> have no effect.
        /// If another application's background music is playing, your game will need to call
        /// in order to pause the other application's background music in order to play the game's music.
        /// </remarks>
        public static bool GameHasControl
        {
            get
            {
                return PlatformGetGameHasControl();
            }
        }

        /// <summary>
        /// Gets or sets the media player volume
        /// </summary>
        /// <value>
        /// Media player volume, from 0.0f (silence) to 1.0f (full volume relative to the current device volume)
        /// </value>
        /// <remarks>
        /// <para>
        /// Volume adjustment is based on a decibel, not multiplicative, scale.
        /// Setting <see cref="Volume"/> to 0.0 subtracts 96 dB from the volume. Setting <see cref="Volume"/> to 1.0 subtracts 0 dB from the volume.
        /// Values in between 0.0f and 1.0f subtract dB from the volume proportionally.
        /// </para>
        /// </remarks>
        public static float Volume
        {
            get { return PlatformGetVolume(); }
            set
            {
                var volume = MathHelper.Clamp(value, 0, 1);

                PlatformSetVolume(volume);
            }
        }

        #endregion

        /// <summary>
        /// Pauses the currently playing song.
        /// </summary>
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
            Play(song, null);
        }

        /// <summary>
        /// Play clears the current playback queue, and then queues up the specified song for playback. 
        /// Playback starts immediately at the given position of the song.
        /// </summary>
        public static void Play(Song song, TimeSpan? startPosition)
        {
            if (song == null)
                throw new ArgumentNullException("song", "This method does not accept null for this parameter.");

            var previousSong = _queue.Count > 0 ? _queue[0] : null;
            _queue.Clear();
            _numSongsInQueuePlayed = 0;
            _queue.Add(song);
            _queue.ActiveSongIndex = 0;
            
            PlaySong(song, startPosition);

            if (previousSong != song)
                EventHelpers.Raise(null, ActiveSongChanged, EventArgs.Empty);
        }

        /// <summary>
        /// Play clears the current playback queue, and then queues up the specified song collection for playback. 
        /// Playback starts immediately at the beginning of the song, specified by song collection index.
        /// </summary>
        public static void Play(SongCollection collection, int index = 0)
		{
            if (collection == null)
                throw new ArgumentNullException("collection", "This method does not accept null for this parameter.");

            _queue.Clear();
            _numSongsInQueuePlayed = 0;

			foreach(var song in collection)
				_queue.Add(song);
			
			_queue.ActiveSongIndex = index;
			
			PlaySong(_queue.ActiveSong, null);
		}

        private static void PlaySong(Song song, TimeSpan? startPosition)
        {
            if (song != null && song.IsDisposed)
                throw new ObjectDisposedException("song");

            PlatformPlaySong(song, startPosition);
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
					EventHelpers.Raise(null, ActiveSongChanged, EventArgs.Empty);
					return;
				}
			}

			MoveNext();
		}

        /// <summary>
        /// Resumes a paused song.
        /// </summary>
        public static void Resume()
        {
            if (State != MediaState.Paused)
                return;

            PlatformResume();
			State = MediaState.Playing;
        }

        /// <summary>
        /// Stops playing a song.
        /// </summary>
        public static void Stop()
        {
            if (State == MediaState.Stopped)
                return;

            PlatformStop();
			State = MediaState.Stopped;
		}

        /// <summary>
        /// Stops currently playing song, moves to the previous song in the queue of playing songs and plays it.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the current song is the last song in the queue, <see cref="MoveNext()"/> will stay on current song
        /// </para>
        /// </remarks>
		public static void MoveNext()
		{
			NextSong(1);
		}

        /// <summary>
        /// Stops currently playing song, moves to the previous song in the queue of playing songs and plays it.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the current song is the first song in the queue, <see cref="MovePrevious()"/> will stay on current song
        /// </para>
        /// </remarks>
        public static void MovePrevious()
		{
			NextSong(-1);
		}
		
		private static void NextSong(int direction)
		{
            Stop();

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

            if (nextSong != null)
                PlaySong(nextSong, null);

            EventHelpers.Raise(null, ActiveSongChanged, EventArgs.Empty);
		}
    }
}
