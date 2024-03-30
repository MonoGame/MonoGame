// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Media;

public sealed partial class Song : IEquatable<Song>, IDisposable
{
    private void PlatformInitialize(string fileName)
    {

    }

    private void PlatformDispose(bool disposing)
    {

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
        return TimeSpan.Zero;
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
        return "";
    }

    private int PlatformGetPlayCount()
    {
        return 0;
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
