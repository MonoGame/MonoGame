// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    /// <summary>
    /// Writes the array value to the output.
    /// </summary>
    [ContentTypeWriter]
    class ArrayWriter<T> : BuiltInContentWriter<T[]>
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
        protected internal override void Write(ContentWriter output, T[] value)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            output.Write(value.Length);
            foreach (var element in value)
                output.WriteObject(element, elementWriter);
        }
    }
}
