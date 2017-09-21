// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
#if WINDOWS_UAP
using Windows.Storage.FileProperties;
#elif IOS
using System.Drawing;
using CoreGraphics;
using MediaPlayer;
using UIKit;
#elif ANDROID
using Android.Graphics;
using Android.Provider;
#endif

namespace Microsoft.Xna.Framework.Media
{
    public sealed class Album : IDisposable
    {
        private Artist artist;
        private Genre genre;
        private string album;
        private SongCollection songCollection;
#if WINDOWS_UAP
        private StorageItemThumbnail thumbnail;
#elif IOS && !TVOS
        private MPMediaItemArtwork thumbnail;
#elif ANDROID
        private Android.Net.Uri thumbnail;
#endif

        public Artist Artist
        {
            get
            {
                return this.artist;
            }
        }

        /// <summary>
        /// Gets the duration of the Album.
        /// </summary>
        public TimeSpan Duration
        {
            get
            {
                return TimeSpan.Zero; // Not implemented
            }
        }

        /// <summary>
        /// Gets the Genre of the Album.
        /// </summary>
        public Genre Genre
        {
            get
            {
                return this.genre;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the Album has associated album art.
        /// </summary>
        public bool HasArt
        {
            get
            {
#if WINDOWS_UAP
                return this.thumbnail != null;
#elif IOS && !TVOS
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
                return false;
            }
        }

        /// <summary>
        /// Gets the name of the Album.
        /// </summary>
        public string Name
        {
            get
            {
                return this.album;
            }
        }

        /// <summary>
        /// Gets a SongCollection that contains the songs on the album.
        /// </summary>
        public SongCollection Songs
        {
            get
            {
                return this.songCollection;
            }
        }

       private Album(SongCollection songCollection, string name, Artist artist, Genre genre)
        {
            this.songCollection = songCollection;
            this.album = name;
            this.artist = artist;
            this.genre = genre;
        }
#if WINDOWS_UAP
        internal Album(SongCollection songCollection, string name, Artist artist, Genre genre, StorageItemThumbnail thumbnail)
            : this(songCollection, name, artist, genre)
        {
            this.thumbnail = thumbnail;
        }
#elif IOS && !TVOS
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

        /// <summary>
        /// Immediately releases the unmanaged resources used by this object.
        /// </summary>
        public void Dispose()
        {
#if WINDOWS_UAP
            if (this.thumbnail != null)
                this.thumbnail.Dispose();
#endif
        }
        
#if IOS && !TVOS
        [CLSCompliant(false)]
        public UIImage GetAlbumArt(int width = 0, int height = 0)
        {
            if (width == 0)
                width = (int)this.thumbnail.Bounds.Width;
            if (height == 0)
                height = (int)this.thumbnail.Bounds.Height;

			return this.thumbnail.ImageWithSize(new CGSize(width, height));
        }
#elif ANDROID
        [CLSCompliant(false)]
        public Bitmap GetAlbumArt(int width = 0, int height = 0)
        {
            var albumArt = MediaStore.Images.Media.GetBitmap(MediaLibrary.Context.ContentResolver, this.thumbnail);
            if (width == 0 || height == 0)
                return albumArt;

            var scaledAlbumArt = Bitmap.CreateScaledBitmap(albumArt, width, height, true);
            albumArt.Dispose();
            return scaledAlbumArt;
        }
#else
        /// <summary>
        /// Returns the stream that contains the album art image data.
        /// </summary>
        public Stream GetAlbumArt()
        {
#if WINDOWS_UAP
            if (this.HasArt)
                return this.thumbnail.AsStream();
            return null;
#else
            throw new NotImplementedException();
#endif
        }
#endif

#if IOS && !TVOS
        [CLSCompliant(false)]
        public UIImage GetThumbnail()
        {
            return this.GetAlbumArt(220, 220);
        }
#elif ANDROID
        [CLSCompliant(false)]
        public Bitmap GetThumbnail()
        {
            return this.GetAlbumArt(220, 220);
        }
#else
        /// <summary>
        /// Returns the stream that contains the album thumbnail image data.
        /// </summary>
        public Stream GetThumbnail()
        {
#if WINDOWS_UAP
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
