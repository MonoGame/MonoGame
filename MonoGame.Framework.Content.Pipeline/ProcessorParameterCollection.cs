// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Represents a collection of processor parameters, usually for a single processor. This class is primarily designed for internal use or for custom processor developers.
    /// </summary>
    [SerializableAttribute]
    public sealed class ProcessorParameterCollection : ReadOnlyCollection<ProcessorParameter>
    {
        /// <summary>
        /// Constructs a new ProcessorParameterCollection instance.
        /// </summary>
        /// <param name="parameters">The parameters in the collection.</param>
        internal ProcessorParameterCollection(IEnumerable<ProcessorParameter> parameters)
            : base(new List<ProcessorParameter>(parameters))
        {
        }
    }
}
