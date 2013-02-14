#region License
/*
MIT License
Copyright © 2006 The Mono.Xna Team

All rights reserved.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion License

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework
{
    internal class ReusableItemList<T> : ICollection<T>, IEnumerator<T>
    {
        private readonly List<T> _list = new List<T>();
        private int _listTop = 0;
        private int _iteratorIndex;

        #region ICollection<T> Members

        public void Add(T item)
        {
            if (_list.Count > _listTop)
            {
                _list[_listTop] = item;                
            }
            else
            {
                _list.Add(item);
            }

            _listTop++;
        }
		
		public void Sort(IComparer<T> comparison)
		{
			_list.Sort(comparison);
		}
			
		
		public T GetNewItem()
		{
			if (_listTop < _list.Count)
			{
				return _list[_listTop++];
			}
			else
			{
				// Damm...Mono fails in this!
				//return (T) Activator.CreateInstance(typeof(T));
				return default(T);
			}
		}

		public T this[int index]
		{
			get
			{
				if (index >= _listTop) 
					throw new IndexOutOfRangeException();
				return _list[index];
			}
			set
			{
				if (index >= _listTop) 
					throw new IndexOutOfRangeException();
				_list[index] = value;
			}
		}
		
        public void Clear()
        {
            _listTop = 0;
        }

        public void Reset()
        {
            Clear();
            _list.Clear();
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array,arrayIndex);
        }

        public int Count
        {
            get 
            {
                return _listTop;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            _iteratorIndex = -1;
            return this;
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            _iteratorIndex = -1;
            return this;
        }

        #endregion

        #region IEnumerator<T> Members

        public T Current
        {
            get
            {
                return _list[_iteratorIndex];
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        #region IEnumerator Members

        object System.Collections.IEnumerator.Current
        {
            get
            {
                return _list[_iteratorIndex];
            }
        }

        public bool MoveNext()
        {
            _iteratorIndex++;
            return (_iteratorIndex < _listTop);
        }

        #endregion
    }
}
