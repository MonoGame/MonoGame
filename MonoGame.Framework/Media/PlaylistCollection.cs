// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Media
{

    public sealed class PlaylistCollection : ICollection<Playlist>, IEnumerable<Playlist>, IEnumerable, IDisposable
    {
		private bool isReadOnly = false;
		private List<Playlist> innerlist = new List<Playlist>();
		
        public void Dispose()
        {
        }

        public IEnumerator<Playlist> GetEnumerator()
        {
            return innerlist.GetEnumerator();
        }
		
        IEnumerator IEnumerable.GetEnumerator()
        {
            return innerlist.GetEnumerator();
        }

        public int Count
        {
            get
            {
				return innerlist.Count;
            }
        }
		
		public bool IsReadOnly
        {
            get { return this.isReadOnly; }
        }

        public Playlist this[int index]
        {
            get
            {
				return this.innerlist[index];
            }
        }
		
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
		
		public void Clear()
        {
            innerlist.Clear();
        }
        
        public PlaylistCollection Clone()
        {
            PlaylistCollection plc = new PlaylistCollection();
            foreach (Playlist playlist in this.innerlist)
                plc.Add(playlist);
            return plc;
        }
        
        public bool Contains(Playlist item)
        {
            return innerlist.Contains(item);
        }
        
        public void CopyTo(Playlist[] array, int arrayIndex)
        {
            innerlist.CopyTo(array, arrayIndex);
        }
		
		public int IndexOf(Playlist item)
        {
            return innerlist.IndexOf(item);
        }
        
        public bool Remove(Playlist item)
        {
            return innerlist.Remove(item);
        }
    }
}

