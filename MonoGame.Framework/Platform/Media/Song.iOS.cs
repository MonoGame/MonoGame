// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Foundation;
using AVFoundation;
using MediaPlayer;
using CoreMedia;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song
    {
        private Album album;
        private Artist artist;
        private Genre genre;
        private string title;
        private TimeSpan duration;
        #if !TVOS
        private MPMediaItem mediaItem;
        #endif
        private AVPlayerItem _sound;
        private AVPlayer _player;
        private NSUrl assetUrl;
        private NSObject playToEndObserver;

        public NSUrl AssetUrl
        {
            get { return this.assetUrl; }
        }

        #if !TVOS
        internal Song(Album album, Artist artist, Genre genre, string title, TimeSpan duration, MPMediaItem mediaItem, NSUrl assetUrl)
        #else
        internal Song(Album album, Artist artist, Genre genre, string title, TimeSpan duration, object mediaItem, NSUrl assetUrl)
        #endif
        {
            this.album = album;
            this.artist = artist;
            this.genre = genre;
            this.title = title;
            this.duration = duration;
            #if !TVOS
            this.mediaItem = mediaItem;
            #endif
            this.assetUrl = assetUrl;
        }

        private void PlatformInitialize(string fileName)
        {
            this.PlatformInitialize(NSUrl.FromFilename(fileName));
        }

        private void PlatformInitialize(NSUrl url)
        {
            _sound = AVPlayerItem.FromUrl(url);
            _player = AVPlayer.FromPlayerItem(_sound);
            playToEndObserver = AVPlayerItem.Notifications.ObserveDidPlayToEndTime(OnFinishedPlaying);
        }

        private void PlatformDispose(bool disposing)
        {
            if (_sound == null)
                return;
                
            playToEndObserver.Dispose ();
            playToEndObserver = null;

            _sound.Dispose();
            _sound = null;

            _player.Dispose();
            _player = null;
        }

        internal void OnFinishedPlaying (object sender, NSNotificationEventArgs args)
		{
			if (DonePlaying != null)
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

        internal void Play(TimeSpan? startPosition)
        {
            if (_player == null)
            {
                // MediaLibrary items are lazy loaded
                if (assetUrl != null)
                    this.PlatformInitialize (assetUrl);
                else
                    return;
            }

            PlatformPlay(startPosition);

            _playCount++;
        }

        private void PlatformPlay(TimeSpan? startPosition)
        {
            
            if (startPosition.HasValue)
                _player.Seek(CMTime.FromSeconds(startPosition.Value.TotalSeconds, 1));
            else
                _player.Seek(CMTime.Zero); // Seek to start to ensure playback at the start.
            
            _player.Play();
        }

		internal void Resume()
		{
            if (_player == null)
				return;

            PlatformResume();
		}

        private void PlatformResume()
        {
			_player.Play();
        }
		
		internal void Pause()
		{			            
            if (_player == null)
				return;
			
            _player.Pause();
        }
		
		internal void Stop()
		{
            if (_player == null)
				return;
			
            _player.Pause();
			_playCount = 0;
		}

		internal float Volume
		{
			get
			{
                if (_player != null)
                    return _player.Volume;
				else
					return 0.0f;
			}
			
			set
			{
                if ( _player != null && _player.Volume != value )
                    _player.Volume = value;
			}			
		}

		internal TimeSpan Position
        {
            get
            {
                return TimeSpan.FromSeconds(_player.CurrentTime.Seconds);		
            }
            set
            {
                _player.Seek(CMTime.FromSeconds(value.TotalSeconds, 1000));
            }
        }

        private Album PlatformGetAlbum()
        {
            return this.album;
        }

        private Artist PlatformGetArtist()
        {
            return this.artist;
        }

        private Genre PlatformGetGenre()
        {
            return this.genre;
        }

        private TimeSpan PlatformGetDuration()
        {
            #if !TVOS
            if (this.mediaItem != null)
                return this.duration;
            #endif
            return _duration;
        }

        private bool PlatformIsProtected()
        {
            return false;
        }

        private bool PlatformIsRated()
        {
            return false;
        }

        private string PlatformGetName()
        {
            return this.title ?? Path.GetFileNameWithoutExtension(_name);
        }

        private int PlatformGetPlayCount()
        {
            return _playCount;
        }

        private int PlatformGetRating()
        {
            return 0;
        }

        private int PlatformGetTrackNumber()
        {
            return 0;
        }
    }
}

