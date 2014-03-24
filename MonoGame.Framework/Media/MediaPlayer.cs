// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if WINDOWS_PHONE
extern alias MicrosoftXnaFramework;
using MsXna_MediaPlayer = MicrosoftXnaFramework::Microsoft.Xna.Framework.Media.MediaPlayer;
#endif


using System;
using Microsoft.Xna.Framework.Audio;
using System.Linq;

#if IOS
using MonoTouch.AudioToolbox;
using MonoTouch.AVFoundation;
using MonoTouch.Foundation;
using MonoTouch.MediaPlayer;
#endif

#if WINDOWS_MEDIA_ENGINE || WINDOWS_MEDIA_SESSION
using SharpDX;
using SharpDX.MediaFoundation;
using SharpDX.Multimedia;
using SharpDX.Win32;
#endif

#if WINDOWS_PHONE
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Shell;
using System.Threading;
#endif

#if WINRT
using Windows.UI.Core;
#endif


namespace Microsoft.Xna.Framework.Media
{
    public static class MediaPlayer
    {
		// Need to hold onto this to keep track of how many songs
		// have played when in shuffle mode
		private static int _numSongsInQueuePlayed = 0;
		private static MediaState _state = MediaState.Stopped;
		private static float _volume = 1.0f;
		private static bool _isMuted = false;
		private static readonly MediaQueue _queue = new MediaQueue();

		public static event EventHandler<EventArgs> ActiveSongChanged;


#if WINDOWS_MEDIA_ENGINE
        private static readonly MediaEngine _mediaEngineEx;
        private static CoreDispatcher _dispatcher;
#endif

#if WINDOWS_MEDIA_SESSION

        private static readonly MediaSession _session;
        private static SimpleAudioVolume _volumeController;
        private static PresentationClock _clock;

        // HACK: Need SharpDX to fix this.
        private static readonly Guid MRPolicyVolumeService = Guid.Parse("1abaa2ac-9d3b-47c6-ab48-c59506de784d");
        private static readonly Guid SimpleAudioVolumeGuid = Guid.Parse("089EDF13-CF71-4338-8D13-9E569DBDC319");

	    private static Callback _callback;

	    private class Callback : IAsyncCallback
	    {
		    public void Dispose()
		    {
		    }

		    public IDisposable Shadow { get; set; }
		    public void Invoke(AsyncResult asyncResultRef)
		    {
			    var ev = _session.EndGetEvent(asyncResultRef);
			
			    if (ev.TypeInfo == MediaEventTypes.EndOfPresentation)
				    OnSongFinishedPlaying(null, null);

			    _session.BeginGetEvent(this, null);
		    }

		    public AsyncCallbackFlags Flags { get; private set; }
		    public WorkQueueId WorkQueueId { get; private set; }
	    }
#endif

#if WINDOWS_PHONE
        internal static MediaElement _mediaElement;
        private static Uri source;
        private static TimeSpan elapsedTime;

        // track state of player before game is deactivated
        private static MediaState deactivatedState;
        private static bool wasDeactivated;
#endif

