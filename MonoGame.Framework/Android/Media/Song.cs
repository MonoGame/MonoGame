using System;
using System.IO;
﻿
namespace Microsoft.Xna.Framework.Media
{
    public sealed class Song : IEquatable<Song>, IDisposable
    {
        static Android.Media.MediaPlayer _androidPlayer;
        static Song _playingSong;

        private readonly string _name;
        private readonly TimeSpan _duration = TimeSpan.Zero;
        private int _playCount;
        private bool _disposed;

        internal delegate void FinishedPlayingHandler(object sender, EventArgs args);
        event FinishedPlayingHandler DonePlaying;

        internal Song (string fileName, int durationMS)
            :this(fileName)
        {
            _duration = TimeSpan.FromMilliseconds(durationMS);
        }
        internal Song(string fileName)
        {
            _name = fileName;
            if (_androidPlayer == null)
            {
                _androidPlayer = new Android.Media.MediaPlayer();
                _androidPlayer.Completion += new EventHandler(AndroidPlayer_Completion);
            }
        }

        ~Song()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                // ...

                _disposed = true;
            }
        }

        private void Prepare()
        {
            var afd = Game.Activity.Assets.OpenFd(_name);
            if (afd != null)
            {
                _androidPlayer.Reset();
                _androidPlayer.SetDataSource(afd.FileDescriptor, afd.StartOffset, afd.Length);
                _androidPlayer.Prepare();
                _androidPlayer.Looping = MediaPlayer.IsRepeating;
                _playingSong = this;
            }
        }

        static void AndroidPlayer_Completion(object sender, EventArgs e)
        {
            var playingSong = _playingSong;
            _playingSong = null;

            if (playingSong != null && playingSong.DonePlaying != null)
                playingSong.DonePlaying(sender, e);
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
            Prepare();
            _androidPlayer.Start();
            _playCount++;
        }

        internal void Resume()
        {
            _androidPlayer.Start();
        }

        internal void Pause()
        {
            _androidPlayer.Pause();
        }

        internal void Stop()
        {
            _androidPlayer.Stop();
            _playingSong = null;
            _playCount = 0;
        }

        internal float Volume
        {
            get
            {
                return 0.0f;
            }

            set
            {
                _androidPlayer.SetVolume(value, value);
            }
        }

        public TimeSpan Duration
        {
            get
            {
                if (_duration == TimeSpan.Zero && _playingSong == this)
                    return TimeSpan.FromMilliseconds(_androidPlayer.Duration);

                return _duration;
            }
        }

        public TimeSpan Position
        {
            get
            {
                if (_playingSong == this)
                    return TimeSpan.FromMilliseconds(_androidPlayer.CurrentPosition);
             
                return TimeSpan.Zero;
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

