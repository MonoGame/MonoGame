// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if WINDOWS_PHONE
extern alias MicrosoftXnaFramework;
using MsAlbum = MicrosoftXnaFramework::Microsoft.Xna.Framework.Media.Album;
#endif
using System;
using System.IO;
#if WINDOWS_STOREAPP
using Windows.Storage.FileProperties;
#elif IOS
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.MediaPlayer;
using MonoTouch.UIKit;
#elif ANDROID
using Android.Graphics;
using Android.Provider;
#endif

namespace Microsoft.Xna.Framework.Media
{
    public sealed class Album : IDisposable
    {
#if WINDOWS_PHONE
        private MsAlbum album;
#else
        private Artist artist;
        private Genre genre;
        private string album;
        private SongCollection songCollection;
#if WINDOWS_STOREAPP
        private StorageItemThumbnail thumbnail;
#elif IOS
        private MPMediaItemArtwork thumbnail;
#elif ANDROID
        private Android.Net.Uri thumbnail;
#endif
#endif

        public Artist Artist
        {
            get
            {
#if WINDOWS_PHONE
                return album.Artist;
#else
                return this.artist;
#endif
            }
        }

        /// <summary>
        /// Gets the duration of the Album.
        /// </summary>
        public TimeSpan Duration
        {
            get
            {
#if WINDOWS_PHONE
                return album.Duration;
#else
                return TimeSpan.Zero; // Not implemented
#endif
            }
        }

        /// <summary>
        /// Gets the Genre of the Album.
        /// </summary>
        public Genre Genre
        {
            get
            {
#if WINDOWS_PHONE
                return album.Genre;
#else
                return this.genre;
#endif
            }
        }

        /// <summary>
        /// Gets a value indicating whether the Album has associated album art.
        /// </summary>
        public bool HasArt
        {
            get
            {
#if WINDOWS_PHONE
                return this.album.HasArt;
#elif WINDOWS_STOREAPP
                return this.thumbnail != null;
#elif IOS
                // If album art is missing the bounds will be: Infinity, Infinity, 0, 0
                return this.thumbnail != null && this.thumbnail.Bounds.Width != 0;
#elif ANDROID
                return this.thumbnail != null;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Gets a value indicating whether the object is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get
            {
#if WINDOWS_PHONE
                return album.IsDisposed;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Gets the name of the Album.
        /// </summary>
        public string Name
        {
            get
            {
#if WINDOWS_PHONE
                return album.Name;
#else
                return this.album;
#endif
            }
        }

        /// <summary>
        /// Gets a SongCollection that contains the songs on the album.
        /// </summary>
        public SongCollection Songs
        {
            get
            {
#if WINDOWS_PHONE
                return new SongCollection(album.Songs);
#else
                return this.songCollection;
#endif
            }
        }

#if WINDOWS_PHONE
        public static explicit operator Album(MsAlbum album)
        {
            return new Album(album);
        }

        private Album(MsAlbum album)
        {
            this.album = album;
        }
#else
        private Album(SongCollection songCollection, string name, Artist artist, Genre genre)
        {
            this.songCollection = songCollection;
            this.album = name;
            this.artist = artist;
            this.genre = genre;
        }
#if WINDOWS_STOREAPP
        internal Album(SongCollection songCollection, string name, Artist artist, Genre genre, StorageItemThumbnail thumbnail)
            : this(songCollection, name, artist, genre)
        {
            this.thumbnail = thumbnail;
        }
#elif IOS
        internal Album(SongCollection songCollection, string name, Artist artist, Genre genre, MPMediaItemArtwork thumbnail)
            : this(songCollection, name, artist, genre)
        {
            this.thumbnail = thumbnail;
        }
#elif ANDROID
        internal Album(SongCollection songCollection, string name, Artist artist, Genre genre, Android.Net.Uri thumbnail)
            : this(songCollection, name, artist, genre)
        {
            this.thumbnail = thumbnail;
        }
#endif
#endif

        /// <summary>
        /// Immediately releases the unmanaged resources used by this object.
        /// </summary>
        public void Dispose()
        {
#if WINDOWS_PHONE
            this.album.Dispose();
#elif WINDOWS_STOREAPP
            if (this.thumbnail != null)
                this.thumbnail.Dispose();
#endif
        }
        
#if IOS
        [CLSCompliant(false)]
        public UIImage GetAlbumArt()
        {
            return this.thumbnail.ImageWithSize(new SizeF(this.thumbnail.Bounds.Width, this.thumbnail.Bounds.Height));
        }
#elif ANDROID
        [CLSCompliant(false)]
        public Bitmap GetAlbumArt()
        {
            return MediaStore.Images.Media.GetBitmap(MediaLibrary.Context.ContentResolver, this.thumbnail);
        }
#else
        /// <summary>
        /// Returns the stream that contains the album art image data.
        /// </summary>
        public Stream GetAlbumArt()
        {
#if WINDOWS_PHONE
            return this.album.GetAlbumArt();
#elif WINDOWS_STOREAPP
            if (this.HasArt)
                return this.thumbnail.AsStream();
            return null;
#else
            throw new NotImplementedException();
#endif
        }
#endif

#if IOS
        [CLSCompliant(false)]
        public UIImage GetThumbnail()
        {
            return this.thumbnail.ImageWithSize(new SizeF(100, 100)); // TODO: Check size
        }
#elif ANDROID
        [CLSCompliant(false)]
        public Bitmap GetThumbnail()
        {
            using (var albumArt = this.GetAlbumArt())
            {
                return Bitmap.CreateScaledBitmap(albumArt, 100, 100, false); // TODO: Check size
            }
        }
#else
        /// <summary>
        /// Returns the stream that contains the album thumbnail image data.
        /// </summary>
        public Stream GetThumbnail()
        {
#if WINDOWS_PHONE
            return this.album.GetThumbnail();
#elif WINDOWS_STOREAPP
            if (this.HasArt)
                return this.thumbnail.AsStream();

            return null;
#else
            throw new NotImplementedException();
#endif
        }
#endif

		/// <summary>
        /// Returns a String representation of this Album.
        /// </summary>
        public override string ToString()
        {
            return this.album.ToString();
        }

        /// <summary>
        /// Gets the hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            return this.album.GetHashCode();
        }
    }
}
