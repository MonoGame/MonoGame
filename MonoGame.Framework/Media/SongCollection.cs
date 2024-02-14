// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Media
{
    /// <summary>
    /// A collection of songs in the song library.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="SongCollection"/> class provides access to songs in the device's song library.
    /// </para>
    /// <para>
    /// Use the <see cref="MediaLibrary.Songs"/> property to obtain the following collections:
    /// All songs in the media library; 
    /// Songs on a particular album; 
    /// Songs associated with a particular artist; 
    /// Songs associated with a particular genre; 
    /// </para>
    /// </remarks>
	public class SongCollection : ICollection<Song>, IEnumerable<Song>, IEnumerable, IDisposable
	{
		private bool isReadOnly = false;
		private List<Song> innerlist = new List<Song>();

        internal SongCollection()
        {

        }

        internal SongCollection(List<Song> songs)
        {
            this.innerlist = songs;
        }

        /// <summary>
        /// Immediately releases the unmanaged resources used by this object.
        /// </summary>
		public void Dispose()
        {
        }

        /// <inheritdoc/>
        public IEnumerator<Song> GetEnumerator()
        {
            return innerlist.GetEnumerator();
        }
		
        IEnumerator IEnumerable.GetEnumerator()
        {
            return innerlist.GetEnumerator();
        }

        /// <summary>
        /// Gets the number of <see cref="Song"/> objects in the <see cref="SongCollection"/>.
        /// </summary>
        public int Count
        {
            get
            {
				return innerlist.Count;
            }
        }

        /// <summary>
        /// Gets whether this collection is read-only,
        /// </summary>
        public bool IsReadOnly
        {
		    get
		    {
		        return this.isReadOnly;
		    }
        }

        /// <summary>
        /// Gets the <see cref="Song"/> at the specified index in the <see cref="SongCollection"/>.
        /// </summary>
        public Song this[int index]
        {
            get
            {
				return this.innerlist[index];
            }
        }

        /// <summary>
        /// Adds a <see cref="Song"/> to this <see cref="SongCollection"/>.
        /// </summary>
		public void Add(Song item)
        {

            if (item == null)
                throw new ArgumentNullException();

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

        /// <summary>
        /// Removes all items from this <see cref="SongCollection"/>.
        /// </summary>
        public void Clear()
        {
            innerlist.Clear();
        }

        /// <inheritdoc cref="ICloneable.Clone"/>
        public SongCollection Clone()
        {
            SongCollection sc = new SongCollection();
            foreach (Song song in this.innerlist)
                sc.Add(song);
            return sc;
        }

        /// <summary>
        /// Determines whether a <see cref="Song"/> is in the <see cref="SongCollection"/>
        /// </summary>
        /// <returns><see langword="true"/> if <paramref name="item"/> is found in the <see cref="SongCollection"/>; otherwise, <see langword="false"/>.</returns>
        public bool Contains(Song item)
        {
            return innerlist.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the collection to an <see cref="Array"/>,
        /// starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied
        /// from collection. The <see cref="Array"/> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(Song[] array, int arrayIndex)
        {
            innerlist.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Searches for the specified <see cref="Song"/> and returns the zero-based index of the first occurence within the entire <see cref="SongCollection"/>.
        /// </summary>
        /// <param name="item">The <see cref="Song"/> to locate</param>
        /// <returns>
        /// The zero-based index of the first occurence of <paramref name="item"/> within the entire <see cref="SongCollection"/>, if found. otherwise, -1.
        /// </returns>
        public int IndexOf(Song item)
        {
            return innerlist.IndexOf(item);
        }

        /// <summary>
        /// Removes the first occurrence of a <see cref="Song"/> from the <see cref="SongCollection"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="SongCollection"/>.</param>
        /// <returns>
        /// <see langword="true"/> if item was successfully removed from the <see cref="SongCollection"/>;
        /// otherwise, <see langword="false"/>. This method also returns <see langword="false"/> if item is not found in the
        /// original <see cref="SongCollection"/>.
        /// </returns>
        public bool Remove(Song item)
        {
            return innerlist.Remove(item);
        }
	}
}

