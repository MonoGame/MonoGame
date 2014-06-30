// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using Tao.Sdl;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song : IEquatable<Song>, IDisposable
    {
        private IntPtr _audioData;
        private int _volume = 128; // in SDL units from 0 to 128

        private void PlatformInitialize(string fileName)
        {
            _audioData = Tao.Sdl.SdlMixer.Mix_LoadMUS(fileName);
        }
        
        internal void SetEventHandler(FinishedPlayingHandler handler) { }

        internal void OnFinishedPlaying()
        {
            MediaPlayer.OnSongFinishedPlaying(null, null);
        }
		
        void PlatformDispose(bool disposing)
        {
            if (_audioData != IntPtr.Zero)
                SdlMixer.Mix_FreeMusic(_audioData);
        }

        internal void Play()
        {
            if (_audioData == IntPtr.Zero)
                return;

            // according to MSDN and http://forums.create.msdn.com/forums/p/85718/614272.aspx
            // songs can only be played with the MediaPlayer class. And this class can only play one song at a time.
            // this means that we can easily use the MusicFinished event here without the risk of receiving an event multiple times.
            // also, the DonePlaying handler of this class will only be set while the song is actually played in MediaPlayer.
            // when the next song starts playing, this will then be overwritten, which shouldn't be a problem
            SdlMixer.Mix_HookMusicFinished(OnFinishedPlaying);
            SdlMixer.Mix_PlayMusic(_audioData, 0);
            _playCount++;
        }

        internal void Resume()
        {
            SdlMixer.Mix_ResumeMusic();
        }

        internal void Pause()
        {
            SdlMixer.Mix_PauseMusic();
        }

        internal void Stop()
        {
            SdlMixer.Mix_HaltMusic();
            _playCount = 0;
        }

        internal float Volume
        {
            // sdl volume goes from 0 to 128 instead of 0 to 1
            get { return _volume / 128f; }
            set
            {
                _volume = (int)(value * 128);
                SdlMixer.Mix_VolumeMusic(_volume);
            }
        }

        // TODO: Implement
        public TimeSpan Position
        {
            get
            {
                // not implemented in sdl?
                return new TimeSpan(0);
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

