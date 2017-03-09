// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song : IEquatable<Song>, IDisposable
    {
        private static float _volume = 1f;

        private IntPtr _music;
        private Stopwatch _timer;
        private TimeSpan? _startPos;

        private void PlatformInitialize(string fileName)
        {
            _music = SdlMixer.LoadMUS(fileName);
            _timer = new Stopwatch();
        }
        
        internal void SetEventHandler(FinishedPlayingHandler handler) { }
		
        void PlatformDispose(bool disposing)
        {
            if (_music != IntPtr.Zero)
            {
                SdlMixer.FreeMusic(_music);
                _music = IntPtr.Zero;
            }
        }

        internal void Play(TimeSpan? startPosition)
        {
            SdlMixer.PlayMusic(_music, 1);
            _timer.Start();
            if (startPosition.HasValue)
                SdlMixer.SetMusicPosition(startPosition.Value.TotalMilliseconds / 1000.0);
            _startPos = startPosition;
            _playCount++;
        }

        internal void Resume()
        {
            SdlMixer.ResumeMusic();
            _timer.Start();
        }

        internal void Pause()
        {
            SdlMixer.PauseMusic();
            _timer.Stop();
        }

        internal void Stop()
        {
            SdlMixer.PauseMusic();
            _timer.Reset();
            _playCount = 0;
        }

        internal float Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                SdlMixer.VolumeMusic((int)(_volume * 128));
            }
        }

        public TimeSpan Position
        {
            get
            {
                return _startPos.HasValue ? _startPos.Value + _timer.Elapsed : _timer.Elapsed;
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

