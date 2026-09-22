// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Audio;
using MonoGame.Interop;

namespace Microsoft.Xna.Framework.Media;

public sealed partial class Song : IEquatable<Song>, IDisposable
{
    private unsafe MGM_Song* _song;
    private ulong _generation;
    private float _volume = 1.0f;

    #region The playback API used by MediaPlayer

    private unsafe void PlatformInitialize(string filePath)
    {
        SoundEffect.Initialize();

        MGM_SongInfo info;
        _song = MGM.Song_Create(filePath, SoundEffect.System, out info);
        if (_song != null && info.duration > 0)
            _duration = TimeSpan.FromMilliseconds(info.duration);
    }

    private unsafe void PlatformDispose(bool disposing)
    {
        Stop();

        if (_song != null)
        {
            MGM.Song_Destroy(_song);
            _song = null;
        }
    }

    private int PlatformGetPlayCount()
    {
        return _playCount;
    }

    internal unsafe float Volume
    {
        get
        {
            return _volume;
        }

        set
        {
            _volume = value;

            if (_song != null)
                MGM.Song_SetVolume(_song, _volume);
        }
    }

    internal unsafe TimeSpan Position
    {
        get
        {
            if (_song == null)
                return TimeSpan.Zero;

            ulong milliseconds = MGM.Song_GetPosition(_song);
            return TimeSpan.FromMilliseconds(milliseconds);
        }
    }

    internal unsafe void Play(TimeSpan? startPosition, FinishedPlayingHandler handler)
    {
        if (_song == null)
            return;

        ulong milliseconds = 0;
        if (startPosition.HasValue)
            milliseconds = (ulong)startPosition.Value.TotalMilliseconds;

        // Only setup the finished callback once.
        if (DonePlaying == null)
            DonePlaying += handler;

        _generation++;
        if (MGM.Song_Play(_song, milliseconds, _generation) != 0)
            _playCount++;
    }

    internal unsafe void Pause()
    {
        if (_song != null)
            MGM.Song_Pause(_song);
    }

    internal unsafe void Resume()
    {
        if (_song != null)
            MGM.Song_Resume(_song);
    }

    internal unsafe void Stop(bool immediate = false)
    {
        _generation++;

        if (_song != null)
            MGM.Song_Stop(_song);
    }

    internal unsafe void Update()
    {
        if (_song == null)
            return;

        MGM_SongEvent songEvent;
        while (MGM.Song_TryDequeueEvent(_song, out songEvent) != 0)
        {
            if (songEvent.generation != _generation)
                continue;

            if (songEvent.type == SongEventType.Completed)
            {
                if (DonePlaying != null)
                    DonePlaying(this, EventArgs.Empty);

                return;
            }

            if (songEvent.type == SongEventType.Failed)
            {
                MediaPlayer.PlatformOnSongFailed(this);
                return;
            }
        }
    }

    #endregion

    #region Media Library Features Not Supported

    private Album PlatformGetAlbum()
    {
        // Not Supported.
        return null;
    }

    private Artist PlatformGetArtist()
    {
        // Not Supported.
        return null;
    }

    private Genre PlatformGetGenre()
    {
        // Not Supported.
        return null;
    }

    private bool PlatformIsProtected()
    {
        // Not Supported.
        return false;
    }

    private bool PlatformIsRated()
    {
        // Not Supported.
        return false;
    }

    private int PlatformGetRating()
    {
        // Not Supported.
        return 0;
    }

    private int PlatformGetTrackNumber()
    {
        // Not Supported.
        return 0;
    }

    #endregion
}
