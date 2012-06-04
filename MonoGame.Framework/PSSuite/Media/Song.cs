using System;
using System.IO;

using Sce.Pss.Core.Audio;

using Microsoft.Xna.Framework.Audio;
﻿
namespace Microsoft.Xna.Framework.Media
{
    public class Song : IEquatable<Song>, IDisposable
    {
        static internal Song _currentSong;
        static internal BgmPlayer _bgmPlayer;
        
        private Bgm _bgm;
        private string _name;
        private int _playCount;

        internal Song(string fileName)
        {
            _name = fileName;
            _bgm = new Bgm(_name);
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

        private void Prepare()
        {
            
        }

        public bool Equals(Song song)
        {
            return ((object)song != null) && (Name == song.Name);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return Equals(obj as Song);
        }

        public static bool operator ==(Song song1, Song song2)
        {
            if ((object)song1 == null)
            {
                return (object)song2 == null;
            }

            return song1.Equals(song2);
        }

        public static bool operator !=(Song song1, Song song2)
        {
            return !(song1 == song2);
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
            _playCount++;
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

        internal bool Loop
        {
            get
            {
                if (_bgmPlayer != null)
                {
                    return _bgmPlayer.Loop;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (_bgmPlayer != null)
                {
                    if (_bgmPlayer.Loop != value)
                    {
                        _bgmPlayer.Loop = value;
                    }
                }
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

        public TimeSpan Duration
        {
            get
            {
                if (_bgmPlayer != null)
                {
                    return new TimeSpan(0, 0, /*(int)_bgmPlayer.Duration*/0);
                }
                else
                {
                    return new TimeSpan(0);
                }
            }
        }

        public TimeSpan Position
        {
            get
            {
                if (_bgmPlayer != null)
                {
                    return new TimeSpan(0, 0, /* (int)_bgmPlayer.CurrentPosition*/ 0);
                }
                else
                {
                    return new TimeSpan(0);
                }
            }
        }

        public bool IsProtected
        {
            get
            {
                return false;
            }
        }

        public bool IsRated
        {
            get
            {
                return false;
            }
        }

        public string Name
        {
            get
            {
                return Path.GetFileNameWithoutExtension(_name);
            }
        }

        public int PlayCount
        {
            get
            {
                return _playCount;
            }
        }

        public int Rating
        {
            get
            {
                return 0;
            }
        }

        public int TrackNumber
        {
            get
            {
                return 0;
            }
        }
    }
}

