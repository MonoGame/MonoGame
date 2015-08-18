// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Windows.Storage;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song
    {
        private Album album;
        private Artist artist;
        private Genre genre;
        
		private MusicProperties musicProperties;

        [CLSCompliant(false)]
        public StorageFile StorageFile
        {
            get { return this.musicProperties.File; }
        }
        
		internal Song(Album album, Artist artist, Genre genre, MusicProperties musicProperties)
		{
            this.album = album;
            this.artist = artist;
            this.genre = genre;
            this.musicProperties = musicProperties;
		}

		private void PlatformInitialize(string fileName)
        {

        }

        private void PlatformDispose(bool disposing)
        {

        }

        private Album PlatformGetAlbum()
        {
            return this.album;
        }

        private void PlatformSetAlbum(Album album)
        {
            this.album = album;
        }

        private Artist PlatformGetArtist()
        {
            return this.artist;
        }

        private Genre PlatformGetGenre()
        {
            return this.genre;
        }

        private TimeSpan PlatformGetDuration()
        {
            if (this.musicProperties != null)
                return this.musicProperties.Duration;

            return _duration;
        }

        private bool PlatformIsProtected()
        {
            if (this.musicProperties != null)
                return this.musicProperties.IsProtected;

            return false;
        }

        private bool PlatformIsRated()
        {
            if (this.musicProperties != null)
                return this.musicProperties.Rating != 0;

            return false;
        }

        private string PlatformGetName()
        {
            if (this.musicProperties != null)
                return this.musicProperties.Title;

            return Path.GetFileNameWithoutExtension(_name);
        }

        private int PlatformGetPlayCount()
        {
            return _playCount;
        }

        private int PlatformGetRating()
        {
            if (this.musicProperties != null)
                return this.musicProperties.Rating;

            return 0;
        }

        private int PlatformGetTrackNumber()
        {
            if (this.musicProperties != null)
                return this.musicProperties.TrackNumber;

            return 0;
        }
    }
}