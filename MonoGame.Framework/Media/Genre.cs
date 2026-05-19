// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Media
{
    /// <summary>
    /// Provides access to genre information in the media library.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Genre class provides information about a genre, including the genre's <see cref="Name"/>,
    /// and the <see cref="Albums"/> and <see cref="Songs"/> in that genre that are on the device.
    /// </para>
    /// <para>
    /// You can obtain a Genre object through the
    /// <see cref="Album.Genre">Album.Genre</see> and <see cref="Song.Genre">Song.Genre</see> properties.
    /// </para>
    /// </remarks>
    public sealed class Genre : IDisposable
    {
        private string genre;

        /// <summary>
        /// Gets the <see cref="AlbumCollection"/> for the Genre.
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
        /// Gets the name of the Genre.
        /// </summary>
        public string Name
        {
            get
            {
                return this.genre;
            }
        }

        /// <summary>
        /// Gets the <see cref="SongCollection"/> for the Genre.
        /// </summary>
        public SongCollection Songs
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Creates a new instance of Genre class.
        /// </summary>
        /// <param name="genre">Name of the genre.</param>
        public Genre(string genre)
        {
            this.genre = genre;
        }

        /// <inheritdoc cref="IDisposable.Dispose()"/>
        public void Dispose()
        {
        }

        /// <summary>
        /// Returns a String representation of the Genre.
        /// </summary>
        public override string ToString()
        {
            return this.genre;
        }

        /// <summary>
        /// Gets the hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            return this.genre.GetHashCode();
        }
    }
}
