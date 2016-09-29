// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

extern alias MicrosoftXnaFramework;
using MsMediaPlayer = MicrosoftXnaFramework::Microsoft.Xna.Framework.Media.MediaPlayer;

using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Shell;
using System.Threading;

namespace Microsoft.Xna.Framework.Media
{
    public static partial class MediaPlayer
    {
        internal static MediaElement _mediaElement;
        private static Uri source;
        private static TimeSpan elapsedTime;

        // track state of player before game is deactivated
        private static MediaState deactivatedState;
        private static bool wasDeactivated;

        private static void PlatformInitialize()
        {
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
        }

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

        #region Properties
        private static bool PlatformGetIsMuted()
        {
            if (playingInternal)
                return MsMediaPlayer.IsMuted;

            return _isMuted;
        }

        private static void PlatformSetIsMuted(bool muted)
        {
            _isMuted = muted;

            if (playingInternal)
                MsMediaPlayer.IsMuted = _isMuted;
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    _mediaElement.IsMuted = _isMuted;
                });
            }
        }


        private static bool PlatformGetIsRepeating()
        {
            return _isRepeating;
        }

        private static void PlatformSetIsRepeating(bool repeating)
        {
            _isRepeating = repeating;

            if (playingInternal)
                MsMediaPlayer.IsRepeating = _isRepeating;
        }

        private static bool PlatformGetIsShuffled()
        {
            return _isShuffled;
        }

        private static void PlatformSetIsShuffled(bool shuffled)
        {
            _isShuffled = shuffled;

            if (playingInternal)
                MsMediaPlayer.IsShuffled = _isShuffled;
        }

        private static TimeSpan PlatformGetPlayPosition()
        {
            if (playingInternal)
                return MsMediaPlayer.PlayPosition;

            if (_mediaElement == null)
                return TimeSpan.Zero;

            if (_mediaElement.Dispatcher.CheckAccess())
                return _mediaElement.Position;

            TimeSpan pos;
            EventWaitHandle Wait = new AutoResetEvent(false);
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                pos = _mediaElement.Position;
                Wait.Set();
            });
            Wait.WaitOne();
            return pos;
        }

        private static MediaState PlatformGetState()
        {
            if (playingInternal)
            {
                switch (MsMediaPlayer.State)
                {
                    case MicrosoftXnaFramework::Microsoft.Xna.Framework.Media.MediaState.Paused:
                        return MediaState.Paused;
                    case MicrosoftXnaFramework::Microsoft.Xna.Framework.Media.MediaState.Playing:
                        return MediaState.Playing;
                    default:
                        return MediaState.Stopped;
                }
            }

            return _state;
        }

        private static bool PlatformGetGameHasControl()
        {
            return (!playingInternal && State == MediaState.Playing) || MsMediaPlayer.GameHasControl;
        }

        private static float PlatformGetVolume()
        {
            if (playingInternal)
                return MsMediaPlayer.Volume;

            return _volume;
        }

        private static void PlatformSetVolume(float volume)
        {
            _volume = volume;

            if (playingInternal)
                MsMediaPlayer.Volume = volume;
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    // Unlike other implementations, MediaElement uses a linear scale for volume
                    // On WP8 a volume of 0.85 seems to refer to 50% volume according to MSDN
                    // http://msdn.microsoft.com/EN-US/library/windowsphone/develop/system.windows.controls.mediaelement.volume%28v=vs.105%29.aspx
                    // Therefore a good approximation could be to use the 4th root of volume
                    _mediaElement.Volume = Math.Pow(_volume, 1/4d);
                });
            }
        }
		
		#endregion

        private static void PlatformPause()
        {
            if (playingInternal)
                MsMediaPlayer.Pause();
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    _mediaElement.Pause();
                });
            }
        }

        private static void PlatformPlaySong(Song song, TimeSpan? startPosition)
        {
            if (startPosition.HasValue)
                throw new Exception("startPosition not implemented on WindowsPhone"); //Should be able to implement for MediaElement, but not possible with MsMediaPlayer (XNA)

            if (song.InternalSong != null)
            {
                playingInternal = true;

                // Ensure only one subscription
                MsMediaPlayer.MediaStateChanged -= MsMediaStateChanged;
                MsMediaPlayer.MediaStateChanged += MsMediaStateChanged;
                MsMediaPlayer.ActiveSongChanged -= MsActiveSongChanged;
                MsMediaPlayer.ActiveSongChanged += MsActiveSongChanged;

                MsMediaPlayer.Play(song.InternalSong);
            }
            else
            {
                playingInternal = false;

                MsMediaPlayer.MediaStateChanged -= MsMediaStateChanged;
                MsMediaPlayer.ActiveSongChanged -= MsActiveSongChanged;

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    _mediaElement.Source = new Uri(song.FilePath, UriKind.Relative);
                    _mediaElement.Play();

                    // Ensure only one subscribe
                    _mediaElement.MediaEnded -= OnSongFinishedPlaying;
                    _mediaElement.MediaEnded += OnSongFinishedPlaying;
                });
            }
        }

        private static void MsMediaStateChanged(object sender, EventArgs args)
        {
            MediaStateChanged(sender, args);
        }

        private static void MsActiveSongChanged(object sender, EventArgs args)
        {
            ActiveSongChanged(sender, args);
        }

        private static void PlatformResume()
        {
            if (playingInternal)
                MsMediaPlayer.Resume();
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    _mediaElement.Play();
                });
            }
        }

        private static void PlatformStop()
        {
            if (playingInternal)
                MsMediaPlayer.Stop();
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    _mediaElement.Stop();
                });
            }
        }
    }
}

