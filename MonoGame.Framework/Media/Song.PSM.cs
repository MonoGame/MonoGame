// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using Sce.PlayStation.Core.Audio;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song : IEquatable<Song>, IDisposable
    {
        static internal Song _currentSong;
        static internal BgmPlayer _bgmPlayer;
		
		private Bgm _bgm;

        private void PlatformInitialize(string fileName)
        {
			_bgm = new Bgm(fileName);
        }
     
        private void PlatformDispose(bool disposing)
        {
            if (_currentSong == this && _bgmPlayer != null)
            {
                _bgmPlayer.Stop();
                _bgmPlayer.Dispose();
            }
			
            _bgm.Dispose();
		}

        internal void SetEventHandler(FinishedPlayingHandler handler)
        {
            // TODO: Implement event handler to fire off
            // when the song's complete.
            return;
        }
		
		internal void Play()
        {
            if (_currentSong != this) //If needed switch up the current song
            {
                if (_bgmPlayer != null)
                {
                    _bgmPlayer.Stop();
                    _bgmPlayer.Dispose();
                }
                _bgmPlayer = _bgm.CreatePlayer();
                _currentSong = this;
            }
            _bgmPlayer.Play();
        }

        internal void Resume()
        {
            if (_bgmPlayer != null && _currentSong == this)
            {
                _bgmPlayer.Resume();
            }
        }

        internal void Pause()
        {
            if (_bgmPlayer != null && _currentSong == this)
            {
                _bgmPlayer.Pause();
            }
        }

        internal void Stop()
        {
            if (_bgmPlayer != null && _currentSong == this)
            {
                _bgmPlayer.Stop();
            }
        }

        internal float Volume
        {
            get
            {
				if (_bgmPlayer != null)
                {
					return _bgmPlayer.Volume;
				}
				else
				{
					return 0.0f;
				}                
            }

            set
            {
                if (_bgmPlayer != null)
                {
                    if (_bgmPlayer.Volume != value)
                    {
                        _bgmPlayer.Volume = value;
                    }
                }
            }
        }
		
		internal TimeSpan Position
		{
			get 
			{
				if (_bgmPlayer == null)
					return TimeSpan.Zero;
				
				return TimeSpan.FromSeconds(_bgmPlayer.Time);
				
			}
		}

        private Album PlatformGetAlbum()
        {
            return null;
        }

        private Artist PlatformGetArtist()
        {
            return null;
        }

        private Genre PlatformGetGenre()
        {
            return null;
        }

        private TimeSpan PlatformGetDuration()
        {
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
            return Path.GetFileNameWithoutExtension(_name);
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

