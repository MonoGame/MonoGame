#region License
/*
 Microsoft Public License (Ms-PL)
 MonoGame - Copyright © 2012 The MonoGame Team
 
 All rights reserved.
 
 This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
 accept the license, do not use the software.
 
 1. Definitions
 The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
 U.S. copyright law.
 
 A "contribution" is the original software, or any additions or changes to the software.
 A "contributor" is any person that distributes its contribution under this license.
 "Licensed patents" are a contributor's patent claims that read directly on its contribution.
 
 2. Grant of Rights
 (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
 each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
 (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
 each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
 
 3. Conditions and Limitations
 (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
 (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
 your patent license from such contributor to the software ends automatically.
 (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
 notices that are present in the software.
 (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
 a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
 code form, you may only do so under a license that complies with this license.
 (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
 or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
 permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
 purpose and non-infringement.
 */
#endregion License

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides methods and properties for maintaining a vertex channel.
    /// This is a generic implementation of VertexChannel and, therefore, can handle strongly typed content data.
    /// </summary>
    public sealed class VertexChannel<T> : VertexChannel, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
    {
        List<T> items;

        /// <summary>
        /// Gets the strongly-typed list for the base class to access.
        /// </summary>
        internal override IList Items
        {
            get
            {
                return items;
            }
        }

        /// <summary>
        /// Gets the type of data contained in this channel.
        /// </summary>
        public override Type ElementType
        {
            get
            {
                return typeof(T);
            }
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        public T this[int index]
        {
            get
            {
                return items[index];
            }
            set
            {
                items[index] = value;
            }
        }

        /// <summary>
        /// true if this object is read-only; false otherwise.
        /// </summary>
        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Creates an instance of VertexChannel.
        /// </summary>
        /// <param name="name">Name of the channel.</param>
        internal VertexChannel(string name)
            : base(name)
        {
            items = new List<T>();
        }

        /// <summary>
        /// Determines whether the specified element is in the channel.
        /// </summary>
        /// <param name="item">Element being searched for.</param>
        /// <returns>true if the element is present; false otherwise.</returns>
        public bool Contains(T item)
        {
            return items.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the channel to an array, starting at the specified index.
        /// </summary>
        /// <param name="array">Array that will receive the copied channel elements.</param>
        /// <param name="arrayIndex">Starting index for copy operation.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets an enumerator interface for reading channel content.
        /// </summary>
        /// <returns>Enumeration of the channel content.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        /// <summary>
        /// Gets the index of the specified item.
        /// </summary>
        /// <param name="item">Item whose index is to be retrieved.</param>
        /// <returns>Index of specified item.</returns>
        public int IndexOf(T item)
        {
            return items.IndexOf(item);
        }

        /// <summary>
        /// Inserts the range of values from the enumerable into the channel.
        /// </summary>
        /// <typeparam name="T">The type of the channel.</typeparam>
        /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
        /// <param name="data">The data to insert into the channel.</param>
        internal override void InsertRange(int index, IEnumerable data)
        {
            if ((index < 0) || (index > items.Count))
                throw new ArgumentOutOfRangeException("index");
            if (data == null)
                throw new ArgumentNullException("data");
            if (!(data is IEnumerable<T>))
                throw new ArgumentException("data");
            items.InsertRange(index, (IEnumerable<T>)data);
        }

        /// <summary>
        /// Reads channel content and automatically converts it to the specified vector format.
        /// </summary>
        /// <typeparam name="TargetType">Target vector format for the converted channel data.</typeparam>
        /// <returns>The converted channel data.</returns>
        public override IEnumerable<TargetType> ReadConvertedContent<TargetType>()
        {
            // The following formats are supported:
            // - Single
            // - Vector2 Structure
            // - Vector3 Structure
            // - Vector4 Structure
            // - Any implementation of IPackedVector Interface.

            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds a new element to the end of the collection.
        /// </summary>
        /// <param name="value">The element to add.</param>
        void ICollection<T>.Add(T value)
        {
            ((ICollection<T>)Items).Add(value);
        }

        /// <summary>
        /// Removes all elements from the collection.
        /// </summary>
        void ICollection<T>.Clear()
        {
            Items.Clear();
        }

        /// <summary>
        /// Removes a specified element from the collection.
        /// </summary>
        /// <param name="value">The element to remove.</param>
        /// <returns>true if the channel was removed; false otherwise.</returns>
        bool ICollection<T>.Remove(T value)
        {
            return ((ICollection<T>)Items).Remove(value);
        }

        /// <summary>
        /// Inserts an element into the collection at the specified position.
        /// </summary>
        /// <param name="index">Index at which to insert the element.</param>
        /// <param name="value">The element to insert.</param>
        void IList<T>.Insert(int index, T value)
        {
            Items.Insert(index, value);
        }

        /// <summary>
        /// Removes the element at the specified index position.
        /// </summary>
        /// <param name="index">Index of the element to remove.</param>
        void IList<T>.RemoveAt(int index)
        {
            Items.RemoveAt(index);
        }
    }
}
