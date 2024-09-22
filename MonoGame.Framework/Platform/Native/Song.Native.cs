// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using MonoGame.Interop;

namespace Microsoft.Xna.Framework.Media;

public sealed partial class Song : IEquatable<Song>, IDisposable
{
    private unsafe MGM_Song* _song;

    private GCHandle _self;

    #region The playback API used by MediaPlayer

    private unsafe void PlatformInitialize(string fileName)
    {
        _song = MGM.Song_Create(fileName);
    }

    private unsafe void PlatformDispose(bool disposing)
    {
        if (_self.IsAllocated)
            _self.Free();

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

    private unsafe TimeSpan PlatformGetDuration()
    {
        if (_song == null)
            return TimeSpan.Zero;

        var milliseconds = MGM.Song_GetDuration(_song);
        return TimeSpan.FromMilliseconds(milliseconds);
    }

    internal unsafe float Volume
    {
        get
        {
            if (_song == null)
                return 0.0f;

            return MGM.Song_GetVolume(_song);
        }

        set
        {
            if (_song != null)
                MGM.Song_SetVolume(_song, value);
        }
    }

    internal unsafe TimeSpan Position
    {
        get
        {
            if (_song == null)
                return TimeSpan.Zero;

            var milliseconds = MGM.Song_GetPosition(_song);
            return TimeSpan.FromMilliseconds(milliseconds);
        }
    }

    internal static void FinishedCallback(nint callbackData)
    {
        var self = GCHandle.FromIntPtr(callbackData);
        var song = self.Target as Song;

        // This could happen if we were disposed.
        if (song == null)
            return;

        // This callback is likely coming from a platform
        // specific native thread.  So queue the event to
        // the main game thread for processing on the
        // next tick.

        Threading.OnUIThread(() => song.DonePlaying(song, EventArgs.Empty));
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
        if (!_self.IsAllocated)
            _self = GCHandle.Alloc(this, GCHandleType.Weak);

        MGM.Song_Play(_song, milliseconds, FinishedCallback, (nint)_self);

        _playCount++;
    }

    internal unsafe void Pause()
    {
        if (_song == null)
            return;

        MGM.Song_Pause(_song);
    }

    internal unsafe void Resume()
    {
        if (_song == null)
            return;

        MGM.Song_Resume(_song);
    }

    internal unsafe void Stop()
    {
        if (_song == null)
            return;

        MGM.Song_Stop(_song);
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

    private string PlatformGetName()
    {
        // Not Supported.
        return string.Empty;
    }

    #endregion
}
