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

#if !WINDOWS_UAP
		private MusicProperties musicProperties;

        [CLSCompliant(false)]
        public StorageFile StorageFile
        {
            get { return this.musicProperties.File; }
        }
#endif

		internal Song(Album album, Artist artist, Genre genre)
        {
            this.album = album;
            this.artist = artist;
            this.genre = genre;
        }

#if !WINDOWS_UAP
		internal Song(Album album, Artist artist, Genre genre, MusicProperties musicProperties)
			: this(album, artist, genre)
		{
			this.musicProperties = musicProperties;
		}
#endif

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
#if !WINDOWS_UAP
            if (this.musicProperties != null)
                return this.musicProperties.Duration;
#endif
            return _duration;
        }

        private bool PlatformIsProtected()
        {
#if !WINDOWS_UAP
            if (this.musicProperties != null)
                return this.musicProperties.IsProtected;
#endif
            return false;
        }

        private bool PlatformIsRated()
        {
#if !WINDOWS_UAP
            if (this.musicProperties != null)
                return this.musicProperties.Rating != 0;
#endif
            return false;
        }

        private string PlatformGetName()
        {
#if !WINDOWS_UAP
            if (this.musicProperties != null)
                return this.musicProperties.Title;
#endif
            return Path.GetFileNameWithoutExtension(_name);
        }

        private int PlatformGetPlayCount()
        {
            return _playCount;
        }

        private int PlatformGetRating()
        {
#if !WINDOWS_UAP
            if (this.musicProperties != null)
                return this.musicProperties.Rating;
#endif
            return 0;
        }

        private int PlatformGetTrackNumber()
        {
#if !WINDOWS_UAP
            if (this.musicProperties != null)
                return this.musicProperties.TrackNumber;
#endif
            return 0;
        }
    }
}