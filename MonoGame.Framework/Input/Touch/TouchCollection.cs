#region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009-2010 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
#endregion License

#region Using clause
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
#endregion Using clause

namespace Microsoft.Xna.Framework.Input.Touch
{
    public struct TouchCollection : IList<TouchLocation>
	{
        private const int MAX_TOUCHES = 12;

        private int _count;
        private TouchLocation _location0;
        private TouchLocation _location1;
        private TouchLocation _location2;
        private TouchLocation _location3;
        private TouchLocation _location4;
        private TouchLocation _location5;
        private TouchLocation _location6;
        private TouchLocation _location7;
        private TouchLocation _location8;
        private TouchLocation _location9;
        private TouchLocation _location10;
        private TouchLocation _location11;

        private bool _isConnected;

		#region Properties

		public bool IsConnected { get { return _isConnected; } }

		#endregion

        public TouchCollection(IList<TouchLocation> touches)
        {
            if (touches == null)
                throw new ArgumentNullException();
            if (touches.Count > MAX_TOUCHES)
                throw new ArgumentOutOfRangeException("Value exceeds max touches of " + MAX_TOUCHES);
   
            _isConnected = true;

            _count = 0;
            _location0 = new TouchLocation();
            _location1 = new TouchLocation();
            _location2 = new TouchLocation();
            _location3 = new TouchLocation();
            _location4 = new TouchLocation();
            _location5 = new TouchLocation();
            _location6 = new TouchLocation();
            _location7 = new TouchLocation();
            _location8 = new TouchLocation();
            _location9 = new TouchLocation();
            _location10 = new TouchLocation();
            _location11 = new TouchLocation();

            for (int i = 0; i < touches.Count; i++)
            {
                _internalAdd(touches[i]);
            }
        }

        private void _internalAdd(TouchLocation touch)
        {
            switch (_count)
            {
                case 0:
                    _location0 = touch;
                    break;
                case 1:
                    _location1 = touch;
                    break;
                case 2:
                    _location2 = touch;
                    break;
                case 3:
                    _location3 = touch;
                    break;
                case 4:
                    _location4 = touch;
                    break;
                case 5:
                    _location5 = touch;
                    break;
                case 6:
                    _location6 = touch;
                    break;
                case 7:
                    _location7 = touch;
                    break;
                case 8:
                    _location8 = touch;
                    break;
                case 9:
                    _location9 = touch;
                    break;
                case 10:
                    _location10 = touch;
                    break;
                case 11:
                    _location11 = touch;
                    break;
            }

            _count++;
        }

        public bool FindById(int id, out TouchLocation touchLocation)
		{
            for (var i = 0; i < _count; i++)
            {
                var location = this[i];
                if (location.Id == id)
                {
                    touchLocation = location;
                    return true;
                }
            }

            touchLocation = default(TouchLocation);
            return false;
		}

        #region IList<TouchLocation>

        public bool IsReadOnly
        {
            get { return true; }
        }

        public int IndexOf(TouchLocation item)
        {
            for (int i = 0; i < _count; i++)
            {
                if (this[i] == item)
                    return i;
            }

            return -1;
        }

        public void Insert(int index, TouchLocation item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public TouchLocation this[int index]
        {
            get
            {
                if (index < 0 || index >= _count)
                {
                    throw new ArgumentOutOfRangeException();
                }

                switch (index)
                {
                    case 0:
                        return _location0;
                    case 1:
                        return _location1;
                    case 2:
                        return _location2;
                    case 3:
                        return _location3;
                    case 4:
                        return _location4;
                    case 5:
                        return _location5;
                    case 6:
                        return _location6;
                    case 7:
                        return _location7;
                    case 8:
                        return _location8;
                    case 9:
                        return _location9;
                    case 10:
                        return _location10;
                    default:
                        return _location11;
                }
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public void Add(TouchLocation item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(TouchLocation item)
        {
            return IndexOf(item) >= 0;
        }

        public void CopyTo(TouchLocation[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException();
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException();
            if (array.Length < _count + arrayIndex)
                throw new ArgumentOutOfRangeException();

            for (int i = 0; i < _count; i++)
            {
                array[arrayIndex + i] = this[i];
            }
        }

        public int Count
        {
            get { return _count; }
        }

        public bool Remove(TouchLocation item)
        {
            throw new NotSupportedException();
        }

        public TouchCollection.Enumerator GetEnumerator()
        {
            return new TouchCollection.Enumerator(this);
        }

        IEnumerator<TouchLocation> IEnumerable<TouchLocation>.GetEnumerator()
        {
            return new TouchCollection.Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new TouchCollection.Enumerator(this);
        }

        public struct Enumerator : IEnumerator<TouchLocation>, IDisposable
        {
            private TouchCollection collection;
            private int position;

            public TouchLocation Current
            {
                get
                {
                    return this.collection[this.position];
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return (object)this.Current;
                }
            }

            internal Enumerator(TouchCollection collection)
            {
                this.collection = collection;
                this.position = -1;
            }

            public bool MoveNext()
            {
                this.position++;

                if (this.position < this.collection.Count)
                    return true;

                this.position = this.collection.Count;

                return false;
            }

            void IEnumerator.Reset()
            {
                this.position = -1;
            }

            public void Dispose()
            {
               
            }
        }

        #endregion // IList<TouchLocation>
    }
}