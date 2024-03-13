// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Represents a set of bones associated with a model.
    /// </summary>
    public class ModelBoneCollection : ReadOnlyCollection<ModelBone>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelBoneCollection"/>
        /// class that is a read-only wrapper around the specified list.
        /// </summary>
        /// <param name="list">The list to wrap.</param>
        public ModelBoneCollection(IList<ModelBone> list)
            : base(list)
        {

        }

        /// <summary>
        /// Retrieves a <see cref="ModelBone"/> from the collection, given the name of the bone.
        /// </summary>
        /// <param name="boneName">The name of the bone to retrieve.</param>
        public ModelBone this[string boneName]
        {
            get
            {
                ModelBone ret;
                if (!TryGetValue(boneName, out ret))
                    throw new KeyNotFoundException();
                return ret;
            }
        }

        /// <summary>
        /// Finds a bone with a given name if it exists in the collection.
        /// </summary>
        /// <param name="boneName">The name of the bone to find.</param>
        /// <param name="value">The bone named boneName, if found.</param>
        /// <returns>true if the bone was found</returns>
        public bool TryGetValue(string boneName, out ModelBone value)
        {
            if (string.IsNullOrEmpty(boneName))
                throw new ArgumentNullException("boneName");

            foreach (ModelBone bone in this)
            {
                if (bone.Name == boneName)
                {
                    value = bone;
                    return true;
                }
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Returns a <see cref="ModelBoneCollection.Enumerator">ModelBoneCollection.Enumerator</see>
        /// that can iterate through a collection.
        /// </summary>
        /// <returns></returns>
        public new Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>
        /// Provides the ability to iterate through the bones in an ModelBoneCollection.
        /// </summary>
        public struct Enumerator : IEnumerator<ModelBone>
        {
            private readonly ModelBoneCollection _collection;
            private int _position;

            internal Enumerator(ModelBoneCollection collection)
            {
                _collection = collection;
                _position = -1;
            }


            /// <summary>
            /// Gets the current element in the ModelBoneCollection.
            /// </summary>
            public ModelBone Current { get { return _collection[_position]; } }

            /// <summary>
            /// Advances the enumerator to the next element of the ModelBoneCollection.
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

            /// <inheritdoc/>
            public void Reset()
            {
                _position = -1;
            }

            #endregion
        }
    }
}
