// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Media
{

    /// <summary>
    /// Class that represents a collection of songs.
    /// </summary>
    public class SongCollection :  ICollection<Song>, IEnumerable<Song>, IEnumerable, IDisposable
    {
        private bool disposed;
        private bool isReadOnly = false;
        private List<Song> innerlist;

        /// <summary>
        /// Initializes a new SongCollection with an empty list
        /// of songs.
        /// </summary>
        public SongCollection()
        {
            innerlist = new List<Song>();
        }

        internal SongCollection(List<Song> songs)
        {
            this.innerlist = songs;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">Whether or not this song collection is disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }

        /// <summary>Returns an enumerator that iterates through the song collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the song collection.</returns>
        public IEnumerator<Song> GetEnumerator()
        {
            return innerlist.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return innerlist.GetEnumerator();
        }

        /// <summary>Gets the number of songs contained in the <see cref="SongCollection" />.</summary>
        /// <returns>The number of songs contained in the <see cref="SongCollection" />.</returns>
        public int Count
        {
            get
            {
                return innerlist.Count;
            }
        }

        /// <summary>Gets a value indicating whether the <see cref="SongCollection" /> is read-only.</summary>
        /// <returns>true if the <see cref="SongCollection" /> is read-only; otherwise, false.</returns>
        public bool IsReadOnly
        {
            get
            {
                return this.isReadOnly;
            }
        }


        /// <summary>
        /// Retrieves the song at the specified zero-based
        /// index of this SongCollection.
        /// </summary>
        /// <param name="index">The index of the song</param>
        /// <returns>the song at the specified index</returns>
        public Song this[int index]
        {
            get
            {
                return this.innerlist[index];
            }
        }

        /// <summary>Adds an item to the <see cref="SongCollection" />.</summary>
        /// <param name="item">The object to add to the <see cref="SongCollection" />.</param>
        /// <exception cref="ArgumentNullException">The song is null.</exception>
        public void Add(Song item)
        {

            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (innerlist.Count == 0)
            {
                this.innerlist.Add(item);
                return;
            }

            for (int i = 0; i < this.innerlist.Count; i++)
            {
                if (item.TrackNumber < this.innerlist[i].TrackNumber)
                {
                    this.innerlist.Insert(i, item);
                    return;
                }
            }

            this.innerlist.Add(item);
        }

        /// <summary>Removes all items from the <see cref="SongCollection" />.</summary>
        public void Clear()
        {
            innerlist.Clear();
        }

        /// <summary>
        /// Creates a new SongCollection instance with
        /// all songs in the original
        /// </summary>
        /// <returns>A new song collection that is a copy of this one</returns>
        public SongCollection Clone()
        {
            SongCollection sc = new SongCollection();
            foreach (Song song in this.innerlist)
                sc.Add(song);
            return sc;
        }

        /// <summary>Determines whether the <see cref="SongCollection" /> contains a specific song.</summary>
        /// <returns>true if <paramref name="item" /> is found in the <see cref="SongCollection" />; otherwise, false.</returns>
        /// <param name="item">The object to locate in the <see cref="SongCollection" />.</param>
        public bool Contains(Song item)
        {
            return innerlist.Contains(item);
        }

        /// <summary>Copies the elements of the <see cref="SongCollection" /> to an <see cref="Array" />, starting at a particular <see cref="Array" /> index.</summary>
        /// <param name="array">The one-dimensional <see cref="Array" /> that is the destination of the elements copied from <see cref="SongCollection" />. The <see cref="Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="array" /> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   <paramref name="arrayIndex" /> is less than 0.</exception>
        /// <exception cref="ArgumentException">The number of elements in the source <see cref="ICollection{T}" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.</exception>
        public void CopyTo(Song[] array, int arrayIndex)
        {
            innerlist.CopyTo(array, arrayIndex);
        }


        /// <summary>
        /// Searches for the song and returns the
        /// zero-based index of the first occurrence
        /// within the entire song collection
        /// </summary>
        /// <param name="item">The song to search for</param>
        /// <returns>The index of the song</returns>
        public int IndexOf(Song item)
        {
            return innerlist.IndexOf(item);
        }

        /// <summary>Removes the first occurrence of a song from the <see cref="SongCollection" />.</summary>
        ///
        /// <returns>true if <paramref name="item" /> was successfully removed from the <see cref="SongCollection" />; otherwise, false.
        /// This method also returns false if <paramref name="item" /> is not found in the original <see cref="SongCollection" />.
        /// </returns>
        /// <param name="item">The object to remove from the <see cref="SongCollection" />.</param>
        public bool Remove(Song item)
        {
            return innerlist.Remove(item);
        }
    }
}

