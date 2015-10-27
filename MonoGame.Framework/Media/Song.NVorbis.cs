// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using MonoGame.Utilities;
using OpenTK.Audio;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song : IEquatable<Song>, IDisposable
    {
        private OggStream stream;
        private float _volume = 1f;

        private void PlatformInitialize(string fileName)
        {
            stream = new OggStream(fileName, OnFinishedPlaying);
            stream.Prepare();

            _duration = stream.GetLength();
        }
        
        internal void SetEventHandler(FinishedPlayingHandler handler) { }

        internal void OnFinishedPlaying()
        {
            MediaPlayer.OnSongFinishedPlaying(null, null);
        }
		
        void PlatformDispose(bool disposing)
        {
            stream.Dispose();
        }

        internal void Play(TimeSpan? startPosition)
        {
            stream.Play();
            if (startPosition != null)
                stream.SeekToPosition((TimeSpan)startPosition);

            _playCount++;
        }

        internal void Resume()
        {
            stream.Resume();
        }

        internal void Pause()
        {
            stream.Pause();
        }

        internal void Stop()
        {
            stream.Stop();
            _playCount = 0;
        }

        internal float Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                stream.Volume = _volume;
            }
        }

        public TimeSpan Position
        {
            get
            {
                return stream.GetPosition();
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

