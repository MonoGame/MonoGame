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

#if WINDOWS_PHONE
extern alias MicrosoftXnaFramework;
using MsXna_MediaPlayer = MicrosoftXnaFramework::Microsoft.Xna.Framework.Media.MediaPlayer;
#endif


using System;

using Microsoft.Xna.Framework.Audio;

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
#elif WINDOWS_PHONE
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Shell;
using System.Threading;
#endif
#if WINRT
using Windows.UI.Core;
#endif

using System.Linq;


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
#if WINDOWS_PHONE
        private static System.Windows.Threading.Dispatcher _dispatcher;
#else
        private static CoreDispatcher _dispatcher;
#endif
#elif WINDOWS_MEDIA_SESSION

        private static readonly MediaSession _session;
        private static SimpleAudioVolume _volumeController;
        private static PresentationClock _clock;

        // HACK: Need SharpDX to fix this.
        private static readonly Guid MRPolicyVolumeService = Guid.Parse("1abaa2ac-9d3b-47c6-ab48-c59506de784d");
        private static readonly Guid SimpleAudioVolumeGuid = Guid.Parse("089EDF13-CF71-4338-8D13-9E569DBDC319");
#elif WINDOWS_PHONE
        internal static MediaElement _mediaElement;
        private static Uri source;
        private static TimeSpan elapsedTime;
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
#elif WINDOWS_MEDIA_SESSION
            MediaManagerState.CheckStartup();
            MediaFactory.CreateMediaSession(null, out _session);
#elif WINDOWS_PHONE
            PhoneApplicationService.Current.Activated += (sender, e) =>
                {
                    if (_mediaElement != null)
                    {
                        if (_mediaElement.Source == null && source != null)
                            Deployment.Current.Dispatcher.BeginInvoke(() => _mediaElement.Source = source);

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
#elif WINDOWS_MEDIA_SESSION
                if (_volumeController != null)
                    _volumeController.Mute = _isMuted;
#elif WINDOWS_PHONE
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    _mediaElement.IsMuted = value;
                });
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
#elif WINDOWS_MEDIA_SESSION
                return _clock != null ? TimeSpan.FromTicks(_clock.Time) : TimeSpan.Zero;
#elif WINDOWS_PHONE
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
#else
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
#elif WINDOWS_PHONE
                return State == MediaState.Playing || MsXna_MediaPlayer.GameHasControl;
#else
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
#elif WINDOWS_MEDIA_SESSION
			    if (_volumeController != null)
                    _volumeController.MasterVolume = _volume;
#elif WINDOWS_PHONE
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    _mediaElement.Volume = value;
                });
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
            if (State != MediaState.Playing || _queue.ActiveSong == null)
                return;

#if WINDOWS_MEDIA_ENGINE
            _mediaEngineEx.Pause();
#elif WINDOWS_MEDIA_SESSION
            _session.Pause();
#elif WINDOWS_PHONE
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                _mediaElement.Pause();
            });
#else
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

#elif WINDOWS_MEDIA_SESSION

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
            catch (Exception e)
            {
                MediaFactory.GetService(_session, MRPolicyVolumeService, SimpleAudioVolumeGuid, out volumeObj);
            }  
          

            _volumeController = CppObject.FromPointer<SimpleAudioVolume>(volumeObj);
            _volumeController.Mute = _isMuted;
            _volumeController.MasterVolume = _volume;

            // Get the clock.
            _clock = _session.Clock.QueryInterface<PresentationClock>();

            // Start playing.
            var varStart = new Variant();
            _session.Start(null, varStart);
#elif WINDOWS_PHONE
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                _mediaElement.Source = new Uri(song.FilePath, UriKind.Relative);
                _mediaElement.Play();

                // Ensure only one subscribe
                _mediaElement.MediaEnded -= OnSongFinishedPlaying;
                _mediaElement.MediaEnded += OnSongFinishedPlaying;
            });
#else
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
					State = MediaState.Stopped;

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
#elif WINDOWS_MEDIA_SESSION
            _session.Start(null, null);
#elif WINDOWS_PHONE
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                _mediaElement.Play();
            });
#else
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
#elif WINDOWS_MEDIA_SESSION
            _session.ClearTopologies();
            _session.Stop();
            _volumeController.Dispose();
            _volumeController = null;
            _clock.Dispose();
            _clock = null;
#elif WINDOWS_PHONE
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                _mediaElement.Stop();
            });
#else		
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

