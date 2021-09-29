// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Media
{
    /// <summary>
    /// A collection of songs in the song library.The SongCollection does not immediately instantiate instances of all songs in
    /// the collection. Instead, individual Song objects are created each time a user accesses a song through the collection's Item. 
    /// </summary>
	public class SongCollection : ICollection<Song>, IEnumerable<Song>, IEnumerable, IDisposable
	{
        private bool isReadOnly = false;
        //The list that holds the collection of songs 
        private readonly List<Song> innerlist;

        /// <summary>
        /// The public constructor that initalizes the song collection list  
        /// </summary>
        public SongCollection()
        {
            innerlist = new List<Song>();
        }

        /// <summary>
        /// Internal constructor that sets the inner list based on the song list that is passed into the constructor
        /// </summary>
        /// <param name="songs"> A list of songs</param>
        internal SongCollection(List<Song> songs)
        {
            this.innerlist = songs;
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Returns an enumerator that iterates through the SongCollection
        /// </summary>
        /// <returns> the enumerator</returns>
		public IEnumerator<Song> GetEnumerator()
        {
            return innerlist.GetEnumerator();
        }
		
        IEnumerator IEnumerable.GetEnumerator()
        {
            return innerlist.GetEnumerator();
        }

        /// <summary>
        /// Count the song items in the Song Collection and returns the number
        /// </summary>
        public int Count
        {
            get
            {
				return innerlist.Count;
            }
        }
		
		public bool IsReadOnly
        {
		    get
		    {
		        return this.isReadOnly;
		    }
        }

        /// <summary>
        /// Checks the song on a given index and return it. 
        /// </summary>
        /// <param name="index"> Index that is specified to search </param>
        /// <returns>Returns the song name at the index </returns>
        public Song this[int index]
        {
            get
            {
				return this.innerlist[index];
            }
        }

        /// <summary>
        /// Allows to add a song item into the song collection list. 
        /// </summary>
        /// <param name="item"> The song that needs be added </param>
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
        /// Clears the song collection list 
        /// </summary>
		public void Clear()
        {
            innerlist.Clear();
        }

        /// <summary>
        /// Songs will be cloned into the list 
        /// </summary>
        /// <returns> Returns the list with the cloned songs </returns>
        public SongCollection Clone()
        {
            SongCollection sc = new SongCollection();
            foreach (Song song in this.innerlist)
                sc.Add(song);
            return sc;
        }

        /// <summary>
        /// Checks whether a song will be in the Song Collection. If it does contain return true otherwise false. 
        /// </summary>
        /// <param name="item"> The song item that will be searched through the list </param>
        /// <returns> Returns true or false</returns>
        public bool Contains(Song item)
        {
            return innerlist.Contains(item);
        }

        /// <summary>
        /// Copies the list into an array
        /// </summary>
        /// <param name="array"> The array that will be get copied intp </param>
        /// <param name="arrayIndex">The index that will be get copied </param>
        public void CopyTo(Song[] array, int arrayIndex)
        {
            innerlist.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Find the index of a song from the song list
        /// </summary>
        /// <param name="item"> Song item that needs to be searched </param>
        /// <returns> The index of the given song </returns>
		public int IndexOf(Song item)
        {
            return innerlist.IndexOf(item);
        }

        /// <summary>
        /// Remove method removes the given song from the Song Collection
        /// </summary>
        /// <param name="item">The song item that needs to be removed</param>
        /// <returns> The list after removing the song. </returns>
        public bool Remove(Song item)
        {
            return innerlist.Remove(item);
        }
	}
}

