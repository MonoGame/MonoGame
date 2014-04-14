// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
{
    /// <summary>
    /// Writes the dictionary to the output.
    /// </summary>
    [ContentTypeWriter]
    class DictionaryWriter<K,V> : BuiltInContentWriter<Dictionary<K,V>>
    {
        ContentTypeWriter keyWriter;
        ContentTypeWriter valueWriter;

        /// <summary>
        /// Initialize the writer.
        /// </summary>
        /// <param name="compiler">Compiler instance calling this writer.</param>
        protected override void Initialize(ContentCompiler compiler)
        {
            base.Initialize(compiler);

            keyWriter = compiler.GetTypeWriter(typeof(K));
            valueWriter = compiler.GetTypeWriter(typeof(V));
        }

        /// <summary>
        /// Writes the value to the output.
        /// </summary>
        /// <param name="output">The output writer object.</param>
        /// <param name="value">The value to write to the output.</param>
        protected internal override void Write(ContentWriter output, Dictionary<K,V> value)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            output.Write(value.Count);
            foreach (var element in value)
            {
                output.WriteObject(element.Key, keyWriter);
                output.WriteObject(element.Value, valueWriter);
            }
        }
    }
}