        static MediaPlayer()
        {
#if WINDOWS_MEDIA_ENGINE
                MediaManager.Startup(true);
                using (var factory = new MediaEngineClassFactory())
                using (var attributes = new MediaEngineAttributes { AudioCategory = AudioStreamCategory.GameMedia })
                {
                    var creationFlags = MediaEngineCreateFlags.AudioOnly;

                    var mediaEngine = new MediaEngine(factory, attributes, creationFlags, MediaEngineExOnPlaybackEvent);
                    _mediaEngineEx = mediaEngine.QueryInterface<MediaEngineEx>();
                }

                _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
#endif

#if WINDOWS_MEDIA_SESSION
            MediaManagerState.CheckStartup();
            MediaFactory.CreateMediaSession(null, out _session);

#endif

#if WINDOWS_PHONE
            PhoneApplicationService.Current.Activated += (sender, e) =>
                {
                    if (_mediaElement != null)
                    {
                        if (_mediaElement.Source == null && source != null)
                        {
                            _mediaElement.AutoPlay = false;
                            Deployment.Current.Dispatcher.BeginInvoke(() => _mediaElement.Source = source);
                        }

                        // Ensure only one subscription
                        _mediaElement.MediaOpened -= MediaElement_MediaOpened;
                        _mediaElement.MediaOpened += MediaElement_MediaOpened;
                    }
                };

            PhoneApplicationService.Current.Deactivated += (sender, e) => 
                {
                    if (_mediaElement != null)
                    {
                        source = _mediaElement.Source;
                        elapsedTime = _mediaElement.Position;

                        wasDeactivated = true;
                        deactivatedState = _state;
                    }
                };
#endif
        }

#if WINDOWS_PHONE
        private static void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (elapsedTime != TimeSpan.Zero)
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    _mediaElement.Position = elapsedTime;
                    elapsedTime = TimeSpan.Zero;
                });

            if (wasDeactivated)
            {
                if (deactivatedState == MediaState.Playing)
                    _mediaElement.Play();
 
                //reset the deactivated flag
                wasDeactivated = false;
 
                //set auto-play back to default
                _mediaElement.AutoPlay = true;
            }
        }
#endif

#if WINDOWS_MEDIA_ENGINE

        private static void MediaEngineExOnPlaybackEvent(MediaEngineEvent mediaEvent, long param1, int param2)
        {
            if (mediaEvent != MediaEngineEvent.Ended)
                return;

            _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => OnSongFinishedPlaying(null, null)).AsTask();
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

#if WINDOWS_MEDIA_ENGINE
                _mediaEngineEx.Muted = value;
#endif

#if WINDOWS_MEDIA_SESSION
                if (_volumeController != null)
                    _volumeController.Mute = _isMuted;
#endif

#if WINDOWS_PHONE
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    _mediaElement.IsMuted = value;
                });
#endif

#if ANDROID || IOS || MONOMAC || PSM || (WINDOWS && OPENGL) || LINUX
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

#if WINDOWS_MEDIA_ENGINE
                _mediaEngineEx.Loop = value;
#endif
            }
        }

        public static bool IsShuffled { get; set; }

        public static bool IsVisualizationEnabled { get { return false; } }

        public static TimeSpan PlayPosition
        {
            get
            {		
#if WINDOWS_MEDIA_ENGINE
                return TimeSpan.FromSeconds(_mediaEngineEx.CurrentTime);
#endif

#if WINDOWS_MEDIA_SESSION
                return _clock != null ? TimeSpan.FromTicks(_clock.Time) : TimeSpan.Zero;
#endif

#if WINDOWS_PHONE
                TimeSpan pos = TimeSpan.Zero;
                EventWaitHandle Wait = new AutoResetEvent(false);
                if(_mediaElement.Dispatcher.CheckAccess()) {
                    pos = _mediaElement.Position;
                }
                else {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        pos = _mediaElement.Position;
                        Wait.Set();
                    });
                    Wait.WaitOne();
                }
                return (pos);
#endif

#if ANDROID || IOS || MONOMAC || PSM || (WINDOWS && OPENGL) || LINUX
				if (_queue.ActiveSong == null)
					return TimeSpan.Zero;

				return _queue.ActiveSong.Position;
#endif
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
#if IOS
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
#endif

#if WINDOWS_PHONE
                return State == MediaState.Playing || MsXna_MediaPlayer.GameHasControl;
#endif

#if ANDROID || IOS || MONOMAC || PSM || (WINDOWS && OPENGL) || LINUX || WINDOWS_MEDIA_SESSION || WINDOWS_MEDIA_ENGINE
                // TODO: Fix me!
                return true;
#endif
            }
        }
		

        public static float Volume
        {
            get { return _volume; }
			set 
			{       
				_volume = value;

#if WINDOWS_MEDIA_ENGINE
                _mediaEngineEx.Volume = value;       
#endif

#if WINDOWS_MEDIA_SESSION
			    if (_volumeController != null)
                    _volumeController.MasterVolume = _volume;
#endif

#if WINDOWS_PHONE
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    _mediaElement.Volume = value;
                });
#endif

