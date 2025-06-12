// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    /// <summary>
    /// Provides methods for maintaining a list of index values.
    /// </summary>
    public sealed class IndexCollection : Collection<int>
    {
        /// <summary>
        /// Initializes a new instance of IndexCollection.
        /// </summary>
        public IndexCollection()
        {
        }

        /// <summary>
        /// Add a range of indices to the collection.
        /// </summary>
        /// <param name="indices">A collection of indices to add.</param>
        public void AddRange(IEnumerable<int> indices)
        {
            foreach (var t in indices)
                Add(t);
        }
    }
}
