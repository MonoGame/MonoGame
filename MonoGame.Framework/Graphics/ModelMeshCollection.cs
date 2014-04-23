// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Represents a collection of ModelMesh objects.
    /// </summary>
    public sealed class ModelMeshCollection : ReadOnlyCollection<ModelMesh>
    {
        internal ModelMeshCollection(IList<ModelMesh> list)
            : base(list)
        {

        }

        /// <summary>
        /// Retrieves a ModelMesh from the collection, given the name of the mesh.
        /// </summary>
        /// <param name="meshName">The name of the mesh to retrieve.</param>
        public ModelMesh this[string meshName]
        {
            get
            {
                ModelMesh ret;
                if (!TryGetValue(meshName, out ret))
                    throw new KeyNotFoundException();
                return ret;
            }
        }

        /// <summary>
        /// Finds a mesh with a given name if it exists in the collection.
        /// </summary>
        /// <param name="meshName">The name of the mesh to find.</param>
        /// <param name="value">The mesh named meshName, if found.</param>
        /// <returns>true if a mesh was found</returns>
        public bool TryGetValue(string meshName, out ModelMesh value)
        {
            if (string.IsNullOrEmpty(meshName))
                throw new ArgumentNullException("meshName");

            foreach (var mesh in this)
            {
                if (string.Compare(mesh.Name, meshName, StringComparison.Ordinal) == 0)
                {
                    value = mesh;
                    return true;
                }
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Returns a ModelMeshCollection.Enumerator that can iterate through a ModelMeshCollection.
        /// </summary>
        /// <returns></returns>
        public new Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>
        /// Provides the ability to iterate through the bones in an ModelMeshCollection.
        /// </summary>
        public struct Enumerator : IEnumerator<ModelMesh>
        {
            private readonly ModelMeshCollection _collection;
            private int _position;

            internal Enumerator(ModelMeshCollection collection)
            {
                _collection = collection;
                _position = -1;
            }


            /// <summary>
            /// Gets the current element in the ModelMeshCollection.
            /// </summary>
            public ModelMesh Current { get { return _collection[_position]; } }

            /// <summary>
            /// Advances the enumerator to the next element of the ModelMeshCollection.
            /// </summary>
            public bool MoveNext()
            {
                _position++;
                return (_position < _collection.Count);
            }

            #region IDisposable

            /// <summary>
            /// Immediately releases the unmanaged resources used by this object.
            /// </summary>
            public void Dispose()
            {
            }

            #endregion

            #region IEnumerator Members

            object IEnumerator.Current
            {
                get { return _collection[_position]; }
            }

            public void Reset()
            {
                _position = -1;
            }

            #endregion
        }
    }
}
