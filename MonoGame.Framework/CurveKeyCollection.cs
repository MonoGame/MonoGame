// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework
{
    public class CurveKeyCollection : ICollection<CurveKey>, IEnumerable<CurveKey>, IEnumerable
    {
        #region Private Fields

        private bool isReadOnly = false;
        private List<CurveKey> innerlist;

        #endregion Private Fields


        #region Properties

        public int Count
        {
            get { return innerlist.Count; }
        }

        public bool IsReadOnly
        {
            get { return this.isReadOnly; }
        }

        public CurveKey this[int index]
        {
            get { return innerlist[index]; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                if (index >= innerlist.Count)
                    throw new IndexOutOfRangeException();

                if (innerlist[index].Position == value.Position)
                    innerlist[index] = value;
                else
                {
                    innerlist.RemoveAt(index);
                    innerlist.Add(value);
                }
            }
        }

        #endregion Properties


        #region Constructors

        public CurveKeyCollection()
        {
            innerlist = new List<CurveKey>();
        }

        #endregion Constructors


        #region Public Methods

        public void Add(CurveKey item)
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
                if (item.Position < this.innerlist[i].Position)
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
        
        public CurveKeyCollection Clone()
        {
            CurveKeyCollection ckc = new CurveKeyCollection();
            foreach (CurveKey key in this.innerlist)
                ckc.Add(key);
            return ckc;
        }
        
        public bool Contains(CurveKey item)
        {
            return innerlist.Contains(item);
        }
        
        public void CopyTo(CurveKey[] array, int arrayIndex)
        {
            innerlist.CopyTo(array, arrayIndex);
        }
        
        public IEnumerator<CurveKey> GetEnumerator()
        {
            return innerlist.GetEnumerator();
        }
        
        public int IndexOf(CurveKey item)
        {
            return innerlist.IndexOf(item);
        }
        
        public bool Remove(CurveKey item)
        {
            return innerlist.Remove(item);
        }
        
        public void RemoveAt(int index)
        {
            innerlist.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return innerlist.GetEnumerator();
        }

        #endregion Public Methods
    }
}
