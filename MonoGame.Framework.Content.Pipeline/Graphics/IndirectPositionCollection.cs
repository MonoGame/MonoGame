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
