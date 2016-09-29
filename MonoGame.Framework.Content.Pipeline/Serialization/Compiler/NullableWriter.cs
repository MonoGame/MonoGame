// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    /// <summary>
    /// Writes the nullable value to the output.
    /// </summary>
    [ContentTypeWriter]
    class NullableWriter<T> : BuiltInContentWriter<Nullable<T>> where T: struct
    {
        ContentTypeWriter _elementWriter;

        /// <inheritdoc/>
        internal override void OnAddedToContentWriter(ContentWriter output)
        {
            base.OnAddedToContentWriter(output);

            _elementWriter = output.GetTypeWriter(typeof(T));
        }

        /// <summary>
        /// Writes the value to the output.
        /// </summary>
        /// <param name="output">The output writer object.</param>
        /// <param name="value">The value to write to the output.</param>
        protected internal override void Write(ContentWriter output, Nullable<T> value)
        {
            output.Write(value.HasValue);
            if (value.HasValue)
                output.WriteObject(value.Value, _elementWriter);
        }
    }
}
