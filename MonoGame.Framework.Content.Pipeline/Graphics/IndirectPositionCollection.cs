// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides methods for maintaining a list of vertex positions.
    /// </summary>
    /// <remarks>This class is designed to collect the vertex positions for a VertexContent object. Use the contents of the PositionIndices property (of the contained VertexContent object) to index into the Positions property of the parent mesh.</remarks>
    public sealed class IndirectPositionCollection : IList<Vector3>, ICollection<Vector3>, IEnumerable<Vector3>, IEnumerable
    {
        List<Vector3> items;

        /// <summary>
        /// Number of positions in the collection.
        /// </summary>
        /// <value>Number of positions.</value>
        public int Count { get { return items.Count; } }

        /// <summary>
        /// Gets or sets the position at the specified index.
        /// </summary>
        /// <value>Position located at index.</value>
        public Vector3 this[int index] { get { return items[index]; } set { items[index] = value; } }

        /// <summary>
        /// Gets a value indicating whether this object is read-only.
        /// </summary>
        /// <value>true if this object is read-only; false otherwise.</value>
        bool System.Collections.Generic.ICollection<Microsoft.Xna.Framework.Vector3>.IsReadOnly { get { return false; } }

        /// <summary>
        /// Initializes a new instance of IndirectPositionCollection.
        /// </summary>
        internal IndirectPositionCollection()
        {
            items = new List<Vector3>();
        }

        /// <summary>
        /// Determines whether the specified position is in the collection.
        /// </summary>
        /// <param name="item">Position being searched for in the collection.</param>
        /// <returns>true if the position was found; false otherwise.</returns>
        public bool Contains(Vector3 item)
        {
            return items.Contains(item);
        }

        /// <summary>
        /// Copies the specified positions to an array, starting at the specified index.
        /// </summary>
        /// <param name="array">Array of positions to be copied.</param>
        /// <param name="arrayIndex">Index of the first copied position.</param>
        public void CopyTo(Vector3[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets an enumerator interface for reading the position values.
        /// </summary>
        /// <returns>Interface for enumerating the collection of position values.</returns>
        public IEnumerator<Vector3> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        /// <summary>
        /// Gets the index of the specified position in a collection.
        /// </summary>
        /// <param name="item">Position being searched for.</param>
        /// <returns>Index of the specified position.</returns>
        public int IndexOf(Vector3 item)
        {
            return items.IndexOf(item);
        }

        /// <summary>
        /// Adds a new element to the end of the collection.
        /// </summary>
        /// <param name="item">The item to add.</param>
        void ICollection<Vector3>.Add(Vector3 item)
        {
            items.Add(item);
        }

        /// <summary>
        /// Removes all elements from the collection.
        /// </summary>
        void ICollection<Vector3>.Clear()
        {
            items.Clear();
        }

        /// <summary>
        /// Removes the specified element from the collection.
        /// </summary>
        /// <param name="item">The item to be removed.</param>
        /// <returns>true if the element was removed; false otherwise.</returns>
        bool ICollection<Vector3>.Remove(Vector3 item)
        {
            return items.Remove(item);
        }

        /// <summary>
        /// Inserts a new element into the collection.
        /// </summary>
        /// <param name="index">Index for element insertion.</param>
        /// <param name="item">Element to insert.</param>
        void IList<Vector3>.Insert(int index, Vector3 item)
        {
            items.Insert(index, item);
        }

        /// <summary>
        /// Removes the element at the specified index position.
        /// </summary>
        /// <param name="index">Index of the element to be removed.</param>
        void IList<Vector3>.RemoveAt(int index)
        {
            items.RemoveAt(index);
        }

        /// <summary>
        /// Returns an enumerator that can iterate through the collection.
        /// </summary>
        /// <returns>Enumerator that can iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }
    }
}
