// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Media;

public static partial class MediaPlayer
{
    private static void PlatformInitialize()
    {
    }

    private static bool PlatformGetIsMuted()
    {
        return _isMuted;
    }

    private static void PlatformSetIsMuted(bool muted)
    {
        _isMuted = muted;

        if (_queue.Count == 0)
            return;

        var newVolume = _isMuted ? 0.0f : _volume;
        _queue.SetVolume(newVolume);
    }

    private static bool PlatformGetIsRepeating()
    {
        return _isRepeating;
    }

    private static void PlatformSetIsRepeating(bool repeating)
    {
        _isRepeating = repeating;
    }

    private static bool PlatformGetIsShuffled()
    {
        return _isShuffled;
    }

    private static void PlatformSetIsShuffled(bool shuffled)
    {
        _isShuffled = shuffled;
    }

    private static TimeSpan PlatformGetPlayPosition()
    {
        if (_queue.ActiveSong == null)
            return TimeSpan.Zero;

        return _queue.ActiveSong.Position;
    }

    private static MediaState PlatformGetState()
    {
        return _state;
    }

    private static float PlatformGetVolume()
    {
        return _volume;
    }

    private static void PlatformSetVolume(float volume)
    {
        _volume = volume;

        if (_queue.ActiveSong == null)
            return;

        _queue.SetVolume(_isMuted ? 0.0f : _volume);
    }

    private static bool PlatformGetGameHasControl()
    {
        return false;
    }

    private static void PlatformPause()
    {
        if (_queue.ActiveSong == null)
            return;

        _queue.ActiveSong.Pause();
    }

    private static void PlatformPlaySong(Song song, TimeSpan? startPosition)
    {
        if (_queue.ActiveSong == null)
            return;

        song.Volume = _isMuted ? 0.0f : _volume;
        song.Play(startPosition, OnSongFinishedPlaying);
    }

    private static void PlatformResume()
    {
        if (_queue.ActiveSong == null)
            return;

        _queue.ActiveSong.Resume();
    }

    private static void PlatformStop()
    {
        // TOOD: What is this loop doing?
        foreach (var song in Queue.Songs)
            _queue.ActiveSong.Stop();
    }
}
