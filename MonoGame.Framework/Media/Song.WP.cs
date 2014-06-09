// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

extern alias MicrosoftXnaFramework;
using System;
using System.IO;
using MsSong = MicrosoftXnaFramework::Microsoft.Xna.Framework.Media.Song;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song
    {
        internal MsSong InternalSong { get; private set; }

        internal Song(MsSong song)
        {
            this.InternalSong = song;
        }

        private void PlatformInitialize(string fileName)
        {

        }

        private void PlatformDispose(bool disposing)
        {
            
        }

        private Album PlatformGetAlbum()
        {
            if (this.InternalSong != null)
                return (Album)this.InternalSong.Album;

            return null;
        }

        private Artist PlatformGetArtist()
        {
            if (this.InternalSong != null)
                return this.InternalSong.Artist;

            return null;
        }

        private Genre PlatformGetGenre()
        {
            if (this.InternalSong != null)
                return this.InternalSong.Genre;

            return null;
        }

        private TimeSpan PlatformGetDuration()
        {
            if (this.InternalSong != null)
                return this.InternalSong.Duration;

            return _duration;
        }

        private bool PlatformIsProtected()
        {
            if (this.InternalSong != null)
                return this.InternalSong.IsProtected;

            return false;
        }

        private bool PlatformIsRated()
        {
            if (this.InternalSong != null)
                return this.InternalSong.IsRated;

            return false;
        }

        private string PlatformGetName()
        {
            if (this.InternalSong != null)
                return this.InternalSong.Name;

            return Path.GetFileNameWithoutExtension(_name);
        }

        private int PlatformGetPlayCount()
        {
            if (this.InternalSong != null)
                return this.InternalSong.PlayCount;

            return _playCount;
        }

        private int PlatformGetRating()
        {
            if (this.InternalSong != null)
                return this.InternalSong.Rating;

            return 0;
        }

        private int PlatformGetTrackNumber()
        {
            if (this.InternalSong != null)
                return this.InternalSong.TrackNumber;

            return 0;
        }
    }
}