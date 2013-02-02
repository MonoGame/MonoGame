// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    /// <summary>
    /// Writes the external reference to the output.
    /// </summary>
    [ContentTypeWriter]
    class ExternalReferenceWriter<T> : BuiltInContentWriter<ExternalReference<T>>
    {
        /// <summary>
        /// Writes the value to the output.
        /// </summary>
        /// <param name="output">The output writer object.</param>
        /// <param name="value">The value to write to the output.</param>
        protected internal override void Write(ContentWriter output, ExternalReference<T> value)
        {
            output.WriteExternalReference(value);
        }
    }
}
