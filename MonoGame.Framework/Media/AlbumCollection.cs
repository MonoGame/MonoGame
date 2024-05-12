﻿// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Media
{
    /// <summary>
    /// A collection of albums in the media library
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="AlbumCollection"/> class provides access to albums in the 
    /// device's media library
    /// </para>
    /// <para>
    /// Use the <see cref="MediaLibrary.Albums"/> property to obtain a collection
    /// of all albums in the media library, the <see cref="Artist.Albums"/> property
    /// to obtain a collection of albums associated with a particular artist, and
    /// the <see cref="Genre.Albums"/> property to obtain a collection of albums
    /// associated with a particular genre.
    /// </para>
    /// </remarks>
    public sealed class AlbumCollection : IDisposable
    {
        private List<Album> albumCollection;

        /// <summary>
        /// Gets the number of Album objects in the AlbumCollection.
        /// </summary>
        public int Count
        {
            get
            {
                return this.albumCollection.Count;
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
        /// Initializes a new instance of the <b>AlbumCollection</b> class, using
        /// a specified collection of <see cref="Album"/> instances.
        /// </summary>
        /// <param name="albums">
        /// The <see cref="Album"/> collection to initialize this <b>AlbumCollection</b> with.
        /// </param>
        public AlbumCollection(List<Album> albums)
        {
            this.albumCollection = albums;
        }

        /// <summary>
        /// Gets the <see cref="Album"/> at the specified index in the <see cref="AlbumCollection"/>.
        /// </summary>
        /// <value>
        /// A new <see cref="Album"/> representing the album at the specified index
        /// in this <b>AlbumCollection</b>
        /// </value>
        /// <param name="index">Index of the Album to get.</param>
        public Album this[int index]
        {
            get
            {
                return this.albumCollection[index];
            }
        }

        /// <summary>
        /// Immediately releases the unmanaged resources used by this object.
        /// </summary>
        public void Dispose()
        {
            foreach (var album in this.albumCollection)
                album.Dispose();
        }
    }
}
