// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    /// <summary>
    /// Writes the list to the output.
    /// </summary>
    [ContentTypeWriter]
    class ListWriter<T> : BuiltInContentWriter<List<T>>
    {
        ContentTypeWriter _elementWriter;

        /// <inheritdoc/>
        internal override void OnAddedToContentWriter(ContentWriter output)
        {
            base.OnAddedToContentWriter(output);

            _elementWriter = output.GetTypeWriter(typeof(T));
        }

        public override bool CanDeserializeIntoExistingObject
        {
            get { return true; }
        }

        /// <summary>
        /// Writes the value to the output.
        /// </summary>
        /// <param name="output">The output writer object.</param>
        /// <param name="value">The value to write to the output.</param>
        protected internal override void Write(ContentWriter output, List<T> value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            output.Write(value.Count);
            foreach (var element in value)
            {
                output.WriteObject(element, _elementWriter);
            }
        }
    }
}
