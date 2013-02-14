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
        ContentTypeWriter elementWriter;

        /// <summary>
        /// Initialize the writer.
        /// </summary>
        /// <param name="compiler">Compiler instance calling this writer.</param>
        protected override void Initialize(ContentCompiler compiler)
        {
            base.Initialize(compiler);

            elementWriter = compiler.GetTypeWriter(typeof(T));
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
                output.WriteObject(value.Value, elementWriter);
        }
    }
}
