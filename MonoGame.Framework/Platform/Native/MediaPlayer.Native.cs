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
        return false;
    }

    private static void PlatformSetIsMuted(bool muted)
    {

    }

    private static bool PlatformGetIsRepeating()
    {
        return false;
    }

    private static void PlatformSetIsRepeating(bool repeating)
    {

    }

    private static bool PlatformGetIsShuffled()
    {
        return false;
    }

    private static void PlatformSetIsShuffled(bool shuffled)
    {

    }

    private static TimeSpan PlatformGetPlayPosition()
    {
        return TimeSpan.Zero;
    }

    private static MediaState PlatformGetState()
    {
        return MediaState.Stopped;
    }

    private static float PlatformGetVolume()
    {
        return 0.0f;
    }

    private static void PlatformSetVolume(float volume)
    {

    }

    private static bool PlatformGetGameHasControl()
    {
        return false;
    }

    private static void PlatformPause()
    {

    }

    private static void PlatformPlaySong(Song song, TimeSpan? startPosition)
    {

    }

    private static void PlatformResume()
    {

    }

    private static void PlatformStop()
    {

    }
}
