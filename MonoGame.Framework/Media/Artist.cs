// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Media
{
    /// <summary>
    /// Provides access to artist information in the media library.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="Artist"/> class provides information about an artist, including the artist's <see cref="Name"/>, <see cref="Albums"/>, and <see cref="Songs"/>
    /// </para>
    /// <para>
    /// You can obtain an <b>Artist</b> through the <see cref="Album.Artist">Album.Artist</see>
    /// and <see cref="Song.Artist">Song.Artist</see> properties.
    /// </para>
    /// </remarks>
    public sealed class Artist : IDisposable
    {
        private string artist;

        /// <summary>
        /// Gets the <see cref="AlbumCollection">AlbumCollection</see> for the Artist.
        /// </summary>
        public AlbumCollection Albums
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the object is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the name of the Artist.
        /// </summary>
        public string Name
        {
            get
            {
                return this.artist;
            }
        }

        /// <summary>
        /// Gets the <see cref="SongCollection"/> for the Artist.
        /// </summary>
        public SongCollection Songs
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Creates a new instance of Artist class.
        /// </summary>
        /// <param name="artist">Name of the artist.</param>
        public Artist(string artist)
        {
            this.artist = artist;
        }

        /// <inheritdoc cref="IDisposable.Dispose()"/>
        public void Dispose()
        {
        }

        /// <summary>
        /// Returns a String representation of the Artist.
        /// </summary>
        public override string ToString()
        {
            return this.artist.ToString();
        }

        /// <summary>
        /// Gets the hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            return this.artist.GetHashCode();
        }
    }
}
