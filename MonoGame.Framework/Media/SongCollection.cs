// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if WINDOWS_PHONE
extern alias MicrosoftXnaFramework;
using MsSongCollection = MicrosoftXnaFramework::Microsoft.Xna.Framework.Media.SongCollection;
#endif

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Media
{
	public class SongCollection : ICollection<Song>, IEnumerable<Song>, IEnumerable, IDisposable
	{
		private bool isReadOnly = false;
		private List<Song> innerlist = new List<Song>();
#if WINDOWS_PHONE
        private MsSongCollection songCollection;

        internal SongCollection(MsSongCollection songCollection)
        {
            this.songCollection = songCollection;
        }
#endif

        internal SongCollection()
        {

        }

        internal SongCollection(List<Song> songs)
        {
            this.innerlist = songs;
        }

		public void Dispose()
        {
#if WINDOWS_PHONE
            if (this.songCollection != null)
                this.songCollection.Dispose();
#endif
        }
		
		public IEnumerator<Song> GetEnumerator()
        {
#if WINDOWS_PHONE
            if (this.songCollection != null)
                throw new NotSupportedException();
#endif
            return innerlist.GetEnumerator();
        }
		
        IEnumerator IEnumerable.GetEnumerator()
        {
#if WINDOWS_PHONE
            if (this.songCollection != null)
                return this.songCollection.GetEnumerator();
#endif
            return innerlist.GetEnumerator();
        }

        public int Count
        {
            get
            {
#if WINDOWS_PHONE
                if (this.songCollection != null)
                    return this.songCollection.Count;
#endif
				return innerlist.Count;
            }
        }
		
		public bool IsReadOnly
        {
		    get
		    {
#if WINDOWS_PHONE
		        if (this.songCollection != null)
		            return true;
#endif
		        return this.isReadOnly;
		    }
        }

        public Song this[int index]
        {
            get
            {
#if WINDOWS_PHONE
                if (this.songCollection != null)
                    return new Song(this.songCollection[index]);
#endif
				return this.innerlist[index];
            }
        }
		
		public void Add(Song item)
        {
#if WINDOWS_PHONE
            if (this.songCollection != null)
                throw new NotSupportedException();
#endif

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
		
		public void Clear()
        {
#if WINDOWS_PHONE
            if (this.songCollection != null)
                throw new NotSupportedException();
#endif
            innerlist.Clear();
        }
        
        public SongCollection Clone()
        {
#if WINDOWS_PHONE
            if (this.songCollection != null)
                throw new NotSupportedException();
#endif
            SongCollection sc = new SongCollection();
            foreach (Song song in this.innerlist)
                sc.Add(song);
            return sc;
        }
        
        public bool Contains(Song item)
        {
#if WINDOWS_PHONE
            if (this.songCollection != null)
                throw new NotSupportedException();
#endif
            return innerlist.Contains(item);
        }
        
        public void CopyTo(Song[] array, int arrayIndex)
        {
#if WINDOWS_PHONE
            if (this.songCollection != null)
                throw new NotSupportedException();
#endif
            innerlist.CopyTo(array, arrayIndex);
        }
		
		public int IndexOf(Song item)
        {
#if WINDOWS_PHONE
            if (this.songCollection != null)
                throw new NotSupportedException();
#endif
            return innerlist.IndexOf(item);
        }
        
        public bool Remove(Song item)
        {
#if WINDOWS_PHONE
            if (this.songCollection != null)
                throw new NotSupportedException();
#endif
            return innerlist.Remove(item);
        }
	}
}

