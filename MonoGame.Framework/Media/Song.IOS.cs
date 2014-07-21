// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using MonoTouch.Foundation;
using MonoTouch.AVFoundation;
using MonoTouch.MediaPlayer;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song
    {
        private Album album;
        private Artist artist;
        private Genre genre;
        private string title;
        private TimeSpan duration;
        private MPMediaItem mediaItem;
        private AVAudioPlayer _sound;
        private NSUrl assetUrl;

        internal Song(Album album, Artist artist, Genre genre, string title, TimeSpan duration, MPMediaItem mediaItem, NSUrl assetUrl)
        {
            this.album = album;
            this.artist = artist;
            this.genre = genre;
            this.title = title;
            this.duration = duration;
            this.mediaItem = mediaItem;
            this.assetUrl = assetUrl;
        }

        private void PlatformInitialize(string fileName)
        {
            this.PlatformInitialize(NSUrl.FromFilename(fileName));
        }

        private void PlatformInitialize(NSUrl url)
        {
            _sound = AVAudioPlayer.FromUrl(url);
            _sound.NumberOfLoops = 0;
            _sound.FinishedPlaying += OnFinishedPlaying;
        }

        [CLSCompliant(false)]
        public Song(NSUrl url)
        {
            PlatformInitialize(url);
        }

        private void PlatformDispose(bool disposing)
        {
            if (_sound == null)
                return;
                
            _sound.FinishedPlaying -= OnFinishedPlaying;
            _sound.Dispose();
            
            _sound = null;
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

		internal void Play()
		{	
            if (_sound == null)
            {
                // MediaLibrary items are lazy loaded
                if (assetUrl != null)
                    this.PlatformInitialize (assetUrl);
                else
                    return;
            }

            PlatformPlay();

            _playCount++;
        }

        private void PlatformPlay()
        {
            // AVAudioPlayer sound.Stop() does not reset the playback position as XNA does.
            // Set Play's currentTime to 0 to ensure playback at the start.
            _sound.CurrentTime = 0.0;
            
            _sound.Play();
        }

		internal void Resume()
		{
			if (_sound == null)
				return;

            PlatformResume();
		}

        private void PlatformResume()
        {
			_sound.Play();
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

		internal TimeSpan Position
        {
            get
            {
                return TimeSpan.FromSeconds(_sound.CurrentTime);		
            }
            set
            {
                _sound.CurrentTime = value.TotalSeconds;
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
            if (this.mediaItem != null)
                return this.duration;

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