#if ANDROID || IOS || MONOMAC || PSM || (WINDOWS && OPENGL) || LINUX
                if (_queue.ActiveSong == null)
					return;

                _queue.SetVolume(_isMuted ? 0.0f : value);
#endif
			}
        }
		
		#endregion
		
        public static void Pause()
        {
            if (State != MediaState.Playing || _queue.ActiveSong == null)
                return;

#if WINDOWS_MEDIA_ENGINE
            _mediaEngineEx.Pause();
#endif

#if WINDOWS_MEDIA_SESSION
            _session.Pause();
#endif

#if WINDOWS_PHONE
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                _mediaElement.Pause();
            });
#endif

#if ANDROID || IOS || MONOMAC || PSM || (WINDOWS && OPENGL) || LINUX
            _queue.ActiveSong.Pause();
#endif

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
#if WINDOWS_MEDIA_ENGINE

            _mediaEngineEx.Source = song.FilePath;            
            _mediaEngineEx.Load();
            _mediaEngineEx.Play();

#endif
#if WINDOWS_MEDIA_SESSION

            // Cleanup the last song first.
            if (State != MediaState.Stopped)
            {
                _session.Stop();
                _volumeController.Dispose();
                _clock.Dispose();
            }

            // Set the new song.
            _session.SetTopology(0, song.GetTopology());

            // Get the volume interface.
            IntPtr volumeObj;

            
            try
            {
                MediaFactory.GetService(_session, MRPolicyVolumeService, SimpleAudioVolumeGuid, out volumeObj);
            }
            catch
            {
                MediaFactory.GetService(_session, MRPolicyVolumeService, SimpleAudioVolumeGuid, out volumeObj);
            }  
          

            _volumeController = CppObject.FromPointer<SimpleAudioVolume>(volumeObj);
            _volumeController.Mute = _isMuted;
            _volumeController.MasterVolume = _volume;

            // Get the clock.
            _clock = _session.Clock.QueryInterface<PresentationClock>();

			//create the callback if it hasn't been created yet
			if (_callback == null)
			{
				_callback = new Callback();
				_session.BeginGetEvent(_callback, null);
			}

            // Start playing.
            var varStart = new Variant();
            _session.Start(null, varStart);
#endif

#if WINDOWS_PHONE
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                _mediaElement.Source = new Uri(song.FilePath, UriKind.Relative);
                _mediaElement.Play();

                // Ensure only one subscribe
                _mediaElement.MediaEnded -= OnSongFinishedPlaying;
                _mediaElement.MediaEnded += OnSongFinishedPlaying;
            });
#endif

#if ANDROID || IOS || MONOMAC || PSM || (WINDOWS && OPENGL) || LINUX
            song.SetEventHandler(OnSongFinishedPlaying);			
			song.Volume = _isMuted ? 0.0f : _volume;
			song.Play();
#endif
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
                Deployment.Current.Dispatcher.BeginInvoke(() =>
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

#if WINDOWS_MEDIA_ENGINE
            _mediaEngineEx.Play(); 
#endif

#if WINDOWS_MEDIA_SESSION
            _session.Start(null, null);
#endif

#if WINDOWS_PHONE
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                _mediaElement.Play();
            });
#endif

#if ANDROID || IOS || MONOMAC || PSM || (WINDOWS && OPENGL) || LINUX
			_queue.ActiveSong.Resume();
#endif
			State = MediaState.Playing;
        }

        public static void Stop()
        {
            if (State == MediaState.Stopped)
                return;

#if WINDOWS_MEDIA_ENGINE
            _mediaEngineEx.Source = null;
#endif

#if WINDOWS_MEDIA_SESSION
            _session.ClearTopologies();
            _session.Stop();
            _volumeController.Dispose();
            _volumeController = null;
            _clock.Dispose();
            _clock = null;
#endif

#if WINDOWS_PHONE
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                _mediaElement.Stop();
            });
#endif

#if ANDROID || IOS || MONOMAC || PSM || (WINDOWS && OPENGL) || LINUX
			// Loop through so that we reset the PlayCount as well
			foreach(var song in Queue.Songs)
				_queue.ActiveSong.Stop();
#endif
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

