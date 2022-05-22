// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides methods and properties for maintaining a vertex channel.
    /// A vertex channel is a list of arbitrary data with one value for each vertex. Channels are stored inside a GeometryContent and identified by name.
    /// </summary>
    public abstract class VertexChannel : IList, ICollection, IEnumerable
    {
        string name;

        /// <summary>
        /// Allows overriding classes to implement the list, and for properties/methods in this class to access it.
        /// </summary>
        internal abstract IList Items
        {
            get;
        }

        /// <summary>
        /// Gets the number of elements in the vertex channel
        /// </summary>
        public int Count
        {
            get
            {
                return Items.Count;
            }
        }

        /// <summary>
        /// Gets the type of data contained in this channel.
        /// </summary>
        public abstract Type ElementType { get; }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        public Object this[int index]
        {
            get
            {
                return Items[index];
            }
            set
            {
                Items[index] = value;
            }
        }

        /// <summary>
        /// Gets the name of the vertex channel.
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            internal set
            {
                name = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether access to the collection is synchronized (thread safe).
        /// </summary>
        bool System.Collections.ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the collection.
        /// </summary>
        Object System.Collections.ICollection.SyncRoot
        {
            get
            {
                return this;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this list has a fixed size.
        /// </summary>
        bool System.Collections.IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this object is read-only.
        /// </summary>
        bool System.Collections.IList.IsReadOnly
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
        {
            Name = name;
        }

        /// <summary>
        /// Determines whether the specified element is in the channel.
        /// </summary>
        /// <param name="value">Element being searched for.</param>
        /// <returns>true if the element is present; false otherwise.</returns>
        public bool Contains(Object value)
        {
            return Items.Contains(value);
        }

        /// <summary>
        /// Copies the elements of the channel to an array, starting at the specified index.
        /// </summary>
        /// <param name="array">Array that will receive the copied channel elements.</param>
        /// <param name="index">Starting index for copy operation.</param>
        public void CopyTo(Array array, int index)
        {
            ((ICollection)Items).CopyTo(array, index);
        }

        /// <summary>
        /// Gets an enumerator interface for reading channel content.
        /// </summary>
        /// <returns>Enumeration of the channel content.</returns>
        public IEnumerator GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        /// <summary>
        /// Gets the index of the specified item.
        /// </summary>
        /// <param name="value">Item whose index is to be retrieved.</param>
        /// <returns>Index of specified item.</returns>
        public int IndexOf(Object value)
        {
            return Items.IndexOf(value);
        }

        /// <summary>
        /// Reads channel content and automatically converts it to the specified vector format.
        /// </summary>
        /// <typeparam name="TargetType">Target vector format of the converted data.</typeparam>
        /// <returns>The converted data.</returns>
        public abstract IEnumerable<TargetType> ReadConvertedContent<TargetType>();

        /// <summary>
        /// Adds a new element to the end of the collection.
        /// </summary>
        /// <param name="value">The element to add.</param>
        /// <returns>Index of the element.</returns>
        int IList.Add(Object value)
        {
            return Items.Add(value);
        }

        /// <summary>
        /// Removes all elements from the collection.
        /// </summary>
        void IList.Clear()
        {
            Items.Clear();
        }

        /// <summary>
        /// Inserts an element into the collection at the specified position.
        /// </summary>
        /// <param name="index">Index at which to insert the element.</param>
        /// <param name="value">The element to insert.</param>
        void IList.Insert(int index, Object value)
        {
            Items.Insert(index, value);
        }

        /// <summary>
        /// Inserts the range of values from the enumerable into the channel.
        /// </summary>
        /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
        /// <param name="data">The data to insert into the channel.</param>
        internal abstract void InsertRange(int index, IEnumerable data);

        /// <summary>
        /// Removes a specified element from the collection.
        /// </summary>
        /// <param name="value">The element to remove.</param>
        void IList.Remove(Object value)
        {
            Items.Remove(value);
        }

        /// <summary>
        /// Removes the element at the specified index position.
        /// </summary>
        /// <param name="index">Index of the element to remove.</param>
        void IList.RemoveAt(int index)
        {
            Items.RemoveAt(index);
        }

        /// <summary>
        /// Removes a range of values from the channel.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range of elements to remove.</param>
        /// <param name="count"> The number of elements to remove.</param>
        internal abstract void RemoveRange(int index, int count);
    }
}
