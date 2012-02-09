using System;
using System.IO;

using Microsoft.Xna.Framework.Audio;
﻿
namespace Microsoft.Xna.Framework.Media
{
    public class Song : IEquatable<Song>, IDisposable
    {
        static internal Android.Media.MediaPlayer _androidPlayer = null;
        private string _name;
        private int _playCount;

        internal Song(string fileName)
        {
            _name = fileName;
            if (_androidPlayer == null)
                _androidPlayer = new Android.Media.MediaPlayer();
        }

        public void Dispose()
        {
        }

        private void Prepare()
        {
            if (_androidPlayer != null)
            {
                var afd = Game.Activity.Assets.OpenFd(_name);
                if (afd != null)
                {
                    _androidPlayer.Reset();
                    _androidPlayer.SetDataSource(afd.FileDescriptor, afd.StartOffset, afd.Length);
                    _androidPlayer.Prepare();
                    _androidPlayer.Looping = true;
                }
            }
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
            if (_androidPlayer != null)
            {
                Prepare();
                _androidPlayer.Start();
                _playCount++;
            }
        }

        internal void Resume()
        {
            if (_androidPlayer != null)
            {
                _androidPlayer.Start();
            }
        }

        internal void Pause()
        {
            if (_androidPlayer != null)
            {
                _androidPlayer.Pause();
            }
        }

        internal void Stop()
        {
            if (_androidPlayer != null)
            {
                _androidPlayer.Stop();
            }
        }

        internal bool Loop
        {
            get
            {
                if (_androidPlayer != null)
                {
                    return _androidPlayer.Looping;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (_androidPlayer != null)
                {
                    if (_androidPlayer.Looping != value)
                    {
                        _androidPlayer.Looping = value;
                    }
                }
            }
        }

        internal float Volume
        {
            get
            {
                return 0.0f;
            }

            set
            {
                if (_androidPlayer != null)
                {
                    _androidPlayer.SetVolume(value, value);
                }
            }
        }

        public TimeSpan Duration
        {
            get
            {
                if (_androidPlayer != null)
                {
                    return new TimeSpan(0, 0, (int)_androidPlayer.Duration);
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
                if (_androidPlayer != null)
                {
                    return new TimeSpan(0, 0, (int)_androidPlayer.CurrentPosition);
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

