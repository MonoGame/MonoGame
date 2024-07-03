// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
#if IOS
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
    /// <summary>
    /// Provides access to an album in the media library
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <b>Album</b> class provides information about an album, 
    /// including the album's <see cref="Name"/>, <see cref="Artist"/>, and <see cref="Songs"/>.
    /// </para>
    /// <para>
    /// You can obtain an <b>Album</b> object through the
    /// <see cref="P:Microsoft.Xna.Framework.Media.AlbumCollection.Item(System.Int32)"/>
    /// indexer and the <see cref="Song.Album">Song.Album</see> property.
    /// </para>
    /// </remarks>
    public sealed class Album : IDisposable
    {
        private Artist artist;
        private Genre genre;
        private string album;
        private SongCollection songCollection;
#if IOS && !TVOS
        private MPMediaItemArtwork thumbnail;
#elif ANDROID
        private Android.Net.Uri thumbnail;
#endif

        /// <summary>
        /// Gets the <see cref="Media.Artist"/> of the Album.
        /// </summary>
        /// <value>
        /// <see cref="Media.Artist"/> of this Album.
        /// </value>
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
        /// Gets the <see cref="Media.Genre"/> of the Album.
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
#if IOS && !TVOS
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
        /// Gets a <see cref="Media.SongCollection"/> that contains the songs on the Album.
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
#if IOS && !TVOS
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

        /// <inheritdoc cref="IDisposable.Dispose()"/>
        public void Dispose()
        { }
        
#if IOS && !TVOS
        public UIImage GetAlbumArt(int width = 0, int height = 0)
        {
            if (width == 0)
                width = (int)this.thumbnail.Bounds.Width;
            if (height == 0)
                height = (int)this.thumbnail.Bounds.Height;

			return this.thumbnail.ImageWithSize(new CGSize(width, height));
        }
#elif ANDROID
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
            throw new NotImplementedException();
        }
#endif

#if IOS && !TVOS
        public UIImage GetThumbnail()
        {
            return this.GetAlbumArt(220, 220);
        }
#elif ANDROID
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
            throw new NotImplementedException();
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
