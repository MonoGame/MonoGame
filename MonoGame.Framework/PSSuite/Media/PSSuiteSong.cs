using System;
using System.IO;

using Sce.Pss.Core.Audio;

using Microsoft.Xna.Framework.Audio;
﻿
namespace Microsoft.Xna.Framework.Media
{
    internal class PSSuiteSong : IDisposable
    {
        static internal PSSuiteSong _currentSong;
        static internal BgmPlayer _bgmPlayer;
        
        private Bgm _bgm;

        internal PSSuiteSong(string fileName)
        {
            _bgm = new Bgm(fileName);
        }

        public void Dispose()
        {
            if (_currentSong == this && _bgmPlayer != null)
            {
                _bgmPlayer.Stop();
                _bgmPlayer.Dispose();
            }
            _bgm.Dispose();
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
    }
}

