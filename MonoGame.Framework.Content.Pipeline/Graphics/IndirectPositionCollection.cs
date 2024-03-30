// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides methods for maintaining a list of vertex positions.
    /// </summary>
    /// <remarks>
    /// This class is designed to collect the vertex positions for a VertexContent object. Use the contents
    /// of the PositionIndices property (of the contained VertexContent object) to index into the Positions 
    /// property of the parent mesh.
    /// </remarks>
    public sealed class IndirectPositionCollection : IList<Vector3>
    {
        private readonly VertexChannel<int> _positionIndices;
        private readonly GeometryContent _geometry;

        /// <summary>
        /// Number of positions in the collection.
        /// </summary>
        /// <value>Number of positions.</value>
        public int Count
        {
            get { return _positionIndices.Count; }
        }

        /// <summary>
        /// Gets or sets the position at the specified index.
        /// </summary>
        /// <value>Position located at index.</value>
        public Vector3 this[int index]
        {
            get
            {
                var remap = _positionIndices[index];
                return _geometry.Parent.Positions[remap];
            } 
            set
            {
                throw Readonly();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this object is read-only.
        /// </summary>
        /// <value>true if this object is read-only; false otherwise.</value>
        bool ICollection<Vector3>.IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// Initializes a new instance of IndirectPositionCollection.
        /// </summary>
        internal IndirectPositionCollection(GeometryContent geom, VertexChannel<int> positionIndices)
        {
            _geometry = geom;
            _positionIndices = positionIndices;
        }

        /// <summary>
        /// Determines whether the specified position is in the collection.
        /// </summary>
        /// <param name="item">Position being searched for in the collection.</param>
        /// <returns>true if the position was found; false otherwise.</returns>
        public bool Contains(Vector3 item)
        {
            return IndexOf(item) > -1;
        }

        /// <summary>
        /// Copies the specified positions to an array, starting at the specified index.
        /// </summary>
        /// <param name="array">Array of positions to be copied.</param>
        /// <param name="arrayIndex">Index of the first copied position.</param>
        public void CopyTo(Vector3[] array, int arrayIndex)
        {
            foreach (var vec in this)
                array[arrayIndex++] = vec;
        }

        /// <summary>
        /// Gets an enumerator interface for reading the position values.
        /// </summary>
        /// <returns>Interface for enumerating the collection of position values.</returns>
        public IEnumerator<Vector3> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
                yield return this[i];
        }

        /// <summary>
        /// Gets the index of the specified position in a collection.
        /// </summary>
        /// <param name="item">Position being searched for.</param>
        /// <returns>Index of the specified position or -1 if not found.</returns>
        public int IndexOf(Vector3 item)
        {
            for (var i = 0; i < Count; i++)
                if (this[i] == item)
                    return i;

            return -1;
        }

        internal Exception Readonly()
        {
            return new NotSupportedException("The collection is read only!");
        }

        void ICollection<Vector3>.Add(Vector3 item)
        {
            throw Readonly();
        }

        void ICollection<Vector3>.Clear()
        {
            throw Readonly();
        }

        bool ICollection<Vector3>.Remove(Vector3 item)
        {
            throw Readonly();
        }

        void IList<Vector3>.Insert(int index, Vector3 item)
        {
            throw Readonly();
        }

        void IList<Vector3>.RemoveAt(int index)
        {
            throw Readonly();
        }

        /// <summary>
        /// Returns an enumerator that can iterate through the collection.
        /// </summary>
        /// <returns>Enumerator that can iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
