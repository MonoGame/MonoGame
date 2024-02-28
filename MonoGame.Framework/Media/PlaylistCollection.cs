// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Media
{
    /// <summary>
    /// A collection of playlists in the media library.
    /// </summary>
    public sealed class PlaylistCollection : ICollection<Playlist>, IEnumerable<Playlist>, IEnumerable, IDisposable
    {
		private bool isReadOnly = false;
		private List<Playlist> innerlist = new List<Playlist>();

        /// <inheritdoc cref="IDisposable.Dispose()"/>
        public void Dispose()
        {
        }

        /// <inheritdoc/>
        public IEnumerator<Playlist> GetEnumerator()
        {
            return innerlist.GetEnumerator();
        }
		
        IEnumerator IEnumerable.GetEnumerator()
        {
            return innerlist.GetEnumerator();
        }

        /// <summary>
        /// Gets the number of <see cref="Playlist"/> objects in the <see cref="PlaylistCollection"/>.
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
            get { return this.isReadOnly; }
        }

        /// <summary>
        /// Gets the <see cref="Playlist"/> at the specified index in the <see cref="PlaylistCollection"/>.
        /// </summary>
        public Playlist this[int index]
        {
            get
            {
				return this.innerlist[index];
            }
        }

        /// <summary>
        /// Adds a <see cref="Playlist"/> to this <see cref="PlaylistCollection"/>.
        /// </summary>
		public void Add(Playlist item)
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
                if (item.Duration < this.innerlist[i].Duration)
                {
                    this.innerlist.Insert(i, item);
                    return;
                }
            }

            this.innerlist.Add(item);
        }

        /// <summary>
        /// Removes all items from this <see cref="PlaylistCollection"/>.
        /// </summary>
		public void Clear()
        {
            innerlist.Clear();
        }

        /// <inheritdoc cref="ICloneable.Clone"/>
        public PlaylistCollection Clone()
        {
            PlaylistCollection plc = new PlaylistCollection();
            foreach (Playlist playlist in this.innerlist)
                plc.Add(playlist);
            return plc;
        }

        /// <summary>
        /// Determines whether the collection contains specified <see cref="Playlist"/>
        /// </summary>
        public bool Contains(Playlist item)
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
        public void CopyTo(Playlist[] array, int arrayIndex)
        {
            innerlist.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Searches for the specified <see cref="Playlist"/> and returns the zero-based index
        /// of the first occurence within the entire <see cref="PlaylistCollection"/>.
        /// </summary>
        /// <param name="item">The <see cref="Playlist"/> to locate</param>
        /// <returns>
        /// The zero-based index of the first occurence of <paramref name="item"/> within
        /// the entire <see cref="PlaylistCollection"/>, if found. otherwise, -1.
        /// </returns>
		public int IndexOf(Playlist item)
        {
            return innerlist.IndexOf(item);
        }

        /// <summary>
        /// Removes the first occurrence of a <see cref="Playlist"/> from the <see cref="PlaylistCollection"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="PlaylistCollection"/>.</param>
        /// <returns>
        /// <see langword="true"/> if item was successfully removed from the <see cref="PlaylistCollection"/>;
        /// otherwise, <see langword="false"/>. This method also returns <see langword="false"/>
        /// if item is not found in the original <see cref="PlaylistCollection"/>.
        /// </returns>
        public bool Remove(Playlist item)
        {
            return innerlist.Remove(item);
        }
    }
}

